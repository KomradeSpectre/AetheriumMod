using Aetherium.Utils;
using KomradeSpectre.Aetherium;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TILER2;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.Navigation.MapNodeGroup;
using static TILER2.MiscUtil;
using static TILER2.StatHooks;
using Object = UnityEngine.Object;

namespace Aetherium.Items
{
    public class InspiringDrone : Item_V2<InspiringDrone>
    {
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Do you want to set all the values of the Drone's stats at once? If false, prepare for a long description. (Default: true)", AutoConfigFlags.PreventNetMismatch, false, true)]
        public bool setAllStatValuesAtOnce { get; private set; } = true;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What percentage of stats from the drone's owner do we transfer over to the drones per stack? (Default: 0.5 (50%))", AutoConfigFlags.PreventNetMismatch)]
        public float allStatValueGrantedPercentage { get; private set; } = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What percentage of the damage stat from the drone's owner do we transfer over to the drones per stack? (Default: 0.5 (50%))", AutoConfigFlags.PreventNetMismatch)]
        public float damageGrantedPercentage { get; private set; } = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What percentage of the attack speed stat from the drone's owner do we transfer over to the drones per stack? (Default: 0.5 (50%))", AutoConfigFlags.PreventNetMismatch)]
        public float attackSpeedGrantedPercentage { get; private set; } = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What percentage of the crit chance stat from the drone's owner do we transfer over to the drones per stack? (Default: 0.5 (50%))", AutoConfigFlags.PreventNetMismatch)]
        public float critChanceGrantedPercentage { get; private set; } = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What percentage of the health stat from the drone's owner do we transfer over to the drones per stack? (Default: 0.5 (50%))", AutoConfigFlags.PreventNetMismatch)]
        public float healthGrantedPercentage { get; private set; } = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What percentage of the regen stat from the drone's owner do we transfer over to the drones per stack? (Default: 0.5 (50%))", AutoConfigFlags.PreventNetMismatch)]
        public float regenGrantedPercentage { get; private set; } = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What percentage of the armor stat from the drone's owner do we transfer over to the drones per stack? (Default: 0.5 (50%))", AutoConfigFlags.PreventNetMismatch)]
        public float armorGrantedPercentage { get; private set; } = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What percentage of the movement speed stat from the drone's owner do we transfer over to the drones per stack? (Default: 0.5 (50%))", AutoConfigFlags.PreventNetMismatch)]
        public float movementSpeedGrantedPercentage { get; private set; } = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("How many seconds till we teleport turrets (tracked individually) close to their owner? (Default: 40 (40 seconds))", AutoConfigFlags.PreventNetMismatch)]
        public float turretTeleportationCooldownDuration { get; private set; } = 40f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("How many seconds till we teleport drones (tracked individually) close to their owner? (Default: 30 (30 seconds))", AutoConfigFlags.PreventNetMismatch)]
        public float droneTeleportationCooldownDuration { get; private set; } = 30f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("How many seconds between checking if we can teleport things? (Default: 10 (10 seconds))", AutoConfigFlags.PreventNetMismatch)]
        public float teleportationCheckCooldownDuration { get; private set; } = 10f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("How far out should we place turrets from the owner when teleporting them? (Default: 20 (20m))", AutoConfigFlags.PreventNetMismatch)]
        public float turretTeleportationDistanceAroundOwner { get; private set; } = 20f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("How far out should we place drones from the owner when teleporting them? (Default: 30 (30m))", AutoConfigFlags.PreventNetMismatch)]
        public float droneTeleportationDistanceAroundOwner { get; private set; } = 30f;

        public override string displayName => "Inspiring Drone";

        public override ItemTier itemTier => ItemTier.Tier3;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.AIBlacklist, ItemTag.Utility });

        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => "When a bot is purchased, it is granted a portion of all your stats, and will be brought to you after a delay if it is too far from you.";

        protected override string GetDescString(string langid = null)
        {
            string description;
            if (setAllStatValuesAtOnce)
            {
                description = $"When a bot is purchased, it gains a <style=cIsUtility>{Pct(allStatValueGrantedPercentage)} boost to each of its stats based on yours</style> <style=cStack>(+{Pct(allStatValueGrantedPercentage)} per stack, linearly)</style>.\n" +
                "Some bots <style=cIsUtility>gain more ammo</style> for their <style=cIsDamage>attacks</style> based on the <style=cIsUtility>bonus to their attack speed</style>, and have their <style=cIsUtility>ammo replenished twice as fast</style> per additional Inspiring Drone.\n" +
                $"Finally, if an inspired bot is too far away from you, it is <style=cIsUtility>teleported</style> to you after a delay <style=cStack>({turretTeleportationCooldownDuration} seconds for Turrets, {droneTeleportationCooldownDuration} seconds for Drones)</style>.";
            }
            else
            {
                description = $"When a bot is purchased, it gains the following stat boosts per stack.\n" +
                    $"Bots gain a <style=cIsDamage>{Pct(damageGrantedPercentage)} damage boost based on yours</style>.\n" +
                    $"Bots gain a <style=cIsDamage>{Pct(attackSpeedGrantedPercentage)} attack speed boost based on yours</style>.\n" +
                    $"Bots gain a <style=cIsDamage>{Pct(critChanceGrantedPercentage)} crit chance boost based on yours</style>.\n" +
                    $"Bots gain a <style=cIsHealing>{Pct(healthGrantedPercentage)} health boost based on yours</style>.\n" +
                    $"Bots gain a <style=cIsHealing>{Pct(regenGrantedPercentage)} regen boost based on yours</style>.\n" +
                    $"Bots gain a <style=cIsUtility>{Pct(armorGrantedPercentage)} armor boost based on yours</style>.\n" +
                    $"Bots gain a <style=cIsUtility>{Pct(movementSpeedGrantedPercentage)} damage boost based on yours</style>.\n" +
                    $"Some bots <style=cIsUtility>gain more ammo</style> for their <style=cIsDamage>attacks</style> based on the <style=cIsUtility>bonus to their attack speed</style>, and have their <style=cIsUtility>ammo replenished twice as fast</style> per additional Inspiring Drone.\n" +
                    $"Finally, if an inspired bot is too far away from you, it is <style=cIsUtility>teleported</style> to you after a delay <style=cStack>({turretTeleportationCooldownDuration} seconds for Turrets, {droneTeleportationCooldownDuration} seconds for Drones)</style>.";
            }
            return description;
        }

        protected override string GetLoreString(string langID = null) => "Log File seems to be a transcript comprised entirely of binary. Decode?\n" +
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

        public InspiringDrone()
        {
            modelResourcePath = "@Aetherium:Assets/Models/Prefabs/InspiringDrone.prefab";
            iconResourcePath = "@Aetherium:Assets/Textures/Icons/InspiringDroneIcon.png";
        }

        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/InspiringDroneTracker.prefab");
                ItemFollowerPrefab = Resources.Load<GameObject>(modelResourcePath);
                displayRules = GenerateItemDisplayRules();
            }
            base.SetupAttributes();
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            //ItemFollowers are for creating itemdisplays you want to lag behind or have a tether.
            //I mean that's not all they have on them, but that's the main purposes.
            //The ItemFollower component I reference here is a slightly modified version of the base one.
            //Since the base one has no virtuals on their methods, couldn't override it.

            var ItemFollower = ItemBodyModelPrefab.AddComponent<ItemFollowerSmooth>();
            ItemFollower.itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemFollower.itemDisplay.rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);
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

        public override void Install()
        {
            base.Install();
            GetStatCoefficients += AddBoostsToBot;
            On.RoR2.CharacterBody.OnInventoryChanged += RemoveItemFromDeployables;
            On.RoR2.CharacterBody.OnInventoryChanged += UpdateAllTrackers;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        public override void Uninstall()
        {
            base.Uninstall();
            GetStatCoefficients -= AddBoostsToBot;
            On.RoR2.CharacterBody.OnInventoryChanged -= RemoveItemFromDeployables;
            On.RoR2.CharacterBody.OnInventoryChanged -= UpdateAllTrackers;
            CharacterBody.onBodyStartGlobal -= CharacterBody_onBodyStartGlobal;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            CharacterMaster botMaster = obj.master;
            if (!botMaster) return;
            MinionOwnership minionOwnership = botMaster.minionOwnership;
            if (!minionOwnership) return;
            if (DronesList.Exists((droneSubstring) => { return botMaster.name.Contains(droneSubstring); }))
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
                    Chat.AddMessage($"HP After: {sender.maxHealth}");
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
                    self.inventory.RemoveItem(itemDef.itemIndex, inventoryCount);
                }
            }
        }

        private void UpdateAllTrackers(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            CharacterMaster ownerMaster = self.master;
            MinionOwnership[] minionOwnerships = Object.FindObjectsOfType<MinionOwnership>();
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

        /// <summary>
        /// Allows a custom drone to be Inspired by Inspiring Drone.
        /// </summary>
        /// <param name="masterName">The CharacterMaster name of the custom drone.</param>
        /// <returns>True if the customd rone is now supported. False if the custom drone is already supported.</returns>
        public bool AddCustomDrone(string masterName)
        {
            if (DronesList.Contains(masterName)) return false;
            DronesList.Add(masterName);
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
            public readonly List<int> BotSkillStocks = new List<int>();
            public readonly List<float> BotRechargeIntervals = new List<float>();
            public readonly List<int> DefaultSkillStocks = new List<int>();
            public readonly List<float> DefaultRechargeIntervals = new List<float>();

            private string OriginalName = "";
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
                    AetheriumPlugin._logger.LogMessage("DOESNT EXIST");
                    return;
                }
                int inventoryCount = instance.GetCount(BotOwnerBody);
                if (BoostCount != inventoryCount)
                {
                    if (OriginalName == "") OriginalName = BotBody.GetDisplayName();
                    BoostCount = inventoryCount;
                    DamageBoost = CalculateStat(BotOwnerBody.damage, instance.damageGrantedPercentage);
                    AttackSpeedBoost = CalculateStat(BotOwnerBody.attackSpeed, instance.attackSpeedGrantedPercentage);
                    CritChanceBoost = CalculateStat(BotOwnerBody.crit, instance.critChanceGrantedPercentage);
                    HealthBoost = CalculateStat(BotOwnerBody.maxHealth, instance.healthGrantedPercentage);
                    RegenBoost = CalculateStat(BotOwnerBody.regen, instance.regenGrantedPercentage);
                    ArmorBoost = CalculateStat(BotOwnerBody.armor, instance.armorGrantedPercentage);
                    MoveSpeedBoost = CalculateStat(BotOwnerBody.moveSpeed, instance.movementSpeedGrantedPercentage);
                    BotName = "";
                    if (BoostCount > 0) BotName += "Inspired ";
                    BotName += OriginalName;
                    BotBody.statsDirty = true;

                    //Add stock to bots that can use it.
                    if (!IsBlacklisted())
                    {
                        //Clear for updating.
                        Chat.AddMessage("FIRED CLEAR");
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
                            BotSkillStocks.Add((DefaultSkillStocks[i] + 1) * (int)Math.Ceiling(AttackSpeedBoost));
                            BotRechargeIntervals.Add(DefaultRechargeIntervals[i] * (float)Math.Pow(0.5, BoostCount));
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
                TeleportNearOwner();
                if (BoostCount < 0) UpdateTrackerBoosts();
            }

            private float CalculateStat(float baseStat, float bonus)
            {
                return baseStat * (instance.setAllStatValuesAtOnce ? instance.allStatValueGrantedPercentage : bonus) * BoostCount;
            }

            private bool IsBlacklisted()
            {
                if (!BotMaster) return true;
                return Array.Exists(BlacklistedStockBots, element => BotMaster.gameObject.name.StartsWith(element));
            }

            private void TeleportNearOwner()
            {
                if (!NetworkServer.active || !BotOwnerBody || !BotBody) return;
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
                        maxDistance = instance.turretTeleportationDistanceAroundOwner;
                        graphType = GraphType.Ground;
                        duration = instance.turretTeleportationCooldownDuration;
                    }
                    else
                    {
                        maxDistance = instance.droneTeleportationDistanceAroundOwner;
                        graphType = GraphType.Air;
                        duration = instance.droneTeleportationCooldownDuration;
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