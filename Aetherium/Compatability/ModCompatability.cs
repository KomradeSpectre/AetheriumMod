using Aetherium.Items;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Aetherium.Utils.MathHelpers;
using System.Text;
using UnityEngine;
using R2API;
using System.Linq;

namespace Aetherium.Compatability
{
    internal static class ModCompatability
    {

        internal static class ArtifactOfTheKingCompat
        {
            public static bool IsArtifactOfTheKingInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Blobface.ArtifactKing");
        }

        internal static class ModdedCharacterDisplayCompat
        {
            public static List<ModdedCharacterItemDisplayRuleInfo> ModdedCharacterItemDisplayInfos = new List<ModdedCharacterItemDisplayRuleInfo>();
            public struct ModdedCharacterItemDisplayRuleInfo
            {
                public string BodyPrefabName;
                public ItemDisplayRule[] ItemDisplayRules;
                public UnityEngine.Object KeyAsset;

                public ModdedCharacterItemDisplayRuleInfo(string bodyPrefabName, ItemDisplayRule[] itemDisplayRules, UnityEngine.Object keyAsset)
                {
                    BodyPrefabName = bodyPrefabName;
                    ItemDisplayRules = itemDisplayRules;
                    KeyAsset = keyAsset;
                }
                
            }

            public static void Init()
            {
                RoR2Application.onLoad += PerformModdedDisplayAdditions;
            }

            private static void PerformModdedDisplayAdditions()
            {
                HashSet<CharacterModel> ToGenerate = new HashSet<CharacterModel>();

                foreach (var moddedCharacterItemDisplay in ModdedCharacterItemDisplayInfos)
                {
                    GameObject bodyPrefab = RoR2.BodyCatalog.FindBodyPrefab(moddedCharacterItemDisplay.BodyPrefabName);
                    if(!bodyPrefab)
                    {
                        AetheriumPlugin.ModLogger.LogDebug($"Didn't find BodyPrefab for {moddedCharacterItemDisplay.BodyPrefabName}");
                        continue;
                    }
                    RoR2.ModelLocator modelLocator = bodyPrefab.GetComponent<RoR2.ModelLocator>();
                    if(!modelLocator || !modelLocator.modelTransform)
                    {
                        AetheriumPlugin.ModLogger.LogDebug($"ModelLocator for {bodyPrefab} was not found.");
                        continue;
                    }
                    RoR2.CharacterModel characterModel = modelLocator.modelTransform.gameObject.GetComponent<RoR2.CharacterModel>();
                    if(characterModel)
                    {
                        ToGenerate.Add(characterModel);

                        List<RoR2.ItemDisplayRuleSet.KeyAssetRuleGroup> list = characterModel.itemDisplayRuleSet.keyAssetRuleGroups.ToList();
                        list.Add(new RoR2.ItemDisplayRuleSet.KeyAssetRuleGroup
                        {
                            keyAsset = moddedCharacterItemDisplay.KeyAsset,
                            displayRuleGroup = new RoR2.DisplayRuleGroup
                            {
                                rules = moddedCharacterItemDisplay.ItemDisplayRules
                            }
                        });
                        characterModel.itemDisplayRuleSet.keyAssetRuleGroups = list.ToArray();

                        AetheriumPlugin.ModLogger.LogDebug($"Successfully added modded display {moddedCharacterItemDisplay.KeyAsset.name} for {bodyPrefab.name}");
                    }
                }

                foreach (CharacterModel model in ToGenerate)
                {
                    model.itemDisplayRuleSet.GenerateRuntimeValues();
                }
            }

            public static void AddModdedCharacterItemDisplayInfo(string bodyPrefabName, ItemDisplayRule[] itemDisplayRules, UnityEngine.Object keyAsset)
            {
                if(String.IsNullOrWhiteSpace(bodyPrefabName) || itemDisplayRules.Length <= 0 || keyAsset.GetType() != typeof(ItemDef) && keyAsset.GetType() != typeof(EquipmentDef))
                {
                    return;
                }

                ModdedCharacterItemDisplayInfos.Add(new ModdedCharacterItemDisplayRuleInfo()
                {
                    BodyPrefabName = bodyPrefabName,
                    ItemDisplayRules = itemDisplayRules,
                    KeyAsset = keyAsset
                });
            }
        }
    }
}
