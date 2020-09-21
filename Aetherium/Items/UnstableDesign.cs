using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using System;
using KomradeSpectre.Aetherium;
using UnityEngine.Networking;
using RoR2.CharacterAI;

namespace Aetherium.Items
{
    class UnstableDesign : Item<UnstableDesign>
    {
        public override string displayName => "Unstable Design";

        public override ItemTier itemTier => RoR2.ItemTier.Lunar;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Cleansable });
        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "Every 30 seconds you are compelled to create a very <color=#FF0000>'FRIENDLY'</color> Lunar Chimera, if one of your creations does not already exist.";

        protected override string NewLangDesc(string langid = null) => "Every 30 seconds you are compelled to create a very <color=#FF0000>'FRIENDLY'</color> Lunar Chimera, if one of your creations does not already exist. " + 
            "\nIt has a <style=cIsDamage>400% base damage boost</style> <style=cStack>(+100% per stack)</style>." +
            "\nIt has a <style=cIsHealing>10% base HP boost</style> <style=cStack>(+10% per stack)</style>." +
            "\nIt has a <style=cIsDamage>300% base attack speed boost</style>." + 
            "\nFinally, it has a <style=cIsUtility>24% base movement speed boost</style> <style=cStack>(+24% per stack)</style>." +
            "\nThis monstrosity <style=cIsDamage>can level up from kills</style>.";

        protected override string NewLangLore(string langID = null) => "We entered this predicament when one of our field testers brought back a blueprint from a whole mountain of them they found on the moon. " +
            "The blueprints seemed to have various formulas and pictures on it relating to the weird constructs we saw roaming the place. " +
            "Jimenez from Engineering got his hands on it and thought he could contribute to the rest of the team by deciphering it and creating the contents for us. " +
            "We are now waiting for Security to handle the very <color=#FF0000>'FRIENDLY'</color> construct that is making a mess of the lower sectors of the station. " +
            "Thanks Jimenez.";

        public static GameObject ItemBodyModelPrefab;

        public UnstableDesign()
        {
            modelPathName = "@Aetherium:Assets/Models/Prefabs/UnstableDesign.prefab";
            iconPathName = "@Aetherium:Assets/Textures/Icons/UnstableDesignIcon.png";
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = AetheriumPlugin.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0.5f, -0.2f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(1, 1, 1)

                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0, -0.07f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0.8f, -2.2f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(7, 7, 7)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0.6f, -0.2f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0.34f, -0.1f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0, -0.23f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0.9f, -0.8f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(3, 3, 3)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0.45f, -0.4f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 2, 5),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(8, 8, 8)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, -0.18f, -0.28f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            return rules;
        }

        protected override void LoadBehavior()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/UnstableDesignRolledUp.prefab");
                regItem.ItemDisplayRules = GenerateItemDisplayRules();
            }
            On.RoR2.CharacterBody.FixedUpdate += SummonLunarChimera;
            On.RoR2.CharacterBody.FixedUpdate += LunarChimeraRetarget;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.CharacterBody.FixedUpdate -= SummonLunarChimera;
            On.RoR2.CharacterBody.FixedUpdate -= LunarChimeraRetarget;
        }

        private void SummonLunarChimera(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (NetworkServer.active)
            {
                var LunarChimeraComponent = self.GetComponent<LunarChimeraComponent>();
                if (!LunarChimeraComponent) { LunarChimeraComponent = self.gameObject.AddComponent<LunarChimeraComponent>(); }

                int InventoryCount = GetCount(self);
                if (InventoryCount > 0)
                {
                    if (LunarChimeraComponent.LastChimeraSpawned == null || !LunarChimeraComponent.LastChimeraSpawned.master || !LunarChimeraComponent.LastChimeraSpawned.master.hasBody)
                    {
                        LunarChimeraComponent.LastChimeraSpawned = null;
                        LunarChimeraComponent.ResummonCooldown -= Time.fixedDeltaTime;
                        if (LunarChimeraComponent.ResummonCooldown <= 0f && SceneCatalog.mostRecentSceneDef != SceneCatalog.GetSceneDefFromSceneName("bazaar"))
                        {

                            RoR2.DirectorSpawnRequest directorSpawnRequest = new RoR2.DirectorSpawnRequest((RoR2.SpawnCard)Resources.Load("SpawnCards/CharacterSpawnCards/cscLunarGolem"), new RoR2.DirectorPlacementRule
                            {
                                placementMode = RoR2.DirectorPlacementRule.PlacementMode.Approximate,
                                minDistance = 10f,
                                maxDistance = 40f,
                                spawnOnTarget = self.transform
                            }, RoR2.RoR2Application.rng);
                            //directorSpawnRequest.summonerBodyObject = self.gameObject;
                            directorSpawnRequest.teamIndexOverride = TeamIndex.Neutral;
                            GameObject gameObject = RoR2.DirectorCore.instance.TrySpawnObject(directorSpawnRequest);

                            if (gameObject)
                            {
                                RoR2.CharacterMaster component = gameObject.GetComponent<RoR2.CharacterMaster>();
                                AIOwnership component2 = gameObject.GetComponent<AIOwnership>();
                                BaseAI component3 = gameObject.GetComponent<BaseAI>();

                                if (component)
                                {
                                    //RoR2.Chat.AddMessage($"Character Master Found: {component}");
                                    component.teamIndex = TeamIndex.Neutral;
                                    component.inventory.GiveItem(ItemIndex.BoostDamage, 30 + (10 * InventoryCount));
                                    component.inventory.GiveItem(ItemIndex.BoostHp, 10 * InventoryCount);
                                    component.inventory.GiveItem(ItemIndex.BoostAttackSpeed, 30);
                                    component.inventory.GiveItem(ItemIndex.Hoof, 2 * InventoryCount);
                                    RoR2.CharacterBody component4 = component.GetBody();
                                    if (component4)
                                    {
                                        //RoR2.Chat.AddMessage($"CharacterBody Found: {component4}");
                                        component4.gameObject.AddComponent<LunarChimeraRetargetComponent>();
                                        LunarChimeraComponent.LastChimeraSpawned = component4;
                                        RoR2.DeathRewards component5 = component4.GetComponent<RoR2.DeathRewards>();
                                        if (component5)
                                        {
                                            //RoR2.Chat.AddMessage($"DeathRewards Found: {component5}");
                                            component5.goldReward = 0;
                                            component5.expReward = 0;
                                        }
                                    }
                                }
                            }
                            if (gameObject) {LunarChimeraComponent.ResummonCooldown = 30f;}
                        }
                    }
                }
            }
            orig(self);
        }

        private void LunarChimeraRetarget(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            var characterMaster = self.master;
            if (characterMaster)
            {
                var baseAIComponent = characterMaster.GetComponent<BaseAI>();
                var retargetComponent = self.GetComponent<LunarChimeraRetargetComponent>();
                if (baseAIComponent && retargetComponent)
                {
                    retargetComponent.RetargetTimer -= Time.fixedDeltaTime;
                    if (retargetComponent.RetargetTimer <= 0)
                    {
                        if (!baseAIComponent.currentEnemy.hasLoS)
                        {
                            baseAIComponent.currentEnemy.Reset();
                            baseAIComponent.ForceAcquireNearestEnemyIfNoCurrentEnemy();
                            retargetComponent.RetargetTimer = 10;
                        }
                    }
                }
            }
            orig(self);
        }

        public class LunarChimeraComponent : MonoBehaviour
        {
            public RoR2.CharacterBody LastChimeraSpawned;
            public float ResummonCooldown;
        }

        public class LunarChimeraRetargetComponent : MonoBehaviour
        {
            public float RetargetTimer;
        }

    }
}
