using System;

namespace UIForia.Systems {

    public class CallbackTask : UITask {

        private readonly Func<float, UITaskResult> task;

        public CallbackTask(Func<float, UITaskResult> task) {
            this.task = task;
        }

        public override UITaskResult Run(float deltaTime) {
            return task(deltaTime);
        }

    }

}