using KomradeSpectre.Aetherium;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using System.Collections.ObjectModel;
using TILER2;
using UnityEngine;
using UnityEngine.Networking;
using static TILER2.MiscUtil;

namespace Aetherium.Items
{
    public class UnstableDesign : Item<UnstableDesign>
    {
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("What should be our duration between summoning the Lunar Chimera? (Default: 30 (30 seconds))", AutoItemConfigFlags.PreventNetMismatch)]
        public float lunarChimeraResummonCooldownDuration { get; private set; } = 30f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("If the Lunar Chimera has lost line of sight, what should the cooldown be between checking for targets? (Default: 10 (10 seconds))", AutoItemConfigFlags.PreventNetMismatch)]
        public float lunarChimeraRetargetingCooldown { get; private set; } = 10f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("What should the Lunar Chimera's base damage boost be? (Default: 40 (400% damage boost). This is how many damage boosting items we give it, which give it a 10% damage boost each. Whole numbers only. First stack.)", AutoItemConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int lunarChimeraBaseDamageBoost { get; private set; } = 40;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("What should the Lunar Chimera's additional damage boost be per stack? (Default: 10 (100% damage boost). This is how many damage boosting items we give it, which give it a 10% damage boost each. Whole numbers only.)", AutoItemConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int lunarChimeraAdditionalDamageBoost { get; private set; } = 10;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("What should the Lunar Chimera's base HP boost be? (Default: 10 (100% HP boost). This is how many HP Boost items we give it, which give it a 10% HP boost each. Whole numbers only.)", AutoItemConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int lunarChimeraBaseHPBoost { get; private set; } = 10;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("What should the Lunar Chimera's base attack speed boost be? (Default: 30 (300% attack speed boost). This is how many attack speed boost items we give it, which give it a 10% attack speed boost each. Whole numbers only.)", AutoItemConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int lunarChimeraBaseAttackSpeedBoost { get; private set; } = 30;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("What should the Lunar Chimera's base movement speed boost be? (Default: 2 (28% movement speed boost). This is how many goat hooves we give it, which give it a 14% movement speed boost each. Whole numbers only.)", AutoItemConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int lunarChimeraBaseMovementSpeedBoost { get; private set; } = 2;

        public override string displayName => "Unstable Design";

        public override ItemTier itemTier => RoR2.ItemTier.Lunar;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Cleansable });
        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "Every 30 seconds you are compelled to create a very <color=#FF0000>'FRIENDLY'</color> Lunar Chimera, if one of your creations does not already exist.";

        protected override string NewLangDesc(string langid = null) => $"Every {lunarChimeraResummonCooldownDuration} seconds you are compelled to create a very <color=#FF0000>'FRIENDLY'</color> Lunar Chimera, if one of your creations does not already exist. " + 
            $"\nIt has a <style=cIsDamage>{Pct(lunarChimeraBaseDamageBoost * 10, 0, 1)} base damage boost</style> <style=cStack>(+{Pct(lunarChimeraAdditionalDamageBoost * 10, 0, 1)} per stack)</style>." +
            $"\nIt has a <style=cIsHealing>{Pct(lunarChimeraBaseHPBoost * 10, 0, 1)} base HP boost</style> <style=cStack>(+{Pct(lunarChimeraBaseHPBoost * 10, 0, 1)} per stack)</style>." +
            $"\nIt has a <style=cIsDamage>{Pct(lunarChimeraBaseAttackSpeedBoost * 10, 0, 1)} base attack speed boost</style>." + 
            $"\nFinally, it has a <style=cIsUtility>{Pct(lunarChimeraBaseMovementSpeedBoost * 14, 0, 1)} base movement speed boost</style> <style=cStack>(+{Pct(lunarChimeraBaseMovementSpeedBoost * 14, 0, 1)} per stack)</style>." +
            "\nThis monstrosity <style=cIsDamage>can level up from kills</style>.";

        protected override string NewLangLore(string langID = null) => "We entered this predicament when one of our field testers brought back a blueprint from a whole mountain of them they found on the moon. " +
            "The blueprints seemed to have various formulas and pictures on it relating to the weird constructs we saw roaming the place. " +
            "Jimenez from Engineering got his hands on it and thought he could contribute to the rest of the team by deciphering it and creating the contents for us. " +
            "We are now waiting for Security to handle the very <color=#FF0000>'FRIENDLY'</color> construct that is making a mess of the lower sectors of the station. " +
            "Thanks Jimenez.";

        public static GameObject ItemBodyModelPrefab;

        public static SkillDef airSkill;

        public UnstableDesign()
        {
            modelPathName = "@Aetherium:Assets/Models/Prefabs/UnstableDesign.prefab";
            iconPathName = "@Aetherium:Assets/Textures/Icons/UnstableDesignIcon.png";
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

        protected override void LoadBehavior()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/UnstableDesignRolledUp.prefab");
                regItem.ItemDisplayRules = GenerateItemDisplayRules();
            }

            On.RoR2.CharacterBody.FixedUpdate += SummonLunarChimera;
            On.RoR2.CharacterBody.FixedUpdate += LunarChimeraRetarget;
            On.RoR2.MapZone.TryZoneStart += LunarChimeraFall;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.CharacterBody.FixedUpdate -= SummonLunarChimera;
            On.RoR2.CharacterBody.FixedUpdate -= LunarChimeraRetarget;
            On.RoR2.MapZone.TryZoneStart -= LunarChimeraFall;
        }

        private void SummonLunarChimera(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (NetworkServer.active && self.master)
            {
                var LunarChimeraComponent = self.master.GetComponent<LunarChimeraComponent>();
                if (!LunarChimeraComponent) { LunarChimeraComponent = self.masterObject.AddComponent<LunarChimeraComponent>(); }

                var SummonerBodyMaster = self.master;
                if (SummonerBodyMaster) //Check if we're a minion or not. If we are, we don't summon a chimera.
                {
                    if (SummonerBodyMaster.teamIndex == TeamIndex.Player && !self.isPlayerControlled)
                    {
                        orig(self);
                        return;
                    }
                }

                int InventoryCount = GetCount(self);
                if (InventoryCount > 0)
                {
                    if (LunarChimeraComponent.LastChimeraSpawned == null || !LunarChimeraComponent.LastChimeraSpawned.master || !LunarChimeraComponent.LastChimeraSpawned.master.hasBody)
                    {
                        LunarChimeraComponent.LastChimeraSpawned = null;
                        LunarChimeraComponent.ResummonCooldown -= Time.fixedDeltaTime;
                        if (LunarChimeraComponent.ResummonCooldown <= 0f && RoR2.SceneCatalog.mostRecentSceneDef != RoR2.SceneCatalog.GetSceneDefFromSceneName("bazaar"))
                        {

                            RoR2.DirectorSpawnRequest directorSpawnRequest = new RoR2.DirectorSpawnRequest((RoR2.SpawnCard)Resources.Load("SpawnCards/CharacterSpawnCards/cscLunarGolem"), new RoR2.DirectorPlacementRule
                            {
                                placementMode = RoR2.DirectorPlacementRule.PlacementMode.Approximate,
                                minDistance = 10f,
                                maxDistance = 40f,
                                spawnOnTarget = self.transform
                            }, RoR2.RoR2Application.rng);
                            //directorSpawnRequest.summonerBodyObject = self.gameObject;
                            directorSpawnRequest.teamIndexOverride = TeamIndex.Player;
                            GameObject gameObject = RoR2.DirectorCore.instance.TrySpawnObject(directorSpawnRequest);

                            if (gameObject)
                            {
                                RoR2.CharacterMaster component = gameObject.GetComponent<RoR2.CharacterMaster>();
                                AIOwnership component2 = gameObject.GetComponent<AIOwnership>();
                                BaseAI component3 = gameObject.GetComponent<BaseAI>();

                                if (component)
                                {
                                    //RoR2.Chat.AddMessage($"Character Master Found: {component}");
                                    component.GetBody().teamComponent.teamIndex = TeamIndex.Neutral;
                                    component.teamIndex = TeamIndex.Neutral;

                                    component.inventory.GiveItem(ItemIndex.BoostDamage, lunarChimeraBaseDamageBoost + (lunarChimeraAdditionalDamageBoost * InventoryCount-1));
                                    component.inventory.GiveItem(ItemIndex.BoostHp, lunarChimeraBaseHPBoost * InventoryCount);
                                    component.inventory.GiveItem(ItemIndex.BoostAttackSpeed, lunarChimeraBaseAttackSpeedBoost);
                                    component.inventory.GiveItem(ItemIndex.Hoof, lunarChimeraBaseMovementSpeedBoost * InventoryCount);

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
                            if (gameObject) {LunarChimeraComponent.ResummonCooldown = lunarChimeraResummonCooldownDuration;}
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
                    if (!airSkill) 
                    {
                        airSkill = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("SprintShootShards"));
                    }

                    var skillComponent = self.GetComponent<SkillLocator>();
                    if (skillComponent)
                    {
                        var body = baseAIComponent.currentEnemy.characterBody;
                        if (body && (!body.characterMotor || !body.characterMotor.isGrounded))
                        {
                            skillComponent.primary.SetSkillOverride(self, airSkill, GenericSkill.SkillOverridePriority.Replacement);
                        }
                        else
                        {
                            skillComponent.primary.UnsetSkillOverride(self, airSkill, GenericSkill.SkillOverridePriority.Replacement);
                        }
                    }
                    retargetComponent.RetargetTimer -= Time.fixedDeltaTime;
                    if (retargetComponent.RetargetTimer <= 0)
                    {
                        if (!baseAIComponent.currentEnemy.hasLoS)
                        {
                            baseAIComponent.currentEnemy.Reset();
                            baseAIComponent.ForceAcquireNearestEnemyIfNoCurrentEnemy();
                            retargetComponent.RetargetTimer = lunarChimeraRetargetingCooldown;
                        }
                    }
                }
            }
            orig(self);
        }

        private void LunarChimeraFall(On.RoR2.MapZone.orig_TryZoneStart orig, RoR2.MapZone self, Collider other)
        {
            var body = other.GetComponent<CharacterBody>();
            if (body)
            {
                var LunarChimeraRetargeter = body.GetComponent<LunarChimeraRetargetComponent>();
                if (LunarChimeraRetargeter)
                {
                    var teamComponent = body.teamComponent;
                    teamComponent.teamIndex = TeamIndex.Player;  //Set the team of it to player to avoid it dying when it falls into a hellzone.
                    orig(self, other);                           //Run the effect of whatever zone it is in on it. Since it is of the Player team, it obviously gets teleported back into the zone.
                    teamComponent.teamIndex = TeamIndex.Neutral; //Now make it hostile again. Thanks Obama.
                    return;
                }
            }
            orig(self, other);
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
