using System;
using System.Threading;
using System.Threading.Tasks;
using Fragsurf.Maps;
using Fragsurf.Shared.Packets;
using Fragsurf.Client;
using Fragsurf.Server;
using UnityEngine.SceneManagement;
using Steamworks;
using Steamworks.Ugc;
using UnityEngine;
using Fragsurf.UI;

namespace Fragsurf.Shared
{
    public enum GameLoadResult
    {
        None,
        Cancelled,
        FailedToConnect,
        FailedToSync,
        MissingMapChange,
        FailedToLoadMap,
        FailedToLoadGamemode,
        MissingBackfill,
        Success
    }

    public enum GameLoaderState 
    { 
        New,
        Playing,
        Creating,
        Joining,
        Unloading,
        ChangingMap,
        Destroyed
    }

    // todo: rewrite game join & creation process so it's not such a clusterfuck and hard to work with
    public class GameLoader : FSComponent
    {
        public event Action PreGameLoaded;
        public event Action GameLoaded;

        public bool Loading;
        public string LoadingHint;
        private bool _cancelled;
        private CancellationTokenSource _cts;

        public static bool RetryRequested;
        public static string LastJoinedAddress;
        public static int LastJoinedPort;
        public static string LastJoinedPassword;

        public GameLoaderState State { get; private set; }

        protected override void _Start()
        {
            if(!Game.IsHost)
            {
                (Game.Network as ClientSocketManager).OnStatusChanged += Socket_OnStatusChanged;
            }
        }

        protected override void _Destroy()
        {
            if (!Game.IsHost && Game.Network is ClientSocketManager csm)
            {
                csm.OnStatusChanged -= Socket_OnStatusChanged;
            }

            PreGameLoaded = null;
            GameLoaded = null;
            State = GameLoaderState.Destroyed;
        }

        private void Socket_OnStatusChanged(ClientSocketStatus status, string reason = null)
        {
            if (Game.IsHost)
            {
                return;
            }

            if(status == ClientSocketStatus.Disconnected)
            {
                Game.Destroy();
            }
        }

        public void Cancel()
        {
            _cancelled = true;
            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
            }
            catch(Exception e) { Debug.LogError(e.Message); }
        }

        public async Task<GameLoadResult> JoinGameAsync(string address, int port = 0, string password = null)
        {
            if(State != GameLoaderState.New)
            {
                Debug.LogError($"{Game.IsHost} Can't join while state is: {State}");
                var m = UGuiManager.Instance.Find<Modal_Dialog>();
                if (m)
                {
                    m.Popup("Couldn't Join", "Leave your current game first");
                }
                return GameLoadResult.None;
            }

            RetryRequested = false;
            State = GameLoaderState.Joining;
            _cancelled = false;

            FileSystem.EmptyTempFolder();

            Loading = true;
            var result = await _JoinGameAsync(address, port, password);
            Loading = false;

            if (result != GameLoadResult.Success)
            {
                return result;
            }

            State = GameLoaderState.Playing;

            LastJoinedAddress = address;
            LastJoinedPort = port;
            LastJoinedPassword = password;

            return result;
        }

        private async Task<GameLoadResult> _JoinGameAsync(string address, int port = 0, string password = null)
        {
            if (Game.IsHost)
            {
                return GameLoadResult.None;
            }

            // 1. Connect
            LoadingHint = "Connecting to host";
            if (_cancelled) { return GameLoadResult.Cancelled; }

            var connectionResult = await (Game.Network as ClientSocketManager).ConnectAsync(address, port, password);
            if(connectionResult != ClientSocketStatus.Connected)
            {
                return GameLoadResult.FailedToConnect;
            }

            // 2. Sync files
            //LoadingHint = "Synchronizing files";
            //if (_cancelled) { return GameLoadResult.Cancelled; }

            //_cts?.Dispose();
            //_cts = new CancellationTokenSource();
            //var syncResult = await Game.GetFSComponent<FileDownloader>().SyncWithHostAsync(_cts);
            //_cts?.Dispose();
            //if (syncResult != FileDownloader.SyncState.Completed)
            //{
            //    return GameLoadResult.FailedToSync;
            //}

            // 3. Request ClientIndex and what map, gamemode to load
            LoadingHint = "Requesting game info";
            if (_cancelled) { return GameLoadResult.Cancelled; }

            Game.Network.BroadcastPacket(PacketUtility.TakePacket<MapChange>());
            var mapChange = await (Game.Network as ClientSocketManager).WaitForPacketAsync<MapChange>(5000);
            if (mapChange == null)
            {
                return GameLoadResult.MissingMapChange;
            }

            Game.ClientIndex = mapChange.ClientIndex;
            State = GameLoaderState.ChangingMap;

            // 5. Load the map
            LoadingHint = "Loading the map: " + mapChange.MapName;
            if (_cancelled) { return GameLoadResult.Cancelled; }

            // with a local server the map should already be loaded, so let's check first
            if (Map.Current == null || Map.Current.Name != mapChange.MapName)
            {
                var mapLoadResult = await Map.LoadAsync(mapChange.MapName);
                if (mapLoadResult != MapLoadState.Loaded)
                {
                    return GameLoadResult.FailedToLoadMap;
                }
            }

            PreGameLoaded?.Invoke();

            // 6. Load the gamemode
            LoadingHint = "Loading the gamemode: " + mapChange.Gamemode;
            if (_cancelled) { return GameLoadResult.Cancelled; }

            var gamemodeLoaded = Game.GamemodeLoader.LoadGamemode(mapChange.Gamemode);
            if(!gamemodeLoaded)
            {
                return GameLoadResult.FailedToLoadGamemode;
            }

            GameLoaded?.Invoke();

            // 7. Notify server we're in
            LoadingHint = "Done, entering game";
            if (_cancelled) { return GameLoadResult.Cancelled; }

            var pi2 = PacketUtility.TakePacket<PlayerIntroduction>();
            pi2.Step = PlayerIntroduction.JoinStep.Introduce;
            Game.Network.BroadcastPacket(pi2);

            State = GameLoaderState.Playing;

            return GameLoadResult.Success;
        }

