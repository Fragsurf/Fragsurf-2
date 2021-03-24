using Fragsurf.Client;
using Fragsurf.Maps;
using Fragsurf.Server;
using Fragsurf.Shared;
using Fragsurf.Utility;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_CreateGame : UGuiModal
    {

        [Header("Map Filters")]
        [SerializeField]
        private TMP_InputField _searchInput;
        [SerializeField]
        private Toggle _onlyForThisGamemode;
        [SerializeField]
        private Toggle _showWorkshopMaps;
        [SerializeField]
        private Toggle _showMountedMaps;
        [SerializeField]
        private Button _refreshButton;

        [Header("Map Details")]
        [SerializeField]
        private TMP_Text _mapTitle;
        [SerializeField]
        private RawImage _mapCover;
        [SerializeField]
        private Button _createButton;

        private GamemodeEntry _gamemodeTemplate;
        private MapEntry _mapTemplate;
        private GamemodeData _selectedGamemode;
        private BaseMap _selectedMap;

        private void Start()
        {
            _mapTemplate = GetComponentInChildren<MapEntry>();
            _gamemodeTemplate = GetComponentInChildren<GamemodeEntry>();
            if (_gamemodeTemplate)
            {
                _gamemodeTemplate.gameObject.SetActive(false);
                PopulateGamemodes();
            }
            if (_mapTemplate)
            {
                _mapTemplate.gameObject.SetActive(false);
            }

            _searchInput.onValueChanged.AddListener((s) => { FilterMaps(); });
            _onlyForThisGamemode.onValueChanged.AddListener((b) => { FilterMaps(); });
            _showWorkshopMaps.onValueChanged.AddListener((b) => { FilterMaps(); });
            _showMountedMaps.onValueChanged.AddListener((b) => { FilterMaps(); });
            _refreshButton.onClick.AddListener(() =>
            {
                if (!_selectedGamemode)
                {
                    return;
                }
                ClearMap();
                PopulateMaps();
            });

            _createButton.onClick.AddListener(CreateGame);

            ClearMap();
        }

        private async void CreateGame()
        {
            if(_selectedGamemode == null || _selectedMap == null)
            {
                Debug.LogError("Selected gamemode or map is null, can't create game");
                return;
            }

            var server = FSGameLoop.GetGameInstance(true);
            if (server)
            {
                server.Destroy();
            }

            var client = FSGameLoop.GetGameInstance(false);
            if (client)
            {
                client.Destroy();
            }

            var cl = new GameObject("[Client]").AddComponent<GameClient>();
            var serverResult = await cl.GameLoader.CreateServerAsync(_selectedMap.Name, _selectedGamemode.Name, "Testing my map!");
            if (serverResult == GameLoadResult.Success)
            {
                var sv = FSGameLoop.GetGameInstance(true) as GameServer;

                var joinResult = await cl.GameLoader.JoinGameAsync("localhost", sv.Socket.GameplayPort, sv.Socket.ServerPassword);
            }
        }

        private async void PopulateGamemodes()
        {
            var gamemodes = await GamemodeLoader.QueryAll();
            _selectedGamemode = null;
            _gamemodeTemplate.Clear();
            foreach(var gm in gamemodes)
            {
                _gamemodeTemplate.Append(new GamemodeEntryData()
                {
                    Name = gm.Name,
                    OnSelect = () =>
                    {
                        _selectedGamemode = gm;
                        PopulateMaps(/*gm.Identifier + "_"*/);
                    }
                });
            }
        }

        private async void PopulateMaps(string prefix = null)
        {
            var maps = await Map.QueryAll(prefix);
            _selectedMap = null;
            _mapTemplate.Clear();
            foreach(var map in maps) 
            {
                _mapTemplate.Append(new MapEntryData()
                {
                    Name = map.Name,
                    Map = map,
                    OnClick = () => 
                    {
                        _selectedMap = map;
                        ShowMap(_selectedGamemode, _selectedMap);
                    }
                });
            }
            FilterMaps();
        }

        private void ShowMap(GamemodeData gamemode, BaseMap map)
        {
            if (map.Cover)
            {
                _mapCover.enabled = true;
                _mapCover.texture = map.Cover;
            }
            _mapTitle.text = $"<size=24>{map.Name}</size>\n<b>author:</b> {map.Author}";
            _mapTitle.transform.parent.gameObject.RebuildLayout();
        }

        private void ClearMap()
        {
            _mapCover.enabled = false;
            _mapTitle.text = $"<size=24>Select a map</size>\n<b>author:</b> ";
        }

        private void FilterMaps()
        {
            foreach(var map in _mapTemplate.Children)
            {
                var entry = map.GetComponent<MapEntry>();
                map.SetActive(IsMapVisible(entry.Map));
            }
        }

        private bool IsMapVisible(BaseMap map)
        {
            if (!string.IsNullOrWhiteSpace(_searchInput.text)
                && map.Name.IndexOf(_searchInput.text, StringComparison.OrdinalIgnoreCase) == -1)
            {
                return false;
            }

            //if(!_showWorkshopMaps.isOn 
            //    && entry.Map is WorkshopMap)
            //{
            //    return false;
            //}

            if(_selectedGamemode
                && _onlyForThisGamemode.isOn 
                && map.Name.IndexOf(_selectedGamemode.Identifier, StringComparison.OrdinalIgnoreCase) == -1)
            {
                return false;
            }

            if(!_showMountedMaps.isOn
                && map.IsMounted)
            {
                return false;
            }

            return true;
        }

    }
}

