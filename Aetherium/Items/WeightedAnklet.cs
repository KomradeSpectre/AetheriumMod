using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ItemStats;
using ItemStats.Stat;
using ItemStats.ValueFormatters;

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;
using static RoR2.Navigation.MapNodeGroup;
using static Aetherium.Compatability.ModCompatability.BetterUICompat;
using static Aetherium.Compatability.ModCompatability.ItemStatsModCompat;
using static Aetherium.Compatability.ModCompatability.TILER2Compat;

using System.Runtime.CompilerServices;
using static R2API.RecalculateStatsAPI;

namespace Aetherium.Items
{
    public class WeightedAnklet : ItemBase<WeightedAnklet>
    {
        public static ConfigOption<float> BaseKnockbackReductionPercentage;
        public static ConfigOption<float> BaseAttackSpeedReductionPercentage;
        public static ConfigOption<float> AttackSpeedReductionPercentageCap;
        public static ConfigOption<float> BaseMovementSpeedReductionPercentage;
        public static ConfigOption<float> MovementSpeedReductionPercentageCap;
        public static ConfigOption<float> AttackSpeedGainedPerLimiterRelease;
        public static ConfigOption<float> MovementSpeedGainedPerLimiterRelease;
        public static ConfigOption<float> DamagePercentageGainedPerLimiterRelease;
        public static ConfigOption<float> BaseCooldownOfLimiterReleaseDodge;
        public static ConfigOption<float> AdditionalCooldownOfLimiterReleaseDodge;


        public override string ItemName => "Weighted Anklet";

        public override string ItemLangTokenName => "WEIGHTED_ANKLET";

        public override string ItemPickupDesc => "A collection of weights slow you down, but finding a way to remove them could greatly benefit you.";

        public override string ItemFullDescription => $"A collection of weights will slow your <style=cIsUtility>attack speed</style> by {FloatToPercentageString(BaseAttackSpeedReductionPercentage)} <style=cStack>(to a minimum of {FloatToPercentageString(AttackSpeedReductionPercentageCap)})</style>, \n" +
            $"and your <style=cIsUtility>movement speed</style> by {FloatToPercentageString(BaseMovementSpeedReductionPercentage)} <style=cStack>(to a minimum of {FloatToPercentageString(MovementSpeedReductionPercentageCap)})</style>. \n" +
            $"If you find a way to remove them, you are granted {AttackSpeedGainedPerLimiterRelease} <style=cIsUtility>attack speed</style>, {MovementSpeedGainedPerLimiterRelease} <style=cIsUtility>movement speed</style>, and {FloatToPercentageString(DamagePercentageGainedPerLimiterRelease)} <style=cIsDamage>damage</style> per removal. \n" +
            $"Additionally, removing an anklet grants you a stack of <style=cIsUtility>Limiter Release Dodge</style>. <style=cIsUtility>Dodge</style> will allow you to dodge one overlap, or blast attack before depleting. \n" +
            $"Once all stacks of dodge are depleted, they will need to recharge <style=cStack>({BaseCooldownOfLimiterReleaseDodge} seconds for the first stack, {AdditionalCooldownOfLimiterReleaseDodge} seconds per each additional stack)</style> before fully replenishing.";

        public override string ItemLore => OrderManifestLoreFormatter(
            ItemName, 

            "7/17/2056",

            "Neptune's Gym and Grill\nEurytrades\nNeptune", 

            "405********", 

            ItemPickupDesc, 

            "Heavy  / Support Equipment Needed / Superdense [Do Not Drop]", 

            "A strange anklet lined with superdense crystals. It's hard to move around in these, but scanners show that the muscle mass of people wearing them increases exponentially.");

