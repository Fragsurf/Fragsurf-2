
namespace Fragsurf.Shared.UserPlugins
{
    public class LuaPluginLoader : BasePluginLoader
    {
        public override string PluginExtension => ".lua";

        protected override IUserPlugin _LoadPlugin(IUserPluginDescriptor descriptor, FSGameLoop game)
        {
            try
            {
                var plugin = new LuaPlugin(descriptor);
                plugin.Game = game;
                plugin.LogSystem = LogSystem;
                plugin.Load();
                return plugin;
            }
            catch (System.Exception e)
            {
                LogSystem?.PrintWarning(e.ToString());
            }

            return null;
        }

        protected override void _UnloadPlugin(IUserPlugin plugin)
        {
            plugin.Unload();
        }
    }
}

