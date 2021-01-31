using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Server.Console
{
    public class EditorConsole : MonoBehaviour
    {

        public InputField input;
        public Text log;

        private void Awake()
        {
            input.onEndEdit.AddListener(val =>
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    ProcessInput(val);
                    ResetInput();
                }
            });

            DevConsole.OnMessageLogged += FSConsole_OnMessageLogged;
        }

        private void OnDestroy()
        {
            DevConsole.OnMessageLogged -= FSConsole_OnMessageLogged;
        }

        private void FSConsole_OnMessageLogged(string message)
        {
            PrintLine(message);
        }

        public void PrintLine(string message)
        {
            log.text += "\n" + message;
        }

        private void ProcessInput(string message)
        {
            DevConsole.ExecuteLine(message);
        }

        private void ResetInput()
        {
            input.text = string.Empty;
            input.ActivateInputField();
        }

    }
}
