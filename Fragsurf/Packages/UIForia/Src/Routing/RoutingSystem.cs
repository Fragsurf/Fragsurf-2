using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;

namespace UIForia.Routing {

    public class RoutingSystem : ISystem {

        private readonly LightList<Router> m_Routers;
        private readonly List<ElementAttribute> m_ScratchAttrList = new List<ElementAttribute>();

        public RoutingSystem() {
            this.m_Routers = new LightList<Router>();
        }
        
        public void OnReset() {
            m_Routers.Clear();    
        }

        public void OnUpdate() {
            for (int i = 0; i < m_Routers.Count; i++) {
                m_Routers[i].Tick();
            }
        }

        public void OnDestroy() {}

        public void OnViewAdded(UIView view) {}

        public void OnViewRemoved(UIView view) {}

        public void OnElementEnabled(UIElement element) {}

        public void OnElementDisabled(UIElement element) {}

        public void OnElementDestroyed(UIElement element) {}

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string attributeValue) {}
        
        public void OnElementCreated(UIElement element) {

            m_ScratchAttrList.Clear();
            element.GetAttributes(m_ScratchAttrList);

            if (m_ScratchAttrList.Count == 0) {
                return;
            }
            
            if (TryGetAttribute("router", m_ScratchAttrList, out ElementAttribute routerAttr)) {
                TryGetAttribute("defaultRoute", m_ScratchAttrList, out ElementAttribute defaultRouteAttr);

                Router router = new Router(element.id, routerAttr.value, defaultRouteAttr.value);

                for (int i = 0; i < m_Routers.Count; i++) {
                    if (m_Routers[i].name == router.name) {
                        throw new Exception("Duplicate router defined with the name: " + router.name);
                    }
                }
                
                m_Routers.Add(router);
                
            }

            else if (TryGetAttribute("route", m_ScratchAttrList, out ElementAttribute routeAttr)) {

                string path = routeAttr.value;
                TryGetAttribute("defaultRoute", m_ScratchAttrList, out ElementAttribute defaultRouteAttr);

                Route route = new Route(path, element, defaultRouteAttr.value);

                // if (TryGetAttribute("onRouteEnter", m_ScratchAttrList, out ElementAttribute onRouteEnterAttr)) { }
                //
                // if (TryGetAttribute("onRouteEnter", m_ScratchAttrList, out ElementAttribute onRouteChangedAttr)) { }
                //
                // if (TryGetAttribute("onRouteEnter", m_ScratchAttrList, out ElementAttribute onRouteExitAttr)) { }

                Router router = FindRouterInHierarchy(element);
                if (router == null) {
                    throw new Exception("Cannot resolve router in hierarchy");
                }

                if (router.TryGetParentRouteFor(element, out Route parent)) {
                    route.path = parent.path + route.path;
                    parent.subRoutes.Add(route);
                }

                router.AddRoute(route);

                if (router.defaultRoute != path) {
                    element.SetEnabled(false);
                }
            }
            
        }
        
        private static bool TryGetAttribute(string name, IReadOnlyList<ElementAttribute> attributes, out ElementAttribute retn) {
            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i].name == name) {
                    retn = attributes[i];
                    return true;
                }
            }

            retn = default;
            return false;
        }

        public Router FindRouterInHierarchy(UIElement element) {
            IHierarchical ptr = element;
            while (ptr != null) {
                for (int i = 0; i < m_Routers.Count; i++) {
                    if (m_Routers[i].hostId == ptr.UniqueId) {
                        return m_Routers[i];
                    }
                }

                ptr = ptr.Parent;
            }

            return null;
        }
        
        public Router FindRouter(string routerName) {
            for (int i = 0; i < m_Routers.Count; i++) {
                if (m_Routers[i].name == routerName) {
                    return m_Routers[i];
                }
            }

            return null;
        }

    }

}