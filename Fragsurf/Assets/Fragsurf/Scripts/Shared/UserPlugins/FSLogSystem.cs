namespace Fragsurf.Shared.UserPlugins
{
    public class FSLogSystem : ILogSystem
    {
        public void PrintError(string message)
        {
            DevConsole.WriteLine(message);
        }

        public void PrintColor(string message, UnityEngine.Color color)
        {
            DevConsole.WriteLine(message/*, color*/);
        }

        public void Print(string message)
        {
            DevConsole.WriteLine(message);
        }

        public void PrintWarning(string message)
        {
            DevConsole.WriteLine(message);
        }
    }
}
