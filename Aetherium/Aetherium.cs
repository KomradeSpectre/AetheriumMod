#undef DEBUG

using Aetherium.CoreModules;
using Aetherium.Equipment;
using Aetherium.Items;
using BepInEx;
using R2API;
using R2API.Networking;
using R2API.Utils;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Aetherium
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(BuffAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(PlayerAPI), nameof(PrefabAPI), nameof(SoundAPI), nameof(OrbAPI), nameof(NetworkingAPI), nameof(EffectAPI), nameof(EliteAPI))]
    public class AetheriumPlugin : BaseUnityPlugin
    {
        public const string ModVer = "0.4.5";
        public const string ModName = "Aetherium";
        public const string ModGuid = "com.KomradeSpectre.Aetherium";

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
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider("@Aetherium", bundle);
                ResourcesAPI.AddProvider(provider);
            }

            //Core Initilizations
            CoreModules.Add(new StatHooks());

            foreach(CoreModule coreModule in CoreModules)
            {
                coreModule.Init();
            }

            //Item Initialization
            Items.Add(new AccursedPotion());
            Items.Add(new AlienMagnet());
            Items.Add(new BlasterSword());
            Items.Add(new BloodSoakedShield());
            Items.Add(new FeatheredPlume());
            Items.Add(new InspiringDrone());
            Items.Add(new SharkTeeth());
            Items.Add(new ShieldingCore());
            Items.Add(new UnstableDesign());
            Items.Add(new Voidheart());
            Items.Add(new WeightedAnklet());
            Items.Add(new WitchesRing());

            foreach(ItemBase item in Items)
            {
                item.Init(base.Config);
            }

            //Equipment Initialization
            Equipments.Add(new JarOfReshaping());

            foreach(EquipmentBase equipments in Equipments)
            {
                equipments.Init(base.Config);
            }
        }
    }
}
