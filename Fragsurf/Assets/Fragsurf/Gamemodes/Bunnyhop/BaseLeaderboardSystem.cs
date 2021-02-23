using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fragsurf.Actors;
using Fragsurf.Maps;
using Fragsurf.Movement;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public abstract class BaseLeaderboardSystem 
    {

        public async Task<SubmitResponse> SubmitRunAsync(LeaderboardIdentifier ldbId, BunnyhopTimelineFrame frame, byte[] replay)
        {
            var response = await _SubmitRun(ldbId, frame, replay);

            if(response.Improved || !File.Exists(ReplayFilePath(ldbId)))
            {
                if(SaveReplay(ldbId, replay, out string filePath))
                {
                    await _SaveReplay(ldbId, filePath);
                }
            }

            return response;
        }

        public abstract Task<IEnumerable<LeaderboardEntry>> Query(LeaderboardIdentifier ldbId, int offset, int count);
        public abstract Task<byte[]> DownloadReplayAsync(LeaderboardIdentifier ldbId, int rank);
        protected abstract Task<bool> _SaveReplay(LeaderboardIdentifier ldbId, string filePath);
        protected abstract Task<SubmitResponse> _SubmitRun(LeaderboardIdentifier ldbId, BunnyhopTimelineFrame frame, byte[] replay);

        /// <summary>
        /// Load the replay data in original, uncompressed format
        /// </summary>
        /// <param name="ldbId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool TryLoadReplay(LeaderboardIdentifier ldbId, out byte[] data)
        {
            var path = ReplayFilePath(ldbId);
            data = null;

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                data = File.ReadAllBytes(path).Decompress();
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save the replay data in compressed gzip format
        /// </summary>
        /// <param name="ldbId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SaveReplay(LeaderboardIdentifier ldbId, byte[] data, out string filePath)
        {
            filePath = ReplayFilePath(ldbId);
            var dir = Path.GetDirectoryName(filePath);

            try
            {
                Directory.CreateDirectory(dir);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.WriteAllBytes(filePath, data.Compress());
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
        public float Improvement;
        public float Takeover;
        public bool Improved;
    }

    public class LeaderboardEntry
    {
        public string UserName;
        public ulong UserId;
        public int TimeMilliseconds;
        public int Rank;
        public int Jumps;
        public int Strafes;
    }

}

