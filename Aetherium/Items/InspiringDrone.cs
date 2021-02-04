using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.CoreModules.StatHooks;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;
using static RoR2.Navigation.MapNodeGroup;

namespace Aetherium.Items
{
    public class InspiringDrone : ItemBase<InspiringDrone>
    {
        public static bool IsGreenRarity;
        public static bool SetAllStatValuesAtOnce;
        public static float AllStatValueGrantedPercentage;
        public static float DamageGrantedPercentage;
        public static float AttackSpeedGrantedPercentage;
        public static float CritChanceGrantedPercentage;
        public static float HealthGrantedPercentage;
        public static float RegenGrantedPercentage;
        public static float ArmorGrantedPercentage;
        public static float MovementSpeedGrantedPercentage;
        public static float TurretTeleportationCooldownDuration;
        public static float DroneTeleportationCooldownDuration;
        public static float TurretTeleportationDistanceAroundOwner;
        public static float DroneTeleportationDistanceAroundOwner;

        public override string ItemName => "Inspiring Drone";

        public override string ItemLangTokenName => "INSPIRING_DRONE";

        public override string ItemPickupDesc => "Your bots are granted a portion of all your stats, and will be brought to you after a delay if they are too far from you.";

        public override string ItemFullDescription => SetAllStatValuesAtOnce ?

            //Set All Values At Once

            $"Bots that you own gain a <style=cIsUtility>{FloatToPercentageString(AllStatValueGrantedPercentage)} boost to each of their stats based on yours</style> <style=cStack>(+{FloatToPercentageString(AllStatValueGrantedPercentage)} per stack, linearly)</style>.\n" +
            "Some bots <style=cIsUtility>gain more ammo</style> for their <style=cIsDamage>attacks</style> based on the <style=cIsUtility>bonus to their attack speed</style>, and have their <style=cIsUtility>ammo replenished twice as fast</style> per additional Inspiring Drone.\n" +
            $"Finally, if one of your bots are too far away from you, it is <style=cIsUtility>teleported</style> to you after a delay <style=cStack>({TurretTeleportationCooldownDuration} seconds for Turrets, {DroneTeleportationCooldownDuration} seconds for Drones)</style>." :

            //Set Values Individually

            $"Bots that you own gain the following stat boosts per stack.\n" +
            $"A <style=cIsDamage>{FloatToPercentageString(DamageGrantedPercentage)} damage boost based on yours</style>.\n" +
            $"A <style=cIsDamage>{FloatToPercentageString(AttackSpeedGrantedPercentage)} attack speed boost based on yours</style>.\n" +
            $"A <style=cIsDamage>{FloatToPercentageString(CritChanceGrantedPercentage)} crit chance boost based on yours</style>.\n" +
            $"A <style=cIsHealing>{FloatToPercentageString(HealthGrantedPercentage)} health boost based on yours</style>.\n" +
            $"A <style=cIsHealing>{FloatToPercentageString(RegenGrantedPercentage)} regen boost based on yours</style>.\n" +
            $"A <style=cIsUtility>{FloatToPercentageString(ArmorGrantedPercentage)} armor boost based on yours</style>.\n" +
            $"A <style=cIsUtility>{FloatToPercentageString(MovementSpeedGrantedPercentage)} damage boost based on yours</style>.\n" +
            $"Some bots <style=cIsUtility>gain more ammo</style> for their <style=cIsDamage>attacks</style> based on the <style=cIsUtility>bonus to their attack speed</style>, and have their <style=cIsUtility>ammo replenished twice as fast</style> per additional Inspiring Drone.\n" +
            $"Finally, if one of your bots are too far away from you, it is <style=cIsUtility>teleported</style> to you after a delay <style=cStack>({TurretTeleportationCooldownDuration} seconds for Turrets, {DroneTeleportationCooldownDuration} seconds for Drones)</style>.";

        public override string ItemLore => "Log File seems to be a transcript comprised entirely of binary. Decode?\n" +
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
            "<style=cMono>[END OF FILE]</style> ";

        public override ItemTier Tier => IsGreenRarity ? ItemTier.Tier2 : ItemTier.Tier3;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.AIBlacklist, ItemTag.Utility };

        public override string ItemModelPath => "@Aetherium:Assets/Models/Prefabs/Item/InspiringDrone/InspiringDrone.prefab";

