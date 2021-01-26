using System;
using UIForia.Animation;
using UIForia.Elements;
using UIForia.Parsing.Style;

namespace UIForia.Rendering {

    public class AnimationRunCommand : IRunCommand {

        public AnimationData animationData;

        public RunCommandType cmdType;

        public AnimationRunCommand(RunCommandType cmdType, RunAction runAction = RunAction.Run) {
            this.cmdType = cmdType;
            RunAction = runAction;
        }

        public void Run(UIElement element, RunCommandType runCommandType) {
            
            if (runCommandType == RunCommandType.Exit && cmdType == RunCommandType.EnterExit) {
                animationData.options.direction = AnimationDirection.Reverse;
            }
            
            switch (RunAction) {
                case RunAction.Run:
                    element.Animator.PlayAnimation(animationData);
                    break;

                case RunAction.Pause:
                    element.Animator.PauseAnimation(animationData);
                    break;

                case RunAction.Resume:
                    element.Animator.ResumeAnimation(animationData);
                    break;

                case RunAction.Stop:
                    element.Animator.StopAnimation(animationData);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public RunCommandType RunCommandType {
            get => cmdType;
        }

        public RunAction RunAction { get; }

    }

}