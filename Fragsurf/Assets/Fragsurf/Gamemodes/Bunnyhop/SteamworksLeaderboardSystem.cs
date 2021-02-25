using Steamworks;
using Steamworks.Data;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class SteamworksLeaderboardSystem : BaseLeaderboardSystem
    {

        public override async Task<LeaderboardEntry> FindRank(LeaderboardIdentifier ldbId, ulong userId)
        {
            if (!SteamClient.IsValid)
            {
                return null;
            }

            var ldbName = GetLeaderboardName(ldbId);
            var ldb = await SteamUserStats.FindOrCreateLeaderboardAsync(ldbName, LeaderboardSort.Ascending, LeaderboardDisplay.TimeMilliSeconds);
            if (!ldb.HasValue)
            {
                return null;
            }

            var entries = await ldb.Value.GetScoresForUsersAsync(new SteamId[] { userId });
            if(entries == null || entries.Length == 0)
            {
                return null;
            }

            return SteamEntryToEntry(entries[0]);
        }

        private LeaderboardEntry SteamEntryToEntry(Steamworks.Data.LeaderboardEntry steamentry)
        {
            var jumps = 0;
            var strafes = 0;

            if (steamentry.Details != null && steamentry.Details.Length >= 2)
            {
                jumps = steamentry.Details[0];
                strafes = steamentry.Details[1];
            }

            return new LeaderboardEntry()
            {
                UserId = steamentry.User.Id,
                Rank = steamentry.GlobalRank,
                TimeMilliseconds = steamentry.Score,
                UserName = steamentry.User.Name,
                Jumps = jumps,
                Strafes = strafes
            };
        }

        public override async Task<IEnumerable<LeaderboardEntry>> QueryFriends(LeaderboardIdentifier ldbId)
        {
            var result = new List<LeaderboardEntry>();

            if (!SteamClient.IsValid)
            {
                return result;
            }

            var ldbName = GetLeaderboardName(ldbId);
            var ldb = await SteamUserStats.FindOrCreateLeaderboardAsync(ldbName, LeaderboardSort.Ascending, LeaderboardDisplay.TimeMilliSeconds);
            if (!ldb.HasValue)
            {
                return result;
            }

            var scores = await ldb.Value.GetScoresFromFriendsAsync();

            if(scores != null)
            {
                foreach (var score in scores)
                {
                    result.Add(SteamEntryToEntry(score));
                }
            }

            return result;
        }

        public override async Task<IEnumerable<LeaderboardEntry>> Query(LeaderboardIdentifier ldbId, int offset, int count)
        {
            var result = new List<LeaderboardEntry>();

            if (!SteamClient.IsValid)
            {
                return result;
            }

            var ldbName = GetLeaderboardName(ldbId);
            var ldb = await SteamUserStats.FindOrCreateLeaderboardAsync(ldbName, LeaderboardSort.Ascending, LeaderboardDisplay.TimeMilliSeconds);
            if (!ldb.HasValue)
            {
                return result;
            }

            var scores = await ldb.Value.GetScoresAsync(count, offset);

            if(scores != null)
            {
                foreach (var score in scores)
                {
                    result.Add(SteamEntryToEntry(score));
                }
            }

            return result;
        }

        public override async Task<byte[]> DownloadReplayAsync(LeaderboardIdentifier ldbId, int rank)
        {
            if (!SteamClient.IsValid)
            {
                return null;
            }

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
                Success = false,
                TimeMilliseconds = (int)(frame.Time * 1000)
            };

            if (!SteamClient.IsValid)
            {
                return response;
            }

            var myRank = await FindRank(ldbId, SteamClient.SteamId);
            var ldbName = GetLeaderboardName(ldbId);
            var leaderboard = await SteamUserStats.FindOrCreateLeaderboardAsync(ldbName, LeaderboardSort.Ascending, LeaderboardDisplay.TimeMilliSeconds);

            if (!leaderboard.HasValue)
            {
                return response;
            }

            var details = new int[]
            {
                frame.Jumps,
                frame.Strafes
            };
            var update = await leaderboard.Value.SubmitScoreAsync((int)(frame.Time * 1000), details);

            if (!update.HasValue || !update.Value.Success)
            {
                return response;
            }

            response.Success = true;
            response.Improvement = myRank != null ? update.Value.Score - myRank.TimeMilliseconds : 0;
            response.NewRank = update.Value.NewGlobalRank;
            response.OldRank = update.Value.OldGlobalRank;
            response.Improved = update.Value.Changed;
            response.TimeMilliseconds = update.Value.Score;

            if(response.NewRank == 1)
            {
                var prevWr = await Query(ldbId, 2, 1);
                if(prevWr != null && prevWr.Count() == 1)
                {
                    response.Takeover = update.Value.Score - prevWr.First().TimeMilliseconds;
                }
                else
                {
                    response.Takeover = response.Improvement;
                }
            }

            return response;
        }

        protected override async Task<bool> _SaveReplay(LeaderboardIdentifier ldbId, string filePath)
        {
            if (!SteamClient.IsValid)
            {
                return false;
            }

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

