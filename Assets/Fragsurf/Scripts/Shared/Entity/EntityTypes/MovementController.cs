using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public abstract class MovementController
    {

        public MovementController(Human human)
        {
            Human = human;
            _reconciliator = new Reconciliator(human);
        }

        protected readonly Human Human;

        public virtual GameObject GroundObject { get; set; }
        public virtual bool Grounded => GroundObject != null;
        public virtual bool JustGrounded { get; set; }
        public virtual bool JustJumped { get; set; }
        public virtual bool Sprinting { get; set; }
        public virtual bool MouseControlsRotation => true;
        public virtual bool ShowsCursor => false;

        private Reconciliator _reconciliator;

        public void RunCommand(UserCmd.CmdFields userCmd, bool prediction)
        {
            if(Human == Human.Local && !prediction)
            {
                _reconciliator.StashServerCmd(userCmd);
                return;
            }

            if (Human.Game.IsHost && MouseControlsRotation)
            {
                Human.Angles = userCmd.Angles;
            }

            Sprinting = userCmd.Buttons.HasFlag(Movement.InputActions.Speed);

            ExecuteMovement(userCmd);

            if(Human == Human.Local && prediction)
            {
                userCmd.Origin = Human.Origin;
                userCmd.Velocity = Human.Velocity;
                userCmd.BaseVelocity = Human.BaseVelocity;
                _reconciliator.StashLocalCmd(userCmd);
            }
        }

        public virtual void Update() { }
        public virtual void ProcessInput(UserCmd userCmd) { }
        public abstract void ExecuteMovement(UserCmd.CmdFields cmd);

    }
}

