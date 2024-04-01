using System.Reflection;
using Colossal.IO.AssetDatabase;
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
    public class Mod : IMod
    {
        public static Harmony harmony;
        public static ILog log = LogManager.GetLogger($"{nameof(America)}.{nameof(Mod)}").SetShowsErrorsInUI(true);

        public static Settings settings;

        public void OnLoad(UpdateSystem updateSystem)
        {
            // log.Info("Loading America");

            settings = new Settings(this);
            settings.RegisterInOptionsUI();

            log.Info("Hello America!");

            GameManager.instance.localizationManager.AddSource("en-US", Settings.GetLocales(settings));
            AssetDatabase.global.LoadSettings(nameof(America), settings, new Settings(this));

            harmony = new Harmony("america");
            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (System.Exception e)
            {
                log.Error(e);
            }

            // if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            //     log.Info($"Current mod asset at {asset.path}");
        }

        public void OnDispose()
        {
            // log.Info(nameof(OnDispose));
            harmony.UnpatchAll();
        }
    }

}
