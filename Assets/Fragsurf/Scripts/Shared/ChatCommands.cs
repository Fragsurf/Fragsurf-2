using Fragsurf.DataEditor;
using Fragsurf.Shared.Player;
using Fragsurf.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Fragsurf.Shared
{
    public class ChatCommands : FSSharedScript
    {

        private List<ChatCommand> _commandInstances = new List<ChatCommand>();

        public List<ChatCommand> FindCommandsStartingWith(string input)
        {
            if (string.IsNullOrEmpty(input)
                || input[0] == '/' && input.Length == 1)
            {
                return new List<ChatCommand>(_commandInstances);
            }

            if(input[0] == '/')
            {
                input = input.Remove(0, 1);
            }

            var result = new List<ChatCommand>();

            foreach(var cmd in _commandInstances)
            {
                foreach(var cmd2 in cmd.Attribute.Commands)
                {
                    if(cmd2.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(cmd);
                        break;
                    }
                }
            }

            return result;
        }

        protected override void OnPlayerChatCommand(BasePlayer player, string[] args)
        {
            if(args == null || args.Length == 0)
            {
                return;
            }

            foreach (var cmd in _commandInstances)
            {
                if (!cmd.Attribute.Commands.Contains(args[0]))
                {
                    continue;
                }
                cmd.Execute(player, args.RemoveAt(0));
            }
        }

        public void Register(FSSharedScript script)
        {
            var t = script.GetType();
            foreach (var method in t.GetMethodsRecursive())
            {
                foreach (var attr in method.GetCustomAttributes<ChatCommandAttribute>())
                {
                    var methodInfo = method.GetBaseDefinition();
                    _commandInstances.Add(new ChatCommand(script, attr, methodInfo));
                }
            }
        }

        public void UnRegister(FSSharedScript script)
        {
            for(int i = _commandInstances.Count - 1; i >= 0; i--)
            {
                if(_commandInstances[i].Owner == script)
                {
                    _commandInstances.RemoveAt(i);
                }
            }
        }

        public class ChatCommand
        {

            public readonly FSSharedScript Owner;
            public readonly ChatCommandAttribute Attribute;
            public readonly MethodInfo MethodInfo;
            public readonly ParameterInfo[] Parameters;

            public ChatCommand(FSSharedScript owner, ChatCommandAttribute attr, MethodInfo methodInfo)
            {
                Owner = owner;
                Attribute = attr;
                MethodInfo = methodInfo;
                Parameters = methodInfo.GetParameters();
            }

            public void Execute(BasePlayer player, string[] args)
            {
                if (Parameters.Length == 0)
                {
                    MethodInfo.Invoke(Owner, null);
                    return;
                }

                if (Parameters.Length == 1)
                {
                    MethodInfo.Invoke(Owner, new object[] { player });
                    return;
                }

                try
                {
                    var parameters = new List<object>() { player };
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (i >= Parameters.Length)
                        {
                            break;
                        }
                        var arg = args[i];
                        var obj = Convert.ChangeType(arg, Parameters[i + 1].ParameterType);
                        parameters.Add(obj);
                    }
                    MethodInfo.Invoke(Owner, parameters.ToArray());
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }

        }

    }

    public class ChatCommandAttribute : Attribute
    {

        public readonly HashSet<string> Commands = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public readonly string Description;

        public ChatCommandAttribute(string description = "", params string[] alternativeCommands)
        {
            Description = description;
            Commands.AddRange(alternativeCommands);
        }

    }
}

