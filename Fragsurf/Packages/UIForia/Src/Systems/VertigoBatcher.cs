//using System.Collections.Generic;
//using UIForia.Util;
//using UnityEngine;
//using Vertigo;
//
//namespace Packages.UIForia.Src.Systems {
//
//    public class VertigoBatcher : IDrawCallBatcher {
//
//        private GeometryCache geometryCache;
//
//        private struct PendingBatch {
//
//            public GeometryCache cache;
//            public Mesh mesh;
//            public Matrix4x4 transform;
//            public StructList<Matrix4x4> transforms;
//            public RangeInt[] geometryRanges;
//            public VertigoMaterial material;
//            public int pass;
//
//        }
//
//        private struct PendingDrawCall {
//
//            public Mesh mesh;
//            public Matrix4x4 matrix;
//            public RangeInt geometryRange;
//            public RangeInt triangleRange;
//
//        }
//
//        private VertigoMaterial lastMaterial;
//
//
//        private StructList<PendingBatch> pendingBatches = new StructList<PendingBatch>();
//        private StructList<PendingDrawCall> pendingDrawCalls = new StructList<PendingDrawCall>();
//        private PendingBatch pendingBatch;
//        private List<Vector3> scratchVector3List = new List<Vector3>();
//        protected readonly VertigoMesh.MeshPool meshPool;
//
//        private static readonly List<Vector3> s_MeshVector3 = new List<Vector3>(0);
//        private static readonly List<Vector4> s_MeshVector4 = new List<Vector4>(0);
//        private static readonly List<Color> s_MeshColor = new List<Color>(0);
//        private static readonly List<int> s_MeshInt = new List<int>(0);
//
//        private static readonly StructList<Vector3> s_Vector3Scratch = new StructList<Vector3>(128);
//        private static readonly StructList<Vector4> s_Vector4Scratch = new StructList<Vector4>(128);
//        private static readonly StructList<Color> s_ColorScratch = new StructList<Color>(128);
//        private static readonly StructList<int> s_IntScratch = new StructList<int>(256);
//            
//        private void PromotePendingBatch() {
//                
//            if (pendingDrawCalls.size == 0) {
//                return;
//            }
//                
//                                
//            VertigoMesh vertigoMesh = meshPool.GetDynamic();
//            Mesh mesh = vertigoMesh.mesh;
//
//            // geometry and triangle ranges refer to our internal cache
//                
//            if (pendingDrawCalls.size > 1) {
//                int vertexCount = 0;
//                int triangleCount = 0;
//                for (int i = 0; i < pendingDrawCalls.size; i++) {
//                    PendingDrawCall call = pendingDrawCalls.array[i];
//                    // maybe don't support this, let Unity do it
//                    if (call.mesh != null) {
//                        // transform mesh vertices by transform
//                        call.mesh.GetVertices(scratchVector3List);
//                        for (int j = 0; j < scratchVector3List.Count; j++) {
//                            scratchVector3List[j] = call.matrix.MultiplyVector(scratchVector3List[j]);
//                        }
//
//                        call.mesh.SetVertices(scratchVector3List);
//                        call.mesh.RecalculateBounds();
//                        scratchVector3List.Clear();
//                    }
//                    else {
//                        // transform geometry vertices by transform
//                        int start = call.geometryRange.start;
//                        int end = call.geometryRange.end;
//                        Vector3[] positions = geometryCache.positions.array;
//                        for (int j = start; j < end; j++) {
//                            positions[j] = call.matrix.MultiplyVector(positions[j]);
//                        }
//
//                        s_Vector3Scratch.EnsureCapacity(vertexCount);
//                        s_Vector4Scratch.EnsureCapacity(vertexCount);
//                        s_ColorScratch.EnsureCapacity(vertexCount);
//                        s_IntScratch.EnsureCapacity(triangleCount);
//                            
////                            s_Vector3Scratch.SetFromRange(geometryCache.positions.array, vertexStart, vertexCount);
////                            ListAccessor<Vector3>.SetArray(s_MeshVector3, s_Vector3Scratch.array, vertexCount);
////                            mesh.SetVertices(s_MeshVector3);
////
////                            s_Vector3Scratch.SetFromRange(geometryCache.normals.array, vertexStart, vertexCount);
////                            ListAccessor<Vector3>.SetArray(s_MeshVector3, s_Vector3Scratch.array, vertexCount);
////                            mesh.SetNormals(s_MeshVector3);
////
////                            s_Vector4Scratch.SetFromRange(geometryCache.texCoord0.array, vertexStart, vertexCount);
////                            ListAccessor<Vector4>.SetArray(s_MeshVector4, s_Vector4Scratch.array, vertexCount);
////                            mesh.SetUVs(0, s_MeshVector4);
////
////                            s_Vector4Scratch.SetFromRange(geometryCache.texCoord1.array, vertexStart, vertexCount);
////                            ListAccessor<Vector4>.SetArray(s_MeshVector4, s_Vector4Scratch.array, vertexCount);
////                            mesh.SetUVs(1, s_MeshVector4);
////
////                            s_ColorScratch.SetFromRange(geometryCache.colors.array, vertexStart, vertexCount);
////                            ListAccessor<Color>.SetArray(s_MeshColor, s_ColorScratch.array, vertexCount);
////                            mesh.SetColors(s_MeshColor);
////                            
////                            s_IntScratch.SetFromRange(cache.triangles.array, triangleStart, triangleCount);
////                            for (int i = 0; i < s_IntScratch.size; i++) {
////                                s_IntScratch.array[i] -= vertexStart;
////                            }
//
//                        ListAccessor<int>.SetArray(s_MeshInt, s_IntScratch.array, triangleCount);
//                        mesh.SetTriangles(s_MeshInt, 0);
//                            
//                        // not sure where this comes from
//                        vertexCount += call.geometryRange.length;
//                        //todo -- recalculate 3d bounds
//                    }
//                }
//
//                // todo -- only copy used channels for each 
//                    
//                    
//                PendingBatch batch = new PendingBatch();
//                batch.transform = Matrix4x4.identity;
//                batch.mesh = CreateMeshFromBatch();
//                batch.material = lastMaterial;
//                pendingBatches.Add(batch);
//            }
//            // todo -- culling, partly handled by layout system for now but should eventually be done in batcher
//            // if size is 1 no need to transform vertices
//            else {
//                PendingDrawCall call = pendingDrawCalls[0];
//                PendingBatch batch = new PendingBatch();
//                batch.transform = call.matrix;
//                batch.mesh = call.mesh != null ? call.mesh : CreateMeshFromBatch();
//                batch.material = lastMaterial;
//                pendingBatches.Add(batch);
//            }
//
//            pendingDrawCalls.Clear();
//        }
//
//        private Mesh CreateMeshFromBatch() {
//                
//            return null;
//        }
//
//        public void AddDrawCall(GeometryCache inputCache, RangeInt shapeRange, VertigoMaterial material, in Matrix4x4 transform) {
//            
//            // if transform didn't change change we don't need to alter vertices in batch
//            // if material didn't change we can just draw again with the same one
//            
//            
//            
//            // if vertex count is large, consider checking to see if we have that shapeid in cache already
//            // unique shapeid = (shape.cacheId | shape.index)
//            
//            // ctx knows if materials changed, maybe don't need to diff them
//            
//            // if setxxxmaterialproperty is called flag for new material ie can't batch
//            
//            // todo if render queue is opaque respect that
//            
//            
//            if (material == lastMaterial) {
//                // all good to batch if last render states are equal
//            }
//            else if (lastMaterial.shaderName == "Vertigo/Default" && material.shaderName == "Vertigo/Default") {
//                
//                if (!material.PassesMatch(lastMaterial)) {
//                    // maybe pass # is an argument to this fn
//                }
//
//                if (!material.RenderStateMatches(lastMaterial)) {
//                    // nope, promote the current batch
//                    PromotePendingBatch();
////                        pendingBatches.Add(new PendingBatch() {
////                            transform = transform,
////                            inputCache,
////                            shapeRange
////                        });
//                }
//
//                if (!material.TexturesMatch(lastMaterial)) {
//                    // nope
//                    // create pending batch for last draw call
//                }
//
//                if (!material.KeywordsMatch(lastMaterial)) {
//                    // check if they are additive
//                    // if so, clone material & merge keywords?
//                }
//
//                // are the textures the same? (or missing)
//                // are the fonts the same? (or missing)
//                // is the mask the same? (or missing)
//                // are the keywords only additive?
//                // is the render state the same?
//                // ok! batch em -> might involve re-writing of attributes
//                // if current batch matrix != transform -> update vertices to reflect transform
//                // calculate bounds for culling? maybe keep the bounds from the shapes around for this purpose?
//            }
//
//            // if the shader is the same and all the values are the same then go ahead and batch
//
//            // if we do decide to batch ->
//
//
//            lastMaterial = material;
//
//            GeometryShape shape = geometryCache.shapes[shapeRange.start];
//            int vertexStart = shape.vertexStart;
//            int vertexCount = shape.vertexCount;
//            int triangleStart = shape.triangleStart;
//            int triangleCount = shape.triangleCount;
//
//            for (int i = shapeRange.start + 1; i < shapeRange.end; i++) {
//                vertexCount += shape.vertexCount;
//                triangleCount += shape.triangleCount;
//                // add shapes to our cache and be sure we copy offsets properly
//            }
//
//            geometryCache.EnsureAdditionalCapacity(vertexCount, triangleCount);
//            geometryCache.positions.AddRange(inputCache.positions, vertexStart, vertexCount);
//            geometryCache.normals.AddRange(inputCache.normals, vertexStart, vertexCount);
//            geometryCache.texCoord0.AddRange(inputCache.texCoord0, vertexStart, vertexCount);
//            geometryCache.texCoord1.AddRange(inputCache.texCoord1, vertexStart, vertexCount);
//            geometryCache.texCoord2.AddRange(inputCache.texCoord2, vertexStart, vertexCount);
//            geometryCache.texCoord3.AddRange(inputCache.texCoord3, vertexStart, vertexCount);
//            geometryCache.colors.AddRange(inputCache.colors, vertexStart, vertexCount);
//            geometryCache.triangles.AddRange(inputCache.triangles, triangleStart, triangleCount);
//                
//            pendingDrawCalls.Add(new PendingDrawCall() {
//                geometryRange = new RangeInt(vertexStart, vertexCount),
//                triangleRange = new RangeInt(triangleStart, triangleCount)
//            });
//
//        }
//
//        public void AddDrawCall(Mesh mesh, VertigoMaterial material, in Matrix4x4 transform) {
//            PromotePendingBatch();
//            pendingBatches.Add(new PendingBatch() {
//                transform = transform,
//                mesh = mesh,
//                pass = 0,
//                // material = material.Clone(materialPool.Get())
//            });
//            //todo -- this is a real draw call, not pending since we won't pool it
//        }
//
//        public void Bake(int width, int height, in Matrix4x4 cameraMatrix, StructList<BatchDrawCall> output) {
//            // do culling & n shit
//            // set material properties
//            // for each pending batch, check culling
//            // for everything in batch that passes culling -> bake into mesh & material, add to output
//
//            // if two pending batches use the same material hash (not sure how to compute this)
//            //     if there is nothing transparent between them, can be baked into one
//            // repeat this until there are no more non mergeable batches
//        }
//
//        public void Clear() { }
//
//    }
//
//}