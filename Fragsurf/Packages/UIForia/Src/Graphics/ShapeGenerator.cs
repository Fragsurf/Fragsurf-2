using System;
using UIForia.Util;
using UnityEngine;

namespace Vertigo {

    public class ShapeGenerator {

        protected Vector2 lastPoint;
        protected PathDef currentPath;

        internal bool requiresGeometryUpdate;

        internal StructList<PathPoint> pointList;
        internal StructList<PathPoint> holeList;
        internal StructList<ShapeDef> shapeList;

        protected bool buildingPath;
        protected bool inHole;
        protected RangeInt currentShapeRange;

        public ShapeGenerator(int capacity = 8) {
            this.shapeList = new StructList<ShapeDef>(capacity);
            this.pointList = new StructList<PathPoint>(capacity * 8);
            this.holeList = new StructList<PathPoint>(capacity * 2);
        }

        public void LineTo(float x, float y) {
            if (!buildingPath) return;
            requiresGeometryUpdate = true;
            if (inHole) {
                holeList.Add(new PathPoint(x, y, PointFlag.Corner | PointFlag.Hole));
                currentPath.holeRange.length++;
            }
            else {
                pointList.Add(new PathPoint(x, y, PointFlag.Corner));
                currentPath.pointRange.length++;
            }

            lastPoint = new Vector2(x, y);
        }

        public void HorizontalLineTo(float x) {
            LineTo(x, lastPoint.y);
        }

        public void VerticalLineTo(float y) {
            LineTo(lastPoint.x, y);
        }

        public void CubicBezierTo(float c0x, float c0y, float c1x, float c1y, float x, float y) {
            requiresGeometryUpdate = true;
            int pointCount = 0;
            Vector2 end = new Vector2(x, y);
            // todo flags
            if (inHole) {
                pointCount = Bezier.CubicCurve(holeList, lastPoint, new Vector2(c0x, c0y), new Vector2(c1x, c1y), end);
                currentPath.holeRange.length += pointCount;
            }
            else {
                pointCount = Bezier.CubicCurve(pointList, lastPoint, new Vector2(c0x, c0y), new Vector2(c1x, c1y), end);
                currentPath.pointRange.length += pointCount;
            }

            lastPoint.x = end.x;
            lastPoint.y = end.y;
        }

        public void RectTo(float x, float y) {
            requiresGeometryUpdate = true;
            Vector2 start = lastPoint;
            HorizontalLineTo(x);
            VerticalLineTo(y);
            HorizontalLineTo(start.x);
            VerticalLineTo(start.y);
        }

        public void BeginPath(float x, float y) {
            requiresGeometryUpdate = true;

            if (buildingPath) {
                // delete old path if it hasn't ended
                pointList.size = currentPath.pointRange.start;
                holeList.size = currentPath.holeRange.start;
            }

            currentShapeRange = new RangeInt(shapeList.size, 0);

            currentPath = new PathDef();
            currentPath.pointRange.start = pointList.size;
            currentPath.holeRange.start = holeList.size;
            currentPath.pointRange.length++;
            pointList.Add(new PathPoint(x, y, PointFlag.Corner));
            buildingPath = true;
            inHole = false; // just in case
            lastPoint = new Vector2(x, y);
        }

        public void BeginPath() {
            requiresGeometryUpdate = true;

            if (buildingPath) {
                // delete old path if it hasn't ended
                pointList.size = currentPath.pointRange.start;
                holeList.size = currentPath.holeRange.start;
            }

            currentShapeRange = new RangeInt(shapeList.size, 0);

            currentPath = new PathDef();
            currentPath.pointRange.start = pointList.size;
            currentPath.holeRange.start = holeList.size;

            inHole = false; // just in case
        }

        public void MoveTo(float x, float y) {
            requiresGeometryUpdate = true;

            lastPoint = new Vector2(x, y);
            currentPath.pointRange.length++;
            pointList.Add(new PathPoint(lastPoint.x, lastPoint.y, PointFlag.Move));
            buildingPath = true;
        }

        public void MoveTo(Vector2 position) {
            requiresGeometryUpdate = true;

            lastPoint = position;
            currentPath.pointRange.length++;
            pointList.Add(new PathPoint(lastPoint.x, lastPoint.y, PointFlag.Move));
            buildingPath = true;
        }

