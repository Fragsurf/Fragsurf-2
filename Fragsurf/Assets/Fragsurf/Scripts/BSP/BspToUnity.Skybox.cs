using SourceUtils.ValveBsp.Entities;
using UnityEngine;

namespace Fragsurf.BSP
{
    public partial class BspToUnity 
    {
        private void GenerateSkybox()
        {
            // yeet it on there
            var worldSpawn = _bsp.Entities[0] as Worldspawn;
            var skyName = worldSpawn.SkyName;

            var front = LoadVmtBaseTexture("materials/skybox/" + skyName + "ft.vmt") as Texture2D;
            var back = LoadVmtBaseTexture("materials/skybox/" + skyName + "bk.vmt") as Texture2D;
            var left = LoadVmtBaseTexture("materials/skybox/" + skyName + "lf.vmt") as Texture2D;
            var right = LoadVmtBaseTexture("materials/skybox/" + skyName + "rt.vmt") as Texture2D;
            var up = LoadVmtBaseTexture("materials/skybox/" + skyName + "up.vmt") as Texture2D;
            var dn = LoadVmtBaseTexture("materials/skybox/" + skyName + "dn.vmt") as Texture2D;

            if (left == null
                || right == null
                || up == null
                || dn == null
                || front == null
                || back == null)
            {
                Debug.Log("materials/skybox/" + skyName + "ft.vmt");
                return;
            }

            front = BspUtils.FlipTextureX(front);
            back = BspUtils.FlipTextureX(back);
            left = BspUtils.FlipTextureX(left);
            right = BspUtils.FlipTextureX(right);
            up = BspUtils.FlipTextureX(up);
            dn = BspUtils.FlipTextureX(dn);
            front = front.Rotate(BspUtils.Rotation.HalfCircle);
            back = back.Rotate(BspUtils.Rotation.HalfCircle);
            left = left.Rotate(BspUtils.Rotation.HalfCircle);
            right = right.Rotate(BspUtils.Rotation.HalfCircle);
            up = up.Rotate(BspUtils.Rotation.Right);
            dn = dn.Rotate(BspUtils.Rotation.Left);

            front.filterMode = FilterMode.Point;
            left.filterMode = FilterMode.Point;
            right.filterMode = FilterMode.Point;
            up.filterMode = FilterMode.Point;
            dn.filterMode = FilterMode.Point;
            back.filterMode = FilterMode.Point;
            front.wrapMode = TextureWrapMode.Clamp;
            left.wrapMode = TextureWrapMode.Clamp;
            right.wrapMode = TextureWrapMode.Clamp;
            up.wrapMode = TextureWrapMode.Clamp;
            dn.wrapMode = TextureWrapMode.Clamp;
            back.wrapMode = TextureWrapMode.Clamp;

            var shader = Shader.Find("Skybox/6 Sided");
            var mat = new Material(shader);
            mat.SetTexture("_FrontTex", back);
            mat.SetTexture("_BackTex", front);
            mat.SetTexture("_LeftTex", right);
            mat.SetTexture("_RightTex", left);
            mat.SetTexture("_DownTex", dn);
            mat.SetTexture("_UpTex", up);

            RenderSettings.skybox = mat;
        }
    }
}

