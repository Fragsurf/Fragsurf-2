using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Fragsurf.Shared.Packets;
using Fragsurf.Movement;

namespace Fragsurf.Server.ServerScripts
{
    public class FakeClient : FSServerScript
    {
        //private bool _shuffle;
        //private bool _duck;
        //private bool _turn;
        //private bool _attack;
        //private bool _broadcast = true;

        //private List<string> _names = new List<string>()
        //{
        //    "[BOT] Andre",
        //    "[BOT] Andy",
        //    "[BOT] Paul",
        //    "[BOT] Brent",
        //    "[BOT] Jacob",
        //    "[BOT] Caroline",
        //    "[BOT] Hampton",
        //    "[BOT] Victor",
        //    "[BOT] Evan",
        //    "[BOT] Gray",
        //    "[BOT] Enza",
        //    "[BOT] Michael",
        //};

        /////// Methods /////

        //protected override void _Initialize()
        //{
        //    base._Initialize();

        //    DevConsole.RegisterCommand("bot.add", "Adds a bot", this, Command_AddFakeClient);
        //    DevConsole.RegisterCommand("bot.place", "Places a bot", this, Command_PlaceBot);
        //    DevConsole.RegisterCommand("bot.give", "Gives an item to all bots", this, Command_GiveBotsItem);
        //    DevConsole.RegisterCommand("bot.kickall", "Kicks all bots", this, Command_KickFakeClients);
        //    DevConsole.RegisterVariable("bot.shuffle", "Bots move left and right", () => _shuffle, v => _shuffle = v, this, ConVarFlags.Cheat);
        //    DevConsole.RegisterVariable("bot.duck", "Bots duck", () => _duck, v => _duck = v, this, ConVarFlags.Cheat);
        //    DevConsole.RegisterVariable("bot.turn", "Bots turn", () => _turn, v => _turn = v, this, ConVarFlags.Cheat);
        //    DevConsole.RegisterVariable("bot.attack", "Bots attack", () => _attack, v => _attack = v, this, ConVarFlags.Cheat);
        //    DevConsole.RegisterVariable("bot.broadcast", "Bots broadcast UserCmd packets", () => _broadcast, v => _broadcast = v, this, ConVarFlags.Cheat);
        //}

        //protected override void _Destroy()
        //{
        //    base._Destroy();

        //    Command_KickFakeClients(null);

        //    DevConsole.RemoveAll(this);
        //}

        //protected override void OnGameUnloaded()
        //{
        //    base.OnGameUnloaded();

        //    Command_KickFakeClients(null);
        //}

        ////protected override void OnMapEvent(object sender, MapEventArgs e)
        ////{
        ////    switch(e.EventType)
        ////    {
        ////        case MapEventType.Unloaded:
                    
        ////            break;
        ////    }
        ////}

        //private float _timer;
        //private float _dir = 7.62f;

        //protected override void _Tick()
        //{
        //    //if (!_shuffle && !_turn && !_duck && !_attack) return;

        //    _timer -= UnityEngine.Time.fixedDeltaTime;

        //    if (_timer <= 0)
        //    {
        //        _dir *= -1;
        //        _timer = 2f;
        //    }

        //    var playerMan = Game.PlayerManager;

        //    for (int i = 0; i < playerMan.Players.Count; i++)
        //    {
        //        var netPlayer = (ServerPlayer)playerMan.Players[i];

        //        if (!netPlayer.IsFake)
        //        {
        //            continue;
        //        }

        //        if (netPlayer.Human == null)
        //        {
        //            Game.EntityManager.SpawnHuman(netPlayer);
        //        }

        //        if (netPlayer.Human.Dead)
        //        {
        //            continue;
        //        }

        //        var fields = new UserCmd.CmdFields();
        //        fields.ClientIndex = netPlayer.ClientIndex;
        //        fields.Buttons = 0;
        //        fields.Angles = _turn ? netPlayer.Human.Angles + new UnityEngine.Vector3(0, _dir < 0 ? 3 : 1, 0) : netPlayer.Human.Angles;
        //        fields.Origin = netPlayer.Human.Origin;
        //        fields.Velocity = netPlayer.Human.Velocity;
        //        fields.BaseVelocity = netPlayer.Human.BaseVelocity;
        //        fields.ForwardMove = _shuffle ? _dir : 0;
        //        //_fakeUserCmd.ForwardMove = _dir;
                

        //        if (_dir < 0 && _duck)
        //        {
        //            fields.Buttons |= InputActions.Duck;
        //        }

        //        if(_attack)
        //        {
        //            fields.Buttons |= InputActions.HandAction;
        //        }

        //        netPlayer.Human.UserRunCommand(fields);

        //        if(_broadcast)
        //        {
        //            var userCmd = PacketUtility.TakePacket<UserCmd>();
        //            userCmd.Fields = fields;
        //            Game.Network.BroadcastPacket(userCmd);
        //        }
        //    }
        //}

        //public ServerPlayer SpawnFakeClient(byte teamNumber = 1)
        //{
        //    var nextIndex = Game.GetFSComponent<SocketManager>().NextClientIndex;
        //    var player = new ServerPlayer((ulong)nextIndex, nextIndex, Game.ElapsedTime, true);
        //    player.DisplayName = _names[UnityEngine.Random.Range(0, _names.Count - 1)];
        //    GameServer.Instance.Socket.InitiatePlayer(player);
        //    Game.PlayerManager.IntroducePlayer(player);
        //    Game.PlayerManager.SetPlayerTeam(player, teamNumber);

        //    return player;
        //}

        //private void Command_PlaceBot(string[] args)
        //{
        //    foreach(var bot in Game.PlayerManager.Players)
        //    {
        //        if(bot.IsFake && bot.Human != null)
        //        {
        //            var player = Game.PlayerManager.Players.FirstOrDefault(x => !x.IsFake && x.Human != null);
        //            if(player != null)
        //            {
        //                var ray = player.Human.GetEyeRay();
        //                if(Physics.Raycast(ray, out RaycastHit hit, 100, 1 << 0))
        //                {
        //                    bot.Human.Origin = hit.point;
        //                }
        //            }
        //            break;
        //        }
        //    }
        //}

        //private void Command_AddFakeClient(string[] args)
        //{
        //    byte teamNumber = 1;
        //    if(args.Length > 1)
        //    {
        //        byte.TryParse(args[1], out teamNumber);
        //    }
        //    SpawnFakeClient(teamNumber);
        //}

        //private void Command_GiveBotsItem(string[] args)
        //{
        //    foreach(var player in Game.PlayerManager.Players)
        //    {
        //        if(!player.IsFake || player.Human == null || player.Human.Dead)
        //        {
        //            continue;
        //        }
        //        player.Human.Give(args[1]);
        //    }
        //}

        //private void Command_KickFakeClients(string[] args)
        //{
        //    for (int i = Game.PlayerManager.Players.Count - 1; i >= 0; i--)
        //    {
        //        var netPlayer = (ServerPlayer)Game.PlayerManager.Players[i];

        //        if (!netPlayer.IsFake)
        //        {
        //            continue;
        //        }

        //        GameServer.Instance.Socket.DisconnectPlayer(netPlayer);
        //    }
        //}

    }
}
