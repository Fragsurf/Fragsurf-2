using System;
using Src.Systems;
using SVGX;
using UIForia.Rendering.Vertigo;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;
using BlendState = Src.Systems.BlendState;
using CompareFunction = UnityEngine.Rendering.CompareFunction;
using DepthState = Src.Systems.DepthState;
using Object = UnityEngine.Object;
using PooledMesh = UIForia.Rendering.Vertigo.PooledMesh;

namespace UIForia.Rendering {

    public struct ClipBatch {

        public Texture2D texture;
        public StructList<Vector4> objectData;
        public StructList<Vector4> colorData;
        public StructList<Matrix4x4> transforms;
        public PooledMesh pooledMesh;

    }

    public class ClipContext {

        internal StructList<Vector3> positionList;
        internal StructList<Vector4> texCoordList0;
        internal StructList<Vector4> texCoordList1;
        internal StructList<int> triangleList;

        internal LightList<ClipData> clippers;

        private Material reset;
        private Material clipDrawMaterial;
        private Material clearMaterial;
        private Material countMaterial;
        private Material blitCountMaterial;
        private Material clearCountMaterial;

        private ClipMaterialPool clipMaterialPool;
        private StructList<ClipBatch> batchesToRender;
        private readonly SimpleRectPacker maskPackerR =  new SimpleRectPacker(1024, 1024, 0);
        private readonly SimpleRectPacker maskPackerG =  new SimpleRectPacker(1024, 1024, 0);
        private readonly SimpleRectPacker maskPackerB =  new SimpleRectPacker(1024, 1024, 0);
        private readonly SimpleRectPacker maskPackerA =  new SimpleRectPacker(1024, 1024, 0);
        private RenderTexture clipTexture;
        private RenderTexture countTexture;

        private readonly MeshPool meshPool;
        private static readonly int s_Color = Shader.PropertyToID("_Color");
        private static readonly int s_MainTex = Shader.PropertyToID("_MainTex");
        private PooledMesh regionMesh;
        private bool requireRegionCounting;

        public ClipContext(UIForiaSettings settings) {
            this.clipDrawMaterial = new Material(settings.sdfPathMaterial);
            this.clearMaterial = new Material(settings.clearClipRegionsMaterial);
            this.clearCountMaterial = new Material(clearMaterial);

            this.clearMaterial.SetColor(s_Color, Color.white);
            this.clearCountMaterial.SetColor(s_Color, new Color(0, 0, 0, 0));

            this.countMaterial = new Material(settings.clipCountMaterial);
            this.blitCountMaterial = new Material(settings.clipBlitMaterial);
            this.clipMaterialPool = new ClipMaterialPool(clipDrawMaterial);
            this.positionList = new StructList<Vector3>();
            this.texCoordList0 = new StructList<Vector4>();
            this.texCoordList1 = new StructList<Vector4>();
            this.triangleList = new StructList<int>();
            this.batchesToRender = new StructList<ClipBatch>();
            this.clippers = new LightList<ClipData>();
            this.meshPool = new MeshPool();
            // todo -- dont use screen dimensions
            this.clipTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            this.countTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            this.clipTexture.name = "UIForia Clip Draw Texture";
            this.countTexture.name = "UIForia Clip Count Texture";
            BlendState blendState = BlendState.Default;
            blendState.sourceBlendMode = BlendMode.One;
            blendState.destBlendMode = BlendMode.One;
            blendState.blendOp = BlendOp.Min;
            DepthState depthState = DepthState.Default;
            depthState.compareFunction = CompareFunction.Equal;
            depthState.writeEnabled = true;

            MaterialUtil.SetupState(clipDrawMaterial, new FixedRenderState(blendState, depthState));
        }

        public void Destroy() {
            Object.DestroyImmediate(clipTexture);
            Object.DestroyImmediate(countTexture);
            Object.DestroyImmediate(clearMaterial);
            Object.DestroyImmediate(countMaterial);
            Object.DestroyImmediate(clearCountMaterial);
            Object.DestroyImmediate(blitCountMaterial);
            regionMesh?.Release();
            clipMaterialPool.Destroy();
            meshPool.Destroy();
        }

        internal void AddClipper(ClipData clipData) {
            clippers.Add(clipData);
        }

