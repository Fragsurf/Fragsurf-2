using System.IO;
using System.Collections.Generic;
using Lidgren.Network;
using Fragsurf.Shared;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;

namespace Fragsurf.Server.ServerScripts
{
    public class FileUploader : FSServerScript
    {

        private class FileUpload
        {
            public string File;
            public FileStream InputStream;
            public int SentOffset;
            public int ChunkLength;
            public int ClientIndex;
        }

        private List<FileUpload> _uploads = new List<FileUpload>();
        private string _downloadUrl;

        protected override void _Initialize()
        {
            base._Initialize();

            DevConsole.RegisterVariable("net.downloadurl", "",
            () => _downloadUrl,
            delegate (string value)
            {
                _downloadUrl = value;
                FileSystem.Build();
            }, this);
        }

        protected override void OnPlayerPacketReceived(BasePlayer player, IBasePacket packet)
        {
            if (packet is FileSync fileSync
                && fileSync.SyncType == FileSync.FileSyncType.Request)
            {
                if (fileSync.Files.Count == 0)
                {
                    return;
                }

                var file = FileSystem.GetFileInfo(fileSync.Files[0].RelativePath);
                if (file == null)
                {
                    return;
                }

                var fu = new FileUpload();
                fu.InputStream = new FileStream(file.FullPath, FileMode.Open, FileAccess.Read);
                fu.File = file.RelativePath;
                fu.ClientIndex = player.ClientIndex;
                fu.ChunkLength = player.MTU - 20;
                fu.SentOffset = 0;
                _uploads.Add(fu);
            }
        }

        protected override void OnPlayerDisconnected(BasePlayer player)
        {
            for(int i = _uploads.Count - 1; i >= 0; i--)
            {
                if(_uploads[i].ClientIndex == player.ClientIndex)
                {
                    _uploads[i].InputStream?.Close();
                    _uploads[i].InputStream?.Dispose();
                    _uploads[i].InputStream = null;
                    _uploads.RemoveAt(i);
                }
            }
        }

        protected override void _Tick()
        {
            for (int i = _uploads.Count - 1; i >= 0; i--)
            {
                if (SendChunk(_uploads[i]))
                {
                    _uploads.RemoveAt(i);
                    break;
                }
            }
        }

        private bool SendChunk(FileUpload fu)
        {
            //if (!fu.Connection.CanSendImmediately(SendCategory.UI_Important.DeliveryMethod, SendCategory.UI_Important.SequenceChannel))
            //{
            //    return false;
            //}

            // send another part of the file!
            long remaining = fu.InputStream.Length - fu.SentOffset;
            int sendBytes = (remaining > fu.ChunkLength ? fu.ChunkLength : (int)remaining);

            var chunk = PacketUtility.TakePacket<FileChunk>();
            chunk.Data = new byte[sendBytes];
            chunk.ChunkLength = sendBytes;
            chunk.File = fu.File;
            chunk.TotalLength = fu.InputStream.Length;

            // just assume we can read the whole thing in one Read()
            fu.InputStream.Read(chunk.Data, 0, sendBytes);

            // send it
            GameServer.Instance.Socket.SendPacketBrute(fu.ClientIndex, chunk);

            fu.SentOffset += sendBytes;

            if(remaining - sendBytes <= 0)
            {
                fu.InputStream.Close();
                fu.InputStream.Dispose();
                fu.InputStream = null;
                return true;
            }

            return false;
        }

    }
}

