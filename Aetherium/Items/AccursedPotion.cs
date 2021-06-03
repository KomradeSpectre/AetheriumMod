using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Compatability.ModCompatability.BetterAPICompat;
using static Aetherium.Compatability.ModCompatability.ItemStatsModCompat;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;

namespace Aetherium.Items
{
    public class AccursedPotion : ItemBase<AccursedPotion>
    {
        //Config
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

                "A strange bottle filled with an ever shifting liquid. Upon closer inspection there is a label for the ingredients, the label reads as follows:\n" +
                "---------------------------------\n" +
                "\n<indent=5%>1 Eye of Darkness, medium well.</indent>\n" +
                "<indent=5%>15 Scalangs, preferably non-endangered.</indent>\n" +
                "<indent=5%>400 Neutron Star Cores, crushed into a fine paste with a simple glass mortar and pestle.</indent>\n" +
                "<indent=5%>7 Essence of Luck, filter through a coffee press to remove bad luck from it before adding.</indent>\n" +
                "<indent=5%>1/4th teaspoon of salt, for taste.</indent>\n" +
                "\n---------------------------------\n" +
                "\nThe label's ingredients panel seems to go on forever, changing as the bottle is rotated." +
                "\nRemote extraction of contents recommended due to hypnohazard.";

        public override ItemTier Tier => ItemTier.Lunar;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.Cleansable };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("AccursedPotion.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("AccursedPotionIcon.png");

        public static BuffDef AccursedPotionSipCooldownDebuff;

        public static GameObject ItemBodyModelPrefab;

        public List<BuffDef> BlacklistedBuffsAndDebuffs = new List<BuffDef>();

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuff();

            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            BaseSipCooldownDuration = config.ActiveBind("Item: " + ItemName, "Base Duration of Sip Cooldown", 30f, "What should the base duration of the Accursed Potion sip cooldown be? (Default: 30 (30s))");
            AdditionalStackSipCooldownReductionPercentage = config.ActiveBind("Item: " + ItemName, "Percentage of Cooldown Reduction per Additional Stack", 0.75f, "How far should each stack reduce the cooldown? (Default: 0.75 (100% - 75% = 25% Reduction per stack))");
            BaseRadiusGranted = config.ActiveBind("Item: " + ItemName, "Default Radius of Accursed Potion Effect Sharing", 20f, "What radius of buff/debuff sharing should the first pickup have? (Default: 20m)");
            AdditionalRadiusGranted = config.ActiveBind("Item: " + ItemName, "Additional Radius Granted per Additional Stack", 5f, "What additional radius of buff/debuff sharing should each stack after grant? (Default: 5m)");
            MaxEffectsAccrued = config.ActiveBind("Item: " + ItemName, "Max Potion Effects Allowed", 8, "How many buffs or debuffs should we be able to have? (Default: 8)");
            BlacklistedBuffsAndDebuffsString = config.ActiveBind("Item: " + ItemName, "Blacklisted Buffs and Debuffs", "", "Which buffs and debuffs should not be allowed to roll via Accursed Potion?");
        }

        private void CreateBuff()
        {
            AccursedPotionSipCooldownDebuff = ScriptableObject.CreateInstance<BuffDef>();
            AccursedPotionSipCooldownDebuff.name = "Aetherium: Accursed Potion Sip Cooldown";
            AccursedPotionSipCooldownDebuff.buffColor = new Color(50, 0, 50);
            AccursedPotionSipCooldownDebuff.canStack = false;
            AccursedPotionSipCooldownDebuff.isDebuff = false;
            AccursedPotionSipCooldownDebuff.iconSprite = MainAssets.LoadAsset<Sprite>("AccursedPotionSipCooldownDebuffIcon.png");

            BuffAPI.Add(new CustomBuff(AccursedPotionSipCooldownDebuff));
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
                    localPos = new Vector3(0.16f, 0.1f, 0.05f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.16f, 0.1f, 0.05f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hip",
                    localPos = new Vector3(0f, -0.3f, 1.6f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(4f, 4f, 4f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.16f, 0.1f, 0.05f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.16f, 0.1f, 0.05f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.16f, 0.1f, 0.05f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(-0.7f, -0.7f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(2f, 2f, 2f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.16f, 0.1f, 0.05f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(1.6f, 0.4f, 0.05f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = new Vector3(5f, 5f, 5f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.1f, 0.1f, 0.05f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.184F, -0.0816F, 0.0971F),
                    localAngles = new Vector3(356.2966F, 206.9181F, 7.7223F),
                    localScale = new Vector3(0.417F, 0.417F, 0.417F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.Run.Start += PopulateBlacklistedBuffsAndDebuffs;
            On.RoR2.CharacterBody.FixedUpdate += ForceFeedPotion;
            RoR2Application.onLoad += OnLoadModCompatability;
        }

        public void OnLoadModCompatability()
        {
            if (IsItemStatsModInstalled)
            {
                CreateAccursedPotionStatDef();
            }

            if (IsBetterUIInstalled)
            {
                //BetterUI Buff Description Compat
                var sipCooldownDebuffInfo = CreateBetterUIBuffInformation($"{ItemLangTokenName}_SIP_COOLDOWN", AccursedPotionSipCooldownDebuff.name, "The potion's cap has sealed itself in place. You are safe for now.", false);

                RegisterBuffInfo(AccursedPotionSipCooldownDebuff, sipCooldownDebuffInfo.Item1, sipCooldownDebuffInfo.Item2);
            }
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
            bool drankPotion = false;
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
                            drankPotion = true;
                        }
                    }
                }
            }
            if (drankPotion)
            {
                AkSoundEngine.PostEvent(722511457, self.gameObject);
            }
            orig(self);
        }

        public void AddBuffAndDot(BuffDef buff, float duration, int stackCount, RoR2.CharacterBody body)
        {
            RoR2.DotController.DotIndex index = (RoR2.DotController.DotIndex)Array.FindIndex(RoR2.DotController.dotDefs, (dotDef) => dotDef.associatedBuff == buff);
            for (int y = 0; y < stackCount; y++)
            {
                if (index != RoR2.DotController.DotIndex.None)
                {
                    RoR2.DotController.InflictDot(body.gameObject, body.gameObject, index, duration, 0.25f);
                }
                else
                {
                    body.AddTimedBuffAuthority(buff.buffIndex, duration);
                }
            }
        }
    }
}