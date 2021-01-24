#undef DEBUG

using Aetherium.CoreModules;
using Aetherium.Equipment;
using Aetherium.Items;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Networking;
using R2API.Utils;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace Aetherium
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(BuffAPI), nameof(LanguageAPI), nameof(ResourcesAPI),
                              nameof(PlayerAPI), nameof(PrefabAPI), nameof(SoundAPI), nameof(OrbAPI),
                              nameof(NetworkingAPI), nameof(EffectAPI))]
    public class AetheriumPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.KomradeSpectre.Aetherium";
        public const string ModName = "Aetherium";
        public const string ModVer = "0.4.7";

        public static AssetBundle MainAssets;
        public static Shader HopooShader = Resources.Load<Shader>("shaders/deferred/hgstandard");

        public List<CoreModule> CoreModules = new List<CoreModule>();
        public List<ItemBase> Items = new List<ItemBase>();
        public List<EquipmentBase> Equipments = new List<EquipmentBase>();

        private void Awake()
        {
#if DEBUG
            Logger.LogWarning("DEBUG mode is enabled! Ignore this message if you are actually debugging.");
            On.RoR2.Networking.GameNetworkManager.OnClientConnect += (self, user, t) => { };
#endif

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Aetherium.aetherium_assets"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider("@Aetherium", MainAssets);
                ResourcesAPI.AddProvider(provider);
            }

            //Core Initializations
            CoreModules.Add(new StatHooks());

            foreach (CoreModule coreModule in CoreModules)
            {
                coreModule.Init();
            }

            //Item Initialization
            ValidateItem(new AccursedPotion(), Items);
            ValidateItem(new AlienMagnet(), Items);
            ValidateItem(new BlasterSword(), Items);
            ValidateItem(new BloodSoakedShield(), Items);
            ValidateItem(new FeatheredPlume(), Items);
            ValidateItem(new InspiringDrone(), Items);
            ValidateItem(new SharkTeeth(), Items);
            ValidateItem(new ShieldingCore(), Items);
            ValidateItem(new UnstableDesign(), Items);
            ValidateItem(new Voidheart(), Items);
            ValidateItem(new WeightedAnklet(), Items);
            ValidateItem(new WitchesRing(), Items);

            foreach (ItemBase item in Items)
            {
                item.Init(base.Config);
            }

            //Equipment Initialization
            EquipmentEnabledCheck(new JarOfReshaping(), Equipments);

            foreach (EquipmentBase equipments in Equipments)
            {
                equipments.Init(base.Config);
            }
        }

        public void ValidateItem(ItemBase item, List<ItemBase> itemList)
        {
            var enabled = Config.Bind<bool>("Item: " + item.ItemName, "Enable Item?", true, "Should this item appear in runs?").Value;
            var aiBlacklist = Config.Bind<bool>("Item: " + item.ItemName, "Blacklist Item from AI Use?", false, "Should the AI not be able to obtain this item?").Value;
            if (enabled) 
            {
                itemList.Add(item);
                if(aiBlacklist) 
                {
                    item.AIBlacklisted = true;
                }
            }
        }

        public void EquipmentEnabledCheck(EquipmentBase equipment, List<EquipmentBase> equipmentList)
        {
            if (Config.Bind<bool>("Equipment: " + equipment.EquipmentName, "Enable Equipment?", true, "Should this equipment appear in runs?").Value) { equipmentList.Add(equipment); }
        }
    }
}