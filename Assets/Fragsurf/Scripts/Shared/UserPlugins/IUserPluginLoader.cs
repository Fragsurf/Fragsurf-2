using System.Collections.Generic;

namespace Fragsurf.Shared.UserPlugins
{
    public interface IUserPluginLoader
    {
        string PluginExtension { get; }
        ILogSystem LogSystem { get; set; }
        string RootDirectory { get; }
        List<IUserPlugin> Plugins { get; }
        void BuildDescriptors(string rootDirectory);
        void LoadPlugins(PluginSpace space, FSGameLoop game);
        void UnloadPlugins(PluginSpace space);
        void ReloadPlugins(PluginSpace space);
    }
}