        public override ItemTier Tier => ItemTier.Lunar;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Cleansable | ItemTag.AIBlacklist};

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("PickupWeightedAnklet.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("WeightedAnkletIcon.png");

        public static GameObject ItemBodyModelPrefab;
        public static GameObject LimiterReleaseEyePrefab;

        public static ItemDef LimiterReleaseItemDef;

        public static BuffDef LimiterReleaseBuffDef;
        public static BuffDef LimiterReleaseDodgeBuffDef;
        public static BuffDef LimiterReleaseDodgeCooldownDebuffDef;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateNetworkMessages();
            CreateBuff();
            CreatePowerupItem();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            BaseKnockbackReductionPercentage = config.ActiveBind<float>("Item: " + ItemName, "Base Knockback Reduction Percentage", 0.25f, "How much knockback reduction in percentage should be given for each Weighted Anklet?");
            BaseAttackSpeedReductionPercentage = config.ActiveBind<float>("Item: " + ItemName, "Base Attack Speed Reduction Percentage", 0.1f, "How much attack speed in percentage should be reduced per Weighted Anklet?");
            AttackSpeedReductionPercentageCap = config.ActiveBind<float>("Item: " + ItemName, "Absolute Lowest Attack Speed Reduction Percentage", 0.1f, "What should the lowest percentage that we should be able to reduce attack speed to be?");
            BaseMovementSpeedReductionPercentage = config.ActiveBind<float>("Item: " + ItemName, "Base Movement Speed Reduction Percentage", 0.1f, "How much movement speed in percentage should be reduced per Weighted Anklet?");
            MovementSpeedReductionPercentageCap = config.ActiveBind<float>("Item: " + ItemName, "Absolute Lowest Movement Speed Reduction Percentage", 0.1f, "What should the lowest percentage we should be able to reduce movement speed to be?");
            AttackSpeedGainedPerLimiterRelease = config.ActiveBind<float>("Item: " + ItemName, "Attack Speed Gained per Limiter Release (Flat)", 0.25f, "How much attack speed should we gain per Limiter Release?");
            MovementSpeedGainedPerLimiterRelease = config.ActiveBind<float>("Item: " + ItemName, "Movement Speed Gained per Limiter Release (Flat)", 1, "How much movement speed should we gain per Limiter Release?");
            DamagePercentageGainedPerLimiterRelease = config.ActiveBind<float>("Item: " + ItemName, "Damage Percentage Gained per Limiter Release (Percentile)", 0.05f, "How much damage in percent should we gain per Limiter Release?");
            BaseCooldownOfLimiterReleaseDodge = config.ActiveBind<float>("Item: " + ItemName, "Base Dodge Depletion Cooldown Duration", 10, "How long (in seconds) should we have to wait for the first stack to replenish?");
            AdditionalCooldownOfLimiterReleaseDodge = config.ActiveBind<float>("Item: " + ItemName, "Additional Dodge Depletion Cooldown Duration for Additional Dodge Stacks", 5, "How long (in seconds) should we have to wait per each additional dodge stack to replenish?");
        }

        private void CreateNetworkMessages()
        {
            NetworkingAPI.RegisterMessageType<SyncTeleportDodge>();
        }

