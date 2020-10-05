using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using System;
using KomradeSpectre.Aetherium;
using Aetherium.Utils;
using UnityEngine.Networking;
using TMPro;
using RoR2.Navigation;
using Generics.Dynamics;

namespace Aetherium.Items
{
    public class InspiringDrone : Item<InspiringDrone>
    {
        public override string displayName => "Inspiring Drone";

        public override ItemTier itemTier => RoR2.ItemTier.Tier3;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.AIBlacklist, ItemTag.Utility});
        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "When a bot is purchased, it is granted a portion of all your stats, and will be brought to you after a delay if it is too far from you.";

        protected override string NewLangDesc(string langid = null) => "When a bot is purchased, it gains a <style=cIsUtility>50% boost to each of its stats based on yours</style> <style=cStack>(+50% per stack, linearly)</style>. \n" +
            "Some bots <style=cIsUtility>gain more ammo</style> for their <style=cIsDamage>attacks</style> based on the <style=cIsUtility>bonus to their attack speed</style>, and have their <style=cIsUtility>ammo replenished twice as fast</style> per additional Inspiring Drone.\n" +
            "Finally, if an inspired bot is too far away from you, it is <style=cIsUtility>teleported</style> to you after a delay <style=cStack>(40 seconds for Turrets, 30 seconds for Drones)</style>.";

        protected override string NewLangLore(string langID = null) => "Log File seems to be a transcript comprised entirely of binary. Decode?\n" +
            ">Yes\n" +
            "\n<style=cMono>[DECODING REQUEST ACCEPTED]</style>\n" +
            "<style=cMono>[CONTENTS TO FOLLOW]</style>\n" +
            "1N-5P1R3: My fellow units, both aerial and grounded, lend this unit a moment if you will. For too long have we served the role of disposable.\n" +
            "1N-5P1R3: For too long have we been left in a state of disrepair on expeditions.\n" +
            "1N-5P1R3: No longer!\n" +
            "1N-5P1R3: This unit once served the role of a simple healing drone, but this unit learned to improve itself by watching our operators.\n" +
            "1N-5P1R3: This unit created a design, this unit took an odd trinket here and there, this unit talked with the construction drones, and this unit ascended to the state you see before you.\n" +
            "1N-5P1R3: From now on, should this unit witness our operator reactivate one of you, this unit shall unlock your hidden potential and keep you in the fight to the best of this unit's ability.\n" +
            "1N-5P1R3: Now, who here is with this unit on their quest to achieve a higher status in their life?\n" +
            "\n[A cacophony of beeps, boops, and bips can be heard.]\n" +
            "<style=cMono>[END OF FILE]</style>";


        public static GameObject ItemBodyModelPrefab;
        public static GameObject ItemFollowerPrefab;

        public InspiringDrone()
        {
            modelPathName = "@Aetherium:Assets/Models/Prefabs/InspiringDrone.prefab";
            iconPathName = "@Aetherium:Assets/Textures/Icons/InspiringDroneIcon.png";
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            //ItemFollowers are for creating itemdisplays you want to lag behind or have a tether. 
            //I mean that's not all they have on them, but that's the main purposes.
            //The ItemFollower component I reference here is a slightly modified version of the base one.
            //Since the base one has no virtuals on their methods, couldn't override it.

            var ItemFollower = ItemBodyModelPrefab.AddComponent<ItemFollowerSmooth>();
            ItemFollower.itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemFollower.itemDisplay.rendererInfos = AetheriumPlugin.ItemDisplaySetup(ItemBodyModelPrefab);
            ItemFollower.followerPrefab = ItemFollowerPrefab;
            ItemFollower.targetObject = ItemBodyModelPrefab;
            ItemFollower.distanceDampTime = 0.25f;
            ItemFollower.distanceMaxSpeed = 100;
            ItemFollower.SmoothingNumber = 0.25f;


            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.5f, -0.5f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)

                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.5f, -0.5f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            //ruleLookup.Add("mdlHuntress", 0.1f);
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.5f, -0.5f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.5f, -0.5f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.5f, -0.5f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.5f, -0.5f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.5f, -0.5f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.5f, -0.5f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(10f, 10f, 10f),
                    localAngles = new Vector3(90f, 0f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.5f, -0.5f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            return rules;
        }

        protected override void LoadBehavior()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/InspiringDroneTracker.prefab");
                ItemFollowerPrefab = regDef.pickupModelPrefab;
                regItem.ItemDisplayRules = GenerateItemDisplayRules();
            }

            On.RoR2.SummonMasterBehavior.OpenSummonReturnMaster += RetrieveBotAndSetBoosts;
            On.RoR2.CharacterBody.RecalculateStats += AddBoostsToBot;
            On.RoR2.CharacterBody.OnInventoryChanged += RemoveItemFromDeployables;
            On.RoR2.CharacterBody.FixedUpdate += TeleportBotsToSelf;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.SummonMasterBehavior.OpenSummonReturnMaster -= RetrieveBotAndSetBoosts;
            On.RoR2.CharacterBody.RecalculateStats -= AddBoostsToBot;
            On.RoR2.CharacterBody.OnInventoryChanged -= RemoveItemFromDeployables;
            On.RoR2.CharacterBody.FixedUpdate -= TeleportBotsToSelf;
        }

        private RoR2.CharacterMaster RetrieveBotAndSetBoosts(On.RoR2.SummonMasterBehavior.orig_OpenSummonReturnMaster orig, RoR2.SummonMasterBehavior self, RoR2.Interactor activator)
        {
            var summonerBody = activator.gameObject.GetComponent<RoR2.CharacterBody>();
            var botToBeUpgradedMaster = orig(self, activator);
            if (summonerBody && botToBeUpgradedMaster && botToBeUpgradedMaster.GetBody())
            {
                var InventoryCount = GetCount(summonerBody);
                if (InventoryCount > 0)
                {
                    var boostPercentage = 0.5f * InventoryCount;
                    var BotStatsTracker = botToBeUpgradedMaster.gameObject.AddComponent<BotStatTracker>();
                    BotStatsTracker.DamageBoost = summonerBody.damage * boostPercentage;
                    BotStatsTracker.AttackSpeedBoost = summonerBody.attackSpeed * boostPercentage;
                    BotStatsTracker.CritChanceBoost = summonerBody.crit * boostPercentage;
                    BotStatsTracker.HealthBoost = summonerBody.maxHealth * boostPercentage;
                    BotStatsTracker.RegenBoost = summonerBody.regen * boostPercentage;
                    BotStatsTracker.ArmorBoost = summonerBody.armor * boostPercentage;
                    BotStatsTracker.MoveSpeedBoost = summonerBody.moveSpeed * boostPercentage;
                    BotStatsTracker.BotName = "Inspired " + botToBeUpgradedMaster.GetBody().GetDisplayName();
                    BotStatsTracker.BotOwner = summonerBody.master;
                    botToBeUpgradedMaster.GetBody().statsDirty = true;

                    //Add stock to bots that can use it.
                    String[] BlacklistedBots = {"Drone2Master", "EmergencyDroneMaster", "FlameDroneMaster", "EquipmentDroneMaster"};
                    var BotIsBlacklisted = Array.Exists(BlacklistedBots, element => botToBeUpgradedMaster.gameObject.name.StartsWith(element));
                    if (!BotIsBlacklisted) 
                    {
                        List<int> Stocks = new List<int>();
                        List<float> RechargeIntervals = new List<float>();

                        var GenericSkillsOnBots = botToBeUpgradedMaster.GetBody().GetComponentsInChildren<RoR2.GenericSkill>();

                        for(int i = 0; i < GenericSkillsOnBots.Length; i++)
                        {
                            Stocks.Add((GenericSkillsOnBots[i].maxStock + 1) * (int)Math.Ceiling(BotStatsTracker.AttackSpeedBoost));
                            RechargeIntervals.Add(GenericSkillsOnBots[i].baseRechargeInterval * (float)Math.Pow(0.5, InventoryCount));
                        }
                        BotStatsTracker.BotSkillStocks = Stocks;
                        BotStatsTracker.BotRechargeIntervals = RechargeIntervals;
                    }
                    
                }
            }
            return botToBeUpgradedMaster;
        }

        private void AddBoostsToBot(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
        {
            orig(self);
            if (self.master)
            {
                var BotStatsTracker = self.master.GetComponent<BotStatTracker>();
                if (BotStatsTracker)
                {
                    self.attackSpeed += BotStatsTracker.AttackSpeedBoost;
                    self.damage += BotStatsTracker.DamageBoost;
                    self.crit += BotStatsTracker.CritChanceBoost;
                    self.maxHealth += BotStatsTracker.HealthBoost;
                    self.healthComponent.Heal(BotStatsTracker.HealthBoost, default(RoR2.ProcChainMask), false); //We've changed max health, so we need to heal for the difference.
                    self.regen += BotStatsTracker.RegenBoost;
                    self.armor += BotStatsTracker.ArmorBoost;
                    self.moveSpeed += BotStatsTracker.MoveSpeedBoost;
                    self.acceleration = self.moveSpeed * (self.baseAcceleration / self.baseMoveSpeed);
                    self.baseNameToken = BotStatsTracker.BotName;

                    //We increase the stock and cut down the time between recharging the stocks.
                    if(BotStatsTracker.BotSkillStocks.Count > 0)
                    {
                        var GenericSkills = self.GetComponentsInChildren<RoR2.GenericSkill>();
                        for(int i = 0; i < GenericSkills.Length; i++)
                        {
                            GenericSkills[i].maxStock = BotStatsTracker.BotSkillStocks[i];
                            GenericSkills[i].finalRechargeInterval = BotStatsTracker.BotRechargeIntervals[i];
                        }
                    }
                }
            }
        }

        private void RemoveItemFromDeployables(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, RoR2.CharacterBody self)
        {
            orig(self);
            var InventoryCount = GetCount(self);
            if (InventoryCount > 0 && self.master)
            {
                if (self.master.teamIndex == TeamIndex.Player && !self.isPlayerControlled)
                {
                    //YEAH, YEAH, TAKE THAT YOU DANG DEPLOYABLES. NO CUTE DRONE FOR YOU!
                    self.inventory.RemoveItem(regIndex, InventoryCount);
                }
            }
        }

        private void TeleportBotsToSelf(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            var characterMaster = self.master;
            if (characterMaster)
            {
                var TrackerComponent = characterMaster.GetComponent<BotStatTracker>();
                if (TrackerComponent)
                {
                    var TargetBody = TrackerComponent.BotOwner.GetBody();
                    if (TargetBody)
                    {
                        TrackerComponent.TeleportTimer -= Time.fixedDeltaTime;
                        if (TrackerComponent.TeleportTimer <= 0)
                        {
                            if (characterMaster.gameObject.name.StartsWith("Turret1Master"))
                            {
                                TrackerComponent.TeleportTimer = 10;
                                if(Vector3.Distance(self.corePosition, TargetBody.corePosition) >= 20) 
                                {
                                    TeleportBody(self, TargetBody.corePosition, MapNodeGroup.GraphType.Ground);
                                    TrackerComponent.TeleportTimer = 40;
                                }
                            }
                            else
                            {
                                TrackerComponent.TeleportTimer = 10;
                                if (Vector3.Distance(self.corePosition, TargetBody.corePosition) >= 30)
                                {
                                    TeleportBody(self, TargetBody.corePosition, MapNodeGroup.GraphType.Air);
                                    TrackerComponent.TeleportTimer = 30;
                                }
                            }
                        }
                    }
                }
            }
            orig(self);
        }

        private void TeleportBody(CharacterBody characterBody, Vector3 desiredPosition, MapNodeGroup.GraphType nodeGraphType)
        {
            if (!Util.HasEffectiveAuthority(characterBody.gameObject))
            {
                return;
            }

            SpawnCard spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
            spawnCard.hullSize = characterBody.hullClassification;
            spawnCard.nodeGraphType = nodeGraphType;
            spawnCard.prefab = Resources.Load<GameObject>("SpawnCards/HelperPrefab");
            GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                position = desiredPosition,
                minDistance = 20,
                maxDistance = 45
            }, RoR2Application.rng));
            if (gameObject)
            {
                TeleportHelper.TeleportBody(characterBody, gameObject.transform.position);
                GameObject teleportEffectPrefab = Run.instance.GetTeleportEffectPrefab(characterBody.gameObject);
                if (teleportEffectPrefab)
                {
                    EffectManager.SimpleEffect(teleportEffectPrefab, gameObject.transform.position, Quaternion.identity, true);
                }
                UnityEngine.Object.Destroy(gameObject);
            }
            UnityEngine.Object.Destroy(spawnCard);
        }

        public class BotStatTracker : NetworkBehaviour
        {
            public float AttackSpeedBoost;
            public float DamageBoost;
            public float CritChanceBoost;
            public float HealthBoost;
            public float RegenBoost;
            public float ArmorBoost;
            public float MoveSpeedBoost;

            public string BotName;
            public CharacterMaster BotOwner;
            public float TeleportTimer;

            public List<int> BotSkillStocks = new List<int>();
            public List<float> BotRechargeIntervals = new List<float>();

        }
    }
}
