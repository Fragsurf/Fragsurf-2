using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Src.Systems {

    public class RenderOwner {

        internal UIView view;
        internal readonly RenderBoxPool painterPool;
        private readonly StructStack<ElemRef> elemRefStack;
        private readonly StructList<RenderOperationWrapper> wrapperList;

        private static readonly DepthComparer2 s_RenderComparer = new DepthComparer2();

        public RenderOwner(UIView view, Camera camera) {
            this.view = view;
            this.painterPool = new RenderBoxPool();
            this.view.dummyRoot.renderBox = new RootRenderBox();
            this.view.dummyRoot.renderBox.element = view.dummyRoot;
            this.elemRefStack = new StructStack<ElemRef>(32);
            this.wrapperList = new StructList<RenderOperationWrapper>(32);
        }

        public void Render(RenderContext renderContext) {
            GatherBoxDataParallel();

            DrawClipShapes(renderContext);

            Draw(renderContext);

//            drawList.QuickClear();
        }

        private void DrawClipShapes(RenderContext ctx) {
            LightList<ClipData> clippers = view.application.LayoutSystem.GetLayoutRunner(view.dummyRoot).clipperList;
            for (int i = 0; i < clippers.size; i++) {
                ClipData clipData = clippers.array[i];
                if (!clipData.isCulled && clipData.visibleBoxCount > 0) {
                    clipData.clipPath = clipData.renderBox?.GetClipShape();
                    ctx.DrawClipData(clipData);
                }
            }
        }

        private void UpdateRenderBox(UIElement element) {
            // get painter
            // see if it the same as current render box
            // if not destroy, create
            // finally, update styles
            element.renderBox.OnDestroy();
            element.renderBox = null;
            // todo -- pool
            CreateRenderBox(element);
        }

        public void CreateRenderBox(UIElement element) {
            string painterId = element.style.Painter;
            RenderBox painter = null;

            if (painterId == null) {
                if (element is UITextElement) {
                    painter = new TextRenderBox();
                }
                else {
                    painter = new StandardRenderBox();
                }
            }
            else {
                painter = painterPool.GetCustomPainter(painterId);
                if (painter == null) {
                    if (element is UITextElement) {
                        painter = new TextRenderBox();
                    }
                    else {
                        painter = new StandardRenderBox();
                    }
                }
            }

            painter.element = element;
            painter.visibility = element.style.Visibility;
            painter.uniqueId = painterId;
            painter.OnInitialize();
            if (element.renderBox != null) {
                // todo -- pool
                element.renderBox.OnDestroy();
                element.renderBox.element = null;
            }

            element.renderBox = painter;
        }


        private struct RenderOperationWrapper {

            public RenderBox renderBox;
            public DrawCommandType renderOperation;

            public RenderOperationWrapper(RenderBox renderBox, DrawCommandType renderOpType) {
                this.renderBox = renderBox;
                this.renderOperation = renderOpType;
            }

        }

        // this is intended to be run while layout is running (ie in parallel)
        public void GatherBoxDataParallel() {
            UIElement root = view.dummyRoot;

            int frameId = root.application.frameId;

            int idx = 0;

            wrapperList.QuickClear();

            elemRefStack.array[elemRefStack.size++].element = root;

            while (elemRefStack.size > 0) {
                UIElement currentElement = elemRefStack.array[--elemRefStack.size].element;
                RenderBox renderBox = currentElement.renderBox;

                renderBox.culled = renderBox.element.layoutResult.isCulled;
                renderBox.clipper = currentElement.layoutResult.clipper;
                renderBox.traversalIndex = idx++;

                if (wrapperList.size + 2 + (currentElement.children.size * 2) >= wrapperList.array.Length) {
                    wrapperList.EnsureAdditionalCapacity(2 + (currentElement.children.size * 2));
                }

                if (elemRefStack.size + 2 + currentElement.children.size >= elemRefStack.array.Length) {
                    elemRefStack.EnsureAdditionalCapacity(2 + (currentElement.children.size * 2));
                }

                if (!renderBox.culled && renderBox.visibility != Visibility.Hidden) {
                    
                    ref RenderOperationWrapper backgroundOp = ref wrapperList.array[wrapperList.size++];
                    backgroundOp.renderBox = renderBox;
                    backgroundOp.renderOperation = DrawCommandType.BackgroundTransparent;

                    if (renderBox.hasForeground) {
                        ref RenderOperationWrapper foreground = ref wrapperList.array[wrapperList.size++];
                        foreground.renderBox = renderBox;
                        foreground.renderOperation = DrawCommandType.ForegroundTransparent;
                    }
                    
                }

                for (int i = currentElement.children.size - 1; i >= 0; i--) {
                    UIElement child = currentElement.children.array[i];
                    if ((child.flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet) {
                        // todo change check on painter
                        if (child.renderBox == null) {
                            CreateRenderBox(child);
                            Debug.Assert(child.renderBox != null, "child.renderBox != null");
                            child.renderBox.Enable();
                        }
                        else if (child.enableStateChangedFrameId == frameId) {
                            UpdateRenderBox(child);
                            child.renderBox.Enable();
                        }

                        if (child.renderBox.scrollFix == 1) {
                            elemRefStack.array[elemRefStack.size++].element = child;
                        }
                    }

                }

                
                for (int i = currentElement.children.size - 1; i >= 0; i--) {
                    UIElement child = currentElement.children.array[i];
                    if ((child.flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet) {
                        if (child.renderBox.scrollFix == 0) {
                            elemRefStack.array[elemRefStack.size++].element = child;
                        }
                    }

                }
            }

            
            RegularSortList();
//            BubbleSortWrapperList();


//            if (!printed) {
//                printed = true;
//                for (int i = 0; i < wrapperList.size; i++) {
//                    Debug.Log(wrapperList.array[i].element + " -- " + (wrapperList.array[i].renderOp));
//                }
//            }
        }

//        private bool printed = false; // todo remove

        private void RegularSortList() {
            wrapperList.Sort(s_RenderComparer);
        }

        private void BubbleSortWrapperList() {
            bool keepIterating = true;

            int count = wrapperList.size - 1;
            RenderOperationWrapper[] array = wrapperList.array;

            while (keepIterating) {
                keepIterating = false;
                for (int i = 0; i < count; i++) {
                    ref RenderOperationWrapper x = ref array[i];
                    ref RenderOperationWrapper y = ref array[i + 1];

                    int val = 0;
                    RenderBox rbA = x.renderBox;
                    RenderBox rbB = y.renderBox;

                    if (x.renderOperation != y.renderOperation) {
                        val = (int) x.renderOperation - (int) y.renderOperation;
                    }
                    else if (rbA.layer != rbB.layer) {
                        val = rbA.layer - rbB.layer;
                    }
                    else if (rbA.zIndex != rbB.zIndex) {
                        val = rbA.zIndex - rbB.zIndex;
                    }
                    else {
                        val = rbA.traversalIndex - rbB.traversalIndex;
                    }

                    if (val > 0) {
                        array[i] = y;
                        array[i + 1] = x;
                        keepIterating = true;
                    }
                }
            }
        }

//        // todo -- can completely get rid of this
//        private void Cull() {
//            // first do an easy screen cull
//            // screen is always aligned
//            // if world space rect is not inside the screen, fail immediately
//
//            for (int i = 0; i < wrapperList.size; i++) {
//                ref RenderOperationWrapper wrapper = ref wrapperList.array[i];
//
//                RenderBox renderBox = wrapper.renderBox;
//
//                switch (wrapper.renderOperation) {
//                    default:
//                    case RenderOpType.Unset:
//                        break;
//
//                    case RenderOpType.DrawBackground: {
//                        renderBox.culled = renderBox.element.layoutResult.isCulled;
//
//                        if (!renderBox.culled && renderBox.visibility != Visibility.Hidden) {
//                            drawList.Add(new DrawCommand(renderBox, DrawCommandType.BackgroundTransparent));
//                        }
//
//                        break;
//                    }
//
//                    case RenderOpType.DrawForeground:
//
//                        if (!renderBox.culled) {
//                            drawList.Add(new DrawCommand(renderBox, DrawCommandType.ForegroundTransparent));
//                        }
//
//                        break;
//
//                    case RenderOpType.PushClipShape: {
//                        break;
//                    }
//
//                    case RenderOpType.PopClipShape: {
//                        break;
//                    }
//                    case RenderOpType.PushPostEffect:
//                        break;
//
//                    case RenderOpType.PopPostEffect:
//                        break;
//                }
//            }
//        }

        private void Draw(RenderContext renderContext) {
            RenderOperationWrapper[] commands = wrapperList.array;
            int commandCount = wrapperList.size;

            // bad api usage, fix this while supporting on the fly clipper creation
            renderContext.clipContext.ConstructClipData();

            for (int i = 0; i < commandCount; i++) {
                ref RenderOperationWrapper cmd = ref commands[i];

                switch (cmd.renderOperation) {
                    case DrawCommandType.BackgroundTransparent:
                        cmd.renderBox.PaintBackground(renderContext);
                        break;

                    case DrawCommandType.ForegroundTransparent:
                        cmd.renderBox.PaintForeground(renderContext);
                        break;

                    case DrawCommandType.BackgroundOpaque:
                        throw new NotImplementedException();

                    case DrawCommandType.ForegroundOpaque:
                        throw new NotImplementedException();

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public enum DrawCommandType {

            BackgroundTransparent,
            ForegroundTransparent,
            BackgroundOpaque,
            ForegroundOpaque,

        }

        public struct DrawCommand {

            public RenderBox renderBox;
            public DrawCommandType commandType;

            public DrawCommand(RenderBox box, DrawCommandType commandType) {
                this.renderBox = box;
                this.commandType = commandType;
            }

        }

        internal struct RenderBoxWrapper {

            public RenderOpType renderOp;
            public UIElement element;
            public RenderBox renderBox;
            public int layer;
            public int zIndex;
            public int traversalIndex;

            public RenderBoxWrapper(UIElement element) {
                this.renderOp = RenderOpType.DrawBackground;
                this.element = element;
                this.renderBox = element.renderBox;
                this.zIndex = renderBox.zIndex;
                this.traversalIndex = -1;
                this.layer = renderBox.layer;
            }

            public RenderBoxWrapper(RenderBoxWrapper wrapper, RenderOpType renderOperation) {
                this.renderOp = renderOperation;
                this.element = wrapper.element;
                this.renderBox = element.renderBox;
                this.zIndex = wrapper.zIndex;
                this.layer = wrapper.layer;
                this.traversalIndex = -1;
            }

        }

        public enum RenderOpType {

            Unset = 0,
            DrawBackground = 1,
            DrawForeground = 2,
            PushClipShape = 3,
            PopClipShape = 4,
            PushPostEffect = 5,
            PopPostEffect = 6,

        }

        private class DepthComparer2 : IComparer<RenderOperationWrapper> {


            public int Compare(RenderOperationWrapper a, RenderOperationWrapper b) {
                RenderBox rbA = a.renderBox;
                RenderBox rbB = b.renderBox;

                if (a.renderOperation != b.renderOperation) {
                    return (int) a.renderOperation - (int) b.renderOperation;
                }

                if (rbA.layer != rbB.layer) {
                    return rbA.layer - rbB.layer;
                }

                // view might be a layer
//                if (rbA.viewDepthIdx != rbB.viewDepthIdx) {
//                    return rbA.viewDepthIdx > rbB.viewDepthIdx ? -1 : 1;
//                }

                if (rbA.zIndex != rbB.zIndex) {
                    return rbA.zIndex - rbB.zIndex;
                }

                return rbA.traversalIndex - rbB.traversalIndex;
            }

        }

        public void Destroy() { }

    }

}