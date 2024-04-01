using System;
using Colossal.Mathematics;
using Game.Simulation;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace America
{
    public class DumpHeightMap
    {
        public static void DumpToFile(TerrainSystem __instance, Bounds2 area)
        {
            // Area originally is in world coordinates

            // Scale the area coordinates to 0 to 1
            area.min -= __instance.playableOffset;
            area.max -= __instance.playableOffset;
            area.min /= __instance.playableArea;
            area.max /= __instance.playableArea;

            var imageWidth = __instance.heightmap.width;
            var imageHeight = __instance.heightmap.height;

            // Convert to image coordinates
            var areaImageWidth = (int)Math.Ceiling(area.Size().x * imageWidth);
            var areaImageHeight = (int)Math.Ceiling(area.Size().y * imageHeight);
            var areaImageX = (int)Math.Floor(area.x.min * imageWidth);
            var areaImageY = imageHeight - (int)Math.Floor(area.y.min * imageHeight) - areaImageHeight - 1;

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
            bitmap.Save("C:\\Users\\jarip\\test4.png", System.Drawing.Imaging.ImageFormat.Png);

        }
    }
}