        public override string ItemIconPath => IsGreenRarity ? "@Aetherium:Assets/Textures/Icons/Item/InspiringDroneIconTier2.png" : "@Aetherium:Assets/Textures/Icons/Item/InspiringDroneIconTier3.png";

        public static GameObject ItemBodyModelPrefab;
        public static GameObject ItemFollowerPrefab;

        private static readonly List<string> DronesList = new List<string>
        {
            "DroneBackup",
            "Drone1",
            "Drone2",
            "EmergencyDrone",
            "FlameDrone",
            "MegaDrone",
            "DroneMissile",
            "Turret1"
        };

        private static readonly List<string> BannedTeleportDrones = new List<string>();

        public InspiringDrone()
        {
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            IsGreenRarity = config.Bind<bool>("Item: " + ItemName, "Should the Inspiring Drone be Green Rarity instead of Red Rarity?", false, "Should the Inspiring Drone show up in the Tier2 (Green Rarity) item pool instead of the Tier3 (Red Rarity) item pool?").Value;
            SetAllStatValuesAtOnce = config.Bind<bool>("Item: " + ItemName, "Set All Stat Gain Percentages at Once?", true, "Do you want to set all the values of the Drone's stats at once? If false, prepare for a long description.").Value;
            AllStatValueGrantedPercentage = config.Bind<float>("Item: " + ItemName, "Stat Gain Percentage (All)", 0.5f, "What percentage of stats from the drone's owner do we transfer over to the drones per stack? 0.5 = 50%").Value;
            DamageGrantedPercentage = config.Bind<float>("Item: " + ItemName, "Damage Stat Gain (Individual)", 0.5f, "What percentage of the damage stat from the drone's owner do we transfer over to the drones per stack?").Value;
            AttackSpeedGrantedPercentage = config.Bind<float>("Item: " + ItemName, "Attack Speed Stat Gain (Individual)", 0.5f, "What percentage of the attack speed stat from the drone's owner do we transfer over to the drones per stack?").Value;
            CritChanceGrantedPercentage = config.Bind<float>("Item: " + ItemName, "Critical Chance Stat Gain (Individual)", 0.5f, "What percentage of the critical chance stat from the drone's owner do we transfer over to the drones per stack?").Value;
            HealthGrantedPercentage = config.Bind<float>("Item: " + ItemName, "Health Stat Gain (Individual)", 0.5f, "What percentage of the health stat from the drone's owner do we transfer over to the drones per stack?").Value;
            RegenGrantedPercentage = config.Bind<float>("Item: " + ItemName, "Regeneration Stat Gain (Individual)", 0.5f, "What percentage of the regeneration stat from the drone's owner do we transfer over to the drones per stack?").Value;
            ArmorGrantedPercentage = config.Bind<float>("Item: " + ItemName, "Armor Stat Gain (Individual)", 0.5f, "What percentage of the armor stat from the drone's owner do we transfer over to the drones per stack?").Value;
            MovementSpeedGrantedPercentage = config.Bind<float>("Item: " + ItemName, "Movement Speed Stat Gain (Individual)", 0.5f, "What percentage of the movement speed stat from the drone's owner do we transfer over to the drones per stack?").Value;
            TurretTeleportationCooldownDuration = config.Bind<float>("Item: " + ItemName, "Duration of Turret Teleportation Cooldown", 40f, "How many seconds till we teleport turrets (tracked individually) close to their owner? (in seconds)").Value;
            DroneTeleportationCooldownDuration = config.Bind<float>("Item: " + ItemName, "Duration of Drone Teleportation Cooldown", 30f, "How many seconds till we teleport drone (tracked individually) close to their owner? (in seconds)").Value;
            TurretTeleportationDistanceAroundOwner = config.Bind<float>("Item: " + ItemName, "Distance Away from Owner to Teleport Turrets", 20f, "How far out should we place turrets from the owner when teleporting them? (in meters)").Value;
            DroneTeleportationDistanceAroundOwner = config.Bind<float>("Item: " + ItemName, "Distance Away from Owner to Teleport Drone", 30f, "How far out should we place drone from the owner when teleporting them? (in meters)").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            //ItemFollowers are for creating itemdisplays you want to lag behind or have a tether.
            //I mean that's not all they have on them, but that's the main purposes.
            //The ItemFollower component I reference here is a slightly modified version of the base one.
            //Since the base one has no virtuals on their methods, couldn't override it.

            ItemBodyModelPrefab = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/Item/InspiringDrone/InspiringDroneTracker.prefab");
            ItemFollowerPrefab = Resources.Load<GameObject>(ItemModelPath);
            var ItemFollower = ItemBodyModelPrefab.AddComponent<ItemFollowerSmooth>();
            ItemFollower.itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemFollower.itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);
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
                    localPos = new Vector3(-8f, -4f, 5f),
                    localAngles = new Vector3(-90f, -180f, 0f),
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

