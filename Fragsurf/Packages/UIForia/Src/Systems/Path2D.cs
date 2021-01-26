using System;
using Src.Systems;
using SVGX;
using UIForia.Extensions;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;
using Vertigo;
using LineJoin = Vertigo.LineJoin;

namespace UIForia.Rendering {

    public class Path2D : ShapeGenerator {

        // todo -- can probably get away with not instantiating point list & hole list.
        // add some data back to shape and we wont need to use point list for data storage
        // this class should be as light as possible since we probably have a bunch of them
        internal GeometryData geometry;
        internal readonly StructList<Matrix4x4> transforms; // can remove and check for null and use identity
        internal readonly StructList<SVGXDrawCall> drawCallList; // can probably remove this list and bake into object data
        internal readonly StructList<ObjectData> objectDataList;

        internal StructList<SVGXFillStyle> fillStyles;
        internal StructList<SVGXStrokeStyle> strokeStyles;
        internal StructList<FixedRenderState> renderStateList;

        private Matrix4x4 currentMatrix;
        private SVGXFillStyle currentFillStyle;
        private SVGXStrokeStyle currentStrokeStyle;
        private FixedRenderState currentFixedRenderState;

        private bool renderStateChanged;
        private bool matrixChanged;

        private static readonly float s_CircleRadii = VertigoUtil.BytesToFloat(250, 250, 250, 250);
        private static readonly float s_RectRadii = VertigoUtil.BytesToFloat(0, 0, 0, 0);
        private const int k_Unused = 0;
        private const int k_StrokeWidthZero = 0;

        public Path2D() {
            // can i get rid of draw call list and use object data instead?
            this.geometry = GeometryData.Create();
            this.drawCallList = new StructList<SVGXDrawCall>(4);
            this.transforms = new StructList<Matrix4x4>(4);
            this.objectDataList = new StructList<ObjectData>(4);
            this.fillStyles = null;
            this.strokeStyles = null;
            this.currentMatrix = Matrix4x4.identity;
            this.currentFillStyle = SVGXFillStyle.Default;
            this.currentStrokeStyle = SVGXStrokeStyle.Default;
            this.currentFixedRenderState = new FixedRenderState(BlendState.Default, DepthState.Default);
            transforms.Add(currentMatrix);
        }

        public new void Clear() {
            base.Clear();
            matrixChanged = false;
            drawCallList.QuickClear();
            fillStyles?.QuickClear();
            strokeStyles?.QuickClear();
            transforms.size = 0;
            objectDataList.size = 0;
            geometry.Clear();
            currentMatrix = Matrix4x4.identity;
            currentFillStyle = SVGXFillStyle.Default;
            currentStrokeStyle = SVGXStrokeStyle.Default;
            currentShapeRange = default;
            currentFixedRenderState = new FixedRenderState(BlendState.Default, DepthState.Default);
            transforms.Add(currentMatrix);
            if (renderStateList != null) {
                renderStateList.size = 0;
            }
        }

        public void SetUVTransform() {
        }

        public void SetStroke(in Color color) {
            currentStrokeStyle.encodedColor = VertigoUtil.ColorToFloat(color);
        }

        public void SetFill(in Color? color) {
            if (color.HasValue) {
                currentFillStyle.encodedColor = VertigoUtil.ColorToFloat(color.Value);
                currentFillStyle.paintMode |= PaintMode.Color;
            }
            else {
                currentFillStyle.encodedColor = 0;
                currentFillStyle.paintMode &= ~PaintMode.Color;
            }
        }

//        public void SetFill(SVGXGradient gradient) {
//            if (gradient != null) {
//                currentFillGradient = gradient;
//                currentFillStyle.paintMode |= PaintMode.Gradient;
//            }
//            else {
//                currentFillGradient = null;
//                currentFillStyle.paintMode &= ~PaintMode.Gradient;
//            }
//        }

        public void SetFill(Texture texture) {
            if (texture != null) {
                currentFillStyle.texture = texture;
                currentFillStyle.paintMode |= PaintMode.Texture;
            }
            else {
                currentFillStyle.texture = null;
                currentFillStyle.paintMode &= ~PaintMode.Texture;
            }
        }

        public void SetFillOpacity(float opacity) {
            currentFillStyle.opacity = opacity;
        }

