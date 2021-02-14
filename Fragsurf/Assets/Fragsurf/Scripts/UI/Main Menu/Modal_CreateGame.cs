using Fragsurf.Shared;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_CreateGame : UGuiModal
    {

        private GamemodeEntry _gamemodeTemplate;

        private void Start()
        {
            _gamemodeTemplate = GetComponentInChildren<GamemodeEntry>();
            if (_gamemodeTemplate)
            {
                _gamemodeTemplate.gameObject.SetActive(false);
                PopulateGamemodes();
            }
        }

        private void PopulateGamemodes()
        {
            if (!_gamemodeTemplate)
            {
                return;
            }
            _gamemodeTemplate.Clear();
            foreach(var gm in Resources.FindObjectsOfTypeAll<GamemodeData>())
            {
                _gamemodeTemplate.Append(new GamemodeEntryData()
                {
                    Name = gm.Name,
                    OnSelect = () =>
                    {
                        Debug.Log("It's ya boy!");
                    }
                });
            }
        }

    }
}

