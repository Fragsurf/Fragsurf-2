using System;
using System.IO;
using System.Collections.Generic;

namespace Fragsurf.Shared.UserPlugins
{
    public abstract class BasePluginLoader : IUserPluginLoader
    {

        public BasePluginLoader()
        {
            Plugins = new List<IUserPlugin>();
            PluginDescriptors = new List<IUserPluginDescriptor>();
        }

        protected FSGameLoop _gameReference;

        public string RootDirectory { get; private set; }
        public ILogSystem LogSystem { get; set; }
        public List<IUserPlugin> Plugins { get; private set; }
        public List<IUserPluginDescriptor> PluginDescriptors { get; private set; }
        public abstract string PluginExtension { get; }

        public void BuildDescriptors(string rootDirectory)
        {
            RootDirectory = rootDirectory;

            PluginDescriptors.Clear();

            LogSystem?.Print("Scanning for plugins in " + RootDirectory);

            if (!Directory.Exists(RootDirectory))
            {
                return;
            }

            var directories = Directory.GetDirectories(RootDirectory);

            foreach (string dir in directories)
            {
                var descriptor = FindDescriptorByDirectory(dir);

                if (descriptor != null)
                {
                    continue;
                }

                try
                {
                    descriptor = new SharpConfigDescriptor(dir);
                    PluginDescriptors.Add(descriptor);
                }
                catch (Exception e)
                {
                    LogSystem?.PrintWarning(e.ToString());
                }
            }
        }

        public void LoadPlugins(PluginSpace space, FSGameLoop game)
        {
            UnloadPlugins(space);

            _gameReference = game;

            foreach(IUserPluginDescriptor descriptor in PluginDescriptors)
            {
                if(space == PluginSpace.All || space == descriptor.Space)
                {
                    LoadPlugin(descriptor, game);
                }
            }
        }

        private IUserPluginDescriptor FindDescriptor(string name)
        {
            foreach(IUserPluginDescriptor descriptor in PluginDescriptors)
            {
                if (string.Equals(descriptor.Name, name))
                    return descriptor;
            }
            return null;
        }

        private bool DescriptorIsLoaded(IUserPluginDescriptor descriptor)
        {
            foreach(IUserPlugin plugin in Plugins)
            {
                if (plugin.Descriptor == descriptor)
                    return true;
            }
            return false;
        }

        public bool LoadPlugin(IUserPluginDescriptor descriptor, FSGameLoop game)
        {
            if (DescriptorIsLoaded(descriptor))
            {
                return false;
            }

            if (!File.Exists(descriptor.EntryFilePath(PluginExtension)))
            {
                return false;
            }

            if (descriptor.Dependencies != null && descriptor.Dependencies.Length > 0)
            {
                LogSystem.PrintColor(descriptor.Name + " has " + descriptor.Dependencies.Length + " dependencies", UnityEngine.Color.cyan);
                foreach (string s in descriptor.Dependencies)
                {
                    LogSystem.PrintColor("> " + s, UnityEngine.Color.cyan);
                    var dependency = FindDescriptor(s);
                    if(dependency == null || !LoadPlugin(dependency, game))
                    {
                        var couldntFind = string.Format("Couldn't load plugin: {0}, missing dependency: {1}",
                            descriptor.Name,
                            s);
                        LogSystem.PrintWarning(couldntFind);
                        return false;
                    }
                }
            }

            var plugin = _LoadPlugin(descriptor, game);
            if (plugin != null)
            {
                Plugins.Add(plugin);

                var successMessage = '[' + descriptor.Space.ToString() + ']'
                    + "Plugin loaded: "
                    + descriptor.Name
                    + " ("
                    + descriptor.Version
                    + ") by "
                    + descriptor.Author
                    + " ["
                    + PluginExtension
                    + ']';

                LogSystem.PrintColor(successMessage, UnityEngine.Color.green);

                return true;
            }

            return false;
        }

        public void UnloadPlugins(PluginSpace space)
        {
            for(int i = Plugins.Count - 1; i >= 0; i--)
            {
                if (space == PluginSpace.All || space == Plugins[i].Descriptor.Space)
                {
                    _UnloadPlugin(Plugins[i]);
                    Plugins.RemoveAt(i);
                }
            }
        }

        public void ReloadPlugins(PluginSpace space = PluginSpace.All)
        {
            UnloadPlugins(space);
            LoadPlugins(space, _gameReference);
        }

        protected abstract IUserPlugin _LoadPlugin(IUserPluginDescriptor descriptor, FSGameLoop game);
        protected abstract void _UnloadPlugin(IUserPlugin plugin);

        protected IUserPluginDescriptor FindDescriptorByDirectory(string directory)
        {
            foreach (IUserPluginDescriptor desc in PluginDescriptors)
            {
                if (string.Equals(desc.Directory, directory))
                    return desc;
            }
            return null;
        }

    }
}
