using System;
using UIForia.Util;

namespace UIForia.Systems {

    public class UITaskGroup : UITask {

        // run all tasks and complete when all are done or any cancel
        private readonly LightList<TaskStatusPair> taskStatusPairs;

        public UITaskGroup() {
            this.taskStatusPairs = new LightList<TaskStatusPair>(4);
        }

        public void AddTask(UITask task) {
            taskStatusPairs.Add(new TaskStatusPair(task, UITaskState.Uninitialized));
        }

        public void AddTask(Func<float, UITaskResult> task) {
            taskStatusPairs.Add(new TaskStatusPair(new CallbackTask(task), UITaskState.Uninitialized));
        }

        public void AddTask(Func<UITaskResult> task) {
            taskStatusPairs.Add(new TaskStatusPair(new CallbackTaskNoArg(task), UITaskState.Uninitialized));
        }

        public void RemoveTask(UITask task) {
            int index = taskStatusPairs.FindIndex(task, (t, item) => item == t.task);
            if (index != -1) {
                taskStatusPairs.RemoveAt(index);
            }
        }

        public void CancelTask(UITask task) {
            int index = taskStatusPairs.FindIndex(task, (t, item) => item == t.task);
            if (index != -1) {
                task.OnCancelled();
                taskStatusPairs[index] = new TaskStatusPair(task, UITaskState.Cancelled);
            }
        }

        public void PauseTask(UITask task) {
            int index = taskStatusPairs.FindIndex(task, (t, item) => item == t.task);
            if (index != -1) {
                task.OnPaused();
                taskStatusPairs[index] = new TaskStatusPair(task, UITaskState.Paused);
            }
        }

        public override UITaskResult Run(float deltaTime) {
            TaskStatusPair[] pairs = taskStatusPairs.Array;
            int completedCount = 0;
            int failedCount = 0;

            for (int i = 0; i < taskStatusPairs.Count; i++) {
                TaskStatusPair pair = pairs[i];
                UITaskState currentTaskState = pair.state;
                UITask task = pair.task;

                if (currentTaskState == UITaskState.Uninitialized) {
                    currentTaskState = UITaskState.Pending;
                    task.OnInitialized();
                }

                if ((currentTaskState & (UITaskState.Pending | UITaskState.Running)) != 0) {
                    UITaskResult result = task.Run(currentTaskState == UITaskState.Pending ? 0 : deltaTime);
                    switch (result) {
                        case UITaskResult.Running:
                            break;
                        case UITaskResult.Completed:
                            task.OnCompleted();
                            break;
                        case UITaskResult.Restarted:
                            break;
                        case UITaskResult.Failed:
                            task.OnFailed();
                            break;
                        case UITaskResult.Cancelled:
                            task.OnCancelled();
                            break;
                        case UITaskResult.Paused: 
                            task.OnPaused();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (currentTaskState == UITaskState.Completed) {
                    completedCount++;
                }

                if (currentTaskState == UITaskState.Failed) {
                    break;
                }
            }

            if (failedCount > 0) {
                return UITaskResult.Failed;
            }

            if (completedCount == taskStatusPairs.Count) {
                return UITaskResult.Completed;
            }

            return UITaskResult.Running;
        }

    }

}