using Fragsurf.Maps;
using Fragsurf.Movement;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public class Human : NetEntity, IDamageable
    {

        public MovementController MovementController;
        public CameraController CameraController;
        public EntityAnimationController AnimationController;
        public BotController BotController;

        private Interactor _interactor;
        private int _ownerId = -1;
        private bool _hasAuthorityNextTick;
        private bool _dead;

        public Vector3 AngleOverride = InvalidAngleOverride;
        public static Vector3 InvalidAngleOverride = new Vector3(-1, -2, -3);
        public float TimeDead { get; private set; }

        public Human(FSGameLoop game) 
            : base(game)
        {
            _autoRecordTimeline = false;
            _interactor = new Interactor(this);
        }

        public static Human Local { get; set; }
        public HumanGameObject HumanGameObject => EntityGameObject as HumanGameObject;
        public bool IsFirstPerson { get; set; }
        public EquippableManager Equippables { get; private set; } = new EquippableManager();
        public UserCmd.CmdFields CurrentCmd { get; protected set; }
        public bool IsBot => BotController != null;

        [NetProperty(true)]
        public virtual Vector3 Velocity { get; set; }
        [NetProperty(true)]
        public virtual Vector3 BaseVelocity { get; set; }
        [NetProperty]
        public int OwnerId
        {
            get => _ownerId;
            set => SetOwnerId(value);
        }
        [NetProperty]
        public bool Ducked { get; set; }
        [NetProperty]
        public int Health { get; set; }
        [NetProperty]
        public bool Dead
        {
            get => _dead;
            set => SetIsDead(value);
        }
        [NetProperty]
        public bool Frozen { get; set; }
        [NetProperty]
        public bool FlashlightOn { get; set; }

        protected override void OnInitialized()
        {
            FlashlightOn = false;

            if(Game.GamemodeLoader.Gamemode.Data.HumanPrefab != null)
            {
                var clone = GameObject.Instantiate(Game.GamemodeLoader.Gamemode.Data.HumanPrefab);
                if (!clone.TryGetComponent(out HumanGameObject huObj))
                {
                    GameObject.Destroy(clone);
                    Debug.LogError("Human Prefab must contain a complete HumanGameObject component");
                }
                else
                {
                    EntityGameObject = huObj;
                }
            }

            if (!EntityGameObject)
            {
                EntityGameObject = new GameObject("Human").AddComponent<HumanGameObject>();
            }

            MovementController = new CSMovementController(this);
            CameraController = new FirstPersonCameraController(this);
        }

        protected override void OnDelete()
        {
            if(Local == this)
            {
                Local = null;
            }
        }

        protected override void OnTick()
        {
            // gives it one tick to update origin & angles from authority before taking control
            if (_hasAuthorityNextTick)
            {
                _hasAuthorityNextTick = false;
                InterpolationMode = InterpolationMode.Frame;
                HasAuthority = true;
            }

            TickPunches();

            BotController?.Tick();

            if (Dead)
            {
                TimeDead += Time.fixedDeltaTime;
            }
        }

        protected override void OnUpdate()
        {
            MovementController?.Update();
            AnimationController?.Update();
        }

        public Ray GetEyeRay()
        {
            var direction = (Quaternion.Euler(Angles + TotalViewPunch() + TotalAimPunch()) * Vector3.forward).normalized;
            var eyePosition = Origin + (Ducked ? HumanGameObject.DuckedEyeOffset : HumanGameObject.EyeOffset);
            return new Ray(eyePosition, direction);
        }

        public virtual void RunCommand(UserCmd cmd, bool prediction)
        {
            if (!Enabled || Dead)
            {
                return;
            }

            if(AngleOverride != InvalidAngleOverride)
            {
                Angles = AngleOverride;
                cmd.Angles = AngleOverride;
                AngleOverride = InvalidAngleOverride;
            }

            if (Frozen)
            {
                cmd.Buttons &= ~InputActions.MoveLeft;
                cmd.Buttons &= ~InputActions.MoveRight;
                cmd.Buttons &= ~InputActions.MoveForward;
                cmd.Buttons &= ~InputActions.MoveBack;
                cmd.Buttons &= ~InputActions.HandAction;
                cmd.Buttons &= ~InputActions.HandAction2;
            }

            CurrentCmd = cmd.Fields;
            MovementController?.ProcessInput(cmd);
            MovementController?.RunCommand(cmd.Fields, prediction);

            // Execute equippables now if:
            // 1. is server
            // 2. isn't server, but predicting
            // 3. isn't server, but is a networked client to simulate gunshots etc
            if(Game.IsHost 
                || (!Game.IsHost && prediction)
                || (!Game.IsHost && this != Local))
            {
                Equippables?.RunCommand(cmd.Fields);
            }

            if (Game.IsHost || prediction)
            {
                if (cmd.Buttons.HasFlag(InputActions.Flashlight))
                {
                    FlashlightOn = !FlashlightOn;
                }

                //Equippables?.RunCommand(cmd.Fields);
                _interactor?.RunCommand(cmd.Fields);

                if(Timeline != null && Timeline.Mode == TimelineMode.Record)
                {
                    Timeline.RecordTick();
                }
            }

            if (HumanGameObject)
            {
                HumanGameObject.OnRunCommand();
            }
        }

        public void Spawn(int teamNumber = 0)
        {
            Map.GetSpawnPoint(out Vector3 pos, out Vector3 angles, teamNumber);
            Origin = pos;
            Angles = angles;
            Velocity = Vector3.zero;
            BaseVelocity = Vector3.zero;
            Health = 100;

            if (Game.IsHost)
            {
                Dead = false;
                TimeDead = 0;
            }
        }

        public void Give(string itemName)
        {
            if (!Game.IsHost)
            {
                return;
            }

            var equippable = Game.EntityManager.SpawnEquippable();
            equippable.ItemName = itemName;
            equippable.HumanId = EntityId;
        }

        private void OnKilled()
        {
            if (HumanGameObject)
            {
                HumanGameObject.OnKilled(default);
            }

            if (Game.IsHost)
            {
                Equippables.DropAllItems();
            }

            Game.EntityManager.RaiseHumanKilled(this);

            TimeDead = 0;
        }

        private void OnSpawned()
        {
            if (HumanGameObject)
            {
                HumanGameObject.OnSpawned();
            }

            Game.EntityManager.RaiseHumanSpawned(this);
        }

        private void SetOwnerId(int value)
        {
            _ownerId = value;
            if (value == Game.ClientIndex)
            {
                Local = this;
                _hasAuthorityNextTick = true;
            }
            var player = Game.PlayerManager.FindPlayer(value);
            if(player != null)
            {
                player.Entity = this;
            }
        }

        public int HammerVelocity(bool horizontalOnly = true)
        {
            var vel = Velocity + BaseVelocity;
            if (horizontalOnly)
            {
                vel.y = 0;
            }
            return (int)(vel.magnitude / .0254f);
        }

        public void ClampVelocity(int xzMax, int yMax)
        {
            var maxY = yMax * .0254f;
            var maxXZ = xzMax * .0254f;
            var vel = Velocity;
            var xz = new Vector3(vel.x, 0, vel.z);
            xz = Vector3.ClampMagnitude(xz, maxXZ);
            xz.y = Mathf.Clamp(vel.y, -maxY, maxY);
            Velocity = xz;
        }

        private struct PunchData
        {
            public float InTime;
            public float StartTime;
            public Vector3 View;
            public Vector3 Aim;
            public Vector3 CurView;
            public Vector3 CurAim;
        }

        private List<PunchData> _punches = new List<PunchData>(128);
        public void Punch(Vector3 view, Vector3 aim)
        {
            _punches.Add(new PunchData()
            {
                StartTime = Game.ElapsedTime,
                View = view,
                Aim = aim,
                CurAim = Vector3.zero,
                CurView = Vector3.zero
            });
        }

        public Vector3 TotalAimPunch()
        {
            var result = Vector3.zero;
            foreach (var p in _punches)
            {
                result += p.CurAim;
            }
            return result;
        }

        public Vector3 TotalViewPunch()
        {
            var result = Vector3.zero;
            foreach (var p in _punches)
            {
                result += p.CurView;
            }
            return result;
        }

        private void TickPunches()
        {
            for (int i = _punches.Count - 1; i >= 0; i--)
            {
                if (_punches[i].InTime < 0.05f)
                {
                    var pd = _punches[i];
                    pd.InTime += Time.fixedDeltaTime;
                    pd.CurView = Vector3.Lerp(Vector3.zero, pd.View, pd.InTime / 0.05f);
                    pd.CurAim = Vector3.Lerp(Vector3.zero, pd.Aim, pd.InTime / 0.05f);
                    _punches[i] = pd;
                }
                else
                {
                    var t = Game.ElapsedTime - _punches[i].StartTime;
                    var view = _punches[i].View.SmoothStep(Vector3.zero, t);
                    var aim = _punches[i].Aim.SmoothStep(Vector3.zero, t);
                    if (view == Vector3.zero && aim == Vector3.zero)
                    {
                        _punches.RemoveAt(i);
                    }
                    else
                    {
                        var rep = _punches[i];
                        rep.CurAim = aim;
                        rep.CurView = view;
                        _punches[i] = rep;
                    }
                }
            }
        }

        private bool _deadInitted;
        private void SetIsDead(bool dead)
        {
            if(_dead == dead && _deadInitted)
            {
                return;
            }
            _deadInitted = true;
            _dead = dead;
            if (dead)
            {
                OnKilled();
            }
            else
            {
                OnSpawned();
            }
        }

        public void Damage(DamageInfo dmgInfo)
        {
            if (Game.IsHost)
            {
                if (!Game.EntityManager.FriendlyFire)
                {
                    var attacker = Game.EntityManager.FindEntity<Human>(dmgInfo.AttackerEntityId);
                    if(attacker != null)
                    {
                        var p1 = Game.PlayerManager.FindPlayer(this);
                        var p2 = Game.PlayerManager.FindPlayer(attacker);
                        if(p1 != null && p2 != null && p1.Team == p2.Team)
                        {
                            dmgInfo.Amount = 0;
                        }
                    }
                }
                var wasDead = Dead;
                Health -= dmgInfo.Amount;
                Dead = Health <= 0;
                dmgInfo.ResultedInDeath = !wasDead && Dead;
                dmgInfo.Viewpunch = new Vector3(-Random.Range(0.5f, 1.25f), Random.Range(-1f, 1f), 0);
                Punch(dmgInfo.Viewpunch, Vector3.zero);
                Game.EntityManager.BroadcastHumanDamaged(this, dmgInfo);
            }
        }

        protected override void OnEnabled()
        {
            SetOutOfGame(false);
        }

        protected override void OnDisabled()
        {
            SetOutOfGame(true);
        }

        private void SetOutOfGame(bool outOfGame)
        {
            DisableLagCompensation = outOfGame;

            if(outOfGame && Game.IsHost)
            {
                Equippables.DropAllItems();
            }

            if (HumanGameObject)
            {
                HumanGameObject.gameObject.SetActive(!outOfGame);
            }
        }

        public void SetAngles(Vector3 newAngles)
        {
            NetCommand(SetClAngleOverride, newAngles);
        }

        private void SetClAngleOverride(Vector3 ang)
        {
            AngleOverride = ang;
        }

    }
}

