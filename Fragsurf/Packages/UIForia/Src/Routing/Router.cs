using System;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Routing {

    public enum RouteTransitionState {

        Pending,
        Completed,
        Cancelled

    }

    public struct RouteTransition {

        public readonly string fromPath;
        public readonly string toPath;
        public readonly Func<float, RouteTransitionState> fn;

        public RouteTransition(string fromPath, string toPath, Func<float, RouteTransitionState> fn) {
            this.fromPath = fromPath;
            this.toPath = toPath;
            this.fn = fn;
        }

    }

    public struct RouteMatch {

        public readonly string url;
        public readonly bool matches;
        public readonly bool parentMatch;
        public LightList<RouteParameter> routeParameters;

        public RouteMatch(string url, bool matches, bool parentMatch, LightList<RouteParameter> routeParameters) {
            this.url = url;
            this.matches = matches;
            this.parentMatch = parentMatch;
            this.routeParameters = routeParameters;
            if (!matches) Release();
        }

        public void Release() {
            if (routeParameters != null) {
                LightList<RouteParameter>.Release(ref routeParameters);
            }
        }
    }

    public class Route {

        public string path;
        public UIElement element;
        public event Action onRouteEnter;
        public event Action onRouteExit;
        public event Action onRouteUpdate;
        internal readonly LightList<RouteParameter> m_RouteParameters;
        public readonly bool isDefaultRoute;
        internal readonly LightList<Route> subRoutes;

        public Route(string path, UIElement element, string defaultRouteParamValue = null) {
            this.path = path;
            this.element = element;
            this.isDefaultRoute = defaultRouteParamValue == "true";
            this.subRoutes = new LightList<Route>();
        }

        public void Enter() {
            element.SetEnabled(true);
            onRouteEnter?.Invoke();
        }

        public void Exit() {
            element.SetEnabled(false);
            onRouteExit?.Invoke();
        }

        public void Update() {
            onRouteUpdate?.Invoke();
        }

        public static RouteMatch Match(string path, string matchPath, bool parentMatch = false) {
            if (path == "*") {
                return new RouteMatch(matchPath, false, parentMatch, null);
            }

            LightList<RouteParameter> routeParameters = LightList<RouteParameter>.Get();

            // note: /test/url does not match /test/url/

            string[] pathSegments = path.Split('/');
            string[] destinationPathSegments = matchPath.Split('/');

            for (int i = 0; i < pathSegments.Length; i++) {
                if (string.IsNullOrEmpty(pathSegments[i])) continue;

                if (destinationPathSegments.Length <= i) {
                    return new RouteMatch(matchPath, false, parentMatch, routeParameters);
                }

                if (pathSegments[i] == "*" && i == pathSegments.Length - 1) {
                    return new RouteMatch(matchPath, false, parentMatch, routeParameters);
                }

                if (pathSegments[i].StartsWith(":")) {
                    string pathVariableName = pathSegments[i].Substring(1, pathSegments[i].Length - 1);
                    routeParameters.Add(new RouteParameter(pathVariableName, destinationPathSegments[i]));
                }
                else if (pathSegments[i] != destinationPathSegments[i]) {
                    return new RouteMatch(matchPath, false, parentMatch, routeParameters);
                }
            }

            return new RouteMatch(matchPath, destinationPathSegments.Length == pathSegments.Length, parentMatch, routeParameters);
        }

        public static RouteMatch Match(Route route, string matchPath, bool parentMatch = false) {
            // if a subroute matches this path then the parent shall always be active
            for (int i = 0; i < route.subRoutes.Count; i++) {
                RouteMatch routeMatch = Match(route.subRoutes[i], matchPath, true);
                if (routeMatch.matches) {
                    return routeMatch;
                }
            }

            return Match(route.path, matchPath, parentMatch);
        }
    }

    public class Router {

        public int hostId;
        public readonly string name;
        private bool isInitialized;
        private int historyIndex;

        private readonly LightList<string> m_HistoryStack;
        private readonly LightList<string> m_ForwardHistoryStack;
        private readonly LightList<Route> m_RouteHandlers;
        private readonly LightList<RouteTransition> m_Transitions;
        private readonly LightList<RouteTransition> m_ActiveTransitions;
        private readonly LightList<RouteParameter> m_CurrentRouteParameters;
        private readonly LightList<Route> m_ActiveParentRoutes;
        private readonly LightList<Route> m_TargetParentRoutes;

        private Route activeRoute;
        private Route targetRoute;

        private string targetUrl;
        public string defaultRoute { set; get; }

        internal Router(int hostId, string name, string defaultRoute = null) {
            this.hostId = hostId;
            this.name = name;
            this.defaultRoute = defaultRoute;

            m_HistoryStack = new LightList<string>();
            m_ForwardHistoryStack = new LightList<string>();
            m_RouteHandlers = new LightList<Route>();
            m_Transitions = new LightList<RouteTransition>();
            m_ActiveTransitions = new LightList<RouteTransition>();
            m_CurrentRouteParameters = new LightList<RouteParameter>();
            m_ActiveParentRoutes = new LightList<Route>();
            m_TargetParentRoutes = new LightList<Route>();

            activeRoute = null;
            targetRoute = null;
        }

        public bool CanGoForwards => m_ForwardHistoryStack.Count > 1;

        public bool CanGoBackwards => m_HistoryStack.Count > 1 && historyIndex != 0;

        public void GoBack() {
            if (historyIndex == 0) {
                return;
            }

            string lastRoute = m_HistoryStack.RemoveLast();
            m_ForwardHistoryStack.Add(lastRoute);

            string route = m_HistoryStack.RemoveLast();
            historyIndex = historyIndex - 2;
            GoTo(route, true);
        }

        public void GoForwards() {
            if (!CanGoForwards) {
                return;
            }

            GoTo(m_ForwardHistoryStack.RemoveLast(), true);
        }

        public Route ActiveRoute => activeRoute;
        public Route TargetRoute => targetRoute;
        public string CurrentUrl { get; private set; }

        public void AddRoute(UIElement element, string path) {
            this.m_RouteHandlers.Add(new Route(path, element));
        }

        public void AddRoute(Route route) {
            this.m_RouteHandlers.Add(route);
            if (activeRoute == null && route.path == defaultRoute) {
                activeRoute = route;
            }
        }

        public void AddTransition(string fromPath, string toPath, Func<float, RouteTransitionState> action) {
            m_Transitions.Add(new RouteTransition(fromPath, toPath, action));
        }

        public void Initialize() {
            if (isInitialized) return;
            isInitialized = true;

            if (defaultRoute == null) {
                for (int i = 0; i < m_RouteHandlers.Count; i++) {
                    if (m_RouteHandlers[i].isDefaultRoute) {
                        GoTo(m_RouteHandlers[i].path);
                        return;
                    }
                }
            }
            else {
                GoTo(defaultRoute);
            }
        }

        private float tickElapsed;

        public void Tick() {
            Initialize();

            if (m_ActiveTransitions.Count == 0) {
                return;
            }

            for (int i = 0; i < m_ActiveTransitions.Count; i++) {
                RouteTransitionState state = m_ActiveTransitions[i].fn(tickElapsed);

                if (state == RouteTransitionState.Cancelled) {
                    // reset transition state
                    m_ActiveTransitions.Clear();
                    return;
                }

                if (state == RouteTransitionState.Completed) {
                    m_ActiveTransitions.RemoveAt(i--);
                }
            }

            if (m_ActiveTransitions.Count == 0) {
                EnterRoute(default);
            }

            tickElapsed += Time.unscaledDeltaTime;
        }

        // todo if goto is called during an active transition the old route will not be disabled leaving two routes active at the same time
        public void GoTo(string path, bool preserveForwardHistory = false) {
            targetUrl = path;
            for (int i = 0; i < m_RouteHandlers.Count; i++) {
                RouteMatch match = Route.Match(m_RouteHandlers[i], path);

                if (match.matches) {
                    if (match.parentMatch) {
                        m_TargetParentRoutes.Add(m_RouteHandlers[i]);
                        continue;
                    }

                    if (!IsTransitionAllowed(path, match.url)) {
                        return;
                    }

                    targetRoute = m_RouteHandlers[i];

                    GatherTransitions(path);

                    if (m_ActiveTransitions.Count == 0) {
                        EnterRoute(match);
                        if (!preserveForwardHistory) {
                            m_ForwardHistoryStack.Clear();
                        }
                    }

                    return;
                }
            }
        }

        private void EnterRoute(RouteMatch match) {
            for (int i = 0; i < m_ActiveParentRoutes.Count; i++) {
                if (!m_TargetParentRoutes.Contains(m_ActiveParentRoutes[i])) {
                    m_ActiveParentRoutes[i]?.Exit();
                }
            }

            for (int i = 0; i < m_TargetParentRoutes.Count; i++) {
                if (!m_ActiveParentRoutes.Contains(m_TargetParentRoutes[i])) {
                    m_TargetParentRoutes[i]?.Enter();
                }
            }

            m_ActiveParentRoutes.Clear();
            m_ActiveParentRoutes.AddRange(m_TargetParentRoutes);
            m_TargetParentRoutes.Clear();

            if (targetRoute != activeRoute) {
                if (!m_ActiveParentRoutes.Contains(activeRoute)) {
                    // only exit the currently active route if it isn't a parent of the target route
                    activeRoute?.Exit();
                }

                targetRoute.Enter();
                activeRoute = targetRoute;
            }
            else {
                activeRoute.Update();
            }

            m_CurrentRouteParameters.Clear();
            m_CurrentRouteParameters.AddRange(match.routeParameters);
            match.Release();

            historyIndex++;
            m_HistoryStack.Add(targetUrl);
            tickElapsed = 0;
            CurrentUrl = targetUrl;
        }

        private void GatherTransitions(string path) {
            if (CurrentUrl == null) return;

            for (int i = 0; i < m_Transitions.Count; i++) {
                if (IsTransitionApplicable(m_Transitions[i], path)) {
                    m_ActiveTransitions.Add(m_Transitions[i]);
                }
            }
        }

        private bool IsTransitionApplicable(RouteTransition transition, string path) {
            bool first = Route.Match(transition.fromPath, CurrentUrl).matches;
            bool second = Route.Match(transition.toPath, path).matches;
            return first && second;
        }

        private bool IsTransitionAllowed(string path, string targetPath) {
            return true;
        }

        public RouteParameter GetParameter(string parameterName) {
            RouteParameter[] routeParameters = m_CurrentRouteParameters.Array;
            for (int i = 0; i < routeParameters.Length; i++) {
                if (routeParameters[i].name == parameterName) {
                    return routeParameters[i];
                }
            }

            return default;
        }

        internal bool TryGetParentRouteFor(UIElement element, out Route route) {
            IHierarchical ptr = element.Parent;
            while (ptr != null) {
                for (int i = 0; i < m_RouteHandlers.Count; i++) {
                    Route r = m_RouteHandlers[i];
                    if (r.element == ptr) {
                        route = r;
                        return true;
                    }
                }

                ptr = ptr.Parent;
            }

            route = null;
            return false;
        }
    }

}
