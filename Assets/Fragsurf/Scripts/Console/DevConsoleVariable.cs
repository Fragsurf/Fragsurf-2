using System;
using System.Globalization;
using System.ComponentModel;
using UnityEngine;
using Fragsurf.Utility;
using Fragsurf.Movement;

namespace Fragsurf 
{
    public class DevConsoleVariable<T> : DevConsoleEntry, ITypeVariable
    {

        public DevConsoleVariable(object owner, string name, string description, Func<T> getter, Action<T> setter)
            : base(owner, name, description)
        {
            _getter = getter;
            _setter = setter;
        }

        private Func<T> _getter;
        private Action<T> _setter;
        private T _lastKnownValue;

        Type ITypeVariable.MyType
        {
            get { return typeof(T); }
        }

        public T GetValue()
        {
            T result = default;
            try
            {
                result = _getter.Invoke();
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError("Failed to get " + Name + ": " + e.Message);
            }
            return result;
        }

        protected override void _OnTick()
        {
            if (!Flags.HasFlag(ConVarFlags.Poll))
            {
                return;
            }

            if(Differs())
            {
                DevConsole.RaiseVariableChanged(Name);
            }
        }

        public void SetValue(T value, bool noEvent = false)
        {
            try
            {
                _setter.Invoke(value);
                _lastKnownValue = value;
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError("Failed to set " + Name + ": " + e.Message);
            }

            if(!noEvent && Differs())
            {
                DevConsole.RaiseVariableChanged(Name);
            }
        }

        private bool Differs()
        {
            var curValue = _getter.Invoke();
            var differs = false;

            if (curValue is IEquatable<T> equatable)
            {
                differs = !equatable.Equals(_lastKnownValue);
            }
            else if (curValue != null)
            {
                differs = !curValue.Equals(_lastKnownValue);
            }
            _lastKnownValue = curValue;
            return differs;
        }

        protected override void _OnExecute(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return;
            }
            SetValue(FromString(args[0]));
        }

        public override string ToString()
        {
            if(typeof(T) == typeof(Color))
            {
                var val = (Color)Convert.ChangeType(_getter.Invoke(), typeof(Color));
                return "#" + ColorUtility.ToHtmlStringRGBA(val);
            }
            var result = _getter.Invoke();
            if(result == null)
            {
                return string.Empty;
            }
            return result.ToString();
        }

        public T FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return default;
            }

            try
            {
                if (typeof(T) == typeof(Color))
                {
                    if (input.Length == 8 && input[0] != '#')
                    {
                        input = "#" + input;
                    }
                    ColorUtility.TryParseHtmlString(input, out Color result);
                    return (T)Convert.ChangeType(result, typeof(T));
                }
                return TryParse<T>(input);
            }
            catch(Exception e)
            {
                DevConsole.WriteLine($"`{input}` is an invalid input for {Name}, expected input: {typeof(T).Name}");
#if UNITY_EDITOR
                Debug.LogError(e.Message);
#endif
                return default;
            }
        }

        /// https://social.msdn.microsoft.com/Forums/en-US/d3a139b0-9c14-400d-94f9-440b64a0122a/convert-or-tryparse-from-string-to-t-generic-possible-work-around?forum=csharplanguage
        private static TType TryParse<TType>(string inValue)
        {
            if (typeof(TType).IsNumeric()
                && inValue.IndexOf(',') != -1)
            {
                inValue = inValue.Replace(',', '.');
            }

            var converter = TypeDescriptor.GetConverter(typeof(TType));
            var result = converter.ConvertFromString(null, CultureInfo.InvariantCulture, inValue);
            return (TType)result;
        }

    }

}

