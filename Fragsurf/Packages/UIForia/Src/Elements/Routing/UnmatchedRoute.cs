namespace UIForia.Elements.Routing {

    public class UnmatchedRoute : RouteElement {
        
        public override bool TryMatch(RouteMatch match, out RouteMatch result) {
            result = RouteMatch.Match(path, match);
            result.matchProgress = result.url.Length - 1;
            return true;
        }
        
    }

}