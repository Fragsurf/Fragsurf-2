using Fragsurf.Actors;
using Fragsurf.Maps;
using Fragsurf.Movement;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.UI;
using Steamworks;
using System.Collections;
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

        private void Start()
        {
            _myRank.onClick.AddListener(() => {

            });

            _top100.onClick.AddListener(() =>
            {

            });

            _friends.onClick.AddListener(() =>
            {

            });

            _rankTemplate = GameObject.FindObjectOfType<Modal_BunnyhopRanksRankEntry>();
            _rankTemplate.gameObject.SetActive(false);
            _trackTemplate = GameObject.FindObjectOfType<Modal_BunnyhopRanksTrackEntry>();
            _trackTemplate.gameObject.SetActive(false);

            LoadTracks();
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
                        LoadRanks(fsmtrack);
                    },
                    Selected = firstTrack
                });
                if (firstTrack)
                {
                    LoadRanks(fsmtrack);
                    firstTrack = false;
                }
            }
        }

        private async void LoadRanks(FSMTrack track)
        {
            if (!SteamClient.IsValid)
            {
                return;
            }

            var cl = FSGameLoop.GetGameInstance(false);
            if (!cl)
            {
                return;
            }

            var id = BaseLeaderboardSystem.GetLeaderboardId(Map.Current.Name, track, MoveStyle.FW);
            var entries = await cl.Get<BunnyhopTracks>().LeaderboardSystem.Query(id, 1, 100);
            _selectedTrack = track;
            _rankTemplate.Clear();

            foreach (var entry in entries)
            {
                var userid = entry.UserId;
                var rank = entry.Rank;
                _rankTemplate.Append(new Modal_BunnyhopRanksRankEntryData()
                {
                    Entry = entry,
                    OnClickProfile = () => SteamFriends.OpenUserOverlay(userid, "steamid"),
                    OnClickReplay = () => SpawnBot(id, rank)
                });
            }
        }

        private async void SpawnBot(LeaderboardIdentifier ldbId, int rank)
        {
            var cl = FSGameLoop.GetGameInstance(false);
            var data = await cl.Get<BunnyhopTracks>().LeaderboardSystem.DownloadReplayAsync(ldbId, rank);
            if(data == null)
            {
                return;
            }

            var tl = EntityTimeline.Deserialize<BunnyhopTimeline>(data);
            if(tl == null)
            {
                return;
            }

            var ent = new Human(cl);
            cl.EntityManager.AddEntity(ent);
            ent.InterpolationMode = InterpolationMode.Frame;
            ent.Replay(tl);
        }

    }
}

