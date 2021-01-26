using System;
using SVGX;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;
using Vertigo;
using Application = UIForia.Application;

namespace Src.Systems {

    public class RenderBoxPool {

        // todo -- this doesn't actually pool right now
        public RenderBox GetCustomPainter(string painterId) {

            if (painterId == "self") {
                return new SelfPaintedRenderBox();
            }

            if (Application.s_CustomPainters.TryGetValue(painterId, out Type boxType)) {
                return (RenderBox) Activator.CreateInstance(boxType);
            }

            return null;
        }

    }

    public class VertigoRenderSystem : IRenderSystem {

        private Camera camera;
        private CommandBuffer commandBuffer;
        private RenderContext renderContext;
        internal LightList<RenderOwner> renderOwners;

        public VertigoRenderSystem(Camera camera, Application application) {
            this.camera = camera;
            this.commandBuffer = new CommandBuffer(); // todo -- per view
            this.commandBuffer.name = "UIForia Main Command Buffer";
            this.renderContext = new RenderContext(application.settings);
            this.renderOwners = new LightList<RenderOwner>();

            if (this.camera != null) {
                this.camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
            }

            application.StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
            application.onViewsSorted += uiViews => {
                renderOwners.Sort((o1, o2) => o1.view.Depth.CompareTo(o2.view.Depth));
            };
        }

        private void HandleStylePropertyChanged(UIElement element, StructList<StyleProperty> propertyList) {
            if (element.renderBox == null) return;

            int count = propertyList.size;
            StyleProperty[] properties = propertyList.array;

            for (int i = 0; i < count; i++) {
                ref StyleProperty property = ref properties[i];
                switch (property.propertyId) {
                    case StylePropertyId.Painter:
                        ReplaceRenderBox(element, property.AsString);
                        break;
                }
            }

            element.renderBox.OnStylePropertyChanged(propertyList);
        }

        private void ReplaceRenderBox(UIElement element, string painterId) {
            renderOwners[0].CreateRenderBox(element);
        }

        public event Action<RenderContext> DrawDebugOverlay2;

        public void OnReset() {
            commandBuffer.Clear();
            for (int i = 0; i < renderOwners.size; i++) {
                renderOwners[i].Destroy();
            }
            renderOwners.QuickClear();
            renderContext.clipContext.Destroy();
            renderContext.clipContext = new ClipContext(Application.Settings);
        }

        public virtual void OnUpdate() {
            renderContext.Clear();

            // todo
            // views can have their own cameras.
            // if they do they are not batchable with other views.
            // for now we can make batching not cross view boundaries, eventually that would be cool though

            camera.orthographicSize = Screen.height * 0.5f;

            for (int i = 0; i < renderOwners.size; i++) {
                renderOwners.array[i].Render(renderContext);
            }

            DrawDebugOverlay2?.Invoke(renderContext);
            renderContext.Render(camera, commandBuffer);
        }

        public void OnDestroy() {
            commandBuffer.Clear();
            renderContext.clipContext.Destroy();
            renderContext.Destroy();
        }

        public void OnViewAdded(UIView view) {
            renderOwners.Add(new RenderOwner(view, camera));
        }

        public void OnViewRemoved(UIView view) {
            for (int i = 0; i < renderOwners.size; i++) {
                if (renderOwners.array[i].view == view) {
                    renderOwners.RemoveAt(i);
                    return;
                }
            }
        }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        public void OnElementCreated(UIElement element) { }

        public void SetCamera(Camera camera) {
            if (this.camera != null) {
                this.camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
            }

            this.camera = camera; // todo -- should be handled by the view

            if (this.camera != null) {
                this.camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
            }
        }

        public RenderContext GetRenderContext()
        {
            return renderContext;
        }
    }

    internal struct SVGXFillStyle {

        public PaintMode paintMode;
        public float encodedColor;
        public Texture texture;
        public SVGXMatrix uvTransform;
        public float opacity;
        public float encodedTint;
        public int gradientId;
        public Color32 shadowColor;
        public float shadowIntensity;
        public float shadowOffsetX;
        public float shadowOffsetY;
        public float shadowSizeX;
        public float shadowSizeY;
        public Color32 shadowTint;
        public float shadowOpacity;

        public static SVGXFillStyle Default => new SVGXFillStyle() {
            paintMode = PaintMode.Color,
            encodedColor = VertigoUtil.ColorToFloat(Color.black),
            texture = null,
            uvTransform = SVGXMatrix.identity,
            opacity = 1f,
            encodedTint = VertigoUtil.ColorToFloat(Color.clear),
            gradientId = -1,
            shadowOpacity = 1
        };

    }

    internal struct SVGXStrokeStyle {

        public PaintMode paintMode;
        public float encodedColor;
        public Texture texture;
        public SVGXMatrix uvTransform;
        public float opacity;
        public float encodedTint;
        public int gradientId;
        public float strokeWidth;
        public Vertigo.LineJoin lineJoin;
        public Vertigo.LineCap lineCap;
        public float miterLimit;

        public static SVGXStrokeStyle Default => new SVGXStrokeStyle() {
            paintMode = PaintMode.Color,
            encodedColor = VertigoUtil.ColorToFloat(Color.black),
            texture = null,
            uvTransform = SVGXMatrix.identity,
            opacity = 1f,
            encodedTint = VertigoUtil.ColorToFloat(Color.clear),
            gradientId = -1,
            lineJoin = Vertigo.LineJoin.Miter,
            lineCap = Vertigo.LineCap.Butt,
            miterLimit = 10f
        };

    }

}