        public void SetStrokeOpacity(float opacity) {
            currentStrokeStyle.opacity = opacity;
        }

        public void SetStrokeWidth(float width) {
            currentStrokeStyle.strokeWidth = width;
        }

        public void Stroke() {
            strokeStyles = strokeStyles ?? StructList<SVGXStrokeStyle>.Get();
            int styleIdx = strokeStyles.size;
            strokeStyles.Add(currentStrokeStyle);
            if (matrixChanged) {
                transforms.Add(currentMatrix); // don't add if we didnt change it
            }

            drawCallList.Add(new SVGXDrawCall(DrawCallType.StandardStroke, styleIdx, transforms.size - 1, currentShapeRange));
        }

        public void Fill(FillMode fillMode = FillMode.Normal) {
            if (currentShapeRange.length == 0) return;
            fillStyles = fillStyles ?? StructList<SVGXFillStyle>.Get();
            int styleIdx = fillStyles.size;
            fillStyles.Add(currentFillStyle);
            if (matrixChanged) {
                matrixChanged = false;
                transforms.Add(currentMatrix); // don't add if we didnt change it
            }

            DrawCallType drawCallType = DrawCallType.StandardFill;

            if (fillMode != FillMode.Normal) {
                drawCallType = DrawCallType.ShadowFill;
            }

            SVGXDrawCall drawCall = new SVGXDrawCall(drawCallType, styleIdx, transforms.size - 1, currentShapeRange);

            if (renderStateChanged) {
                renderStateChanged = false;
                renderStateList = renderStateList ?? new StructList<FixedRenderState>(4);
                renderStateList.Add(currentFixedRenderState);
            }

            if (renderStateList != null) {
                drawCall.renderStateId = renderStateList.size - 1;
            }
            else {
                drawCall.renderStateId = -1;
            }

            drawCallList.Add(drawCall);
        }

        public void LineTo(Vector2 position) {
            LineTo(position.x, position.y);
        }

        private int lastShapeCount = -1;