        public override void Hooks()
        {
            GetStatCoefficients += AddBoostsToBot;
            On.RoR2.CharacterBody.OnInventoryChanged += RemoveItemFromDeployables;
            On.RoR2.CharacterBody.OnInventoryChanged += UpdateAllTrackers;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            CharacterMaster botMaster = obj.master;
            if (!botMaster) return;
            MinionOwnership minionOwnership = botMaster.minionOwnership;
            if (!minionOwnership) return;
            if (IsDroneSupported(botMaster))
            {
                CharacterMaster ownerMaster = botMaster.minionOwnership.ownerMaster;
                if (ownerMaster)
                {
                    BotStatTracker tracker = BotStatTracker.GetOrAddComponent(botMaster, ownerMaster, obj, ownerMaster.GetBody());
                    tracker.UpdateTrackerBoosts();
                }
            }
        }

        private void AddBoostsToBot(CharacterBody sender, StatHookEventArgs args)
        {
            CharacterMaster master = sender.master;
            if (master)
            {
                BotStatTracker tracker = master.GetComponent<BotStatTracker>();
                if (tracker)
                {
                    tracker.ApplyTrackerBoosts(args);
                    //Chat.AddMessage($"HP After: {sender.maxHealth}");
                }
            }
        }

        private void RemoveItemFromDeployables(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            var inventoryCount = GetCount(self);
            if (inventoryCount > 0 && self.master && self.inventory)
            {
                if (self.master.teamIndex == TeamIndex.Player && !self.isPlayerControlled)
                {
                    //YEAH, YEAH, TAKE THAT YOU DANG DEPLOYABLES. NO CUTE DRONE FOR YOU!
                    self.inventory.RemoveItem(Index, inventoryCount);
                }
            }
        }

        private void UpdateAllTrackers(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            CharacterMaster ownerMaster = self.master;
            MinionOwnership[] minionOwnerships = UnityEngine.Object.FindObjectsOfType<MinionOwnership>();
            foreach (MinionOwnership minionOwnership in minionOwnerships)
            {
                if (minionOwnership && minionOwnership.ownerMaster && minionOwnership.ownerMaster == ownerMaster)
                {
                    CharacterMaster minionMaster = minionOwnership.GetComponent<CharacterMaster>();
                    if (minionMaster)
                    {
                        BotStatTracker tracker = minionMaster.GetComponent<BotStatTracker>();
                        if (tracker) tracker.UpdateTrackerBoosts();
                    }
                }
            }
        }

        private bool IsDroneSupported(CharacterMaster botMaster)
        {
            return IsDroneSupported(botMaster.name);
        }

        private bool IsDroneSupported(string botMasterName)
        {
            return DronesList.Exists((droneSubstring) => { return botMasterName.Contains(droneSubstring); });
        }

        private bool IsDroneTeleportBanned(CharacterMaster botMaster)
        {
            return IsDroneTeleportBanned(botMaster.name);
        }

        private bool IsDroneTeleportBanned(string botMasterName)
        {
            return BannedTeleportDrones.Exists((droneSubstring) => { return botMasterName.Contains(droneSubstring); });
        }

        /// <summary>
        /// Allows a custom drone to be Inspired by Inspiring Drone.
        /// </summary>
        /// <param name="masterName">The CharacterMaster name of the custom drone.</param>
        /// <returns>True if the custom drone is now supported. False if the custom drone is already supported.</returns>
        public bool AddCustomDrone(string masterName)
        {
            if (IsDroneSupported(masterName)) return false;
            DronesList.Add(masterName);
            return true;
        }

        /// <summary>
        /// Allows a drone to be banned from teleporting near the player.
        /// </summary>
        /// <param name="masterName">The CharacterMaster name of the custom drone.</param>
        /// <returns>True if the custom drone is now banned. False if the custom drone is not supported or if the drone is already banned.</returns>
        public bool BanTeleportDrone(string masterName)
        {
            if (!IsDroneSupported(masterName) || IsDroneTeleportBanned(masterName)) return false;
            BannedTeleportDrones.Add(masterName);
            return true;
        }

