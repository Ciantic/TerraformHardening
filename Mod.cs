using System.Reflection;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.Simulation;
using HarmonyLib;
using Unity.Entities.UniversalDelegates;

namespace America
{
    public class Mod : IMod
    {
        public static Harmony harmony;
        public static ILog log = LogManager.GetLogger($"{nameof(America)}.{nameof(Mod)}").SetShowsErrorsInUI(true);

        public void OnLoad(UpdateSystem updateSystem)
        {
            harmony = new Harmony("america");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            //     log.Info($"Current mod asset at {asset.path}");
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }
    }

    [HarmonyPatch(typeof(CityServiceBudgetSystem), nameof(CityServiceBudgetSystem.GetGovernmentSubsidy))]
    class GetGovernmentSubsidyPatch
    {
        public static void Postfix(ref int __result)
        {
            // TODO: This could be negative, Government only takes money from the player not give it
            __result = 0;
        }
    }
}
