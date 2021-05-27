using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Fragsurf.Shared;
using MessagePack;
using Fragsurf.Maps;
using Fragsurf.Shared.Player;
using System.Linq;
using Fragsurf.Shared.Entity;

namespace Fragsurf.Gamemodes.Tricksurf
{
    [Inject(InjectRealm.Client, typeof(Tricksurf))]
    public class CL_TrickLog : FSSharedScript
    {
        [MessagePackObject]
        public class TrickLog
        {
            [Key(0)] public List<LogEntry> Entries = new List<LogEntry>();
            [Key(1)] public int Version = 1;
        }

        [MessagePackObject]
        public class LogEntry
        {
            [Key(0)] public TrickCompletion Completion;
            [Key(1)] public DateTime Date;
            //[Key(2)] public TrickReplay Replay;
        }

        private TrickLog _log;
        private SH_Tricksurf _tricksurf => Game.GetFSComponent<SH_Tricksurf>();

        public int CompletionCount => _log.Entries.Count;

        public static event Action<int> OnNewTrickCompleted;

        protected override void _Start()
        {
            base._Start();

            if (Game.IsHost)
            {
                return;
            }

            _tricksurf.OnTrickCompleted += OnTrickCompleted;

            LoadDataV3();
        }

        private void SaveDataV3()
        {
            if (_log != null)
            {
                var filePath = GetLogPath();
                using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    MessagePackSerializer.Serialize(fs, _log);
                }
            }
        }

        private async Task SaveDataV3Async()
        {
            var filePath = GetLogPath();
            await Task.Run(() =>
            {
                if (_log != null)
                {
                    using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
                    {
                        MessagePackSerializer.Serialize(fs, _log);
                    }
                }
            });
        }

        private bool LoadDataV3()
        {
            try
            {
                var logPath = GetLogPath();
                if (File.Exists(logPath))
                {
                    using (var fs = new FileStream(logPath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        _log = MessagePackSerializer.Deserialize<TrickLog>(fs);
                    }
                    return true;
                }
            }
            catch (Exception e) { Debug.LogError(e); }

            _log = new TrickLog()
            {
                Entries = new List<LogEntry>()
            };

            return false;
        }

        protected override void _Destroy()
        {
            base._Destroy();

            _tricksurf.OnTrickCompleted -= OnTrickCompleted;

            SaveDataV3();
        }

        public bool IsCompleted(int trickId)
        {
            return GetLogEntry(trickId) != null;
        }

        //public string GetCompletionDateString(int trickid)
        //{
        //    var logEntry = _log.Entries.Find(x => x.Completion.TrickId == trickid);
        //    if(logEntry != null)
        //    {
        //        return logEntry.CompletionDate.ToString("MMMM dd, yyyy H:mm");
        //    }
        //    return "n/a";
        //}

        public LogEntry GetLogEntry(int trickId)
        {
            return _log.Entries.FirstOrDefault(x => x.Completion.TrickId == trickId);
        }

        private async void OnTrickCompleted(BasePlayer player, TrickCompletion completion)
        {
            if(player.Entity != Human.Local)
            {
                return;
            }

            var loggedTrick = _log.Entries.FirstOrDefault(x => x.Completion.TrickId == completion.TrickId);
            if(loggedTrick != null)
            {
                return;
            }

            OnNewTrickCompleted?.Invoke(completion.TrickId);

            _log.Entries.Add(new LogEntry()
            {
                Completion = completion,
                Date = DateTime.Now
            });

            await SaveDataV3Async();
        }

        private string GetLogPath()
        {
            var path = Path.Combine(Application.persistentDataPath, "TrickData", Map.Current.Name);
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return path;
        }

        public static bool IsTrickCompleted(int trickId)
        {
            var game = FSGameLoop.GetGameInstance(false);
            if(game == null)
            {
                return false;
            }
            var tl = game.Get<CL_TrickLog>();
            if(tl == null)
            {
                return false;
            }
            return tl.IsCompleted(trickId);
        }

    }
}