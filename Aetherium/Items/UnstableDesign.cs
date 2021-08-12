using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using ItemStats;
using ItemStats.Stat;
using ItemStats.ValueFormatters;

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Compatability.ModCompatability.ArtifactOfTheKingCompat;
using static Aetherium.Compatability.ModCompatability.ItemStatsModCompat;
using System.Runtime.CompilerServices;

namespace Aetherium.Items
{
    public class UnstableDesign : ItemBase<UnstableDesign>
    {
        public static ConfigOption<float> SecondsBeforeFirstLunarChimeraSpawn;
        public static ConfigOption<float> LunarChimeraResummonCooldownDuration;
        public static ConfigOption<float> LunarChimeraRetargetingCooldown;
        public static ConfigOption<float> LunarChimeraMinimumSpawnDistanceFromUser;
        public static ConfigOption<float> LunarChimeraMaximumSpawnDistanceFromUser;
        public static ConfigOption<int> LunarChimeraBaseDamageBoost;
        public static ConfigOption<int> LunarChimeraAdditionalDamageBoost;
        public static ConfigOption<int> LunarChimeraBaseHPBoost;
        public static ConfigOption<int> LunarChimeraBaseAttackSpeedBoost;
        public static ConfigOption<int> LunarChimeraBaseMovementSpeedBoost;
        public static ConfigOption<bool> EnableTargetingIndicator;
        public static ConfigOption<bool> ShouldUnstableDesignPullAggroOnTargets;
        public static ConfigOption<bool> ReplacePrimaryAirSkillIfArtifactOfTheKingInstalled;

        public override string ItemName => "Unstable Design";

        public override string ItemLangTokenName => "UNSTABLE_DESIGN";

        public override string ItemPickupDesc => "Every 30 seconds you are compelled to create a very <color=#FF0000>'FRIENDLY'</color> Lunar Chimera, if one of your creations does not already exist.";

        public override string ItemFullDescription => $"Every {LunarChimeraResummonCooldownDuration} seconds you are compelled to create a very <color=#FF0000>'FRIENDLY'</color> Lunar Chimera, if one of your creations does not already exist. " +
            $"\nIt has a <style=cIsDamage>{FloatToPercentageString(LunarChimeraBaseDamageBoost * 10, 1)} base damage boost</style> <style=cStack>(+{FloatToPercentageString(LunarChimeraAdditionalDamageBoost * 10, 1)} per stack)</style>." +
            $"\nIt has a <style=cIsHealing>{FloatToPercentageString(LunarChimeraBaseHPBoost * 10, 1)} base HP boost</style> <style=cStack>(+{FloatToPercentageString(LunarChimeraBaseHPBoost * 10, 1)} per stack)</style>." +
            $"\nIt has a <style=cIsDamage>{FloatToPercentageString(LunarChimeraBaseAttackSpeedBoost * 10, 1)} base attack speed boost</style>." +
            $"\nFinally, it has a <style=cIsUtility>{FloatToPercentageString(LunarChimeraBaseMovementSpeedBoost * 14, 1)} base movement speed boost</style> <style=cStack>(+{FloatToPercentageString(LunarChimeraBaseMovementSpeedBoost * 14, 1)} per stack)</style>." +
            "\nThis monstrosity <style=cIsDamage>can level up from kills</style>.";

        public override string ItemLore => "We entered this predicament when one of our field testers brought back a blueprint from a whole mountain of them they found on the moon. " +
            "The blueprints seemed to have various formulas and pictures on it relating to the weird constructs we saw roaming the place. " +
            "Jimenez from Engineering got his hands on it and thought he could contribute to the rest of the team by deciphering it and creating the contents for us. " +
            "We are now waiting for Security to handle the very <color=#FF0000>'FRIENDLY'</color> construct that is making a mess of the lower sectors of the station. " +
            "Thanks Jimenez.";

