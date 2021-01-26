using UIForia.Elements;
using UIForia.Parsing.Style;

namespace UIForia.Rendering {
    
    public interface IRunCommand {
        
        void Run(UIElement element, RunCommandType direction);
        RunCommandType RunCommandType { get; }
        RunAction RunAction { get; }

    }
}
