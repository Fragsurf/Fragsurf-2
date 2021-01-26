using System;
using System.Collections.Generic;
using System.Diagnostics;
using Systems.SelectorSystem;
using Src.Systems;
using UIForia.Animation;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Routing;
using UIForia.Sound;
using UIForia.Systems;
using UIForia.Systems.Input;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace UIForia {

    public abstract class Application {

        private static SizeInt UIApplicationSize;

        public static float dpiScaleFactor = Mathf.Max(1, Screen.dpi / 96f);

        public static readonly float originalDpiScaleFactor = Mathf.Max(1, Screen.dpi / 96f);

        public float DPIScaleFactor {
            get => dpiScaleFactor;
            set => dpiScaleFactor = value;
        }

        public static SizeInt UiApplicationSize => UIApplicationSize;

        public static List<Application> Applications = new List<Application>();

        internal Stopwatch layoutTimer = new Stopwatch();
        internal Stopwatch renderTimer = new Stopwatch();
        internal Stopwatch bindingTimer = new Stopwatch();
        internal Stopwatch loopTimer = new Stopwatch();

        public readonly string id;
        internal SelectorSystem selectorSystem;
        internal IStyleSystem styleSystem;
        internal ILayoutSystem layoutSystem;
        internal IRenderSystem renderSystem;
        internal InputSystem inputSystem;
        internal RoutingSystem routingSystem;
        internal AnimationSystem animationSystem;
        internal UISoundSystem soundSystem;
        internal LinqBindingSystem linqBindingSystem;

        private int elementIdGenerator;

        protected ResourceManager resourceManager;

        protected List<ISystem> systems;

        public event Action<UIElement> onElementRegistered;
        public event Action<UIElement> onElementDestroyed;
        public event Action<UIElement> onElementEnabled;
        public event Action<UIView[]> onViewsSorted;
        public event Action<UIView> onViewRemoved;
        public event Action onRefresh;

        public MaterialDatabase materialDatabase;

        internal CompiledTemplateData templateData;

        internal int frameId;
        protected internal List<UIView> views;

        internal static Dictionary<string, Type> s_CustomPainters;

        private UITaskSystem m_BeforeUpdateTaskSystem;
        private UITaskSystem m_AfterUpdateTaskSystem;

        public static UIForiaSettings Settings;

        static Application() {
            s_CustomPainters = new Dictionary<string, Type>();
        }

        public UIForiaSettings settings => Settings;

        private int NextElementId => elementIdGenerator++;

        public TemplateMetaData[] zz_Internal_TemplateMetaData => templateData.templateMetaData;

        private TemplateSettings templateSettings;
        private bool isPreCompiled;

        protected Application(bool isPreCompiled, TemplateSettings templateSettings, ResourceManager resourceManager, Action<UIElement> onElementRegistered) {
            this.isPreCompiled = isPreCompiled;
            this.templateSettings = templateSettings;
            this.onElementRegistered = onElementRegistered;
            this.id = templateSettings.applicationName;
            this.resourceManager = resourceManager ?? new ResourceManager();
            Settings = Settings ? Settings : Resources.Load<UIForiaSettings>("UIForiaSettings");
            if (Settings == null) {
                throw new Exception("UIForiaSettings are missing. Use the UIForia/Create UIForia Settings to create it");
            }

            Applications.Add(this);
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += OnEditorReload;
#endif
        }

#if UNITY_EDITOR
        private void OnEditorReload() {
            templateData?.Destroy();
            templateData = null;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= OnEditorReload;
        }
#endif

        protected virtual void CreateSystems() {
            styleSystem = new StyleSystem();
            layoutSystem = new AwesomeLayoutSystem(this);
            inputSystem = new GameInputSystem(layoutSystem, new KeyboardInputManager());
            renderSystem = new VertigoRenderSystem(Camera ?? Camera.current, this);
            routingSystem = new RoutingSystem();
            animationSystem = new AnimationSystem();
            linqBindingSystem = new LinqBindingSystem();
            soundSystem = new UISoundSystem();
        }

        internal void Initialize() {
            systems = new List<ISystem>();
            views = new List<UIView>();

            CreateSystems();

            systems.Add(styleSystem);
            systems.Add(linqBindingSystem);
            systems.Add(routingSystem);
            systems.Add(inputSystem);
            systems.Add(animationSystem);
            systems.Add(layoutSystem);
            systems.Add(renderSystem);

            m_BeforeUpdateTaskSystem = new UITaskSystem();
            m_AfterUpdateTaskSystem = new UITaskSystem();

            UIView view = null;

            // Stopwatch timer = Stopwatch.StartNew();

            if (isPreCompiled) {
                templateData = TemplateLoader.LoadPrecompiledTemplates(templateSettings);
            }
            else {
                templateData = TemplateLoader.LoadRuntimeTemplates(templateSettings.rootType, templateSettings);
            }

            materialDatabase = templateData.materialDatabase;

            UIElement rootElement = templateData.templates[0].Invoke(null, new TemplateScope(this));

            view = new UIView(this, "Default", rootElement, Matrix4x4.identity, new Size(Width, Height));

            views.Add(view);

            for (int i = 0; i < systems.Count; i++) {
                systems[i].OnViewAdded(view);
            }

            //timer.Stop();
            //Debug.Log("Initialized UIForia application in " + timer.Elapsed.TotalSeconds.ToString("F2") + " seconds");
        }

        public UIView CreateView<T>(string name, Size size, in Matrix4x4 matrix) where T : UIElement {
            if (templateData.TryGetTemplate<T>(out DynamicTemplate dynamicTemplate)) {
                UIElement element = templateData.templates[dynamicTemplate.templateId].Invoke(null, new TemplateScope(this));

                UIView view = new UIView(this, name, element, matrix, size);

                view.Depth = views.Count;
                views.Add(view);

                for (int i = 0; i < systems.Count; i++) {
                    systems[i].OnViewAdded(view);
                }

                return view;
            }

            throw new TemplateNotFoundException($"Unable to find a template for {typeof(T)}. This is probably because you are trying to load this template dynamically and did include the type in the {nameof(TemplateSettings.dynamicallyCreatedTypes)} list.");
        }

        public UIView CreateView<T>(string name, Size size) where T : UIElement {
            return CreateView<T>(name, size, Matrix4x4.identity);
        }

        internal static void ProcessClassAttributes(Type type, Attribute[] attrs) {
            for (var i = 0; i < attrs.Length; i++) {
                Attribute attr = attrs[i];
                if (attr is CustomPainterAttribute paintAttr) {
                    if (type.GetConstructor(Type.EmptyTypes) == null || !typeof(RenderBox).IsAssignableFrom(type)) {
                        throw new Exception($"Classes marked with [{nameof(CustomPainterAttribute)}] must provide a parameterless constructor" +
                                            $" and the class must extend {nameof(RenderBox)}. Ensure that {type.FullName} conforms to these rules");
                    }

                    if (s_CustomPainters.ContainsKey(paintAttr.name)) {
                        throw new Exception(
                            $"Failed to register a custom painter with the name {paintAttr.name} from type {type.FullName} because it was already registered.");
                    }

                    s_CustomPainters.Add(paintAttr.name, type);
                }
            }
        }

        public IStyleSystem StyleSystem => styleSystem;
        public IRenderSystem RenderSystem => renderSystem;
        public ILayoutSystem LayoutSystem => layoutSystem;
        public InputSystem InputSystem => inputSystem;
        public RoutingSystem RoutingSystem => routingSystem;
        public UISoundSystem SoundSystem => soundSystem;

        public Camera Camera { get; private set; }

        public ResourceManager ResourceManager => resourceManager;

        public void SetScreenSize(int width, int height) {
            UIApplicationSize.width = width;
            UIApplicationSize.height = height;
        }

        public float Width => UiApplicationSize.width / dpiScaleFactor;

        public float Height => UiApplicationSize.height / dpiScaleFactor;

        public void SetCamera(Camera camera) {
            Rect rect = camera.pixelRect;
            UIApplicationSize.height = (int) rect.height;
            UIApplicationSize.width = (int) rect.width;

            if (views.Count > 0) {
                views[0].Viewport = new Rect(0, 0, Width, Height);
            }

            Camera = camera;
            RenderSystem.SetCamera(camera);
        }

        public UIView RemoveView(UIView view) {
            if (!views.Remove(view)) return null;

            for (int i = 0; i < systems.Count; i++) {
                systems[i].OnViewRemoved(view);
            }

            DestroyElement(view.dummyRoot);
            onViewRemoved?.Invoke(view);
            return view;
        }

        public void Refresh() {
            if (isPreCompiled) {
                Debug.Log("Cannot refresh application because it is using precompiled templates");
                return;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (ISystem system in systems) {
                system.OnDestroy();
            }

            for (int i = views.Count - 1; i >= 0; i--) {
                views[i].Destroy();
            }

            resourceManager.Reset();

            materialDatabase.Destroy();
            templateData.Destroy();

            m_AfterUpdateTaskSystem.OnDestroy();
            m_BeforeUpdateTaskSystem.OnDestroy();

            elementIdGenerator = 0;

            Initialize();

            onRefresh?.Invoke();

            stopwatch.Stop();
            Debug.Log("Refreshed " + id + " in " + stopwatch.Elapsed.TotalSeconds.ToString("F2") + " seconds");
        }

        public void Destroy() {
            Applications.Remove(this);
            templateData?.Destroy();

            foreach (ISystem system in systems) {
                system.OnDestroy();
            }

            for (int i = views.Count - 1; i >= 0; i--) {
                views[i].Destroy();
            }

            onElementEnabled = null;
            onElementDestroyed = null;
            onElementRegistered = null;
        }

        public static void DestroyElement(UIElement element) {
            element.View.application.DoDestroyElement(element);
        }

        internal void DoDestroyElement(UIElement element, bool removingChildren = false) {
            // do nothing if already destroyed
            if ((element.flags & UIElementFlags.Alive) == 0) {
                return;
            }

            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            LightList<UIElement> toInternalDestroy = LightList<UIElement>.Get();

            stack.Push(element);

            while (stack.Count > 0) {
                UIElement current = stack.array[--stack.size];

                if ((current.flags & UIElementFlags.Alive) == 0) {
                    continue;
                }

                current.flags &= ~(UIElementFlags.Alive);
                current.enableStateChangedFrameId = frameId;
                current.OnDestroy();
                toInternalDestroy.Add(current);

                UIElement[] children = current.children.array;
                int childCount = current.children.size;

                if (stack.size + childCount >= stack.array.Length) {
                    Array.Resize(ref stack.array, stack.size + childCount + 16);
                }

                for (int i = childCount - 1; i >= 0; i--) {
                    // inline stack push
                    stack.array[stack.size++] = children[i];
                }
            }

            if (element.parent != null && !removingChildren) {
                element.parent.children.Remove(element);
                for (int i = 0; i < element.parent.children.Count; i++) {
                    element.parent.children[i].siblingIndex = i;
                }
            }

            for (int i = 0; i < systems.Count; i++) {
                systems[i].OnElementDestroyed(element);
            }

            for (int i = 0; i < toInternalDestroy.size; i++) {
                toInternalDestroy[i].InternalDestroy();
            }

            LightList<UIElement>.Release(ref toInternalDestroy);
            LightStack<UIElement>.Release(ref stack);

            onElementDestroyed?.Invoke(element);
        }

        private LightList<UIElement> activeBuffer = new LightList<UIElement>(32);
        private LightList<UIElement> queuedBuffer = new LightList<UIElement>(32);

        public void Update() {
            // input queries against last frame layout
            inputSystem.Read();
            loopTimer.Reset();
            loopTimer.Start();

            Rect rect;
            if (!ReferenceEquals(Camera, null)) {
                rect = Camera.pixelRect;
            }
            else {
                rect = new Rect(0, 0, 1920, 1080);
            }

            UIApplicationSize.height = (int) rect.height;
            UIApplicationSize.width = (int) rect.width;

            for (int i = 0; i < views.Count; i++) {
                views[i].Viewport = new Rect(0, 0, Width, Height);
            }

            inputSystem.OnUpdate();
            m_BeforeUpdateTaskSystem.OnUpdate();

            activeBuffer.Clear();

            for (int i = 0; i < views.Count; i++) {
                activeBuffer.Add(views[i].RootElement);
            }

            linqBindingSystem.BeginFrame();
            bindingTimer.Reset();
            bindingTimer.Start();

            // right now, out of order elements wont get bindings until next frame. this miiight be ok but probably will cause weirdness. likely want this to change
            for (int i = 0; i < views.Count; i++) {
                linqBindingSystem.NewUpdateFn(views[i].RootElement);
            }

            // bool loop = true;

            // while (loop) {
            //     
            //     for (int i = 0; i < activeBuffer.size; i++) {
            //         linqBindingSystem.NewUpdateFn(views[i].RootElement);
            //     }
            //
            //     if (queuedBuffer.size == 0) {
            //         break;
            //     }
            //
            //     LightList<UIElement> tmp = activeBuffer;
            //     activeBuffer = queuedBuffer;
            //     queuedBuffer = tmp;
            //     activeBuffer.Clear();
            //     // sort queued buffer by depth?
            // }

            bindingTimer.Stop();

            // style & attribute bindings (no user code here)
            // linqBindingSystem.UpdateStylesAndAttributes();

            animationSystem.OnUpdate();

            routingSystem.OnUpdate();

            styleSystem.OnUpdate(); // buffer changes here

            // todo -- read changed data into layout/render thread
            layoutTimer.Reset();
            layoutTimer.Start();
            layoutSystem.OnUpdate();
            layoutTimer.Stop();

            renderTimer.Reset();
            renderTimer.Start();
            renderSystem.OnUpdate();
            renderTimer.Stop();

            m_AfterUpdateTaskSystem.OnUpdate();

            frameId++;
            loopTimer.Stop();
        }

        /// <summary>
        /// Note: you don't need to remove tasks from the system. Any canceled or otherwise completed task gets removed
        /// from the system automatically.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public UITask RegisterBeforeUpdateTask(UITask task) {
            return m_BeforeUpdateTaskSystem.AddTask(task);
        }

        /// <summary>
        /// Note: you don't need to remove tasks from the system. Any canceled or otherwise completed task gets removed
        /// from the system automatically.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public UITask RegisterAfterUpdateTask(UITask task) {
            return m_AfterUpdateTaskSystem.AddTask(task);
        }

        internal void DoEnableElement(UIElement element, bool queued) {
            element.flags |= UIElementFlags.Enabled;

            // if element is not enabled (ie has a disabled ancestor or is not alive), no-op 
            if ((element.flags & UIElementFlags.SelfAndAncestorEnabled) != UIElementFlags.SelfAndAncestorEnabled) {
                return;
            }

            if (queued) {
                queuedBuffer.Add(element);
            }

            StructStack<ElemRef> stack = StructStack<ElemRef>.Get();
            // if element is now enabled we need to walk it's children
            // and set enabled ancestor flags until we find a self-disabled child
            stack.array[stack.size++].element = element;

            // stack operations in the following code are inlined since this is a very hot path
            while (stack.size > 0) {
                // inline stack pop
                UIElement child = stack.array[--stack.size].element;

                child.flags |= UIElementFlags.AncestorEnabled;

                // if the element is itself disabled or destroyed, keep going
                if ((child.flags & UIElementFlags.Enabled) == 0) {
                    continue;
                }

                child.style.UpdateInheritedStyles(); // todo -- move this
                try {
                    child.OnEnable();
                }
                catch (Exception e) {
                    Debug.Log(e);
                }

                // We need to run all runCommands now otherwise animations in [normal] style groups won't run after enabling.
                child.style.RunCommands();

                //if ((child.flags & UIElementFlags.HasBeenEnabled) == 0) {
                // todo -- run once bindings if present
                //    child.View.ElementCreated(child);
                //}

                // register the flag set even if we get disabled via OnEnable, we just want to track that OnEnable was called at least once
                child.flags |= UIElementFlags.HasBeenEnabled;

                // only continue if calling enable didn't re-disable the element
                if ((child.flags & UIElementFlags.SelfAndAncestorEnabled) == UIElementFlags.SelfAndAncestorEnabled) {
                    child.enableStateChangedFrameId = frameId;
                    UIElement[] children = child.children.array;
                    int childCount = child.children.size;
                    if (stack.size + childCount >= stack.array.Length) {
                        Array.Resize(ref stack.array, stack.size + childCount + 16);
                    }

                    for (int i = childCount - 1; i >= 0; i--) {
                        // inline stack push
                        stack.array[stack.size++].element = children[i];
                    }
                }
            }

            for (int i = 0; i < systems.Count; i++) {
                systems[i].OnElementEnabled(element);
            }

            StructStack<ElemRef>.Release(ref stack);

            onElementEnabled?.Invoke(element);
        }

        public void DoDisableElement(UIElement element) {
            // if element is already disabled or destroyed, no op
            if ((element.flags & UIElementFlags.Alive) == 0) {
                return;
            }

            bool wasDisabled = element.isDisabled;
            element.flags &= ~(UIElementFlags.Enabled);

            if (wasDisabled) {
                return;
            }

            // if element is now enabled we need to walk it's children
            // and set enabled ancestor flags until we find a self-disabled child
            StructStack<ElemRef> stack = StructStack<ElemRef>.Get();
            stack.array[stack.size++].element = element;

            // stack operations in the following code are inlined since this is a very hot path
            while (stack.size > 0) {
                // inline stack pop
                UIElement child = stack.array[--stack.size].element;

                child.flags &= ~(UIElementFlags.AncestorEnabled);

                // if destroyed the whole subtree is also destroyed, do nothing.
                // if already disabled the whole subtree is also disabled, do nothing.

                if ((child.flags & (UIElementFlags.Alive | UIElementFlags.Enabled)) == 0) {
                    continue;
                }

                // todo -- profile not calling disable when it's not needed
                // if (child.flags & UIElementFlags.RequiresEnableCall) {
                try {
                    child.OnDisable();
                }
                catch (Exception e) {
                    Debug.Log(e);
                }
                // }

                // todo -- maybe do this on enable instead
                if (child.style.currentState != StyleState.Normal) {
                    // todo -- maybe just have a clear states method
                    child.style.ExitState(StyleState.Hover);
                    child.style.ExitState(StyleState.Active);
                    child.style.ExitState(StyleState.Focused);
                }

                child.enableStateChangedFrameId = frameId;

                // if child is still disabled after OnDisable, traverse it's children
                if (!child.isEnabled) {
                    UIElement[] children = child.children.array;
                    int childCount = child.children.size;
                    if (stack.size + childCount >= stack.array.Length) {
                        Array.Resize(ref stack.array, stack.size + childCount + 16);
                    }

                    for (int i = childCount - 1; i >= 0; i--) {
                        // inline stack push
                        stack.array[stack.size++].element = children[i];
                    }
                }
            }

            // avoid checking in the loop if this is the originally disabled element
            if (element.parent.isEnabled) {
                element.flags |= UIElementFlags.AncestorEnabled;
            }

            StructStack<ElemRef>.Release(ref stack);

            for (int i = 0; i < systems.Count; i++) {
                systems[i].OnElementDisabled(element);
            }
        }

        public UIElement GetElement(int elementId) {
            LightStack<UIElement> stack = LightStack<UIElement>.Get();

            for (int i = 0; i < views.Count; i++) {
                stack.Push(views[i].RootElement);

                while (stack.size > 0) {
                    UIElement element = stack.PopUnchecked();

                    if (element.id == elementId) {
                        LightStack<UIElement>.Release(ref stack);
                        return element;
                    }

                    if (element.children == null) continue;

                    for (int j = 0; j < element.children.size; j++) {
                        stack.Push(element.children.array[j]);
                    }
                }
            }

            LightStack<UIElement>.Release(ref stack);
            return null;
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) {
            for (int i = 0; i < systems.Count; i++) {
                systems[i].OnAttributeSet(element, attributeName, currentValue, previousValue);
            }
        }

        public static void RefreshAll() {

            for (int i = 0; i < Applications.Count; i++) {
                Applications[i].Refresh();
            }
        }

        public UIView GetView(int i) {
            if (i < 0 || i >= views.Count) return null;
            return views[i];
        }

        public UIView GetView(string name) {
            for (int i = 0; i < views.Count; i++) {
                UIView v = views[i];
                if (v.name == name) {
                    return v;
                }
            }

            return null;
        }

        public static Application Find(string appId) {
            return Applications.Find((app) => app.id == appId);
        }

        public static bool HasCustomPainter(string name) {
            return s_CustomPainters.ContainsKey(name);
        }

        public UIView[] GetViews() {
            return views.ToArray();
        }

        private void InitializeElement(UIElement child) {
            bool parentEnabled = child.parent.isEnabled;

            UIView view = child.parent.View;

            StructStack<ElemRef> elemRefStack = StructStack<ElemRef>.Get();
            elemRefStack.Push(new ElemRef() {element = child});

            while (elemRefStack.size > 0) {
                UIElement current = elemRefStack.array[--elemRefStack.size].element;

                current.hierarchyDepth = current.parent.hierarchyDepth + 1;

                current.View = view;

                if (current.parent.isEnabled) {
                    current.flags |= UIElementFlags.AncestorEnabled;
                }
                else {
                    current.flags &= ~UIElementFlags.AncestorEnabled;
                }

                if ((current.flags & UIElementFlags.Created) == 0) {
                    current.flags |= UIElementFlags.Created;
                    for (int i = 0; i < systems.Count; i++) {
                        systems[i].OnElementCreated(current);
                    }

                    try {
                        
                        onElementRegistered?.Invoke(current);
                        current.OnCreate();
                    }
                    catch (Exception e) {
                        Debug.LogWarning(e);
                    }
                }

                UIElement[] children = current.children.array;
                int childCount = current.children.size;
                // reverse this? inline stack push
                for (int i = 0; i < childCount; i++) {
                    children[i].siblingIndex = i;
                    elemRefStack.Push(new ElemRef() {element = children[i]});
                }
            }

            if (parentEnabled && child.isEnabled) {
                child.enableStateChangedFrameId = frameId;
                child.flags &= ~UIElementFlags.Enabled;
                DoEnableElement(child, false);
            }

            StructStack<ElemRef>.Release(ref elemRefStack);
        }

        internal void InsertChild(UIElement parent, UIElement child, uint index) {
            child.parent = parent;
            parent.children.Insert((int) index, child);

            bool parentEnabled = parent.isEnabled;

            UIView view = parent.View;

            StructStack<ElemRef> elemRefStack = StructStack<ElemRef>.Get();
            elemRefStack.Push(new ElemRef() {element = child});

            while (elemRefStack.size > 0) {
                UIElement current = elemRefStack.Pop().element;

                current.hierarchyDepth = current.parent.hierarchyDepth + 1;

                current.View = view;

                if (current.parent.isEnabled) {
                    current.flags |= UIElementFlags.AncestorEnabled;
                }
                else {
                    current.flags &= ~UIElementFlags.AncestorEnabled;
                }

                if ((current.flags & UIElementFlags.Created) == 0) {
                    current.flags |= UIElementFlags.Created;
//                    current.style.Initialize();
                    for (int i = 0; i < systems.Count; i++) {
                        systems[i].OnElementCreated(current);
                    }

                    onElementRegistered?.Invoke(current);
                    try {
                        current.OnCreate();
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                    }

                    view.ElementRegistered(current);
                }

                UIElement[] children = current.children.array;
                int childCount = current.children.size;
                // reverse this?
                for (int i = 0; i < childCount; i++) {
                    children[i].siblingIndex = i;
                    elemRefStack.Push(new ElemRef() {element = children[i]});
                }
            }

            for (int i = 0; i < parent.children.size; i++) {
                parent.children.array[i].siblingIndex = i;
            }

            if (parentEnabled && child.isEnabled) {
                child.enableStateChangedFrameId = frameId;
                child.flags &= ~UIElementFlags.Enabled;
                DoEnableElement(child, false);
            }

            StructStack<ElemRef>.Release(ref elemRefStack);
        }

        public void SortViews() {
            // let's bubble sort the views since only once view is out of place
            for (int i = (views.Count - 1); i > 0; i--) {
                for (int j = 1; j <= i; j++) {
                    if (views[j - 1].Depth > views[j].Depth) {
                        UIView tempView = views[j - 1];
                        views[j - 1] = views[j];
                        views[j] = tempView;
                    }
                }
            }

            onViewsSorted?.Invoke(views.ToArray());
        }

        internal void GetElementCount(out int totalElementCount, out int enabledElementCount, out int disabledElementCount) {
            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            totalElementCount = 0;
            enabledElementCount = 0;

            for (int i = 0; i < views.Count; i++) {
                stack.Push(views[i].RootElement);

                while (stack.size > 0) {
                    totalElementCount++;
                    UIElement element = stack.PopUnchecked();

                    if (element.isEnabled) {
                        enabledElementCount++;
                    }

                    if (element.children == null) continue;

                    for (int j = 0; j < element.children.size; j++) {
                        stack.Push(element.children.array[j]);
                    }
                }
            }

            disabledElementCount = totalElementCount - enabledElementCount;
            LightStack<UIElement>.Release(ref stack);
        }

        public UIElement CreateSlot(string slotName, TemplateScope scope, int defaultSlotId, UIElement root, UIElement parent) {
            int slotId = ResolveSlotId(slotName, scope.slotInputs, defaultSlotId, out UIElement contextRoot);
            if (contextRoot == null) {
                Assert.AreEqual(slotId, defaultSlotId);
                contextRoot = root;
            }

            // context 0 = innermost
            // context[context.size - 1] = outermost

            scope.innerSlotContext = root;
            // for each override with same name add to reference array at index?
            // will have to be careful with names but can change to unique ids when we need alias support and match on that
            UIElement retn = templateData.slots[slotId](contextRoot, parent, scope);
            retn.View = parent.View;
            return retn;
        }

        public UIElement CreateTemplate(int templateSpawnId, UIElement contextRoot, UIElement parent, TemplateScope scope) {
            UIElement retn = templateData.slots[templateSpawnId](contextRoot, parent, scope);

            InitializeElement(retn);

            return retn;
        }

        /// Returns the shell of a UI Element, space is allocated for children but no child data is associated yet, only a parent, view, and depth
        public UIElement CreateElementFromPool( UIElement element, UIElement parent, int childCount, int attributeCount, int originTemplateId) {
            // children get assigned in the template function but we need to setup the list here
          
            element.application = this;
            element.templateMetaData = templateData.templateMetaData[originTemplateId];
            element.id = NextElementId;
            element.style = new UIStyleSet(element);
            element.layoutResult = new LayoutResult(element);
            element.flags = UIElementFlags.Enabled | UIElementFlags.Alive | UIElementFlags.NeedsUpdate;

            element.children = LightList<UIElement>.GetMinSize(childCount);

            if (attributeCount > 0) {
                element.attributes = new StructList<ElementAttribute>(attributeCount);
                element.attributes.size = attributeCount;
            }

            element.parent = parent;

            parent?.children.Add(element);

            return element;
        }

        public TemplateMetaData GetTemplateMetaData(int metaDataId) {
            return templateData.templateMetaData[metaDataId];
        }

        public UIElement CreateElementFromPoolWithType(UIElement instance, UIElement parent, int childCount, int attrCount, int originTemplateId) {
            // children get assigned in the template function but we need to setup the list here
          
            instance.application = this;
            instance.templateMetaData = templateData.templateMetaData[originTemplateId];
            instance.id = NextElementId;
            instance.style = new UIStyleSet(instance);
            instance.layoutResult = new LayoutResult(instance);
            instance.flags = UIElementFlags.Enabled | UIElementFlags.Alive | UIElementFlags.NeedsUpdate;

            instance.children = LightList<UIElement>.GetMinSize(childCount);

            if (attrCount > 0) {
                instance.attributes = new StructList<ElementAttribute>(attrCount);
                instance.attributes.size = attrCount;
            }

            instance.parent = parent;

            parent?.children.Add(instance);

            return instance;
        }

        public static int ResolveSlotId(string slotName, StructList<SlotUsage> slotList, int defaultId, out UIElement contextRoot) {
            if (slotList == null) {
                contextRoot = null;
                return defaultId;
            }

            for (int i = 0; i < slotList.size; i++) {
                if (slotList.array[i].slotName == slotName) {
                    contextRoot = slotList.array[i].context;
                    return slotList.array[i].slotId;
                }
            }

            contextRoot = null;
            return defaultId;
        }

        // Doesn't expect to create the root
        public void HydrateTemplate(int templateId, UIElement root, TemplateScope scope) {
            templateData.templates[templateId](root, scope);
            scope.Release();
        }

        public static void SetCustomPainters(Dictionary<string, Type> dictionary) {
            s_CustomPainters = dictionary;
        }

    }

}