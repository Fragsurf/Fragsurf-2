using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_ServerBrowserServerEntry : EntryElement<Modal_ServerBrowserServerEntry.Data>
    {

        public class Data
        {
            public string ServerName;
            public Action OnClick;
        }

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private Button _button;

        public override void LoadData(Data data)
        {
            _name.text = data.ServerName;
            _button.onClick.AddListener(() => data.OnClick?.Invoke());
        }

    }
}

