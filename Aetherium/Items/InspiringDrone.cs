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

namespace Aetherium.Items
{
    class InspiringDrone : Item<InspiringDrone>
    {
        public override string displayName => "Inspiring Drone";

        public override ItemTier itemTier => RoR2.ItemTier.Tier3;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.AIBlacklist, ItemTag.Utility});
        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "When a drone is purchased, it is granted a portion of all your stats.";

        protected override string NewLangDesc(string langid = null) => "When a bot is purchased, it gains a <style=cIsUtility>50% boost to each of its stats based on yours</style> <style=cStack>(+50% per stack, linearly)</style>. \n" +
            "Additionally, some bots <style=cIsUtility>gain more ammo for their attacks based on the bonus to their attack speed</style>, and have their ammo replenished twice as fast per additional Inspiring Drone.";

        protected override string NewLangLore(string langID = null) => "[Engineer's Notes]: Let me preface this by saying that none of us have built a drone with the model number '1N-5P1R3'.\n" +
            "I ran diagnostics on '1N-SP1R3' and some of the parts that popped up were sourced from a healing drone that we had reported missing in action. The thing of note with this drone are the other parts on it.\n" +
            "\n- <indent=5%>Two Flashlights retrofitted with a camera near the center. Reported missing from Exploration two weeks ago.</indent>\n" +
            "- <indent=5%>Two High Performance Walkie Talkies. Reported missing from Security two weeks ago.</indent>\n" +
            "- <indent=5%>Two LEDs attuned to light green. Reported missing from the Mess Hall's vending machines two weeks ago.</indent>\n" +
            "- <indent=5%>A computer screen, some glass, and a handful of high precision trackballs. Reported missing from R&D two weeks ago.</indent>\n" +
            "- <indent=5%>A handful of various metal parts, and two vernier thrusters. Reported missing from my department (Engineering) two weeks ago.</indent>\n" +
            "- <indent=5%>Its hat, which seems to be fashioned out of sheet metal. As far as I can guess, the little fella made it.</indent>\n" +
            "\nYou get the point by now.\n" +
            "Aside from the odd choice in parts this drone is made of, it seems to communicate with and affect bots nearby it in a seemingly beneficial way." +
            "When we're not looking, the drone seems to 'upgrade' other bots with the capability to absorb the combat data of their operators when they're activated, according to diagnostics ran on said bots.\n" +
            "The way the little guy is so industrious inspires me as well. I think I'll produce a few copies of this model if I can, but I'm not sure how to make his little hat..";


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
                            TeleportBody(self, TargetBody.corePosition);
                            if (characterMaster.gameObject.name.StartsWith("Turret1Master"))
                            {
                                TrackerComponent.TeleportTimer = 20;
                            }
                            else
                            {
                                TrackerComponent.TeleportTimer = 10;
                            }
                        }
                    }
                }
            }
            orig(self);
        }

        private void TeleportBody(CharacterBody characterBody, Vector3 desiredPosition)
        {
            if (!Util.HasEffectiveAuthority(characterBody.gameObject))
            {
                return;
            }

            SpawnCard spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
            spawnCard.hullSize = characterBody.hullClassification;
            spawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
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
