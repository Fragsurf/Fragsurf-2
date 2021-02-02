using System;
using System.Globalization;
using System.ComponentModel;
using UnityEngine;
using Fragsurf.Utility;

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

        public void SetValue(T value, bool noEvent = false)
        {
            try
            {
                _setter.Invoke(value);
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError("Failed to set " + Name + ": " + e.Message);
            }

            if(!noEvent)
            {
                DevConsole.RaiseVariableChanged(Name);
            }
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
                return ColorUtility.ToHtmlStringRGBA(val);
            }
            return _getter.Invoke().ToString();
        }

        public T FromString(string input)
        {
            if(typeof(T) == typeof(Color))
            {
                ColorUtility.TryParseHtmlString(input, out Color result);
                return (T)Convert.ChangeType(result, typeof(T));
            }
            return TryParse<T>(input);
        }

        /// https://social.msdn.microsoft.com/Forums/en-US/d3a139b0-9c14-400d-94f9-440b64a0122a/convert-or-tryparse-from-string-to-t-generic-possible-work-around?forum=csharplanguage
        public static TType TryParse<TType>(string inValue)
        {
            if (typeof(TType).IsNumeric()
                && inValue.IndexOf(',') != -1)
            {
                inValue = inValue.Replace(',', '.');
            }

            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(TType));
                var result = converter.ConvertFromString(null, CultureInfo.InvariantCulture, inValue);
                return (TType)result;
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogError(e.Message);
#endif
                return default;
            }
        }

    }

}

