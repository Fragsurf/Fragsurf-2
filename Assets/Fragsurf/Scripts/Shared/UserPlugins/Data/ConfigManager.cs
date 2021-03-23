using System.IO;
using SharpConfig;

namespace Fragsurf.Shared.UserPlugins.Data
{
    public class ConfigManager
    {

        ///// Constructor /////

        public ConfigManager(IUserPlugin plugin)
        {
            _plugin = plugin;
            _configPath = Path.Combine(plugin.Descriptor.Directory, "Data", "Config.ini");

            if(!File.Exists(_configPath))
            {
                _config = new Configuration();
            }
            else
            {
                _config = Configuration.LoadFromFile(_configPath);
            }
        }

        ///// Fields /////

        private string _configPath;
        private IUserPlugin _plugin;
        private Configuration _config;

        ///// Methods /////

        public Section this[int section]
        {
            get { return _config[section]; }
        }

        public Section this[string section]
        {
            get { return _config[section]; }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            if (_config == null)
                return;

            if (!File.Exists(_configPath))
                File.Create(_configPath);

            _config.SaveToFile(_configPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="entryName"></param>
        /// <param name="value"></param>
        public void Set(string section, string entryName, object value)
        {
            if (!_config.Contains(section))
                _config.Add(section);
            if (_config[section].Contains(entryName))
                _config[section][entryName].SetValue(value);
            else
                _config[section].Add(entryName, value);
        }

    }
}

