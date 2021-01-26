using System;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    // style.Material.SetInstanceFloat("name", value);
    
    // string -> annoying / slow to re-parse
    // struct/id -> annoying to set
    
    // style.Material = new MaterialRef("glitch", MaterialFloat("shake", 34));
    
    // style.Material = MaterialRef.Set(style.Material, Float("shake", 34));
    
    // MaterialRef.Create("name");
    // clean up sucks w/ pointer version in current uiforia
    // but object would be fine I think, can convert to pointer & watch it in upgrade
    // still dont know how to handle nested properties 
    
    // <Element style:Material=" 'glitch { val: 34 } '"/>
    
    // <Element style.hover:Material.glitch.shake="3"/>
    
    // <Element style:material="glitch" material.hover:glitch.shake="5"/>
    
    // [keyframes] { [material:glitch] { shake = 5; } }
    
    // style.Material = Application.MaterialRenderData("name", new MaterialProperty() {
    //    
    // });
    
    // element.style.SetMaterial(MakeMaterial("glitch", new FloatProperty("name", value));
    
    // 3 levels
    //     base material (with properties set via import)
    //     style overrides (with properties mixed between import & style)
    //     instance overrides (with properties mixed between import & style & instance)
    
    // so style sheet will create material entries
    // 
    
    // element.style.SetMaterialTexture("glitch", "name", new TextureProperty(), StyleState.Hover);
    // materialDatabase
    // set material name to use
    // then override properties
    // property overrides are stored per-element in a map
    // kill map entry when element destroyed
    // need template compiler to handle proper code gen for this
    // need render system to know if an override material is in use
    // then lookup per element property overrides for that material


    public struct PolyRect {

        public Vector2 p0;
        public Vector2 p1;
        public Vector2 p2;
        public Vector2 p3;

        public PolyRect(in Vector2 p0, in Vector2 p1, in Vector2 p2, in Vector2 p3) {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

    }

    public struct RenderBounds {

        public float xMin;
        public float yMin;
        public float xMax;
        public float yMax;

        public RenderBounds(float xMin, float yMin, float xMax, float yMax) {
            this.xMin = xMin;
            this.yMin = yMin;
            this.xMax = xMax;
            this.yMax = yMax;
        }

    }

    public abstract class RenderBox {

        internal string uniqueId;

        protected internal UIElement element;

        public float opacity;
        public Visibility visibility;
        public Overflow overflowX;
        public Overflow overflowY;
        public ClipBehavior clipBehavior;
        public bool culled;
        public bool hasForeground;
        public int zIndex;
        public int layer;
        protected Path2D clipPath;
        internal ClipData clipper;
        public bool didRender;
        public int traversalIndex;
        public int scrollFix;

        public virtual RenderBounds RenderBounds {
            get {
                switch (element.style.ClipBounds) { // todo -- cache this
                    case ClipBounds.ContentBox: {
                        OffsetRect border = element.layoutResult.border;
                        OffsetRect padding = element.layoutResult.padding;
                        return new RenderBounds(
                            border.left + padding.left,
                            border.top + padding.top,
                            element.layoutResult.actualSize.width - border.right - padding.right,
                            element.layoutResult.actualSize.height - border.bottom - padding.bottom
                        );
                    }
                    case ClipBounds.BorderBox:
                        return new RenderBounds(0, 0, element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);
                }

                return new RenderBounds(0, 0, element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);
            }
        }

        public virtual void OnInitialize() {
            overflowX = element.style.OverflowX;
            overflowY = element.style.OverflowY;
            zIndex = element.style.ZIndex;
            layer = element.style.Layer;
            clipBehavior = element.style.ClipBehavior;
            visibility = element.style.Visibility;
            scrollFix = element.style.ScrollBehaviorX == ScrollBehavior.Fixed || element.style.ScrollBehaviorY == ScrollBehavior.Fixed ? 1 : 0;
        }

        public virtual void OnDestroy() { }


        public virtual void OnStylePropertyChanged(StructList<StyleProperty> propertyList) {
            for (int i = 0; i < propertyList.size; i++) {
                ref StyleProperty property = ref propertyList.array[i];
                switch (property.propertyId) {
                    case StylePropertyId.Material:
                        // materialInfo = property.AsString;
                        break;
                    case StylePropertyId.OverflowX:
                        overflowX = property.AsOverflow;
                        break;
                    case StylePropertyId.OverflowY:
                        overflowY = property.AsOverflow;
                        break;
                    case StylePropertyId.ZIndex:
                        zIndex = property.AsInt;
                        break;
                    case StylePropertyId.Layer:
                        layer = property.AsInt;
                        break;
                    case StylePropertyId.ClipBehavior:
                        clipBehavior = property.AsClipBehavior;
                        break;
                    case StylePropertyId.Visibility:
                        visibility = property.AsVisibility;
                        break;
                }
            }
        }

        public abstract void PaintBackground(RenderContext ctx);

        public virtual void PaintForeground(RenderContext ctx) { }

        public static float ResolveFixedSize(UIElement element, float baseSize, UIFixedLength length) {
            switch (length.unit) {
                case UIFixedUnit.Unset:
                case UIFixedUnit.Pixel:
                    return length.value;
                case UIFixedUnit.Percent:
                    return baseSize * length.value;
                case UIFixedUnit.Em:
                    return element.style.GetResolvedFontSize() * length.value;
                case UIFixedUnit.ViewportWidth:
                    return element.View.Viewport.width * length.value;
                case UIFixedUnit.ViewportHeight:
                    return element.View.Viewport.height * length.value;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Path2D GetClipPathFromElement() {
            Size size = element.layoutResult.actualSize;
            float elementWidth = size.width;
            float elementHeight = size.height;
            float min = Mathf.Min(elementWidth, elementHeight);

            if (element is UITextElement) {
                return null;
            }

            float bevelTopLeft = ResolveFixedSize(element, min, element.style.CornerBevelTopLeft);
            float bevelTopRight = ResolveFixedSize(element, min, element.style.CornerBevelTopRight);
            float bevelBottomRight = ResolveFixedSize(element, min, element.style.CornerBevelBottomRight);
            float bevelBottomLeft = ResolveFixedSize(element, min, element.style.CornerBevelBottomLeft);

            float radiusTopLeft = ResolveFixedSize(element, min, element.style.BorderRadiusTopLeft);
            float radiusTopRight = ResolveFixedSize(element, min, element.style.BorderRadiusTopRight);
            float radiusBottomRight = ResolveFixedSize(element, min, element.style.BorderRadiusBottomRight);
            float radiusBottomLeft = ResolveFixedSize(element, min, element.style.BorderRadiusBottomLeft);

            if (radiusBottomLeft > 0 ||
                radiusBottomRight > 0 ||
                radiusTopLeft > 0 ||
                radiusTopRight > 0 ||
                bevelTopRight > 0 ||
                bevelTopLeft > 0 ||
                bevelBottomLeft > 0 ||
                bevelBottomRight > 0) {
                // todo -- decorated rect w/ cut

                // todo -- if padding or border box would be larger enough to ignore cut / radius we can return null here also
                if (element.layoutResult.padding.top > 0 &&
                    element.layoutResult.padding.bottom > 0 &&
                    element.layoutResult.padding.right > 0 &&
                    element.layoutResult.padding.left > 0) {
                    return null;
                }

                clipPath = clipPath ?? new Path2D();
                clipPath.Clear(); // todo -- only clear if changed    

                clipPath.BeginPath();
                clipPath.SetFill(Color.white);
                clipPath.RoundedRect(0, 0, size.width, size.height, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft);
                clipPath.Fill();
                return clipPath;
            }
            else {
                return null;
            }
        }

        public virtual Path2D GetClipShape() {
            return GetClipPathFromElement();
        }

        public virtual void Enable() { }

    }

}