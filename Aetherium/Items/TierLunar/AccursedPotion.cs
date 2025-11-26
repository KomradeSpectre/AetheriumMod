using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MiscHelpers;
using static Aetherium.Utils.MathHelpers;

namespace Aetherium.Items.TierLunar
{
    public class AccursedPotion : ItemBase<AccursedPotion>
    {
        //Config
        public static ConfigOption<bool> EnableSounds;

        public static ConfigOption<float> BaseSipCooldownDuration;
        public static ConfigOption<float> AdditionalStackSipCooldownReductionPercentage;
        public static ConfigOption<float> BaseRadiusGranted;
        public static ConfigOption<float> AdditionalRadiusGranted;
        public static ConfigOption<int> MaxEffectsAccrued;
        public static ConfigOption<string> BlacklistedBuffsAndDebuffsString;

        //Lang

        public override string ItemName => "Accursed Potion";
        public override string ItemLangTokenName => "ACCURSED_POTION";
        public override string ItemPickupDesc => "Every so often you are forced to drink a strange potion, sharing its effects with enemies around you.";

        public override string ItemFullDescription => $"Every <style=cIsUtility>{BaseSipCooldownDuration}</style> seconds <style=cStack>(reduced by {FloatToPercentageString(1 - AdditionalStackSipCooldownReductionPercentage)} per stack)</style> you are forced " +
                $"to drink a strange potion, sharing its effects with enemies in a <style=cIsUtility>{BaseRadiusGranted}m radius</style> <style=cStack>(+{AdditionalRadiusGranted}m per stack)</style> around you.</style>" +
                $" Max: {MaxEffectsAccrued} buffs or debuffs can be applied at any time.";

        public override string ItemLore =>

            "A dim light fills the shack. The figure cackles as they stir the bubbling cauldron before them.\n" +
            "\"There is no such thing as an elixir of immortality.\", they bellow, \"At least not in a static conventional sense. To create it, one must surrender to the unpredictability of chaos.\"\n\n" +

            "The figure rolls a crude bauble on the damp wooden table and takes a moment to observe it before adding another ingredient to the brew. " +
            "\"For it is in that chaos that we are granted a random possibility that we may near such a concoction, if the wheel of fate decides it so. Ah, perhaps this will do.\", they say before taking a sip from the cauldron.\n\n" +

            "For just a moment, there can be seen an expression of absolute joy on the figure's visage before it disappears and they begin letting off a faint glow. \"Alas, it does not seem the universe has seen it fit to grant me the gift I seek, but the one it deems I deserve. " +
            "Let us hope it may smile upon you where it has refused to do so for me.\"\n\n" +

            "The shack fills with light. When it begins to dim, the figure can no longer be seen and a thought starts to burrow its way into your mind, \"I know what they forgot, I can succeed where they had failed.\" Confident, you begin adding ingredients to the brew.\n\n" +

            "The work must be completed, but it seems it can wait a moment. You have a visitor.";

