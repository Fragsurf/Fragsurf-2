// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.Rendering;
// using Vertigo;
//
// namespace Graphics {
//
//     [TestFixture]
//     public class LowLevelGFX {
//
//         private CommandBuffer commandBuffer = new CommandBuffer();
//
//         [Test]
//         public void DrawsSimpleShapeCache() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.SetMaterial(material);
//             ctx.BeginFrame();
//             ShapeCache geometry = new ShapeCache();
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//             ctx.Draw(geometry);
//             Assert.AreEqual(4, ctx.vertexCount);
//             Assert.AreEqual(6, ctx.triangleCount);
//             ctx.Draw(geometry);
//             Assert.AreEqual(8, ctx.vertexCount);
//             Assert.AreEqual(12, ctx.triangleCount);
//             ctx.EndFrame(commandBuffer, Matrix4x4.identity, Matrix4x4.identity);
//         }
//
//         [Test]
//         public void CreateBatchWhenMaterialIsSet() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.SetMaterial(material);
//             ctx.BeginFrame();
//
//             ShapeCache geometry = new ShapeCache();
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//             ctx.Draw(geometry);
//             Assert.AreEqual(4, ctx.vertexCount);
//             Assert.AreEqual(0, ctx.pendingBatches.size);
//             ctx.SetMaterial(material);
//             Assert.AreEqual(1, ctx.pendingBatches.size);
//             ctx.Draw(geometry);
//             Assert.AreEqual(1, ctx.pendingBatches.size);
//             Assert.AreEqual(4, ctx.vertexCount);
//             ctx.EndFrame(commandBuffer, Matrix4x4.identity, Matrix4x4.identity);
//         }
//
//         [Test]
//         public void NoNewBatchWhenNoPendingDrawCalls() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.BeginFrame();
//
//             ctx.SetMaterial(material);
//             ShapeCache geometry = new ShapeCache();
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//             ctx.Draw(geometry);
//             Assert.AreEqual(0, ctx.pendingBatches.size);
//             ctx.EndFrame(commandBuffer, Matrix4x4.identity, Matrix4x4.identity);
//         }
//
//
//         [Test]
//         public void CreateBatchWhenMaterialPropertyChanges() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.BeginFrame();
//             ctx.SetMaterial(material);
//             ShapeCache geometry = new ShapeCache();
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//             ctx.Draw(geometry);
//             ctx.SetFloatProperty(100, 200f);
//             ctx.Draw(geometry);
//             ctx.EndFrame(commandBuffer, Matrix4x4.identity, Matrix4x4.identity);
//
//             Assert.AreEqual(2, ctx.pendingBatches.size);
//         }
//
//         [Test]
//         public void DoNotCreateBatchWhenMaterialPropertyChangesAgainWithoutDrawing() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.BeginFrame();
//             ctx.SetMaterial(material);
//             ShapeCache geometry = new ShapeCache();
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//             ctx.Draw(geometry);
//             ctx.SetFloatProperty(100, 200f);
//             ctx.SetFloatProperty(200, 200f);
//             ctx.Draw(geometry);
//             ctx.EndFrame(commandBuffer, Matrix4x4.identity, Matrix4x4.identity);
//
//             Assert.AreEqual(2, ctx.pendingBatches.size);
//         }
//
//         [Test]
//         public void CreateBatchWhenKeywordIsEnabled() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.BeginFrame();
//             ctx.SetMaterial(material);
//             ShapeCache geometry = new ShapeCache();
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//             ctx.Draw(geometry);
//             ctx.EnableKeyword("SOME_KEYWORD");
//             ctx.Draw(geometry);
//             ctx.EndFrame(commandBuffer, Matrix4x4.identity, Matrix4x4.identity);
//
//             Assert.AreEqual(2, ctx.pendingBatches.size);
//         }
//
//         [Test]
//         public void CreateBatchWhenKeywordIsDisabled() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.BeginFrame();
//             ctx.SetMaterial(material);
//             ShapeCache geometry = new ShapeCache();
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//             ctx.Draw(geometry);
//             ctx.DisableKeyword("SOME_KEYWORD");
//             ctx.Draw(geometry);
//             ctx.EndFrame(commandBuffer, Matrix4x4.identity, Matrix4x4.identity);
//
//             Assert.AreEqual(2, ctx.pendingBatches.size);
//         }
//
//         [Test]
//         public void CreatesMeshForSingleShape() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.SetMaterial(material);
//             ctx.BeginFrame();
//             ShapeCache geometry = new ShapeCache();
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//             ctx.Draw(geometry);
//             Assert.AreEqual(geometry.vertexCount, ctx.positionList.size);
//             Assert.AreEqual(geometry.triangleCount, ctx.triangleList.size);
//             ctx.Render();
//             Assert.AreEqual(1, ctx.pendingBatches.size);
//             Mesh mesh = ctx.pendingBatches[0].mesh.mesh;
//             Assert.AreEqual(new[] {
//                 geometry.positionList[0],
//                 geometry.positionList[1],
//                 geometry.positionList[2],
//                 geometry.positionList[3]
//             }, mesh.vertices);
//             Assert.AreEqual(new[] {0, 1, 2, 2, 3, 0}, mesh.triangles);
//         }
//
//         [Test]
//         public void CreatesMeshForMultipleShapesInBatch() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.SetMaterial(material);
//             ctx.BeginFrame();
//             ShapeCache geometry = new ShapeCache();
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//             ctx.Draw(geometry);
//             ctx.Draw(geometry);
//             ctx.Draw(geometry);
//             ctx.Draw(geometry);
//             Assert.AreEqual(16, ctx.positionList.size);
//             Assert.AreEqual(24, ctx.triangleList.size);
//             ctx.Render();
//             Assert.AreEqual(1, ctx.pendingBatches.size);
//             Mesh mesh = ctx.pendingBatches[0].mesh.mesh;
//             Assert.AreEqual(new[] {
//                 geometry.positionList[0],
//                 geometry.positionList[1],
//                 geometry.positionList[2],
//                 geometry.positionList[3],
//                 geometry.positionList[0],
//                 geometry.positionList[1],
//                 geometry.positionList[2],
//                 geometry.positionList[3],
//                 geometry.positionList[0],
//                 geometry.positionList[1],
//                 geometry.positionList[2],
//                 geometry.positionList[3],
//                 geometry.positionList[0],
//                 geometry.positionList[1],
//                 geometry.positionList[2],
//                 geometry.positionList[3]
//             }, mesh.vertices);
//             Assert.AreEqual(new[] {
//                 0, 1, 2, 2, 3, 0,
//                 4, 5, 6, 6, 7, 4,
//                 8, 9, 10, 10, 11, 8,
//                 12, 13, 14, 14, 15, 12
//             }, mesh.triangles);
//         }
//
//         [Test]
//         public void CreatesMeshForMultipleShapesAcrossBatches() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.SetMaterial(material);
//             ctx.BeginFrame();
//             ShapeCache geometry = new ShapeCache();
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//             ctx.Draw(geometry);
//             ctx.Draw(geometry);
//             Assert.AreEqual(8, ctx.positionList.size);
//             Assert.AreEqual(12, ctx.triangleList.size);
//             ctx.SetFloatProperty(10, 14f);
//             ctx.Draw(geometry);
//             ctx.Draw(geometry);
//             Assert.AreEqual(8, ctx.positionList.size);
//             Assert.AreEqual(12, ctx.triangleList.size);
//             ctx.Render();
//             Assert.AreEqual(2, ctx.pendingBatches.size);
//             Mesh mesh0 = ctx.pendingBatches[0].mesh.mesh;
//             Mesh mesh1 = ctx.pendingBatches[1].mesh.mesh;
//             Assert.AreEqual(new[] {
//                 geometry.positionList[0],
//                 geometry.positionList[1],
//                 geometry.positionList[2],
//                 geometry.positionList[3],
//                 geometry.positionList[0],
//                 geometry.positionList[1],
//                 geometry.positionList[2],
//                 geometry.positionList[3],
//             }, mesh0.vertices);
//             Assert.AreEqual(new[] {
//                 0, 1, 2, 2, 3, 0,
//                 4, 5, 6, 6, 7, 4,
//             }, mesh0.triangles);
//             Assert.AreEqual(new[] {
//                 geometry.positionList[0],
//                 geometry.positionList[1],
//                 geometry.positionList[2],
//                 geometry.positionList[3],
//                 geometry.positionList[0],
//                 geometry.positionList[1],
//                 geometry.positionList[2],
//                 geometry.positionList[3],
//             }, mesh1.vertices);
//             Assert.AreEqual(new[] {
//                 0, 1, 2, 2, 3, 0,
//                 4, 5, 6, 6, 7, 4,
//             }, mesh1.triangles);
//         }
//
//         [Test]
//         public void DrawFromShapeRange() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.SetMaterial(material);
//             ctx.BeginFrame();
//             ShapeCache geometry = new ShapeCache();
//
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//             geometry.FillRect(100, 0, 100, 100, new RenderState());
//             geometry.FillRect(200, 0, 100, 100, new RenderState());
//             geometry.FillRect(300, 0, 100, 100, new RenderState());
//
//             ctx.Draw(geometry, new RangeInt(1, 2));
//             Assert.AreEqual(8, ctx.positionList.size);
//             Assert.AreEqual(12, ctx.triangleList.size);
//             ctx.EndFrame(commandBuffer, Matrix4x4.identity, Matrix4x4.identity);
//             Assert.AreEqual(1, ctx.pendingBatches.size);
//             Mesh mesh0 = ctx.pendingBatches[0].mesh.mesh;
//             Assert.AreEqual(new[] {
//                 geometry.positionList[4],
//                 geometry.positionList[5],
//                 geometry.positionList[6],
//                 geometry.positionList[7],
//                 geometry.positionList[8],
//                 geometry.positionList[9],
//                 geometry.positionList[10],
//                 geometry.positionList[11],
//             }, mesh0.vertices);
//             Assert.AreEqual(new[] {
//                 0, 1, 2, 2, 3, 0,
//                 4, 5, 6, 6, 7, 4
//             }, mesh0.triangles);
//         }
//
//         [Test]
//         public void DrawFromShapeAtIndex() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.SetMaterial(material);
//             ctx.BeginFrame();
//             ShapeCache geometry = new ShapeCache();
//
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//             geometry.FillRect(100, 0, 100, 100, new RenderState());
//             geometry.FillRect(200, 0, 100, 100, new RenderState());
//             geometry.FillRect(300, 0, 100, 100, new RenderState());
//             geometry.FillRect(400, 0, 100, 100, new RenderState());
//
//             ctx.Draw(geometry, 2);
//             ctx.Draw(geometry, 4);
//             Assert.AreEqual(8, ctx.positionList.size);
//             Assert.AreEqual(12, ctx.triangleList.size);
//             ctx.EndFrame(commandBuffer, Matrix4x4.identity, Matrix4x4.identity);
//             Assert.AreEqual(1, ctx.pendingBatches.size);
//             Mesh mesh0 = ctx.pendingBatches[0].mesh.mesh;
//             Assert.AreEqual(new[] {
//                 geometry.positionList[8],
//                 geometry.positionList[9],
//                 geometry.positionList[10],
//                 geometry.positionList[11],
//                 geometry.positionList[16],
//                 geometry.positionList[17],
//                 geometry.positionList[18],
//                 geometry.positionList[19],
//             }, mesh0.vertices);
//             Assert.AreEqual(new[] {
//                 0, 1, 2, 2, 3, 0,
//                 4, 5, 6, 6, 7, 4
//             }, mesh0.triangles);
//         }
//
//         [Test]
//         public void DrawFromShapeAtMultipleIndices() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.SetMaterial(material);
//             ctx.BeginFrame();
//             ShapeCache geometry = new ShapeCache();
//
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//             geometry.FillRect(100, 0, 100, 100, new RenderState());
//             geometry.FillRect(200, 0, 100, 100, new RenderState());
//             geometry.FillRect(300, 0, 100, 100, new RenderState());
//             geometry.FillRect(400, 0, 100, 100, new RenderState());
//
//             ctx.Draw(geometry, new int[] {1, 2, 4});
//             Assert.AreEqual(12, ctx.positionList.size);
//             Assert.AreEqual(18, ctx.triangleList.size);
//             ctx.EndFrame(commandBuffer, Matrix4x4.identity, Matrix4x4.identity);
//             Assert.AreEqual(1, ctx.pendingBatches.size);
//             Mesh mesh0 = ctx.pendingBatches[0].mesh.mesh;
//             Assert.AreEqual(new[] {
//                 geometry.positionList[4],
//                 geometry.positionList[5],
//                 geometry.positionList[6],
//                 geometry.positionList[7],
//
//                 geometry.positionList[8],
//                 geometry.positionList[9],
//                 geometry.positionList[10],
//                 geometry.positionList[11],
//
//                 geometry.positionList[16],
//                 geometry.positionList[17],
//                 geometry.positionList[18],
//                 geometry.positionList[19],
//             }, mesh0.vertices);
//             Assert.AreEqual(new[] {
//                 0, 1, 2, 2, 3, 0,
//                 4, 5, 6, 6, 7, 4,
//                 8, 9, 10, 10, 11, 8
//             }, mesh0.triangles);
//         }
//
//         [Test]
//         public void RespectTransformPosition() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.SetMaterial(material);
//             ctx.BeginFrame();
//
//             ctx.SetTransform(Matrix4x4.Translate(new Vector3(100, 100)));
//
//             ShapeCache geometry = new ShapeCache();
//
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//
//             int calls = 0;
//             ctx.onDebugCommandBuffer += (mesh, matrix, mat, subMesh, pass, propertyBlock) => {
//                 calls++; 
//                 Assert.AreEqual(matrix, Matrix4x4.Translate(new Vector3(100, 100)));
//             };
//
//             ctx.Draw(geometry);
//             ctx.EndFrame(commandBuffer, Matrix4x4.identity, Matrix4x4.identity);
//             Assert.AreEqual(1, calls);
//         }
//         
//         [Test]
//         public void ApplyTransformToBatch() {
//             Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
//             GraphicsContext ctx = new GraphicsContext();
//             ctx.SetMaterial(material);
//             ctx.BeginFrame();
//
//             ctx.SetTransform(Matrix4x4.Translate(new Vector3(100, 100)));
//
//             ShapeCache geometry = new ShapeCache();
//
//             geometry.FillRect(0, 0, 100, 100, new RenderState());
//
//             int calls = 0;
//             ctx.onDebugCommandBuffer += (mesh, matrix, mat, subMesh, pass, propertyBlock) => {
//                 calls++; 
//                 Assert.AreEqual(matrix, Matrix4x4.Translate(new Vector3(200, -200)));
//                 // todo -- figure out what mesh should be need to apply some transform to vertices
//             };
//
//             ctx.Draw(geometry);
//             ctx.SetTransform(Matrix4x4.Translate(new Vector3(200, -200)));
//             ctx.Draw(geometry);
//
//             ctx.EndFrame(commandBuffer, Matrix4x4.identity, Matrix4x4.identity);
//             Assert.AreEqual(1, calls);
//         }
//
//     }
//
// }