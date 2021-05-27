using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Fragsurf.Shared;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;
using Fragsurf.Shared.Entity;
using Fragsurf.Actors;
using Newtonsoft.Json;
using Fragsurf.Maps;
using Fragsurf.Movement;

namespace Fragsurf.Gamemodes.Tricksurf
{
    [Inject(InjectRealm.Shared, typeof(Tricksurf))]
    public class SH_Tricksurf : FSSharedScript
    {

        private class PlayerTrickData
        {
            public PlayerTrickData(TrickData data)
            {
                Track = new TrickTrack(data);
            }

            public int Points;
            public TrickTrack Track;
            public Trick MostRecentTrick;
            public int StartSpeed;
            public int LastTouchedTriggerId;
            public int ComboTrickId;
            public int ComboCount;
        }

        public event Action<BasePlayer, TrickCompletion> OnTrickCompleted;
        public event Action<BasePlayer, TouchInfo> OnTriggerEntered;
        public event Action<BasePlayer> OnTrackInvalidated;
        public event Action<TrickData> OnTricksRebuilt;
        public event Action<TrickData> OnTricksLoaded;

        private Dictionary<BasePlayer, PlayerTrickData> _trickData = new Dictionary<BasePlayer, PlayerTrickData>();

        private List<FSMTrigger> _fsmTriggers;
        private Dictionary<string, FSMTrigger> _triggerCache;

        public TrickData TrickData { get; private set; }

        public string GetTrickDataPath()
        {
            var fileName = Path.GetFileNameWithoutExtension(Map.Current.Name);
            return Gamemode.GetDataDirectory() + "/tricks/" + fileName + ".json";
        }

        public string GetCustomTrickDataPath()
        {
            var fileName = Path.GetFileNameWithoutExtension(Map.Current.Name);
            return Path.Combine(Application.persistentDataPath, "UserTricks", fileName + ".json");
        }

        private void Tune()
        {
            if (TrickData == null || string.IsNullOrWhiteSpace(TrickData.tune))
            {
                return;
            }

            switch (TrickData.tune)
            {
                case "csgo":
                    DevConsole.SetVariable("mv.aircap", 0.800016f, true, true);
                    break;
                case "css":
                    DevConsole.SetVariable("mv.aircap", 0.762f, true, true);
                    break;
            }
        }

        public void ReplaceTricks(List<Trick> newTricks)
        {
            if (TrickData == null)
            {
                LoadTrickData();
            }

            try
            {
                TrickData.tricks.Clear();
                TrickData.tricks.AddRange(newTricks);
                BuildTrickDataTree();
            }
            catch (Exception e)
            {
                DevConsole.WriteLine("Failed to load trick data.");
                DevConsole.WriteLine(e.ToString());
            }
        }

        public void AppendTricks(string jsonSource)
        {
            if (TrickData == null)
            {
                LoadTrickData();
            }

            try
            {
                var tricks = JsonConvert.DeserializeObject<List<Trick>>(jsonSource);
                if (tricks == null)
                {
                    return;
                }
                // this is just a temporary patch for tricks that don't have custom flagged yet...
                // in the future, remove this
                foreach(var trick in tricks)
                {
                    trick.custom = true;
                }
                TrickData.tricks.AddRange(tricks);
                BuildTrickDataTree();
            }
            catch (Exception e)
            {
                DevConsole.WriteLine("Failed to load trick data.");
                DevConsole.WriteLine(e.ToString());
            }
        }

        public void LoadTrickData()
        {
            var jsonFilePath = GetTrickDataPath();
            var jsonData = File.Exists(jsonFilePath) ? File.ReadAllText(jsonFilePath) : string.Empty;

            try
            {
                TrickData = JsonConvert.DeserializeObject<TrickData>(jsonData);
            }
            catch (Exception e)
            {
                DevConsole.WriteLine("Failed to load tricks from file: " + jsonFilePath);
                DevConsole.WriteLine(e.ToString());
            }

            if(TrickData == null)
            {
                TrickData = new TrickData()
                {
                    tricks = new List<Trick>(),
                    triggers = new Dictionary<int, string>(),
                    whop_origin = "0,0,0",
                    whop_velocity = "0,0,0",
                    chains = new Dictionary<int, int>()
                };
            }

            BuildTrickDataTree();
            Tune();

            OnTricksLoaded?.Invoke(TrickData);
        }