        private void CreateBuff()
        {
            LimiterReleaseBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            LimiterReleaseBuffDef.name = "Aetherium: Limiter Release";
            LimiterReleaseBuffDef.buffColor = new Color(255, 255, 255);
            LimiterReleaseBuffDef.canStack = true;
            LimiterReleaseBuffDef.isDebuff = false;
            LimiterReleaseBuffDef.iconSprite = MainAssets.LoadAsset<Sprite>("WeightedAnkletLimiterReleaseBuffIcon.png");

            BuffAPI.Add(new CustomBuff(LimiterReleaseBuffDef));

            LimiterReleaseDodgeBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            LimiterReleaseDodgeBuffDef.name = "Aetherium: Limiter Release Dodge";
            LimiterReleaseDodgeBuffDef.buffColor = new Color(48, 255, 48);
            LimiterReleaseDodgeBuffDef.canStack = true;
            LimiterReleaseDodgeBuffDef.isDebuff = false;
            LimiterReleaseDodgeBuffDef.iconSprite = MainAssets.LoadAsset<Sprite>("WeightedAnkletLimiterReleaseDodgeBuffIcon.png");

            BuffAPI.Add(new CustomBuff(LimiterReleaseDodgeBuffDef));

            LimiterReleaseDodgeCooldownDebuffDef = ScriptableObject.CreateInstance<BuffDef>();
            LimiterReleaseDodgeCooldownDebuffDef.name = "Aetherium: Limiter Release Dodge Cooldown";
            LimiterReleaseDodgeCooldownDebuffDef.buffColor = new Color(48, 255, 48);
            LimiterReleaseDodgeCooldownDebuffDef.canStack = false;
            LimiterReleaseDodgeCooldownDebuffDef.isDebuff = false;
            LimiterReleaseDodgeCooldownDebuffDef.iconSprite = MainAssets.LoadAsset<Sprite>("WeightedAnkletLimiterReleaseDodgeCooldownDebuffIcon.png");

            BuffAPI.Add(new CustomBuff(LimiterReleaseDodgeCooldownDebuffDef));

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("DisplayWeightedAnklet.prefab");
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.32f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.00707F, 0.40019F, -0.00026F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.00002F, 1.70849F, -0.16102F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(3F, 3F, 3F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.00537F, 0.17835F, 0.02795F),
                    localAngles = new Vector3(349.1283F, 359.4232F, 356.149F),
                    localScale = new Vector3(0.35F, 0.35F, 0.35F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0F, 0.35617F, 0.0156F),
                    localAngles = new Vector3(355F, 0F, 0F),
                    localScale = new Vector3(0.14F, 0.14F, 0.14F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0.0035F, 0.31555F, -0.00034F),
                    localAngles = new Vector3(350F, 0F, 0F),
                    localScale = new Vector3(0.12F, 0.12F, 0.12F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FootFrontL",
                    localPos = new Vector3(-0.00003F, 1.00002F, -0.01375F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.45876F, 0.45876F, 0.45876F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.00509F, 0.35102F, 0.00169F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.3F, 0.3F, 0.3F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.07303F, 3.00001F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(1.3F, 1.3F, 1.3F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0F, 0.39F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.0007F, 0.35803F, -0.0036F),
                    localAngles = new Vector3(355.047F, 0F, 357.3693F),
                    localScale = new Vector3(0.2168F, 0.2168F, 0.2168F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LeftLeg",
                    localPos = new Vector3(0.00052F, 0.01958F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.00533F, 0.00533F, 0.00533F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0F, 0.40653F, -0.00001F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.48563F, 0.48563F, 0.48563F)
                }
            });

            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00075F, 0.09531F, 0.11188F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0.00327F, 0.28571F, -0.00983F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.17F, 0.17F, 0.17F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0.01626F, 0.30262F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.39776F, 0.39776F, 0.39776F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Model",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            return rules;
        }

