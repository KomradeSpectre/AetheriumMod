﻿using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.CoreModules.StatHooks;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;

namespace Aetherium.Items
{
    public class WeightedAnklet : ItemBase<WeightedAnklet>
    {
        public static ConfigEntry<float> BaseKnockbackReductionPercentage;
        public static ConfigEntry<float> BaseMovementSpeedReductionPercentage;
        public static ConfigEntry<float> MovementSpeedReductionPercentageCap;

        public override string ItemName => "Weighted Anklet";

        public override string ItemLangTokenName => "WEIGHTED_ANKLET";

        public override string ItemPickupDesc => "Gain resistance to <style=cIsDamage>knockback</style>, BUT <style=cIsUtility>lose speed</style>.";

        public override string ItemFullDescription => $"Gain a {FloatToPercentageString(BaseKnockbackReductionPercentage.Value)} reduction to knockback from attacks <style=cStack>(+{FloatToPercentageString(BaseKnockbackReductionPercentage.Value)} per stack (up to 100%) linearly)</style>. Lose {FloatToPercentageString(BaseMovementSpeedReductionPercentage.Value)} move speed <style=cStack>(+{FloatToPercentageString(BaseMovementSpeedReductionPercentage.Value)} per stack (up to {FloatToPercentageString(1 - MovementSpeedReductionPercentageCap.Value)}) linearly)</style>.";

        public override string ItemLore => "An old anklet lined with strangely superdense crystals. FOREWARNING: Please take care of how many you put on if you find these. One of our field testers put 10 of these on during testing, and attempts to move him since have failed.";

        public override ItemTier Tier => ItemTier.Lunar;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Cleansable };

        public override string ItemModelPath => "@Aetherium:Assets/Models/Prefabs/Item/WeightedAnklet/WeightedAnklet.prefab";
        public override string ItemIconPath => "@Aetherium:Assets/Textures/Icons/Item/WeightedAnkletIcon.png";


        public static GameObject ItemBodyModelPrefab;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            BaseKnockbackReductionPercentage = config.Bind<float>(ItemName, "Base Knockback Reduction Percentage", 0.25f, "How much knockback reduction in percentage should be given for each Weighted Anklet?");
            BaseMovementSpeedReductionPercentage = config.Bind<float>(ItemName, "Base Movement Speed Reduction Percentage", 0.1f, "How much movement speed in percentage should be reduced per Weighted Anklet?");
            MovementSpeedReductionPercentageCap = config.Bind<float>(ItemName, "Absolute Lowest Movement Speed Reduction Percentage", 0.6f, "What should be the lowest percentage of movespeed reduction be?");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = Resources.Load<GameObject>(ItemModelPath);
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.32f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)

                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.4f, 0.02f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 3f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(2, 2, 2)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.25f, 0f),
                    localAngles = new Vector3(-19f, 0f, -4f),
                    localScale = new Vector3(0.28f, 0.28f, 0.28f)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.41f, 0.02f),
                    localAngles = new Vector3(-5f, 0f, 0f),
                    localScale = new Vector3(0.19f, 0.19f, 0.19f)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.32f, 0.025f),
                    localAngles = new Vector3(-10f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FootFrontL",
                    localPos = new Vector3(0f, 1f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.4f, 0.4f, 0.4f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.01f, 0.39f, 0.02f),
                    localAngles = new Vector3(-6f, 0f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 3f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1.5f, 1.5f, 1.5f)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.39f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            return rules;

        }

        public override void Hooks()
        {
            GetStatCoefficients += MoveSpeedReduction;
            On.RoR2.HealthComponent.TakeDamage += ReduceKnockback;
        }

        private void MoveSpeedReduction(CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            if (InventoryCount > 0)
            {
                args.moveSpeedMultAdd -= Mathf.Min(InventoryCount * BaseMovementSpeedReductionPercentage.Value, MovementSpeedReductionPercentageCap.Value);
            }
        }

        private void ReduceKnockback(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            var InventoryCount = GetCount(self.body);
            if (InventoryCount > 0)
            {
                var percentReduction = Mathf.Clamp(1 - (InventoryCount * BaseKnockbackReductionPercentage.Value), 0, 1);
                damageInfo.force *= percentReduction;
            }
            orig(self, damageInfo);
        }
    }
}
