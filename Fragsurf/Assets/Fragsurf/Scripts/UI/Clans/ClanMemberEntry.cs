using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class ClanMemberEntryData
    {
        public Friend Friend;
        public Action OnClick;
    }
    public class ClanMemberEntry : EntryElement<ClanMemberEntryData>
    {

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private Button _button;

        public override void LoadData(ClanMemberEntryData data)
        {
            _name.text = data.Friend.Name;
        }

    }
}

