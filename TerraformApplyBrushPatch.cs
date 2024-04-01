using System;
using System.CodeDom;
using System.ComponentModel;
using System.Reflection;
using Colossal.Logging;
using Colossal.Mathematics;
using Game;
using Game.Areas;
using Game.City;
using Game.Modding;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Simulation;
using Game.Tools;
using HarmonyLib;
using Unity.Burst;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace America
{
  [HarmonyPatch(typeof(TerrainSystem), nameof(TerrainSystem.ApplyBrush))]
  class TerraformApplyBrushPatch
  {

    //     public static void Prepare()
    //     {
    //       GetEntityQueryInvoker = MethodInvoker.GetHandler(
    //     AccessTools.Method(typeof(TerrainSystem), "GetEntityQuery")
    // );
    //     }

    /*
      Notes: 

      WaterSystem.HasWater could be used to check if terraformed area is underwater, and adjust the price accordingly
    */

    // TerraformingType type, Bounds area, Brush brush, Texture texture
    public static bool Prefix(TerrainSystem __instance, TerraformingType type, Bounds2 area, Brush brush, Texture texture)
    {

      // var heightMapTexture = Traverse.Create(__instance).Field("m_Heightmap").GetValue() as RenderTexture;


      Mod.log.Info("Area:  " + area.min.x + " " + area.max.x + " | " + area.min.y + " " + area.max.y);

      // Scale the area coordinates to 0 to 1
      // Bottom left of playable area is 0,0 and top right is 1,1
      area.min -= __instance.playableOffset;
      area.max -= __instance.playableOffset;
      area.min /= __instance.playableArea;
      area.max /= __instance.playableArea;

      Mod.log.Info("Area:  " + area.min.x + " " + area.max.x + " | " + area.min.y + " " + area.max.y);
      Mod.log.Info("Brush target coords: " + brush.m_Target);

      var imageWidth = __instance.heightmap.width;
      var imageHeight = __instance.heightmap.height;

      // Scale the area coordinates to image coordinates
      var areaImageWidth = (int)Math.Ceiling(area.Size().x * imageWidth);
      var areaImageHeight = (int)Math.Ceiling(area.Size().y * imageHeight);
      var areaImageX = (int)Math.Floor(area.x.min * imageWidth);
      var areaImageY = imageHeight - (int)Math.Floor(area.y.min * imageHeight) - areaImageHeight - 1;

      Mod.log.Info("Height scale offset:  " + __instance.heightScaleOffset);
      Mod.log.Info("Image:  " + imageWidth + " x " + imageHeight);
      Mod.log.Info("Area image:  " + areaImageX + " " + areaImageY + " | " + areaImageWidth + " x " + areaImageHeight);
      Mod.log.Info("Graphics format: " + __instance.heightmap.graphicsFormat);

      try
      {
        // From heightmap, extract the area to lookTexture
        var oldActive = RenderTexture.active;
        RenderTexture.active = __instance.heightmap as RenderTexture;
        // var lookTexture = new Texture2D(areaImageWidth, areaImageHeight, TextureFormat.RGBA32, false);

        // areaImageX = 0;
        // areaImageY = 0;
        // areaImageWidth = imageWidth;
        // areaImageHeight = imageHeight;
        var lookTexture = new Texture2D(areaImageWidth, areaImageHeight, __instance.heightmap.graphicsFormat, TextureCreationFlags.DontInitializePixels | TextureCreationFlags.DontUploadUponCreate);

        lookTexture.ReadPixels(new Rect(areaImageX, areaImageY, areaImageWidth, areaImageHeight), 0, 0);
        // lookTexture.ReadPixels(new Rect(areaImageY, areaImageX, areaImageHeight, areaImageWidth), 0, 0);
        lookTexture.Apply();
        RenderTexture.active = oldActive;


        var bitmap = new System.Drawing.Bitmap(lookTexture.width, lookTexture.height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        for (var x = 0; x < lookTexture.width; x++)
        {
          for (var y = 0; y < lookTexture.height; y++)
          {
            var color = lookTexture.GetPixel(x, y);
            bitmap.SetPixel(x, lookTexture.height - y - 1, System.Drawing.Color.FromArgb((int)(color.r * 255), (int)(color.r * 255), (int)(color.r * 255)));
          }
        }
        bitmap.Save("C:\\Users\\jarip\\test4.png", System.Drawing.Imaging.ImageFormat.Png);


        // GraphicsFormat.U16_Norm = short
        var values = lookTexture.GetPixelData<short>(0);
        Mod.log.Info("Shorts: " + values.Length);
        Mod.log.Info("Shorts: " + values[0]);
      }
      catch (Exception e)
      {
        Mod.log.Error(e);
      }

      if (Mod.settings.TerraforminCostMultiplier > 0)
      {

        var query = __instance.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<Game.City.City>());
        var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob);
        var lookup = __instance.CheckedStateRef.GetComponentLookup<PlayerMoney>();
        var playerMoney = lookup[entities[0]];
        var cost = (int)(Mod.settings.TerraforminCostMultiplier * brush.m_Size * brush.m_Strength);
        if (playerMoney.money < cost)
        {
          entities.Dispose();
          return false;
        }
        playerMoney.Subtract(cost);
        lookup[entities[0]] = playerMoney;
        entities.Dispose();
      }

      return true;
    }
  }
}