        internal void UpdateGeometry() {
            geometry.Clear(); // todo -- can be optimized for adding / updating only

            SVGXDrawCall[] drawCalls = drawCallList.array;
            int rangeStart = 0;

            for (int i = 0; i < drawCallList.size; i++) {
                ref SVGXDrawCall drawCall = ref drawCalls[i];

                GeometryRange range = new GeometryRange(geometry.positionList.size, 0, geometry.triangleList.size, 0);
                RangeInt objectRange = new RangeInt();

                switch (drawCall.type) {
                    case DrawCallType.StandardStroke:
                        for (int j = drawCall.shapeRange.start; j < drawCall.shapeRange.end; j++) {
                            GenerateStrokeGeometry(ref shapeList.array[j], strokeStyles[drawCall.styleIdx]);
                        }

                        break;

                    case DrawCallType.StandardFill: {
                        for (int j = drawCall.shapeRange.start; j < drawCall.shapeRange.end; j++) {
                            GenerateFillGeometry(ref shapeList.array[j], fillStyles[drawCall.styleIdx]);
                        }

                        break;
                    }

                    case DrawCallType.ShadowFill:
                        for (int j = drawCall.shapeRange.start; j < drawCall.shapeRange.end; j++) {
                            GenerateShadowFillGeometry(ref shapeList.array[j], fillStyles[drawCall.styleIdx]);
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                range.vertexEnd = geometry.positionList.size;
                range.triangleEnd = geometry.triangleList.size;
                drawCall.geometryRange = range;
                drawCall.objectRange = objectRange;
                drawCall.objectRange.start = rangeStart;
                drawCall.objectRange.length = drawCall.shapeRange.length;
                rangeStart = drawCall.objectRange.end;
            }
        }

        private static float EncodeCornerRadii(float width, float height, Vector2 top, Vector2 bottom) {
            float min = math.min(width, height);

            if (min <= 0) min = 0.0001f;

            float halfMin = min * 0.5f;

            float cornerRadiusTopLeft = top.x;
            float cornerRadiusTopRight = top.y;
            float cornerRadiusBottomLeft = bottom.x;
            float cornerRadiusBottomRight = bottom.y;

            cornerRadiusTopLeft = math.clamp(cornerRadiusTopLeft, 0, halfMin) / min;
            cornerRadiusTopRight = math.clamp(cornerRadiusTopRight, 0, halfMin) / min;
            cornerRadiusBottomLeft = math.clamp(cornerRadiusBottomLeft, 0, halfMin) / min;
            cornerRadiusBottomRight = math.clamp(cornerRadiusBottomRight, 0, halfMin) / min;

            byte b0 = (byte) (((cornerRadiusTopLeft * 1000)) * 0.5f);
            byte b1 = (byte) (((cornerRadiusTopRight * 1000)) * 0.5f);
            byte b2 = (byte) (((cornerRadiusBottomLeft * 1000)) * 0.5f);
            byte b3 = (byte) (((cornerRadiusBottomRight * 1000)) * 0.5f);

            return VertigoUtil.BytesToFloat(b0, b1, b2, b3);
        }

        private void GenerateShadowStrokeGeometry() {
        }

        private void GenerateStrokeGeometry(ref ShapeDef shape, in SVGXStrokeStyle strokeStyle) {
            float strokeWidth = strokeStyle.strokeWidth;
            if (strokeWidth <= 0) {
                strokeWidth = 1f;
            }

            Vector4 colorData = new Vector4(strokeStyle.encodedColor, strokeStyle.encodedTint, strokeStyle.opacity, (int) strokeStyle.paintMode);
            ObjectData objectData = new ObjectData();
            objectData.colorData = colorData;

            switch (shape.shapeType) {
                case ShapeType.Unset:
                    break;

                case ShapeType.Rect: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    objectData.geometryRange = GeometryGenerator.StrokeRect(geometry, position.x, position.y, size.x, size.y, strokeWidth);
                    objectData.objectData = new Vector4((int) ShapeType.Rect, 0, VertigoUtil.PackSizeVector(size), strokeWidth);
                    break;
                }

                case ShapeType.RoundedRect: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    Vector2 radiiTop = pointList.array[shape.pointRange.start + 0].position;
                    Vector2 radiiBottom = pointList.array[shape.pointRange.start + 1].position;
                    float clip = Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    objectData.geometryRange = GeometryGenerator.FillDecoratedRect(geometry, position, size.x, size.y, cornerDefinition);
                    objectData.objectData = new Vector4((int) ShapeType.RoundedRect, EncodeCornerRadii(size.x, size.y, radiiTop, radiiBottom), VertigoUtil.PackSizeVector(size), strokeWidth);
                    break;
                }

                case ShapeType.Circle: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    float clip = Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    objectData.geometryRange = GeometryGenerator.FillDecoratedRect(geometry, position, size.x, size.y, cornerDefinition);
                    objectData.objectData = new Vector4((int) ShapeType.Circle, s_CircleRadii, VertigoUtil.PackSizeVector(size), strokeWidth);
                    break;
                }

                case ShapeType.Ellipse: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    float clip = Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    objectData.geometryRange = GeometryGenerator.FillDecoratedRect(geometry, position, size.x, size.y, cornerDefinition);
                    objectData.objectData = new Vector4((int) ShapeType.Ellipse, s_CircleRadii, VertigoUtil.PackSizeVector(size), strokeWidth);
                    break;
                }

                case ShapeType.Rhombus:
                    break;
                case ShapeType.Triangle:
                    break;
                case ShapeType.Polygon:
                    break;
                case ShapeType.Text:
                    break;

                case ShapeType.Path: {
                    GeometryGenerator.RenderState renderState = new GeometryGenerator.RenderState();
                    renderState.lineCap = strokeStyle.lineCap;
                    renderState.lineJoin = strokeStyle.lineJoin;
                    renderState.miterLimit = (int) strokeStyle.miterLimit;
                    renderState.strokeWidth = strokeWidth;
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.Path, (int) strokeStyle.paintMode);
                    objectData.geometryRange = GeometryGenerator.StrokeOpenPath(geometry, pointList, shape.pointRange, renderState);
                    objectData.objectData = new Vector4(flags, 0, 0, 0);

                    break;
                }