        public async Task<GameLoadResult> CreateServerAsync(string mapName, string gamemode, string name = null, string pass = null)
        {
            if(GameServer.Instance != null)
            {
                GameServer.Instance.Destroy();
                await Task.Delay(100);
            }

            Loading = true;
            LoadingHint = "Creating server";

            var obj = new GameObject("[Server]");
            var server = obj.AddComponent<GameServer>();
            GameObject.DontDestroyOnLoad(obj);
            server.IsLocalServer = true;

            if (!string.IsNullOrEmpty(name))
            {
                DevConsole.ExecuteLine("server.name \"" + name + "\"");
            }
            if (!string.IsNullOrEmpty(pass))
            {
                DevConsole.ExecuteLine("server.password \"" + pass + "\"");
            }

            var result = await server.GameLoader.CreateGameAsync(mapName, gamemode);
            if(result != GameLoadResult.Success)
            {
                Debug.LogError("Fucked: " + result);
                server.Destroy();
                if (!Game.IsHost)
                {
                    Game.Destroy();
                    UGuiManager.Instance.Popup("Couldn't load that map, something went wrong.");
                }
                Loading = false;
            }

            return result;
        }

        public async Task<GameLoadResult> CreateGameAsync(string mapName, string gamemode)
        {
            if (State != GameLoaderState.New)
            {
                Debug.LogError($"{Game.IsHost} Can't create game while state is: {State}");
                return GameLoadResult.None;
            }

            State = GameLoaderState.Creating;

            FileSystem.ClearDownloadList();

            _cancelled = false;
            Loading = true;
            var result = await _CreateGameAsync(mapName, gamemode);
            Loading = false;

            if(result == GameLoadResult.Success)
            {
                State = GameLoaderState.Playing;
            }

            return result;
        }

        private async Task<GameLoadResult> _CreateGameAsync(string mapName, string gamemode)
        {
            State = GameLoaderState.ChangingMap;

            LoadingHint = "Loading map: " + mapName;

            if(ulong.TryParse(mapName, out ulong workshopId))
            {
                LoadingHint = "Downloading map from workshop";

                if(!SteamServer.IsValid
                    && !SteamClient.IsValid)
                {
                    return GameLoadResult.FailedToLoadMap;
                }

                var item = await Item.GetAsync(workshopId);
                if (!item.HasValue)
                {
                    return GameLoadResult.FailedToLoadMap;
                }

                var download = await SteamUGC.DownloadAsync(workshopId);
                if(!download)
                {
                    return GameLoadResult.FailedToLoadMap;
                }

                var provider = FileSystem.AddLocalProvider(item.Value.Directory);
                FileSystem.Build();
                foreach(var file in provider.Files)
                {
                    file.Value.WorkshopId = workshopId;
                }
            }

            if (Map.Current == null || Map.Current.Name != mapName)
            {
                var mapLoadResult = await Map.LoadAsync(mapName);
                if (mapLoadResult != MapLoadState.Loaded)
                {
                    return GameLoadResult.FailedToLoadMap;
                }
            }

            if (_cancelled) 
            {
                return GameLoadResult.Cancelled; 
            }

            PreGameLoaded?.Invoke();

            LoadingHint = "Loading gamemode: " + gamemode;

            var gamemodeLoaded = Game.GamemodeLoader.LoadGamemode(gamemode);
            if (!gamemodeLoaded)
            {
                return GameLoadResult.FailedToLoadGamemode;
            }
            GameLoaded?.Invoke();

            State = GameLoaderState.Playing;

            LoadingHint = "Done, game is created.";

            return GameLoadResult.Success;
        }

    }
}
