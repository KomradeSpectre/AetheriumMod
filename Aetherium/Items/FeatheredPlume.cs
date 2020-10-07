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
    public class FeatheredPlume : Item<FeatheredPlume>
    {
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("If set to true, will use the new icon art drawn by WaltzingPhantom, else it will use the old icon art. Client only.", AutoItemConfigFlags.None)]
        public bool useNewIcons { get; private set; } = true;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("How many seconds should feathered plume's buff last with a single stack? (Default: 5 seconds)", AutoItemConfigFlags.PreventNetMismatch)]
        public float baseDurationOfBuffInSeconds { get; private set; } = 5;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("How many additional seconds of buff should each feathered plume after the first give? (Default: 0.5f seconds)", AutoItemConfigFlags.PreventNetMismatch)]
        public float additionalDurationOfBuffInSeconds { get; private set; } = 0.5f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("How many buff stacks should each feather give? (Default: 3)", AutoItemConfigFlags.PreventNetMismatch)]
        public int buffStacksPerFeatheredPlume { get; private set; } = 3;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("How much movespeed in percent should each stack of Feathered Plume's buff give? (Default: 0.07f)", AutoItemConfigFlags.PreventNetMismatch)]
        public float moveSpeedPercentageBonusPerBuffStack { get; private set; } = 0.07f;

        public override string displayName => "Feathered Plume";

        public override ItemTier itemTier => RoR2.ItemTier.Tier1;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "After taking damage, gain a boost in speed.";

        protected override string NewLangDesc(string langid = null) => $"Gain a temporary <style=cIsUtility>{Pct(moveSpeedPercentageBonusPerBuffStack)} speed boost</style> upon taking damage that stacks {buffStacksPerFeatheredPlume} times for {baseDurationOfBuffInSeconds} seconds. <style=cStack>(+{buffStacksPerFeatheredPlume} stacks and +{additionalDurationOfBuffInSeconds} second duration per additional Feathered Plume.)</style>";

        protected override string NewLangLore(string langID = null) => "A feather plucked from a legendary alloy vulture. Field testers have noted it to allow them to 'Haul Ass' away from conflict when they get injured.";

        private static List<RoR2.CharacterBody> Playername = new List<RoR2.CharacterBody>();

        public static GameObject ItemBodyModelPrefab;

        public BuffIndex SpeedBuff { get; private set; }


        public FeatheredPlume()
        {
            postConfig += (configFile) =>
            {
                modelPathName = "@Aetherium:Assets/Models/Prefabs/FeatheredPlume.prefab";
                iconPathName = useNewIcons ? "@Aetherium:Assets/Textures/Icons/FeatheredPlumeIconAlt.png" : "@Aetherium:Assets/Textures/Icons/FeatheredPlumeIcon.png";
            };

            onAttrib += (tokenIdent, namePrefix) =>
            {
                var speedBuff = new R2API.CustomBuff(
                    new BuffDef
                    {
                        buffColor = Color.green,
                        canStack = true,
                        isDebuff = false,
                        name = namePrefix + "FeatheredPlumeSpeed",
                        iconPath = "@Aetherium:Assets/Textures/Icons/FeatheredPlumeBuffIcon.png"
                    });
                SpeedBuff = R2API.BuffAPI.Add(speedBuff);
            };

        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = AetheriumPlugin.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.4f, 0.4f, 0.4f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.35f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                    
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.30f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 2.3f, 2f),
                    localAngles = new Vector3(90f, 0f, 0f),
                    localScale = generalScale * 6
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0f, 0.15f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.15f, -0.05f),
                    localAngles = new Vector3(-22.5f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.25f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0f, 1.4f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale * 5
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.2f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0f, 0.75f),
                    localAngles = new Vector3(115f, 0f, 0f),
                    localScale = generalScale * 8
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.20f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
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
            On.RoR2.HealthComponent.TakeDamage += CalculateSpeedReward;
            GetStatCoefficients += AddSpeedReward;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.HealthComponent.TakeDamage -= CalculateSpeedReward;
            GetStatCoefficients -= AddSpeedReward;
        }

        private void CalculateSpeedReward(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            var InventoryCount = GetCount(self.body);
            if (InventoryCount > 0 && self.body.GetBuffCount(SpeedBuff) < buffStacksPerFeatheredPlume * InventoryCount)
            {
                self.body.AddTimedBuffAuthority(SpeedBuff, (baseDurationOfBuffInSeconds + (additionalDurationOfBuffInSeconds * InventoryCount-1)));
            }
            orig(self, damageInfo);
        }

        private void AddSpeedReward(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(SpeedBuff)) { args.moveSpeedMultAdd += moveSpeedPercentageBonusPerBuffStack * sender.GetBuffCount(SpeedBuff); }
        }
    }
}
