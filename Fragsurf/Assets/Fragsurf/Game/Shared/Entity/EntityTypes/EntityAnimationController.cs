using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public class EntityAnimationController
    {

        protected readonly Human Human;
        protected Animator Animator => Human.EntityGameObject.Animator;

        private bool _initialized;

        public EntityAnimationController(Human human)
        {
            Human = human;
        }

        public void Update() 
        { 
            if(Human == null
                || !Human.IsValid()
                || Human.EntityGameObject == null
                || Human.EntityGameObject.Animator == null)
            {
                return;
            }

            if (!_initialized)
            {
                Initialize();
            }

            OnUpdate();
        }

        protected virtual void OnUpdate() { }

        private void Initialize()
        {
            Animator.cullingMode = Human.Game.IsHost
                ? AnimatorCullingMode.AlwaysAnimate
                : AnimatorCullingMode.CullCompletely;
            _initialized = true;
        }

    }
}