        public override ItemTier Tier => ItemTier.Lunar;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.Cleansable };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("AccursedPotion.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("AccursedPotionIcon.png");

        public static BuffDef AccursedPotionSipCooldownDebuff;

        public static GameObject ItemBodyModelPrefab;

        public static NetworkSoundEventDef AccursedPotionGulp;

        public List<BuffDef> BlacklistedBuffsAndDebuffs = new List<BuffDef>();

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateSound();
            CreateBuff();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            EnableSounds = config.ActiveBind<bool>("Item: " + ItemName, "Enable Sounds?", true, "Should this item be able to emit sounds in certain conditions?");

            BaseSipCooldownDuration = config.ActiveBind("Item: " + ItemName, "Base Duration of Sip Cooldown", 30f, "What should the base duration of the Accursed Potion sip cooldown be? (Default: 30 (30s))");
            AdditionalStackSipCooldownReductionPercentage = config.ActiveBind("Item: " + ItemName, "Percentage of Cooldown Reduction per Additional Stack", 0.75f, "How far should each stack reduce the cooldown? (Default: 0.75 (100% - 75% = 25% Reduction per stack))");
            BaseRadiusGranted = config.ActiveBind("Item: " + ItemName, "Default Radius of Accursed Potion Effect Sharing", 20f, "What radius of buff/debuff sharing should the first pickup have? (Default: 20m)");
            AdditionalRadiusGranted = config.ActiveBind("Item: " + ItemName, "Additional Radius Granted per Additional Stack", 5f, "What additional radius of buff/debuff sharing should each stack after grant? (Default: 5m)");
            MaxEffectsAccrued = config.ActiveBind("Item: " + ItemName, "Max Potion Effects Allowed", 8, "How many buffs or debuffs should we be able to have? (Default: 8)");
            BlacklistedBuffsAndDebuffsString = config.ActiveBind("Item: " + ItemName, "Blacklisted Buffs and Debuffs", "", "Which buffs and debuffs should not be allowed to roll via Accursed Potion?");
        }

        private void CreateSound()
        {
            AccursedPotionGulp = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            AccursedPotionGulp.eventName = "Aetherium_Gulp";
            R2API.ContentAddition.AddNetworkSoundEventDef(AccursedPotionGulp);
        }

