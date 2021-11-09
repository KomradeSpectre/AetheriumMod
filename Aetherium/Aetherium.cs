#define DEBUG
#undef DEBUGMULTIPLAYER
#define DEBUGMATERIALS

using Aetherium.Artifacts;
using Aetherium.StandaloneBuffs;
using Aetherium.CoreModules;
using Aetherium.Equipment;
using Aetherium.Equipment.EliteEquipment;
using Aetherium.Interactables;
using Aetherium.Items;
using Aetherium.Utils;
using BepInEx;
using InLobbyConfig.Fields;
using R2API;
using R2API.Networking;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Aetherium
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency(TILER2.TILER2Plugin.ModGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("dev.ontrigger.itemstats", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.xoxfaby.BetterUI", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(BuffAPI), nameof(LanguageAPI), nameof(ResourcesAPI),
                              nameof(PrefabAPI), nameof(SoundAPI), nameof(OrbAPI),
                              nameof(NetworkingAPI), nameof(EffectAPI), nameof(DirectorAPI), nameof(ProjectileAPI), nameof(ArtifactAPI), nameof(RecalculateStatsAPI), nameof(UnlockableAPI), nameof(EliteAPI), nameof(LoadoutAPI))]
    public class AetheriumPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.KomradeSpectre.Aetherium";
        public const string ModName = "Aetherium";
        public const string ModVer = "0.6.4";

        internal static BepInEx.Logging.ManualLogSource ModLogger;

        public static AssetBundle MainAssets;

        public static Dictionary<string, string> ShaderLookup = new Dictionary<string, string>()
        {
            {"fake ror/hopoo games/deferred/hgstandard", "shaders/deferred/hgstandard"},
            {"fake ror/hopoo games/fx/hgcloud intersection remap", "shaders/fx/hgintersectioncloudremap" },
            {"fake ror/hopoo games/fx/hgcloud remap", "shaders/fx/hgcloudremap" },
            {"fake ror/hopoo games/fx/hgdistortion", "shaders/fx/hgdistortion" },
            {"fake ror/hopoo games/deferred/hgsnow topped", "shaders/deferred/hgsnowtopped" },
            {"fake ror/hopoo games/fx/hgsolid parallax", "shaders/fx/hgsolidparallax" }
        };

        public List<CoreModule> CoreModules = new List<CoreModule>();
        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
        public List<BuffBase> Buffs = new List<BuffBase>();
        public List<ItemBase> Items = new List<ItemBase>();
        public List<EquipmentBase> Equipments = new List<EquipmentBase>();
        public List<EliteEquipmentBase> EliteEquipments = new List<EliteEquipmentBase>();
        public List<InteractableBase> Interactables = new List<InteractableBase>();

        public static HashSet<ItemDef> BlacklistedFromPrinter = new HashSet<ItemDef>();

        // For modders that seek to know whether or not one of the items or equipment are enabled for use in...I dunno, adding grip to Blaster Sword?
        public static Dictionary<ArtifactBase, bool> ArtifactStatusDictionary = new Dictionary<ArtifactBase, bool>();
        public static Dictionary<BuffBase, bool> BuffStatusDictionary = new Dictionary<BuffBase, bool>();
        public static Dictionary<ItemBase, bool> ItemStatusDictionary = new Dictionary<ItemBase, bool>();
        public static Dictionary<EquipmentBase, bool> EquipmentStatusDictionary = new Dictionary<EquipmentBase, bool>();
        public static Dictionary<EliteEquipmentBase, bool> EliteEquipmentStatusDictionary = new Dictionary<EliteEquipmentBase, bool>();
        public static Dictionary<InteractableBase, bool> InteractableStatusDictionary = new Dictionary<InteractableBase, bool>();

        private void Awake()
        {
            #if DEBUG
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
            #endif

            #if DEBUGMULTIPLAYER
            Logger.LogWarning("DEBUG mode is enabled! Ignore this message if you are actually debugging.");
            On.RoR2.Networking.GameNetworkManager.OnClientConnect += (self, user, t) => { };
            #endif

            ModLogger = this.Logger;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Aetherium.aetherium_assets"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }

            using (var bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Aetherium.AetheriumSounds.bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                SoundAPI.SoundBanks.Add(bytes);
            }

            //Material shader autoconversion
            ShaderConversion(MainAssets);

            #if DEBUGMATERIALS
            AttachControllerFinderToObjects(MainAssets);
            #endif

            //Core Initializations
            var CoreModuleTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(CoreModule)));

            ModLogger.LogInfo("--------------CORE MODULES---------------------");

            foreach (var coreModuleType in CoreModuleTypes)
            {
                CoreModule coreModule = (CoreModule)Activator.CreateInstance(coreModuleType);

                coreModule.Init();

                ModLogger.LogInfo("Core Module: " + coreModule.Name + " Initialized!");
            }

            //Achievement

            var disableArtifacts = Config.ActiveBind<bool>("Artifacts", "Disable All Artifacts?", false, "Do you wish to disable every artifact in Aetherium?");
            if (!disableArtifacts)
            {
                //Artifact Initialization
                var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));

                ModLogger.LogInfo("--------------ARTIFACTS---------------------");

                foreach (var artifactType in ArtifactTypes)
                {
                    ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                    if (ValidateArtifact(artifact, Artifacts))
                    {
                        artifact.Init(Config);

                        ModLogger.LogInfo("Artifact: " + artifact.ArtifactName + " Initialized!");
                    }
                }
            }

            var disableBuffs = Config.ActiveBind<bool>("Artifacts", "Disable All Standalone Buff?", false, "Do you wish to disable every standalone buff in Aetherium?");
            if (!disableBuffs)
            {
                //Standalone Buff Initialization
                var BuffTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(BuffBase)));

                ModLogger.LogInfo("--------------BUFFS---------------------");

                foreach (var buffType in BuffTypes)
                {
                    BuffBase buff = (BuffBase)Activator.CreateInstance(buffType);
                    if (ValidateBuff(buff, Buffs))
                    {
                        buff.Init(Config);

                        ModLogger.LogInfo("Buff: " + buff.BuffName + " Initialized!");
                    }
                }
            }

            var disableItems = Config.ActiveBind<bool>("Items", "Disable All Items?", false, "Do you wish to disable every item in Aetherium?");
            if (!disableItems)
            {
                //Item Initialization
                var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));

                ModLogger.LogInfo("----------------------ITEMS--------------------");

                foreach (var itemType in ItemTypes)
                {
                    ItemBase item = (ItemBase)System.Activator.CreateInstance(itemType);
                    if (ValidateItem(item, Items))
                    {
                        item.Init(Config);

                        ModLogger.LogInfo("Item: " + item.ItemName + " Initialized!");
                    }
                }

                IL.RoR2.ShopTerminalBehavior.GenerateNewPickupServer += ItemBase.BlacklistFromPrinter;
            }

            var disableEquipment = Config.ActiveBind<bool>("Equipment", "Disable All Equipment?", false, "Do you wish to disable every equipment in Aetherium?");
            if (!disableEquipment)
            {
                //Equipment Initialization
                var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));

                ModLogger.LogInfo("-----------------EQUIPMENT---------------------");

                foreach (var equipmentType in EquipmentTypes)
                {
                    EquipmentBase equipment = (EquipmentBase)System.Activator.CreateInstance(equipmentType);
                    if (ValidateEquipment(equipment, Equipments))
                    {
                        equipment.Init(Config);

                        ModLogger.LogInfo("Equipment: " + equipment.EquipmentName + " Initialized!");
                    }
                }
            }

            //Equipment Initialization
            var EliteEquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EliteEquipmentBase)));

            ModLogger.LogInfo("-------------ELITE EQUIPMENT---------------------");

            foreach (var eliteEquipmentType in EliteEquipmentTypes)
            {
                EliteEquipmentBase eliteEquipment = (EliteEquipmentBase)System.Activator.CreateInstance(eliteEquipmentType);
                if (ValidateEliteEquipment(eliteEquipment, EliteEquipments))
                {
                    eliteEquipment.Init(Config);

                    ModLogger.LogInfo("Elite Equipment: " + eliteEquipment.EliteEquipmentName + " Initialized!");
                }
            }

            //Interactables Initialization
            var InteractableTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(InteractableBase)));

            ModLogger.LogInfo("-----------------INTERACTABLES---------------------");

            foreach(var interactableType in InteractableTypes)
            {
                InteractableBase interactable = (InteractableBase)System.Activator.CreateInstance(interactableType);
                if(ValidateInteractable(interactable, Interactables))
                {
                    interactable.Init(Config);
                    ModLogger.LogInfo("Interactable: " + interactable.InteractableName + " Initialized!");
                }
            }

            Compatability.ModCompatability.ModdedCharacterDisplayCompat.Init();

            ModLogger.LogInfo("-----------------------------------------------");
            ModLogger.LogInfo("AETHERIUM INITIALIZATIONS DONE");

            if (ArtifactStatusDictionary.Count > 0) { ModLogger.LogInfo($"Artifacts Enabled: {ArtifactStatusDictionary.Count}"); }

            if (BuffStatusDictionary.Count > 0) { ModLogger.LogInfo($"Standalone Buffs Enabled: {BuffStatusDictionary.Count}"); }

            if (ItemStatusDictionary.Count > 0) { ModLogger.LogInfo($"Items Enabled: {ItemStatusDictionary.Count}"); }

            if (EquipmentStatusDictionary.Count > 0){ ModLogger.LogInfo($"Equipment Enabled: {EquipmentStatusDictionary.Count}"); }

            if (EliteEquipmentStatusDictionary.Count > 0){ ModLogger.LogInfo($"Elite Equipment Enabled: {EliteEquipmentStatusDictionary.Count}"); }

            if (InteractableStatusDictionary.Count > 0){ ModLogger.LogInfo($"Interactables Enabled: {InteractableStatusDictionary.Count}"); }

            ModLogger.LogInfo("-----------------------------------------------");
        }

        public bool ValidateArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            var enabled = Config.Bind<bool>("Artifact: " + artifact.ArtifactName, "Enable Artifact?", true, "Should this artifact appear for selection?").Value;

            ArtifactStatusDictionary.Add(artifact, enabled);

            if (enabled)
            {
                artifactList.Add(artifact);
            }
            return enabled;
        }

        public bool ValidateBuff(BuffBase buff, List<BuffBase> buffList)
        {
            var enabled = Config.Bind<bool>("Buff: " + buff.BuffName, "Enable Buff?", true, "Should this buff be registered for use in the game?").Value;

            BuffStatusDictionary.Add(buff, enabled);

            if (enabled)
            {
                buffList.Add(buff);
            }
            return enabled;
        }

        public bool ValidateItem(ItemBase item, List<ItemBase> itemList)
        {
            var enabled = Config.Bind<bool>("Item: " + item.ItemName, "Enable Item?", true, "Should this item appear in runs?").Value;
            var aiBlacklist = Config.Bind<bool>("Item: " + item.ItemName, "Blacklist Item from AI Use?", false, "Should the AI not be able to obtain this item?").Value;
            var printerBlacklist = Config.Bind<bool>("Item: " + item.ItemName, "Blacklist Item from Printers?", false, "Should the printers be able to print this item?").Value;

            ItemStatusDictionary.Add(item, enabled);

            if (enabled)
            {
                itemList.Add(item);
                if (aiBlacklist)
                {
                    item.AIBlacklisted = true;
                }
                if (printerBlacklist)
                {
                    item.PrinterBlacklisted = true;
                }
            }
            return enabled;
        }

        public bool ValidateEquipment(EquipmentBase equipment, List<EquipmentBase> equipmentList)
        {
            var enabled = Config.Bind<bool>("Equipment: " + equipment.EquipmentName, "Enable Equipment?", true, "Should this equipment appear in runs?").Value;

            EquipmentStatusDictionary.Add(equipment, enabled);

            if (enabled)
            {
                equipmentList.Add(equipment);
                return true;
            }
            return false;
        }

        public bool ValidateEliteEquipment(EliteEquipmentBase eliteEquipment, List<EliteEquipmentBase> eliteEquipmentList)
        {
            var enabled = Config.Bind<bool>("Equipment: " + eliteEquipment.EliteEquipmentName, "Enable Elite Equipment?", true, "Should this elite equipment appear in runs? If disabled, the associated elite will not appear in runs either.").Value;

            EliteEquipmentStatusDictionary.Add(eliteEquipment, enabled);

            if (enabled)
            {
                eliteEquipmentList.Add(eliteEquipment);
                return true;
            }
            return false;
        }

        public bool ValidateInteractable(InteractableBase interactable, List<InteractableBase> interactableList)
        {
            var enabled = Config.Bind<bool>("Interactable: " + interactable.InteractableName, "Enable Interactable?", true, "Should this interactable appear in runs?").Value;

            InteractableStatusDictionary.Add(interactable, enabled);

            if (enabled)
            {
                interactableList.Add(interactable);
                return true;
            }
            return false;
        }

        public static void ShaderConversion(AssetBundle assets)
        {
            var materialAssets = assets.LoadAllAssets<Material>().Where(material => material.shader.name.StartsWith("Fake RoR"));

            foreach (Material material in materialAssets)
            {
                var replacementShader = Resources.Load<Shader>(ShaderLookup[material.shader.name.ToLowerInvariant()]);
                if (replacementShader) { material.shader = replacementShader; }
            }
        }

        public static void AttachControllerFinderToObjects(AssetBundle assetbundle)
        {
            if (!assetbundle) { return; }

            var gameObjects = assetbundle.LoadAllAssets<GameObject>();

            foreach (GameObject gameObject in gameObjects)
            {
                var foundRenderers = gameObject.GetComponentsInChildren<Renderer>().Where(x => x.sharedMaterial && x.sharedMaterial.shader.name.StartsWith("Hopoo Games"));

                foreach (Renderer renderer in foundRenderers)
                {
                    var controller = renderer.gameObject.AddComponent<MaterialControllerComponents.HGControllerFinder>();
                    controller.Renderer = renderer;
                }
            }

            gameObjects = null;
        }
    }
}