                case ShapeType.ClosedPath: {
                    GeometryGenerator.RenderState renderState = new GeometryGenerator.RenderState();
                    renderState.lineCap = strokeStyle.lineCap;
                    renderState.lineJoin = strokeStyle.lineJoin;
                    renderState.miterLimit = (int) strokeStyle.miterLimit;
                    renderState.strokeWidth = strokeWidth;

                    int flags = BitUtil.SetHighLowBits((int) ShapeType.ClosedPath, (int) strokeStyle.paintMode);
                    objectData.geometryRange = GeometryGenerator.StrokeClosedPath(geometry, pointList, shape.pointRange, renderState);
                    objectData.objectData = new Vector4(flags, 0, 0, 0);
                }
                    break;
                case ShapeType.Sprite:
                    break;
                case ShapeType.Sector: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size * 2; // double size for stroke or it cuts off weirdly
                    position -= (shape.bounds.size * 0.5f);
                    Vector2 angleAndWidth = pointList.array[shape.pointRange.start].position;
                    float rotation = pointList.array[shape.pointRange.start + 1].position.x;
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.Sector, (int) strokeStyle.paintMode);
                    objectData.geometryRange = GeometryGenerator.FillRect(geometry, position.x, position.y, size.x, size.y);
                    Vector2 pivot = new Vector2(position.x + (size.x * 0.5f), -(position.y + (size.y * 0.5f)));
                    for (int i = objectData.geometryRange.vertexStart; i < objectData.geometryRange.vertexEnd; i++) {
                        Vector2 v = VectorExtensions.Rotate(geometry.positionList.array[i], pivot, rotation);
                        geometry.positionList.array[i].x = v.x;
                        geometry.positionList.array[i].y = v.y;
                        geometry.texCoordList1.array[i] = new Vector4(angleAndWidth.x, angleAndWidth.y, 0, 0);
                    }

                    objectData.objectData = new Vector4(flags, VertigoUtil.PackSizeVector(angleAndWidth), VertigoUtil.PackSizeVector(size), strokeWidth);
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            objectDataList.Add(objectData);
        }

        private void GenerateFillGeometry(ref ShapeDef shape, in SVGXFillStyle fillStyle) {
            ObjectData objectData = new ObjectData();
            objectData.colorData = new Vector4(fillStyle.encodedColor, fillStyle.encodedTint, fillStyle.opacity, k_Unused);

            switch (shape.shapeType) {
                case ShapeType.Polygon: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    int segmentCount = (int) pointList.array[shape.pointRange.start].position.x;
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.Polygon, (int) fillStyle.paintMode);
                    objectData.geometryRange = GeometryGenerator.FillRegularPolygon(geometry, position, size.x, size.y, segmentCount);
                    objectData.objectData = new Vector4(flags, 0, VertigoUtil.PackSizeVector(size), k_StrokeWidthZero);
                    break;
                }

                case ShapeType.Ellipse: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    float clip = Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.Ellipse, (int) fillStyle.paintMode);
                    objectData.geometryRange = GeometryGenerator.FillDecoratedRect(geometry, position, size.x, size.y, cornerDefinition);
                    objectData.objectData = new Vector4(flags, s_CircleRadii, VertigoUtil.PackSizeVector(size), k_StrokeWidthZero);
                    break;
                }

                case ShapeType.Rect: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.Rect, (int) fillStyle.paintMode);
                    objectData.geometryRange = GeometryGenerator.FillRect(geometry, position.x, position.y, size.x, size.y);
                    objectData.objectData = new Vector4(flags, 0, VertigoUtil.PackSizeVector(size), k_StrokeWidthZero);
                    break;
                }

                case ShapeType.ClosedPath:
                case ShapeType.Path: {
                    objectData.geometryRange = GeometryGenerator.FillClosedPath(new GeometryGenerator.PathData() {
                        bounds = shape.bounds,
                        pointRange = shape.pointRange,
                        holeRange = shape.holeRange,
                        points = pointList,
                        holes = holeList
                    }, geometry);

                    int flags = BitUtil.SetHighLowBits((int) ShapeType.ClosedPath, (int) fillStyle.paintMode);
                    objectData.objectData = new Vector4(flags, 0, 0, 0);

                    break;
                }

                case ShapeType.RoundedRect: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    Vector2 radiiTop = pointList.array[shape.pointRange.start + 0].position;
                    Vector2 radiiBottom = pointList.array[shape.pointRange.start + 1].position;
                    float clip = 0; //Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.RoundedRect, (int) fillStyle.paintMode);
                    objectData.geometryRange = GeometryGenerator.FillDecoratedRect(geometry, position, size.x, size.y, cornerDefinition);
                    objectData.objectData = new Vector4(flags, EncodeCornerRadii(size.x, size.y, radiiTop, radiiBottom), VertigoUtil.PackSizeVector(size), k_StrokeWidthZero);
                    break;
                }

                case ShapeType.Circle: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    float clip = Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.Circle, (int) fillStyle.paintMode);
                    objectData.geometryRange = GeometryGenerator.FillDecoratedRect(geometry, position, size.x, size.y, cornerDefinition);
                    objectData.objectData = new Vector4(flags, s_CircleRadii, VertigoUtil.PackSizeVector(size), k_StrokeWidthZero);
                    break;
                }

                case ShapeType.Text:
                    break;

                case ShapeType.Triangle: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.Triangle, (int) fillStyle.paintMode);
                    objectData.geometryRange = GeometryGenerator.FillRect(geometry, position.x, position.y, size.x, size.y);
                    objectData.objectData = new Vector4(flags, 0, VertigoUtil.PackSizeVector(size), k_StrokeWidthZero);

                    Vector2 p0 = pointList.array[shape.pointRange.start + 0].position;
                    Vector2 p1 = pointList.array[shape.pointRange.start + 1].position;
                    Vector2 p2 = pointList.array[shape.pointRange.start + 2].position;
                    // remap points into uvs from our bounds
                    float p0X = MathUtil.PercentOfRange(p0.x, position.x, position.x + size.x);
                    float p0Y = 1 - MathUtil.PercentOfRange(p0.y, position.y, position.y + size.y);
                    float p1X = MathUtil.PercentOfRange(p1.x, position.x, position.x + size.x);
                    float p1Y = 1 - MathUtil.PercentOfRange(p1.y, position.y, position.y + size.y);
                    float p2X = MathUtil.PercentOfRange(p2.x, position.x, position.x + size.x);
                    float p2Y = 1 - MathUtil.PercentOfRange(p2.y, position.y, position.y + size.y);
                    objectData.objectData.y = p2Y;
                    for (int i = objectData.geometryRange.vertexStart; i < objectData.geometryRange.vertexEnd; i++) {
                        geometry.texCoordList0.array[i].z = p0X;
                        geometry.texCoordList0.array[i].w = p0Y;
                        geometry.texCoordList1.array[i] = new Vector4(p1X, p1Y, p2X, 0); // cant use w since object id goes there
                    }

                    break;
                }

                case ShapeType.Sector: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    Vector2 angleAndWidth = pointList.array[shape.pointRange.start].position;
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.Sector, (int) fillStyle.paintMode);
                    objectData.geometryRange = GeometryGenerator.FillRect(geometry, position.x, position.y, size.x, size.y);
