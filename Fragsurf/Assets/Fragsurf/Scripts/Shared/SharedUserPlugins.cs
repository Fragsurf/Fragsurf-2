using System.Collections.Generic;
using Fragsurf.Shared.Entity;
using Fragsurf.Maps;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;
using Fragsurf.Shared.UserPlugins;
using Fragsurf.Utility;

namespace Fragsurf.Shared
{
    public class SharedUserPlugins : FSComponent
    {

        private ILogSystem _logSystem = new FSLogSystem();
        private bool _inMap;
        private const string _loadTemplPacketLabel = "LoadTempPlugins";
        private const string _clearTempPacketLabel = "ClearTempPlugins";

        private List<IUserPluginLoader> _pluginLoaders
            = new List<IUserPluginLoader>()
            {
                //new JsPluginLoader(),
                new LuaPluginLoader()
            };

        protected override void _Hook()
        {
            Game.PlayerManager.OnPlayerConnected += OnPlayerConnected;
            Game.PlayerManager.OnPlayerDisconnected += OnPlayerDisconnected;
            Game.PlayerManager.OnPlayerIntroduced += OnPlayerIntroduced;
            Game.PlayerManager.OnPlayerRunCommand += OnPlayerRunCommand;
            Game.PlayerManager.OnPlayerChangedTeam += OnPlayerChangedTeam;
            Game.PlayerManager.OnChatCommand += OnPlayerChatCommand;
            Game.EntityManager.OnEntityAdded += OnEntityAdded;
            Game.EntityManager.OnEntityDestroyed += OnEntityDestroyed;
            Game.EntityManager.OnEntityUpdated += OnEntityUpdated;
            Game.EntityManager.OnHumanTrigger += OnHumanTrigger;
            Game.GameLoader.GameLoaded += OnGameLoaded;
            Game.GameLoader.GameUnloaded += OnGameUnloaded;
            Game.PlayerManager.OnPlayerPacketReceived += OnPlayerPacketReceived;
        }

        protected override void _Unhook()
        {
            Game.PlayerManager.OnPlayerConnected -= OnPlayerConnected;
            Game.PlayerManager.OnPlayerDisconnected -= OnPlayerDisconnected;
            Game.PlayerManager.OnPlayerIntroduced -= OnPlayerIntroduced;
            Game.PlayerManager.OnPlayerRunCommand -= OnPlayerRunCommand;
            Game.PlayerManager.OnPlayerChangedTeam -= OnPlayerChangedTeam;
            Game.PlayerManager.OnChatCommand -= OnPlayerChatCommand;
            Game.EntityManager.OnEntityAdded -= OnEntityAdded;
            Game.EntityManager.OnEntityDestroyed -= OnEntityDestroyed;
            Game.EntityManager.OnEntityUpdated -= OnEntityUpdated;
            Game.EntityManager.OnHumanTrigger -= OnHumanTrigger;
            Game.GameLoader.GameLoaded -= OnGameLoaded;
            Game.GameLoader.GameUnloaded -= OnGameUnloaded;
            Game.PlayerManager.OnPlayerPacketReceived -= OnPlayerPacketReceived;
        }

        protected override void _Initialize()
        {
            DevConsole.RegisterCommand("plugins.folder", "Opens the plugins folder", this, delegate (string[] args)
            {
                OpenInFileBrowser.Open(Structure.PluginsPath);
            });

            if(Game.IsHost)
            {
                DevConsole.RegisterCommand("plugins.reload", "Reloads all plugins", this, delegate (string[] args)
                {
                    LoadLoaders(Structure.PluginsPath);
                });

                DevConsole.RegisterCommand("plugins.sync", "Synchronize plugins for all clients", this, delegate (string[] args)
                {
                    SynchronizeWithPlayers();
                });
            }
        }

        protected override void _Destroy()
        {
            UnloadLoaders();
        }

