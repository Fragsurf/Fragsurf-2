using UnityEngine;

namespace Fragsurf.Shared.UserPlugins
{
    public interface ILogSystem
    {
        void Print(string message);
        void PrintColor(string message, Color color);
        void PrintWarning(string message);
        void PrintError(string message);
    }
}

