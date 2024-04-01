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

        [SettingsUISlider(min = 0, max = 10, step = 0.1f, scaleDragVolume = true, scalarMultiplier = 1, unit = Unit.kFloatTwoFractions)]
        public float TerraformingCostMultiplier
        {
            get;
            set;
        } = 3f;

        [SettingsUIConfirmation]
        public bool DisableGovernmentSubsidies { get; set; } = true;

        public static IDictionarySource GetLocales(Settings settings)
        {
            var translations = new Dictionary<string, string> {
                { settings.GetSettingsLocaleID(), "America" },
                { settings.GetOptionLabelLocaleID(nameof(DisableGovernmentSubsidies)), "Disable Government Subsidies" },
                { settings.GetOptionDescLocaleID(nameof(DisableGovernmentSubsidies)), "Sets government subsidy to zero" },
                // { settings.GetOptionGroupLocaleID("Settings"), "Settings" },
                { settings.GetOptionLabelLocaleID(nameof(TerraformingCostMultiplier)), "Terraforming cost multiplier" },
                { settings.GetOptionDescLocaleID(nameof(TerraformingCostMultiplier)), @"
                Cost will be: multiplier * differenceInHeight.
                
                For instance if you level only the parts that was leveled will cost something."}
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