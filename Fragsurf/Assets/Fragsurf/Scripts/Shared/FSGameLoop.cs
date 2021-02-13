using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Shared.LagComp;
using Fragsurf.Shared.Player;
using Fragsurf.Shared.Entity;
using System.Reflection;
using System.Linq;

namespace Fragsurf.Shared
{
    public abstract class FSGameLoop : TimeSteppedBehaviour
    {
        public bool IsLocalServer;
        public int ClientIndex = -1;

        public Action PostExecConfig;

        public abstract bool IsHost { get; }
        public abstract INetworkInterface Network { get; }
        protected abstract void RegisterComponents();
        protected abstract void Initialize();

        public LagCompensator LagCompensator => Get<LagCompensator>(true);
        public GamemodeLoader GamemodeLoader => Get<GamemodeLoader>(true);
        public PhysicsManager Physics => Get<PhysicsManager>(true);
        public SharedUserPlugins UserPlugins => Get<SharedUserPlugins>(true);
        public GameLoader GameLoader => Get<GameLoader>(true);
        public TextChat TextChat => Get<TextChat>(true);
        public EntityManager EntityManager => Get<EntityManager>(true);
        public GameMovement GameMovement => Get<GameMovement>(true);
        public PlayerManager PlayerManager => Get<PlayerManager>(true);
        public FileTransfer FileTransfer => Get<FileTransfer>(true);
        public SoundEmitter SoundEmitter => GetFSComponent<SoundEmitter>(true);
        public GameObjectPool Pool => GetFSComponent<GameObjectPool>(true);

        public int ScopeLayer => IsHost ? Layers.Host : Layers.Client;
        public GameObject ObjectContainer { get; private set; }

        public ConfigDirectory DefaultConfig { get; } = new ConfigDirectory();
        public List<FSComponent> FSComponents { get; } = new List<FSComponent>();
        public bool Initialized { get; private set; }
        public bool Live => GameLoader.State == GameLoaderState.Playing && !_destroyed;

        protected override void Awake()
        {
            base.Awake();

            _gameInstances.Add(this);

            ObjectContainer = new GameObject("[Container]");
            ObjectContainer.transform.SetParent(transform, true);

            Application.runInBackground = true;

            DevConsole.RegisterObject(this);

            FileSystem.Init();

            // core components
            // order may be important (yikes)
            AddFSComponent<ConsoleLog>();
            AddFSComponent<GameLoader>();
            AddFSComponent<GameMovement>();
            AddFSComponent<PlayerManager>();
            AddFSComponent<EntityManager>();
            AddFSComponent<PhysicsManager>();
            AddFSComponent<GamemodeLoader>();
            AddFSComponent<FrameInterpolator>();
            AddFSComponent<VoiceChat>();
            AddFSComponent<TextChat>();
            AddFSComponent<SettingReplicator>();
            AddFSComponent<FileTransfer>();
            AddFSComponent<SharedUserPlugins>();
            AddFSComponent<SoundEmitter>();
            AddFSComponent<GameObjectPool>();
            AddFSComponent<UserCmdHandler>();

            InjectComponents();

            RegisterComponents();

            DefaultConfig.ExecuteFiles(Structure.ConfigsPath, "Game");
            PostExecConfig?.Invoke();

            foreach (var fs in FSComponents)
            {
                fs.OnStart();
            }

            Initialize();

            Initialized = true;
        }

        private bool _destroyed;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _gameInstances.Remove(this);

            if (_destroyed)
            {
                return;
            }

            _destroyed = true;

            UnityEngine.Debug.Log("Destroying " + (IsHost ? "Host" : "Client"));

            // unhook then destroy, to make sure we don't lose references
            foreach (var fsc in FSComponents)
            {
                fsc.Unhook();
            }

            for (int i = FSComponents.Count - 1; i >= 0; i--)
            {
                try
                {
                    FSComponents[i].Destroy();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"Failed to destroy {FSComponents[i].GetType().Name}: {e.Message}");
                }
            }

