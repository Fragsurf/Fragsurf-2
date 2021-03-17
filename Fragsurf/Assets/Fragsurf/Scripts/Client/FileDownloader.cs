using Fragsurf.Shared;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;
using System.Threading;
using System.Threading.Tasks;
using Steamworks.Ugc;
using System.Collections.Generic;

namespace Fragsurf.Client
{
    public class FileDownloader : FSSharedScript
    {

        public enum SyncState
        {
            None,
            WaitingForFileSync,
            TimedOut,
            Completed,
            Failed
        }

        private List<FSFileInfo> _filesToSync;

        public bool Downloading { get; private set; }
        public Item Item { get; private set; }
        public int DownloadProgress { get; private set; }

        public async Task<SyncState> SyncWithHostAsync(CancellationTokenSource cts = null)
        {
            _filesToSync = null;
            var initiateSync = PacketUtility.TakePacket<FileSync>();
            initiateSync.SyncType = FileSync.FileSyncType.Initiate;
            Game.Network.BroadcastPacket(initiateSync);
            var timeout = 10000f;
            var rebuild = false;
            while(timeout > 0)
            {
                await Task.Delay(100);
                timeout -= 100f;
                if(_filesToSync != null)
                {
                    break;
                }
                if(timeout <= 0)
                {
                    return SyncState.TimedOut;
                }
            }
            foreach (var file in _filesToSync)
            {
                if (file.WorkshopId == 0)
                {
                    continue;
                }
                var item = await Item.GetAsync(file.WorkshopId);
                if(!item.HasValue)
                {
                    return SyncState.Failed;
                }

                Item = item.Value;

                if(!await Item.Subscribe())
                {
                    return SyncState.Failed;
                }

                if(cts == null)
                {
                    cts = new CancellationTokenSource();
                }
                try
                {
                    var downloadResult = await Item.DownloadAsync((v) => DownloadProgress = (int)(v * 100), 60, cts.Token);
                    cts?.Dispose();
                    if (!downloadResult)
                    {
                        return SyncState.Failed;
                    }
                }
                catch
                {
                    return SyncState.Failed;
                }

                rebuild = true;
            }
            if(rebuild)
            {
                await FileSystem.BuildAsync();
            }
            return SyncState.Completed;
        }

        protected override void OnPlayerPacketReceived(BasePlayer player, IBasePacket packet)
        {
            if(packet is FileSync fileSync)
            {
                _filesToSync = new List<FSFileInfo>(fileSync.Files);
            }
        }

    }
}

