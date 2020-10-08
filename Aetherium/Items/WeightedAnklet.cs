using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using static TILER2.MiscUtil;
using System;
using KomradeSpectre.Aetherium;

namespace Aetherium.Items
{
    public class WeightedAnklet : Item<WeightedAnklet>
    {
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("How much knockback reduction in percentage should be given for each Weighted Anklet? (Default: 0.25 (25%))", AutoItemConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float baseKnockbackReductionPercentage { get; private set; } = 0.25f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("How much movement speed in percentage should be reduced per Weighted Anklet? (Default: 0.1 (10%))", AutoItemConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float baseMovementSpeedReductionPercentage { get; private set; } = 0.1f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("What should be the lowest percentage of movespeed reduction be? (Default: 0.6 (means only 40% move speed can be lost total))", AutoItemConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float movementSpeedReductionPercentageCap { get; private set; } = 0.6f;

        public override string displayName => "Weighted Anklet";

        public override ItemTier itemTier => RoR2.ItemTier.Lunar;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Cleansable });
        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "Gain resistance to <style=cIsDamage>knockback</style>, BUT <style=cIsUtility>lose speed</style>.";

        protected override string NewLangDesc(string langid = null) => $"Gain a {Pct(baseKnockbackReductionPercentage)} reduction to knockback from attacks <style=cStack>(+{Pct(baseKnockbackReductionPercentage)} per stack (up to 100%) linearly)</style>. Lose {Pct(baseMovementSpeedReductionPercentage)} move speed <style=cStack>(+{Pct(baseMovementSpeedReductionPercentage)} per stack (up to {Pct(1 - movementSpeedReductionPercentageCap)}) linearly)</style>.";

        protected override string NewLangLore(string langID = null) => "An old anklet lined with strangely superdense crystals. FOREWARNING: Please take care of how many you put on if you find these. One of our field testers put 10 of these on during testing, and attempts to move him since have failed.";

        public static GameObject ItemBodyModelPrefab;

        public WeightedAnklet() 
        {
            modelPathName = "@Aetherium:Assets/Models/Prefabs/WeightedAnklet.prefab";
            iconPathName = "@Aetherium:Assets/Textures/Icons/WeightedAnkletIcon.png";
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = AetheriumPlugin.ItemDisplaySetup(ItemBodyModelPrefab);

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

        protected override void LoadBehavior()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = regDef.pickupModelPrefab;
                regItem.ItemDisplayRules = GenerateItemDisplayRules();
            }
            On.RoR2.CharacterBody.RecalculateStats += MoveSpeedReduction;
            On.RoR2.HealthComponent.TakeDamage += ReduceKnockback;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.CharacterBody.RecalculateStats -= MoveSpeedReduction;
            On.RoR2.HealthComponent.TakeDamage -= ReduceKnockback;
        }

        private void MoveSpeedReduction(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
        {
            orig(self);
            var InventoryCount = GetCount(self);
            if (InventoryCount > 0)
            {
                self.moveSpeed *= Mathf.Clamp(1 - (InventoryCount * baseMovementSpeedReductionPercentage), movementSpeedReductionPercentageCap, 1);
            }
        }

        private void ReduceKnockback(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            var InventoryCount = GetCount(self.body);
            if(InventoryCount > 0)
            {
                var percentReduction = Mathf.Clamp(1 - (InventoryCount * baseKnockbackReductionPercentage), 0, 1);
                damageInfo.force = damageInfo.force * percentReduction;
            }
            orig(self, damageInfo);
        }
    }
}
