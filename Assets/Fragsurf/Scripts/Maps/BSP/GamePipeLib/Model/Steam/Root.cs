/**
* Copyright (c) 2017 Joseph Shaw & Big Sky Software LLC
* Distributed under the GNU GPL v2. For full terms see the file LICENSE.txt
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GamePipeLib.Interfaces;

namespace GamePipeLib.Model.Steam
{
    public class SteamRoot
    {
        private static object _rootLock = new object();
        private static StringCollection Archives;

        private SteamRoot()
        {
        }

        private static Dictionary<int, SteamRoot> _instances = new Dictionary<int, SteamRoot>();
        public static SteamRoot Instance
        {
            get
            {
                var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                SteamRoot instance;
                //If the instance doesn't exist, first create a lock so we aren't concurrently accessing instances, then check to ensure our instance wasn't added while we waited for the lock
                lock (_rootLock)
                {
                    if (!_instances.TryGetValue(threadId, out instance))
                    {
                        instance = new SteamRoot();
                        _instances.Add(threadId, instance);
                    }
                }
                return instance;
            }
        }

        public static bool DropInstanceForThreadId(int threadId)
        {
            lock (_rootLock)
            {
                return _instances.Remove(threadId);
            }
        }

        public bool SteamRestartRequired { get; set; }

        private ObservableCollection<SteamLibrary> _Libraries = null;
        public ObservableCollection<SteamLibrary> Libraries
        {
            get
            {
                if (_Libraries == null)
                {
                    UnityEngine.Debug.Log("Discovering Libraries....");
                    _Libraries = new ObservableCollection<SteamLibrary>(DiscoverLibraries());
                    UnityEngine.Debug.Log($"Found {_Libraries.Count} libraries!");
                }
                return _Libraries;
            }
        }

        public string SteamDirectory { get { return Utils.SteamDirParsingUtils.SteamDirectory; } }

        private IEnumerable<SteamLibrary> DiscoverLibraries()
        {

            var steamApps = Path.Combine(SteamDirectory, "SteamApps");
            Regex libraryRegex = new Regex("^\\s*\"\\d+\"\\s*\"(?'path'.*)\"\\s*$", RegexOptions.Multiline);
            var libraryFile = Path.Combine(steamApps, "libraryfolders.vdf");
            List<SteamLibrary> result = new List<SteamLibrary>();

            if (Directory.Exists(steamApps))
            {
                result.Add(new SteamLibrary(steamApps));
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Steamapps folder not found at {steamApps}");
            }

            if (File.Exists(libraryFile))
            {
                var contents = File.ReadAllText(libraryFile);
                var matches = libraryRegex.Matches(contents);
                foreach (Match match in matches)
                {
                    dynamic path = Path.Combine(match.Groups["path"].Value.Replace("\\\\", "\\"), "SteamApps");
                    if (Directory.Exists(path))
                    {
                        result.Add(new SteamLibrary(path));
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"Steam library directory not found at {path}");

                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Steam Libraries file not found at {libraryFile}");
            }

            if (Archives == null)
                Archives = new StringCollection();

            foreach (var item in Archives)
            {
                if (Directory.Exists(item))
                {
                    result.Add(new SteamArchive(item));
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Archive directory not found at {libraryFile}");
                }
            }
            return result;
        }


        public ILocalSteamApplication GetGame(string appId)
        {
            foreach (var lib in Libraries)
            {
                var game = lib.GetGameOrBundleById(appId);
                if (game != null)
                    return game;
            }
            return null;
        }

        public SteamLibrary GetLibraryForGame(string appId)
        {
            foreach (var lib in Libraries)
            {
                var game = lib.GetGameOrBundleById(appId);
                if (game != null)
                    return lib;
            }
            return null;
        }

        public IEnumerable<ILocalSteamApplication> GetAllGames()
        {
            return Libraries.SelectMany(x => x.Games).ToArray();
        }

    }
}
