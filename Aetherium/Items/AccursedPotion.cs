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
using UnityEngine.Networking;

namespace Aetherium.Items
{
    public class AccursedPotion : Item<AccursedPotion>
    {
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("What should the base duration of the Accursed Potion sip cooldown be? (Default: 30 (30s))", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseSipCooldownDuration { get; private set; } = 30f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("How far should each stack reduce the cooldown? (Default: 0.75 (75%))", AutoItemConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float additionalStackSipCooldownReductionPercentage { get; private set; } = 0.75f;

        public override string displayName => "Accursed Potion";

        public override ItemTier itemTier => RoR2.ItemTier.Lunar;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "Every so often you are forced to drink a strange potion, sharing its effects with enemies around you.";

        protected override string NewLangDesc(string langid = null) => $"";

        protected override string NewLangLore(string langID = null) => "";

        private static List<RoR2.CharacterBody> Playername = new List<RoR2.CharacterBody>();

        public Xoroshiro128Plus random = new Xoroshiro128Plus((ulong)System.DateTime.Now.Ticks);

        public static GameObject ItemBodyModelPrefab;

        public static BuffIndex AccursedPotionSipCooldownDebuff;

        public AccursedPotion()
        {
            modelPathName = "@Aetherium:Assets/Models/Prefabs/AccursedPotion.prefab";
            iconPathName = "@Aetherium:Assets/Textures/Icons/AccursedPotionIcon.png";

            onAttrib += (tokenIdent, namePrefix) =>
            {
                var sipCooldownDebuff = new R2API.CustomBuff(
                    new RoR2.BuffDef
                    {
                        buffColor = new Color(50, 0, 50),
                        canStack = false,
                        isDebuff = false,
                        name = namePrefix + ": Accursed Potion Sip Cooldown",
                        iconPath = "@Aetherium:Assets/Textures/Icons/AccursedPotionSipCooldownDebuffIcon.png"
                    });
                AccursedPotionSipCooldownDebuff = R2API.BuffAPI.Add(sipCooldownDebuff);
            };
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = AetheriumPlugin.ItemDisplaySetup(ItemBodyModelPrefab);

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

        protected override void LoadBehavior()
        {
            if (ItemBodyModelPrefab == null)
            {
                var meshes = regDef.pickupModelPrefab.GetComponentsInChildren<MeshRenderer>();
                meshes[2].material.SetFloat("_FillAmount", 0.28f);
                var wobble = meshes[2].gameObject.AddComponent<Wobble>();
                wobble.MaxWobble = 0.02f;

                ItemBodyModelPrefab = regDef.pickupModelPrefab;
                regItem.ItemDisplayRules = GenerateItemDisplayRules();

            }

            On.RoR2.CharacterBody.FixedUpdate += ForceFeedPotion;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.CharacterBody.FixedUpdate -= ForceFeedPotion;
        }

        private void ForceFeedPotion(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (NetworkServer.active)
            {
                var InventoryCount = GetCount(self);
                if (InventoryCount > 0)
                {
                    if (!self.HasBuff(AccursedPotionSipCooldownDebuff) && self.activeBuffsListCount <= 8)
                    {
                        BuffIndex ChosenBuff = RoR2.BuffCatalog.buffDefs[random.RangeInt(0, RoR2.BuffCatalog.buffCount - 1)].buffIndex;
                        if (RoR2.BuffCatalog.GetBuffDef(ChosenBuff).iconPath != null && ChosenBuff != BuffIndex.Immune && ChosenBuff != BuffIndex.HiddenInvincibility)
                        {
                            var BuffCount = RoR2.BuffCatalog.GetBuffDef(ChosenBuff).canStack ? InventoryCount : 1;

                            var randomEffectDuration = random.RangeFloat(10, 20);
                            RoR2.TeamMask enemyTeams = RoR2.TeamMask.GetEnemyTeams(self.teamComponent.teamIndex);
                            RoR2.HurtBox[] hurtBoxes = new RoR2.SphereSearch
                            {
                                radius = 20 + (5 * (InventoryCount - 1)),
                                mask = RoR2.LayerIndex.entityPrecise.mask,
                                origin = self.corePosition
                            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                            for (int i = 0; i < hurtBoxes.Length; i++)
                            {
                                var body = hurtBoxes[i].healthComponent.body;
                                if (body)
                                {
                                    CheckIfAuthorityThenAddBuff(ChosenBuff, randomEffectDuration, BuffCount, body);
                                }
                            }
                            CheckIfAuthorityThenAddBuff(AccursedPotionSipCooldownDebuff, baseSipCooldownDuration * (float)Math.Pow(additionalStackSipCooldownReductionPercentage, InventoryCount - 1), 1, self);
                            CheckIfAuthorityThenAddBuff(ChosenBuff, randomEffectDuration, BuffCount, self);
                        }
                    }
                }
            }
            orig(self);
        }

        public void CheckIfAuthorityThenAddBuff(BuffIndex buff, float duration, int stackCount, RoR2.CharacterBody body)
        {
            for (int y = 0; y < stackCount; y++)
            {
                if (body.localPlayerAuthority)
                {
                    body.AddTimedBuffAuthority(buff, duration);
                }
                else
                {
                    body.AddTimedBuff(buff, duration);
                }
            }
        }
    }
}
