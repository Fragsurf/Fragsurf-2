using System;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    internal interface IUITaskRunner {

        UITask AddTask(UITask task);
        void CompleteTask(UITask task);
        void FailTask(UITask task);
        void CancelTask(UITask task);
        void RestartTask(UITask task);

    }

    internal struct TaskStatusPair {

        public readonly UITask task;
        public readonly UITaskState state;

        public TaskStatusPair(UITask task, UITaskState state) {
            this.task = task;
            this.state = state;
        }

    }

    // owned by a UIForia Application
    public class UITaskSystem : IUITaskRunner {

        private const UITaskState k_CanBeEnded = UITaskState.Pending | UITaskState.Restarting | UITaskState.Running;

        private LightList<UITask> thisFrame;
        private LightList<UITask> nextFrame;

        public UITaskSystem() {
            thisFrame = new LightList<UITask>(16);
            nextFrame = new LightList<UITask>(16);
        }

        public int ActiveTaskCount => thisFrame.Count;

        internal void OnDestroy() {
            thisFrame.QuickClear();
            nextFrame.QuickClear();
        }

        internal void OnUpdate() {
            UITask[] tasks = thisFrame.Array;
            float delta = Time.unscaledDeltaTime;

            int count = thisFrame.Count;
            for (int i = 0; i < count; i++) {
                UITask task = tasks[i];
                UITaskState state = task.state;

                float time = delta;

                if (state == UITaskState.Pending) {
                    task.OnInitialized();
                    task.StartTime = Time.unscaledTime;
                    state = UITaskState.Running;
                    time = 0;
                }
                else if (state == UITaskState.Restarting) {
                    task.RestartTime = Time.unscaledTime;
                    time = 0;
                }

                if ((state & (UITaskState.Running | UITaskState.Restarting)) != 0) {
                    task.FrameCount++;
                    task.ElapsedTime += time;
                    UITaskResult result = task.Run(time);
                    switch (result) {
                        case UITaskResult.Running:
                            task.state = UITaskState.Running;
                            nextFrame.Add(task);
                            break;

                        case UITaskResult.Completed:
                            task.state = UITaskState.Completed;
                            task.OnCompleted();
                            break;

                        case UITaskResult.Restarted:
                            task.ResetCount++;
                            task.state = UITaskState.Restarting;
                            nextFrame.Add(task);
                            break;

                        case UITaskResult.Failed:
                            task.state = UITaskState.Failed;
                            task.OnFailed();
                            task.owner = null;
                            break;

                        case UITaskResult.Cancelled:
                            task.state = UITaskState.Cancelled;
                            task.OnCancelled();
                            task.owner = null;
                            break;

                        default:
                            task.state = UITaskState.Cancelled;
                            task.OnCancelled();
                            task.owner = null;
                            break;
                    }
                }
            }

            LightList<UITask> swap = nextFrame;
            nextFrame = thisFrame;
            thisFrame = swap;
            nextFrame.QuickClear();
        }

        public UITask AddTask(UITask task) {
            if (task.owner != null) {
                throw new Exception("Tasks can only be added to a task runner once.");
            }

            task.owner = this;
            task.state = UITaskState.Pending;
            thisFrame.Add(task);
            return task;
        }

        public void CompleteTask(UITask task) {
            if (task.owner == null) {
                throw new Exception("Tasks can only be completed by their owner.");
            }

            if (task.owner != this) {
                throw new Exception("Tasks can only be completed by their owner.");
            }

            if ((task.state & k_CanBeEnded) != 0) {
                task.state = UITaskState.Completed;
                task.OnCompleted();
                thisFrame.Remove(task);
                task.owner = null;
            }
        }

        public void FailTask(UITask task) {
            if (task.owner == null) {
                throw new Exception("Tasks can only be failed by their owner.");
            }

            if (task.owner != this) {
                throw new Exception("Tasks can only be failed by their owner.");
            }

            if ((task.state & k_CanBeEnded) != 0) {
                task.state = UITaskState.Failed;
                task.OnFailed();
                thisFrame.Remove(task);
                task.owner = null;
            }
        }


        public void CancelTask(UITask task) {
            if (task.owner == null) {
                throw new Exception("Tasks can only be cancelled by their owner.");
            }

            if (task.owner != this) {
                throw new Exception("Tasks can only be cancelled by their owner.");
            }

            if ((task.state & k_CanBeEnded) != 0) {
                task.state = UITaskState.Cancelled;
                task.OnCancelled();
                thisFrame.Remove(task);
                task.owner = null;
            }
        }

        public void RestartTask(UITask task) {
            if (task.owner == null) {
                throw new Exception("Tasks can only be cancelled by their owner.");
            }

            if (task.owner != this) {
                throw new Exception("Tasks can only be cancelled by their owner.");
            }

            if (task.state != UITaskState.Pending && task.state != UITaskState.Restarting) {
                task.state = UITaskState.Restarting;
                task.OnRestarted();
                task.ResetCount++;
            }
        } 

    }

}