        // todo for tomorrow
        // stop allocating meshes, pool materials
        // handle not drawing screen aligned clipping bounds
        // handle multiple clip shapes in a path
        // maybe handle textures
        // handle lots of clip shapes
        // handle masking channels
        // profile

        public void ConstructClipData() {
            maskPackerR.Clear();
            maskPackerG.Clear();
            maskPackerB.Clear();
            maskPackerA.Clear();
            // todo -- profile sorting by size
            // might not need the -1 if screen level one is never sent
            // also don't need to render clipper for a view since it is automatically rectangular
            // basically any pure screen aligned rectangle doesn't need to be rendered out
            for (int i = 0; i < clippers.size; i++) {
                ClipData clipData = clippers.array[i];

                if (clipData.clipPath == null) {
                    float xy = VertigoUtil.PackSizeVector(new Vector2(clipData.aabb.x, clipData.aabb.y));
                    float zw = VertigoUtil.PackSizeVector(new Vector2(clipData.aabb.z, clipData.aabb.w));
                    clipData.packedBoundsAndChannel.x = xy;
                    clipData.packedBoundsAndChannel.y = zw;
                    continue;
                }

                clipData.zIndex = i + 1; // will have to change depending on how we decide to handle different channels

                int width = (int) (clipData.aabb.z - clipData.aabb.x);
                int height = (int) (clipData.aabb.w - clipData.aabb.y);
                SimpleRectPacker.PackedRect region;

                clipData.textureChannel = -1;
                if (maskPackerR.TryPackRect(width, height, out region)) {
                    clipData.textureChannel = 0;
                }
                // todo -- other channels don't work right now, need to figure out if we use additional draw calls or super double dip region packing
//                else if (maskPackerG.TryPackRect(width, height, out region)) {
//                    clipData.textureChannel = 1;
//                }
//                else if (maskPackerB.TryPackRect(width, height, out region)) {
//                    clipData.textureChannel = 2;
//                }
//                else if (maskPackerA.TryPackRect(width, height, out region)) {
//                    clipData.textureChannel = 3;
//                }
                else {
                    Debug.Log($"Can't fit {width}, {height} into clip texture");
                }

                // note this is in 2 point form, not height & width
                if (clipData.textureChannel != -1) {
                    clipData.clipTexture = clipTexture;
                    clipData.textureRegion = region;
                    clipData.clipUVs = new Vector4(
                        region.xMin / (float) clipTexture.width,
                        region.yMin / (float) clipTexture.height,
                        region.xMax / (float) clipTexture.width,
                        region.yMax / (float) clipTexture.height
                    );
                    float xy = VertigoUtil.PackSizeVector(new Vector2(clipData.aabb.x, clipData.aabb.y));
                    float zw = VertigoUtil.PackSizeVector(new Vector2(clipData.aabb.z, clipData.aabb.w));
                    clipData.packedBoundsAndChannel.x = xy;
                    clipData.packedBoundsAndChannel.y = zw;
                    clipData.packedBoundsAndChannel.z = clipData.textureChannel;
                }
            }
        }

        private void Gather() {
            for (int i = 0; i < clippers.size; i++) {
                ClipData clipper = clippers.array[i];
                ClipData ptr = clipper.parent;
                while (ptr != null) {
                    ptr.dependents.Add(clipper);
                    ptr = ptr.parent;
                }
            }
        }

        // depth buffer means our regions are locked per channel
        // 2 options: 1. each channel is its own set of draw calls, this is easy but maybe not as fast. do this as a first pass
        //            2. try to re-use regions for different channels, almost certainly leads to less throughput but faster since we don't need extra draw calls
        // probably means we have sub-sorting regions, ie large packers would have sub-packers
        // would definitely want to sort by size in that case and first try to pack larger regions into themselves
        // would likely update rect packer to be channel aware, when trying to place next item instead of moving over try colliding a different channel instead

