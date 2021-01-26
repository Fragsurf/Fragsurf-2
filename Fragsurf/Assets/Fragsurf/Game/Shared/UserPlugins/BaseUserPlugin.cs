using System;
using System.Collections.Generic;
using System.IO;
using Fragsurf.Shared.UserPlugins.Data;

namespace Fragsurf.Shared.UserPlugins
{
    public abstract class BaseUserPlugin : IUserPlugin
    {

        // PROPERTIES

        public FSGameLoop Game { get; set; }
        public IUserPluginDescriptor Descriptor { get; set; }
        public ILogSystem LogSystem { get; set; }
        public bool Loaded { get; protected set; }
        public string EntryFilePath => Path.Combine(Descriptor.Directory, Path.GetFileNameWithoutExtension(Descriptor.EntryFile) + FileExtension);
        public ConfigManager Config { get; private set; }

        private List<FSSharedScript> _addedScripts = new List<FSSharedScript>();

        // ABSTRACT

        public abstract string FileExtension { get; }
        public abstract void Action(string path, params object[] parameters);
        public abstract object Func(string path, params object[] parameters);
        public abstract object GetValue(string path);
        public abstract void RaiseEvent(string eventName, params object[] parameters);
        public abstract bool Initialize();
        protected abstract void _Load();
        protected abstract void _Unload();
        public virtual void Update() { }

        // METHODS

        public BaseUserPlugin(IUserPluginDescriptor descriptor)
        {
            Descriptor = descriptor;
            Config = new ConfigManager(this);
        }

        public void Load()
        {
            _Load();
        }

        public void Unload()
        {
            foreach(var script in _addedScripts)
            {
                script.Destroy();
            }
            _addedScripts.Clear();
            _Unload();
        }

    }

}

