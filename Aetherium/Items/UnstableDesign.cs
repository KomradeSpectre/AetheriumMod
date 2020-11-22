using Aetherium.Utils;
using KomradeSpectre.Aetherium;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TILER2;
using UnityEngine;
using UnityEngine.Networking;
using static TILER2.MiscUtil;

namespace Aetherium.Items
{
    public class UnstableDesign : Item_V2<UnstableDesign>
    {
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What should be our duration between summoning the Lunar Chimera? (Default: 30 (30 seconds))", AutoConfigFlags.PreventNetMismatch)]
        public float lunarChimeraResummonCooldownDuration { get; private set; } = 30f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("If the Lunar Chimera has lost line of sight, what should the cooldown be between checking for targets? (Default: 10 (10 seconds))", AutoConfigFlags.PreventNetMismatch)]
        public float lunarChimeraRetargetingCooldown { get; private set; } = 10f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What should the Lunar Chimera's base damage boost be? (Default: 40 (400% damage boost). This is how many damage boosting items we give it, which give it a 10% damage boost each. Whole numbers only. First stack.)", AutoConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int lunarChimeraBaseDamageBoost { get; private set; } = 40;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What should the Lunar Chimera's additional damage boost be per stack? (Default: 10 (100% damage boost). This is how many damage boosting items we give it, which give it a 10% damage boost each. Whole numbers only.)", AutoConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int lunarChimeraAdditionalDamageBoost { get; private set; } = 10;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What should the Lunar Chimera's base HP boost be? (Default: 10 (100% HP boost). This is how many HP Boost items we give it, which give it a 10% HP boost each. Whole numbers only.)", AutoConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int lunarChimeraBaseHPBoost { get; private set; } = 10;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What should the Lunar Chimera's base attack speed boost be? (Default: 30 (300% attack speed boost). This is how many attack speed boost items we give it, which give it a 10% attack speed boost each. Whole numbers only.)", AutoConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int lunarChimeraBaseAttackSpeedBoost { get; private set; } = 30;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What should the Lunar Chimera's base movement speed boost be? (Default: 2 (28% movement speed boost). This is how many goat hooves we give it, which give it a 14% movement speed boost each. Whole numbers only.)", AutoConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int lunarChimeraBaseMovementSpeedBoost { get; private set; } = 2;

        public override string displayName => "Unstable Design";