        private void CreateBuff()
        {
            AccursedPotionSipCooldownDebuff = ScriptableObject.CreateInstance<BuffDef>();
            AccursedPotionSipCooldownDebuff.name = "Aetherium: Accursed Potion Sip Cooldown";
            AccursedPotionSipCooldownDebuff.buffColor = new Color(50, 0, 50);
            AccursedPotionSipCooldownDebuff.canStack = false;
            AccursedPotionSipCooldownDebuff.isDebuff = false;
            AccursedPotionSipCooldownDebuff.iconSprite = MainAssets.LoadAsset<Sprite>("AccursedPotionSipCooldownDebuffIcon.png");

            ContentAddition.AddBuffDef(AccursedPotionSipCooldownDebuff);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.14626F, 0.09623F, 0.05736F),
                    localAngles = new Vector3(0F, 237.9584F, 180F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.13893F, 0.1018F, 0.05313F),
                    localAngles = new Vector3(355.1616F, 81.55997F, 180F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.12508F, 0.52882F, 1.05645F),
                    localAngles = new Vector3(2.58644F, 15.4937F, 144.3143F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.25473F, 0.13682F, 0.02785F),
                    localAngles = new Vector3(6.06942F, 86.23837F, 170.9439F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.17959F, 0.02055F, -0.07736F),
                    localAngles = new Vector3(4.91717F, 72.05508F, 179.4397F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.14068F, 0.09356F, 0.04851F),
                    localAngles = new Vector3(353.4428F, 85.47656F, 180.9776F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(-0.5854F, -0.70073F, -0.27005F),
                    localAngles = new Vector3(357.1272F, 53.86685F, 346.9289F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.13296F, 0.14329F, 0.04933F),
                    localAngles = new Vector3(0F, 266.8607F, 180F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(1.55242F, 0.82693F, -0.12087F),
                    localAngles = new Vector3(1.77184F, 278.9485F, 211.384F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.11077F, 0.1393F, 0.05012F),
                    localAngles = new Vector3(0F, 90.62645F, 180F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.22591F, 0.0032F, 0.0343F),
                    localAngles = new Vector3(351.5648F, 261.5977F, 172.71F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.01245F, -0.00126F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.00339F, 0.00339F, 0.00339F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.27697F, -0.32539F, -0.0984F),
                    localAngles = new Vector3(0F, 48.7075F, 0F),
                    localScale = new Vector3(0.03752F, 0.03752F, 0.03752F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.1634F, -0.06965F, -0.00002F),
                    localAngles = new Vector3(1.8743F, 269.9206F, 0.62295F),
                    localScale = new Vector3(0.02054F, 0.02054F, 0.02054F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.13865F, -0.04514F, -0.03566F),
                    localAngles = new Vector3(11.17084F, 282.0667F, 0F),
                    localScale = new Vector3(0.02965F, 0.02965F, 0.02965F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos =    new Vector3(-0.0809F, 0.0543F, 0.19989F),
                    localAngles = new Vector3(356.8972F, 183.9839F, 175.9352F),
                    localScale =  new Vector3(0.07479F, 0.07479F, 0.07479F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.00583F, 0.00067F, 0.00583F),
                    localAngles = new Vector3(358.4079F, 152.9051F, 176.8915F),
                    localScale = new Vector3(0.00084F, 0.00084F, 0.00084F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.Run.Start += PopulateBlacklistedBuffsAndDebuffs;
            On.RoR2.CharacterBody.FixedUpdate += ForceFeedPotion;
        }

        private void PopulateBlacklistedBuffsAndDebuffs(On.RoR2.Run.orig_Start orig, RoR2.Run self)
        {
            string testString = BlacklistedBuffsAndDebuffsString;
            var testStringArray = testString.Split(',');
            if (testStringArray.Length > 0)
            {
                foreach (string stringToTest in testStringArray)
                {
                    var buff = Array.Find<BuffDef>(RoR2.BuffCatalog.buffDefs, buffDef => buffDef.name == stringToTest);
                    if (!buff) { continue; }

                    BlacklistedBuffsAndDebuffs.Add(buff);
                }
            }

            orig(self);
        }

        private void ForceFeedPotion(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (NetworkServer.active)
            {
                var InventoryCount = GetCount(self);
                if (InventoryCount > 0)
                {
                    if (!self.HasBuff(AccursedPotionSipCooldownDebuff) && self.activeBuffsListCount <= MaxEffectsAccrued)
                    {
                        BuffDef ChosenBuff = BuffCatalog.buffDefs[RoR2.Run.instance.stageRng.RangeInt(0, RoR2.BuffCatalog.buffCount - 1)];

                        if (BlacklistedBuffsAndDebuffs.Contains(ChosenBuff))
                        {
                            ChosenBuff = null;
                        }

                        if (ChosenBuff.iconSprite != null && ChosenBuff != RoR2Content.Buffs.Immune && ChosenBuff != RoR2Content.Buffs.HiddenInvincibility)
                        {
                            var BuffCount = ChosenBuff.canStack ? InventoryCount : 1;

                            var randomEffectDuration = RoR2.Run.instance.stageRng.RangeFloat(10, 20);
                            RoR2.TeamMask enemyTeams = RoR2.TeamMask.GetEnemyTeams(self.teamComponent.teamIndex);
                            RoR2.HurtBox[] hurtBoxes = new RoR2.SphereSearch
                            {
                                radius = BaseRadiusGranted + (AdditionalRadiusGranted * (InventoryCount - 1)),
                                mask = RoR2.LayerIndex.entityPrecise.mask,
                                origin = self.corePosition
                            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                            for (int i = 0; i < hurtBoxes.Length; i++)
                            {
                                var body = hurtBoxes[i].healthComponent.body;
                                if (body)
                                {
                                    AddBuffAndDot(ChosenBuff, randomEffectDuration, BuffCount, body);
                                }
                            }
                            AddBuffAndDot(AccursedPotionSipCooldownDebuff, BaseSipCooldownDuration * (float)Math.Pow(AdditionalStackSipCooldownReductionPercentage, InventoryCount - 1), 1, self);
                            AddBuffAndDot(ChosenBuff, randomEffectDuration, BuffCount, self);

                            if (EnableSounds)
                            {
                                EntitySoundManager.EmitSoundServer(AccursedPotionGulp.akId, self.gameObject);
                            }
                        }
                    }
                }
            }
            orig(self);
        }
    }
}