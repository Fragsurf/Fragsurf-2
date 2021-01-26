using UIForia.Util;

namespace UIForia.Rendering {

    public struct UIStyleRunCommand {

        public UIStyle style { get; internal set; }
        public LightList<IRunCommand> runCommands { get; internal set; }

        public static UIStyleRunCommand CreateInstance() {
            return new UIStyleRunCommand() {
                style = new UIStyle()
            };
        }

    }

}