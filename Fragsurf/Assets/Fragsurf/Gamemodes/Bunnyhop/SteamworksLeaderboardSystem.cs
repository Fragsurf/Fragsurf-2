using Steamworks;
using Steamworks.Data;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class SteamworksLeaderboardSystem : BaseLeaderboardSystem
    {

        public override async Task<IEnumerable<LeaderboardEntry>> Query(LeaderboardIdentifier ldbId, int offset, int count)
        {
            var result = new List<LeaderboardEntry>();
            var ldbName = GetLeaderboardName(ldbId);
            var ldb = await SteamUserStats.FindOrCreateLeaderboardAsync(ldbName, LeaderboardSort.Ascending, LeaderboardDisplay.TimeMilliSeconds);
            if (!ldb.HasValue)
            {
                return result;
            }

            var scores = await ldb.Value.GetScoresAsync(count, offset);
            foreach(var score in scores)
            {
                result.Add(new LeaderboardEntry()
                {
                    UserId = score.User.Id,
                    Rank = score.GlobalRank,
                    TimeMilliseconds = score.Score,
                    UserName = score.User.Name,
                    Jumps = 0,
                    Strafes = 0
                });
            }

            return result;
        }

        public override async Task<byte[]> DownloadReplayAsync(LeaderboardIdentifier ldbId, int rank)
        {
            var ldbName = GetLeaderboardName(ldbId);
            var ldb = await SteamUserStats.FindOrCreateLeaderboardAsync(ldbName, LeaderboardSort.Ascending, LeaderboardDisplay.TimeMilliSeconds);
            if (!ldb.HasValue)
            {
                return null;
            }

            var score = await ldb.Value.GetScoresAsync(1, rank);
            if(score.Length == 0)
            {
                return null;
            }

            var result = await SteamRemoteStorage.UGCDownload(score[0].Ugc.FileId);

            if(result == null || result.Length == 0)
            {
                return null;
            }

            return result.Decompress();
        }

        protected override async Task<SubmitResponse> _SubmitRun(LeaderboardIdentifier ldbId, BunnyhopTimelineFrame frame, byte[] replay)
        {
            var response = new SubmitResponse()
            {
                Success = false
            };

            if (!SteamClient.IsValid)
            {
                return response;
            }

            var ldbName = GetLeaderboardName(ldbId);
            var leaderboard = await SteamUserStats.FindOrCreateLeaderboardAsync(ldbName, LeaderboardSort.Ascending, LeaderboardDisplay.TimeMilliSeconds);

            if (!leaderboard.HasValue)
            {
                return response;
            }

            var update = await leaderboard.Value.SubmitScoreAsync((int)(frame.Time * 1000));

            if (!update.HasValue)
            {
                return response;
            }

            response.Success = true;
            response.Improvement = 0f;
            response.NewRank = update.Value.NewGlobalRank;
            response.OldRank = update.Value.OldGlobalRank;
            response.Improved = update.Value.Changed;

            return response;
        }

        protected override async Task<bool> _SaveReplay(LeaderboardIdentifier ldbId, string filePath)
        {
            byte[] data;

            try
            {
                data = File.ReadAllBytes(filePath);
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }

            var ldbName = GetLeaderboardName(ldbId);
            var ldb = await SteamUserStats.FindOrCreateLeaderboardAsync(ldbName, LeaderboardSort.Ascending, LeaderboardDisplay.TimeMilliSeconds);
            if (!ldb.HasValue)
            {
                return false;
            }

            var relativePath = ReplayFilePath(ldbId, true);
            if(!SteamRemoteStorage.FileWrite(relativePath, data))
            {
                return false;
            }

            var ugc = await SteamRemoteStorage.FileShare(relativePath);
            if (!ugc.HasValue)
            {
                return false;
            }

            var attachResult = await ldb.Value.AttachUgc(ugc.Value);
            if(attachResult != Result.OK)
            {
                return false;
            }

            return true;
        }

        private string GetLeaderboardName(LeaderboardIdentifier ldb)
        {
            var result = string.Join("-", ldb.Map, ldb.TrackType, ldb.TrackName);
            if (ldb.Number > 0) result = string.Join("-", result, "n" + ldb.Number);
            return result;
        }

    }
}

