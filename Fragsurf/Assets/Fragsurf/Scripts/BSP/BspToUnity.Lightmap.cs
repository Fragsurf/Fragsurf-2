using SourceUtils;
using SourceUtils.ValveBsp;
using System;
using System.Linq;
using UnityEngine;

namespace Fragsurf.BSP
{
	public partial class BspToUnity
	{

		private void GenerateLightmapPixels()
		{
			var lightingLump = _bsp.Lighting.Length > 0 ? _bsp.Lighting.LumpType : _bsp.LightingHdr.LumpType;
			var hdr = lightingLump == _bsp.LightingHdr.LumpType;

			using (var sampleStream = _bsp.GetLumpStream(lightingLump))
			{
				var lightmap = _bsp.LightmapLayout;
				var width = lightmap.TextureSize.X;
				var height = lightmap.TextureSize.Y;
				var colors = new Color[width * height];

				var sampleBuffer = new ColorRGBExp32[256 * 256];
				var faces = hdr ? _bsp.FacesHdr : _bsp.Faces;
				var fullbright = true;

				for (int i = 0, iEnd = faces.Length; i < iEnd; ++i)
				{
					var face = faces[i];
					if (face.LightOffset == -1)
					{
						continue;
					}

					var rect = lightmap.GetLightmapRegion(i);
					var sampleCount = rect.Width * rect.Height;

					sampleStream.Seek(face.LightOffset, System.IO.SeekOrigin.Begin);

					LumpReader<ColorRGBExp32>.ReadLumpFromStream(sampleStream, sampleCount, sampleBuffer);

					for (var y = 0; y < rect.Height; ++y)
					{
						for (var x = 0; x < rect.Width; ++x)
						{
							var s = Math.Max(0, Math.Min(x, rect.Width - 1));
							var t = Math.Max(0, Math.Min(y, rect.Height - 1));
							var sampleIndex = s + t * rect.Width;
							var sample = sampleBuffer[sampleIndex];
							if (fullbright && (sample.R != 0 || sample.G != 0 || sample.B != 0))
							{
								fullbright = false;
							}
							colors[width * (rect.Y + y) + (rect.X + x)] = new Color(
								TexLightToLinear(sample.R, sample.Exponent),
								TexLightToLinear(sample.G, sample.Exponent),
								TexLightToLinear(sample.B, sample.Exponent),
								1.0f).gamma;
						}
					}
				}

				if (fullbright)
				{
					for (int i = 0; i < colors.Length; i++)
					{
						colors[i] = Color.white;
					}
				}

				var dir = new Texture2D(512, 512, UnityEngine.TextureFormat.RGBA32, false);
				var dirColors = new Color[512 * 512];
				for(int i = 0; i < 512* 512; i++)
				{
					dirColors[i] = Color.white;
				}
				dir.SetPixels(dirColors);
				dir.Apply();
				var tex = new Texture2D(width, height, UnityEngine.TextureFormat.RGBA32, false);
				tex.SetPixels(colors);
				tex.Apply();
				var lightmaps = LightmapSettings.lightmaps.ToList();
				lightmaps.Clear();
				lightmaps.Add(new LightmapData
				{
					lightmapColor = tex,
					lightmapDir = dir
				});
				LightmapSettings.lightmaps = lightmaps.ToArray();
			}
		}

		private float TexLightToLinear(byte c, sbyte exponent)
		{
			return (float)c * (float)Mathf.Pow(2.2f, exponent) * (1.0f / 255.0f);
		}

	}
}