//                    Vector2 pivot = new Vector2(position.x + (size.x * 0.5f), -(position.y + (size.y * 0.5f)));
                    float rotation = pointList.array[shape.pointRange.start + 1].position.x;

                    for (int i = objectData.geometryRange.vertexStart; i < objectData.geometryRange.vertexEnd; i++) {
                        geometry.texCoordList1.array[i] = new Vector4(angleAndWidth.x, angleAndWidth.y, rotation, 0);
                    }

                    objectData.objectData = new Vector4(flags, s_CircleRadii, VertigoUtil.PackSizeVector(size), k_StrokeWidthZero);
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            objectDataList.Add(objectData);
        }

        private void GenerateShadowFillGeometry(ref ShapeDef shape, in SVGXFillStyle fillStyle) {
            ObjectData objectData = new ObjectData();
            objectData.colorData = new Vector4(VertigoUtil.ColorToFloat(fillStyle.shadowColor), VertigoUtil.ColorToFloat(fillStyle.shadowTint), fillStyle.shadowOpacity, fillStyle.shadowIntensity);

            int paintMode = (int) ((fillStyle.shadowTint.a > 0) ? (PaintMode.Shadow | PaintMode.ShadowTint) : PaintMode.Shadow);
            Vector2 position = shape.bounds.position;
            Vector2 size = shape.bounds.size + new Vector2(fillStyle.shadowSizeX, fillStyle.shadowSizeY) + new Vector2(fillStyle.shadowIntensity, fillStyle.shadowIntensity);
            position -= new Vector2(fillStyle.shadowSizeX, fillStyle.shadowSizeY) * 0.5f;
            position -= new Vector2(fillStyle.shadowIntensity, fillStyle.shadowIntensity) * 0.5f;
            position += new Vector2(fillStyle.shadowOffsetX, fillStyle.shadowOffsetY);

            switch (shape.shapeType) {
                case ShapeType.Unset:
                    break;

                case ShapeType.Rect: {
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.Rect, paintMode);
                    objectData.geometryRange = GeometryGenerator.FillRect(geometry, fillStyle.shadowOffsetX + position.x, fillStyle.shadowOffsetY + position.y, size.x, size.y);
                    objectData.objectData = new Vector4(flags, s_RectRadii, VertigoUtil.PackSizeVector(size), k_StrokeWidthZero);
                    break;
                }

                case ShapeType.RoundedRect: {
                    Vector2 radiiTop = pointList.array[shape.pointRange.start + 0].position;
                    Vector2 radiiBottom = pointList.array[shape.pointRange.start + 1].position;
                    // clip should be min radius * 0.5 clamped to half size
                    float clip = 0; //Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.RoundedRect, paintMode);
                    objectData.geometryRange = GeometryGenerator.FillDecoratedRect(geometry, position, size.x, size.y, cornerDefinition);
                    objectData.objectData = (new Vector4(flags, EncodeCornerRadii(shape.bounds.size.x, shape.bounds.size.y, radiiTop, radiiBottom), VertigoUtil.PackSizeVector(size), k_StrokeWidthZero));
                    break;
                }

                case ShapeType.Circle: {
                    float clip = Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.Circle, paintMode);
                    objectData.geometryRange = GeometryGenerator.FillDecoratedRect(geometry, position, size.x, size.y, cornerDefinition);
                    objectData.objectData = new Vector4(flags, s_CircleRadii, VertigoUtil.PackSizeVector(size), k_StrokeWidthZero);
                    break;
                }

                case ShapeType.Ellipse: {
                    float clip = Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.Ellipse, paintMode);
                    objectData.geometryRange = GeometryGenerator.FillDecoratedRect(geometry, position, size.x, size.y, cornerDefinition);
                    objectData.objectData = new Vector4(flags, s_CircleRadii, VertigoUtil.PackSizeVector(size), k_StrokeWidthZero);
                    break;
                }

                case ShapeType.Rhombus:
                    throw new NotImplementedException();

                case ShapeType.Triangle: {
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.Triangle, paintMode);
                    objectData.geometryRange = GeometryGenerator.FillRect(geometry, position.x, position.y, size.x, size.y);
                    Vector2 originalPosition = shape.bounds.position;
                    Vector2 originalSize = shape.bounds.size;
                    objectData.objectData = new Vector4(flags, 0, VertigoUtil.PackSizeVector(originalSize), k_StrokeWidthZero);
                    Vector2 p0 = pointList.array[shape.pointRange.start + 0].position;
                    Vector2 p1 = pointList.array[shape.pointRange.start + 1].position;
                    Vector2 p2 = pointList.array[shape.pointRange.start + 2].position;
                    // remap points into uvs from our bounds
                    float p0X = MathUtil.PercentOfRange(p0.x, originalPosition.x, originalPosition.x + originalSize.x);
                    float p0Y = 1 - MathUtil.PercentOfRange(p0.y, originalPosition.y, originalPosition.y + originalSize.y);
                    float p1X = MathUtil.PercentOfRange(p1.x, originalPosition.x, originalPosition.x + originalSize.x);
                    float p1Y = 1 - MathUtil.PercentOfRange(p1.y, originalPosition.y, originalPosition.y + originalSize.y);
                    float p2X = MathUtil.PercentOfRange(p2.x, originalPosition.x, originalPosition.x + originalSize.x);
                    float p2Y = 1 - MathUtil.PercentOfRange(p2.y, originalPosition.y, originalPosition.y + originalSize.y);
                    objectData.objectData.y = p2Y;
                    for (int i = objectData.geometryRange.vertexStart; i < objectData.geometryRange.vertexEnd; i++) {
                        geometry.texCoordList0.array[i].z = p0X;
                        geometry.texCoordList0.array[i].w = p0Y;
                        geometry.texCoordList1.array[i] = new Vector4(p1X, p1Y, p2X, 0); // cant use w since object id goes there
                    }

                    break;
                }

                case ShapeType.Polygon:
                    throw new NotImplementedException();
                case ShapeType.Text:
                    throw new NotImplementedException();
                case ShapeType.Path:
                    throw new NotImplementedException();
                case ShapeType.ClosedPath:
                    throw new NotImplementedException();
                case ShapeType.Sprite:
                    throw new NotImplementedException();
                case ShapeType.Sector: {
                    Vector2 angleAndWidth = pointList.array[shape.pointRange.start].position;
                    float rotation = pointList.array[shape.pointRange.start + 1].position.x;
                    int flags = BitUtil.SetHighLowBits((int) ShapeType.Sector, paintMode);
                    objectData.geometryRange = GeometryGenerator.FillRect(geometry, position.x, position.y, size.x, size.y);
                    Vector2 pivot = new Vector2(position.x + (size.x * 0.5f), -(position.y + (size.y * 0.5f)));
                    for (int i = objectData.geometryRange.vertexStart; i < objectData.geometryRange.vertexEnd; i++) {
                        Vector2 v = VectorExtensions.Rotate(geometry.positionList.array[i], pivot, rotation);
                        geometry.positionList.array[i].x = v.x;
                        geometry.positionList.array[i].y = v.y;
                        geometry.texCoordList1.array[i] = new Vector4(angleAndWidth.x, angleAndWidth.y, 0, 0);
                    }

                    objectData.objectData = new Vector4(flags, VertigoUtil.PackSizeVector(angleAndWidth), VertigoUtil.PackSizeVector(size), k_StrokeWidthZero);
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            objectDataList.Add(objectData);
        }

        public void SetStrokeJoin(LineJoin joinType) {
            currentStrokeStyle.lineJoin = joinType;
        }

        public void SetShadowColor(Color shadowColor) {
            currentFillStyle.shadowColor = shadowColor;
        }

        public void SetShadowTint(Color color) {
            currentFillStyle.shadowTint = color;
        }

        public void SetShadowOffset(Vector2 shadowOffset) {
            currentFillStyle.shadowOffsetX = shadowOffset.x;
            currentFillStyle.shadowOffsetY = shadowOffset.y;
        }

        public void SetShadowOffset(float x, float y) {
            currentFillStyle.shadowOffsetX = x;
            currentFillStyle.shadowOffsetY = y;
        }

        public void SetShadowSize(float x, float y) {
            currentFillStyle.shadowSizeX = x;
            currentFillStyle.shadowSizeY = y;
        }

        public void SetShadowSize(Vector2 size) {
            currentFillStyle.shadowSizeX = size.x;
            currentFillStyle.shadowSizeY = size.y;
        }

        public void SetShadowIntensity(float shadowIntensity) {
            if (shadowIntensity < 0) shadowIntensity = 0;
            currentFillStyle.shadowIntensity = shadowIntensity;
        }

        public void SetShadowOpacity(float shadowOpacity) {
            currentFillStyle.shadowOpacity = shadowOpacity;
        }

        public void SetDepthState(in DepthState? depthState) {
            renderStateChanged = true;


            if (!depthState.HasValue) {
                currentFixedRenderState.depthState = DepthState.Default;
            }
            else {
                currentFixedRenderState.depthState = depthState.Value;
            }
        }

        public void SetBlendState(in BlendState? blendState) {
            renderStateChanged = true;


            if (!blendState.HasValue) {
                currentFixedRenderState.blendState = BlendState.Default;
            }
            else {
                currentFixedRenderState.blendState = blendState.Value;
            }
        }

        internal struct ObjectData {

            public GeometryRange geometryRange;
            public Vector4 objectData;
            public Vector4 colorData;

        }

        public void SetTransform(Matrix4x4 matrix) {
            currentMatrix = matrix;
            matrixChanged = true;
        }

    }

    public enum FillMode {

        Normal,
        Shadow,

    }

}
