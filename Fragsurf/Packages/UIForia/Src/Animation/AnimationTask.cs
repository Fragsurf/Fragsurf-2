using System.Collections.Generic;
using UIForia.Systems;

namespace UIForia.Animation {

    public abstract class AnimationTask : UITask {

        public AnimationData animationData;
        public readonly IList<AnimationTriggerState> triggerStates;
        
        protected AnimationTask(AnimationData animationData, IList<AnimationTrigger> triggers) {
            this.animationData = animationData;
            if (triggers != null) {
                triggerStates = new List<AnimationTriggerState>(triggers.Count);
                for (int i = 0; i < triggers.Count; i++) {
                    triggerStates.Add(new AnimationTriggerState(triggers[i]));
                }
            }
        }

       
        
    }

}