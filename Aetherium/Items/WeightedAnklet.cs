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
using static Aetherium.CoreModules.StatHooks;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;
using static RoR2.Navigation.MapNodeGroup;

namespace Aetherium.Items
{
    public class WeightedAnklet : ItemBase<WeightedAnklet>
    {
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

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("WeightedAnklet.prefab");
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
            CreateItem();
            CreatePowerupItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
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
            LimiterReleaseBuffDef.buffColor = new Color(48, 255, 48);
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
            ItemBodyModelPrefab = ItemModel;
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
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
                    localPos = new Vector3(0f, 0.4f, 0.02f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 3f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(2, 2, 2)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.25f, 0f),
                    localAngles = new Vector3(-19f, 0f, -4f),
                    localScale = new Vector3(0.28f, 0.28f, 0.28f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.41f, 0.02f),
                    localAngles = new Vector3(-5f, 0f, 0f),
                    localScale = new Vector3(0.19f, 0.19f, 0.19f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.32f, 0.025f),
                    localAngles = new Vector3(-10f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FootFrontL",
                    localPos = new Vector3(0f, 1f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.4f, 0.4f, 0.4f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.01f, 0.39f, 0.02f),
                    localAngles = new Vector3(-6f, 0f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 3f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1.5f, 1.5f, 1.5f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
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

        private ItemDisplayRuleDict CreateLimiterItemDisplayRules()
        {
            LimiterReleaseEyePrefab = MainAssets.LoadAsset<GameObject>("LimiterReleaseEyeTrail.prefab");
            var itemDisplay = LimiterReleaseEyePrefab.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(LimiterReleaseEyePrefab);
            itemDisplay.gameObject.AddComponent<LimiterTrailSizeHandler>();

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
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
            return rules;
        }

        private void CreatePowerupItem()
        {
            LanguageAPI.Add("HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_NAME", "Weighted Anklet Limiter Release");
            LanguageAPI.Add("HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_PICKUP", "You feel much lighter, and your senses keener.");
            LanguageAPI.Add("HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_DESCRIPTION", $"You gain [x] movement speed (+[x] per stack), [x] attack speed (+[x] per stack), and [x] damage bonus (+[x] per stack). Gain the ability to dodge [x] times out of the way of close ranged attacks and behind the attacker before entering a cooldown period.");

            LimiterReleaseItemDef = ScriptableObject.CreateInstance<ItemDef>();
            LimiterReleaseItemDef.name = "HIDDEN_ITEM_WEIGHTED_ANKLET_LIMITER_RELEASE";
            LimiterReleaseItemDef.nameToken = "HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_NAME";
            LimiterReleaseItemDef.pickupToken = "HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_PICKUP";
            LimiterReleaseItemDef.descriptionToken = "HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_DESCRIPTION";
            LimiterReleaseItemDef.loreToken = "";
            LimiterReleaseItemDef.hidden = true;
            LimiterReleaseItemDef.canRemove = false;
            LimiterReleaseItemDef.tier = ItemTier.NoTier;

            ItemAPI.Add(new CustomItem(LimiterReleaseItemDef, CreateLimiterItemDisplayRules()));
        }

        public override void Hooks()
        {
            GetStatCoefficients += ManageBonusesAndPenalties;
            On.RoR2.CharacterMaster.OnInventoryChanged += ManageLimiter;
            On.RoR2.CharacterBody.FixedUpdate += ManageLimiterBuff;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += ManageLimiterBuffCooldown;

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