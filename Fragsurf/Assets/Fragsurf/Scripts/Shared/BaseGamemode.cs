using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SharpConfig;
using UnityEngine;
using System.Reflection;
using Fragsurf.UI;
using UnityEngine.UI;

namespace Fragsurf.Shared
{
    public abstract class BaseGamemode
    {

        public bool LockVars = true;

        private Configuration _config;
        private FSGameLoop _game;
        private List<FSComponent> _addedComponents = new List<FSComponent>();
        private List<GameObject> _objectsInstantiated = new List<GameObject>();

        public List<UGuiModal> Modals { get; } = new List<UGuiModal>();
        public GamemodeData Data { get; set; }

        public void Load(GamemodeData data, FSGameLoop game)
        {
            Data = data;

            LoadConfig();

            if (game.IsServer || Server.GameServer.Instance == null)
            {
                ExecuteGameConfig();
            }

            _game = game;
            _Load(game);

            InjectComponents();

            foreach (var c in _addedComponents)
            {
                c.OnStart();
            }

            if (!game.IsServer)
            {
                foreach(var obj in Data.InstantiateOnLoad)
                {
                    var clone = GameObject.Instantiate(obj);
                    _objectsInstantiated.Add(clone);
                    Modals.AddRange(clone.GetComponentsInChildren<UGuiModal>(true));
                }
            }
        }

        public void Unload(FSGameLoop game)
        {
            Modals.Clear();
            foreach(var obj in _objectsInstantiated)
            {
                GameObject.Destroy(obj);
            }

            _Unload(game);

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
            var realm = InjectRealm.Shared | (_game.IsServer ? InjectRealm.Server : InjectRealm.Client);
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
            catch(Exception e) { Debug.LogError(e.Message); return default; }
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
            if(_config == null || _config.SectionCount == 0)
            {
                return;
            }
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

