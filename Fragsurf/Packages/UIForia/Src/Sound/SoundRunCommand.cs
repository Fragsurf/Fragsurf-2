using System;
using UIForia.Elements;
using UIForia.Parsing.Style;
using UIForia.Rendering;

namespace UIForia.Sound {
    public class SoundRunCommand : IRunCommand {

        public UISoundData soundData;

        public SoundRunCommand(RunCommandType cmdType, RunAction runAction = RunAction.Run) {
            RunCommandType = cmdType;
            RunAction = runAction;
        }

        public void Run(UIElement element, RunCommandType direction) {
            switch (RunAction) {
                case RunAction.Run:
                    element.application.SoundSystem.PlaySound(element, soundData);
                    break;
                case RunAction.Pause:
                    element.application.SoundSystem.PauseSound(element, soundData);
                    break;
                case RunAction.Resume:
                    element.application.SoundSystem.SoundResumed(element, soundData);
                    break;
                case RunAction.Stop:
                    element.application.SoundSystem.StopSound(element, soundData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public RunCommandType RunCommandType { get; set; }

        public RunAction RunAction { get; }

    }
}
