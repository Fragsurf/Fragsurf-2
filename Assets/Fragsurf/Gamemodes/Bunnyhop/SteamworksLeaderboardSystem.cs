using Steamworks;
using Steamworks.Data;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;

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
            var ldb = await SteamUserStats.FindLeaderboardAsync(ldbName);
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
            var timestamp = 0;
            var jumps = 0;
            var strafes = 0;

            if (steamentry.Details != null && steamentry.Details.Length >= 3)
            {
                timestamp = steamentry.Details[0];
                jumps = steamentry.Details[1];
                strafes = steamentry.Details[2];
            }

            return new LeaderboardEntry()
            {
                UserId = steamentry.User.Id,
                Rank = steamentry.GlobalRank,
                TimeMilliseconds = steamentry.Score,
                DisplayName = steamentry.User.Name,
                Jumps = jumps,
                Strafes = strafes,
                UnixTimestamp = timestamp
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
            var ldb = await SteamUserStats.FindLeaderboardAsync(ldbName);
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
            var ldb = await SteamUserStats.FindLeaderboardAsync(ldbName);
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

        public override async Task<IEnumerable<Top10Entry>> QueryRecentTops(LeaderboardIdentifier ldbId, int count)
        {
            var result = new List<Top10Entry>();

            if (!SteamClient.IsValid)
            {
                return result;
            }

            var ldbName = GetRtopLeaderboardName(ldbId);
            var ldb = await SteamUserStats.FindLeaderboardAsync(ldbName);
            if (!ldb.HasValue)
            {
                return result;
            }

            var scores = await ldb.Value.GetScoresAsync(count, 1);

            if(scores == null || scores.Length == 0)
            {
                return result;
            }

            foreach(var score in scores)
            {
                result.Add(LdbEntryToTop10(score));
            }

            return result;
        }

        private Top10Entry LdbEntryToTop10(Steamworks.Data.LeaderboardEntry entry)
        {
            var result = new Top10Entry() 
            { 
                DisplayName = entry.User.Name,
                UserId = entry.User.Id
            };

            if (entry.Details.Length < 6)
            {
                return result;
            }

            result.UnixTimestamp = entry.Score;
            result.NewRank = entry.Details[0];
            result.OldRank = entry.Details[1];
            result.TimeMilliseconds = entry.Details[2];
            result.ImprovementMilliseconds = entry.Details[3];
            result.Jumps = entry.Details[4];
            result.Strafes = entry.Details[5];

            return result;
        }

        public override async Task<byte[]> DownloadReplayAsync(LeaderboardIdentifier ldbId, int rank)
        {
            if (!SteamClient.IsValid)
            {
                return null;
            }

            var ldbName = GetLeaderboardName(ldbId);
            var ldb = await SteamUserStats.FindLeaderboardAsync(ldbName);
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

        protected override async Task<SubmitResponse> _SubmitRun(LeaderboardIdentifier ldbId, BunnyhopTimelineFrame frame)
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

            var unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var details = new int[]
            {
                unixTimestamp,
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

            if(response.Improved && response.NewRank <= 10)
            {
                var rtopDetails = new int[]
                {
                    response.NewRank,
                    response.OldRank,
                    response.TimeMilliseconds,
                    response.Improvement,
                    frame.Jumps,
                    frame.Strafes
                };
                var rtopLdbName = GetRtopLeaderboardName(ldbId);
                var rtopLeaderboard = await SteamUserStats.FindOrCreateLeaderboardAsync(rtopLdbName, LeaderboardSort.Descending, LeaderboardDisplay.TimeMilliSeconds);

                if (!rtopLeaderboard.HasValue)
                {
                    return response;
                }

                await rtopLeaderboard.Value.ReplaceScore(unixTimestamp, rtopDetails);
            }

            return response;
        }

        protected override async Task<bool> _SaveReplay(LeaderboardIdentifier ldbId, byte[] data)
        {
            if (!SteamClient.IsValid)
            {
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

        private string GetRtopLeaderboardName(LeaderboardIdentifier ldbId)
        {
            return $"rtop--" + GetLeaderboardName(ldbId);
        }

        private string GetLeaderboardName(LeaderboardIdentifier ldb)
        {
            var result = string.Join("--", ldb.Map, ldb.TrackType, ldb.TrackName);
            if (ldb.Number > 0) result = string.Join("--", result, "n" + ldb.Number);
            return result;
        }

    }
}

