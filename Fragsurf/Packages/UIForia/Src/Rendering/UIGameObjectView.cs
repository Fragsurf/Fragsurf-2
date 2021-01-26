//using System;
//using UIForia.Systems;
//using UnityEngine;
//
//namespace UIForia.Rendering {
//
//    public sealed class UIGameObjectView : UIView {
//
//        private readonly RectTransform rectTransform;
//        
//        public UIGameObjectView(Type elementType, RectTransform viewTransform) : base(elementType) {
//            this.rectTransform = viewTransform;
//            layoutSystem = new LayoutSystem(styleSystem);
//            renderSystem = new RenderSystem(Camera.main, layoutSystem, styleSystem);
//            inputSystem = new GOInputSystem(layoutSystem, styleSystem);
//            systems.Add(inputSystem);
//            systems.Add(layoutSystem);
//            systems.Add(renderSystem);
//        }
//
//        public override void Update() {
//            Rect viewport = rectTransform.rect;
//            viewport.y = viewport.height + viewport.y;
//            layoutSystem.SetViewportRect(viewport);
//            styleSystem.SetViewportRect(viewport);
//            base.Update();
//        }
//
//    }
//
//}