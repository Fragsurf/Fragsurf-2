using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SharpConfig;
using Fragsurf.FSM;
using UnityEngine;
using System.Reflection;

namespace Fragsurf.Shared
{
    public abstract class BaseGamemode
    {
        public abstract string Name { get; }
        public virtual string[] Dependencies { get; } = null;
        public bool LockVars = true;
        public static Func<BaseGamemode> GetDefaultGamemode = () => {return new DefaultGamemode();};

        protected List<FSComponent> _addedComponents = new List<FSComponent>();
        public Configuration Config;

        private FSGameLoop _game;
        private GameObject _resource;

        public void Load(FSGameLoop game)
        {
            if (game.IsHost || Server.GameServer.Instance == null)
            {
                ExecuteGameConfig();
            }

            _game = game;
            Directory.CreateDirectory(GetDataDirectory());
            _Load(game);

            InjectComponents();

            foreach (var c in _addedComponents)
            {
                c.OnStart();
            }

            if (!game.IsHost)
            {
                var resource = Resources.Load<GameObject>(Name);
                if (resource != null)
                {
                    _resource = GameObject.Instantiate(resource, game.ObjectContainer.transform);
                }
            }
        }

        public void Unload(FSGameLoop game)
        {
            _Unload(game);

            if (_resource)
            {
                GameObject.Destroy(_resource);
            }

            // unhook then destroy, to make sure we don't lose references
            foreach (var fs in _addedComponents)
            {
                fs.Unhook();
            }

            foreach (var fs in _addedComponents)
            {
                fs.Destroy();
            }

            _addedComponents.Clear();
        }

        protected abstract void _Load(FSGameLoop game);
        protected abstract void _Unload(FSGameLoop game);

        protected void Register<T>()
            where T : FSComponent, new()
        {
            var instance = _game.AddFSComponent<T>(false);
            instance.Gamemode = this;
            _addedComponents.Add(instance);
        }

        protected void Register(Type t)
        {
            var instance = _game.AddFSComponent(t, false);
            instance.Gamemode = this;
            _addedComponents.Add(instance);
        }

        private void InjectComponents()
        {
            var realm = InjectRealm.Shared | (_game.IsHost ? InjectRealm.Server : InjectRealm.Client);
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes().Where(x => x.IsSubclassOf(typeof(FSComponent))))
                {
                    var attrib = type.GetCustomAttribute<InjectAttribute>();
                    if (attrib == null
                        || !realm.HasFlag(attrib.Realm)
                        || attrib.GamemodeType != GetType())
                    {
                        continue;
                    }
                    Register(type);
                }
            }
        }

        public static T GetInstance<T>(string name)
        {
            var typeToInstantiate = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase) && typeof(T).IsAssignableFrom(t));

            if (typeToInstantiate == null)
            {
                return default;
            }

            try
            {
                return (T)Activator.CreateInstance(typeToInstantiate);
            }
            catch { return default; }
        }

        public string GetDataDirectory()
        {
            return Structure.GamemodeDataPath + "\\" + Name;
        }

        public string GetConfigPath()
        {
            return GetDataDirectory() + "\\" + Name + ".ini";
        }

        protected virtual void InitConfig() { }

        public void WriteConfig()
        {
            var path = GetConfigPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            Config.SaveToFile(path);
        }

        public void LoadConfig()
        {
            var cfgPath = GetConfigPath();

            if (File.Exists(cfgPath))
            {
                Config = Configuration.LoadFromFile(cfgPath);
            }
            else
            {
                Config = new Configuration();
                InitConfig();
                WriteConfig();
            }
        }

        public virtual void ExecuteGameConfig()
        {
            DevConsole.SetVariable("game.tickrate", 100, true, true);
            DevConsole.SetVariable("game.physx", true, true, true);
            DevConsole.SetVariable("mv.autobhop", true, true, true);
            DevConsole.SetVariable("mv.gravity", 20.3199f, true, true);
            DevConsole.SetVariable("mv.aircap", 0.762f, true, true);
            DevConsole.SetVariable("mv.airacceleration", 1500f, true, true);
            DevConsole.SetVariable("mv.acceleration", 6.2f, true, true);
            DevConsole.SetVariable("mv.friction", 4f, true, true);
            DevConsole.SetVariable("mv.stopspeed", 4.2f, true, true);
            DevConsole.SetVariable("mv.jumppower", 7.15f, true, true);
            DevConsole.SetVariable("mv.maxspeed", 6.9f, true, true);
            DevConsole.SetVariable("mv.maxvelocity", 88.9f, true, true);
            DevConsole.SetVariable("mv.stepsize", 0.48f, true, true);
            DevConsole.SetVariable("mv.duckdistance", 0.35f, true, true);
            DevConsole.SetVariable("mv.duckdistance", 0.4f, true, true);
            DevConsole.SetVariable("mv.falldamage", false, true, true);
            DevConsole.SetVariable("mv.waterpreventsfalldamage", true, true, true);
            DevConsole.SetVariable("mv.solidplayers", false, true, true);
            DevConsole.SetVariable("mv.movinguprapidlyfactor", 0.85f, true, true);
            DevConsole.SetVariable("mv.maxclimbspeed", 6.9f, true, true);
            DevConsole.SetVariable("mv.forwardspeed", 10.16f, true, true);
            DevConsole.SetVariable("mv.sidespeed", 10.16f, true, true);
        }
    }

    public class DefaultGamemode : BaseGamemode
    {
        public override string Name => "Default";

        protected override void _Load(FSGameLoop game)
        {
        }

        protected override void _Unload(FSGameLoop game)
        {
        }
    }
}

