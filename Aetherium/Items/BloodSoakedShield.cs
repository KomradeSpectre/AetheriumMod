using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using ItemStats;
using ItemStats.Stat;
using ItemStats.ValueFormatters;

using static Aetherium.AetheriumPlugin;
using static Aetherium.CoreModules.StatHooks;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;
using System.Collections.Generic;

namespace Aetherium.Items
{
    public class BloodSoakedShield : ItemBase<BloodSoakedShield>
    {
        public static ConfigOption<bool> UseNewIcons;
        public static ConfigOption<float> ShieldPercentageRestoredPerKill;
        public static ConfigOption<float> AdditionalShieldPercentageRestoredPerKillDiminishing;
        public static ConfigOption<float> MaximumPercentageShieldRestoredPerKill;
        public static ConfigOption<float> BaseGrantShieldMultiplier;

        public override string ItemName => "Blood Soaked Shield";
        public override string ItemLangTokenName => "BLOOD_SOAKED_SHIELD";
        public override string ItemPickupDesc => "Killing an enemy <style=cIsHealing>restores</style> a small portion of <style=cIsHealing>shield</style>.";

        public override string ItemFullDescription => $"Killing an enemy restores <style=cIsUtility>{FloatToPercentageString(ShieldPercentageRestoredPerKill)} max shield</style> " +
            $"<style=cStack>(+{FloatToPercentageString(AdditionalShieldPercentageRestoredPerKillDiminishing)} per stack hyperbolically.)</style> " +
            $"This item will grant <style=cIsUtility>{FloatToPercentageString(BaseGrantShieldMultiplier)}</style> of your max health as shield on pickup once.";

        public override string ItemLore => "An old gladiatorial round shield. The bloody spikes and Greek lettering give you an accurate picture of what it was used to do. Somehow, holding it makes you feel empowered.";

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Healing };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("BloodSoakedShield.prefab");
        public override Sprite ItemIcon => UseNewIcons ? MainAssets.LoadAsset<Sprite>("BloodSoakedShieldIconAlt.png") : MainAssets.LoadAsset<Sprite>("BloodSoakedShieldIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public BloodSoakedShield()
        {
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            UseNewIcons = config.ActiveBind<bool>("Item: " + ItemName, "Use Alternative Icon Art?", true, "If set to true, will use the new icon art drawn by WaltzingPhantom, else it will use the old icon art.");
            ShieldPercentageRestoredPerKill = config.ActiveBind<float>("Item: " + ItemName, "Percentage of Shield Restored per Kill", 0.1f, "How much shield in percentage should be restored per kill? 0.1 = 10%");
            AdditionalShieldPercentageRestoredPerKillDiminishing = config.ActiveBind<float>("Item: " + ItemName, "Additional Shield Restoration Percentage per Additional BSS Stack (Diminishing)", 0.1f, "How much additional shield per kill should be granted with diminishing returns (hyperbolic scaling) on additional stacks? 0.1 = 10%");
            MaximumPercentageShieldRestoredPerKill = config.ActiveBind<float>("Item: " + ItemName, "Absolute Maximum Shield Restored per Kill", 0.5f, "What should our maximum percentage shield restored per kill be? 0.5 = 50%");
            BaseGrantShieldMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Shield Granted on First BSS Stack", 0.08f, "How much should the starting shield be upon receiving the item? 0.08 = 8%");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.3f, 0.3f, 0.3f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0, 0.23f, -0.05f),
                    localAngles = new Vector3(0, -180, -90),
                    localScale = generalScale
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0, 0.2f, -0.05f),
                    localAngles = new Vector3(0, 180, -90),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0, 0, 0.65f),
                    localAngles = new Vector3(0, 0, 270),
                    localScale = new Vector3(2, 2, 2)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(-0.014f, 0.127f, -0.08f),
                    localAngles = new Vector3(0, 160, 180),
                    localScale = new Vector3(0.3f, 0.3f, 0.3f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0f, 0.15f, 0.07f),
                    localAngles = new Vector3(0, 0, 180),
                    localScale = new Vector3(0.32f, 0.32f, 0.32f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.036f, 0.21f, -0.041f),
                    localAngles = new Vector3(350, 180, 90),
                    localScale = generalScale
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "WeaponPlatform",
                    localPos = new Vector3(-0.16f, -0.1f, 0.1f),
                    localAngles = new Vector3(0, -90, -90),
                    localScale = new Vector3(0.5f, 0.5f, 0.5f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MechLowerArmL",
                    localPos = new Vector3(0, 0.2f, -0.09f),
                    localAngles = new Vector3(0, 180, 90),
                    localScale = new Vector3(0.32f, 0.32f, 0.32f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.7f, 3, 0.7f),
                    localAngles = new Vector3(0, 45, 270),
                    localScale = new Vector3(2, 2, 2)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.058f, 0.23f, 0),
                    localAngles = new Vector3(10, -90, 90),
                    localScale = new Vector3(0.3f, 0.3f, 0.3f)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.0718F, 0.0361F, -0.0153F),
                    localAngles = new Vector3(28.3891F, 105.4747F, 280.3181F),
                    localScale = new Vector3(0.1533F, 0.1533F, 0.1533F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            if (IsItemStatsModInstalled)
            {
                RoR2Application.onLoad += ItemStatsModCompat;
            }

            GetStatCoefficients += GrantBaseShield;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GrantShieldReward;
        }

        private void ItemStatsModCompat()
        {
            ItemStatDef BloodSoakedShieldStatDef = new ItemStatDef
            {
                Stats = new List<ItemStat>()
                {
                    new ItemStat
                    (
                        (itemCount, ctx) => InverseHyperbolicScaling(ShieldPercentageRestoredPerKill, AdditionalShieldPercentageRestoredPerKillDiminishing, MaximumPercentageShieldRestoredPerKill, (int)itemCount),
                        (value, ctx) => $"Shield Restored Per Kill: {value.FormatPercentage()}"
                    ),
                    new ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? ctx.Master.GetBody().healthComponent.fullHealth * BaseGrantShieldMultiplier: 0,
                        (value, ctx) => $"Base Shield Granted By Item: {value.FormatInt(" Shield")}"
                    )
                }
            };
            ItemStatsMod.AddCustomItemStatDef(ItemDef.itemIndex, BloodSoakedShieldStatDef);
        }

        private void GrantBaseShield(CharacterBody sender, StatHookEventArgs args)
        {
            if (GetCount(sender) > 0)
            {
                HealthComponent healthC = sender.GetComponent<HealthComponent>();
                args.baseShieldAdd += healthC.fullHealth * BaseGrantShieldMultiplier;
            }
        }

        private void GrantShieldReward(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, RoR2.GlobalEventManager self, RoR2.DamageReport damageReport)
        {
            if (damageReport?.attackerBody)
            {
                int inventoryCount = GetCount(damageReport.attackerBody);
                if (inventoryCount > 0)
                {
                    var percentage = InverseHyperbolicScaling(ShieldPercentageRestoredPerKill, AdditionalShieldPercentageRestoredPerKillDiminishing, MaximumPercentageShieldRestoredPerKill, inventoryCount);
                    damageReport.attackerBody.healthComponent.RechargeShield(damageReport.attackerBody.healthComponent.fullShield * percentage);
                }
            }
            orig(self, damageReport);
        }
    }
}