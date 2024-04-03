using System;
using Colossal.Mathematics;
using Game.City;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using HarmonyLib;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace America
{
  [HarmonyPatch(typeof(TerrainSystem), nameof(TerrainSystem.ApplyBrush))]
  class TerraformApplyBrushPatch
  {

    private static Texture2D HeightTextureBefore;
    private static bool RanOutOfMoney = false;

    private static Texture2D GetHeights(TerrainSystem __instance, Bounds2 area)
    {
      area.min -= __instance.playableOffset;
      area.max -= __instance.playableOffset;
      area.min /= __instance.playableArea;
      area.max /= __instance.playableArea;

      var imageWidth = __instance.heightmap.width;
      var imageHeight = __instance.heightmap.height;

      // Scale the area coordinates to image coordinates
      var areaImageWidth = (int)Math.Ceiling(area.Size().x * imageWidth);
      var areaImageHeight = (int)Math.Ceiling(area.Size().y * imageHeight);
      var areaImageX = (int)Math.Floor(area.x.min * imageWidth);
      var areaImageY = imageHeight - (int)Math.Floor(area.y.min * imageHeight) - areaImageHeight - 1;
      var lookTexture = new Texture2D(areaImageWidth, areaImageHeight, __instance.heightmap.graphicsFormat, TextureCreationFlags.DontInitializePixels | TextureCreationFlags.DontUploadUponCreate);
      var oldActive = RenderTexture.active;
      RenderTexture.active = __instance.heightmap as RenderTexture;
      try
      {
        lookTexture.ReadPixels(new Rect(areaImageX, areaImageY, areaImageWidth, areaImageHeight), 0, 0);
        lookTexture.Apply();
        RenderTexture.active = oldActive;
        return lookTexture;
      }
      catch (Exception e)
      {
        Mod.log.Error(e);
        RenderTexture.active = oldActive;
        throw e;
      }
    }

    // TerraformingType type, Bounds area, Brush brush, Texture texture
    public static bool Prefix(TerrainSystem __instance, TerraformingType type, Bounds2 area, Brush brush, Texture texture)
    {
      if (Mod.settings.TerraformingCostMultiplier == 0)
      {
        return true;
      }

      // Check if player has no money, don't allow terraforming
      {
        var query = __instance.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Game.City.City>());
        var entities = query.ToEntityArray(Allocator.Temp);
        var lookup = __instance.CheckedStateRef.GetComponentLookup<PlayerMoney>();
        var playerMoney = lookup[entities[0]];
        var money = playerMoney.money;
        entities.Dispose();
        if (money <= 0)
        {
          // Mod.log.Info("Not enough money to terraform");
          RanOutOfMoney = true;
          return false;
        }
        else
        {
          RanOutOfMoney = false;
        }
      }

      // Store heights before terraforming
      HeightTextureBefore = GetHeights(__instance, area);

      return true;
    }

    public static void Postfix(TerrainSystem __instance, TerraformingType type, Bounds2 area, Brush brush, Texture texture)
    {
      if (Mod.settings.TerraformingCostMultiplier == 0 || RanOutOfMoney)
      {
        return;
      }

      // Compare old heights with new heights and get difference in meters (diffInMeters)
      float diffInMeters;
      {
        var heightTextureAfter = GetHeights(__instance, area);
        var heightsAfter = heightTextureAfter.GetPixelData<short>(0);
        var heightsBefore = HeightTextureBefore.GetPixelData<short>(0);
        var diff = 0;
        for (int i = 0; i < heightsAfter.Length; i++)
        {
          diff += Math.Abs(heightsAfter[i] - heightsBefore[i]);
        }
        Texture2D.Destroy(heightTextureAfter);
        Texture2D.Destroy(HeightTextureBefore);
        HeightTextureBefore = null;

        var scaler = short.MaxValue / __instance.heightScaleOffset.x;
        diffInMeters = diff / scaler;
      }

      // Mod.log.Info($"TerraformApplyBrushPatch: {__instance.heightScaleOffset} heightmap changes");
      // Mod.log.Info($"TerraformApplyBrushPatch: {diffInMeters} heightmap changes");

      // Reduce user money by cost of terraforming
      {
        /*
          Notes: 

          WaterSystem.HasWater could be used to check if terraformed area is underwater, and adjust the price accordingly
        */

        var query = __instance.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<Game.City.City>());
        var entities = query.ToEntityArray(Allocator.Temp);
        var lookup = __instance.CheckedStateRef.GetComponentLookup<PlayerMoney>();
        var playerMoney = lookup[entities[0]];
        var cost = (int)(Mod.settings.TerraformingCostMultiplier * diffInMeters);
        playerMoney.Subtract(cost);
        lookup[entities[0]] = playerMoney;
        entities.Dispose();
      }
    }
  }

}