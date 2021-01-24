using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;

namespace Aetherium.Items
{
    public class AccursedPotion : ItemBase<AccursedPotion>
    {
        //Config

        public float BaseSipCooldownDuration;
        public float AdditionalStackSipCooldownReductionPercentage;
        public float BaseRadiusGranted;
        public float AdditionalRadiusGranted;
        public float MaxEffectsAccrued;

        //Lang

        public override string ItemName => "Accursed Potion";
        public override string ItemLangTokenName => "ACCURSED_POTION";
        public override string ItemPickupDesc => "Every so often you are forced to drink a strange potion, sharing its effects with enemies around you.";

        public override string ItemFullDescription => $"Every <style=cIsUtility>{BaseSipCooldownDuration}</style> seconds <style=cStack>(reduced by {FloatToPercentageString(1 - AdditionalStackSipCooldownReductionPercentage)} per stack)</style> you are forced " +
                $"to drink a strange potion, sharing its effects with enemies in a <style=cIsUtility>{BaseRadiusGranted}m radius</style> <style=cStack>(+{AdditionalRadiusGranted}m per stack)</style> around you.</style>" +
                $" Max: {MaxEffectsAccrued} buffs or debuffs can be applied at any time.";

        public override string ItemLore => OrderManifestLoreFormatter(
                ItemName,

                "9/29/3065",

                "Bepsi Coler Corp\nNeo California, Rex Federation\nTerra",

                "8011*******",

                ItemPickupDesc,

                "Fragile / Hypnohazard / Quasi-Liquid Containment Procedures",

                "A strange bottle filled with an ever shifting liquid. Upon closer inspection there is a label for the ingredients, the label reads as follows:\n" +
                "---------------------------------\n" +
                "\n<indent=5%>1 Eye of Darkness, medium well.</indent>\n" +
                "<indent=5%>15 Scalangs, preferably non-endangered.</indent>\n" +
                "<indent=5%>400 Neutron Star Cores, crushed into a fine paste with a simple glass mortar and pestle.</indent>\n" +
                "<indent=5%>7 Essence of Luck, filter through a coffee press to remove bad luck from it before adding.</indent>\n" +
                "<indent=5%>1/4th teaspoon of salt, for taste.</indent>\n" +
                "\n---------------------------------\n" +
                "\nThe label's ingredients panel seems to go on forever, changing as the bottle is rotated." +
                "\nRemote extraction of contents recommended due to hypnohazard.");

        public override ItemTier Tier => ItemTier.Lunar;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.Cleansable };

        public override string ItemModelPath => "@Aetherium:Assets/Models/Prefabs/Item/AccursedPotion/AccursedPotion.prefab";
        public override string ItemIconPath => "@Aetherium:Assets/Textures/Icons/Item/AccursedPotionIcon.png";

        //public override bool CanRemove => false;

        public static BuffIndex AccursedPotionSipCooldownDebuff;

        public static Xoroshiro128Plus random = new Xoroshiro128Plus((ulong)System.DateTime.Now.Ticks);

        public static GameObject ItemBodyModelPrefab;

        public AccursedPotion()
        {
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateMaterials();
            CreateBuff();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            BaseSipCooldownDuration = config.Bind<float>("Item: " + ItemName, "Base Duration of Sip Cooldown", 30f, "What should the base duration of the Accursed Potion sip cooldown be? (Default: 30 (30s))").Value;
            AdditionalStackSipCooldownReductionPercentage = config.Bind<float>("Item: " + ItemName, "Percentage of Cooldown Reduction per Additional Stack", 0.75f, "How far should each stack reduce the cooldown? (Default: 0.75 (100% - 75% = 25% Reduction per stack))").Value;
            BaseRadiusGranted = config.Bind<float>("Item: " + ItemName, "Default Radius of Accursed Potion Effect Sharing", 20f, "What radius of buff/debuff sharing should the first pickup have? (Default: 20m)").Value;
            AdditionalRadiusGranted = config.Bind<float>("Item: " + ItemName, "Additional Radius Granted per Additional Stack", 5f, "What additional radius of buff/debuff sharing should each stack after grant? (Default: 5m)").Value;
            MaxEffectsAccrued = config.Bind<int>("Item: " + ItemName, "Max Potion Effects Allowed", 8, "How many buffs or debuffs should we be able to have? (Default: 8)").Value;
        }

        private void CreateMaterials()
        {
            

            var accursedPotionLabel = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/Item/AccursedPotion/AccursedPotionWrapper.mat");
            accursedPotionLabel.shader = AetheriumPlugin.HopooShader;
            accursedPotionLabel.SetFloat("_Smoothness", 0.25f);

            var accursedPotionStopper = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/Item/AccursedPotion/AccursedPotionStopper.mat");
            accursedPotionStopper.shader = AetheriumPlugin.HopooShader;
            accursedPotionStopper.SetFloat("_Smoothness", 0.6f);
            accursedPotionStopper.SetColor("_EmColor", new Color(255, 112, 212));
            accursedPotionStopper.SetFloat("_EmPower", 0.00001f);

        }

        private void CreateBuff()
        {
            var sipCooldownDebuff = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = new Color(50, 0, 50),
                canStack = false,
                isDebuff = false,
                name = "Aetherium: Accursed Potion Sip Cooldown",
                iconPath = "@Aetherium:Assets/Textures/Icons/Buff/AccursedPotionSipCooldownDebuffIcon.png"
            });
            AccursedPotionSipCooldownDebuff = R2API.BuffAPI.Add(sipCooldownDebuff);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = Resources.Load<GameObject>(ItemModelPath);
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
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
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.FixedUpdate += ForceFeedPotion;
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
                        BuffIndex ChosenBuff = RoR2.BuffCatalog.buffDefs[random.RangeInt(0, RoR2.BuffCatalog.buffCount - 1)].buffIndex;
                        if (RoR2.BuffCatalog.GetBuffDef(ChosenBuff).iconPath != null && ChosenBuff != BuffIndex.Immune && ChosenBuff != BuffIndex.HiddenInvincibility)
                        {
                            var BuffCount = RoR2.BuffCatalog.GetBuffDef(ChosenBuff).canStack ? InventoryCount : 1;

                            var randomEffectDuration = random.RangeFloat(10, 20);
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
                        }
                    }
                }
            }
            orig(self);
        }

        public void AddBuffAndDot(BuffIndex buff, float duration, int stackCount, RoR2.CharacterBody body)
        {
            DotController.DotIndex index = (DotController.DotIndex)Array.FindIndex(DotController.dotDefs, (dotDef) => dotDef.associatedBuff == buff);
            for (int y = 0; y < stackCount; y++)
            {
                if (index != DotController.DotIndex.None)
                {
                    DotController.InflictDot(body.gameObject, body.gameObject, index, duration, 0.25f);
                }
                else
                {
                    body.AddTimedBuffAuthority(buff, duration);
                }
            }
        }
    }
}