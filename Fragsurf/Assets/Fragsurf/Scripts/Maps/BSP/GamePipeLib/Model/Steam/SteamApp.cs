/**
* Copyright (c) 2017 Joseph Shaw & Big Sky Software LLC
* Distributed under the GNU GPL v2. For full terms see the file LICENSE.txt
*/
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using GamePipeLib.Interfaces;

//Helpful info about ACF files: https://wiki.singul4rity.com/steam:filestructures:acf
namespace GamePipeLib.Model.Steam
{
    public enum AppStateFlags
    {
        StateInvalid = 0,
        StateUninstalled = 0x01,
        StateUpdateRequired = 0x02,
        StateFullyInstalled = 0x04,
        StateEncrypted = 0x08,
        StateLocked = 0x10,
        StateFilesMissing = 0x20,
        StateAppRunning = 0x40,
        StateFilesCorrupt = 0x80,
        StateUpdateRunning = 0x0100,
        StateUpdatePaused = 0x0200,
        StateUpdateStarted = 0x0400,
        StateUninstalling = 0x0800,
        StateBackupRunning = 0x1000,
        StateReconfiguring = 0x010000,
        StateValidating = 0x020000,
        StateAddingFiles = 0x040000,
        StatePreallocating = 0x080000,
        StateDownloading = 0x100000,
        StateStaging = 0x200000,
        StateCommitting = 0x400000,
        StateUpdateStopping = 0x800000
    }

    public class SteamApp : ILocalSteamApplication
    {
        private System.IO.FileSystemWatcher _watcher;
        private long _acfDiskSize;
        public SteamApp(string acfFilePath)
        {
            _AcfFile = acfFilePath;
            try
            {
                InitializeFromAcf();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to initialize new SteamApp object from ACF File: {acfFilePath}" + ex);
            }
            //_watcher = new FileSystemWatcher(_AcfFile);
            //_watcher.Changed += _watcher_Changed;
            //_watcher.Deleted += _watcher_Deleted;
        }

        private readonly string _AcfFile;
        public string AcfFile { get { return _AcfFile; } }

        public string GameName { get; private set; }
        public string AppId { get; private set; }
        public string GameDir { get; private set; }
        public string InstallDir { get; private set; }

        private AppStateFlags _AppState;
        public AppStateFlags AppState
        {
            get { return _AppState; }
            private set
            {
                if (_AppState != value)
                {
                    _AppState = value;
                }
            }
        }

        public string ImageUrl { get { return this.GetSteamImageUrl(); } }

        public void InitializeFromAcf()
        {
            var contents = File.ReadAllText(AcfFile);
            var pairs = GamePipeLib.Utils.SteamDirParsingUtils.ParseStringPairs(contents);
            int count = 0;
            foreach (var pair in pairs)
            {
                if (count >= 5)
                {
                    return;  //return once we've got what we're looking for
                }
                switch (pair.Item1.ToLower())   //Compare in lower case
                {
                    case "appid":
                        AppId = pair.Item2;
                        count++;
                        break;
                    case "stateflags":
                        AppState = (AppStateFlags)long.Parse(pair.Item2);
                        count++;
                        break;
                    case "name":
                        GameName = pair.Item2;
                        count++;
                        break;
                    case "installdir":
                        var commonFolder = Path.Combine(Path.GetDirectoryName(AcfFile), "common");
                        var installDir = pair.Item2.Replace("\\\\", "\\").Trim();//Convert "\\" to "\", double slashes for escape charcters
                        string[] splitString = { "\\" };
                        InstallDir = installDir.Split(splitString, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                        if (string.IsNullOrWhiteSpace(InstallDir))
                        {
                            UnityEngine.Debug.LogError($"Failed to identify Game Directory for ACF File: {AcfFile} {GameName}");
                        }
                        else
                        {
                            GameDir = Path.Combine(commonFolder, InstallDir);
                        }
                        count++;
                        break;

                }
            }


        }
        public void RefreshFromAcf()
        {
            string contents = null;
            try
            {
                contents = File.ReadAllText(AcfFile);
            }
            catch (Exception) { return; }
            var pairs = GamePipeLib.Utils.SteamDirParsingUtils.ParseStringPairs(contents);
            int count = 0;
            foreach (var pair in pairs)
            {
                if (count >= 2) return;  //return once we've got what we're looking for
                switch (pair.Item1.ToLower())   //Compare in lower case
                {
                    case "stateflags":
                        AppState = (AppStateFlags)long.Parse(pair.Item2);
                        count++;
                        break;
                }
            }
        }

    }
}
