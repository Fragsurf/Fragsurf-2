using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SharpConfig;
using UnityEngine;
using System.Reflection;

namespace Fragsurf.Shared
{
    public abstract class BaseGamemode
    {

        public bool LockVars = true;

        private Configuration _config;
        private FSGameLoop _game;
        private GameObject _resource;
        private List<FSComponent> _addedComponents = new List<FSComponent>();

        public GamemodeData Data { get; private set; }

        public void Load(FSGameLoop game)
        {
            LoadConfig();

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
                var resource = Resources.Load<GameObject>(Data.Name);
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
            return Structure.GamemodeDataPath + "\\" + Data.Name;
        }

        public string GetConfigPath()
        {
            return GetDataDirectory() + "\\" + Data.Name + ".ini";
        }

        protected virtual void InitConfig() { }

        public void WriteConfig()
        {
            var path = GetConfigPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            _config.SaveToFile(path);
        }

        public void LoadConfig()
        {
            var cfgPath = GetConfigPath();

            if (File.Exists(cfgPath))
            {
                _config = Configuration.LoadFromFile(cfgPath);
            }
            else
            {
                _config = new Configuration();
                InitConfig();
                WriteConfig();
            }
        }

        public virtual void ExecuteGameConfig()
        {
            DevConsole.SetVariable("game.tickrate", 100, true, true);
            DevConsole.SetVariable("game.physx", true, true, true);
        }
    }

    public class DefaultGamemode : BaseGamemode
    {
        protected override void _Load(FSGameLoop game)
        {
        }

        protected override void _Unload(FSGameLoop game)
        {
        }
    }
}

