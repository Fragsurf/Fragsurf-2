using System;
using System.IO;
using XLua;
using System.Collections.Generic;

namespace Fragsurf.Shared.UserPlugins
{
    public class LuaPlugin : BaseUserPlugin
    {

        public LuaPlugin(IUserPluginDescriptor descriptor)
            : base(descriptor)
        {
        }

        private LuaEnv _luaEnv;
        private LuaTable _sandbox;
        private FileSystemWatcher _watcher;
        private bool _refresh;
        private Dictionary<string, List<LuaFunction>> _eventSubscriptions = new Dictionary<string, List<LuaFunction>>(StringComparer.OrdinalIgnoreCase);

        // https://stackoverflow.com/questions/1224708/how-can-i-create-a-secure-lua-sandbox
        private const string _luaSandbox = @"
-- save a pointer to globals that would be unreachable in sandbox
local e=_ENV

-- sample sandbox environment
sandbox_env = {
    ipairs = ipairs,
    next = next,
    pairs = pairs,
    pcall = pcall,
    tonumber = tonumber,
    tostring = tostring,
    type = type,
    unpack = unpack,
    coroutine = { create = coroutine.create, resume = coroutine.resume, 
        running = coroutine.running, status = coroutine.status, 
        wrap = coroutine.wrap },
    string = { byte = string.byte, char = string.char, find = string.find, 
        format = string.format, gmatch = string.gmatch, gsub = string.gsub, 
        len = string.len, lower = string.lower, match = string.match, 
        rep = string.rep, reverse = string.reverse, sub = string.sub, 
        upper = string.upper },
    table = { insert = table.insert, maxn = table.maxn, remove = table.remove, 
        sort = table.sort },
    math = { abs = math.abs, acos = math.acos, asin = math.asin, 
        atan = math.atan, atan2 = math.atan2, ceil = math.ceil, cos = math.cos, 
        cosh = math.cosh, deg = math.deg, exp = math.exp, floor = math.floor, 
        fmod = math.fmod, frexp = math.frexp, huge = math.huge, 
        ldexp = math.ldexp, log = math.log, log10 = math.log10, max = math.max, 
        min = math.min, modf = math.modf, pi = math.pi, pow = math.pow, 
        rad = math.rad, random = math.random, sin = math.sin, sinh = math.sinh, 
        sqrt = math.sqrt, tan = math.tan, tanh = math.tanh },
    os = { clock = os.clock, difftime = os.difftime, time = os.time },
    Vector2 = CS.UnityEngine.Vector2,
    Vector3 = CS.UnityEngine.Vector3,
    Vector4 = CS.UnityEngine.Vector4,
    Quaternion = CS.UnityEngine.Quaternion,
    Fragsurf = CS.Fragsurf,
    Game = Game,
    Logger = Logger,
    Plugin = Plugin,
    DataPath = DataPath,
    print = Logger.Print
}

function run_sandbox(sb_func, ...)
  local sb_orig_env=_ENV
  if (not sb_func) then return nil end
  _ENV=sandbox_env
  local sb_ret={e.pcall(sb_func, ...)}
  _ENV=sb_orig_env
  return e.table.unpack(sb_ret)
end

-- pcall_rc, result_or_err_msg = run_sandbox(sandbox_env, my_func, arg1, arg2)
";

        public override string FileExtension => ".lua";

        public override bool Initialize()
        {
            return true;
        }

        protected override void _Load()
        {
            _luaEnv?.Dispose();
            _watcher?.Dispose();
            _luaEnv = new LuaEnv();
            _luaEnv.Global.Set("Game", Game);
            _luaEnv.Global.Set("Logger", LogSystem);
            _luaEnv.Global.Set("Plugin", this);
            _luaEnv.Global.Set("DataPath", Path.Combine(Descriptor.Directory, "Data"));
            _luaEnv.DoString(_luaSandbox);

            _sandbox = _luaEnv.Global.Get<LuaTable>("sandbox_env");

            var entryScript = File.ReadAllText(EntryFilePath);
            _luaEnv.DoString(entryScript, "chunk", _sandbox);

            if(Convert.ToBoolean(Func("load")))
            {
                _watcher = new FileSystemWatcher();
                _watcher.Path = Path.GetDirectoryName(EntryFilePath);
                _watcher.Filter = Path.GetFileName(EntryFilePath);
                _watcher.EnableRaisingEvents = true;
                _watcher.Changed += (sender, e) =>
                {
                    _refresh = true;
                };
            }

            Loaded = true;
        }

        protected override void _Unload()
        {
            Action("_unload");
            _eventSubscriptions.Clear();
            _watcher?.Dispose();
            _luaEnv?.Dispose();
            _luaEnv = null;
            _watcher = null;
            Loaded = false;
        }

        public override void Update()
        {
            if(_refresh)
            {
                DevConsole.ExecuteLine("plugins.reload");
                _refresh = false;
            }
        }

        public void AddEvent(string eventName, LuaFunction function)
        {
            if(!_eventSubscriptions.ContainsKey(eventName))
            {
                _eventSubscriptions[eventName] = new List<LuaFunction>();
            }
            _eventSubscriptions[eventName].Add(function);
        }

        public void RemoveEvent(string eventName, LuaFunction function)
        {
            if(_eventSubscriptions.ContainsKey(eventName))
            {
                _eventSubscriptions[eventName].Remove(function);
            }
        }

        public override void RaiseEvent(string eventName, params object[] parameters)
        {
            if(_eventSubscriptions.ContainsKey(eventName))
            {
                foreach(var func in _eventSubscriptions[eventName])
                {
                    try
                    {
                        func.Call(parameters);
                    }
                    catch(Exception e)
                    {
                        LogSystem?.PrintError(e.Message);
                    }
                }
            }
        }

        public override void Action(string path, params object[] parameters)
        {
            var func = _sandbox.Get<LuaFunction>(path);
            if(func != null)
            {
                try
                {
                    func.Call(parameters);
                }
                catch(LuaException e)
                {
                    LogSystem.PrintError(e.ToString());
                }
            }
        }

        public override object Func(string path, params object[] parameters)
        {
            var func = _sandbox.Get<LuaFunction>(path);
            if (func != null)
            {
                try
                {
                    var result = func.Call(parameters);
                    if(result != null && result.Length > 0)
                    {
                        return result[0];
                    }
                    return null;
                }
                catch (LuaException e)
                {
                    LogSystem.PrintError("Failed to call " + path + ": " +e.ToString());
                }
            }
            return null;
        }

        public override object GetValue(string path)
        {
            return _luaEnv.Global.Get<object>(path);
        }

    }

}