        public void Clip(Camera camera, CommandBuffer commandBuffer) {
            // breaks on refresh if we don't do this :(
            this.clearMaterial.SetColor(s_Color, Color.white);
            this.clearCountMaterial.SetColor(s_Color, new Color(0, 0, 0, 0));
            requireRegionCounting = false;

            for (int i = 0; i < batchesToRender.size; i++) {
                batchesToRender[i].pooledMesh.Release();
                StructList<Matrix4x4>.Release(ref batchesToRender.array[i].transforms);
                StructList<Vector4>.Release(ref batchesToRender.array[i].objectData);
                StructList<Vector4>.Release(ref batchesToRender.array[i].colorData);
            }

            batchesToRender.Clear();
            Gather();

            Vector3 cameraOrigin = camera.transform.position;
            cameraOrigin.x -= 0.5f * Screen.width;
            cameraOrigin.y += (0.5f * Screen.height);
            cameraOrigin.z += 2;

            Matrix4x4 origin = Matrix4x4.TRS(cameraOrigin, Quaternion.identity, Vector3.one);

            LightList<ClipData> texturedClippers = LightList<ClipData>.Get();

            regionMesh?.Release();

            regionMesh = GetRegionMesh(out requireRegionCounting);

            clipTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.Default); // todo -- use lower resolution

#if DEBUG
            commandBuffer.BeginSample("UIFora Clip Draw");
#endif
            commandBuffer.SetRenderTarget(clipTexture);

            // probably don't need this actually, can bake it into clear. keep for debugging
            commandBuffer.ClearRenderTarget(true, true, Color.black);

            commandBuffer.DrawMesh(regionMesh.mesh, origin, clearMaterial, 0, 0);

            // todo -- handle multiple shapes from one path

            ClipBatch batch = new ClipBatch();
            batch.transforms = StructList<Matrix4x4>.Get();
            batch.colorData = StructList<Vector4>.Get();
            batch.objectData = StructList<Vector4>.Get();

            for (int i = 0; i < clippers.size; i++) {
                ClipData clipData = clippers[i];
                Path2D clipPath = clipData.clipPath;

                if (clipPath == null) {
                    // todo if transform is not identity we need to generate a rotated or skewed rect for the clip shape
                    continue;
                }

                clipPath.UpdateGeometry(); // should early out if no update required

                if (AnyShapeUsesTextures(clipPath)) {
                    // todo -- handle textures
                    // todo -- handle text
                    continue;
                }

                batch = DrawShapesInPath(batch, clipPath, clipData, clipData);

                for (int j = 0; j < clipData.dependents.size; j++) {
                    batch = DrawShapesInPath(batch, clipPath, clipData, clipData.dependents[j]);
                }
            }

            FinalizeBatch(batch, false);

            for (int i = 0; i < batchesToRender.size; i++) {
                ref ClipBatch clipBatch = ref batchesToRender.array[i];

                ClipPropertyBlock propertyBlock = clipMaterialPool.GetPropertyBlock(clipBatch.transforms.size);

                propertyBlock.SetData(clipBatch);

                commandBuffer.DrawMesh(clipBatch.pooledMesh.mesh, origin, clipDrawMaterial, 0, 0, propertyBlock.matBlock);
            }

            commandBuffer.SetRenderTarget(countTexture);

#if DEBUG
            commandBuffer.EndSample("UIFora Clip Draw");
            commandBuffer.BeginSample("UIForia Clip Count");
#endif
            if (requireRegionCounting) {
                // todo -- only need to count-blend a region if it has more than 1 draw into it
                // todo -- skip the pass entirely if no region needs a count
                // probably don't need this actually, can bake it into clear. keep for debugging
                commandBuffer.ClearRenderTarget(true, true, Color.black);

                commandBuffer.DrawMesh(regionMesh.mesh, origin, clearCountMaterial, 0, 0);

                for (int i = 0; i < batchesToRender.size; i++) {
                    ref ClipBatch clipBatch = ref batchesToRender.array[i];

                    ClipPropertyBlock propertyBlock = clipMaterialPool.GetPropertyBlock(clipBatch.transforms.size);

                    propertyBlock.SetData(clipBatch);

                    commandBuffer.DrawMesh(batchesToRender[i].pooledMesh.mesh, origin, countMaterial, 0, 0, propertyBlock.matBlock);
                }
            }
#if DEBUG
            commandBuffer.EndSample("UIForia Clip Count");
            commandBuffer.BeginSample("UIForia Clip Blit");
#endif
            if (requireRegionCounting) {
                commandBuffer.SetRenderTarget(clipTexture);

                blitCountMaterial.SetTexture(s_MainTex, countTexture);

                commandBuffer.DrawMesh(regionMesh.mesh, origin, blitCountMaterial, 0, 0);
            }

#if DEBUG
            commandBuffer.EndSample("UIForia Clip Blit");
#endif
            commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

