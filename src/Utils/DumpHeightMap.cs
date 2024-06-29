using System;
using Colossal.Mathematics;
using Game.Simulation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace TerraformHardening
{
    public class DumpHeightMap
    {
        public static void DumpToFile(TerrainSystem __instance, Bounds2 area, string prefix)
        {
            // Area originally is in world coordinates

            // Scale the area coordinates to 0 to 1
            area.min -= __instance.playableOffset;
            area.max -= __instance.playableOffset;
            area.min /= __instance.playableArea;
            area.max /= __instance.playableArea;
            int4 area1 = new int4(
                (int)math.max(math.floor(area.min.x * __instance.heightmap.width) - 1f, 0.0f),
                (int)math.max(math.floor(area.min.y * __instance.heightmap.height) - 1f, 0.0f),
                (int)math.min(math.ceil(area.max.x * __instance.heightmap.width) + 1f, __instance.heightmap.width - 1),
                (int)math.min(math.ceil(area.max.y * __instance.heightmap.height) + 1f, __instance.heightmap.height - 1)
            );
            area1.zw -= area1.xy;
            // area1.zw = math.clamp(area1.zw, new int2(
            //     __instance.heightmap.width / this.m_TerrainMinMax.size,
            //     __instance.heightmap.height / this.m_TerrainMinMax.size),
            //     new int2(
            //         __instance.heightmap.width,
            //         __instance.heightmap.height)
            //     );

            var areaImageWidth = area1.z;
            var areaImageHeight = area1.w;
            var areaImageX = area1.x;
            var areaImageY = area1.y;

            Mod.log.Info($"DumpToFile: area size {areaImageWidth} x {areaImageHeight}");
            Mod.log.Info($"DumpToFile: area coords x={areaImageX}, y={areaImageY}");

            /* To dump full playable height map 
            areaImageWidth = imageWidth;
            areaImageHeight = imageHeight;
            areaImageX = 0;
            areaImageY = 0;
            */

            // Create a new texture to copy the area to
            var lookTexture = new Texture2D(areaImageWidth, areaImageHeight, __instance.heightmap.graphicsFormat, TextureCreationFlags.DontInitializePixels | TextureCreationFlags.DontUploadUponCreate);

            // Copy the area to lookTexture
            var oldActive = RenderTexture.active;
            RenderTexture.active = __instance.heightmap as RenderTexture;
            lookTexture.ReadPixels(new Rect(areaImageX, areaImageY, areaImageWidth, areaImageHeight), 0, 0);
            lookTexture.Apply();
            RenderTexture.active = oldActive;

            // Save the texture to a file
            var bitmap = new System.Drawing.Bitmap(lookTexture.width, lookTexture.height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // This is not a good way because height map is R16_UNorm which means this will loose information:
            for (var x = 0; x < lookTexture.width; x++)
            {
                for (var y = 0; y < lookTexture.height; y++)
                {
                    var color = lookTexture.GetPixel(x, y);
                    bitmap.SetPixel(x, lookTexture.height - y - 1, System.Drawing.Color.FromArgb((int)(color.r * 255), (int)(color.r * 255), (int)(color.r * 255)));
                }
            }
            bitmap.Save($"C:\\Users\\jarip\\test4-{prefix}.png", System.Drawing.Imaging.ImageFormat.Png);

        }
    }
}