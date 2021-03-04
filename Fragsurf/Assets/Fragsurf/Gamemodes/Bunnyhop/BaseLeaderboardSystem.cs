using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Fragsurf.Actors;
using Fragsurf.Movement;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public abstract class BaseLeaderboardSystem 
    {

        public async Task<SubmitResponse> SubmitRunAsync(LeaderboardIdentifier ldbId, BunnyhopTimelineFrame frame, BunnyhopTimeline timeline)
        {
            var response = await _SubmitRun(ldbId, frame);

            if (response.Improved || !File.Exists(ReplayFilePath(ldbId)))
            {
                try
                {
                    var data = await timeline.SerializeAsync();
                    var compressed = await data.CompressAsync();
                    if(await SaveReplayAsync(ldbId, compressed))
                    {
                        await _SaveReplay(ldbId, compressed);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }

            return response;
        }

        public abstract Task<LeaderboardEntry> FindRank(LeaderboardIdentifier ldbId, ulong userId);
        public abstract Task<IEnumerable<LeaderboardEntry>> QueryFriends(LeaderboardIdentifier ldbId);
        public abstract Task<IEnumerable<LeaderboardEntry>> Query(LeaderboardIdentifier ldbId, int offset, int count);
        public abstract Task<byte[]> DownloadReplayAsync(LeaderboardIdentifier ldbId, int rank);
        protected abstract Task<bool> _SaveReplay(LeaderboardIdentifier ldbId, byte[] data);
        protected abstract Task<SubmitResponse> _SubmitRun(LeaderboardIdentifier ldbId, BunnyhopTimelineFrame frame);

        /// <summary>
        /// Load the replay data in original, uncompressed format
        /// </summary>
        /// <param name="ldbId"></param>
        /// <param name="uncompressedData"></param>
        /// <returns></returns>
        public bool TryLoadReplay(LeaderboardIdentifier ldbId, out byte[] uncompressedData)
        {
            var path = ReplayFilePath(ldbId);
            uncompressedData = null;

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                uncompressedData = File.ReadAllBytes(path).Decompress();
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves the replay data to file
        /// </summary>
        /// <param name="ldbId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> SaveReplayAsync(LeaderboardIdentifier ldbId, byte[] data)
        {
            var filePath = ReplayFilePath(ldbId);
            var dir = Path.GetDirectoryName(filePath);

            try
            {
                Directory.CreateDirectory(dir);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                using var fs = new FileStream(filePath, FileMode.OpenOrCreate);
                fs.SetLength(0);
                fs.Position = 0;
                await fs.WriteAsync(data, 0, data.Length);
                DevConsole.WriteLine("Replay saved: " + filePath);
                return true;
            }
            catch(Exception e)
            {
                DevConsole.WriteLine($"Failed to save replay {filePath}\n{e.Message}");
                Debug.LogError(e.Message);
            }
            return false;
        }

        public string ReplayFilePath(LeaderboardIdentifier ldbId, bool relative = false)
        {
            const string fileExtension = ".fragsurfreplay";

            var sanitizedMapName = MakeValidFileName(ldbId.Map);
            var directory = relative 
                ? Path.Combine("Replays", "Bunnyhop", sanitizedMapName)
                : Path.Combine(Structure.SavePath, "Replays", "Bunnyhop", sanitizedMapName);
            var fileName = string.Empty;

            if ((ldbId.TrackType == FSMTrackType.Linear || ldbId.TrackType == FSMTrackType.Staged)
                && ldbId.Number == 0)
            {
                fileName = $"main_{ldbId.Style}";
            }
            else if(ldbId.TrackType == FSMTrackType.Staged && ldbId.Number > 0)
            {
                fileName = $"s_{ldbId.Number}_{ldbId.Style}";
            }
            else if(ldbId.TrackType == FSMTrackType.Bonus)
            {
                fileName = $"b_{ldbId.Number}_{ldbId.Style}";
            }

            fileName += fileExtension;

            return Path.Combine(directory, fileName);
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        public static LeaderboardIdentifier GetLeaderboardId(string map, FSMTrack track, MoveStyle moveStyle, int number = 0)
        {
            return new LeaderboardIdentifier()
            {
                Map = map,
                Number = number,
                Style = moveStyle,
                TrackName = track.TrackName,
                TrackType = track.TrackType
            };
        }

    }

    public class LeaderboardIdentifier
    {
        public string Map;
        public FSMTrackType TrackType;
        public string TrackName;
        public int Number;
        public MoveStyle Style;
    }

    public class SubmitResponse
    {
        public bool Success;
        public int OldRank;
        public int NewRank;
        public int Improvement;
        public int Takeover;
        public int TimeMilliseconds;
        public bool Improved;
    }

    public class LeaderboardEntry
    {
        public string DisplayName;
        public ulong UserId;
        public int TimeMilliseconds;
        public int Rank;
        public int UnixTimestamp;
        public int Jumps;
        public int Strafes;

        public DateTime DateTime
        {
            get
            {
                var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(UnixTimestamp).ToLocalTime();
                return dtDateTime;
            }
        }

        public string GetDate()
        {
            return DateTime.ToString("ddd, MMM dd yyyy hh:mm tt");
        }

    }

}