            DevConsole.RemoveAll(this);
        }

        protected override void OnTick()
        {
            for (int i = 0; i < FSComponents.Count; i++)
            {
                if (FSComponents[i].Started
                    && FSComponents[i].enabled)
                {
                    FSComponents[i].Tick();
                }
            }
        }

        protected override void OnFrame()
        {
            for (int i = 0; i < FSComponents.Count; i++)
            {
                if (FSComponents[i].Started
                    && FSComponents[i].enabled)
                {
                    FSComponents[i].OnUpdate();
                }
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < FSComponents.Count; i++)
            {
                if (FSComponents[i].Started
                    && FSComponents[i].enabled)
                {
                    FSComponents[i].OnLateUpdate(); 
                }
            }
        }

        public void Destroy()
        {
            GameObject.DestroyImmediate(gameObject);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            for (int i = 0; i < FSComponents.Count; i++)
            {
                if (FSComponents[i].Started)
                {
                    FSComponents[i].DrawGizmos();
                }
            }
        }
#endif

        [ConCommand("file.rebuild", "Rebuilds the file system")]
        private void RebuildFileSystem()
        {
            FileSystem.Build();
        }

        [ConCommand("destruct", "Kills the game process in a dirty way.")]
        private void Destruct()
        {
            if (Application.isEditor)
            {
                return;
            }
            Process.GetCurrentProcess().Kill();
        }

        [ConCommand("quit", "Exits the game")]
        public static void Quit()
        {
            var cl = GetGameInstance(false);
            if (cl)
            {
                cl.Destroy();
            }

            if (!Application.isEditor)
            {
                Application.Quit();
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        [ConCommand("gc", "Runs the garbage collector")]
        public void CollectGarbage()
        {
            GC.Collect(2, GCCollectionMode.Forced);
        }

        public void AddFSComponent(FSComponent fsc, bool autoReady = true)
        {
            fsc.Initialize(this);
            FSComponents.Add(fsc);

            if (Initialized && autoReady)
            {
                fsc.OnStart();
            }
        }

        public FSComponent AddFSComponent(Type type, bool autoReady = true)
        {
            var result = gameObject.AddComponent(type) as FSComponent;

            AddFSComponent(result, autoReady);

            return result;
        }

        public T AddFSComponent<T>(bool autoReady = true)
            where T : FSComponent, new()
        {
            if(gameObject.GetComponent<T>() != null)
            {
                UnityEngine.Debug.LogError("Adding same component twice: " + typeof(T));
                return null;
            }

            T result = gameObject.AddComponent<T>();
            AddFSComponent(result, autoReady);

            return result;
        }

        private Dictionary<Type, object> _getCache = new Dictionary<Type, object>();
        public T GetFSComponent<T>(bool allowCache = false)
            where T : FSComponent
        {
            var type = typeof(T);
            if(_getCache.ContainsKey(type))
            {
                return _getCache[type] as T;
            }
            for (int i = FSComponents.Count - 1; i >= 0; i--)
            {
                if (FSComponents[i] is T t1)
                {
                    if(allowCache)
                    {
                        _getCache[type] = FSComponents[i];
                    }
                    return t1;
                }
            }
            return null;
        }

        public T Get<T>(bool allowCache = false) 
            where T : FSComponent
        {
            return GetFSComponent<T>(allowCache);
        }

        public void RemoveFSComponent(FSComponent fsc)
        {
            var type = fsc.GetType();
            if (_getCache.ContainsKey(type))
            {
                _getCache.Remove(type);
            }
            FSComponents.Remove(fsc);
        }

        public GameObject NewGameObject()
        {
            var result = new GameObject();
            result.transform.SetParent(ObjectContainer.transform, true);
            return result;
        }

        public new T Instantiate<T>(T original)
            where T : UnityEngine.Object
        {
            var result = GameObject.Instantiate<T>(original);
            if(result is GameObject obj)
            {
                obj.transform.SetParent(ObjectContainer.transform, true);
            }
            return result;
        }

        private void InjectComponents()
        {
            var realm = InjectRealm.Shared | (IsHost ? InjectRealm.Server : InjectRealm.Client);
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes().Where(x => x.IsSubclassOf(typeof(FSComponent))))
                {
                    var attrib = type.GetCustomAttribute<InjectAttribute>();
                    if (attrib == null
                        || !realm.HasFlag(attrib.Realm)
                        || attrib.GamemodeType != null)
                    {
                        continue;
                    }
                    AddFSComponent(type);
                }
            }
        }

        public static FSGameLoop GetGameInstance(bool host)
        {
            foreach (var inst in _gameInstances)
            {
                if (inst.IsHost == host)
                {
                    return inst;
                }
            }
            return null;
        }

        private static List<FSGameLoop> _gameInstances = new List<FSGameLoop>();

    }
}
