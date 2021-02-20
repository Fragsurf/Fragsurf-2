using System;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Structs;
using UnityEngine.UI;
using Fragsurf.Utility;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.Experimental.GlobalIllumination;
using System.Runtime.InteropServices;
using System.Text;

namespace Fragsurf.UI
{
    public class Modal_Browser : UGuiModal
    {

        private static bool _htmlSurfaceInit;

        [SerializeField]
        private RawImage _browserImage;
        [SerializeField]
        private TMP_InputField _addressBar;
        [SerializeField]
        private TMP_Text _statusText;

        private Browser _browser;
        private Texture2D _texture;

        private void Start()
        {
            if (!SteamClient.IsValid)
            {
                return;
            }
            if (!_htmlSurfaceInit)
            {
                var init = SteamHTMLSurface.Init();
                if (!init)
                {
                    return;
                }
                _htmlSurfaceInit = true;
            }
            //Dispatch.OnDebugCallback += (e, b, c) =>
            //{
            //    Debug.Log(e + ":" + b + ":" + c);
            //};
            SteamHTMLSurface.OnBrowserReady += SteamHTMLSurface_OnBrowserReady;
            SteamHTMLSurface.OnFinishedLoading += SteamHTMLSurface_OnFinishedLoading;
            SteamHTMLSurface.OnStartRequest += SteamHTMLSurface_OnStartRequest;
            SteamHTMLSurface.OnNeedsPaint += SteamHTMLSurface_OnNeedsPaint;
            SteamHTMLSurface.OnJSAlert += SteamHTMLSurface_OnJSAlert;
            SteamHTMLSurface.OnJSConfirm += SteamHTMLSurface_OnJSConfirm;
            SteamHTMLSurface.OnNewWindow += SteamHTMLSurface_OnBrowserNewWindow;
            SteamHTMLSurface.OnNewTab += SteamHTMLSurface_OnNewTab;
            SteamHTMLSurface.OnStatusText += SteamHTMLSurface_OnStatusText;
            _addressBar.onSubmit.AddListener(OnAddressBarSubmit);
            CreateBrowser();
        }

        private void SteamHTMLSurface_OnStatusText(Browser browser, string message)
        {
            if(browser.Handle != _browser.Handle)
            {
                return;
            }
            _statusText.text = message;
        }

        private void SteamHTMLSurface_OnNewTab(Browser arg1, string arg2)
        {
            Debug.Log("New tab!!");
            _browser.LoadURL(arg2, null);
        }

        private void SteamHTMLSurface_OnBrowserNewWindow(Browser arg1, BrowserNewWindow arg2)
        {
            Debug.Log("New window!!");
            arg2.NewBrowser.Remove();
            _browser.LoadURL(arg2.Url, null);
        }

        private void OnAddressBarSubmit(string value)
        {
            _browser.LoadURL(value, null);
            _addressBar.DeactivateInputField(true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (SteamClient.IsValid)
            {
                _browser.Remove();
            }
        }

        private void Update()
        {
            if(!IsOpen || !SteamClient.IsValid)
            {
                return;
            }

            Vector3[] corners = new Vector3[4];
            _browserImage.GetComponent<RectTransform>().GetWorldCorners(corners);
            var sz = corners[2] - corners[0];
            var min = new Vector2(corners[0].x, corners[0].y);
            var rect = new Rect(min.x, min.y, sz.x, sz.y);
            var mpos = Input.mousePosition - new Vector3(min.x, min.y);
            _browser.MouseMove((int)mpos.x, (int)(sz.y - mpos.y));

            if (rect.Contains(Input.mousePosition))
            {
                _browser.SetKeyFocus(true);
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    _browser.MouseDown(0);
                }
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    _browser.MouseDown(1);
                }
                if (Input.GetKeyDown(KeyCode.Mouse2))
                {
                    _browser.MouseDown(2);
                }
                if(Input.mouseScrollDelta.y != 0)
                {
                    _browser.MouseWheel((int)(Input.mouseScrollDelta.y * 32));
                }

                if (Input.anyKey)
                {
                    var shift = Input.GetKey(KeyCode.LeftShift);
                    var ctrl = Input.GetKey(KeyCode.LeftControl);
                    var alt = Input.GetKey(KeyCode.LeftAlt);
                    foreach (KeyCode key in Enum.GetValues(typeof(KeyCode))) 
                    {
                        if (!mappings.ContainsKey(key))
                        {
                            continue;
                        }
                        if (Input.GetKeyDown(key))
                        {
                            _browser.KeyDown(mappings[key], alt, ctrl, shift, false);
                        }
                        if (Input.GetKeyUp(key))
                        {
                            _browser.KeyUp(mappings[key], alt, ctrl, shift);
                        }
                    }
                }
            }
            else
            {
                _browser.SetKeyFocus(false);
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                _browser.MouseUp(0);
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                _browser.MouseUp(1);
            }
            if (Input.GetKeyUp(KeyCode.Mouse2))
            {
                _browser.MouseUp(2);
            }
        }

