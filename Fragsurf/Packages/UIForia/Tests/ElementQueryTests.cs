// using JetBrains.Annotations;
// using NUnit.Framework;
// using Tests.Mocks;
// using UIForia.Attributes;
// using UIForia.Elements;
//
// [TestFixture]
// public class ElementQueryTests {
//
//     [TemplateTagName("FindTestThing")]
//     [Template(TemplateType.String, @"
//         <UITemplate>
//             <Contents>
//                 <Group x-id='target'/>
//                 <Group x-some-attr='some-unused-value'>
//                     <Group x-id='find-me'/>
//                 </Group>
//                 <FindTestThingScoped x-id='child'/>
//             </Contents>
//         </UITemplate>
//     ")]
//     public class FindTestThing : UIElement {
//
//         public UIElement target;
//         public UIElement shouldBeNull;
//         public UIElement findMe;
//         public FindTestThingScoped child;
//
//         public override void OnCreate() {
//             target = FindById("target");
//             shouldBeNull = FindById("only-find-from-self");
//             findMe = FindById("find-me");
//             child = (FindTestThingScoped) FindById("child");
//         }
//
//     }
//
//     [TemplateTagName("FindTestThingScoped")]
//     [Template(TemplateType.String, @"
//         <UITemplate>
//             <Contents>
//                <Group x-id='only-find-from-self'/>
//             </Contents>
//         </UITemplate>
//     ")]
//     [UsedImplicitly]
//     public class FindTestThingScoped : UIElement {
//
//         public UIElement childThing;
//
//         public override void OnCreate() {
//             childThing = FindById("only-find-from-self");
//         }
//
//     }
//
//     [Test]
//     public void Query_FindById() {
//         MockApplication app = MockApplication.Setup<FindTestThing>();
//
//         FindTestThing root = (FindTestThing) app.RootElement.GetChild(0);
//         Assert.IsInstanceOf<UIElement>(root.target);
//     }
//
//     [Test]
//     public void Query_FindById_SearchChildren() {
//         MockApplication app = MockApplication.Setup<FindTestThing>();
//
//         FindTestThing root = (FindTestThing) app.RootElement.GetChild(0);
//         Assert.IsNotNull(root.findMe);
//     }
//
//     [Test]
//     public void Query_FindById_FromChild() {
//         MockApplication app = MockApplication.Setup<FindTestThing>();
//         FindTestThing root = (FindTestThing) app.RootElement.GetChild(0);
//         Assert.IsNotNull(root.child.childThing);
//     }
//
// }