            LightList<ClipData>.Release(ref texturedClippers);
        }

        private ClipBatch FinalizeBatch(ClipBatch clipBatch, bool createNewBatch = true) {
            clipBatch.pooledMesh = meshPool.Get();

            clipBatch.pooledMesh.SetVertices(positionList.array, positionList.size);
            clipBatch.pooledMesh.SetTextureCoord0(texCoordList0.array, texCoordList0.size);
            clipBatch.pooledMesh.SetTextureCoord1(texCoordList1.array, texCoordList1.size);
            clipBatch.pooledMesh.SetTriangles(triangleList.array, triangleList.size);

            positionList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
            triangleList.size = 0;
            // todo handle texture n stuff
            batchesToRender.Add(clipBatch);

            if (createNewBatch) {
                clipBatch = new ClipBatch();
                clipBatch.transforms = StructList<Matrix4x4>.Get();
                clipBatch.colorData = StructList<Vector4>.Get();
                clipBatch.objectData = StructList<Vector4>.Get();
            }

            return clipBatch;
        }

        private bool BatchCanHandleShape(in ClipBatch batch, in SVGXDrawCall drawCall) {
            return true;
        }

        private bool AnyShapeUsesTextures(Path2D path) {
            return false;
        }

        private ClipBatch DrawShapesInPath(ClipBatch clipBatch, Path2D path, ClipData clipData, ClipData target) {
            int vertexAdjustment = 0;

            for (int drawCallIndex = 0; drawCallIndex < path.drawCallList.size; drawCallIndex++) {
                ref SVGXDrawCall drawCall = ref path.drawCallList.array[drawCallIndex];

                if (!BatchCanHandleShape(clipBatch, drawCall)) {
                    // handle batch breaking here    
                    clipBatch = FinalizeBatch(clipBatch);
                    throw new NotImplementedException();
                }

                int objectStart = drawCall.objectRange.start;
                int objectEnd = drawCall.objectRange.end;
                int insertIdx = clipBatch.objectData.size;

                // todo -- keep looping until batch would break
                // break conditions: texture change (can mitigate)
                //                   too many vertices
                //                   too many objects

                clipBatch.objectData.EnsureAdditionalCapacity(objectEnd - objectStart);
                clipBatch.colorData.EnsureAdditionalCapacity(objectEnd - objectStart);
                clipBatch.transforms.EnsureAdditionalCapacity(objectEnd - objectStart);

                GeometryRange range = drawCall.geometryRange;
                int vertexCount = range.vertexEnd - range.vertexStart;
                int triangleCount = range.triangleEnd - range.triangleStart;

                int start = positionList.size;

                positionList.AddRange(path.geometry.positionList, range.vertexStart, vertexCount);
                texCoordList0.AddRange(path.geometry.texCoordList0, range.vertexStart, vertexCount);
                texCoordList1.AddRange(path.geometry.texCoordList1, range.vertexStart, vertexCount);

                Vector4[] texCoord1 = texCoordList1.array;

                for (int objIdx = objectStart; objIdx < objectEnd; objIdx++) {
                    clipBatch.objectData.array[insertIdx] = path.objectDataList.array[objIdx].objectData;
                    clipBatch.colorData.array[insertIdx] = path.objectDataList.array[objIdx].colorData;
                    Matrix4x4 matrix;

                    if (path.transforms != null) {
                        matrix = path.transforms.array[drawCall.transformIdx];
                    }
                    else {
                        matrix = Matrix4x4.identity;
                    }

                    float x = matrix.m03;
                    float y = matrix.m13;

                    float xDiff = target.textureRegion.xMin - x;
                    float yDiff = target.textureRegion.yMin - y;

                    if (target != clipData) {
                        xDiff += (clipData.aabb.x - target.aabb.x);
                        yDiff += clipData.aabb.y - target.aabb.y;
                    }

                    matrix.m03 = x + xDiff;
                    matrix.m13 = -(y + yDiff);
                    matrix.m23 = target.zIndex;

                    clipBatch.transforms[insertIdx] = matrix;

                    ref Path2D.ObjectData objectData = ref path.objectDataList.array[objIdx];
                    int geometryStart = objectData.geometryRange.vertexStart;
                    int geometryEnd = objectData.geometryRange.vertexEnd;
                    for (int s = geometryStart; s < geometryEnd; s++) {
                        texCoord1[start + (s - vertexAdjustment)].w = insertIdx;
                    }

                    insertIdx++;
                }

                clipBatch.objectData.size = insertIdx;
                clipBatch.colorData.size = insertIdx;
                clipBatch.transforms.size = insertIdx;

                triangleList.EnsureAdditionalCapacity(triangleCount);

                int offset = triangleList.size;
                int[] triangles = triangleList.array;
                int[] geometryTriangles = path.geometry.triangleList.array;

                for (int t = 0; t < triangleCount; t++) {
                    triangles[offset + t] = start + (geometryTriangles[range.triangleStart + t] - range.vertexStart);
                }

                triangleList.size += triangleCount;
            }

            return clipBatch;
        }

