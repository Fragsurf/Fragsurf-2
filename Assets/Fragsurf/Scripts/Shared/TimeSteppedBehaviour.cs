using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.Shared 
{
    public class TimeSteppedBehaviour : MonoBehaviour
    {

        public float ElapsedTime { get; private set; }
        public float Alpha { get; private set; }
        public float DeltaTime { get; private set; }
        public float FixedDeltaTime { get; private set; }
        public int CurrentTick { get; private set; }

        protected virtual void Awake()
        {
            TimeStep.Instance.OnFrame.AddListener(_OnFrame);
            TimeStep.Instance.OnTick.AddListener(_OnTick);
        }

        protected virtual void OnDestroy()
        {
            if (TimeStep.Instance)
            {
                TimeStep.Instance.OnFrame.RemoveListener(_OnFrame);
                TimeStep.Instance.OnTick.RemoveListener(_OnTick);
            }
        }

        private void _OnFrame(float elapsedTime, float deltaTime, float alpha)
        {
            ElapsedTime = elapsedTime;
            DeltaTime = deltaTime;
            Alpha = alpha;

            OnFrame();
        }

        private void _OnTick(float elapsedTime, float fixedDeltaTime)
        {
            ElapsedTime = elapsedTime;
            FixedDeltaTime = fixedDeltaTime;
            CurrentTick++;

            OnTick();
        }

        protected virtual void OnFrame() { }
        protected virtual void OnTick() { }

    }
}