        private void BuildTrickDataTree()
        {
            TrickData.BuildTree();
            foreach(var t in _trickData)
            {
                t.Value.Track.UpdateTrickData(TrickData);
            }
            try
            {
                OnTricksRebuilt?.Invoke(TrickData);
            }
            catch(Exception e) { Debug.LogError(e.ToString()); }
        }

        protected override void _Start()
        {
            OnTrickCompleted += (player, completionData) => { Game.UserPlugins?.InvokeEventSubscriptions("OnTrickCompleted", player, completionData); };
            OnTriggerEntered += (player, touchData) => { Game.UserPlugins?.InvokeEventSubscriptions("OnTrickTrigger", player, touchData); };
            OnTrackInvalidated += (player) => { Game.UserPlugins?.InvokeEventSubscriptions("OnTrickInvalidated", player); };

            LoadTrickData();

            if(!Game.IsHost)
            {
                DevConsole.RegisterCommand("trick.test", "", this, (args) =>
                {
                    var triggers = args[1].Split(',');
                    var frames = new List<TouchInfo>(triggers.Length);
                    var track = new TrickTrack(TrickData);
                    foreach (var triggerName in triggers)
                    {
                        var cleanName = triggerName.Trim();
                        var fsmtrig = FindTrigger(cleanName);

                        var triggerId = -1;
                        foreach(var kvp in TrickData.triggers)
                        {
                            if(string.Equals(kvp.Value, cleanName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                triggerId = kvp.Key;
                                break;
                            }
                        }

                        if(triggerId == -1)
                        {
                            return;
                        }

                        var frame = new TouchInfo()
                        {
                            Tick = Game.CurrentTick,
                            Admissible = fsmtrig != null ? IsAdmissible(fsmtrig) : true,
                            TriggerId = triggerId,
                        };
                        frames.Add(frame);
                    }
                    var trickCache = new Trick[128];
                    var trickCount = track.Test(frames, trickCache);
                    DevConsole.WriteLine("[TRICK TEST RESULT]");
                    DevConsole.WriteLine(" - Tricks in sequence: " + trickCount);
                    for(int i = 0; i < trickCount; i++)
                    {
                        DevConsole.WriteLine("  - " + trickCache[i].name);
                    }
                }, false);
            }

            _fsmTriggers = GameObject.FindObjectsOfType<FSMTrigger>().ToList();
            _triggerCache = new Dictionary<string, FSMTrigger>();
            foreach (var trigger in _fsmTriggers)
            {
                trigger.ActorName = trigger.ActorName.ToLower();
                var id = TrickData.GetTriggerId(trigger.ActorName);
                if (!_triggerCache.ContainsKey(trigger.ActorName))
                {
                    _triggerCache.Add(trigger.ActorName, trigger);
                }
                // this is a dumb hack.  my GUESS is that disabling/enabling the collider pushes it to the back of the
                // physics execution order.  this is useful if we have 2 triggers in same place but 1 always needs to execute first.
                if(TrickData.lateload != null && TrickData.lateload.Contains(TrickData.GetTriggerId(trigger.ActorName)))
                {
                    var collider = trigger.GetComponent<Collider>();
                    collider.enabled = false;
                    collider.enabled = true;
                }
            }
        }

        protected override void OnPlayerIntroduced(BasePlayer player)
        {
            if(TrickData != null)
            {
                _trickData.Add(player, new PlayerTrickData(TrickData));

                if(Application.isEditor)
                {
                    _trickData[player].Track.Debug = true;
                }
            }
        }

        protected override void OnPlayerDisconnected(BasePlayer player)
        {
            _trickData.Remove(player);
        }

        //protected override void OnPlayerRunCommand(IPlayer player)
        //{
        //    if (!player.Human.Movement.MoveData.JustJumped)
        //    {
        //        return;
        //    }

        //    if (_trickData[player].PrespeedTrigger != null)
        //    {
        //        var vel = player.Human.Velocity;
        //        vel.y = 0;
        //        var speed = (int)(vel.magnitude / UBsp.UnityBsp.WORLD_SCALE);
        //        var invalid = speed > 405;
        //        //_trickData[player].Track.RegisterInvalidStart(_trickData[player].PrespeedTrigger.TargetName, invalid);
        //        _trickData[player].StartSpeed = speed;
        //        _trickData[player].PrespeedTrigger = null;
        //    }
        //}

        protected override void OnPlayerPacketReceived(BasePlayer player, IBasePacket packet)
        {
            if (Game.IsHost)
            {
                return;
            }

            if (packet is TrickCompletion tc)
            {
                var playerRef = Game.PlayerManager.FindPlayer(tc.ClientIndex);
                var trick = TrickData.GetTrick(tc.TrickName);

                if(trick == null || playerRef == null)
                {
                    DevConsole.WriteLine($"SH_Tricksurf missing data\n > Client index: {tc.ClientIndex}\n > Trick: {tc.TrickName}");
                    return;
                }

                _trickData[playerRef].Points += tc.Points;
                _trickData[playerRef].MostRecentTrick = trick;

                if(!DevConsole.GetVariable<bool>("game.modified"))
                {
                    // todo: tricklog
                    //var tl = Get<SH_TrickLog>();
                    //if (playerRef.ClientIndex == Game.ClientIndex && !tl.IsCompleted(tc.TrickName))
                    //{
                    //    Fragsurf.Client.Surface.SurfaceManager.Toast($"Nice, you've completed a new trick! <span class='green'>{tl.CompletionCount + 1}</span> out of <span class='green'>{TrickData.tricks.Count}</span>");
                    //}
                }

                OnTrickCompleted?.Invoke(playerRef, tc);
            }
        }

        public IEnumerable<FSMTrigger> FindTriggers(int triggerId)
        {
            var triggerName = TrickData.triggers.FirstOrDefault(x => x.Key == triggerId).Value;
            if(string.IsNullOrEmpty(triggerName))
            {
                yield break;
            }
            foreach(var trigger in _fsmTriggers)
            {
                if(string.Equals(triggerName, trigger.ActorName))
                {
                    yield return trigger;
                }
            }
        }

        public FSMTrigger FindTrigger(string name)
        {
            if(_triggerCache.ContainsKey(name))
            {
                return _triggerCache[name];
            }
            return null;
        }

        protected override void OnHumanSpawned(Human hu)
        {
            var player = Game.PlayerManager.FindPlayer(hu);
            if(player != null)
            {
                InvalidateTrack(player);
            }
        }

        protected override void OnHumanTrigger(NetEntity ent, FSMTrigger trigger, TriggerEventType type, float offset = 0)
        {
            if(!(ent is Human hu)
                || string.IsNullOrWhiteSpace(trigger.ActorName))
            {
                return;
            }

            var player = Game.PlayerManager.FindPlayer(hu);
            if(player == null)
            {
                return;
            }

            var playerTrickData = _trickData[player];
            var triggerId = TrickData.GetTriggerId(trigger.ActorName);

            var invalidatedField = trigger.GetCustomProperty("InvalidateTricks");
            if (invalidatedField != null && invalidatedField.BoolValue)
            {
                playerTrickData.Track.Invalidate();
            }

            if (triggerId == -1)
            {
                return;
            }

            if (!playerTrickData.Track.Invalidated && playerTrickData.LastTouchedTriggerId == triggerId)
            {
                if (type == TriggerEventType.Exit)
                {
                    if (trigger.TriggerCondition == FSMTriggerCondition.Grounded)
                    {
                        var speed = hu.HammerVelocity(true);
                        var invalidStart = speed > 405;
                        playerTrickData.Track.UpdateTouchSpeedAndTime(trigger.ActorName, hu.Velocity, invalidStart, Game.ElapsedTime, offset);
                        playerTrickData.StartSpeed = speed;
                    }
                }
                return;
            }

            playerTrickData.LastTouchedTriggerId = triggerId;

            var touchInfo = new TouchInfo()
            {
                Tick = Game.CurrentTick,
                Trigger = trigger,
                TriggerId = triggerId,
                Admissible = IsAdmissible(trigger),
                Velocity = hu.Velocity,
                Time = Game.ElapsedTime,
                ZoneOffset = offset,
                Style = MoveStyle.FW
            };

            if (type == TriggerEventType.Enter || type == TriggerEventType.Stay)
            {
                if (_trickData[player].Track.Invalidated)
                {
                    playerTrickData.Track.RegisterTouch(trigger.ActorName, touchInfo, out TrickCompletion _);
                    playerTrickData.Track.Invalidated = false;
                    if(type == TriggerEventType.Stay)
                    {
                        OnTriggerEntered?.Invoke(player, touchInfo);
                    }
                }
                if(type == TriggerEventType.Enter)
                {
                    if(Game.IsHost)
                    {
                        RegisterTouch(player, trigger, touchInfo);
                    }
                    OnTriggerEntered?.Invoke(player, touchInfo);
                }
            }
            else if(type == TriggerEventType.Exit)
            {
                if(trigger.TriggerCondition == FSMTriggerCondition.Grounded)
                {
                    var speed = hu.HammerVelocity(true);
                    var invalidStart = speed > 405;
                    playerTrickData.Track.UpdateTouchSpeedAndTime(trigger.ActorName, hu.Velocity, invalidStart, Game.ElapsedTime, offset);
                    playerTrickData.StartSpeed = speed;
                }
            }
        }

        private int _completionId;
        public int GetCompletionId()
        {
            _completionId++;
            return _completionId;
        }

        private void RegisterTouch(BasePlayer player, FSMTrigger fsmTrigger, TouchInfo touchInfo)
        {
            if (_trickData[player].Track.RegisterTouch(fsmTrigger.ActorName, touchInfo, out TrickCompletion completion))
            {
                var trick = TrickData.GetTrick(completion.TrickId);
                _trickData[player].Points += trick.points;

                if (completion.TrickId == _trickData[player].ComboTrickId)
                {
                    _trickData[player].ComboCount++;
                }
                else
                {
                    _trickData[player].ComboTrickId = completion.TrickId;
                    _trickData[player].ComboCount = 1;
                }

                completion.ClientIndex = player.ClientIndex;
                completion.Points = trick.points;
                completion.CompletionId = GetCompletionId();
                completion.ComboCount = _trickData[player].ComboCount;

                OnTrickCompleted?.Invoke(player, completion);

                Game.Network.BroadcastPacket(completion);
            }
        }

        private bool IsAdmissible(FSMTrigger trigger)
        {
            var admissibleField = trigger.GetCustomProperty("Admissible");
            if (admissibleField != null)
            {
                return admissibleField.BoolValue;
            }
            else if (trigger.TriggerCondition == FSMTriggerCondition.InAir || trigger.TriggerCondition == FSMTriggerCondition.None)
            {
                return true;
            }
            return false;
        }

        public TrickTrack GetTrack(BasePlayer player)
        {
            if(!_trickData.ContainsKey(player))
            {
                return null;
            }
            return _trickData[player].Track;
        }

        public void InvalidateTrack(BasePlayer player)
        {
            if(!_trickData.ContainsKey(player))
            {
                return;
            }
            _trickData[player].ComboTrickId = 0;
            _trickData[player].ComboCount = 0;
            _trickData[player].LastTouchedTriggerId = 0;
            _trickData[player].Track.Invalidate();
            OnTrackInvalidated?.Invoke(player);
        }

        public void EnableDetection(BasePlayer player, bool enabled)
        {
            GetTrack(player)?.EnableDetection(enabled);
        }

        public int GetPoints(BasePlayer player)
        {
            return _trickData.ContainsKey(player) ? _trickData[player].Points : 0;
        }

        public int GetStartSpeed(BasePlayer player)
        {
            return _trickData.ContainsKey(player) ? _trickData[player].StartSpeed : 0;
        }

        public int GetLastTouchedTrigger(BasePlayer player)
        {
            return _trickData.ContainsKey(player) ? _trickData[player].LastTouchedTriggerId : 0;
        }

    }
}

