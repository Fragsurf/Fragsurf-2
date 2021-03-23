using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_ConsoleEntryData
    {
        public string Message;
        public float ElapsedTime;
    }

    public class Modal_ConsoleEntry : EntryElement<Modal_ConsoleEntryData>
    {

        [SerializeField]
        private TMP_Text _message;

        public override void LoadData(Modal_ConsoleEntryData data)
        {
            _message.text = data.Message;
        }
    }
}

