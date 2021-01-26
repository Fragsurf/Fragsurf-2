using System.Collections.Generic;
using System.Linq;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;
using Vertigo;
using ShapeType = Vertigo.ShapeType;

namespace UIForia.Rendering {

//    public class Polygon {
//
//        public StructList<Vector2> pointList;
//
//        public Polygon() {
//            this.pointList = new StructList<Vector2>();
//        }
//
//        public Polygon Clip(Polygon subject) {
//            Polygon retn = new Polygon();
//            retn.pointList = new StructList<Vector2>();
//            SutherlandHodgman.GetIntersectedPolygon(subject.pointList, pointList, ref retn.pointList);
//            return retn;
//        }
//
//        public Rect GetBounds() {
//            float minX = float.MaxValue;
//            float minY = float.MaxValue;
//            float maxX = float.MinValue;
//            float maxY = float.MinValue;
//            for (int i = 0; i < pointList.size; i++) {
//                Vector2 point = pointList.array[i];
//                if (point.x < minX) minX = point.x;
//                if (point.x > maxX) maxX = point.x;
//                if (point.y < minY) minY = point.y;
//                if (point.y > maxY) maxY = point.y;
//            }
//
//            return new Rect(minX, minY, maxX - minX, maxY - minY);
//        }
//
//        public void Rotate(float angle) {
//            Rect bounds = GetBounds();
//            Vector2 pivot = bounds.center;
//
//            for (int i = 0; i < pointList.size; i++) {
//                pointList[i] = pointList[i].Rotate(pivot, angle);
//            }
//        }
//
//        public Polygon GetScreenRect() {
//            Polygon retn = new Polygon();
//            Rect bounds = GetBounds();
//            retn.pointList = new StructList<Vector2>();
//            retn.pointList.Add(new Vector2(bounds.x, bounds.y));
//            retn.pointList.Add(new Vector2(bounds.xMax, bounds.y));
//            retn.pointList.Add(new Vector2(bounds.xMax, bounds.yMax));
//            retn.pointList.Add(new Vector2(bounds.x, bounds.yMax));
//            return retn;
//        }
//
//    }

    public class ClipShape {

        public Texture texture;
        public Path2D path;
        
        public ClipShape() {
            this.path = new Path2D();
        }

        public virtual bool ShouldCull(in Bounds bounds) {
            return false;
        }

        public void SetFromElement(UIElement element) {
            path.Clear();

            Size size = element.layoutResult.actualSize;

            path.SetFill(Color.white);
            
            float elementWidth = size.width;
            float elementHeight = size.height;
            float min = Mathf.Min(elementWidth, elementHeight);

            float bevelTopLeft = RenderBox.ResolveFixedSize(element, min, element.style.CornerBevelTopLeft);
            float bevelTopRight = RenderBox.ResolveFixedSize(element, min, element.style.CornerBevelTopRight);
            float bevelBottomRight = RenderBox.ResolveFixedSize(element, min, element.style.CornerBevelBottomRight);
            float bevelBottomLeft = RenderBox.ResolveFixedSize(element, min, element.style.CornerBevelBottomLeft);

            float radiusTopLeft = RenderBox.ResolveFixedSize(element, min, element.style.BorderRadiusTopLeft);
            float radiusTopRight = RenderBox.ResolveFixedSize(element, min, element.style.BorderRadiusTopRight);
            float radiusBottomRight = RenderBox.ResolveFixedSize(element, min, element.style.BorderRadiusBottomRight);
            float radiusBottomLeft = RenderBox.ResolveFixedSize(element, min, element.style.BorderRadiusBottomLeft);

            path.SetTransform(element.layoutResult.matrix.ToMatrix4x4());
            if (radiusBottomLeft > 0 ||
                radiusBottomRight > 0 ||
                radiusTopLeft > 0 ||
                radiusTopRight > 0 ||
                bevelTopRight > 0 ||
                bevelTopLeft > 0 ||
                bevelBottomLeft > 0 ||
                bevelBottomRight > 0) {
                // todo -- decorated rect w/ cut
                path.BeginPath();
                path.RoundedRect(0, 0, size.width, size.height, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft);
                path.Fill();
            }
            else {
                path.BeginPath();
                path.Rect(0, 0, size.width, size.height);
                path.Fill();
            }
        }

    }

}