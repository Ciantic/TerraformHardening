using System.Reflection;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Simulation;
using Game.Tools;
using HarmonyLib;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

namespace America
{

    [HarmonyPatch(typeof(CityServiceBudgetSystem), nameof(CityServiceBudgetSystem.GetGovernmentSubsidy))]
    class GetGovernmentSubsidyPatch
    {
        public static void Postfix(ref int __result)
        {
            // int population, int moneyDelta, int expenses, int loanInterest

            if (Mod.settings.DisableGovernmentSubsidies)
            {
                __result = 0;
            }
        }
    }
}