        public void ClosePath() {
            requiresGeometryUpdate = true;

            if (!buildingPath) return;
            currentShapeRange.length++;
            buildingPath = false;
            inHole = false;
            ShapeDef shapeDef = new ShapeDef(ShapeType.ClosedPath);
            shapeDef.pointRange = currentPath.pointRange;
            shapeDef.holeRange = currentPath.holeRange;
            shapeDef.bounds = ComputePathBounds();
            ClampHoles(shapeDef.holeRange, shapeDef.bounds);
            shapeList.Add(shapeDef);
            currentPath = default;
            lastPoint.x = 0;
            lastPoint.y = 0;
        }

        public void EndPath() {
            requiresGeometryUpdate = true;

            if (!buildingPath) return;
            currentShapeRange.length++;
            buildingPath = false;
            inHole = false;
            ShapeDef shapeDef = new ShapeDef(ShapeType.Path);
            shapeDef.pointRange = currentPath.pointRange;
            shapeDef.holeRange = currentPath.holeRange;
            shapeDef.bounds = ComputePathBounds();
            ClampHoles(shapeDef.holeRange, shapeDef.bounds);
            shapeList.Add(shapeDef);
            currentPath = default;
            lastPoint.x = 0;
            lastPoint.y = 0;
        }

        public void BeginHole(float x, float y) {
            requiresGeometryUpdate = true;

            inHole = true;
            holeList.Add(new PathPoint(x, y, PointFlag.Corner | PointFlag.Hole | PointFlag.HoleStart));
            currentPath.holeRange.length++;
            lastPoint.x = x;
            lastPoint.y = y;
        }

        public void CloseHole() {
            requiresGeometryUpdate = true;

            inHole = false;
        }

        public int Rect(float x, float y, float width, float height) {
            requiresGeometryUpdate = true;

            ShapeDef shapeDef = new ShapeDef(ShapeType.Rect);
            shapeDef.bounds = new Rect(x, y, width, height);
            shapeList.Add(shapeDef);
            currentShapeRange.length++;
            return shapeList.size - 1;
        }

        public int Rect(in Rect rect) {
            requiresGeometryUpdate = true;

            ShapeDef shapeDef = new ShapeDef(ShapeType.Rect);
            shapeDef.bounds = rect;
            shapeList.Add(shapeDef);
            currentShapeRange.length++;
            return shapeList.size - 1;
        }

        public int RoundedRect(float x, float y, float width, float height, float r0, float r1, float r2, float r3) {
            requiresGeometryUpdate = true;

            ShapeDef shapeDef = new ShapeDef(ShapeType.RoundedRect);
            shapeDef.pointRange = new RangeInt(pointList.size, 2);
            pointList.Add(new PathPoint(r0, r1));
            pointList.Add(new PathPoint(r2, r3));
            shapeDef.bounds = new Rect(x, y, width, height);
            shapeList.Add(shapeDef);
            currentShapeRange.length++;
            return shapeList.Count - 1;
        }

        public int Circle(float x, float y, float r) {
            requiresGeometryUpdate = true;

            currentShapeRange.length++;
            float diameter = r * 2;
            ShapeDef shapeDef = new ShapeDef(ShapeType.Circle);
            shapeDef.bounds = new Rect(x, y, diameter, diameter);
            shapeList.Add(shapeDef);
            return shapeList.Count - 1;
        }

        public int Sector(float x, float y, float radius, float rotation, float angle, float width) {
            requiresGeometryUpdate = true;

            currentShapeRange.length++;
            float diameter = radius * 2;
            ShapeDef shapeDef = new ShapeDef(ShapeType.Sector);
            shapeDef.bounds = new Rect(x, y, diameter, diameter);
            shapeDef.pointRange.start = pointList.size;
            shapeDef.pointRange.length = 2;
            pointList.Add(new PathPoint(angle, width));
            pointList.Add(new PathPoint(rotation, 0));
            shapeList.Add(shapeDef);
            return shapeList.Count - 1;
        }

        public int Ellipse(float x, float y, float rw, float rh) {
            requiresGeometryUpdate = true;

            currentShapeRange.length++;
            ShapeDef shapeDef = new ShapeDef(ShapeType.Ellipse);
            shapeDef.bounds = new Rect(x, y, rw * 2, rh * 2);
            shapeList.Add(shapeDef);
            return shapeList.Count - 1;
        }