        public override ItemTier itemTier => ItemTier.Lunar;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Cleansable });
        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Every 30 seconds you are compelled to create a very <color=#FF0000>'FRIENDLY'</color> Lunar Chimera, if one of your creations does not already exist.";

        protected override string GetDescString(string langid = null) => $"Every {lunarChimeraResummonCooldownDuration} seconds you are compelled to create a very <color=#FF0000>'FRIENDLY'</color> Lunar Chimera, if one of your creations does not already exist. " + 
            $"\nIt has a <style=cIsDamage>{Pct(lunarChimeraBaseDamageBoost * 10, 0, 1)} base damage boost</style> <style=cStack>(+{Pct(lunarChimeraAdditionalDamageBoost * 10, 0, 1)} per stack)</style>." +
            $"\nIt has a <style=cIsHealing>{Pct(lunarChimeraBaseHPBoost * 10, 0, 1)} base HP boost</style> <style=cStack>(+{Pct(lunarChimeraBaseHPBoost * 10, 0, 1)} per stack)</style>." +
            $"\nIt has a <style=cIsDamage>{Pct(lunarChimeraBaseAttackSpeedBoost * 10, 0, 1)} base attack speed boost</style>." + 
            $"\nFinally, it has a <style=cIsUtility>{Pct(lunarChimeraBaseMovementSpeedBoost * 14, 0, 1)} base movement speed boost</style> <style=cStack>(+{Pct(lunarChimeraBaseMovementSpeedBoost * 14, 0, 1)} per stack)</style>." +
            "\nThis monstrosity <style=cIsDamage>can level up from kills</style>.";

        protected override string GetLoreString(string langID = null) => "We entered this predicament when one of our field testers brought back a blueprint from a whole mountain of them they found on the moon. " +
            "The blueprints seemed to have various formulas and pictures on it relating to the weird constructs we saw roaming the place. " +
            "Jimenez from Engineering got his hands on it and thought he could contribute to the rest of the team by deciphering it and creating the contents for us. " +
            "We are now waiting for Security to handle the very <color=#FF0000>'FRIENDLY'</color> construct that is making a mess of the lower sectors of the station. " +
            "Thanks Jimenez.";

        public static GameObject ItemBodyModelPrefab;
        public static SpawnCard lunarChimeraSpawnCard;
        public static GameObject lunarChimeraMasterPrefab;
        public static GameObject lunarChimeraBodyPrefab;
        public static SkillDef airSkill
        {
            get
            {
                if (!_airSkill) _airSkill = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("SprintShootShards"));
                return _airSkill;
            }
        }

        private static readonly string nameSuffix = "UnstableDesign(Aetherium)";
        private static SkillDef _airSkill = null;

        public UnstableDesign()
        {
            modelResourcePath = "@Aetherium:Assets/Models/Prefabs/UnstableDesign.prefab";
            iconResourcePath = "@Aetherium:Assets/Textures/Icons/UnstableDesignIcon.png";
        }

        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/UnstableDesignRolledUp.prefab");
                displayRules = GenerateItemDisplayRules();
            }
            base.SetupAttributes();
        }

        public override void SetupBehavior()
        {
            base.SetupBehavior();
            lunarChimeraSpawnCard = Resources.Load<SpawnCard>("SpawnCards/CharacterSpawnCards/cscLunarGolem");
            lunarChimeraSpawnCard = Object.Instantiate(lunarChimeraSpawnCard);
            lunarChimeraMasterPrefab = lunarChimeraSpawnCard.prefab;
            lunarChimeraMasterPrefab = lunarChimeraMasterPrefab.InstantiateClone($"{lunarChimeraMasterPrefab.name}{nameSuffix}");
            CharacterMaster masterPrefab = lunarChimeraMasterPrefab.GetComponent<CharacterMaster>();
            lunarChimeraBodyPrefab = masterPrefab.bodyPrefab;
            lunarChimeraBodyPrefab = lunarChimeraBodyPrefab.InstantiateClone($"{lunarChimeraBodyPrefab.name}{nameSuffix}");
            masterPrefab.bodyPrefab = lunarChimeraBodyPrefab;
            lunarChimeraSpawnCard.prefab = lunarChimeraMasterPrefab;
            MasterCatalog.getAdditionalEntries += list => list.Add(lunarChimeraMasterPrefab);
            BodyCatalog.getAdditionalEntries += list => list.Add(lunarChimeraBodyPrefab);
            NetworkingAPI.RegisterMessageType<AssignOwner>();
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
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
            return rules;
        }

        public override void Install()
        {
            base.Install();
            On.RoR2.CharacterBody.FixedUpdate += SummonLunarChimera;
            On.RoR2.MapZone.TryZoneStart += LunarChimeraFall;
        }

        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.CharacterBody.FixedUpdate -= SummonLunarChimera;
            On.RoR2.MapZone.TryZoneStart -= LunarChimeraFall;
        }

        private void SummonLunarChimera(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            int inventoryCount = GetCount(self);
            CharacterMaster master = self.master;
            if (NetworkServer.active && inventoryCount > 0 && master && !IsMinion(master)) //Check if we're a minion or not. If we are, we don't summon a chimera.
            {
                LunarChimeraComponent lcComponent = LunarChimeraComponent.GetOrCreateComponent(master);
                if (!lcComponent.LastChimeraSpawned || !lcComponent.LastChimeraSpawned.master || !lcComponent.LastChimeraSpawned.master.hasBody)
                {
                    lcComponent.LastChimeraSpawned = null;
                    lcComponent.ResummonCooldown -= Time.fixedDeltaTime;
                    if (lcComponent.ResummonCooldown <= 0f && SceneCatalog.mostRecentSceneDef != SceneCatalog.GetSceneDefFromSceneName("bazaar"))
                    {
                        DirectorPlacementRule placeRule = new DirectorPlacementRule
                        {
                            placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                            minDistance = 10f,
                            maxDistance = 40f,
                            spawnOnTarget = self.transform
                        };
                        DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(lunarChimeraSpawnCard, placeRule, RoR2Application.rng)
                        {
                            teamIndexOverride = TeamIndex.Player
                            //summonerBodyObject = self.gameObject
                        };
                        GameObject gameObject = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                        if (gameObject)
                        {
                            CharacterMaster cMaster = gameObject.GetComponent<CharacterMaster>();
                            if (cMaster)
                            {
                                //RoR2.Chat.AddMessage($"Character Master Found: {component}");
                                cMaster.teamIndex = TeamIndex.Neutral;
                                cMaster.inventory.GiveItem(ItemIndex.BoostDamage, lunarChimeraBaseDamageBoost + (lunarChimeraAdditionalDamageBoost * inventoryCount - 1));
                                cMaster.inventory.GiveItem(ItemIndex.BoostHp, lunarChimeraBaseHPBoost * inventoryCount);
                                cMaster.inventory.GiveItem(ItemIndex.BoostAttackSpeed, lunarChimeraBaseAttackSpeedBoost);
                                cMaster.inventory.GiveItem(ItemIndex.Hoof, lunarChimeraBaseMovementSpeedBoost * inventoryCount);
                                cMaster.minionOwnership.SetOwner(master);

                                CharacterBody cBody = cMaster.GetBody();
                                if (cBody)
                                {
                                    //RoR2.Chat.AddMessage($"CharacterBody Found: {component4}");
                                    cBody.teamComponent.teamIndex = TeamIndex.Neutral;
                                    cBody.gameObject.AddComponent<LunarChimeraRetargetComponent>();
                                    lcComponent.LastChimeraSpawned = cBody;
                                    DeathRewards deathRewards = cBody.GetComponent<DeathRewards>();
                                    if (deathRewards)
                                    {
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
                            lcComponent.ResummonCooldown = lunarChimeraResummonCooldownDuration;
                        }
                    }
                }
            }
            orig(self);
        }

        private void LunarChimeraFall(On.RoR2.MapZone.orig_TryZoneStart orig, MapZone self, Collider other)
        {
            if (IsUnstableDesignChimera(other.gameObject))
            {
                CharacterBody body = other.GetComponent<CharacterBody>();
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

        private bool IsMinion(CharacterMaster master)
        {
            // Replace the old minion checker so that it can support enemies that get lunar items too
            return master.minionOwnership &&
                   master.minionOwnership.ownerMaster;
        }

        private bool IsUnstableDesignChimera(GameObject obj) => obj.name.Contains(nameSuffix);

        public class LunarChimeraComponent : MonoBehaviour
        {
            public CharacterBody LastChimeraSpawned;
            public float ResummonCooldown = 0f;
            public Queue<NetworkInstanceId> syncIds = new Queue<NetworkInstanceId>();
            public NetworkInstanceId netId;
            public CharacterMaster master;

            private void Awake()
            {
                master = gameObject.GetComponent<CharacterMaster>();
                netId = gameObject.GetComponent<NetworkIdentity>().netId;
            }

            private void FixedUpdate()
            {
                if (syncIds.Count > 0)
                {
                    NetworkInstanceId syncId = syncIds.Dequeue();
                    GameObject supposedChimera = Util.FindNetworkObject(syncId);
                    if (supposedChimera)
                    {
                        LastChimeraSpawned = supposedChimera.GetComponent<CharacterBody>();
                        CharacterMaster cMaster = LastChimeraSpawned.master;
                        cMaster.minionOwnership.ownerMasterId = netId;
                        MinionOwnership.MinionGroup.SetMinionOwner(cMaster.minionOwnership, netId);
                    }
                    else
                    {
                        syncIds.Enqueue(syncId);
                    }
                }
            }

            public static LunarChimeraComponent GetOrCreateComponent(CharacterMaster master)
            {
                return GetOrCreateComponent(master.gameObject);
            }

            public static LunarChimeraComponent GetOrCreateComponent(GameObject masterObject)
            {
                return masterObject.GetComponent<LunarChimeraComponent>() ?? masterObject.AddComponent<LunarChimeraComponent>();
            }
        }

        public class LunarChimeraRetargetComponent : MonoBehaviour
        {
            // make public if you want it to be viewable in RuntimeInspector
            private float retargetTimer = 0f;
            private CharacterMaster master;
            private CharacterBody body;

            private void Awake()
            {
                body = gameObject.GetComponent<CharacterBody>();
                master = body.master;
                SetCooldown();
            }

            private void FixedUpdate()
            {
                BaseAI baseAIComponent = master.GetComponent<BaseAI>();
                if (baseAIComponent)
                {
                    SkillLocator skillComponent = gameObject.GetComponent<SkillLocator>();
                    if (skillComponent)
                    {
                        CharacterBody targetBody = baseAIComponent.currentEnemy.characterBody;
                        if (targetBody && (!targetBody.characterMotor || !targetBody.characterMotor.isGrounded))
                        {
                            skillComponent.primary.SetSkillOverride(body, airSkill, GenericSkill.SkillOverridePriority.Replacement);
                        }
                        else
                        {
                            skillComponent.primary.UnsetSkillOverride(body, airSkill, GenericSkill.SkillOverridePriority.Replacement);
                        }
                    }
                    retargetTimer -= Time.fixedDeltaTime;
                    if (retargetTimer <= 0)
                    {
                        if (!baseAIComponent.currentEnemy.hasLoS)
                        {
                            baseAIComponent.currentEnemy.Reset();
                            baseAIComponent.ForceAcquireNearestEnemyIfNoCurrentEnemy();
                            SetCooldown();
                        }
                    }
                }
            }

            private void SetCooldown(float? customCooldown = null)
            {
                if (customCooldown == null) retargetTimer = instance.lunarChimeraRetargetingCooldown;
                else retargetTimer = (float)customCooldown;
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
                GameObject owner = Util.FindNetworkObject(ownerNetId);
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
}
