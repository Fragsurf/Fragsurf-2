/**
* Copyright (c) 2017 Joseph Shaw & Big Sky Software LLC
* Distributed under the GNU GPL v2. For full terms see the file LICENSE.txt
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System;

namespace GamePipeLib.Interfaces
{

    public interface IAppProvider
    {
        IEnumerable<BasicSteamApp> GetAvailableIds();
        IEnumerable<Tuple<string, long>> GetFilesForApp(string appId, bool acceptCompressedFiles);
        IEnumerable<string> GetDirectoriesForApp(string appId);
        bool CanCopy(string appId);
        bool CanCopyIfForced(string appId);
        Stream GetFileStream(string appId, string file, bool acceptCompressedFiles, bool validation, int bufferSize);
        uint GetTransferredCrc(string appId, string file);
        string GetAcfFileContent(string appId);
        long GetMeasuredGameSize(string appId);
    }

    
    [DataContract]
    public class BasicSteamApp : ISteamApplication
    {
        public BasicSteamApp()
        {

        }
        public BasicSteamApp(ISteamApplication source)
        {
            GameName = source.GameName;
            AppId = source.AppId;
            InstallDir = source.InstallDir;
        }

        [DataMember]
        public string InstallDir { get; set; }
        [DataMember]
        public string GameName { get; set; }
        [DataMember]
        public string AppId { get; set; }
        [DataMember]
        public long DiskSize { get; set; }

        public  string ReadableDiskSize { get {return GamePipeLib.Utils.FileUtils.GetReadableFileSize(DiskSize); } }
        public string ImageUrl { get { return this.GetSteamImageUrl(); } }
    }
}