        private void SteamHTMLSurface_OnBrowserReady(Browser browser) 
        {
            Debug.Log("Browser ready: " + browser.Handle);

            if (browser.Handle != _browser.Handle)
            {
                return;
            }

            Debug.Log("Browser ready");
        }

        private void SteamHTMLSurface_OnJSConfirm(Browser browser, string arg2)
        {
            if (browser.Handle != _browser.Handle)
            {
                return;
            }

            Debug.Log("Confirm dialog");
            browser.JSDialogResponse(true);
        }

        private void SteamHTMLSurface_OnJSAlert(Browser browser, string arg2)
        {
            if (browser.Handle != _browser.Handle)
            {
                return;
            }

            Debug.Log("JS Alert");
            browser.JSDialogResponse(true);
        }

        private void SteamHTMLSurface_OnFinishedLoading(Browser browser, string url, string arg3) 
        {
            if (browser.Handle != _browser.Handle)
            {
                return;
            }

            _addressBar.text = url;
            _addressBar.DeactivateInputField(true);
        }

        private void SteamHTMLSurface_OnStartRequest(Browser browser, string url, string target, string arg4, bool arg5)
        {
            if(browser.Handle != _browser.Handle)
            {
                return;
            }
            Debug.Log("Start request");
            browser.AllowStartRequest(false);
        }

        private void SteamHTMLSurface_OnNeedsPaint(Browser browser, BrowserPaint paint)
        {
            if(browser.Handle != _browser.Handle)
            {
                return;
            }

            if (_texture == null || _texture.width != paint.Width || _texture.height != paint.Height)
            {
                _texture = new Texture2D(paint.Width, paint.Height, TextureFormat.BGRA32, false, false);
                _browserImage.texture = _texture;
            }

            var rect = _browserImage.GetComponent<RectTransform>().GetScreenSpaceRect();
            if(paint.Width != rect.width || paint.Height != rect.height)
            {
                SetSize();
            }

            int dataSize = (int)(paint.Width * paint.Height * 4);
            _texture.LoadRawTextureData(paint.ImageData, dataSize);
            _texture.Apply(true);
        }

        private async void CreateBrowser()
        {
            _browser = await SteamHTMLSurface.CreateBrowser(null, null);
            SetSize();
            _browser.LoadURL("https://fragsurf.com", string.Empty);
        }

        private void SetSize()
        {
            var rect = _browserImage.GetComponent<RectTransform>().GetScreenSpaceRect();
            _browser.SetSize((int)rect.width, (int)rect.height);
        }

