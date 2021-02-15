/**
* Copyright (c) 2017 Joseph Shaw & Big Sky Software LLC
* Distributed under the GNU GPL v2. For full terms see the file LICENSE.txt
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using GamePipeLib.Interfaces;
using GamePipeLib.Utils;
using System.Collections.ObjectModel;

namespace GamePipeLib.Model.Steam
{

    public class SteamLibrary
    {
        private System.IO.FileSystemWatcher _watcher;
        public readonly bool _isArchive;

        public SteamLibrary(string libraryDirectory, bool isArchive = false)
        {
            _LibraryDirectory = libraryDirectory;
            _isArchive = isArchive;
        }

        private readonly string _LibraryDirectory;
        public string SteamDirectory
        {
            get { return _LibraryDirectory; }
        }

        private ObservableCollection<ILocalSteamApplication> _Games;
        public ObservableCollection<ILocalSteamApplication> Games
        {
            get
            {
                if ((_Games == null))
                {
                    _Games = new ObservableCollection<ILocalSteamApplication>(GenerateGames());
                }
                return _Games;
            }
        }

        private IEnumerable<ILocalSteamApplication> GenerateGames()
        {
            if (string.IsNullOrWhiteSpace(SteamDirectory) || Directory.Exists(SteamDirectory) == false)
            {
                return null;
            }
            var files = Directory.EnumerateFiles(SteamDirectory, "*.acf");
            var apps = (from file in files
                        let game = (ILocalSteamApplication)new SteamApp(file)
                        where string.IsNullOrWhiteSpace(game.GameName) == false
                        where string.IsNullOrWhiteSpace(game.AppId) == false
                        where string.IsNullOrWhiteSpace(game.InstallDir) == false
                        orderby game.GameName
                        group game by game.InstallDir.ToLower() into groups
                        select (groups.Count() > 1
                            ? (ILocalSteamApplication)new SteamBundle(groups)
                            : groups.First())).ToList();


            return apps;
        }

        public ILocalSteamApplication GetGameById(string id)
        {

            return Games.Where(x => x.AppId == id).FirstOrDefault();
        }

        public ILocalSteamApplication GetGameOrBundleById(string id)
        {
            var found = GetGameById(id);
            if (found == null)
            {
                found = (from bundle in Games.OfType<SteamBundle>()
                         where bundle.AppsInBundle.Any(app => app.AppId == id)
                         select bundle).FirstOrDefault();
            }
            return found;
        }
        public IEnumerable<BasicSteamApp> GetAvailableIds()
        {
            return Games.Select(x => new BasicSteamApp(x));
        }

        public virtual IEnumerable<Tuple<string, long>> GetFilesForApp(string appId, bool acceptCompressedFiles)
        {
            var game = GetGameById(appId);
            if (game == null) throw new ArgumentException(string.Format("App ID {0} not found in {1}", appId, SteamDirectory));

            string baseDir = game.GameDir + "\\";

            return (from file in Directory.EnumerateFiles(game.GameDir, "*", SearchOption.AllDirectories)
                    let info = new FileInfo(file)
                    let path = (file.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase)
                           ? file.Substring(baseDir.Length)
                           : file)
                    orderby info.Length descending
                    select new Tuple<string, long>(path, info.Length));
        }

        public virtual IEnumerable<string> GetDirectoriesForApp(string appId)
        {
            var game = GetGameById(appId);
            if (game == null) throw new ArgumentException(string.Format("App ID {0} not found in {1}", appId, SteamDirectory));

            string baseDir = SteamDirectory + "\\";
            return Directory.EnumerateDirectories(game.GameDir, "*", SearchOption.AllDirectories).DefaultIfEmpty(game.GameDir)
                .Select(path => path.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase)
                                ? path.Substring(baseDir.Length)
                                : path);
        }

        public uint GetTransferredCrc(string appId, string file)
        {
            throw new NotSupportedException("Cast the read stream to a CrcStream and get its crc instead.");
        }

        public string GetAcfFileContent(string appId)
        {
            string acfFileName = string.Format("appmanifest_{0}.acf", appId);
            string acfPath = Path.Combine(SteamDirectory, acfFileName);
            if (File.Exists(acfPath))
                return File.ReadAllText(acfPath);

            return null;
        }

        public bool HasApp(string appId)
        {
            var game = GetGameById(appId);
            return (game != null);
        }

        public bool HasGameDir(string installDir)
        {
            string path = Path.Combine(SteamDirectory, "common", installDir);
            return (Directory.Exists(path));
        }

        public void OpenGameDir(string installDir, bool openInstallDir = true, bool openBackupDirToo = false)
        {
            if (openInstallDir)
            {
                string path = Path.Combine(SteamDirectory, "common", installDir);
                try
                {
                    if (Directory.Exists(path)) System.Diagnostics.Process.Start(path);
                }
                catch (Exception) { }
            }
            if (openBackupDirToo)
            {
                string backupDir = string.Format("_gpbackup_{0}", installDir);
                string backupPath = Path.Combine(SteamDirectory, "common", backupDir);
                try
                {
                    if (Directory.Exists(backupPath)) System.Diagnostics.Process.Start(backupPath);
                }
                catch (Exception) { }
            }
        }

        public virtual Stream GetReadFileStream(string appId, string file, bool acceptCompressedFiles, bool validation, int bufferSize)
        {
            var game = GetGameById(appId);
            if (game == null) throw new ArgumentException(string.Format("App ID {0} not found in {1}", appId, SteamDirectory));
            var gameDir = Path.GetFullPath(game.GameDir);
            var fullPath = Path.GetFullPath(Path.Combine(gameDir, file));

            if (fullPath.StartsWith(gameDir, StringComparison.OrdinalIgnoreCase) == false)
                throw new ArgumentException(string.Format("The file request is outside the game directory for {0}. File: {1}", game.GameName, file));

            return FileUtils.OpenReadStream(fullPath, validation, bufferSize);
        }

        public virtual Stream GetWriteFileStream(string file, bool validation, int bufferSize)
        {
            var fullPath = Path.GetFullPath(Path.Combine(SteamDirectory, file));

            return FileUtils.OpenWriteStream(fullPath, validation, bufferSize);
        }

    }
}
