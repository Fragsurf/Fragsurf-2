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

        protected GameObject GroundObject;
        protected readonly Human Human;

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

            if (Human.Game.IsServer && MouseControlsRotation)
            {
                Human.Angles = userCmd.Angles;
            }

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