        private void OnGUI()
        {
            if(_browser.Handle == 0)
            {
                return;
            }
            if(Event.current.type == EventType.KeyDown)
            {
                var unicode = Event.current.character;
                if (unicode == 10) unicode = (char)13; // enter is special I guess
                _browser.KeyChar(unicode, Event.current.alt, Event.current.control, Event.current.shift);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int ToUnicode(
            uint virtualKeyCode,
            uint scanCode,
            byte[] keyboardState,
            StringBuilder receivingBuffer,
            int bufferSize,
            uint flags
        );

        private static Dictionary<KeyCode, int> mappings = new Dictionary<KeyCode, int>() 
        {
		    // http://www.w3.org/2002/09/tests/keys.html
		    {KeyCode.Escape, 27},
            {KeyCode.F1, 112},
            {KeyCode.F2, 113},
            {KeyCode.F3, 114},
            {KeyCode.F4, 115},
            {KeyCode.F5, 116},
            {KeyCode.F6, 117},
            {KeyCode.F7, 118},
            {KeyCode.F8, 119},
            {KeyCode.F9, 120},
            {KeyCode.F10, 121},
            {KeyCode.F11, 122},
            {KeyCode.F12, 123},
            {KeyCode.SysReq, 44}, {KeyCode.Print, 44},
            {KeyCode.ScrollLock, 145},
            {KeyCode.Pause, 19},
            {KeyCode.BackQuote, 192},


            {KeyCode.Alpha0, 48},
            {KeyCode.Alpha1, 49},
            {KeyCode.Alpha2, 50},
            {KeyCode.Alpha3, 51},
            {KeyCode.Alpha4, 52},
            {KeyCode.Alpha5, 53},
            {KeyCode.Alpha6, 54},
            {KeyCode.Alpha7, 55},
            {KeyCode.Alpha8, 56},
            {KeyCode.Alpha9, 57},
            {KeyCode.Minus, 189},
            {KeyCode.Equals, 187},
            {KeyCode.Backspace, 8},

            {KeyCode.Tab, 9},
		    //char keys
		    {KeyCode.LeftBracket, 219},
            {KeyCode.RightBracket, 221},
            {KeyCode.Backslash, 220},

            {KeyCode.CapsLock, 20},
		    //char keys
		    {KeyCode.Semicolon, 186},
            {KeyCode.Quote, 222},
            {KeyCode.Return, 13},

            {KeyCode.LeftShift, 16},
		    //char keys
		    {KeyCode.Comma, 188},
            {KeyCode.Period, 190},
            {KeyCode.Slash, 191},
            {KeyCode.RightShift, 16},

            {KeyCode.LeftControl, 17},
            {KeyCode.LeftCommand, 91}, {KeyCode.LeftWindows, 91},
            {KeyCode.LeftAlt, 18},
            {KeyCode.Space, 32},
            {KeyCode.RightAlt, 18},
            {KeyCode.RightCommand, 92}, {KeyCode.RightWindows, 92},
            {KeyCode.Menu, 93},
            {KeyCode.RightControl, 17},


            {KeyCode.Insert, 45},
            {KeyCode.Home, 36},
            {KeyCode.PageUp, 33},

            {KeyCode.Delete, 46},
            {KeyCode.End, 35},
            {KeyCode.PageDown, 34},

            {KeyCode.UpArrow, 38},
            {KeyCode.LeftArrow, 37},
            {KeyCode.DownArrow, 40},
            {KeyCode.RightArrow, 39},


            {KeyCode.Numlock, 144},
            {KeyCode.KeypadDivide, 111},
            {KeyCode.KeypadMultiply, 106},
            {KeyCode.KeypadMinus, 109},

            {KeyCode.Keypad7, 103},
            {KeyCode.Keypad8, 104},
            {KeyCode.Keypad9, 105},
            {KeyCode.KeypadPlus, 107},

            {KeyCode.Keypad4, 100},
            {KeyCode.Keypad5, 101},
            {KeyCode.Keypad6, 102},

            {KeyCode.Keypad1, 97},
            {KeyCode.Keypad2, 98},
            {KeyCode.Keypad3, 99},
            {KeyCode.KeypadEnter, 13},

            {KeyCode.Keypad0, 96},
            {KeyCode.KeypadPeriod, 110},

            { KeyCode.A, 0x41 },
            { KeyCode.B, 66 },
            { KeyCode.C, 67 },
            { KeyCode.D, 68 },
            { KeyCode.E, 69 },
            { KeyCode.F, 70 },
            { KeyCode.G, 71 },
            { KeyCode.H, 72 },
            { KeyCode.I, 73 },
            { KeyCode.J, 74 },
            { KeyCode.K, 75 },
            { KeyCode.L, 76 },
            { KeyCode.M, 77 },
            { KeyCode.N, 78 },
            { KeyCode.O, 79 },
            { KeyCode.P, 80 },
            { KeyCode.Q, 81 },
            { KeyCode.R, 82 },
            { KeyCode.S, 83 },
            { KeyCode.T, 84 },
            { KeyCode.U, 85 },
            { KeyCode.V, 86 },
            { KeyCode.W, 87 },
            { KeyCode.X, 88 },
            { KeyCode.Y, 89 },
            { KeyCode.Z, 90 },
        };

    }
}

