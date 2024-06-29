using System;
using Colossal.Mathematics;
using Game.City;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using HarmonyLib;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace TerraformHardening
{
  [HarmonyPatch(typeof(TerrainSystem), nameof(TerrainSystem.ApplyBrush))]
  class TerraformApplyBrushPatch
  {

    private static Texture2D HeightTextureBefore;
    private static bool RanOutOfMoney = false;
    // private static DateTime LastDump = DateTime.MinValue;

    private static Texture2D GetHeights(TerrainSystem __instance, Bounds2 area)
    {

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
        Texture2D.Destroy(lookTexture);
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
      // if (DateTime.Now - LastDump > TimeSpan.FromSeconds(3))
      // {
      //   LastDump = DateTime.Now;
      //   DumpHeightMap.DumpToFile(__instance, area, "before");
      // }

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
        // DumpHeightMap.DumpToFile(__instance, area, "after");
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