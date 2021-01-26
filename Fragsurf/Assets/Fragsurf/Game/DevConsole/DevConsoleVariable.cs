using System;
using System.Globalization;
using System.ComponentModel;

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
            SetValue(TryParse<T>(args[0]));
        }

        /// https://social.msdn.microsoft.com/Forums/en-US/d3a139b0-9c14-400d-94f9-440b64a0122a/convert-or-tryparse-from-string-to-t-generic-possible-work-around?forum=csharplanguage
        public static TType TryParse<TType>(string inValue)
        {
            if(inValue.IndexOf(',') != -1)
            {
                inValue = inValue.Replace(',', '.');
            }

            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(TType));
                return (TType)converter.ConvertFromString(null, CultureInfo.InvariantCulture, inValue);
            }
            catch
            {
                return default;
            }
        }

    }
}

