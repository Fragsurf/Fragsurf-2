using System;
using UIForia.Attributes;

namespace UIForia.Elements.Routing {

    [TemplateTagName("GuardedRoute")]
    public class GuardedRoute : RouteElement {

        public Func<bool> guardFn;
        
        public override bool TryMatch(RouteMatch match, out RouteMatch result) {
            if (guardFn == null || !guardFn()) {
                result = default(RouteMatch);
                result.matchProgress = -1;
                return false;
            }
            
            return base.TryMatch(match, out result);
        }

    }

}