        public override ItemTier Tier => ItemTier.Lunar;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Cleansable };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("UnstableDesign.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("UnstableDesignIcon.png");

        public static GameObject ItemBodyModelPrefab;
        public static RoR2.SpawnCard LunarChimeraSpawnCard;
        public static GameObject LunarChimeraMasterPrefab;
        public static GameObject LunarChimeraBodyPrefab;

        public static GameObject TargetingIndicatorSphere;
        public static GameObject TargetingIndicatorArrow;

        public static SkillDef airSkill
        {
            get
            {
                if (!_airSkill) 
                {
                    if (ReplacePrimaryAirSkillIfArtifactOfTheKingInstalled && IsArtifactOfTheKingInstalled) 
                    {
                        _airSkill = IsArtifactOfTheKingInstalled ? SkillCatalog.allSkillDefs.Where(x => x.activationState.typeName == "EntityStates.LunarExploderMonster.Weapon.FireExploderShards").First() : SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("SprintShootShards"));
                    }
                    else
                    {
                        _airSkill = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("SprintShootShards"));
                    }
                    
                } 
                return _airSkill;
            }
        }

        private static readonly string nameSuffix = "UnstableDesignAetherium";
        private static SkillDef _airSkill = null;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateTargetingPrefabs();
            CreateSpawncard();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            SecondsBeforeFirstLunarChimeraSpawn = config.ActiveBind<float>("Item: " + ItemName, "Seconds Before First Lunar Chimera Spawn", 15f, "How many seconds into a stage/life before the Lunar Chimera can spawn for the first time?");
            LunarChimeraResummonCooldownDuration = config.ActiveBind<float>("Item: " + ItemName, "Duration of Chimera Resummoning Cooldown", 30f, "What should be our duration between summoning the Lunar Chimera?");
            LunarChimeraRetargetingCooldown = config.ActiveBind<float>("Item: " + ItemName, "Duration of Chimera Retargeting Cooldown", 10f, "If the Lunar Chimera has lost line of sight, what should the cooldown be between checking for targets?");
            LunarChimeraMinimumSpawnDistanceFromUser = config.ActiveBind<float>("Item: " + ItemName, "Minimum Spawn Distance Away From User", 80f, "What is the closest distance the Lunar Chimera should spawn to the user?");
            LunarChimeraMaximumSpawnDistanceFromUser = config.ActiveBind<float>("Item: " + ItemName, "Maximum Spawn Distance Away From User", 170f, "What is the furthest distance away the Lunar Chimera should spawn away from the user?");
            LunarChimeraBaseDamageBoost = config.ActiveBind<int>("Item: " + ItemName, "Base Damage Boosting Item Amount", 40, "What should the Lunar Chimera's base damage boost be? (Default: 40 (400% damage boost). This is how many damage boosting items we give it, which give it a 10% damage boost each. Whole numbers only. First stack.)");
            LunarChimeraAdditionalDamageBoost = config.ActiveBind<int>("Item: " + ItemName, "Additional Damage Boosting Item Amount", 10, "What should the Lunar Chimera's additional damage boost be per stack? (Default: 10 (100% damage boost). This is how many damage boosting items we give it, which give it a 10% damage boost each. Whole numbers only.)");
            LunarChimeraBaseHPBoost = config.ActiveBind<int>("Item: " + ItemName, "HP Boosting Item Amount", 10, "What should the Lunar Chimera's base HP boost be? (Default: 10 (100% HP boost). This is how many HP Boost items we give it, which give it a 10% HP boost each. Whole numbers only.)");
            LunarChimeraBaseAttackSpeedBoost = config.ActiveBind<int>("Item: " + ItemName, "Attack Speed Item Amount", 30, "What should the Lunar Chimera's base attack speed boost be? (Default: 30 (300% attack speed boost). This is how many attack speed boost items we give it, which give it a 10% attack speed boost each. Whole numbers only.)");
            LunarChimeraBaseMovementSpeedBoost = config.ActiveBind<int>("Item: " + ItemName, "Movement Speed Item Amount", 2, "What should the Lunar Chimera's base movement speed boost be? (Default: 2 (28% movement speed boost). This is how many goat hooves we give it, which give it a 14% movement speed boost each. Whole numbers only.)");
            EnableTargetingIndicator = config.ActiveBind<bool>("Item: " + ItemName, "Enable Targeting Indicator?", true, "Should a targeting indicator appear over things targeted by the Unstable Design?");
            ShouldUnstableDesignPullAggroOnTargets = config.ActiveBind<bool>("Item: " + ItemName, "Should Unstable Design Pull Aggression From Its Targets?", true, "Should Unstable Design Pull Aggro? If true, anything targeted by the Unstable Design (outside of players) will start attacking it.");
            ReplacePrimaryAirSkillIfArtifactOfTheKingInstalled = config.ActiveBind<bool>("Item: " + ItemName, "Replace Unstable Design Air Skill if AOTK is Installed", false, "Should the default primary (Lunar Sprint Shards) be replaced with the Lunar Exploder's Primary (Tri-Orb Shot) if Artifact of the King is installed?");
        }

        private void CreateTargetingPrefabs()
        {
            TargetingIndicatorSphere = PrefabAPI.InstantiateClone(MainAssets.LoadAsset<GameObject>("UnstableDesignTargetingSphere"), "Unstable Design Targeting Indicator", true);
            TargetingIndicatorSphere.AddComponent<NetworkIdentity>();
            TargetingIndicatorSphere.AddComponent<UnstableDesignPinpointerDestroyer>();

            var scaleCurve1 = TargetingIndicatorSphere.AddComponent<ObjectScaleCurve>();
            scaleCurve1.useOverallCurveOnly = true;
            scaleCurve1.overallCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(0.1f, 0.4f) });

            PrefabAPI.RegisterNetworkPrefab(TargetingIndicatorSphere);

            TargetingIndicatorArrow = PrefabAPI.InstantiateClone(MainAssets.LoadAsset<GameObject>("UnstableDesignTargetingIndicator"), "Unstable Design Targeting Arrow", true);
            TargetingIndicatorArrow.AddComponent<NetworkIdentity>();
            TargetingIndicatorArrow.AddComponent<UnstableDesignPinpointerDestroyer>();

            var scaleCurve2 = TargetingIndicatorArrow.AddComponent<ObjectScaleCurve>();
            scaleCurve2.useOverallCurveOnly = true;
            scaleCurve2.overallCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(0.1f, 0.35f) });

            PrefabAPI.RegisterNetworkPrefab(TargetingIndicatorArrow);
        }

        private void CreateSpawncard()
        {
            LunarChimeraSpawnCard = Resources.Load<RoR2.SpawnCard>("SpawnCards/CharacterSpawnCards/cscLunarGolem");
            LunarChimeraSpawnCard = UnityEngine.Object.Instantiate(LunarChimeraSpawnCard);
            LunarChimeraMasterPrefab = LunarChimeraSpawnCard.prefab;
            LunarChimeraMasterPrefab = LunarChimeraMasterPrefab.InstantiateClone($"{LunarChimeraMasterPrefab.name}{nameSuffix}", true);
            LunarChimeraMasterPrefab.AddComponent<UnstableDesignPinpointComponent>();
            RoR2.CharacterMaster masterPrefab = LunarChimeraMasterPrefab.GetComponent<RoR2.CharacterMaster>();
            LunarChimeraBodyPrefab = masterPrefab.bodyPrefab;
            LunarChimeraBodyPrefab = LunarChimeraBodyPrefab.InstantiateClone($"{LunarChimeraBodyPrefab.name}{nameSuffix}", true);

            if (LunarChimeraBodyPrefab)
            {
                LanguageAPI.Add("AETHERIUM_MONSTERS_UNSTABLE_DESIGN_CHIMERA_NAME", $"The Unstable Design");

                var body = LunarChimeraBodyPrefab.GetComponent<CharacterBody>();
                body.baseNameToken = "AETHERIUM_MONSTERS_UNSTABLE_DESIGN_CHIMERA_NAME";

                var skinnedMeshRenderer = LunarChimeraBodyPrefab.GetComponentInChildren<SkinnedMeshRenderer>();

                if (skinnedMeshRenderer)
                {
                    skinnedMeshRenderer.sharedMesh = MainAssets.LoadAsset<Mesh>("Body_low_0");
                }
            }


            masterPrefab.bodyPrefab = LunarChimeraBodyPrefab;
            LunarChimeraSpawnCard.prefab = LunarChimeraMasterPrefab;
            RoR2.MasterCatalog.getAdditionalEntries += list => list.Add(LunarChimeraMasterPrefab);
            RoR2.BodyCatalog.getAdditionalEntries += list => list.Add(LunarChimeraBodyPrefab);
            NetworkingAPI.RegisterMessageType<AssignOwner>();
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("UnstableDesignRolledUp.prefab");
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0.5f, -0.2f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0, -0.07f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0.8f, -2.2f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(7, 7, 7)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0.6f, -0.2f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0.34f, -0.1f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0, -0.23f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0.9f, -0.8f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(3, 3, 3)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0.45f, -0.4f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 2, 5),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(8, 8, 8)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, -0.18f, -0.28f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.0124F, 0.3718F, -0.1209F),
                    localAngles = new Vector3(0.786F, 40.7421F, 355.3591F),
                    localScale = new Vector3(0.4842F, 0.4842F, 0.4842F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.FixedUpdate += SummonLunarChimera;
            On.RoR2.MapZone.TryZoneStart += LunarChimeraFall;
            On.RoR2.DeathRewards.OnKilledServer += RewardPlayerHalf;
            RoR2Application.onLoad += OnLoadModCompat;
        }

        private void OnLoadModCompat()
        {
            if (IsItemStatsModInstalled)
            {
                CreateUnstableDesignStatDef();
            }
        }

        private void SummonLunarChimera(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            int inventoryCount = GetCount(self);
            RoR2.CharacterMaster master = self.master;
            if (NetworkServer.active && inventoryCount > 0 && master && !IsMinion(master) && master.currentLifeStopwatch >= SecondsBeforeFirstLunarChimeraSpawn) //Check if we're a minion or not. If we are, we don't summon a chimera.
            {
                LunarChimeraComponent lcComponent = LunarChimeraComponent.GetOrCreateComponent(master);
                if (!lcComponent.LastChimeraSpawned || !lcComponent.LastChimeraSpawned.master || !lcComponent.LastChimeraSpawned.master.hasBody)
                {
                    lcComponent.LastChimeraSpawned = null;
                    lcComponent.ResummonCooldown -= Time.fixedDeltaTime;
                    if (lcComponent.ResummonCooldown <= 0f && RoR2.SceneCatalog.mostRecentSceneDef != RoR2.SceneCatalog.GetSceneDefFromSceneName("bazaar"))
                    {
                        RoR2.DirectorPlacementRule placeRule = new RoR2.DirectorPlacementRule
                        {
                            placementMode = RoR2.DirectorPlacementRule.PlacementMode.Approximate,
                            minDistance = LunarChimeraMinimumSpawnDistanceFromUser,
                            maxDistance = LunarChimeraMaximumSpawnDistanceFromUser,
                            spawnOnTarget = self.transform
                        };
                        RoR2.DirectorSpawnRequest directorSpawnRequest = new RoR2.DirectorSpawnRequest(LunarChimeraSpawnCard, placeRule, RoR2.RoR2Application.rng)
                        {
                            teamIndexOverride = TeamIndex.Player
                            //summonerBodyObject = self.gameObject
                        };
                        GameObject gameObject = RoR2.DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                        if (gameObject)
                        {
                            RoR2.CharacterMaster cMaster = gameObject.GetComponent<RoR2.CharacterMaster>();
                            if (cMaster)
                            {
                                //RoR2.Chat.AddMessage($"Character Master Found: {component}");
                                cMaster.teamIndex = TeamIndex.Neutral;
                                cMaster.inventory.GiveItem(RoR2Content.Items.BoostDamage, LunarChimeraBaseDamageBoost + (LunarChimeraAdditionalDamageBoost * inventoryCount - 1));
                                cMaster.inventory.GiveItem(RoR2Content.Items.BoostHp, LunarChimeraBaseHPBoost * inventoryCount);
                                cMaster.inventory.GiveItem(RoR2Content.Items.BoostAttackSpeed, LunarChimeraBaseAttackSpeedBoost);
                                cMaster.inventory.GiveItem(RoR2Content.Items.Hoof, LunarChimeraBaseMovementSpeedBoost * inventoryCount);
                                cMaster.minionOwnership.SetOwner(master);

                                RoR2.CharacterBody cBody = cMaster.GetBody();
                                if (cBody)
                                {
                                    //RoR2.Chat.AddMessage($"CharacterBody Found: {component4}");
                                    cBody.teamComponent.teamIndex = TeamIndex.Neutral;
                                    cBody.gameObject.AddComponent<LunarChimeraRetargetComponent>();
                                    lcComponent.LastChimeraSpawned = cBody;
                                    RoR2.DeathRewards deathRewards = cBody.GetComponent<RoR2.DeathRewards>();
                                    
                                    if (deathRewards)
                                    {
                                        deathRewards.logUnlockableDef = null;
                                        //RoR2.Chat.AddMessage($"DeathRewards Found: {component5}");
                                        deathRewards.goldReward = 0;
                                        deathRewards.expReward = 0;
                                    }

                                    NetworkIdentity bodyNet = cBody.GetComponent<NetworkIdentity>();
                                    if (bodyNet)
                                    {
                                        new AssignOwner(lcComponent.netId, bodyNet.netId).Send(NetworkDestination.Clients);
                                    }
                                }
                            }
                            lcComponent.ResummonCooldown = LunarChimeraResummonCooldownDuration;
                        }
                    }
                }
            }
            orig(self);
        }

        private void LunarChimeraFall(On.RoR2.MapZone.orig_TryZoneStart orig, RoR2.MapZone self, Collider other)
        {
            if (IsUnstableDesignChimera(other.gameObject))
            {
                RoR2.CharacterBody body = other.GetComponent<RoR2.CharacterBody>();
                if (body)
                {
                    var teamComponent = body.teamComponent;
                    teamComponent.teamIndex = TeamIndex.Player; //Set the team of it to player to avoid it dying when it falls into a hellzone.
                    orig(self, other); //Run the effect of whatever zone it is in on it. Since it is of the Player team, it obviously gets teleported back into the zone.
                    teamComponent.teamIndex = TeamIndex.Neutral; //Now make it hostile again. Thanks Obama.
                    return;
                }
            }
            orig(self, other);
        }

        private void RewardPlayerHalf(On.RoR2.DeathRewards.orig_OnKilledServer orig, RoR2.DeathRewards self, RoR2.DamageReport damageReport)
        {
            if (damageReport.attackerBody && damageReport.attackerBody.name.Contains(nameSuffix))
            {
                var ownerMaster = damageReport.attackerOwnerMaster;

                if (ownerMaster)
                {
                    var ownerBody = ownerMaster.GetBody();

                    if (ownerBody)
                    {
                        var inventoryCount = GetCount(ownerBody);
                        if (inventoryCount > 0)
                        {
                            ownerMaster.GiveExperience(self.expReward / 2);
                            ownerMaster.GiveMoney(self.goldReward / 2);
                        }
                    }
                }

                damageReport.attackerMaster.GiveExperience(self.expReward);
                damageReport.attackerMaster.GiveMoney(self.goldReward);

                self.goldReward = 0;
                self.expReward = 0;
            }
            orig(self, damageReport);
        }

        private bool IsMinion(RoR2.CharacterMaster master)
        {
            // Replace the old minion checker so that it can support enemies that get lunar items too
            return master.minionOwnership &&
                   master.minionOwnership.ownerMaster;
        }

        private bool IsUnstableDesignChimera(GameObject obj) => obj.name.Contains(nameSuffix);

        public class LunarChimeraComponent : MonoBehaviour
        {
            public RoR2.CharacterBody LastChimeraSpawned;
            public float ResummonCooldown = 0f;
            public Queue<NetworkInstanceId> syncIds = new Queue<NetworkInstanceId>();
            public NetworkInstanceId netId;
            public RoR2.CharacterMaster master;

            private void Awake()
            {
                master = gameObject.GetComponent<RoR2.CharacterMaster>();
                netId = gameObject.GetComponent<NetworkIdentity>().netId;
            }

            private void FixedUpdate()
            {
                if (syncIds.Count > 0)
                {
                    NetworkInstanceId syncId = syncIds.Dequeue();
                    GameObject supposedChimera = RoR2.Util.FindNetworkObject(syncId);
                    if (supposedChimera)
                    {
                        LastChimeraSpawned = supposedChimera.GetComponent<RoR2.CharacterBody>();
                        RoR2.CharacterMaster cMaster = LastChimeraSpawned.master;
                        cMaster.minionOwnership.ownerMasterId = netId;
                        RoR2.MinionOwnership.MinionGroup.SetMinionOwner(cMaster.minionOwnership, netId);
                    }
                    else
                    {
                        syncIds.Enqueue(syncId);
                    }
                }
            }

            public static LunarChimeraComponent GetOrCreateComponent(RoR2.CharacterMaster master)
            {
                return GetOrCreateComponent(master.gameObject);
            }

            public static LunarChimeraComponent GetOrCreateComponent(GameObject masterObject)
            {
                LunarChimeraComponent thisComponent = masterObject.GetComponent<LunarChimeraComponent>();
                if (!thisComponent) thisComponent = masterObject.AddComponent<LunarChimeraComponent>();
                return thisComponent;
            }
        }

        public class LunarChimeraRetargetComponent : MonoBehaviour
        {
            // make public if you want it to be viewable in RuntimeInspector
            private float retargetTimer = 0f;

            private RoR2.CharacterMaster master;
            private RoR2.CharacterBody body;

            public GameObject TrackerObject;
            public GameObject TrackerArrow;

            private void Awake()
            {
                body = gameObject.GetComponent<RoR2.CharacterBody>();
                if (body)
                {
                    master = body.master;
                }
                SetCooldown();
            }

            private void FixedUpdate()
            {
                if (master)
                {
                    BaseAI baseAIComponent = master.GetComponent<BaseAI>();
                    if (baseAIComponent)
                    {
                        RoR2.CharacterBody targetBody = baseAIComponent.currentEnemy.characterBody;

                        if (targetBody)
                        {
                            var pinpointerComponent = master.GetComponent<UnstableDesignPinpointComponent>();
                            if (pinpointerComponent && NetworkServer.active)
                            {
                                pinpointerComponent.Origin = targetBody.gameObject;
                            }

                            RoR2.SkillLocator skillComponent = gameObject.GetComponent<RoR2.SkillLocator>();
                            if (skillComponent)
                            {
                                if (!targetBody.characterMotor || !targetBody.characterMotor.isGrounded)
                                {
                                    skillComponent.primary.SetSkillOverride(body, airSkill, RoR2.GenericSkill.SkillOverridePriority.Replacement);

                                    if (ReplacePrimaryAirSkillIfArtifactOfTheKingInstalled && IsArtifactOfTheKingInstalled)
                                    {
                                        skillComponent.primary.maxStock = 4;
                                        skillComponent.primary.finalRechargeInterval = 1 / 4f;
                                    }
                                }
                                else
                                {
                                    skillComponent.primary.UnsetSkillOverride(body, airSkill, RoR2.GenericSkill.SkillOverridePriority.Replacement);
                                }
                            }

                            retargetTimer -= Time.fixedDeltaTime;
                            if (retargetTimer <= 0)
                            {
                                if (!baseAIComponent.currentEnemy.hasLoS)
                                {
                                    baseAIComponent.currentEnemy.Reset();
                                    baseAIComponent.ForceAcquireNearestEnemyIfNoCurrentEnemy();

                                    if (baseAIComponent.currentEnemy != null && !baseAIComponent.currentEnemy.hasLoS)
                                    {
                                        AkSoundEngine.PostEvent(3906026136, body.gameObject);
                                    }

                                    if (baseAIComponent.currentEnemy == null)
                                    {
                                        AkSoundEngine.PostEvent(4013028197, body.gameObject);
                                    }

                                    SetCooldown();
                                }
                            }

                            if (ShouldUnstableDesignPullAggroOnTargets)
                            {
                                PullAggressionFromTarget(targetBody);
                            }
                        }
                    }

                }
            }

            private void PullAggressionFromTarget(CharacterBody targetBody)
            {
                if (targetBody && targetBody.master && !targetBody.isPlayerControlled && !targetBody.isBoss)
                {
                    var targetAIComponent = targetBody.master.GetComponent<BaseAI>();
                    if (targetAIComponent)
                    {
                        if (targetAIComponent.currentEnemy == null || targetAIComponent.currentEnemy != null && targetAIComponent.currentEnemy.gameObject != body.gameObject)
                        {
                            targetAIComponent.currentEnemy.gameObject = body.gameObject;
                        }
                    }
                }
            }

            private void SetCooldown(float? customCooldown = null)
            {
                if (customCooldown == null) retargetTimer = LunarChimeraRetargetingCooldown;
                else retargetTimer = (float)customCooldown;
            }
        }

        public class UnstableDesignPinpointerDestroyer : MonoBehaviour
        {
            public CharacterBody OwnerBody;

            public void FixedUpdate()
            {
                if (!OwnerBody || OwnerBody && OwnerBody.healthComponent && !OwnerBody.healthComponent.alive)
                {
                    UnityEngine.Object.Destroy(this.gameObject);
                }
            }

        }

        public class AssignOwner : INetMessage
        {
            private NetworkInstanceId ownerNetId;
            private NetworkInstanceId minionNetId;

            public AssignOwner()
            {
            }

            public AssignOwner(NetworkInstanceId ownerNetId, NetworkInstanceId minionNetId)
            {
                this.ownerNetId = ownerNetId;
                this.minionNetId = minionNetId;
            }

            public void Deserialize(NetworkReader reader)
            {
                ownerNetId = reader.ReadNetworkId();
                minionNetId = reader.ReadNetworkId();
            }

            public void OnReceived()
            {
                if (NetworkServer.active) return;
                GameObject owner = RoR2.Util.FindNetworkObject(ownerNetId);
                if (!owner) return;

                LunarChimeraComponent lcComponent = LunarChimeraComponent.GetOrCreateComponent(owner);
                lcComponent.syncIds.Enqueue(minionNetId);
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(ownerNetId);
                writer.Write(minionNetId);
            }
        }
    }

    public class UnstableDesignPinpointComponent : NetworkBehaviour
    {
        [SyncVar]
        public GameObject Origin;

        public GameObject LunarChimeraBody => gameObject.GetComponent<CharacterMaster>()?.GetBodyObject();

        public GameObject TrackerObject;
        public GameObject TrackerArrow;

        public void FixedUpdate()
        {
            if (UnstableDesign.EnableTargetingIndicator)
            {
                ManageTrackingIndicators();
            }
        }

        private void ManageTrackingIndicators()
        {
            if (!Origin || !LunarChimeraBody) { return; }

            var targetBody = Origin.GetComponent<CharacterBody>();
            var body = LunarChimeraBody.GetComponent<CharacterBody>();

            if (targetBody && body)
            {
                if (!TrackerObject)
                {
                    TrackerObject = GameObject.Instantiate(UnstableDesign.TargetingIndicatorSphere);
                    var visualDestroyer = TrackerObject.GetComponent<UnstableDesign.UnstableDesignPinpointerDestroyer>();
                    visualDestroyer.OwnerBody = body;
                }
                if (!TrackerArrow)
                {
                    TrackerArrow = GameObject.Instantiate(UnstableDesign.TargetingIndicatorArrow);
                    var visualDestroyer = TrackerArrow.GetComponent<UnstableDesign.UnstableDesignPinpointerDestroyer>();
                    visualDestroyer.OwnerBody = body;
                }

                if (TrackerObject && TrackerArrow)
                {
                    var calculatedUpPosition = targetBody.mainHurtBox.collider.ClosestPointOnBounds(targetBody.transform.position + new Vector3(0, 10000, 0)) + (Vector3.up * 3);
                    TrackerObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    TrackerObject.transform.position = calculatedUpPosition;
                    TrackerArrow.transform.position = ClosestPointOnSphereToPoint(TrackerObject.transform.position, 0.4f, body.transform.position);
                    TrackerArrow.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

                    var lookRotation = body.transform.position - TrackerArrow.transform.position;

                    if (lookRotation != Vector3.zero)
                    {
                        TrackerArrow.transform.rotation = Quaternion.LookRotation(lookRotation);
                    }
                }
            }
            else
            {
                if (TrackerObject)
                {
                    UnityEngine.Object.Destroy(TrackerObject);
                }
                if (TrackerArrow)
                {
                    UnityEngine.Object.Destroy(TrackerArrow);
                }

            }

        }
    }
}