using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf
{
    public class DevConsoleHotkeys : MonoBehaviour
    {
        private class HotkeyInfo
        {
            public KeyCode Key;
            public string Command;
        }

        private List<HotkeyInfo> _hotkeyInfos = new List<HotkeyInfo>();

        public void Register(KeyCode key, string command)
        {
            var info = new HotkeyInfo()
            {
                Key = key,
                Command = command
            };
            _hotkeyInfos.Add(info);
        }

        private void Update()
        {
            foreach(var hk in _hotkeyInfos)
            {
                if(Input.GetKeyDown(hk.Key))
                {
                    // down
                }
                else if (Input.GetKeyUp(hk.Key))
                {
                    // up
                }
                else if (Input.GetKey(hk.Key))
                {
                    // hold
                }
            }
        }

    }
}

