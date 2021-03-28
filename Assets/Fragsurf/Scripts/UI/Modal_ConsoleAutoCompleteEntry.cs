using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class AutoCompleteEntryData
    {
        public string Name;
        public string Description;
        public string Value;
        public Action OnClick;
    }

    public class Modal_ConsoleAutoCompleteEntry : EntryElement<AutoCompleteEntryData>
    {

        [SerializeField]
        private TMP_Text _text;
        [SerializeField]
        private Button _button;

        public override void LoadData(AutoCompleteEntryData data)
        {
            var val = !string.IsNullOrWhiteSpace(data.Value)
                ? $" <color=yellow>{data.Value}</color> "
                : string.Empty;
            _text.text = $"{data.Name}{val} - <color=#CCC>{data.Description}</color>";
            if(data.OnClick != null)
            {
                _button.onClick.AddListener(() => data.OnClick.Invoke());
            }
        }
    }
}

