using Fragsurf.Shared;

namespace Fragsurf.Client
{
    public class ConnectLaunchParam : FSSharedScript
    {
        protected override void _Start()
        {
            if (LaunchParams.Contains("connect"))
            {
                DevConsole.ExecuteLine($"net.connect {LaunchParams.CommandLine.Arguments["connect"][0]}");
            }
        }
    }
}
