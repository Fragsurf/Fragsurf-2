using Fragsurf.Maps;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public class Human : NetEntity
    {

        public MovementController MovementController;
        public CameraController CameraController;
        public EntityAnimationController AnimationController;

        private int _ownerId = -1;
        private bool _hasAuthorityNextTick;

        public Human(FSGameLoop game) 
            : base(game)
        {
            _autoRecordTimeline = false;
        }

        public static Human Local { get; set; }
        public HumanGameObject HumanGameObject => EntityGameObject as HumanGameObject;
        public bool IsFirstPerson { get; set; }
        public EquippableManager Equippables { get; private set; } = new EquippableManager();

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

        protected override void _Start()
        {
            EntityGameObject = new GameObject("Human").AddComponent<HumanGameObject>();
            MovementController = new DefaultMovementController(this);
            CameraController = new FirstPersonCameraController(this);
        }

        protected override void _Delete()
        {
            if(Local == this)
            {
                Local = null;
            }
        }

        protected override void _Tick()
        {
            // gives it one tick to update origin & angles from authority before taking control
            if (_hasAuthorityNextTick)
            {
                _hasAuthorityNextTick = false;
                InterpolationMode = InterpolationMode.Frame;
                HasAuthority = true;
            }
        }

        protected override void _Update()
        {
            MovementController?.Update();
            AnimationController?.Update();
        }

        public Ray GetEyeRay()
        {
            return new Ray();
        }

        public virtual void RunCommand(UserCmd cmd, bool prediction)
        {
            MovementController?.ProcessInput(cmd);
            MovementController?.RunCommand(cmd.Fields, prediction);
            EntityGameObject.SendMessage("OnHumanRunCommand");

            if(Timeline != null && Timeline.Mode == TimelineMode.Record && (prediction || Game.IsHost))
            {
                Timeline?.RecordTick();
            }
        }

        public void Spawn(int teamNumber = 0)
        {
            Map.GetSpawnPoint(out Vector3 pos, out Vector3 angles, teamNumber);
            Origin = pos;
            Angles = angles;
            Velocity = Vector3.zero;
            BaseVelocity = Vector3.zero;
        }

        private void OnKilled()
        {
            // disable colliders (maybe disable gameobject while ragdoll is active?)
        }

        private void OnSpawned()
        {
            
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

    }
}

