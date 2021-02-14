using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class MapEntryData
    {
        public string Name;
        public Action OnClick;
    }
    public class MapEntry : EntryElement<MapEntryData>
    {

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private Button _button;

        public override void LoadData(MapEntryData data)
        {
            _name.text = data.Name;
            if (_button)
            {
                _button.onClick.AddListener(() =>
                {
                    data.OnClick?.Invoke();
                });
            }
        }

    }
}

