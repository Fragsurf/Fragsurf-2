using System.Collections;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_ConsoleAutoComplete : MonoBehaviour
    {

        [SerializeField]
        private TMP_InputField _input;
        private Modal_ConsoleAutoCompleteEntry _autoCompleteTemplate;

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

        private void OnInputChanged(string newValue)
        {
            _autoCompleteTemplate.Clear();
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

    }
}