        private PooledMesh GetRegionMesh(out bool requireCountPass) {
            positionList.EnsureAdditionalCapacity(clippers.size * 4);
            texCoordList0.EnsureAdditionalCapacity(clippers.size * 4);
            texCoordList1.EnsureAdditionalCapacity(clippers.size * 4);
            triangleList.EnsureAdditionalCapacity(clippers.size * 6);
            int vertIdx = 0;
            int triIdx = 0;

            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            int[] triangles = triangleList.array;
            requireCountPass = false;

            for (int i = 0; i < clippers.size; i++) {
                ClipData clipData = clippers.array[i];

                if (clipData.clipPath == null) continue;

                SimpleRectPacker.PackedRect region = clipData.textureRegion;
                int cnt = 1; // 1 because we always draw self
                ClipData ptr = clipData.parent;
                while (ptr != null) {
                    // only add 1 to cnt if parent clipper is actually rendered!

                    if (ptr.clipPath != null) {
                        cnt++;
                    }

                    ptr = ptr.parent;
                }

                clipData.regionDrawCount = cnt;
                requireRegionCounting = requireRegionCounting || cnt > 1;

                positions[vertIdx + 0] = new Vector3(region.xMin, -region.yMin, clipData.zIndex);
                positions[vertIdx + 1] = new Vector3(region.xMax, -region.yMin, clipData.zIndex);
                positions[vertIdx + 2] = new Vector3(region.xMax, -region.yMax, clipData.zIndex);
                positions[vertIdx + 3] = new Vector3(region.xMin, -region.yMax, clipData.zIndex);

                texCoord0[vertIdx + 0] = new Vector4(clipData.clipUVs.x, clipData.clipUVs.y, cnt, 0);
                texCoord0[vertIdx + 1] = new Vector4(clipData.clipUVs.z, clipData.clipUVs.y, cnt, 0);
                texCoord0[vertIdx + 2] = new Vector4(clipData.clipUVs.z, clipData.clipUVs.w, cnt, 0);
                texCoord0[vertIdx + 3] = new Vector4(clipData.clipUVs.x, clipData.clipUVs.w, cnt, 0);

                triangles[triIdx++] = vertIdx + 0;
                triangles[triIdx++] = vertIdx + 1;
                triangles[triIdx++] = vertIdx + 2;
                triangles[triIdx++] = vertIdx + 2;
                triangles[triIdx++] = vertIdx + 3;
                triangles[triIdx++] = vertIdx + 0;
                vertIdx += 4;
            }

            // no need to set sizes for lists!

            PooledMesh pooledMesh = meshPool.Get();
            pooledMesh.SetVertices(positions, vertIdx);
            pooledMesh.SetTextureCoord0(texCoord0, vertIdx);
            pooledMesh.SetTriangles(triangles, triIdx);

            return pooledMesh;
        }

        public void Clear() {
            clippers.Clear();
            positionList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
            triangleList.size = 0;
            regionMesh?.Release();
        }

    }

}