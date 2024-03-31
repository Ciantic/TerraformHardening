using System.ComponentModel;
using System.Reflection;
using Colossal.Logging;
using Game;
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

namespace America
{
  [HarmonyPatch(typeof(TerrainSystem), nameof(TerrainSystem.ApplyBrush))]
  class TerraformApplyBrushPatch
  {

    public static FastInvokeHandler GetEntityQueryInvoker = null;

    //     public static void Prepare()
    //     {
    //       GetEntityQueryInvoker = MethodInvoker.GetHandler(
    //     AccessTools.Method(typeof(TerrainSystem), "GetEntityQuery")
    // );
    //     }

    // TerraformingType type, Bounds area, Brush brush, Texture texture
    public static bool Prefix(TerrainSystem __instance, TerraformingType type, Bounds area, Brush brush, Texture texture)
    {
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