using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_ChatboxAutoCompleteEntryData
    {
        public string Command;
        public string Description;
        public Action OnClick;
    }
    public class Modal_ChatboxAutoCompleteEntry : EntryElement<Modal_ChatboxAutoCompleteEntryData>
    {

        [SerializeField]
        private TMP_Text _text;
        [SerializeField]
        private Button _button;

        public override void LoadData(Modal_ChatboxAutoCompleteEntryData data)
        {
            _text.text = $"/{data.Command} <color=white>| {data.Description}</color>";
            _button.onClick.AddListener(() => data.OnClick?.Invoke());
        }

    }
}
