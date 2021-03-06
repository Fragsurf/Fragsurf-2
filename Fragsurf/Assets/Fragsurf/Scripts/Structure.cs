using Fragsurf.Utility;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fragsurf
{
    public static class Structure
    {
        public const string Version = "0.0.1";
        public const int AppId = 1033410;
        public const int ServerAppId = 1035540;

        static Structure()
        {
            //var ci = new System.Globalization.CultureInfo("de-DE");
            //System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            //System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
        }

        public static bool DedicatedServer => LaunchParams.Contains("server") || SceneManager.GetActiveScene().name == "GameServer";
        public static string RootPath => Directory.GetParent(Application.dataPath).ToString();
        public static string RuntimePath => Application.streamingAssetsPath;
        public static string MapsFolder => Path.Combine(RuntimePath, "Maps");
        public static string TempPath => Path.Combine(Application.streamingAssetsPath, "temp");
        public static string ConfigsPath => Path.Combine(RuntimePath, "Configs");
        public static string GamemodeDataPath => Path.Combine(RuntimePath, "Gamemodes");
        public static string LogsPath => Path.Combine(RootPath, "Logs");
        public static string PluginsPath => Path.Combine(RuntimePath, "Plugins");
        public static string PluginsTempPath => Path.Combine(TempPath, "Plugins");
        public static string SavePath => Path.Combine(Application.persistentDataPath, "Saves");

    }

    public static class LaunchParams
    {
        public static readonly SimpleCommandLineParser CommandLine;
        public static IDictionary<string, string[]> Arguments => CommandLine.Arguments;
        public static bool Contains(string value) => CommandLine.Contains(value);

        static LaunchParams()
        {
            CommandLine = new SimpleCommandLineParser(System.Environment.GetCommandLineArgs());
        }
    }
}
