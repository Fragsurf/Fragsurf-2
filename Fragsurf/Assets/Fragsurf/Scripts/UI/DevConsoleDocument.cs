using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fragsurf.UI
{
    public class DevConsoleDocument : GameDocument
    {

        private ScrollView _history;
        private TextField _input;
        private VisualElement _autoComplete;
        private WaitForEndOfFrame _eof = new WaitForEndOfFrame();
        private string _autoCompleted;

        protected override void _OnEnable()
        {
            if (!UiDocument.visualTreeAsset)
            {
                UiDocument.visualTreeAsset = GameObject.Instantiate<VisualTreeAsset>(Resources.Load<VisualTreeAsset>("UI/DevConsole"));
            }

            DocumentName = "DevConsole";
            OpenOnStart = false;
            CursorType = CursorType.Always;
            SingleInstance = true;

            _history = UiDocument.rootVisualElement.Q<ScrollView>("History");
            _autoComplete = UiDocument.rootVisualElement.Q("AutoComplete");
            _input = UiDocument.rootVisualElement.Q<TextField>("Input");

            PopulateAutoComplete(null);

            DevConsole.OnMessageLogged += WriteLine;
        }

        protected override void _OnDisable()
        {
            DevConsole.OnMessageLogged -= WriteLine;
        }

        protected override void _Update()
        {
            if(FocusedElement == _input)
            {
                var inputValue = _input.value;
                if((Input.GetKeyDown(KeyCode.Return)
                || Input.GetKeyDown(KeyCode.KeypadEnter)))
                {
                    if (!string.IsNullOrWhiteSpace(inputValue))
                    {
                        DevConsole.ExecuteLine(inputValue);
                        SetInput(string.Empty);
                    }
                }
                if(!string.IsNullOrWhiteSpace(inputValue) 
                    && !string.Equals(_autoCompleted, inputValue, System.StringComparison.OrdinalIgnoreCase))
                {
                    _autoCompleted = inputValue;
                    var commands = DevConsole.GetEntriesStartingWith(inputValue).Select(x => x.Name).ToList();
                    PopulateAutoComplete(commands);
                }
            }
        }

        protected override void _OnOpen()
        {
            SetInput(string.Empty);
            _autoCompleted = string.Empty;
        }

        private void PopulateAutoComplete(List<string> entries)
        {
            _autoComplete.visible = false;
            _autoComplete.Children().ToList().ForEach(e => e.RemoveFromHierarchy());
            if(entries == null || entries.Count == 0)
            {
                return;
            }
            _autoComplete.visible = true;
            foreach (var entry in entries)
            {
                var label = new Label(entry);
                label.RegisterCallback<MouseDownEvent>((e) =>
                {
                    SetInput(entry + " ");
                });
                _autoComplete.Insert(_autoComplete.childCount, label);
            }
        }

        private void SetInput(string value, bool takeFocus = true)
        {
            _input.value = value;
            if (takeFocus)
            {
                StartCoroutine(TakeFocusAfterFrame());
            }
        }

        private void WriteLine(string message)
        {
            var label = new Label(message);
            label.RegisterCallback<MouseDownEvent>((e) =>
            {
                Debug.Log(message);
            });
            _history.Insert(_history.childCount, label);
            StartCoroutine(ScrollToElement(_history.Children().Last()));
        }

        private IEnumerator TakeFocusAfterFrame()
        {
            _input.delegatesFocus = true;
            _input.focusable = true;
            yield return _eof;
            _input.Focus();
            yield return _eof;
            _input.SendEvent(KeyDownEvent.GetPooled('\u2192', KeyCode.RightArrow, EventModifiers.FunctionKey));
        }

        private IEnumerator ScrollToElement(VisualElement scrollTo)
        {
            yield return _eof;
            _history.ScrollTo(scrollTo);
        }

    }
}