        public int RegularPolygon(float x, float y, float width, float height, int sides) {
            requiresGeometryUpdate = true;

            currentShapeRange.length++;
            if (sides < 3) sides = 3;
            ShapeDef shapeDef = new ShapeDef(ShapeType.Polygon);
            shapeDef.pointRange.start = pointList.size;
            shapeDef.pointRange.length = 1;
            pointList.Add(new PathPoint(sides, 0));
            shapeDef.bounds = new Rect(x, y, width, height);
            shapeList.Add(shapeDef);
            return shapeList.size - 1;
        }

        public int Triangle(float x0, float y0, float x1, float y1, float x2, float y2) {
            requiresGeometryUpdate = true;

            currentShapeRange.length++;
            ShapeDef shapeDef = new ShapeDef(ShapeType.Triangle);
            shapeDef.pointRange.start = pointList.size;
            shapeDef.pointRange.length = 3;
            pointList.Add(new PathPoint(x0, y0));
            pointList.Add(new PathPoint(x1, y1));
            pointList.Add(new PathPoint(x2, y2));
            float minX = x0;
            float minY = y0;
            float maxX = x0;
            float maxY = y0;
            minX = x1 < minX ? x1 : minX;
            minX = x2 < minX ? x2 : minX;
            minY = y1 < minY ? y1 : minY;
            minY = y2 < minY ? y2 : minY;
            maxX = x1 > maxX ? x1 : maxX;
            maxX = x2 > maxX ? x2 : maxX;
            maxY = y1 > maxY ? y1 : maxY;
            maxY = y2 > maxY ? y2 : maxY;
            shapeDef.bounds = new Rect(minX, minY, maxX - minX, maxY - minY);
            shapeList.Add(shapeDef);
            return shapeList.size - 1;
        }

        public int Rhombus(float x, float y, float width, float height) {
            requiresGeometryUpdate = true;

            currentShapeRange.length++;
            ShapeDef shapeDef = new ShapeDef(ShapeType.Rhombus);
            shapeDef.bounds = new Rect(x, y, width, height);
            shapeList.Add(shapeDef);
            return shapeList.size - 1;
        }

        // equallateral triangle & iso triangle should be simple once triangle is done
        // could also add trapezoid, vesica , & cross

        private Rect ComputePathBounds() {
            int start = currentPath.pointRange.start;
            int end = currentPath.pointRange.end;
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            PathPoint[] array = pointList.array;
            for (int i = start; i < end; i++) {
                float x = array[i].position.x;
                float y = array[i].position.y;

                if (x < minX) {
                    minX = x;
                }

                if (x > maxX) {
                    maxX = x;
                }

                if (y < minY) {
                    minY = y;
                }

                if (y > maxY) {
                    maxY = y;
                }
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        private void ClampHoles(in RangeInt holeRange, in Rect rect) {
            int start = holeRange.start;
            int end = holeRange.end;

            if (end - start == 0) {
                return;
            }

            PathPoint[] array = holeList.array;

            float minX = rect.xMin;
            float minY = rect.yMin;
            float maxX = rect.xMax;
            float maxY = rect.yMax;
            for (int i = start; i < end; i++) {
                float x = array[i].position.x;
                float y = array[i].position.y;
                if (x < minX) x = minX;
                if (x > maxX) x = maxX;
                if (y < minY) y = minY;
                if (y > maxY) y = maxY;
                array[i].position.x = x;
                array[i].position.y = y;
            }
        }

        public void Clear() {
            shapeList.QuickClear();
            pointList.size = 0;
            holeList.size = 0;
            inHole = false;
            buildingPath = false;
            lastPoint = Vector2.zero;
            requiresGeometryUpdate = true;
        }

        [Flags]
        public enum PointFlag {

            Hole = 1 << 1,
            Corner = 1 << 2,
            HoleStart = 1 << 3,
            Move = 1 << 4

        }

        public struct PathPoint {

            public PointFlag flags;
            public Vector2 position;

            public PathPoint(float x, float y, PointFlag flags = 0) {
                this.position.x = x;
                this.position.y = y;
                this.flags = flags;
            }

        }

        public struct PathDef {

            public RangeInt pointRange;
            public RangeInt holeRange;

        }

        internal struct ShapeDef {

            public Rect bounds;
            public RangeInt pointRange;
            public RangeInt holeRange;
            public ShapeType shapeType;

            public ShapeDef(ShapeType shapeType) {
                this.shapeType = shapeType;
                this.pointRange = default;
                this.holeRange = default;
                this.bounds = default;
            }

        }

    }

}