        private void LoadLoaders(string rootDirectory)
        {
            UnloadLoaders();

            foreach (IUserPluginLoader loader in _pluginLoaders)
            {
                loader.LogSystem = _logSystem;
                loader.BuildDescriptors(rootDirectory);
            }

            LoadPlugins(PluginSpace.Global);

            if(_inMap)
            {
                LoadPlugins(PluginSpace.InGame);
            }

            if (Game.IsHost)
            {
                SynchronizeWithPlayers();
            }
        }

        private void UnloadLoaders()
        {
            //ClearAllEvents();

            foreach(var loader in _pluginLoaders)
            {
                loader.UnloadPlugins(PluginSpace.All);
            }
        }

        private void LoadPlugins(PluginSpace space)
        {
            foreach (var loader in _pluginLoaders)
            {
                loader.LoadPlugins(space, Game);
            }
        }

        private void UnloadPlugins(PluginSpace space)
        {
            //ClearAllEvents();

            foreach (IUserPluginLoader loader in _pluginLoaders)
            {
                loader.UnloadPlugins(space);
            }
        }

        //public void AddEvent(IUserPlugin plugin, string name, string path)
        //{
        //    _subscriptions.Add(new PluginEvent
        //    {
        //        Plugin = plugin,
        //        MethodPath = path,
        //        EventName = name
        //    });
        //}

        //public void RemoveEvent(IUserPlugin plugin, string name)
        //{
        //    for (int i = _subscriptions.Count - 1; i >= 0; i--)
        //    {
        //        if (_subscriptions[i].Plugin == plugin
        //            && string.Equals(_subscriptions[i].EventName, name))
        //        {
        //            _subscriptions.RemoveAt(i);
        //        }
        //    }
        //}

        //public void ClearAllEvents()
        //{
        //    _subscriptions.Clear();
        //}

        protected override void _Update()
        {
            foreach(var loader in _pluginLoaders)
            {
                try
                {
                    foreach (var plugin in loader.Plugins)
                    {
                        if (plugin.Loaded)
                        {
                            plugin.Update();
                        }
                    }
                }
                catch { }
            }
            InvokeEventSubscriptions("OnUpdate");
        }

        protected override void _Tick()
        {
            InvokeEventSubscriptions("OnTick");
        }

        private void OnGameLoaded()
        {
            // todo: needs testing.  this use to be in OnMapEvent event
            _inMap = true;
            LoadPlugins(PluginSpace.InGame);
            //

            if (Game.IsHost)
            {
                LoadLoaders(Structure.PluginsPath);
            }

            InvokeEventSubscriptions("OnGameLoaded");
        }

        private void OnGameUnloaded()
        {
            // todo: needs testing.  this use to be in OnMapEvent event
            _inMap = false;
            UnloadPlugins(PluginSpace.InGame);
            //

            InvokeEventSubscriptions("OnGameUnloaded");
        }

        private void OnEntityAdded(NetEntity entity)
        {
            InvokeEventSubscriptions("OnEntityAdded", entity);
        }

        private void OnEntityDestroyed(NetEntity entity)
        {
            InvokeEventSubscriptions("OnEntityDestroyed", entity);
        }

        private void OnEntityUpdated(NetEntity entity, int remoteTick, double remoteTime)
        {
            InvokeEventSubscriptions("OnEntityUpdated", entity);
        }

        private void OnHumanTrigger(NetEntity human, Actors.FSMTrigger trigger, TriggerEventType type, float offset = 0f)
        {
            InvokeEventSubscriptions("OnHumanTrigger", human, trigger, type, offset);
        }

        private void OnPlayerChangedTeam(IPlayer player)
        {
            InvokeEventSubscriptions("OnPlayerChangedTeam", player);
        }

        private void OnPlayerChatCommand(IPlayer player, string[] args)
        {
            InvokeEventSubscriptions("OnPlayerChatCommand", player, args);
        }

        private void OnPlayerConnected(IPlayer player)
        {
            InvokeEventSubscriptions("OnPlayerConnected", player);
        }

        private void OnPlayerDisconnected(IPlayer player)
        {
            InvokeEventSubscriptions("OnPlayerDisconnected", player);
        }

