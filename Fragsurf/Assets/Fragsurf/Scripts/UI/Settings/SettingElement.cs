using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.UI
{
    public abstract class SettingElement : MonoBehaviour
    {

        public string SettingName { get; private set; }

        public void Initialize(string settingName)
        {
            SettingName = settingName;
            _Initialize();
        }

        protected abstract void _Initialize();

    }
}