        public class BotStatTracker : MonoBehaviour
        {
            public float AttackSpeedBoost;
            public float DamageBoost;
            public float CritChanceBoost;
            public float HealthBoost;
            public float RegenBoost;
            public float ArmorBoost;
            public float MoveSpeedBoost;
            public string BotName;
            public CharacterMaster BotOwnerMaster;
            public CharacterMaster BotMaster;
            public CharacterBody BotOwnerBody;
            public CharacterBody BotBody;
            public float TeleportTimer = 0f;
            public int BoostCount = -1;
            public List<int> BotSkillStocks = new List<int>();
            public List<float> BotRechargeIntervals = new List<float>();
            public List<int> DefaultSkillStocks = new List<int>();
            public List<float> DefaultRechargeIntervals = new List<float>();

            private string OriginalName = "";
            private bool forceRecalculateOnSpawn = true;
            private bool reassignBodies = false;
            private readonly string[] BlacklistedStockBots = { "Drone2", "EmergencyDrone", "FlameDrone", "EquipmentDrone" };

            public static BotStatTracker GetOrAddComponent(CharacterMaster bot, CharacterMaster owner, CharacterBody botBody, CharacterBody ownerBody)
            {
                BotStatTracker tracker = bot.gameObject.GetComponent<BotStatTracker>();
                if (!tracker)
                {
                    tracker = bot.gameObject.AddComponent<BotStatTracker>();
                    tracker.BotMaster = bot;
                    tracker.BotOwnerMaster = owner;
                }
                tracker.BotBody = botBody;
                tracker.BotOwnerBody = ownerBody;
                return tracker;
            }

            public void UpdateTrackerBoosts()
            {
                if (!BotBody || !BotOwnerBody)
                {
                    reassignBodies = true;
                    //AetheriumPlugin._logger.LogMessage("DOESNT EXIST");
                    return;
                }
                int inventoryCount = instance.GetCount(BotOwnerBody);
                if (BoostCount != inventoryCount)
                {
                    if (OriginalName == "") OriginalName = BotBody.GetDisplayName();
                    BoostCount = inventoryCount;
                    DamageBoost = CalculateStat(BotOwnerBody.damage, DamageGrantedPercentage);
                    AttackSpeedBoost = CalculateStat(BotOwnerBody.attackSpeed, AttackSpeedGrantedPercentage);
                    CritChanceBoost = CalculateStat(BotOwnerBody.crit, CritChanceGrantedPercentage);
                    HealthBoost = CalculateStat(BotOwnerBody.maxHealth, HealthGrantedPercentage);
                    RegenBoost = CalculateStat(BotOwnerBody.regen, RegenGrantedPercentage);
                    ArmorBoost = CalculateStat(BotOwnerBody.armor, ArmorGrantedPercentage);
                    MoveSpeedBoost = CalculateStat(BotOwnerBody.moveSpeed, MovementSpeedGrantedPercentage);
                    BotName = "";
                    if (BoostCount > 0) BotName += "Inspired ";
                    BotName += OriginalName;

                    //Add stock to bots that can use it.
                    if (!IsBlacklisted())
                    {
                        //Clear for updating.
                        //Chat.AddMessage("FIRED CLEAR");
                        BotSkillStocks.Clear();
                        BotRechargeIntervals.Clear();

                        //Assign Default values for the stocks for recomputation upon changing item count.
                        if (DefaultSkillStocks.Count <= 0 && DefaultRechargeIntervals.Count <= 0)
                        {
                            var GenericSkillsOnBots = BotBody.GetComponentsInChildren<RoR2.GenericSkill>();
                            for (int i = 0; i < GenericSkillsOnBots.Length; i++)
                            {
                                DefaultSkillStocks.Add(GenericSkillsOnBots[i].maxStock);
                                DefaultRechargeIntervals.Add(GenericSkillsOnBots[i].baseRechargeInterval);
                            }
                        }
                        for (int i = 0; i < DefaultSkillStocks.Count; i++)
                        {
                            BotSkillStocks.Add(DefaultSkillStocks[i] * (Mathf.CeilToInt(AttackSpeedBoost) + 1));
                            BotRechargeIntervals.Add(DefaultRechargeIntervals[i] * Mathf.Pow(.8f, BoostCount));
                        }
                    }
                }
            }

