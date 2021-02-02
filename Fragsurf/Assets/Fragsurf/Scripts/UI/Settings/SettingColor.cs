using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI 
{
    public class SettingColor : SettingElement
    {

        [SerializeField]
        private Button _openColorPicker;
        [SerializeField]
        private Image _previewColorImage;

        protected override void _Initialize()
        {
            _openColorPicker.onClick.AddListener(() =>
            {
                var color = DevConsole.GetVariable<Color>(SettingName);
                var colorPicker = UGuiManager.Instance.Find<Modal_ColorPicker>();
                colorPicker.OpenWithCallback(color, OnSave, OnCancel);
            });
        }

        public override void LoadValue(string val)
        {
            if (val != null && val.Length == 8 && val[0] != '#')
            {
                val = "#" + val;
            }
            ColorUtility.TryParseHtmlString(val, out Color color);
            _previewColorImage.color = color;
        }

        private void OnSave(Color newColor)
        {
            _previewColorImage.color = newColor;
            Queue($"{SettingName} #{ColorUtility.ToHtmlStringRGBA(newColor)}");
        }

        private void OnCancel()
        {
            LoadValue();
        }

    }
}


