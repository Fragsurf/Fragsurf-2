using Fragsurf.Actors;
using Fragsurf.Client;
using Fragsurf.Maps;
using Fragsurf.Movement;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.UI;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class Modal_BunnyhopRanks : UGuiModal
    {

        [SerializeField]
        private Button _myRank;
        [SerializeField]
        private Button _top100;
        [SerializeField]
        private Button _friends;

        private Modal_BunnyhopRanksRankEntry _rankTemplate;
        private Modal_BunnyhopRanksTrackEntry _trackTemplate;
        private FSMTrack _selectedTrack;
        private int _selectedNumber;
        private bool _loadingRanks;
        private bool _loadingReplay;

        private BaseLeaderboardSystem LeaderboardSystem => FSGameLoop.GetGameInstance(false).Get<BunnyhopTracks>().LeaderboardSystem;

        private void Start()
        {
            _myRank.onClick.AddListener(() => 
            {
                if (!_selectedTrack)
                {
                    return;
                }
                LoadRanksAroundMe(_selectedTrack, 10, 50, _selectedNumber);
            });

            _top100.onClick.AddListener(() =>
            {
                if (!_selectedTrack)
                {
                    return;
                }
                LoadRanks(_selectedTrack, 1, 100, _selectedNumber);
            });

            _friends.onClick.AddListener(() =>
            {
                if (!_selectedTrack)
                {
                    return;
                }
                LoadFriendsRanks(_selectedTrack, _selectedNumber);
            });

            _rankTemplate = GameObject.FindObjectOfType<Modal_BunnyhopRanksRankEntry>();
            _rankTemplate.gameObject.SetActive(false);
            _trackTemplate = GameObject.FindObjectOfType<Modal_BunnyhopRanksTrackEntry>();
            _trackTemplate.gameObject.SetActive(false);

            LoadTracks();
        }

        private void Update()
        {
            var btnsDisabled = AreButtonsDisabled();
            _friends.interactable = !btnsDisabled;
            _top100.interactable = !btnsDisabled;
            _myRank.interactable = !btnsDisabled;
        }

        private void LoadTracks()
        {
            _trackTemplate.Clear();
            var firstTrack = true;
            foreach (var fsmtrack in GameObject.FindObjectsOfType<FSMTrack>())
            {
                _trackTemplate.Append(new Modal_BunnyhopRanksTrackEntryData()
                {
                    Track = fsmtrack,
                    OnClick = () =>
                    {
                        LoadRanks(fsmtrack, 1, 100);
                    },
                    Selected = firstTrack
                });

                if(fsmtrack.TrackType == FSMTrackType.Staged)
                {
                    var stageIdx = 1;
                    foreach(var stage in fsmtrack.StageData.Stages)
                    {
                        var idx = stageIdx;
                        _trackTemplate.Append(new Modal_BunnyhopRanksTrackEntryData()
                        {
                            Track = fsmtrack,
                            Number = stageIdx,
                            OnClick = () =>
                            {
                                LoadRanks(fsmtrack, 1, 100, idx);
                            }
                        });
                        stageIdx++;
                    }
                }

                if (firstTrack)
                {
                    LoadRanks(fsmtrack, 1, 100);
                    firstTrack = false;
                }
            }
        }

        private async void LoadRanksAroundMe(FSMTrack track, int offset, int count, int number = 0)
        {
            _loadingRanks = true;
            _selectedTrack = track;
            _selectedNumber = number;
            _rankTemplate.Clear();

            offset = Mathf.Clamp(offset, 5, 15);
            count = Mathf.Clamp(count, 1, 100);

            try
            {
                var ldbId = BaseLeaderboardSystem.GetLeaderboardId(Map.Current.Name, track, MoveStyle.FW, number);
                var myRank = await LeaderboardSystem.FindRank(ldbId, SteamClient.SteamId);

                if (myRank == null)
                {
                    _loadingRanks = false;
                    return;
                }

                offset = Mathf.Max(1, myRank.Rank - offset);
                var entries = await LeaderboardSystem.Query(ldbId, offset, count);
                AddEntries(ldbId, entries);
            }
            finally
            {
                _loadingRanks = false;
            }
        }

        private async void LoadFriendsRanks(FSMTrack track, int number = 0)
        {
            _loadingRanks = true;
            _rankTemplate.Clear();
            _selectedTrack = track;
            _selectedNumber = number;

            var ldbId = BaseLeaderboardSystem.GetLeaderboardId(Map.Current.Name, track, MoveStyle.FW, number);

            try
            {
                var entries = await LeaderboardSystem.QueryFriends(ldbId);
                AddEntries(ldbId, entries);
            }
            finally
            {
                _loadingRanks = false;
            }
        }

        private async void LoadRanks(FSMTrack track, int offset, int count, int number = 0)
        {
            _loadingRanks = true;
            _selectedTrack = track;
            _selectedNumber = number;
            _rankTemplate.Clear();

            try
            {
                offset = Mathf.Max(offset, 1);
                count = Mathf.Clamp(count, 1, 100);
                var ldbId = BaseLeaderboardSystem.GetLeaderboardId(Map.Current.Name, track, MoveStyle.FW, number);
                var entries = await LeaderboardSystem.Query(ldbId, offset, count);
                AddEntries(ldbId, entries);
            }
            finally
            {
                _loadingRanks = false;
            }
        }

        private void AddEntries(LeaderboardIdentifier ldbId, IEnumerable<LeaderboardEntry> entries)
        {
            _rankTemplate.Clear();

            foreach (var entry in entries)
            {
                var userid = entry.UserId;
                var rank = entry.Rank;
                _rankTemplate.Append(new Modal_BunnyhopRanksRankEntryData()
                {
                    Entry = entry,
                    OnClickProfile = () => SteamFriends.OpenUserOverlay(userid, "steamid"),
                    OnClickReplay = () => SpawnBot(ldbId, rank),
                    DisableButtons = AreButtonsDisabled
                });
            }
        }

        private bool AreButtonsDisabled()
        {
            return _loadingRanks || _loadingReplay;
        }

        private async void SpawnBot(LeaderboardIdentifier ldbId, int rank)
        {
            _loadingReplay = true;

            try // lazy me
            {
                var cl = FSGameLoop.GetGameInstance(false);
                var data = await cl.Get<BunnyhopTracks>().LeaderboardSystem.DownloadReplayAsync(ldbId, rank);
                if (data == null)
                {
                    return;
                }

                var tl = EntityTimeline.Deserialize<BunnyhopTimeline>(data);
                if (tl == null)
                {
                    return;
                }

                var ent = new ReplayHuman(cl);
                ent.EntityId = -int.MaxValue;
                cl.EntityManager.AddEntity(ent, false);
                ent.InterpolationMode = InterpolationMode.Frame;
                ent.DisplayName = $"{ldbId.TrackName} - {ldbId.TrackType} [{ldbId.Style}]";
                ent.Replay(tl);

                cl.Get<SpectateController>().Spectate(ent);

                UGuiManager.Instance.OpenModal<Modal_ReplayTools>();
            }
            finally
            {
                _loadingReplay = false;
            }
        }

    }
}