            public void ApplyTrackerBoosts(StatHookEventArgs args)
            {
                if (!BotBody || BoostCount < 0) return;
                args.attackSpeedMultAdd += AttackSpeedBoost;
                args.baseDamageAdd += DamageBoost;
                args.critAdd += CritChanceBoost;
                args.baseHealthAdd += HealthBoost;
                args.baseRegenAdd += RegenBoost;
                args.armorAdd += ArmorBoost;
                BotBody.moveSpeed += MoveSpeedBoost;
                BotBody.acceleration = BotBody.moveSpeed * (BotBody.baseAcceleration / BotBody.baseMoveSpeed);
                BotBody.baseNameToken = BotName;

                //We increase the stock and cut down the time between recharging the stocks.
                if (!IsBlacklisted() && BotSkillStocks.Count > 0 && BotRechargeIntervals.Count > 0)
                {
                    var GenericSkills = BotBody.GetComponentsInChildren<GenericSkill>();
                    for (int i = 0; i < GenericSkills.Length; i++)
                    {
                        GenericSkills[i].maxStock = BotSkillStocks[i];
                        GenericSkills[i].finalRechargeInterval = BotRechargeIntervals[i];
                    }
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
            private void FixedUpdate()
            {
                if (reassignBodies)
                {
                    if (!BotBody) BotBody = BotMaster.GetBody();
                    if (!BotOwnerBody) BotOwnerBody = BotOwnerMaster.GetBody();
                    if (BotBody && BotOwnerBody) reassignBodies = false;
                }
                TeleportNearOwner();
                if (BoostCount < 0) UpdateTrackerBoosts();
                if (forceRecalculateOnSpawn && BotBody)
                {
                    BotBody.RecalculateStats();
                    forceRecalculateOnSpawn = false;
                }
            }

            private float CalculateStat(float baseStat, float bonus)
            {
                return baseStat * (SetAllStatValuesAtOnce ? AllStatValueGrantedPercentage : bonus) * BoostCount;
            }

            private bool IsBlacklisted()
            {
                if (!BotMaster) return true;
                return Array.Exists(BlacklistedStockBots, element => BotMaster.gameObject.name.StartsWith(element));
            }

            private void TeleportNearOwner()
            {
                if (!NetworkServer.active || instance.IsDroneTeleportBanned(BotMaster) || !BotOwnerBody || !BotBody) return;
                if (!Util.HasEffectiveAuthority(BotBody.gameObject) || instance.GetCount(BotOwnerMaster) <= 0) return;
                if (TeleportTimer > 0)
                {
                    TeleportTimer -= Time.fixedDeltaTime;
                    return;
                }
                else
                {
                    float distance = Vector3.Distance(BotBody.corePosition, BotOwnerBody.corePosition);
                    float maxDistance, duration;
                    GraphType graphType;
                    if (BotMaster.gameObject.name.StartsWith("Turret1Master"))
                    {
                        maxDistance = TurretTeleportationDistanceAroundOwner;
                        graphType = GraphType.Ground;
                        duration = TurretTeleportationCooldownDuration;
                    }
                    else
                    {
                        maxDistance = DroneTeleportationDistanceAroundOwner;
                        graphType = GraphType.Air;
                        duration = DroneTeleportationCooldownDuration;
                    }
                    TeleportLogic(distance, maxDistance, graphType, duration);
                }
            }

            private void TeleportLogic(float distance, float maxDistance, GraphType graphType, float duration)
            {
                if (distance >= maxDistance)
                {
                    if (!TeleportBody(BotOwnerBody.corePosition, graphType)) return;
                    if (BotMaster.gameObject.name.StartsWith("Turret1Master"))
                    {
                        BotBody.transform.position += BotBody.transform.up * .9f;
                    }
                    TeleportTimer = duration;
                }
            }

            private bool TeleportBody(Vector3 desiredPosition, GraphType nodeGraphType)
            {
                SpawnCard spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
                spawnCard.hullSize = BotBody.hullClassification;
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
                    TeleportHelper.TeleportBody(BotBody, gameObject.transform.position);
                    GameObject teleportEffectPrefab = Run.instance.GetTeleportEffectPrefab(BotBody.gameObject);
                    if (teleportEffectPrefab)
                    {
                        EffectManager.SimpleEffect(teleportEffectPrefab, gameObject.transform.position, Quaternion.identity, true);
                    }
                    Destroy(gameObject);
                    Destroy(spawnCard);
                    return true;
                }
                else
                {
                    Destroy(spawnCard);
                    return false;
                }
            }
        }
    }
}