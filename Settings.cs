using System.Collections.Generic;
using System.Reflection;
using Colossal;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Settings;
using Game.Simulation;
using Game.Tools;
using Game.UI;
using HarmonyLib;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

namespace America
{


    [FileLocation(nameof(America))]
    public class Settings : ModSetting
    {

        public Settings(IMod mod) : base(mod)
        {
        }

        public override void SetDefaults()
        {

        }

        [SettingsUISlider(min = 0, max = 10000, step = 1f, scaleDragVolume = true, scalarMultiplier = 1, unit = Unit.kFloatTwoFractions)]
        public float TerraforminCostMultiplier
        {
            get;
            set;
        } = 100f;

        [SettingsUIConfirmation]
        public bool DisableGovernmentSubsidies { get; set; } = true;

        public static IDictionarySource GetLocales(Settings settings)
        {
            var translations = new Dictionary<string, string> {
                { settings.GetSettingsLocaleID(), "America" },
                { settings.GetOptionLabelLocaleID(nameof(DisableGovernmentSubsidies)), "Disable Government Subsidies" },
                { settings.GetOptionDescLocaleID(nameof(DisableGovernmentSubsidies)), "Sets government subsidy to zero" },
                // { settings.GetOptionGroupLocaleID("Settings"), "Settings" },
                { settings.GetOptionLabelLocaleID(nameof(TerraforminCostMultiplier)), "Terraforming cost multiplier" },
                { settings.GetOptionDescLocaleID(nameof(TerraforminCostMultiplier)), @"
                Cost will be: multiplier * brushSize * brushStrength.
                
                For example if multiplier is 45, brush is sized 100 and brush strength of 50% then the cost will be 45 * 100 * 0.5 = 2250.
                
                Note: Currently all terraforming actions has a cost, even if they don't cause any effect for example leveling already leveled terrain."}
            };

            return new LocaleDictionary(translations);
        }

    }

    public struct LocaleDictionary : IDictionarySource
    {
        private Dictionary<string, string> dictionary;
        public LocaleDictionary(Dictionary<string, string> dictionary)
        {
            this.dictionary = dictionary;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return dictionary;
        }

        public void Unload()
        {

        }
    }

}