using Fragsurf.Client;
using Fragsurf.Maps;
using Fragsurf.Server;
using Fragsurf.Shared;
using Fragsurf.Utility;
using Steamworks;
using System;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_CreateGame : UGuiModal
    {

        [Header("Misc")]
        [SerializeField]
        private GameObject _disabledCover;

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

        [Header("Lobby Settings")]
        [SerializeField]
        private TMP_InputField _lobbyName;
        [SerializeField]
        private TMP_InputField _lobbyPass;

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

            ResetSettingInputs();

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

        private void ResetSettingInputs()
        {
            _lobbyName.text = SteamClient.IsValid ? $"{SteamClient.Name}'s Lobby" : "Unknown's Lobby";
            _lobbyPass.text = string.Empty;
        }

        private void Update()
        {
            _disabledCover.SetActive(_selectedGamemode == null);
        }

        private void OnDisable()
        {
            ResetSettingInputs();
        }

        private async void CreateGame()
        {
            if (_selectedGamemode == null || _selectedMap == null)
            {
                UGuiManager.Instance.Popup("Selected gamemode or map is null, can't create game");
                return;
            }

            var n = SteamClient.IsValid ? SteamClient.Name : "Unknown";
            var lobbyName = string.IsNullOrEmpty(_lobbyName.text) ? $"{n}'s Lobby" : _lobbyName.text;
            var lobbyPass = _lobbyPass.text;

            var created = await GameCreator.Instance.CreateGame(lobbyName, lobbyPass, _selectedGamemode.Name, _selectedMap.Name);

            if (!created)
            {
                // do something
            }
        }

        private async void PopulateGamemodes()
        {
            var gamemodes = await GamemodeLoader.QueryAll();
            _selectedGamemode = null;
            _gamemodeTemplate.Clear();
            var firstSelected = false;
            foreach(var gm in gamemodes)
            {
                _gamemodeTemplate.Append(new GamemodeEntryData()
                {
                    Name = gm.Name,
                    Selected = !firstSelected,
                    OnSelect = () =>
                    {
                        _selectedGamemode = gm;
                        ClearMap();
                        PopulateMaps(/*gm.Identifier + "_"*/);
                    }
                });
                if(!firstSelected)
                {
                    firstSelected = true;
                    _selectedGamemode = gm;
                    ClearMap();
                    PopulateMaps(/*gm.Identifier + "_"*/);
                }
            }
        }

        private async void PopulateMaps(string prefix = null)
        {
            _mapTemplate.Clear();
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
                    },
                    IsNew = map.IsNew
                });
            }
            FilterMaps();
        }

        private void ShowMap(GamemodeData gamemode, BaseMap map)
        {
            if (map.Cover)
            {
                _mapCover.texture = map.Cover;
            }
            _mapCover.enabled = true;
            _mapTitle.text = $"<size=24>{map.Name}</size>\n<b>author:</b> {map.Author}";
            _mapTitle.transform.parent.gameObject.RebuildLayout();

            _mapCover.texture = map.LoadCoverImage();
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
                && !_selectedGamemode.MapHasPrefix(map.Name))
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

