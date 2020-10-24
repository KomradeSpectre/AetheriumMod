using Aetherium.OrbVisuals;
using Aetherium.Utils;
using EntityStates;
using KomradeSpectre.Aetherium;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RewiredConsts;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using TILER2;
using UnityEngine;
using UnityEngine.Networking;

namespace Aetherium.Items
{
    public class WitchesRing : Item_V2<WitchesRing>
    {
        public override string displayName => "Witches Ring";

        public override ItemTier itemTier => RoR2.ItemTier.Tier3;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.OnKillEffect});
        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => "When you kill an enemy that has debuffs, spread them to the nearest enemy in range to it.";

        protected override string GetDescString(string langid = null) => $"";

        protected override string GetLoreString(string langID = null) => "";

        private static List<RoR2.CharacterBody> Playername = new List<RoR2.CharacterBody>();
        public static GameObject ItemBodyModelPrefab;

        public WitchesRing()
        {
            modelResourcePath = "@Aetherium:Assets/Models/Prefabs/WitchesRing.prefab";
            iconResourcePath = "@Aetherium:Assets/Textures/Icons/WitchesRingIcon.png";
        }

        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(modelResourcePath);
                displayRules = GenerateItemDisplayRules();
            }
            base.SetupAttributes();
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.2f, 0.2f, 0.2f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HandL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.05f, 0.15f, -0.12f),
                    localAngles = new Vector3(0f, -90f, 0f),
                    localScale = new Vector3(0.14f, 0.14f, 0.14f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(-2f, 6f, 0f),
                    localAngles = new Vector3(45f, -90f, 0f),
                    localScale = generalScale * 10
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.22f, -0.28f),
                    localAngles = new Vector3(0f, -90, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.15f, -0.12f),
                    localAngles = new Vector3(0f, -90f, 0f),
                    localScale = new Vector3(0.14f, 0.14f, 0.14f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.19f, -0.22f),
                    localAngles = new Vector3(0f, -90f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.17f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "WeaponPlatform",
                    localPos = new Vector3(0.2f, 0.05f, 0.2f),
                    localAngles = new Vector3(0f, -180f, 0f),
                    localScale = generalScale * 2
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.22f, -0.26f),
                    localAngles = new Vector3(0f, -90, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 4.4f),
                    localAngles = new Vector3(0f, 90f, 0f),
                    localScale = generalScale * 4
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.2f, -0.22f),
                    localAngles = new Vector3(0f, -90f, 0f),
                    localScale = generalScale
                }
            });
            return rules;
        }

        public override void Install()
        {
            base.Install();

            On.RoR2.GlobalEventManager.OnCharacterDeath += SpreadBuffOnKill;

        }

        public override void Uninstall()
        {
            base.Uninstall();

            On.RoR2.GlobalEventManager.OnCharacterDeath -= SpreadBuffOnKill;

        }

        private void SpreadBuffOnKill(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, RoR2.GlobalEventManager self, RoR2.DamageReport damageReport)
        {
            var body = damageReport.attackerBody;
            var enemyDeathTarget = damageReport.victimBody;
            if (body && enemyDeathTarget)
            {
                var InventoryCount = GetCount(body);
                if(InventoryCount > 0)
                {
                    if (enemyDeathTarget.activeBuffsList.Length > 0)
                    {
                        List<BuffIndex> buffsToAdd = new List<BuffIndex>();

                        foreach(BuffIndex buff in enemyDeathTarget.activeBuffsList)
                        {
                            if (!BuffCatalog.GetBuffDef(buff).isDebuff) { continue; }
                            buffsToAdd.Add(buff);
                        }

                        var cachedPositionOfDeathTarget = enemyDeathTarget.corePosition;
                        orig(self, damageReport);
                        RoR2.TeamMask enemyTeams = RoR2.TeamMask.GetEnemyTeams(body.teamComponent.teamIndex);
                        RoR2.HurtBox[] hurtBoxes = new RoR2.SphereSearch
                        {
                            radius = 30,
                            mask = RoR2.LayerIndex.entityPrecise.mask,
                            origin = body.corePosition
                        }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                        if (hurtBoxes.Length > 0)
                        {
                            for (int i = 0; i < InventoryCount; i++)
                            {
                                var enemyBody = hurtBoxes[i].healthComponent.body;
                                if (enemyBody)
                                {
                                    foreach(BuffIndex buff in buffsToAdd)
                                    {
                                        enemyBody.AddTimedBuffAuthority(buff, 5 * InventoryCount);
                                        Chat.AddMessage($"{buff}");
                                    }
                                }
                            }
                        }
                        return;
                    }
                }
            }
            orig(self, damageReport);
        }
    }
}
