using System;

namespace UIForia.Systems {

    public class CallbackTaskWithContextNoArg : UITask {

        private readonly Func<UITask, UITaskResult> task;

        public CallbackTaskWithContextNoArg(Func<UITask, UITaskResult> task) {
            this.task = task;
        }

        public override UITaskResult Run(float deltaTime) {
            return task(this);
        }

    }

}