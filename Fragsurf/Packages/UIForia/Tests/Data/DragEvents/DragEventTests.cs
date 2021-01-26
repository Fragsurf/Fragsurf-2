using System;
using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using static Tests.TestUtils;

namespace DragEventTests {

    public class DragEventTests {

        [SetUp]
        public void Setup() {
            MockApplication.s_GenerateCode = false;
            MockApplication.s_UsePreCompiledTemplates = false;
        }

        public class TestDragEvent : DragEvent {

            public string sourceName;

            public TestDragEvent(string sourceName) {
                this.sourceName = sourceName;
            }

        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_method")]
        public class DragTestThing_CreateMethod : UIElement {

            public DragEvent CreateDragFromChild(MouseInputEvent evt, int index) {
                return new TestDragEvent("child" + index);
            }

        }

        [Test]
        public void DragCreate_FromChildTemplate_Method() {
            MockApplication testView = MockApplication.Setup<DragTestThing_CreateMethod>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));

            testView.InputSystem.MouseDown(new Vector2(20, 20));
            testView.Update();

            Assert.IsNull(testView.InputSystem.CurrentDragEvent);

            testView.InputSystem.MouseDragMove(new Vector2(25, 20));
            testView.Update();

            Assert.IsInstanceOf<TestDragEvent>(testView.InputSystem.CurrentDragEvent);
            Assert.AreEqual("child0", As<TestDragEvent>(testView.InputSystem.CurrentDragEvent).sourceName);
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_lambda")]
        public class DragTestThing_CreateLambda : UIElement {

            public DragEvent CreateDragFromChild(MouseInputEvent evt, int index) {
                return new TestDragEvent("child" + index);
            }

        }

        [Test]
        public void DragCreate_FromChildTemplate_Lambda() {
            MockApplication testView = MockApplication.Setup<DragTestThing_CreateLambda>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));

            testView.InputSystem.MouseDown(new Vector2(20, 20));
            testView.Update();

            Assert.IsNull(testView.InputSystem.CurrentDragEvent);

            testView.InputSystem.MouseDragMove(new Vector2(25, 20));
            testView.Update();

            Assert.IsInstanceOf<TestDragEvent>(testView.InputSystem.CurrentDragEvent);
            Assert.AreEqual("child1", As<TestDragEvent>(testView.InputSystem.CurrentDragEvent).sourceName);
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_lambda_arg")]
        public class DragTestThing_CreateLambdaArg : UIElement {

            public DragEvent CreateDragFromChild(MouseInputEvent evt, int index) {
                return new TestDragEvent("child" + index);
            }

        }

