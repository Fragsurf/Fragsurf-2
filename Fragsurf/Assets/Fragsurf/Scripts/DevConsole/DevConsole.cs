using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Fragsurf.Utility;

namespace Fragsurf
{

    public delegate void ConsoleLogHandler(string message);

    public static class DevConsole
    {

        private class CmdCache
        {
            public string CommandName;
            public string[] CommandArgs;
        }

        public static event ConsoleLogHandler OnMessageLogged;
        public static event Action<string> OnVariableChanged;

        private static Dictionary<string, List<DevConsoleEntry>> _entries = new Dictionary<string, List<DevConsoleEntry>>();
        private static Dictionary<string, CmdCache> _commandCache = new Dictionary<string, CmdCache>();
        private static ConVarFlags _lockedFlags;

        public static void LockFlags(bool locked, ConVarFlags flags)
        {
            if (locked)
            {
                _lockedFlags |= flags;
            }
            else
            {
                _lockedFlags &= ~_lockedFlags;
            }
        }

        public static void RaiseVariableChanged(string variableName)
        {
            OnVariableChanged?.Invoke(variableName);
        }

        public static void RegisterObject(object owner)
        {
            var t = owner.GetType();
            var bindingFlags = BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Instance
                | BindingFlags.FlattenHierarchy;

            foreach (var prop in t.GetProperties(bindingFlags))
            {
                foreach(var attr in prop.GetCustomAttributes<ConVarAttribute>())
                {
                    var getterType = typeof(Func<>).MakeGenericType(prop.PropertyType);
                    var setterType = typeof(Action<>).MakeGenericType(prop.PropertyType);
                    var entryType = typeof(DevConsoleVariable<>).MakeGenericType(prop.PropertyType);
                    var getter = prop.GetGetMethod().CreateDelegate(getterType, owner);
                    var setter = prop.GetSetMethod().CreateDelegate(setterType, owner);
                    var entry = Activator.CreateInstance(entryType, owner, attr.Path, attr.Description, getter, setter) as DevConsoleEntry;
                    entry.Flags = attr.Flags;
                    AddEntry(entry);
                }
            }

            foreach (var method in t.GetMethodsRecursive())
            {
                foreach (var attr in method.GetCustomAttributes<ConCommandAttribute>())
                {
                    var methodInfo = method.GetBaseDefinition();
                    var entry = new DevConsoleCommand2(owner, attr.Path, attr.Description, methodInfo);
                    entry.Flags = attr.Flags;
                    AddEntry(entry);
                }
            }
        }

        public static List<DevConsoleEntry> GetEntriesStartingWith(string value, bool skipDuplicates = true)
        {
            var result = new List<DevConsoleEntry>();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }

            foreach(var kvp in _entries)
            {
                if(kvp.Key.StartsWith(value, StringComparison.OrdinalIgnoreCase)
                    && kvp.Value != null 
                    && kvp.Value.Count > 0)
                {
                    result.Add(kvp.Value[0]);
                }
            }

            return result;
        }

        public static void RegisterCommand(string name, string description, object owner, Action<string[]> callback, bool silent = false, ConVarFlags flags = ConVarFlags.None)
        {
            var entry = new DevConsoleCommand(owner, name, description, callback);

            if (silent)
            {
                flags |= ConVarFlags.Silent;
            }

            entry.Flags = flags;

            AddEntry(entry);
        }

        public static void RegisterVariable<T>(string name, string description, Func<T> getter, Action<T> setter, object owner, ConVarFlags flags = ConVarFlags.None)
        {
            var entry = new DevConsoleVariable<T>(owner, name, description, getter, setter);
            entry.Flags = flags;

            AddEntry(entry);
        }

        private static void AddEntry(DevConsoleEntry entry)
        {
            if (!_entries.ContainsKey(entry.Name))
            {
                _entries[entry.Name] = new List<DevConsoleEntry>();
            }

            _entries[entry.Name].Add(entry);
        }

        public static void RemoveVariable<T>(string name, object owner)
        {
            if(!_entries.ContainsKey(name))
            {
                return;
            }
            _entries[name].RemoveAll(x => x is DevConsoleVariable<T> && x.Name == name && x.Owner == owner);
        }

        public static void RemoveAll(object owner)
        {
            foreach(var kvp in _entries)
            {
                kvp.Value.RemoveAll(x => x.Owner == owner);
            }
        }

        public static void RemoveCommand(string name, object owner)
        {
            if (!_entries.ContainsKey(name))
            {
                return;
            }
            _entries[name].RemoveAll(x => x is DevConsoleCommand && x.Name == name && x.Owner == owner);
        }

        public static bool VariableHasFlags(string name, ConVarFlags flags)
        {
            if(!_entries.ContainsKey(name)
                || _entries[name].Count == 0)
            {
                return false;
            }
            return _entries[name][0].Flags.HasFlag(flags);
        }

        public static List<string> GetVariablesWithFlags(ConVarFlags flags)
        {
            var result = new List<string>();
            foreach(var kvp in _entries)
            {
                foreach(var entry in kvp.Value)
                {
                    if(entry.Flags.HasFlag(flags)
                        && entry.GetType().GetGenericTypeDefinition() == typeof(DevConsoleVariable<>))
                    {
                        result.Add(entry.Name);
                    }
                }
            }
            return result;
        }

