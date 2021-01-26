namespace UIForia.Elements.Routing {

    public interface IRouterElement {

        void AddChildRoute(RouteElement routeElement);
        void RemoveChildRoute(RouteElement routeElement);

    }

}