using System.IO;
using System.Collections.Generic;
using SharpConfig;

namespace Fragsurf.Shared
{
    public class ConfigDirectory
    {

        ///// Fields /////

        private Dictionary<Configuration, string> _configs = new Dictionary<Configuration, string>();

        ///// Properties /////

        public Dictionary<Configuration, string> Configs => _configs;

        ///// Methods /////

        public void ExecuteFiles(string directory, string primaryName)
        {
            if (!Directory.Exists(directory))
                return;

            var files = Directory.GetFiles(directory, "*.ini");
            if (files.Length == 0)
                return;

            _configs.Clear();
            for (int i = 0; i < files.Length; i++)
            {
                DevConsole.WriteLine($"Executing file: {files[i]}");
                var config = Read(files[i], primaryName);
                if(config != null)
                {
                    _configs.Add(config, files[i]);
                }
            }
        }

        public void WriteAll()
        {
            foreach(KeyValuePair<Configuration, string> pair in _configs)
            {
                pair.Key.SaveToFile(pair.Value);
            }
        }

        public void ExecutePostLoad()
        {
            foreach (var cfg in _configs.Keys)
            {
                foreach(var section in cfg)
                {
                    if(section.Name != "PostLoad")
                    {
                        continue;
                    }
                    foreach (Setting setting in section)
                    {
                        if (setting.IsArray)
                            continue;
                        var lineToExecute = setting.Name + " " + setting.RawValue;
                        DevConsole.ExecuteLine(lineToExecute);
                    }
                }
            }
        }

        private Configuration Read(string path, string primaryName)
        {
            try
            {
                var config = Configuration.LoadFromFile(path);
                if (config == null)
                {
                    DevConsole.WriteLine("Invalid config path:" + path);
                    return null;
                }
                foreach(Section section in config)
                {
                    if(section.Name == "PostLoad"
                        || section.Name == "Binds")
                    {
                        continue;
                    }

                    foreach(Setting setting in section)
                    {
                        if (setting.IsArray)
                            continue;
                        var lineToExecute = setting.Name + " " + setting.RawValue;
                        DevConsole.ExecuteLine(lineToExecute);
                    }
                }
                return config;
            }
            catch(System.Exception e)
            {
                DevConsole.WriteLine(e.ToString());
            }

            return null;
        }

    }
}

