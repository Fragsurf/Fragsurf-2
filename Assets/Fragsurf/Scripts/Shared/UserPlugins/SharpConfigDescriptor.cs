using System.IO;
using SharpConfig;

namespace Fragsurf.Shared.UserPlugins
{
    public class SharpConfigDescriptor : IUserPluginDescriptor
    {

        public SharpConfigDescriptor(string pluginDirectory)
        {
            _directory = pluginDirectory;
            _directoryName = new DirectoryInfo(_directory).Name;
            var filePath = pluginDirectory + '/' + InfoFileName;
            _config = Configuration.LoadFromFile(filePath);
        }

        private string _directory;
        private string _directoryName;
        private Configuration _config;

        private const string InfoFileName = "plugin.ini";

        public string Directory => _directory;
        public string DirectoryName => _directoryName;
        public string Name => GetConfigString("name", "{NAME}");
        public string Author => GetConfigString("author", "{AUTHOR}");
        public string Version => GetConfigString("version", "{VERSION}");
        public string[] Dependencies => GetStringArray("dependencies", null);
        public string Description => GetConfigString("description", "{DESCRIPTION}");
        public string EntryFile => GetConfigString("entryfile", "init");
        public PluginSpace Space => GetConfigEnum("space", PluginSpace.InGame);

        public string EntryFilePath(string extension)
        {
            return Directory
                + '/'
                + EntryFile
                + extension;
        }

        public string GetConfigString(string entryName, string def = null)
        {
            if (_config == null)
                return def;

            if (!_config["Plugin"].Contains(entryName))
                return def;

            return _config["Plugin"][entryName].StringValue;
        }

        public bool GetConfigBool(string entryName, bool def = false)
        {
            if (_config == null)
                return def;

            if (!_config["Plugin"].Contains(entryName))
                return def;

            return _config["Plugin"][entryName].BoolValue;
        }

        public string[] GetStringArray(string entryName, string[] def)
        {
            if (_config == null)
                return def;

            if (!_config["Plugin"].Contains(entryName))
                return def;

            if (!_config["Plugin"][entryName].IsArray)
                return def;

            return _config["Plugin"][entryName].StringValueArray;
        }

        public TEnum GetConfigEnum<TEnum>(string entryName, TEnum def)
            where TEnum : struct
        {
            if (_config == null)
                return def;

            if (!_config["Plugin"].Contains(entryName))
                return def;

            var value = _config["Plugin"][entryName].StringValue;
            System.Enum.TryParse(value, true, out def);
            return def;
        }

    }
}