        private ItemDisplayRuleDict CreateLimiterItemDisplayRules()
        {
            LimiterReleaseEyePrefab = MainAssets.LoadAsset<GameObject>("LimiterReleaseEyeTrail.prefab");
            var itemDisplay = LimiterReleaseEyePrefab.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(LimiterReleaseEyePrefab);
            itemDisplay.gameObject.AddComponent<LimiterTrailSizeHandler>();

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(0.1f, 0.25f, 0.15f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.1f, 0.25f, 0.15f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.16f, 0.14f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.26f, 0.1f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(0.425f, 2.9f, -1f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0.05f, 0.03f, 0.15f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(-0.05f, 0.03f, 0.15f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(0.05f, 0.06f, 0.11f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.05f, 0.06f, 0.11f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(0.06f, 0.15f, 0.15f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.06f, 0.15f, 0.15f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Eye",
                    localPos = new Vector3(0f, 0.86f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(0.055f, 0.13f, 0.12f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.055f, 0.13f, 0.12f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(-1.6f, 1.9f, 0.3f),
                    localAngles = new Vector3(0f, 10f, -10f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(1.6f, 1.9f, 0.3f),
                    localAngles = new Vector3(0f, -10f, 10f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(-1.3f, 2.65f, 0.46f),
                    localAngles = new Vector3(0f, 10f, -10f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(1.3f, 2.65f, 0.46f),
                    localAngles = new Vector3(0f, -10f, 10f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.06f, 0.16f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.0105F, 0.0505F, 0.1133F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.0609F, 0.0609F, 0.0609F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00394F, 0.00396F, 0.00369F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.003F, 0.003F, 0.003F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00394F, 0.00396F, 0.00369F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.003F, 0.003F, 0.003F)
                },
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00059F, 0.19447F, 0.22901F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                },
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(0.04522F, 0.16908F, -0.02963F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.01F, 0.01F, 0.01F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.04522F, 0.16908F, -0.02963F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.01F, 0.01F, 0.01F)
                },
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Model",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Model",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Model",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Model",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });

            return rules;
        }

        private void CreatePowerupItem()
        {
            LanguageAPI.Add("HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_NAME", "Weighted Anklet Limiter Release");
            LanguageAPI.Add("HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_PICKUP", "You feel much lighter, and your senses keener.");
            LanguageAPI.Add("HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_DESCRIPTION", $"You gain <style=cIsUtility>{MovementSpeedGainedPerLimiterRelease}</style> movement speed <style=cStack>(+{MovementSpeedGainedPerLimiterRelease} per stack)</style>, <style=cIsUtility>{AttackSpeedGainedPerLimiterRelease}</style> attack speed <style=cStack>(+{AttackSpeedGainedPerLimiterRelease} per stack)</style>, and <style=cIsDamage>{FloatToPercentageString(DamagePercentageGainedPerLimiterRelease)}</style> damage bonus <style=cStack>(+{FloatToPercentageString(DamagePercentageGainedPerLimiterRelease)} per stack)</style>. Gain the ability to dodge one time per stack out of the way of close ranged attacks and behind the attacker before entering a cooldown period of <style=cIsUtility>{BaseCooldownOfLimiterReleaseDodge}</style> <style=cStack>(+{AdditionalCooldownOfLimiterReleaseDodge} per stack)</style>.");

            LimiterReleaseItemDef = ScriptableObject.CreateInstance<ItemDef>();
            LimiterReleaseItemDef.name = "HIDDEN_ITEM_WEIGHTED_ANKLET_LIMITER_RELEASE";
            LimiterReleaseItemDef.nameToken = "HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_NAME";
            LimiterReleaseItemDef.pickupToken = "HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_PICKUP";
            LimiterReleaseItemDef.descriptionToken = "HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_DESCRIPTION";
            LimiterReleaseItemDef.loreToken = "";
            LimiterReleaseItemDef.canRemove = false;
            LimiterReleaseItemDef.hidden = true;
            LimiterReleaseItemDef.tier = ItemTier.NoTier;

            ItemAPI.Add(new CustomItem(LimiterReleaseItemDef, CreateLimiterItemDisplayRules()));
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += RemoveWeightedAnkletAndLimitersFromDeployables;
            R2API.RecalculateStatsAPI.GetStatCoefficients += ManageBonusesAndPenalties;
            On.RoR2.CharacterMaster.OnInventoryChanged += ManageLimiter;
            On.RoR2.CharacterBody.FixedUpdate += ManageLimiterBuff;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += ManageLimiterBuffCooldown;
            On.RoR2.HealthComponent.TakeDamage += ReduceKnockback;
            RoR2Application.onLoad += OnLoadModCompat;

            var methodBlast = typeof(RoR2.BlastAttack).GetMethod("HandleHits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            new MonoMod.RuntimeDetour.Hook(methodBlast, new Action<Action<RoR2.BlastAttack, RoR2.BlastAttack.HitPoint[]>, RoR2.BlastAttack, RoR2.BlastAttack.HitPoint[]>((orig, self, hitPoints) =>
            {
                List<RoR2.CharacterBody> DodgedBodies = new List<RoR2.CharacterBody>();
                List<RoR2.BlastAttack.HitPoint> HitPointList = new List<RoR2.BlastAttack.HitPoint>();
                foreach(RoR2.BlastAttack.HitPoint hitpoint in hitPoints)
                {
                    var hurtbox = hitpoint.hurtBox;
                    if (hurtbox && hurtbox.healthComponent && hurtbox.healthComponent.body)
                    {
                        var body = hurtbox.healthComponent.body;
                        if (body.HasBuff(LimiterReleaseDodgeBuffDef))
                        {
                            if (!DodgedBodies.Contains(body)) { DodgedBodies.Add(body); }
                            continue;
                        }

                    }
                    HitPointList.Add(hitpoint);
                }
                if(DodgedBodies.Count > 0)
                {
                    foreach(RoR2.CharacterBody dodgeBody in DodgedBodies)
                    {
                        if (self.attacker) 
                        {
                            var attackerBody = self.attacker.GetComponent<RoR2.CharacterBody>();
                            if (attackerBody)
                            {
                                TeleportBody(dodgeBody, attackerBody, self.attacker.transform.position, dodgeBody.isFlying ? GraphType.Air : GraphType.Ground);

                                var teleportCameraComponent = dodgeBody.GetComponent<LimiterDodgeCameraTrackPostTeleport>();
                                if (!teleportCameraComponent) { teleportCameraComponent = dodgeBody.gameObject.AddComponent<LimiterDodgeCameraTrackPostTeleport>(); }

                                teleportCameraComponent.dodgeBody = dodgeBody;
                                teleportCameraComponent.attackerBody = attackerBody;
                                teleportCameraComponent.Timer = 0.1f;
                            }

                        }

                        dodgeBody.RemoveBuff(LimiterReleaseDodgeBuffDef);
                        if (dodgeBody.GetBuffCount(LimiterReleaseDodgeBuffDef) <= 0)
                        {
                            dodgeBody.AddTimedBuff(LimiterReleaseDodgeCooldownDebuffDef, BaseCooldownOfLimiterReleaseDodge + (AdditionalCooldownOfLimiterReleaseDodge * (GetCountSpecific(dodgeBody, LimiterReleaseItemDef) - 1)));
                        }

                    }
                }
                orig(self, HitPointList.ToArray());
                
            }));

            var methodOverlap = typeof(RoR2.OverlapAttack).GetMethod("ProcessHits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            new MonoMod.RuntimeDetour.Hook(methodOverlap, new Action<Action<RoR2.OverlapAttack, List<RoR2.OverlapAttack.OverlapInfo>>, RoR2.OverlapAttack, List<RoR2.OverlapAttack.OverlapInfo>>((orig, self, hitList) =>
            {
                List<RoR2.CharacterBody> DodgedBodies = new List<RoR2.CharacterBody>();
                List<RoR2.OverlapAttack.OverlapInfo> HitPointList = new List<RoR2.OverlapAttack.OverlapInfo>();
                foreach (RoR2.OverlapAttack.OverlapInfo hitpoint in hitList)
                {
                    var hurtbox = hitpoint.hurtBox;
                    if (hurtbox && hurtbox.healthComponent && hurtbox.healthComponent.body)
                    {
                        var body = hurtbox.healthComponent.body;
                        if (body.HasBuff(LimiterReleaseDodgeBuffDef))
                        {
                            if (!DodgedBodies.Contains(body)) { DodgedBodies.Add(body); }
                            continue;
                        }

                    }
                    HitPointList.Add(hitpoint);
                }
                if (DodgedBodies.Count > 0)
                {
                    foreach (RoR2.CharacterBody dodgeBody in DodgedBodies)
                    {
                        if (self.attacker)
                        {
                            var attackerBody = self.attacker.GetComponent<RoR2.CharacterBody>();
                            if (attackerBody)
                            {
                                var teleportBool = TeleportBody(dodgeBody, attackerBody, self.attacker.transform.position, dodgeBody.isFlying ? GraphType.Air : GraphType.Ground);

                                var teleportCameraComponent = dodgeBody.GetComponent<LimiterDodgeCameraTrackPostTeleport>();
                                if (!teleportCameraComponent) { teleportCameraComponent = dodgeBody.gameObject.AddComponent<LimiterDodgeCameraTrackPostTeleport>(); }

                                teleportCameraComponent.dodgeBody = dodgeBody;
                                teleportCameraComponent.attackerBody = attackerBody;
                                teleportCameraComponent.Timer = 0.1f;
                            }
                        }

                        dodgeBody.RemoveBuff(LimiterReleaseDodgeBuffDef);
                        if (dodgeBody.GetBuffCount(LimiterReleaseDodgeBuffDef) <= 0)
                        {
                            dodgeBody.AddTimedBuff(LimiterReleaseDodgeCooldownDebuffDef, BaseCooldownOfLimiterReleaseDodge + (AdditionalCooldownOfLimiterReleaseDodge * (GetCountSpecific(dodgeBody, LimiterReleaseItemDef) - 1)));
                        }

                    }
                }
                orig(self, HitPointList);

            }));

        }

        private void OnLoadModCompat()
        {
            if (IsItemStatsModInstalled)
            {
                CreateWeightedAnkletStatDef();
            }

            if (IsBetterUIInstalled)
            {
                var limiterReleaseBuffInfo = CreateBetterUIBuffInformation($"LIMITER_RELEASE_BONUS_BUFF", LimiterReleaseBuffDef.name, "A weight has been lifted from you. Your training has made you much stronger than you once were.");
                RegisterBuffInfo(LimiterReleaseBuffDef, limiterReleaseBuffInfo.Item1, limiterReleaseBuffInfo.Item2);

                var dodgeBuffInfo = CreateBetterUIBuffInformation($"LIMITER_RELEASE_DODGE_BUFF", LimiterReleaseDodgeBuffDef.name, "You can sense certain attacks around you, and move behind the attacker.");
                RegisterBuffInfo(LimiterReleaseDodgeBuffDef, dodgeBuffInfo.Item1, dodgeBuffInfo.Item2);

                var dodgeCooldownDebuffInfo = CreateBetterUIBuffInformation($"LIMITER_RELEASE_DODGE_COOLDOWN_DEBUFF", LimiterReleaseDodgeCooldownDebuffDef.name, "For the moment, you can no longer dodge out of the way of certain attacks automatically.", false);
                RegisterBuffInfo(LimiterReleaseDodgeCooldownDebuffDef, dodgeCooldownDebuffInfo.Item1, dodgeCooldownDebuffInfo.Item2);
            }

            if (IsTILER2Installed)
            {
                BlacklistItemFromFakeInventory(ItemDef);
            }
        }

        private void RemoveWeightedAnkletAndLimitersFromDeployables(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, RoR2.CharacterBody self)
        {
            orig(self);
            var InventoryCount = GetCount(self);
            if (InventoryCount > 0 && self.master)
            {
                if (self.master.teamIndex == TeamIndex.Player && !self.isPlayerControlled)
                {
                    //Deployables have no muscle mass, can't get swole!
                    self.inventory.RemoveItem(ItemDef, InventoryCount);
                    var limiterReleaseCount = GetCountSpecific(self, LimiterReleaseItemDef);
                    self.inventory.RemoveItem(LimiterReleaseItemDef, limiterReleaseCount);
                }
            }
        }

        private void ManageBonusesAndPenalties(RoR2.CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            if (InventoryCount > 0)
            {
                args.moveSpeedMultAdd -= Mathf.Min(InventoryCount * BaseMovementSpeedReductionPercentage, MovementSpeedReductionPercentageCap);
                args.attackSpeedMultAdd -= Mathf.Min(InventoryCount * BaseAttackSpeedReductionPercentage, AttackSpeedReductionPercentageCap);
            }

            var LimiterReleaseCount = GetCountSpecific(sender, LimiterReleaseItemDef);
            if (LimiterReleaseCount > 0)
            {
                args.baseAttackSpeedAdd += LimiterReleaseCount * AttackSpeedGainedPerLimiterRelease;
                args.baseMoveSpeedAdd += LimiterReleaseCount * MovementSpeedGainedPerLimiterRelease;
                args.damageMultAdd += LimiterReleaseCount * DamagePercentageGainedPerLimiterRelease;
            }

        }

        private void ManageLimiter(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, RoR2.CharacterMaster self)
        {
            orig(self);
            var ankletTracker = self.GetComponent<AnkletTracker>();
            if (!ankletTracker) { ankletTracker = self.gameObject.AddComponent<AnkletTracker>(); }

            var inventoryCount = GetCount(self);
            if (inventoryCount > ankletTracker.AnkletStacks)
            {
                ankletTracker.AnkletStacks = inventoryCount;
            }
            else if (inventoryCount < ankletTracker.AnkletStacks)
            {
                var calculatedStacks = ankletTracker.AnkletStacks - inventoryCount;
                ankletTracker.AnkletStacks = inventoryCount;
                self.inventory.GiveItem(LimiterReleaseItemDef, calculatedStacks);
            }
        }

        private void ManageLimiterBuff(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {

            orig(self);

            if (self.inventory)
            {
                var inventoryCount = self.inventory.GetItemCount(LimiterReleaseItemDef);
                var buffCount = self.GetBuffCount(LimiterReleaseBuffDef);

                if (buffCount < inventoryCount)
                {
                    var iterations = inventoryCount - buffCount;
                    for (int i = 1; i <= iterations; i++)
                    {
                        self.AddBuff(LimiterReleaseBuffDef);
                        self.AddBuff(LimiterReleaseDodgeBuffDef);
                    }
                }
                else if(buffCount > inventoryCount)
                {
                    var iterations = buffCount - inventoryCount;
                    for(int i = 1; i <= iterations; i++)
                    {
                        self.RemoveBuff(LimiterReleaseBuffDef);
                        self.RemoveBuff(LimiterReleaseDodgeBuffDef);
                    }
                }
            }
        }

        private void ManageLimiterBuffCooldown(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, RoR2.CharacterBody self, RoR2.BuffDef buffDef)
        {
            if(buffDef == LimiterReleaseDodgeCooldownDebuffDef)
            {
                var ankletTracker = self.master.GetComponent<AnkletTracker>();
                if (ankletTracker)
                {
                    for(int i = 1; i <= self.GetBuffCount(LimiterReleaseBuffDef); i++)
                    {
                        self.AddBuff(LimiterReleaseDodgeBuffDef);
                    }
                }
            }

            orig(self, buffDef);
        }

        private bool TeleportBody(RoR2.CharacterBody body, CharacterBody attackerbody, Vector3 desiredPosition, GraphType nodeGraphType)
        {
            RoR2.SpawnCard spawnCard = ScriptableObject.CreateInstance<RoR2.SpawnCard>();
            spawnCard.hullSize = body.hullClassification;
            spawnCard.nodeGraphType = nodeGraphType;
            spawnCard.prefab = Resources.Load<GameObject>("SpawnCards/HelperPrefab");
            GameObject gameObject = RoR2.DirectorCore.instance.TrySpawnObject(new RoR2.DirectorSpawnRequest(spawnCard, new RoR2.DirectorPlacementRule
            {
                placementMode = RoR2.DirectorPlacementRule.PlacementMode.Approximate,
                position = desiredPosition,
                minDistance = 10,
                maxDistance = 20
            }, RoR2.RoR2Application.rng));
            if (gameObject)
            {
                if (NetworkServer.active)
                {
                    var bodyIdentity = body.gameObject.GetComponent<NetworkIdentity>();
                    var attackerBodyIdentity = attackerbody.gameObject.GetComponent<NetworkIdentity>();
                    if (bodyIdentity && attackerBodyIdentity)
                    {
                        new SyncTeleportDodge(gameObject.transform.position, bodyIdentity.netId, attackerBodyIdentity.netId).Send(R2API.Networking.NetworkDestination.Clients);
                    }
                }
                RoR2.TeleportHelper.TeleportBody(body, gameObject.transform.position);
                GameObject teleportEffectPrefab = RoR2.Run.instance.GetTeleportEffectPrefab(body.gameObject);
                if (teleportEffectPrefab)
                {
                    RoR2.EffectManager.SimpleEffect(teleportEffectPrefab, gameObject.transform.position, Quaternion.identity, true);
                }
                UnityEngine.Object.Destroy(gameObject);
                UnityEngine.Object.Destroy(spawnCard);
                return true;
            }
            else
            {
                UnityEngine.Object.Destroy(spawnCard);
                return false;
            }
        }

        private void ReduceKnockback(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            var InventoryCount = GetCount(self.body);
            if (InventoryCount > 0)
            {
                var percentReduction = Mathf.Clamp(1 - (InventoryCount * BaseKnockbackReductionPercentage), 0, 1);
                damageInfo.force *= percentReduction;
            }
            orig(self, damageInfo);
        }

        public class AnkletTracker : MonoBehaviour
        {
            public int AnkletStacks;
        }

        public class LimiterDodgeCameraTrackPostTeleport : MonoBehaviour
        {
            public CharacterBody dodgeBody;
            public CharacterBody attackerBody;
            public float Timer = 1;

            public void FixedUpdate()
            {
                if(!dodgeBody || !attackerBody)
                {
                    UnityEngine.Object.Destroy(this);
                }

                Timer -= Time.fixedDeltaTime;
                if(Timer <= 0)
                {
                    if (dodgeBody.master.playerCharacterMasterController && dodgeBody.master.playerCharacterMasterController.networkUser && dodgeBody.master.playerCharacterMasterController.networkUser.cameraRigController)
                    {
                        var Camera = dodgeBody.master.playerCharacterMasterController.networkUser.cameraRigController;
                        Camera.SetPitchYawFromLookVector(attackerBody.corePosition - dodgeBody.corePosition);
                    }
                    UnityEngine.Object.Destroy(this);
                }
            }
        }

        public class LimiterTrailSizeHandler : MonoBehaviour
        {
            public ItemDisplay ItemDisplay;
            public TrailRenderer TrailRenderer;
            public CharacterMaster OwnerMaster;
            public void FixedUpdate()
            {

                if (!OwnerMaster || !ItemDisplay || !TrailRenderer)
                {
                    ItemDisplay = this.GetComponentInParent<ItemDisplay>();
                    if (ItemDisplay)
                    {

                        TrailRenderer = ItemDisplay.GetComponent<TrailRenderer>();

                        if (TrailRenderer)
                        {
                            TrailRenderer.transform.localScale = ItemDisplay.transform.localScale;
                        }
                        //Debug.Log("Found ItemDisplay: " + itemDisplay);
                        var characterModel = ItemDisplay.GetComponentInParent<CharacterModel>();

                        if (characterModel)
                        {
                            var body = characterModel.body;
                            if (body)
                            {
                                OwnerMaster = body.master;
                            }
                        }
                    }
                }

                if(ItemDisplay && TrailRenderer)
                {
                    if(TrailRenderer.widthMultiplier != ItemDisplay.transform.localScale.x)
                    {
                        TrailRenderer.widthMultiplier = ItemDisplay.transform.localScale.x;
                    }

                    if(ItemDisplay.GetVisibilityLevel() != VisibilityLevel.Invisible && !TrailRenderer.enabled)
                    {
                        TrailRenderer.enabled = true;
                    }
                    else if (ItemDisplay.GetVisibilityLevel() == VisibilityLevel.Invisible && TrailRenderer.enabled)
                    {
                        TrailRenderer.enabled = false;
                    }
                }
            }
        }

        public class SyncTeleportDodge : INetMessage
        {
            private Vector3 Position;
            private NetworkInstanceId BodyID;
            private NetworkInstanceId AttackerBodyID;

            public SyncTeleportDodge()
            {
            }

            public SyncTeleportDodge(Vector3 position, NetworkInstanceId bodyID, NetworkInstanceId attackerBodyID)
            {
                Position = position;
                BodyID = bodyID;
                AttackerBodyID = attackerBodyID;
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(Position);
                writer.Write(BodyID);
                writer.Write(AttackerBodyID);
            }

            public void Deserialize(NetworkReader reader)
            {
                Position = reader.ReadVector3();
                BodyID = reader.ReadNetworkId();
                AttackerBodyID = reader.ReadNetworkId();
            }

            public void OnReceived()
            {
                if (NetworkServer.active) return;

                var playerGameObject = RoR2.Util.FindNetworkObject(BodyID);
                var attackerGameObject = RoR2.Util.FindNetworkObject(AttackerBodyID);

                if (playerGameObject && attackerGameObject)
                {
                    var body = playerGameObject.GetComponent<RoR2.CharacterBody>();
                    var attackerBody = attackerGameObject.GetComponent<CharacterBody>();

                    if (body && attackerBody)
                    {
                        RoR2.TeleportHelper.TeleportBody(body, Position);

                        var teleportCameraComponent = body.GetComponent<LimiterDodgeCameraTrackPostTeleport>();
                        if (!teleportCameraComponent) { teleportCameraComponent = body.gameObject.AddComponent<LimiterDodgeCameraTrackPostTeleport>(); }

                        teleportCameraComponent.dodgeBody = body;
                        teleportCameraComponent.attackerBody = attackerBody;
                        teleportCameraComponent.Timer = 0.1f;

                    }
                }
            }
        }
    }
}