        public static string GetVariableAsString(string variableName)
        {
            var type = GetVariableType(variableName);
            if(type == null)
            {
                return string.Empty;
            }
            var method = typeof(DevConsole).GetMethod(nameof(DevConsole.GetVariable));
            var generic = method.MakeGenericMethod(type);
            var result = generic.Invoke(null, new object[] { variableName });
            return result != null ? result.ToString() : string.Empty;
        }

        public static T GetVariable<T>(string varName)
        {
            var entry = FindEntry(varName);
            if (entry == null
                || !(entry is DevConsoleVariable<T> variable))
            {
                return default;
            }
            return variable.GetValue();
        }

        public static void SetVariable<T>(string varName, T value, bool bypassLock = false, bool noEvent = false)
        {
            if(!_entries.ContainsKey(varName))
            {
                return;
            }
            foreach(var entry in _entries[varName])
            {
                if (!(entry is DevConsoleVariable<T> v))
                {
                    continue;
                }

                if (IsLocked(entry.Flags) && !bypassLock)
                {
                    continue;
                }

                v.SetValue(value, noEvent);
            }
        }

        public static void ExecuteLine(string line, bool bypassLock = false)
        {
            if(string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            string cmdName;
            string[] args;

            if(_commandCache.ContainsKey(line))
            {
                cmdName = _commandCache[line].CommandName;
                args = _commandCache[line].CommandArgs;
            }
            else
            {
                var split = ParseArguments(line, ' ', '"').ToArray();
                cmdName = split[0];
                args = split.Length > 1 ? split.Skip(1).ToArray() : null;

                _commandCache.Add(line, new CmdCache()
                {
                    CommandName = cmdName,
                    CommandArgs = args
                });
            }

            if(!_entries.ContainsKey(cmdName))
            {
                return;
            }

            foreach (var entry in _entries[cmdName])
            {
                if (IsLocked(entry.Flags) && !bypassLock)
                {
                    continue;
                }

                if (!entry.Flags.HasFlag(ConVarFlags.Silent))
                {
                    WriteLine("> " + line);
                }

                entry.Execute(args);
            }
        }

        private static bool IsLocked(ConVarFlags flags)
        {
            return (_lockedFlags & flags) != ConVarFlags.None;
        }

        public static IEnumerable<string> ParseArguments(string line, char delimiter, char textQualifier)
        {
            if (line == null)
                yield break;

            else
            {
                char prevChar, nextChar, currentChar;
                var inString = false;

                var token = new StringBuilder();

                for (int i = 0; i < line.Length; i++)
                {
                    currentChar = line[i];
                    if (i > 0)
                        prevChar = line[i - 1];
                    else
                        prevChar = '\0';
                    if (i + 1 < line.Length)
                        nextChar = line[i + 1];
                    else
                        nextChar = '\0';
                    if (currentChar == textQualifier && (prevChar == '\0' || prevChar == delimiter) && !inString)
                    {
                        inString = true;
                        continue;
                    }
                    if (currentChar == textQualifier && (nextChar == '\0' || nextChar == delimiter) && inString)
                    {
                        inString = false;
                        continue;
                    }
                    if (currentChar == delimiter && !inString)
                    {
                        yield return token.ToString();
                        token = token.Remove(0, token.Length);
                        continue;
                    }
                    token = token.Append(currentChar);
                }
                yield return token.ToString();
            }
        }

        public static void WriteLine(string line)
        {
            try {
                OnMessageLogged?.Invoke(line);
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
                UnityEngine.Debug.LogError(e.StackTrace);
            }
        }

        public static List<string> Split(string stringToSplit, params char[] delimiters)
        {
            List<string> results = new List<string>();

            bool inQuote = false;
            StringBuilder currentToken = new StringBuilder();
            for (int index = 0; index < stringToSplit.Length; ++index)
            {
                char currentCharacter = stringToSplit[index];
                if (currentCharacter == '"')
                {
                    // When we see a ", we need to decide whether we are
                    // at the start or send of a quoted section...
                    inQuote = !inQuote;
                }
                else if (delimiters.Contains(currentCharacter) && inQuote == false)
                {
                    // We've come to the end of a token, so we find the token,
                    // trim it and add it to the collection of results...
                    string result = currentToken.ToString().Trim();
                    if (result != "") results.Add(result);

                    // We start a new token...
                    currentToken = new StringBuilder();
                }
                else
                {
                    // We've got a 'normal' character, so we add it to
                    // the curent token...
                    currentToken.Append(currentCharacter);
                }
            }

            // We've come to the end of the string, so we add the last token...
            string lastResult = currentToken.ToString().Trim();
            if (lastResult != "") results.Add(lastResult);

            return results;
        }

        public static Type GetVariableType(string entryName)
        {
            var entry = FindEntry(entryName);
            if(entry is ITypeVariable typeVar)
            {
                return typeVar.MyType;
            }
            return null;
        }

        public static string GetDescription(string entryName)
        {
            var entry = FindEntry(entryName);
            return entry != null ? entry.Description : string.Empty;
        }

        private static DevConsoleEntry FindEntry(string name)
        {
            if(!_entries.ContainsKey(name) || _entries[name].Count == 0)
            {
                return null;
            }
            return _entries[name][0];
        }

    }
}

