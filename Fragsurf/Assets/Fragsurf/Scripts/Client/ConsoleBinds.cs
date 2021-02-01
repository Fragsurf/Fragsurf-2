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

            public BindData(KeyCode key, string command)
            {
                Type = (command[0] == '+') ? BindType.Hold : BindType.Normal;
                Key = key;
                Command = command;
                ReleaseCommand = Command.Replace('+', '-');
            }

            public readonly BindType Type;
            public readonly KeyCode Key;
            public readonly string Command;
            public readonly string ReleaseCommand;
            public bool IsPressed;
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
                var keyDown = Input.GetKeyDown(bind.Key);
                var keyUp = Input.GetKeyUp(bind.Key);

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
            if (string.IsNullOrWhiteSpace(command)
                || !Enum.TryParse(key, true, out KeyCode keyCode))
            {
                return;
            }

            Unbind(key);

            Binds.Add(new BindData(keyCode, command));
        }

        [ConCommand("unbind", "Unbinds all commands from a key")]
        public void Unbind(string key)
        {
            if (!Enum.TryParse(key, true, out KeyCode keyCode))
            {
                Debug.Log("No key: " + key);
                return;
            }
            foreach(var bd in FindBindDatas(keyCode))
            {
                RemoveBind(bd);
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

        public List<BindData> FindBindDatas(KeyCode key)
        {
            return Binds.Where(x => x.Key == key).ToList();
        }

        public List<BindData> FindBindDatas(string command)
        {
            return Binds.Where(x => string.Equals(x.Command, command, StringComparison.OrdinalIgnoreCase)).ToList();
        }

    }
}