        [Test]
        public void DragCreate_FromChildTemplate_LambdaArg() {
            MockApplication testView = MockApplication.Setup<DragTestThing_CreateLambdaArg>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));

            testView.InputSystem.MouseDown(new Vector2(20, 20));
            testView.Update();

            Assert.IsNull(testView.InputSystem.CurrentDragEvent);

            testView.InputSystem.MouseDragMove(new Vector2(25, 20));
            testView.Update();

            Assert.IsInstanceOf<TestDragEvent>(testView.InputSystem.CurrentDragEvent);
            Assert.AreEqual("child3", As<TestDragEvent>(testView.InputSystem.CurrentDragEvent).sourceName);
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_lambda_arg_invalid_retn")]
        public class DragTestThing_CreateLambdaArgInvalidRetn : UIElement {

            public void CreateDragFromChild(MouseInputEvent evt, int index) { }

        }

        [Test]
        public void CreateLambdaArgInvalidRetn() {
            CompileException exception = Assert.Throws<CompileException>(() => { MockApplication.Setup<DragTestThing_CreateLambdaArgInvalidRetn>(); });
            Assert.IsTrue(exception.Message.Contains(@"drag:create=""(e) => CreateDragFromChild(e, 3)"""));
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_annotation_param")]
        public class DragTestThing_CreateAnnotationWithParameter : UIElement {

            [OnDragCreate]
            public DragEvent CreateDrag(MouseInputEvent evt) {
                return new TestDragEvent("from class: " + evt.MousePosition);
            }

        }

        [Test]
        public void DragCreate_CreateAnnotationWithParameter() {
            MockApplication testView = MockApplication.Setup<DragTestThing_CreateAnnotationWithParameter>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));

            testView.InputSystem.MouseDown(new Vector2(20, 20));
            testView.Update();

            Assert.IsNull(testView.InputSystem.CurrentDragEvent);

            testView.InputSystem.MouseDragMove(new Vector2(25, 20));
            testView.Update();

            Assert.IsInstanceOf<TestDragEvent>(testView.InputSystem.CurrentDragEvent);
            Assert.AreEqual("from class: " + new Vector2(25, 20), As<TestDragEvent>(testView.InputSystem.CurrentDragEvent).sourceName);
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_annotation_param")]
        public class DragTestThing_CreateAnnotationNoParameter : UIElement {

            [OnDragCreate]
            public DragEvent CreateDrag() {
                return new TestDragEvent("from class");
            }

        }

        [Test]
        public void CreateAnnotationNoParameter() {
            MockApplication testView = MockApplication.Setup<DragTestThing_CreateAnnotationNoParameter>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));

            testView.InputSystem.MouseDown(new Vector2(20, 20));
            testView.Update();

            Assert.IsNull(testView.InputSystem.CurrentDragEvent);

            testView.InputSystem.MouseDragMove(new Vector2(25, 20));
            testView.Update();

            Assert.IsInstanceOf<TestDragEvent>(testView.InputSystem.CurrentDragEvent);
            Assert.AreEqual("from class", As<TestDragEvent>(testView.InputSystem.CurrentDragEvent).sourceName);
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_annotation_invalid_param")]
        public class DragTestThing_CreateAnnotationInvalidParameter : UIElement {

            [OnDragCreate]
            public DragEvent CreateDrag(int x) {
                return new TestDragEvent("from class");
            }

        }

        [Test]
        public void CreateAnnotationInvalidParameter() {
            CompileException exception = Assert.Throws<CompileException>(() => { MockApplication.Setup<DragTestThing_CreateAnnotationInvalidParameter>(); });
            Assert.IsTrue(
                exception.Message.Contains(CompileException.InvalidInputAnnotation("CreateDrag", typeof(DragTestThing_CreateAnnotationInvalidParameter), typeof(OnDragCreateAttribute), typeof(MouseInputEvent), typeof(int)).Message));
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_annotation_invalid_param_count")]
        public class DragTestThing_CreateAnnotationInvalidParameterCount : UIElement {

            [OnDragCreate]
            public DragEvent CreateDrag(MouseInputEvent evt, int x) {
                return new TestDragEvent("from class");
            }

        }

        [Test]
        public void CreateAnnotationInvalidParameterCount() {
            CompileException exception = Assert.Throws<CompileException>(() => { MockApplication.Setup<DragTestThing_CreateAnnotationInvalidParameterCount>(); });
            Assert.IsTrue(exception.Message.Contains(
                CompileException.TooManyInputAnnotationArguments("CreateDrag", typeof(DragTestThing_CreateAnnotationInvalidParameterCount), typeof(OnDragCreateAttribute), typeof(MouseInputEvent), 2)
                .Message));
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_annotation_invalid_return")]
        public class DragTestThing_CreateAnnotationInvalidReturn : UIElement {

            [OnDragCreate]
            public void CreateDrag(MouseInputEvent evt) { }

        }

        [Test]
        public void CreateAnnotationInvalidReturn() {
            CompileException exception = Assert.Throws<CompileException>(() => { MockApplication.Setup<DragTestThing_CreateAnnotationInvalidReturn>(); });
            Assert.IsTrue(exception.Message.Contains(CompileException.InvalidDragCreatorAnnotationReturnType("CreateDrag", typeof(DragTestThing_CreateAnnotationInvalidReturn), typeof(void)).Message));
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_annotation_null")]
        public class DragTestThing_CreateAnnotationNull : UIElement {

            public bool wasCalled;

            [OnDragCreate]
            public DragEvent CreateDrag(MouseInputEvent evt) {
                wasCalled = true;
                return null;
            }

        }

        [Test]
        public void CreateDragAnnotationNull() {
            MockApplication testView = MockApplication.Setup<DragTestThing_CreateAnnotationNull>();
            DragTestThing_CreateAnnotationNull root = testView.RootElement as DragTestThing_CreateAnnotationNull;

            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));

            testView.InputSystem.MouseDown(new Vector2(20, 20));
            testView.Update();

            Assert.IsNull(testView.InputSystem.CurrentDragEvent);

            testView.InputSystem.MouseDragMove(new Vector2(25, 20));
            testView.Update();

            Assert.IsNull(testView.InputSystem.CurrentDragEvent);
            Assert.IsTrue(root.wasCalled);
        }

        [Template("Data/DragEvents/DragEventTest_DragHierarchy.xml")]
        public class DragHandlerTestThing : UIElement {

            public LightList<string> dragList = new LightList<string>();
            public bool ignoreEnter;
            public bool ignoreExit;

            public void HandleDragEnterChild(UIElement el, int index) {
                if (ignoreEnter) return;
                dragList.Add("enter:child" + index);
            }

            public void HandleDragExitChild(UIElement el, int index) {
                if (ignoreExit) return;
                dragList.Add("exit:child" + index);
            }

            [OnDragCreate]
            public TestDragEvent OnDragCreate() {
                return new TestDragEvent("root");
            }

        }

        [Test]
        public void DragEnter_Fires() {
            MockApplication testView = MockApplication.Setup<DragHandlerTestThing>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
            DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement;
            testView.Update();

            Assert.AreEqual(0, root.dragList.Count);
            root.ignoreExit = true;
            testView.InputSystem.MouseDown(new Vector2(10, 10));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            Assert.AreEqual(new[] {"enter:child0"}, root.dragList.ToArray());

            testView.InputSystem.MouseDragMove(new Vector2(130, 30));
            testView.Update();

            Assert.AreEqual(2, root.dragList.Count);
            Assert.AreEqual(new[] {"enter:child0", "enter:child1"}, root.dragList.ToArray());
        }

        [Test]
        public void DragEnter_DoesNotFireAgainForSamePosition() {
            MockApplication testView = MockApplication.Setup<DragHandlerTestThing>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
            DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement;
            testView.Update();

            Assert.AreEqual(0, root.dragList.Count);
            root.ignoreExit = true;
            testView.InputSystem.MouseDown(new Vector2(10, 10));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            Assert.AreEqual(1, root.dragList.Count);
            Assert.AreEqual(new[] {"enter:child0"}, root.dragList.ToArray());
        }

        [Test]
        public void DragEnter_DoesNotFireAgainForPositionSameElement() {
            MockApplication testView = MockApplication.Setup<DragHandlerTestThing>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
            DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement;
            testView.Update();

            Assert.AreEqual(0, root.dragList.Count);
            root.ignoreExit = true;
            testView.InputSystem.MouseDown(new Vector2(10, 10));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            Assert.AreEqual(1, root.dragList.Count);
            Assert.AreEqual(new[] {"enter:child0"}, root.dragList.ToArray());
        }

        [Test]
        public void DragEnter_FiresForNewElement() {
            MockApplication testView = MockApplication.Setup<DragHandlerTestThing>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
            DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement;
            testView.Update();

            Assert.AreEqual(0, root.dragList.Count);
            root.ignoreExit = true;

            testView.InputSystem.MouseDown(new Vector2(10, 10));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(130, 30));
            testView.Update();

            Assert.AreEqual(2, root.dragList.Count);
            Assert.AreEqual(new[] {"enter:child0", "enter:child1"}, root.dragList.ToArray());
        }

        [Test]
        public void DragEnter_FiresForReEnteringElement() {
            MockApplication testView = MockApplication.Setup<DragHandlerTestThing>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
            DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement;

            Assert.AreEqual(0, root.dragList.Count);
            root.ignoreExit = true;

            testView.InputSystem.MouseDown(new Vector2(10, 10));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(130, 30));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            Assert.AreEqual(3, root.dragList.Count);
            Assert.AreEqual(new[] {"enter:child0", "enter:child1", "enter:child0"}, root.dragList.ToArray());
        }

        [Test]
        public void DragExit_FiresAndPropagates() {
            MockApplication testView = MockApplication.Setup<DragHandlerTestThing>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
            DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement;

            testView.InputSystem.MouseDown(new Vector2(10, 10));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            Assert.AreEqual(1, root.dragList.Count);
            Assert.AreEqual(new[] {"enter:child0"}, root.dragList.ToArray());

            testView.InputSystem.MouseDragMove(new Vector2(130, 30));
            testView.Update();

            Assert.AreEqual(3, root.dragList.Count);
            Assert.AreEqual(new[] {"enter:child0", "exit:child0", "enter:child1"}, root.dragList.ToArray());
        }

        [Test]
        public void DragExit_FireOnlyForExitedElement() {
            MockApplication testView = MockApplication.Setup<DragHandlerTestThing>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
            DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement;

            testView.InputSystem.MouseDown(new Vector2(10, 10));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            Assert.AreEqual(1, root.dragList.Count);
            Assert.AreEqual(new[] {"enter:child0"}, root.dragList.ToArray());

            testView.InputSystem.MouseDragMove(new Vector2(40, 30));
            testView.Update();

            Assert.AreEqual(1, root.dragList.Count);
            Assert.AreEqual(new[] {"enter:child0"}, root.dragList.ToArray());
        }

        [Test]
        public void DragExit_FireAgainWhenReenteredElement() {
            MockApplication testView = MockApplication.Setup<DragHandlerTestThing>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
            DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement;

            testView.InputSystem.MouseDown(new Vector2(10, 10));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(130, 30));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            Assert.AreEqual(5, root.dragList.Count);
            Assert.AreEqual(new[] {"enter:child0", "exit:child0", "enter:child1", "exit:child1", "enter:child0"}, root.dragList.ToArray());
        }

        [Template("Data/DragEvents/DragEventTest_DragHierarchy.xml#move")]
        public class DragHandlerTestThing_Move : UIElement {

            public LightList<string> dragList = new LightList<string>();

            public void HandleDragMoveChild(int index) {
                dragList.Add("move:child" + index);
            }

            public void HandleDragHoverChild(int index) {
                dragList.Add("hover:child" + index);
            }

            [OnDragCreate]
            public TestDragEvent OnDragCreate() {
                return new TestDragEvent("root");
            }

        }

        [Test]
        public void DragMove_FiresAndPropagates() {
            MockApplication testView = MockApplication.Setup<DragHandlerTestThing_Move>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
            DragHandlerTestThing_Move root = (DragHandlerTestThing_Move) testView.RootElement;

            testView.InputSystem.MouseDown(new Vector2(10, 10));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            Assert.AreEqual(new string[0], root.dragList.ToArray());

            testView.InputSystem.MouseDragMove(new Vector2(30, 20));
            testView.Update();

            Assert.AreEqual(new[] {"move:child0"}, root.dragList.ToArray());
        }

        [Test]
        public void DragMove_FiresAgainWhenMovedAndContains() {
            MockApplication testView = MockApplication.Setup<DragHandlerTestThing_Move>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
            DragHandlerTestThing_Move root = (DragHandlerTestThing_Move) testView.RootElement;

            testView.InputSystem.MouseDown(new Vector2(10, 10));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 20));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(31, 20));
            testView.Update();

            Assert.AreEqual(new[] {"move:child0", "move:child0"}, root.dragList.ToArray());
        }

        [Test]
        public void DragMove_DoesNotFireAgainWhenNotMovedAndContains() {
            MockApplication testView = MockApplication.Setup<DragHandlerTestThing_Move>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
            DragHandlerTestThing_Move root = (DragHandlerTestThing_Move) testView.RootElement;

            testView.InputSystem.MouseDown(new Vector2(10, 10));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(31, 20));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(31, 20));
            testView.Update();

            Assert.AreEqual(new[] {"move:child0", "hover:child0"}, root.dragList.ToArray());
        }

        [Template("Data/DragEvents/DragEventTest_DragHierarchy.xml#move_with_event")]
        public class DragHandlerTestThing_MoveWithDragEvent : UIElement {

            public LightList<string> dragList = new LightList<string>();

            public void HandleDragMoveChild(DragEvent evt, int index) {
                if (evt is TestDragEvent textEvt) dragList.Add($"move:child{index}:{textEvt.sourceName}");
            }

            public void HandleDragHoverChild(DragEvent evt, int index) {
                if (evt is TestDragEvent textEvt) dragList.Add($"hover:child{index}:{textEvt.sourceName}");
            }

            [OnDragCreate]
            public TestDragEvent OnDragCreate() {
                return new TestDragEvent("root");
            }

        }

        [Test]
        public void DragMove_FiresAndPropagatesWithDragEvent() {
            MockApplication testView = MockApplication.Setup<DragHandlerTestThing_MoveWithDragEvent>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
            DragHandlerTestThing_MoveWithDragEvent root = (DragHandlerTestThing_MoveWithDragEvent) testView.RootElement;

            testView.InputSystem.MouseDown(new Vector2(10, 10));
            testView.Update();

            testView.InputSystem.MouseDragMove(new Vector2(30, 30));
            testView.Update();

            Assert.AreEqual(new string[0], root.dragList.ToArray());

            testView.InputSystem.MouseDragMove(new Vector2(30, 20));
            testView.Update();

            Assert.AreEqual(new[] {"move:child0:root"}, root.dragList.ToArray());
        }

        public class DragElementWithSyncBinding : UIContainerElement {
            public float value;

            private Action<Vector2> onDragUpdate;

            public override void OnCreate() {
                onDragUpdate = OnDragUpdate;
            }

            private void OnDragUpdate(Vector2 position) {
                value = position.x;
            }

            public class TestDragEvent : DragEvent {

                public readonly Action<Vector2> dragUpdate;

                public TestDragEvent(Action<Vector2> dragUpdate) {
                    this.dragUpdate = dragUpdate;
                }

                public override void Update() {
                    dragUpdate?.Invoke(MousePosition);
                }
            }

            [OnDragCreate()]
            public DragEvent OnDragCreate() {
                return new TestDragEvent(onDragUpdate);
            }
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_and_change_sync_binding")]
        public class DragElementWithSyncBindingTestWrapper : UIElement {

            public float syncValue;

        }

        [Test]
        public void Drag_element_and_update_synced_value_while_not_hovering_draggeed_element() {
            MockApplication testView = MockApplication.Setup<DragElementWithSyncBindingTestWrapper>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
            DragElementWithSyncBindingTestWrapper root = (DragElementWithSyncBindingTestWrapper) testView.RootElement;

            testView.InputSystem.MouseDown(new Vector2(10, 10));
            testView.Update();

            // drag move the mouse over to the sibling element
            testView.InputSystem.MouseDragMove(new Vector2(20, 90));
            testView.Update();
            Assert.AreEqual(20, root.children[0].layoutResult.screenPosition.x);
            
            testView.InputSystem.MouseDragMove(new Vector2(31, 89));
            testView.Update();
            Assert.AreEqual(31, root.children[0].layoutResult.screenPosition.x);
            
            testView.InputSystem.MouseDragMove(new Vector2(30, 75));
            testView.Update();
            Assert.AreEqual(30, root.children[0].layoutResult.screenPosition.x);
            
            testView.InputSystem.MouseDragMove(new Vector2(400, 120));
            testView.Update();
            Assert.AreEqual(400, root.children[0].layoutResult.screenPosition.x);
        }

    }

}