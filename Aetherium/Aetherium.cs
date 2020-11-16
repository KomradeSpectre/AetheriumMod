#undef DEBUG

using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Networking;
using R2API.Utils;
using System.Reflection;
using TILER2;
using UnityEngine;
using static TILER2.MiscUtil;
using Path = System.IO.Path;

namespace KomradeSpectre.Aetherium
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(TILER2Plugin.ModGuid, TILER2Plugin.ModVer)]
    [BepInDependency(EliteSpawningOverhaul.EsoPlugin.PluginGuid)]
    [BepInDependency("com.rob.MinerUnearthed", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.RicoValdezio.AffixGen", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(PlayerAPI), nameof(PrefabAPI), nameof(SoundAPI), nameof(OrbAPI), nameof(NetworkingAPI), nameof(EffectAPI), nameof(EliteAPI))]
    public class AetheriumPlugin : BaseUnityPlugin
    {
        public const string ModVer = "0.4.2";
        public const string ModName = "Aetherium";
        public const string ModGuid = "com.KomradeSpectre.Aetherium";
        
        //private static ConfigFile cfgFile;

        internal static FilingDictionary<CatalogBoilerplate> masterItemList = new FilingDictionary<CatalogBoilerplate>();

        internal static BepInEx.Logging.ManualLogSource _logger;
        private static ConfigFile ConfigFile;

        //Wasn't sure where to put this, so move it at your discretion, also move the check below
        internal static bool affixGenEnabled = false;

        private void Awake() //Sourced almost entirely from ThinkInvis' Classic Items. It is also extremely handy.
        {
            _logger = Logger;

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

            ConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            //This is the check for AffixGen, if you move the bool, move this too
            foreach (System.Collections.Generic.KeyValuePair<string, PluginInfo> keyValuePair in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                if (keyValuePair.Key == "com.RicoValdezio.AffixGen")
                {
                    affixGenEnabled = true;
                    break;
                }
            }

            masterItemList = T2Module.InitAll<CatalogBoilerplate>(new T2Module.ModInfo
            {
                displayName = "Aetherium",
                longIdentifier = "AETHERIUMMOD",
                shortIdentifier = "ATHRM",
                mainConfigFile = ConfigFile
            });


            T2Module.SetupAll_PluginAwake(masterItemList);
            T2Module.SetupAll_PluginStart(masterItemList);
        }

        private void Start()
        {
            CatalogBoilerplate.ConsoleDump(Logger, masterItemList);
        }

    }
}
