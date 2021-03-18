using System.Threading.Tasks;
using Fragsurf.Shared.Player;
using Fragsurf.Shared.Packets;
using System.IO;
using System.Collections.Generic;

namespace Fragsurf.Shared
{
    public class FileTransfer : FSSharedScript
    {

        private const int Mtu = 980;
        private const int MaxChunkReceivedTimeout = 10000;

        private List<DownloadState> _downloads = new List<DownloadState>();

        public async Task<TransferStatus> UploadFileAsync(BasePlayer player, FSFileInfo file, bool temporaryFile)
        {
            var upload = new UploadState(file);
            upload.Status = TransferStatus.InProgress;
            upload.IsTemporaryFile = temporaryFile;
            while(upload.Status == TransferStatus.InProgress)
            {
                if(player.Disconnected)
                {
                    upload.Status = TransferStatus.Failed;
                    break;
                }
                SendChunk(player, upload);
                await Task.Delay(100);
            }

            return upload.Status;
        }

        protected override void OnPlayerPacketReceived(BasePlayer player, IBasePacket packet)
        {
            if(Game.IsServer || !(packet is FileChunk chunk))
            {
                return;
            }
            ReceiveChunk(chunk);
        }

        private void SendChunk(BasePlayer player, UploadState state)
        {
            long remaining = state.InputStream.Length - state.SentOffset;
            int sendBytes = (remaining > state.ChunkLength ? state.ChunkLength : (int)remaining);

            var chunk = PacketUtility.TakePacket<FileChunk>();
            chunk.Data = new byte[sendBytes];
            chunk.ChunkLength = sendBytes;
            chunk.File = state.File.RelativePath;
            chunk.TotalLength = state.InputStream.Length;
            chunk.Temporary = state.IsTemporaryFile;

            state.InputStream.Read(chunk.Data, 0, sendBytes);
            Game.Network.SendPacket(player.ClientIndex, chunk);

            state.SentOffset += sendBytes;

            if (remaining - sendBytes <= 0)
            {
                state.InputStream.Close();
                state.InputStream.Dispose();
                state.Status = TransferStatus.Success;
            }
        }

        private void ReceiveChunk(FileChunk chunk)
        {
            var dl = _downloads.Find(x => string.Equals(x.File, chunk.File));
            if(dl == null)
            {
                dl = new DownloadState();
                dl.File = chunk.File;
                dl.DataStream = new MemoryStream();
                dl.StartTime = Game.ElapsedTime;
                dl.Status = TransferStatus.InProgress;
                dl.TotalLength = chunk.TotalLength;
                _downloads.Add(dl);
            }
            else
            {
                if (dl.Status != TransferStatus.InProgress)
                {
                    // something went wrong, tell host to stop?
                    return;
                }
            }

            dl.DataStream.Write(chunk.Data, 0, chunk.ChunkLength);
            dl.Offset += chunk.ChunkLength;
            dl.ChunkReceivedTimeout = MaxChunkReceivedTimeout;

            if (dl.DataStream.Length >= dl.TotalLength)
            {
                var fullPath = Path.Combine(Structure.RuntimePath, chunk.Temporary ? "temp" : "download", dl.File);
                var dirPath = Path.GetDirectoryName(fullPath);
                Directory.CreateDirectory(dirPath);
                using (var fs = new FileStream(fullPath, FileMode.OpenOrCreate))
                {
                    fs.SetLength(0);
                    dl.DataStream.WriteTo(fs);
                }
                dl.DataStream.Close();
                dl.DataStream.Dispose();
                dl.DataStream = null;
                dl.Status = TransferStatus.Success;
                _downloads.Remove(dl);
            }
        }

        public enum TransferStatus
        {
            None,
            InProgress,
            Failed,
            TimedOut,
            Success
        }

        private class UploadState
        {
            public UploadState(FSFileInfo file)
            {
                File = file;
                InputStream = new FileStream(file.FullPath, FileMode.Open, FileAccess.Read);
            }

            public readonly FSFileInfo File;
            public readonly FileStream InputStream;
            public int SentOffset;
            public int ChunkLength = Mtu;
            public TransferStatus Status;
            public bool IsTemporaryFile;
        }

        private class DownloadState
        {
            public string File;
            public float StartTime;
            public long TotalLength;
            public long Offset;
            public MemoryStream DataStream;
            public TransferStatus Status;
            public int ChunkReceivedTimeout;
        }

    }
}

