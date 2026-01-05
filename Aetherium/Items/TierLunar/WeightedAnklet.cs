using Aetherium.Achievements;
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

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;
using static RoR2.Navigation.MapNodeGroup;

using System.Runtime.CompilerServices;
using static R2API.RecalculateStatsAPI;

namespace Aetherium.Items.TierLunar
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


        public override string ItemName => "Weighted Anklet";

        public override string ItemLangTokenName => "WEIGHTED_ANKLET";

        public override string ItemPickupDesc => "A collection of weights slow you down, but finding a way to remove them could greatly benefit you.";

        public override string ItemFullDescription => $"A collection of weights will slow your <style=cIsUtility>attack speed</style> by {FloatToPercentageString(BaseAttackSpeedReductionPercentage)} <style=cStack>(to a minimum of {FloatToPercentageString(AttackSpeedReductionPercentageCap)})</style>, \n" +
            $"and your <style=cIsUtility>movement speed</style> by {FloatToPercentageString(BaseMovementSpeedReductionPercentage)} <style=cStack>(to a minimum of {FloatToPercentageString(MovementSpeedReductionPercentageCap)})</style>. \n" +
            $"If you find a way to remove them, you are granted {AttackSpeedGainedPerLimiterRelease} <style=cIsUtility>attack speed</style>, {MovementSpeedGainedPerLimiterRelease} <style=cIsUtility>movement speed</style>, and {FloatToPercentageString(DamagePercentageGainedPerLimiterRelease)} <style=cIsDamage>damage</style> per removal.";

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
            CreateBuff();
            CreateAchievement();
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
        }

        private void CreateBuff()
        {
            LimiterReleaseBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            LimiterReleaseBuffDef.name = "Aetherium: Limiter Release";
            LimiterReleaseBuffDef.buffColor = new Color(255, 255, 255);
            LimiterReleaseBuffDef.canStack = true;
            LimiterReleaseBuffDef.isDebuff = false;
            LimiterReleaseBuffDef.iconSprite = MainAssets.LoadAsset<Sprite>("WeightedAnkletLimiterReleaseBuffIcon.png");

            ContentAddition.AddBuffDef(LimiterReleaseBuffDef);

        }

        private void CreateAchievement()
        {
            if(RequireUnlock)
            {
            }
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
                    childName = "KneeL",
                    localPos = new Vector3(0F, 0.00906F, -0.00053F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.00868F, 0.00868F, 0.00868F)
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
                    childName = "Head",
                    localPos = new Vector3(-0.16338F, 0.07396F, 0.06667F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.16338F, 0.07396F, -0.06667F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00303F, 0.00374F, 0.0016F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = LimiterReleaseEyePrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00303F, 0.00374F, -0.0016F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });

            return rules;
        }

        private void CreatePowerupItem()
        {
            Language.Language.Add("HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_NAME", "Weighted Anklet Limiter Release");
            Language.Language.Add("HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_PICKUP", "You feel much lighter, and your senses keener.");
            Language.Language.Add("HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_DESCRIPTION", $"You gain <style=cIsUtility>{MovementSpeedGainedPerLimiterRelease}</style> movement speed <style=cStack>(+{MovementSpeedGainedPerLimiterRelease} per stack)</style>, <style=cIsUtility>{AttackSpeedGainedPerLimiterRelease}</style> attack speed <style=cStack>(+{AttackSpeedGainedPerLimiterRelease} per stack)</style>, and <style=cIsDamage>{FloatToPercentageString(DamagePercentageGainedPerLimiterRelease)}</style> damage bonus <style=cStack>(+{FloatToPercentageString(DamagePercentageGainedPerLimiterRelease)} per stack)</style>.");

            LimiterReleaseItemDef = ScriptableObject.CreateInstance<ItemDef>();
            LimiterReleaseItemDef.name = "HIDDEN_ITEM_WEIGHTED_ANKLET_LIMITER_RELEASE";
            LimiterReleaseItemDef.nameToken = "HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_NAME";
            LimiterReleaseItemDef.pickupToken = "HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_PICKUP";
            LimiterReleaseItemDef.descriptionToken = "HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_DESCRIPTION";
            LimiterReleaseItemDef.loreToken = "";
            LimiterReleaseItemDef.canRemove = false;
            LimiterReleaseItemDef.hidden = true;
            LimiterReleaseItemDef.deprecatedTier = ItemTier.NoTier;
            LimiterReleaseItemDef.requiredExpansion = AetheriumExpansionDef;

            ItemAPI.Add(new CustomItem(LimiterReleaseItemDef, CreateLimiterItemDisplayRules()));
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += RemoveWeightedAnkletAndLimitersFromDeployables;
            R2API.RecalculateStatsAPI.GetStatCoefficients += ManageBonusesAndPenalties;
            On.RoR2.CharacterMaster.OnInventoryChanged += ManageLimiter;
            On.RoR2.CharacterBody.FixedUpdate += ManageLimiterBuff;
            On.RoR2.HealthComponent.TakeDamage += ReduceKnockback;

        }

        private void RemoveWeightedAnkletAndLimitersFromDeployables(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, RoR2.CharacterBody self)
        {
            orig(self);
            var InventoryCount = GetCount(self);
            if(InventoryCount > 0 && self.master)
            {
                if(self.master.teamIndex == TeamIndex.Player && !self.isPlayerControlled)
                {
                    self.inventory.RemoveItem(ItemDef, InventoryCount);
                    var limiterReleaseCount = GetCountSpecific(self, LimiterReleaseItemDef);
                    self.inventory.RemoveItem(LimiterReleaseItemDef, limiterReleaseCount);
                }
            }
        }

        private void ManageBonusesAndPenalties(RoR2.CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            if(InventoryCount > 0)
            {
                args.moveSpeedMultAdd -= Mathf.Min(InventoryCount * BaseMovementSpeedReductionPercentage, MovementSpeedReductionPercentageCap);
                args.attackSpeedMultAdd -= Mathf.Min(InventoryCount * BaseAttackSpeedReductionPercentage, AttackSpeedReductionPercentageCap);
            }

            var LimiterReleaseCount = GetCountSpecific(sender, LimiterReleaseItemDef);
            if(LimiterReleaseCount > 0)
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
            if(!ankletTracker) { ankletTracker = self.gameObject.AddComponent<AnkletTracker>(); }

            var inventoryCount = GetCount(self);
            if(inventoryCount > ankletTracker.AnkletStacks)
            {
                ankletTracker.AnkletStacks = inventoryCount;
            }
            else if(inventoryCount < ankletTracker.AnkletStacks)
            {
                var calculatedStacks = ankletTracker.AnkletStacks - inventoryCount;
                ankletTracker.AnkletStacks = inventoryCount;
                self.inventory.GiveItem(LimiterReleaseItemDef, calculatedStacks);
            }
        }

        private void ManageLimiterBuff(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {

            orig(self);

            if(self.inventory)
            {
                var inventoryCount = self.inventory.GetItemCount(LimiterReleaseItemDef);
                var buffCount = self.GetBuffCount(LimiterReleaseBuffDef);

                if(buffCount < inventoryCount)
                {
                    var iterations = inventoryCount - buffCount;
                    for (int i = 1; i <= iterations; i++)
                    {
                        self.AddBuff(LimiterReleaseBuffDef);
                    }
                }
                else if(buffCount > inventoryCount)
                {
                    var iterations = buffCount - inventoryCount;
                    for(int i = 1; i <= iterations; i++)
                    {
                        self.RemoveBuff(LimiterReleaseBuffDef);
                    }
                }
            }
        }

        private void ReduceKnockback(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            var InventoryCount = GetCount(self.body);
            if(InventoryCount > 0)
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

        public class LimiterTrailSizeHandler : MonoBehaviour
        {
            public ItemDisplay ItemDisplay;
            public TrailRenderer TrailRenderer;
            public CharacterMaster OwnerMaster;
            public void FixedUpdate()
            {

                if(!OwnerMaster || !ItemDisplay || !TrailRenderer)
                {
                    ItemDisplay = this.GetComponentInParent<ItemDisplay>();
                    if(ItemDisplay)
                    {

                        TrailRenderer = ItemDisplay.GetComponent<TrailRenderer>();

                        if(TrailRenderer)
                        {
                            TrailRenderer.transform.localScale = ItemDisplay.transform.localScale;
                        }
                        var characterModel = ItemDisplay.GetComponentInParent<CharacterModel>();

                        if(characterModel)
                        {
                            var body = characterModel.body;
                            if(body)
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
                    else if(ItemDisplay.GetVisibilityLevel() == VisibilityLevel.Invisible && TrailRenderer.enabled)
                    {
                        TrailRenderer.enabled = false;
                    }
                }
            }
        }
    }
}