        private void OnPlayerIntroduced(IPlayer player)
        {
            InvokeEventSubscriptions("OnPlayerIntroduced", player);

            SynchronizeWithPlayer(player);
        }

        private void OnPlayerRunCommand(IPlayer player)
        {
            InvokeEventSubscriptions("OnPlayerRunCommand", player);
        }

        private List<FSFileInfo> _filesToSync = new List<FSFileInfo>();

        private async void SynchronizeWithPlayers()
        {
            await FileSystem.BuildAsync();

            if (Game == null)
            {
                UnityEngine.Debug.LogError("Game is null!");
                return;
            }

            _filesToSync.Clear();

            foreach (var provider in FileSystem.FileProviders)
            {
                foreach (var file in provider.Files)
                {
                    if (!(file.Value.Extension == ".lua" || file.Value.Extension == ".ini" || file.Value.Extension == ".html"))
                    {
                        continue;
                    }
                    _filesToSync.Add(file.Value);
                }
            }

            foreach (var player in Game.PlayerManager.Players)
            {
                if (player.Introduced)
                {
                    SynchronizeWithPlayer(player);
                }
            }
        }

        private async void SynchronizeWithPlayer(IPlayer player)
        {
            var cp1 = PacketUtility.TakePacket<CustomPacket>();
            cp1.Label = _clearTempPacketLabel;
            cp1.Sc = SendCategory.FileTransfer;
            Game.Network.SendPacket(player.ClientIndex, cp1);

            foreach(var file in _filesToSync)
            {
                if (file.FullPath.IsSubPathOf(Structure.PluginsPath))
                {
                    var result = await Game.FileTransfer.UploadFileAsync(player, file, true);
                    if (result != FileTransfer.TransferStatus.Success)
                    {
                        UnityEngine.Debug.LogError("Failed to transfer plugin file: " + file.RelativePath);
                    }
                }
            }

            var cp = PacketUtility.TakePacket<CustomPacket>();
            cp.Label = _loadTemplPacketLabel;
            cp.Sc = SendCategory.FileTransfer;
            Game.Network.SendPacket(player.ClientIndex, cp);
        }

        private void OnPlayerPacketReceived(IPlayer player, IBasePacket packet)
        {
            if(Game.IsHost || !(packet is CustomPacket cp))
            {
                return;
            }
            if(cp.Label == _loadTemplPacketLabel)
            {
                LoadLoaders(Structure.PluginsTempPath);
            }
            else if(cp.Label == _clearTempPacketLabel)
            {
                FileSystem.EmptyTempFolder();
            }
        }

        public IUserPlugin FindPlugin(string pluginName)
        {
            foreach (IUserPluginLoader loader in _pluginLoaders)
            {
                foreach (IUserPlugin plugin in loader.Plugins)
                {
                    if (string.Equals(plugin.Descriptor.Name, pluginName))
                        return plugin;
                }
            }
            return null;
        }

        public void InvokeEventSubscriptions(string eventName, params object[] parameters)
        {
            foreach (IUserPluginLoader loader in _pluginLoaders)
            {
                foreach (IUserPlugin plugin in loader.Plugins)
                {
                    if (!plugin.Loaded)
                        continue;
                    plugin.RaiseEvent(eventName, parameters);
                }
            }
            //foreach (var ev in _subscriptions)
            //{
            //    if (!ev.Plugin.Loaded)
            //        continue;
            //    if (!string.Equals(ev.EventName, eventName, System.StringComparison.InvariantCultureIgnoreCase))
            //        continue;
            //    ev.Plugin.Action(ev.MethodPath, args);
            //}
        }

        //public void InvokeUpMethod(string methodName, params object[] parameters)
        //{
        //    foreach (IUserPluginLoader loader in _pluginLoaders)
        //    {
        //        foreach (IUserPlugin plugin in loader.Plugins)
        //        {
        //            if (!plugin.Loaded)
        //                continue;
        //            plugin.RaiseEvent(methodName, parameters);
        //        }
        //    }
        //}
    }
}

