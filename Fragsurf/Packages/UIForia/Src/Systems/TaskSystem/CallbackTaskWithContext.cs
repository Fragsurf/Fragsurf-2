using System;

namespace UIForia.Systems {

    public class CallbackTaskWithContext : UITask {

        private readonly Func<UITask, float, UITaskResult> task;

        public CallbackTaskWithContext(Func<UITask, float, UITaskResult> task) {
            this.task = task;
        }

        public override UITaskResult Run(float deltaTime) {
            return task(this, deltaTime);
        }

    }

}