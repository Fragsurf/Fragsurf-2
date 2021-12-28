using System.Collections.Generic;
using Fragsurf.Network;

namespace Fragsurf.Shared.Packets
{
    public class FileSync : IBasePacket
    {

        public enum FileSyncType
        {
            Initiate,
            Manifest,
            Success,
            Request
        }

        public FileSyncType SyncType;
        public string DownloadUrl = string.Empty;
        public List<FSFileInfo> Files = new List<FSFileInfo>();

        public int ByteSize => 0;
        public SendCategory Sc => SendCategory.FileTransfer;
		public bool DisableAutoPool => false;

        public void Reset()
        {
            SyncType = FileSyncType.Initiate;
            Files.Clear();
            DownloadUrl = string.Empty;
        }

        public void Read(NetBuffer buffer)
        {
            Files.Clear();
            SyncType = (FileSyncType)buffer.ReadByte();

            if(SyncType == FileSyncType.Manifest)
                DownloadUrl = buffer.ReadString();

            if (SyncType == FileSyncType.Manifest
                || SyncType == FileSyncType.Request)
            {
                var fileCount = buffer.ReadInt32();
                for (int i = 0; i < fileCount; i++)
                {
                    var fi = new FSFileInfo();
                    fi.RelativePath = buffer.ReadString();
                    fi.WorkshopId = buffer.ReadUInt64();

                    if(SyncType == FileSyncType.Manifest)
                        fi.Hash = buffer.ReadBytes(16);
                    Files.Add(fi);
                }
            }
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write((byte)SyncType);

            if (SyncType == FileSyncType.Manifest)
                buffer.Write(DownloadUrl);

            if (SyncType == FileSyncType.Manifest
                || SyncType == FileSyncType.Request)
            {
                buffer.Write(Files.Count);
                for (int i = 0; i < Files.Count; i++)
                {
                    buffer.Write(Files[i].RelativePath);
                    buffer.Write(Files[i].WorkshopId);

                    if(SyncType == FileSyncType.Manifest)
                        buffer.Write(Files[i].Hash);
                }
            }
        }

    }
}
