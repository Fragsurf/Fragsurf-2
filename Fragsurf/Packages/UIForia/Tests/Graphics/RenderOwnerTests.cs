// using System;
// using NUnit.Framework;
// using Src.Systems;
// using Tests.Mocks;
// using UIForia;
// using UIForia.Elements;
// using UIForia.Rendering;
// using UIForia.Util;
// using UnityEngine;
//
// [TestFixture]
// public class RenderOwnerTests {
//
//     [Test]
//     public void SortsCorrectlyBasedOnTraversalIndex() {
//         string template = @"
//         <UITemplate>
//             <Contents style.preferredWidth='100f' style.preferredHeight='200f'>
//                 <Group x-id='e0' style.preferredWidth='100f' style.preferredHeight='100f'>
//                     <Group x-id='e1' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
//                     <Group x-id='e2' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
//                     <Group x-id='e3' style.preferredWidth='100f' style.preferredHeight='100f'/>
//                     <Group x-id='e4'>
//                         <Group x-id='e5' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
//                         <Group x-id='e6' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
//                         <Group x-id='e7' style.preferredWidth='100f' style.preferredHeight='100f'/>
//                     </Group>
//                 </Group>
//                 <Group x-id='e8' style.preferredWidth='100f' style.preferredHeight='100f'>
//                     <Group x-id='e9' style.preferredWidth='100f' style.preferredHeight='100f'/>
//                     <Group x-id='e10' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
//                     <Group x-id='e11' style.preferredWidth='100f' style.preferredHeight='100f'/>
//                     <Group x-id='e12'>
//                         <Group x-id='e13' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
//                         <Group x-id='e14' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
//                         <Group x-id='e15' style.preferredWidth='100f' style.preferredHeight='100f'/>
//                     </Group>
//                 </Group>
//                 <Group x-id='e16' style.preferredWidth='100f' style.preferredHeight='100f'>
//                     <Group x-id='e17' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
//                     <Group x-id='e18' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
//                     <Group x-id='e19' style.preferredWidth='100f' style.preferredHeight='100f'/>
//                     <Group x-id='e20'>
//                         <Group x-id='e21' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
//                         <Group x-id='e22' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
//                         <Group x-id='e23' style.preferredWidth='100f' style.preferredHeight='100f'/>
//                     </Group>
//                 </Group>
//             </Contents>
//         </UITemplate>
//         ";
//         throw new NotImplementedException("Upgrade this");
//         MockApplication app = MockApplication.Setup<LayoutSystemTests.LayoutTestThing>();
//         LayoutSystemTests.LayoutTestThing root = (LayoutSystemTests.LayoutTestThing) app.RootElement.GetChild(0);
//
//         app.Update();
//
//         ((VertigoRenderSystem) app.RenderSystem).renderOwners[0].GatherBoxDataParallel();
//         StructList<RenderOwner.RenderBoxWrapper> elements = null; //((VertigoRenderSystem) app.RenderSystem).renderOwners[0].WrapperList;
//
//         for (int i = 0; i < elements.size; i++) {
//             if (elements[i].renderOp != RenderOwner.RenderOpType.DrawBackground) {
//                 elements.RemoveAt(i--);
//                 continue;
//             }
//             //   Debug.Log(elements[i].element + " " + elements[i].renderOp);
//         }
//
//         Assert.AreEqual(root.FindById("e21"), elements[25].element);
//         Assert.AreEqual(root.FindById("e17"), elements[24].element);
//         Assert.AreEqual(root.FindById("e13"), elements[23].element);
//         Assert.AreEqual(root.FindById("e5"), elements[22].element);
//         Assert.AreEqual(root.FindById("e1"), elements[21].element);
//         Assert.AreEqual(root.FindById("e22"), elements[20].element);
//         Assert.AreEqual(root.FindById("e18"), elements[19].element);
//         Assert.AreEqual(root.FindById("e14"), elements[18].element);
//         Assert.AreEqual(root.FindById("e10"), elements[17].element);
//         Assert.AreEqual(root.FindById("e6"), elements[16].element);
//         Assert.AreEqual(root.FindById("e2"), elements[15].element);
//         Assert.AreEqual(root.FindById("e23"), elements[14].element);
//         Assert.AreEqual(root.FindById("e20"), elements[13].element);
//         Assert.AreEqual(root.FindById("e19"), elements[12].element);
//         Assert.AreEqual(root.FindById("e16"), elements[11].element);
//         Assert.AreEqual(root.FindById("e15"), elements[10].element);
//         Assert.AreEqual(root.FindById("e12"), elements[9].element);
//         Assert.AreEqual(root.FindById("e11"), elements[8].element);
//         Assert.AreEqual(root.FindById("e9"), elements[7].element);
//         Assert.AreEqual(root.FindById("e8"), elements[6].element);
//         Assert.AreEqual(root.FindById("e7"), elements[5].element);
//         Assert.AreEqual(root.FindById("e4"), elements[4].element);
//         Assert.AreEqual(root.FindById("e3"), elements[3].element);
//         Assert.AreEqual(root.FindById("e0"), elements[2].element);
//         Assert.AreEqual(root, elements[1].element);
//         Assert.AreEqual(app.RootElement, elements[0].element);
//     }
//
//     [CustomPainter("TestPainterRenderOrder")]
//     public class TestPainterRenderOrder : RenderBox {
//
//         public override void OnInitialize() {
//             hasForeground = true;
//         }
//
//         public override RenderBounds RenderBounds { get; }
//         public override void PaintBackground(RenderContext ctx) { }
//
//     }
//
//     [Test]
//     public void SortsCorrectlyBasedOnTraversalIndexWithForegroundPainter() {
//         string template = @"
//         <UITemplate>
//             <Style>
//                 style painted-foreground {
//                     Painter =  ""TestPainterRenderOrder""; 
//                 }
//             </Style>
//             <Contents style.preferredWidth='100f' style.preferredHeight='200f'>
//                 <Group x-id='e0' style.preferredWidth='100f' style.preferredHeight='100f'>
//                     <Group x-id='e1' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
//                     <Group x-id='e2' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
//                     <Group x-id='e3' style.preferredWidth='100f' style.preferredHeight='100f'/>
//                     <Group x-id='e4'>
//                         <Group x-id='e5' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
//                         <Group x-id='e6' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
//                         <Group x-id='e7' style.preferredWidth='100f' style.preferredHeight='100f'/>
//                     </Group>
//                 </Group>
//                 <Group x-id='e8' style.preferredWidth='100f' style.preferredHeight='100f' style='painted-foreground'>
//                     <Group x-id='e9' style.preferredWidth='100f' style.preferredHeight='100f'/>
//                     <Group x-id='e10' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
//                     <Group x-id='e11' style.preferredWidth='100f' style.preferredHeight='100f'/>
//                     <Group x-id='e12'>
//                         <Group x-id='e13' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
//                         <Group x-id='e14' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
//                         <Group x-id='e15' style.preferredWidth='100f' style.preferredHeight='100f'/>
//                     </Group>
//                 </Group>
//                 <Group x-id='e16' style.preferredWidth='100f' style.preferredHeight='100f'>
//                     <Group x-id='e17' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
//                     <Group x-id='e18' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
//                     <Group x-id='e19' style.preferredWidth='100f' style.preferredHeight='100f'/>
//                     <Group x-id='e20'>
//                         <Group x-id='e21' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
//                         <Group x-id='e22' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
//                         <Group x-id='e23' style.preferredWidth='100f' style.preferredHeight='100f'/>
//                     </Group>
//                 </Group>
//             </Contents>
//         </UITemplate>
//         ";
//                 throw new NotImplementedException("Upgrade this");
//
//         MockApplication app = MockApplication.Setup<LayoutSystemTests.LayoutTestThing>();
//         LayoutSystemTests.LayoutTestThing root = (LayoutSystemTests.LayoutTestThing) app.RootElement.GetChild(0);
//
//         app.Update();
//
//         ((VertigoRenderSystem) app.RenderSystem).renderOwners[0].GatherBoxDataParallel();
//         StructList<RenderOwner.RenderBoxWrapper> elements = null;//((VertigoRenderSystem) app.RenderSystem).renderOwners[0].WrapperList;
//
//         for (int i = 0; i < elements.size; i++) {
//             if (elements[i].renderOp != RenderOwner.RenderOpType.DrawBackground && elements[i].renderOp != RenderOwner.RenderOpType.DrawForeground) {
//                 elements.RemoveAt(i--);
//             }
//         }
//         
//         int idx = elements.size;
//         Assert.AreEqual(root.FindById("e21"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e17"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e13"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e5"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e1"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e22"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e18"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e14"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e10"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e6"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e2"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e23"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e20"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e19"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e16"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e8"), elements[--idx].element);
//         Assert.AreEqual(true, elements[idx].renderOp == RenderOwner.RenderOpType.DrawForeground);
//         Assert.AreEqual(root.FindById("e15"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e12"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e11"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e9"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e8"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e7"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e4"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e3"), elements[--idx].element);
//         Assert.AreEqual(root.FindById("e0"), elements[--idx].element);
//         Assert.AreEqual(root, elements[--idx].element);
//         Assert.AreEqual(app.RootElement, elements[--idx].element);
//     }
//
// }