using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using System.Reflection;
using UnityEngine;
using Path = System.IO.Path;
using TILER2;
using static TILER2.MiscUtil;
using RoR2;
using R2API.AssetPlus;

namespace KomradeSpectre.Aetherium
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(TILER2Plugin.ModGuid, TILER2Plugin.ModVer)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(PlayerAPI), nameof(PrefabAPI), nameof(SoundAPI))]
    public class AetheriumPlugin : BaseUnityPlugin
    {
        public const string ModVer = "0.2.0";
        public const string ModName = "Aetherium";
        public const string ModGuid = "com.KomradeSpectre.Aetherium";
        
        //private static ConfigFile cfgFile;

        internal static FilingDictionary<ItemBoilerplate> masterItemList = new FilingDictionary<ItemBoilerplate>();

        internal static BepInEx.Logging.ManualLogSource _logger;
        private static ConfigFile ConfigFile;

        private void Awake() //Sourced almost entirely from ThinkInvis' Classic Items. It is also extremely handy.
        {
            _logger = Logger;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Aetherium.aetherium_assets"))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider("@Aetherium", bundle);
                ResourcesAPI.AddProvider(provider);
            }

            ConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            masterItemList = ItemBoilerplate.InitAll("Aetherium");

            foreach (ItemBoilerplate x in masterItemList)
            {
                x.SetupConfig(ConfigFile);
            }

            int longestName = 0;
            foreach (ItemBoilerplate x in masterItemList)
            {
                x.SetupAttributes("AETHERIUM", "ATHRM");
                if (x.itemCodeName.Length > longestName) longestName = x.itemCodeName.Length;
            }

            Logger.LogMessage("Index dump follows (pairs of name / index):");
            foreach (ItemBoilerplate x in masterItemList)
            {
                if (x is Equipment eqp)
                    Logger.LogMessage("Equipment ATHRM" + x.itemCodeName.PadRight(longestName) + " / " + ((int)eqp.regIndex).ToString());
                else if (x is Item item)
                    Logger.LogMessage("     Item ATHRM" + x.itemCodeName.PadRight(longestName) + " / " + ((int)item.regIndex).ToString());
                else if (x is Artifact afct)
                    Logger.LogMessage(" Artifact ATHRM" + x.itemCodeName.PadRight(longestName) + " / " + ((int)afct.regIndex).ToString());
                else
                    Logger.LogMessage("    Other ATHRM" + x.itemCodeName.PadRight(longestName) + " / N/A");
            }

            foreach (ItemBoilerplate x in masterItemList)
            {
                x.SetupBehavior();
            }
        }

        public static CharacterModel.RendererInfo[] ItemDisplaySetup(GameObject obj)
        {
            MeshRenderer[] meshes = obj.GetComponentsInChildren<MeshRenderer>();
            CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[meshes.Length];

            for (int i = 0; i < meshes.Length; i++)
            {
                renderInfos[i] = new CharacterModel.RendererInfo 
                {
                    defaultMaterial = meshes[i].material, 
                    renderer = meshes[i], 
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false //We allow the mesh to be affected by overlays like OnFire or PredatoryInstinctsCritOverlay.
                };
            }

            return renderInfos;

        }

    }
}
