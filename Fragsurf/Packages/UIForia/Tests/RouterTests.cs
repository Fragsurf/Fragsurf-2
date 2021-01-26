// using NUnit.Framework;
// using Tests.Mocks;
// using UIForia.Attributes;
// using UIForia.Elements;
// using UIForia.Routing;
//
// [TestFixture]
// public class RouterTests {
//     
//     [Template(TemplateType.String, @"
//     <UITemplate>
//         <Contents x-router=""game"" x-defaultRoute=""/user/1"">
//              
//             <Group x-route=""/user/:id"">
//                 This is you
//                 <Div x-id=""inner-route"" x-route=""/something/:id2"">NEST1</Div>
//             </Group>
//             <Group x-id=""friends"" x-route=""/user/:id/friends"">These are your Friends</Group>
//         </Contents>
//     </UITemplate>
//     ")]
//     private class ParsersRouterNestedThing : UIElement { }
//    
//     [Test]
//     public void ParseDefaultRouteWithParameter() {
//         MockApplication app = MockApplication.Setup<ParsersRouterNestedThing>();
//         app.Update();
//         
//         var uiElement1 = app.RootElement.GetChild(0).GetChild(0);
//         var uiElement2 = app.RootElement.GetChild(0).GetChild(1);
//         
//         Assert.IsInstanceOf<UIGroupElement>(uiElement1);
//         Assert.IsInstanceOf<UIGroupElement>(uiElement2);
//         
//         Router router = app.RootElement.application.RoutingSystem.FindRouter("game");
//         string id = router.GetParameter("id");
//         
//         Assert.True(uiElement1.isEnabled);
//         Assert.AreEqual("1", id);
//         
//         Assert.True(uiElement2.isDisabled);
//     }
//
//     [Template(TemplateType.String, @"
//     <UITemplate>
//         <Contents x-router=""game"" x-defaultRoute=""/user/1"">
//              
//             <Group x-route=""/user/:id"">
//                 This is you
//                 <Div x-id=""inner-route"" x-route=""/something/:id2"">NEST1</Div>
//             </Group>
//             <Group x-id=""friends"" x-route=""/user/:id/friends"">These are your Friends</Group>
//         </Contents>
//     </UITemplate>
//     ")]
//     private class DeeplyNestedRouters : UIElement { }
//    
//     [Test]
//     public void DeeplyNestedRoutersShouldWork() {
//         MockApplication app = MockApplication.Setup<DeeplyNestedRouters>();
//         app.Update();
//         
//         var uiElement1 = app.RootElement.GetChild(0).GetChild(0);
//         var uiElement2 = app.RootElement.GetChild(0).GetChild(1);
//         
//         Assert.IsInstanceOf<UIGroupElement>(uiElement1);
//         Assert.IsInstanceOf<UIGroupElement>(uiElement2);
//         
//         Router router = app.RootElement.application.RoutingSystem.FindRouter("game");
//         string id = router.GetParameter("id");
//         
//         Assert.True(uiElement1.isEnabled);
//         Assert.AreEqual("1", id);
//         
//         Assert.True(uiElement2.isDisabled);
//     }
//
//     [Test]
//     public void ParseInnerRouteWithParameter() {
//         MockApplication app = MockApplication.Setup<ParsersRouterNestedThing>();
//         app.Update();
//         
//         var uiElement1 = app.RootElement.GetChild(0).GetChild(0);
//         Router router = app.RootElement.application.RoutingSystem.FindRouter("game");
//         router.GoTo("/user/2/something/23");
//         
//         app.Update();
//
//         var innerRouteElement = app.RootElement.GetChild(0).FindById<UIElement>("inner-route");
//         var friendsElement = app.RootElement.GetChild(0).FindById<UIElement>("friends");
//         Assert.IsInstanceOf<UIDivElement>(innerRouteElement);
//         Assert.True(uiElement1.isEnabled);
//         Assert.True(innerRouteElement.isEnabled);
//         Assert.True(friendsElement.isDisabled);
//
//         string id = router.GetParameter("id");
//         string id2 = router.GetParameter("id2");
//         Assert.AreEqual("2", id);
//         Assert.AreEqual("23", id2);
//     }
//
//     [Template(TemplateType.String, @"
//     <UITemplate>
//         <Contents x-router=""game"">
//              
//             <Group x-id=""help"" x-route=""/help"">
//                 Get some help
//                 <Div x-id=""faq-route1"" x-route=""/faq/1"" x-defaultRoute=""true"">fact 1</Div>
//                 <Div x-id=""faq-route2"" x-route=""/faq/2"">fact 2</Div>
//                 <Div x-id=""faq-route3"" x-route=""/faq/3"">fact 3</Div>
//             </Group>
//             <Group x-id=""friends"" x-route=""/user/:id/friends"">These are your Friends</Group>
//         </Contents>
//     </UITemplate>
//     ")]
//     private class RoutesWithDefaults : UIElement { }
//
//     [Test]
//     public void ParseInnerRouteWithDefaultAttribute() {
//         MockApplication app = MockApplication.Setup<RoutesWithDefaults>();
//         app.Update();
//        
//         var helpElement = app.RootElement.GetChild(0).FindById<UIGroupElement>("help");
//         var faqRoute1 = app.RootElement.GetChild(0).FindById<UIDivElement>("faq-route1");
//         var faqRoute2 = app.RootElement.GetChild(0).FindById<UIDivElement>("faq-route2");
//         var faqRoute3 = app.RootElement.GetChild(0).FindById<UIDivElement>("faq-route3");
//         
//         var friendsElement = app.RootElement.GetChild(0).FindById<UIElement>("friends");
//         Assert.True(helpElement.isEnabled);
//         Assert.True(faqRoute1.isEnabled);
//         Assert.True(faqRoute2.isDisabled);
//         Assert.True(faqRoute3.isDisabled);
//         Assert.True(friendsElement.isDisabled);
//     }
//     
//     [Test]
//     public void GoBackAndForwards() {
//         MockApplication app = MockApplication.Setup<RoutesWithDefaults>();
//         app.Update();
//        
//         var helpElement = app.RootElement.GetChild(0).FindById<UIGroupElement>("help");
//         var faqRoute1 = app.RootElement.GetChild(0).FindById<UIDivElement>("faq-route1");
//         var faqRoute2 = app.RootElement.GetChild(0).FindById<UIDivElement>("faq-route2");
//         var faqRoute3 = app.RootElement.GetChild(0).FindById<UIDivElement>("faq-route3");
//         var friendsElement = app.RootElement.GetChild(0).FindById<UIElement>("friends");
//         
//         // starting at faq1 and go to users friends
//         app.RootElement.application.RoutingSystem.FindRouter("game").GoTo("/user/1/friends");
//         app.Update();
//         
//         Assert.True(helpElement.isDisabled);
//         Assert.True(faqRoute1.isDisabled);
//         Assert.True(faqRoute2.isDisabled);
//         Assert.True(faqRoute3.isDisabled);
//         
//         // just the friends are active!
//         Assert.True(friendsElement.isEnabled);
//         
//         // let's go back 
//         app.RootElement.application.RoutingSystem.FindRouter("game").GoBack();
//         app.Update();
//         Assert.True(helpElement.isEnabled);
//         Assert.True(faqRoute1.isEnabled);
//         Assert.True(faqRoute2.isDisabled);
//         Assert.True(faqRoute3.isDisabled);
//         Assert.True(friendsElement.isDisabled);
//     }
//     
//     [Template(TemplateType.String, @"
//     <UITemplate>
//         <Contents x-router=""game"" x-defaultRoute=""/character/1"">
//             <Group x-route=""/character"">
//                 This is you
//                 <Div x-id=""characterDetail"" x-route=""/:characterId"">The Shredder</Div>
//             </Group>
//             <Group x-id=""friends"" x-route=""/user/:id/friends"">These are your Friends</Group>
//             <Group x-router=""chat"">
//                 <Div x-id=""chatWithMatt"" x-route=""/chat/matt"" x-defaultRoute=""true"" />
//                 <Div x-id=""chatWithChristian"" x-route=""/chat/christian"" />
//             </Group> 
//        </Contents>
//     </UITemplate>
//     ")]
//     private class GameViewWithLotsOfRouters : UIElement { }
//     
//     [Test]
//     public void NavigateDifferentRouters() {
//         MockApplication app = MockApplication.Setup<GameViewWithLotsOfRouters>();
//         app.Update();
//        
//         var characterDetail = app.RootElement.GetChild(0).FindById<UIDivElement>("characterDetail");
//         var friendsElement = app.RootElement.GetChild(0).FindById<UIGroupElement>("friends");
//         var chatWithChristian = app.RootElement.GetChild(0).FindById<UIDivElement>("chatWithChristian");
//         var chatWithMatt = app.RootElement.GetChild(0).FindById<UIDivElement>("chatWithMatt");
//         
//         Assert.True(characterDetail.isEnabled);
//         Assert.True(friendsElement.isDisabled);
//         Assert.True(chatWithMatt.isEnabled);
//         Assert.True(chatWithChristian.isDisabled);
//
//         var gameRouter = app.RootElement.application.RoutingSystem.FindRouter("game");
//         var chatRouter = app.RootElement.application.RoutingSystem.FindRouter("chat");
//         
//         gameRouter.GoTo("/user/12/friends");
//         chatRouter.GoTo("/chat/christian");
//         app.Update();
//         
//         Assert.True(characterDetail.isDisabled);
//         Assert.True(friendsElement.isEnabled);
//         Assert.True(chatWithMatt.isDisabled);
//         Assert.True(chatWithChristian.isEnabled);
//
//         string friendId = gameRouter.GetParameter("id");
//         Assert.AreEqual("12", friendId);
//         Assert.IsNull(chatRouter.GetParameter("id").value);
//     }
//
// }
//
// //
// //    [Template(TemplateType.String, @"
// //    <UITemplate>
// //        <Contents>
// //            <Router>
// //                <Route path=""'/user'""/>
// //            </Router>
// //        </Contents>
// //    </UITemplate>
// //    ")]
// //    private class ParsersRouterRootThing : UIElement { }
// //
// //    [Test]
// //    public void ParsersRouterRoot() {
// //        MockApplication app = new MockApplication(typeof(ParsersRouterRootThing));
// //        Assert.IsInstanceOf<RouterElement>(app.RootElement.GetChild(0));
// //    }
// //
// //    [Template(TemplateType.String, @"
// //    <UITemplate>
// //        <Contents>
// //            <Router>
// //                <Route path=""'/options/:id'""/>
// //                <Route path=""'/options2/:id'""/>
// //                <UnmatchedRoute/>
// //            </Router>
// //        </Contents>
// //    </UITemplate>
// //    ")]
// //    private class ParsersRouterNestedThing : UIElement { }
// //
// //    [Test]
// //    public void ParsesRouterWithSubRoutes() {
// //        MockApplication app = new MockApplication(typeof(ParsersRouterNestedThing));
// //        app.Update();
// //        Assert.IsInstanceOf<RouterElement>(app.RootElement.GetChild(0));
// //        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
// //        Assert.AreEqual("/options/:id", router.FindByType<RouteElement>()[0].path);
// //        Assert.AreEqual("/options2/:id", router.FindByType<RouteElement>()[1].path);
// //        Assert.IsTrue(router.FindByType<RouteElement>()[0].isDisabled);
// //        Assert.IsTrue(router.FindByType<RouteElement>()[1].isDisabled);
// //        Assert.IsTrue(router.FindByType<RouteElement>()[2].isEnabled); 
// //    }
// //
// //    [Test]
// //    public void CallsEnterHandlerForMatchedRoute() {
// //        MockApplication app = new MockApplication(typeof(ParsersRouterNestedThing));
// //        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
// //        bool didEnter = false;
// //        router.onRouteEnter += () => { didEnter = true; };
// //        app.Update();
// //        app.Router.GoTo("/options/1");
// //        Assert.IsTrue(didEnter);
// //        Assert.IsTrue(router.FindByType<RouteElement>()[0].isEnabled);
// //        Assert.IsTrue(router.FindByType<RouteElement>()[1].isDisabled);
// //        Assert.IsTrue(router.FindByType<RouteElement>()[2].isDisabled);
// //    }
// //
// //    [Test]
// //    public void CallsExitHandlerForMatchedRoute() {
// //        MockApplication app = new MockApplication(typeof(ParsersRouterNestedThing));
// //        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
// //        bool didExit = false;
// //        router.onRouteExit += () => { didExit = true; };
// //        app.Update();
// //        app.Router.GoTo("/options/1");
// //        app.Router.GoTo("/options2/4");
// //        Assert.IsTrue(didExit);
// //        Assert.IsTrue(router.FindByType<RouteElement>()[0].isDisabled);
// //        Assert.IsTrue(router.FindByType<RouteElement>()[1].isEnabled);
// //        Assert.IsTrue(router.FindByType<RouteElement>()[2].isDisabled);
// //    }
// //
// //    [Test]
// //    public void MatchesUnmatchedRoute() {
// //        MockApplication app = new MockApplication(typeof(ParsersRouterNestedThing));
// //        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
// //        app.Update();
// //        app.Router.GoTo("/opt");
// //        Assert.IsTrue(router.FindByType<RouteElement>()[0].isDisabled);
// //        Assert.IsTrue(router.FindByType<RouteElement>()[1].isDisabled);
// //        Assert.IsTrue(router.FindByType<RouteElement>()[2].isEnabled);
// //    }
// //
// //    [Template(TemplateType.String, @"
// //    <UITemplate>
// //        <Contents>
// //            <Router>
// //                <Route path=""'/users'"">
// //                    <ChildRouter>
// //                        <Route path=""'/settings'""/>        
// //                        <Route path=""'/friends'""/>        
// //                        <Route path=""'/:id'""/>        
// //                    </ChildRouter>
// //                 </Route>
// //                <Route path=""'/options/:id'""/>
// //                <Route path=""'/options2/:id'""/>
// //                <UnmatchedRoute/>
// //            </Router>
// //        </Contents>
// //    </UITemplate>
// //    ")]
// //    private class ParsersRouterChildNestedThing : UIElement { }
// //
// //    [Test]
// //    public void MatchChildRouter() {
// //        MockApplication app = new MockApplication(typeof(ParsersRouterChildNestedThing));
// //        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
// //        app.Update();
// //        app.Router.GoTo("/users/settings");
// //        ChildRouterElement childRouter = router.FindFirstByType<ChildRouterElement>();
// //
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isEnabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isDisabled);
// //    }
// //
// //    [Test]
// //    public void UpdateChildRouter() {
// //        MockApplication app = new MockApplication(typeof(ParsersRouterChildNestedThing));
// //        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
// //        app.Update();
// //        app.Router.GoTo("/users/settings");
// //
// //        ChildRouterElement childRouter = router.FindFirstByType<ChildRouterElement>();
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isEnabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isDisabled);
// //
// //        app.Router.GoTo("/users/friends");
// //
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isEnabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isDisabled);
// //
// //        app.Router.GoTo("/users/other");
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isEnabled);
// //        Assert.AreEqual(childRouter.FindByType<RouteElement>()[2].GetRouteParameter("id").value, "other");
// //    }
// //
// //    [Test]
// //    public void ExitReEnterChildRouter() {
// //        MockApplication app = new MockApplication(typeof(ParsersRouterChildNestedThing));
// //        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
// //        app.Update();
// //        app.Router.GoTo("/users/settings");
// //
// //        ChildRouterElement childRouter = router.FindFirstByType<ChildRouterElement>();
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isEnabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isDisabled);
// //
// //        app.Router.GoTo("/options");
// //
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isDisabled);
// //
// //        app.Router.GoTo("/users/other");
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isEnabled);
// //        Assert.AreEqual(childRouter.FindByType<RouteElement>()[2].GetRouteParameter("id").value, "other");
// //    }
// //
// //    [Test]
// //    public void ChangeParameterChildRouter() {
// //        MockApplication app = new MockApplication(typeof(ParsersRouterChildNestedThing));
// //        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
// //        app.Update();
// //        app.Router.GoTo("/users/settings");
// //
// //        ChildRouterElement childRouter = router.FindFirstByType<ChildRouterElement>();
// //
// //        app.Router.GoTo("/users/other");
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isEnabled);
// //        Assert.AreEqual("other", childRouter.FindByType<RouteElement>()[2].GetRouteParameter("id").value);
// //
// //        app.Router.GoTo("/users/other_thing");
// //
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isEnabled);
// //        Assert.AreEqual("other_thing", childRouter.FindByType<RouteElement>()[2].GetRouteParameter("id").value);
// //    }
// //
// //    [Test]
// //    public void ChildRouterOnRouteChanged() {
// //        MockApplication app = new MockApplication(typeof(ParsersRouterChildNestedThing));
// //        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
// //        app.Update();
// //
// //        RouteElement current = null;
// //        int callCount = 0;
// //        ChildRouterElement childRouter = router.FindFirstByType<ChildRouterElement>();
// //        childRouter.onRouteChanged += () => {
// //            callCount++;
// //            current = childRouter.ActiveChild;
// //        };
// //
// //        app.Router.GoTo("/users/settings");
// //        Assert.AreEqual(0, callCount);
// //
// //        app.Router.GoTo("/users/friends");
// //        Assert.AreEqual(current, childRouter.FindByType<RouteElement>()[1]);
// //        Assert.AreEqual(1, callCount);
// //
// //        app.Router.GoTo("/users/other_thing");
// //        Assert.AreEqual(current, childRouter.FindByType<RouteElement>()[2]);
// //        Assert.AreEqual(2, callCount);
// //
// //        app.Router.GoTo("/users/blah");
// //        Assert.AreEqual(current, childRouter.FindByType<RouteElement>()[2]);
// //        Assert.AreEqual(2, callCount);
// //    }
// //
// //    [Template(TemplateType.String, @"
// //    <UITemplate>
// //        <Contents>
// //            <Router>
// //                <Route path=""'/users'"">
// //                    <ChildRouter path=""'/extra'"">
// //                        <Route path=""'/settings'""/>        
// //                        <Route path=""'/friends'""/>        
// //                        <Route path=""'/:id'""/>        
// //                    </ChildRouter>
// //                 </Route>
// //                <Route path=""'/options/:id'""/>
// //                <Route path=""'/options2/:id'""/>
// //                <UnmatchedRoute/>
// //            </Router>
// //        </Contents>
// //    </UITemplate>
// //    ")]
// //    private class ParsersRouterChildWithPathNestedThing : UIElement { }
// //    
// //    [Test]
// //    public void ChildRouterWithPath() {
// //        MockApplication app = new MockApplication(typeof(ParsersRouterChildWithPathNestedThing));
// //        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
// //        app.Update();
// //        app.Router.GoTo("/users/extra/settings");
// //
// //        ChildRouterElement childRouter = router.FindFirstByType<ChildRouterElement>();
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isEnabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isDisabled);
// //
// //        app.Router.GoTo("/users/extra/friends");
// //
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isEnabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isDisabled);
// //
// //        app.Router.GoTo("/users/extra/other");
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
// //        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isEnabled);
// //        Assert.AreEqual(childRouter.FindByType<RouteElement>()[2].GetRouteParameter("id").value, "other");
// //    }
// //
// //}