using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_ConsoleAutoComplete : MonoBehaviour
    {

        [SerializeField]
        private Modal_Console _console;
        [SerializeField]
        private TMP_InputField _input;
        private Modal_ConsoleAutoCompleteEntry _autoCompleteTemplate;
        private int _autoCompleteIndex;
        private int _historyIndex = -1;
        private bool _dontResetHistoryIndex;

        private void Start()
        {
            _autoCompleteTemplate = gameObject.GetComponentInChildren<Modal_ConsoleAutoCompleteEntry>(true);
            if (_autoCompleteTemplate)
            {
                _autoCompleteTemplate.gameObject.SetActive(false);
                _input.onValueChanged.AddListener(OnInputChanged);
            }
        }

        private void OnDestroy()
        {
            if (_input)
            {
                _input.onValueChanged.RemoveListener(OnInputChanged);
            }
        }

        private void Update()
        {
            Update_NavigateAutoComplete();
            Update_NavigateHistory();
        }

        private void OnInputChanged(string newValue)
        {
            _autoCompleteTemplate.Clear();
            _autoCompleteIndex = -1;
            if (!_dontResetHistoryIndex)
            {
                _historyIndex = -1;
            }
            _dontResetHistoryIndex = false;
            var entries = DevConsole.GetEntriesStartingWith(newValue);
            foreach (var entry in entries)
            {
                var data = new AutoCompleteEntryData()
                {
                    Name = entry.Name,
                    Description = entry.Description,
                    Value = DevConsole.GetVariableAsString(entry.Name),
                    OnClick = () =>
                    {
                        _input.text = $"{entry.Name} ";
                        _input.ActivateInputField();
                        StartCoroutine(MoveToEndOfInput());
                    }
                };
                _autoCompleteTemplate.Append(data);
            }
        }

        private IEnumerator MoveToEndOfInput()
        {
            yield return new WaitForEndOfFrame();
            _input.MoveToEndOfLine(false, false);
        }

        private void Update_NavigateAutoComplete()
        {
            if (_autoCompleteTemplate.Children == null)
            {
                return;
            }

            bool autoCompleteChanged = false;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                autoCompleteChanged = true;
                _autoCompleteIndex--;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                autoCompleteChanged = true;
                _autoCompleteIndex++;
            }

            if (autoCompleteChanged)
            {
                if (_autoCompleteIndex < -1)
                {
                    _autoCompleteIndex = _autoCompleteTemplate.Children.Count - 1;
                }

                if (_autoCompleteIndex >= _autoCompleteTemplate.Children.Count)
                {
                    _autoCompleteIndex = -1;
                }

                if (_autoCompleteIndex == -1)
                {
                    _input.ActivateInputField();
                    StartCoroutine(MoveToEndOfInput());
                }
                else
                {
                    _autoCompleteTemplate.Children[_autoCompleteIndex].GetComponent<Button>().Select();
                }
            }
        }

        private void Update_NavigateHistory()
        {
            if ((Input.GetKeyDown(KeyCode.Backspace)
                || Input.GetKeyDown(KeyCode.LeftControl)
                || Input.GetKeyDown(KeyCode.RightArrow))
                && !_input.isFocused)
            {
                _input.ActivateInputField();
                StartCoroutine(MoveToEndOfInput());
            }

            bool historyIndexChanged = false;

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                historyIndexChanged = true;
                _historyIndex = Input.GetKey(KeyCode.LeftShift)
                    ? _historyIndex - 1
                    : _historyIndex + 1;
            }

            if (historyIndexChanged)
            {
                if (_historyIndex >= _console.InputHistory.Size)
                {
                    _historyIndex = -1;
                }

                if(_historyIndex < -1)
                {
                    _historyIndex = _console.InputHistory.Size - 1;
                }

                _dontResetHistoryIndex = true;

                _input.text = _historyIndex == -1
                    ? string.Empty
                    : _console.InputHistory[_historyIndex];

                _input.ActivateInputField();
                StartCoroutine(MoveToEndOfInput());
            }
        }

    }
}

