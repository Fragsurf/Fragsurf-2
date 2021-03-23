using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.UI;

namespace Fragsurf.Client
{
    public class ConsoleBinds
    {

        public static bool Blocked;

        public class BindData
        {

            public BindData(string key, string command)
            {
                Type = (command[0] == '+') ? BindType.Hold : BindType.Normal;
                KeyName = key;
                Command = command;
                ReleaseCommand = Command.Replace('+', '-');

                if (Enum.TryParse(key, true, out _keyCode))
                {
                    _scroll = 0;
                    IsValid = true;
                }
                else if (key.Equals("mwheelup", StringComparison.OrdinalIgnoreCase))
                {
                    _scroll = 1;
                    IsValid = true;
                }
                else if (key.Equals("mwheeldown", StringComparison.OrdinalIgnoreCase))
                {
                    _scroll = -1;
                    IsValid = true;
                }
            }

            public readonly BindType Type;
            public readonly string KeyName;
            public readonly string Command;
            public readonly string ReleaseCommand;
            public readonly bool IsValid;
            public bool IsPressed;

            private int _scroll;
            private KeyCode _keyCode;

            public bool JustDown()
            {
                if(_scroll != 0)
                {
                    return Mathf.Sign(Input.mouseScrollDelta.y) == Mathf.Sign(_scroll);
                }
                return Input.GetKeyDown(_keyCode);
            }

            public bool JustUp()
            {
                if (_scroll != 0)
                {
                    return IsPressed && Mathf.Sign(Input.mouseScrollDelta.y) != Mathf.Sign(_scroll);
                }
                return Input.GetKeyUp(_keyCode);
            }

        }

        public enum BindType
        {
            None,
            Normal,
            Hold
        }

        public ConsoleBinds()
        {
            DevConsole.RegisterObject(this);
        }

        ~ConsoleBinds()
        {
            DevConsole.RemoveAll(this);
        }

        public List<BindData> Binds { get; } = new List<BindData>();

        public void Clear()
        {
            Binds.Clear();
        }

        public void Update()
        {
            if (UGuiManager.Instance.HasFocusedInput()
                || Blocked)
            {
                return;
            }

            foreach(var bind in Binds)
            {
                var keyDown = bind.JustDown();
                var keyUp = bind.JustUp();

                bind.IsPressed = keyDown && !keyUp;

                if (bind.Type == BindType.Hold)
                {
                    if (bind.IsPressed)
                    {
                        DevConsole.ExecuteLine(bind.Command);
                    }
                    else if (keyUp)
                    {
                        DevConsole.ExecuteLine(bind.ReleaseCommand);
                    }
                }
                else if (keyDown)
                {
                    DevConsole.ExecuteLine(bind.Command);
                }
            }
        }

        [ConCommand("bind", "Binds a key to a command", ConVarFlags.None)]
        public void Bind(string key, string command)
        {
            var scrollDown = key.Equals("mwheeldown", StringComparison.OrdinalIgnoreCase);
            var scrollUp = key.Equals("mwheelup", StringComparison.OrdinalIgnoreCase);
            var scroll = scrollDown ? -1 : (scrollUp ? 1 : 0);

            if (string.IsNullOrWhiteSpace(command)
                || !Enum.TryParse(key, true, out KeyCode keyCode)
                && !scrollDown
                && !scrollUp)
            {
                return;
            }

            Unbind(key);
            Binds.Add(new BindData(key, command));
        }

        [ConCommand("unbind", "Unbinds all commands from a key")]
        public void Unbind(string key)
        {
            for (int i = Binds.Count - 1; i >= 0; i--)
            {
                if (Binds[i].KeyName.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    Binds.RemoveAt(i);
                }
            }
        }

        [ConCommand("unbindcommand", "Unbinds a command from any key bound to it")]
        public void UnbindCommand(string command)
        {
            var bindDatas = FindBindDatas(command);
            foreach(var bd in bindDatas)
            {
                RemoveBind(bd);
            }
        }

        [ConCommand("hardbind", "Unbinds a command from all keys using it, and binds it to only the given key.")]
        public void HardBind(string key, string command)
        {
            UnbindCommand(command);
            Bind(key, command);
        }

        private void RemoveBind(BindData bind)
        {
            Binds.Remove(bind);
        }

        public List<BindData> FindBindDatas(string command)
        {
            return Binds.Where(x => string.Equals(x.Command, command, StringComparison.OrdinalIgnoreCase)).ToList();
        }

    }
}

