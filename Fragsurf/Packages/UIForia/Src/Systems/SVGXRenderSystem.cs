// using System;
// using SVGX;
// using UIForia.Elements;
// using UIForia.Extensions;
// using UIForia.Layout;
// using UIForia.Rendering;
// using UIForia.Util;
// using UnityEngine;
//
// namespace UIForia.Systems {
//
//     public enum BorderType {
//
//         None = 0,
//         UniformNormal,
//         UniformRounded,
//         VaryingNormal,
//         VaryingRounded,
//
//     }
//
//     public class SVGXRenderSystem : IRenderSystem {
//
//         private readonly ImmediateRenderContext ctx;
//         private readonly GFX gfx;
//         private Camera m_Camera;
//         private LightList<UIView> views;
//         private ILayoutSystem layoutSystem;
//
//         public SVGXRenderSystem(Application application, Camera camera, ILayoutSystem layoutSystem) {
//             application.onViewsSorted += uiViews => {
//                 views.Clear();
//                 views.AddRange(uiViews);
//             };
//             this.m_Camera = camera;
//             gfx = new GFX(camera);
//             ctx = new ImmediateRenderContext();
//             this.views = new LightList<UIView>();
//             this.layoutSystem = layoutSystem;
//         }
//
//         public void OnReset() {
//             this.views.Clear();
//         }
//
//         public void OnUpdate() {
//             ctx.Clear();
//             
//
//             for (int i = 0; i < views.Count; i++) {
//                 RenderView(views[i]);
//             }
//
//             gfx.Render(ctx);
//         }
//         public event Action<RenderContext> DrawDebugOverlay2;
//
//         private static void DrawNormalFill(ImmediateRenderContext ctx, UIElement element) {
//             Color bgColor = element.style.BackgroundColor;
//             Texture2D bgImage = element.style.BackgroundImage;
//
//             if (bgColor.IsDefined() && bgImage != null) {
//                 ctx.SetFill(bgImage, bgColor);
//                 ctx.Fill();
//             }
//             else if (bgColor.IsDefined()) {
//                 ctx.SetFill(bgColor);
//                 ctx.Fill();
//             }
//             else if (bgImage != null) {
//                 ctx.SetFill(bgImage);
//                 ctx.Fill();
//             }
//         }
//
//         private void RenderView(UIView view) {
//             m_Camera.orthographic = true;
//             m_Camera.orthographicSize = Screen.height * 0.5f;
//         }
//
//         public static void PaintElement(ImmediateRenderContext ctx, UIElement current) {
//             ctx.BeginPath();
//
//             LayoutResult layoutResult = current.layoutResult;
//             OffsetRect borderRect = layoutResult.border;
//
//             Vector4 border = layoutResult.border;
//             Vector4 resolveBorderRadius = layoutResult.borderRadius;
//             float width = layoutResult.actualSize.width;
//             float height = layoutResult.actualSize.height;
//             bool hasUniformBorder = border.x == border.y && border.z == border.x && border.w == border.x;
//             bool hasBorder = border.x > 0 || border.y > 0 || border.z > 0 || border.w > 0;
//
//             ctx.EnableScissorRect(current.layoutResult.clipRect);
//
//             ctx.SetFillOpacity(current.style.Opacity);
//             ctx.SetStrokeOpacity(current.style.Opacity);
//             if (resolveBorderRadius == Vector4.zero) {
//                 ctx.Rect(borderRect.left, borderRect.top, width - borderRect.Horizontal, height - borderRect.Vertical);
//
//                 if (!hasBorder) {
//                     DrawNormalFill(ctx, current);
//                 }
//                 else {
//                     if (hasUniformBorder) {
//                         DrawNormalFill(ctx, current);
//                         ctx.SetStrokePlacement(StrokePlacement.Outside);
//                         ctx.SetStrokeWidth(border.x);
//                         ctx.SetStroke(current.style.BorderColorTop);
//                         ctx.Stroke();
//                     }
//                     else {
//                         DrawNormalFill(ctx, current);
//
//                         ctx.SetStrokeOpacity(1f);
//                         ctx.SetStrokePlacement(StrokePlacement.Inside);
//      
//
//                         // todo this isn't really working correctly,
//                         // compute single stroke path on cpu. current implementation has weird blending overlap artifacts with transparent border color
//
//                         if (borderRect.top > 0) {
//                             ctx.BeginPath();
//                             ctx.SetStroke(current.style.BorderColorTop);
//                             ctx.SetFill(current.style.BorderColorTop);
//                             ctx.Rect(borderRect.left, 0, width - borderRect.Horizontal, borderRect.top);
//                             ctx.Fill();
//                         }
//
//                         if (borderRect.right > 0) {
//                             ctx.BeginPath();
//                             ctx.SetStroke(current.style.BorderColorRight);
//                             ctx.SetFill(current.style.BorderColorRight);
//                             ctx.Rect(width - borderRect.right, 0, borderRect.right, height);
//                             ctx.Fill();
//                         }
//
//                         if (borderRect.left > 0) {
//                             ctx.BeginPath();
//                             ctx.SetStroke(current.style.BorderColorLeft);
//                             ctx.SetFill(current.style.BorderColorLeft);
//                             ctx.Rect(0, 0, borderRect.left, height);
//                             ctx.Fill();
//                         }
//
//                         if (borderRect.bottom > 0) {
//                             ctx.BeginPath();
//                             ctx.SetStroke(current.style.BorderColorBottom);
//                             ctx.SetFill(current.style.BorderColorBottom);
//                             ctx.Rect(borderRect.left, height - borderRect.bottom, width - borderRect.Horizontal, borderRect.bottom);
//                             ctx.Fill();
//                         }
//                     }
//                 }
//             }
//             // todo -- might need to special case non uniform border with border radius
//             else {
//                 ctx.BeginPath();
//                 ctx.RoundedRect(new Rect(borderRect.left, borderRect.top, width - borderRect.Horizontal, height - borderRect.Vertical), resolveBorderRadius.x, resolveBorderRadius.y, resolveBorderRadius.z, resolveBorderRadius.w);
//                 DrawNormalFill(ctx, current);
//                 if (hasBorder) {
//                     ctx.SetStrokeWidth(borderRect.top);
//                     ctx.SetStroke(current.style.BorderColorTop);
//                     ctx.Stroke();
//                 }
//             }
//         }
//
//         public void OnDestroy() { }
//
//         public void OnViewAdded(UIView view) {
//             views.Add(view);
//             views.Sort((a, b) => a.Depth < b.Depth ? -1 : 1);
//         }
//
//         public void OnViewRemoved(UIView view) {
//             views.Remove(view);
//         }
//
//         public void OnElementEnabled(UIElement element) { }
//
//         public void OnElementDisabled(UIElement element) { }
//
//         public void OnElementDestroyed(UIElement element) { }
//
//         public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }
//
//         public void OnElementCreated(UIElement element) { }
//
//         public event Action<ImmediateRenderContext> DrawDebugOverlay;
//
//         public void SetCamera(Camera camera) {
//             if (!camera.orthographic) {
//                 throw new Exception("The camera used to render the UI must be marked as orthographic");
//             }
//
//             if (camera.farClipPlane < 50000) {
//                 Debug.LogWarning("The camera used to render the UI should have a far clip plane set to at least 50000");
//             }
//
//             m_Camera = camera;
//             gfx.SetCamera(camera);
//         }
//
//     }
//
// }