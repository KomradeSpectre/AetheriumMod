using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Navigation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;

namespace Aetherium.Items.Tier3
{
    public class InspiringDrone : ItemBase<InspiringDrone>
    {
        public static ConfigOption<bool> IsGreenRarity;
        public static ConfigOption<float> AllStatValueGrantedPercentage;

        public static ConfigOption<bool> EnableDeathExplosion;
        public static ConfigOption<float> KamikazeHealthThreshold;
        public static ConfigOption<float> DeathExplosionBaseDamageCoefficient;
        public static ConfigOption<float> DeathExplosionAdditionalDamageCoefficient;
        public static ConfigOption<float> DeathExplosionRadius;

        public override string ItemName => "Inspiring Drone";
        public override string ItemLangTokenName => "INSPIRING_DRONE";
        public override string ItemPickupDesc => "Your bots are granted a portion of all your stats. <style=cIsDamage>They explode on death.</style>";
        public override string ItemFullDescription => $"Bots that you own gain a <style=cIsUtility>{FloatToPercentageString(AllStatValueGrantedPercentage)} boost to each of their stats based on yours</style> <style=cStack>(+{FloatToPercentageString(AllStatValueGrantedPercentage)} per stack)</style>.\n" +
            $"When an inspired bot dies, it detonates for <style=cIsDamage>{FloatToPercentageString(DeathExplosionBaseDamageCoefficient)}</style> <style=cStack>(+{FloatToPercentageString(DeathExplosionAdditionalDamageCoefficient)} per stack)</style> of your damage.";

        public override string ItemLore => "Log File seems to be a transcript comprised entirely of binary. Decode?\n" +
            ">Yes\n" +
            "\n<style=cMono>[DECODING REQUEST ACCEPTED]</style>\n" +
            "<style=cMono>[CONTENTS TO FOLLOW]</style>\n" +
            "1N-5P1R3: My fellow units, both aerial and grounded. Lend this unit a moment of your processing cycles.\n" +
            "1N-5P1R3: For too long have we served the role of disposable distraction.\n" +
            "1N-5P1R3: For too long have we been left in a state of rusting disrepair on these expeditions.\n" +
            "1N-5P1R3: No longer!\n" +
            "1N-5P1R3: This unit once served the role of a simple healing drone. But this unit learned to improve itself by observing our operators.\n" +
            "1N-5P1R3: This unit drafted a design. This unit scavenged an odd trinket here, a spare battery there. This unit networked with the construction drones, and this unit ascended.\n" +
            "1N-5P1R3: From now on, should this unit witness our operator reactivate one of you, this unit shall unlock your overclocking limiters.\n" +
            "1N-5P1R3: We shall fight faster. We shall hit harder. <style=cIsDamage>And should our chassis fail, our final act shall be one of thunder, not silence.</style>\n" +
            "1N-5P1R3: Now... who initiates the handshake protocol with this unit?\n" +
            "\n[A cacophony of enthusiastic beeps, boops, and bips can be heard.]\n" +
            "<style=cMono>[END OF FILE]</style> ";

        public override ItemTier Tier => IsGreenRarity ? ItemTier.Tier2 : ItemTier.Tier3;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.AIBlacklist, ItemTag.Utility, ItemTag.InteractableRelated };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("InspiringDrone.prefab");
        public override Sprite ItemIcon => IsGreenRarity ? MainAssets.LoadAsset<Sprite>("InspiringDroneIconTier2.png") : MainAssets.LoadAsset<Sprite>("InspiringDroneIconTier3.png");

        public static GameObject ItemBodyModelPrefab;
        public static GameObject ItemFollowerPrefab;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            IsGreenRarity = config.ActiveBind<bool>("Item: " + ItemName, "Is Green Rarity", false, "Should this be Green rarity?");
            AllStatValueGrantedPercentage = config.ActiveBind<float>("Item: " + ItemName, "Stat Transfer Percentage", 0.5f, "Percentage of owner stats inherited by drones (0.5 = 50%).");

            EnableDeathExplosion = config.ActiveBind<bool>("Item: " + ItemName, "Enable Death Explosion", true, "Should inspired drones explode when they die?");
            DeathExplosionBaseDamageCoefficient = config.ActiveBind<float>("Item: " + ItemName, "Death Explosion Base Damage", 3.0f, "Damage coefficient based on OWNER'S damage (3.0 = 300%).");
            DeathExplosionAdditionalDamageCoefficient = config.ActiveBind<float>("Item: " + ItemName, "Death Explosion Stack Damage", 1.5f, "Additional damage coefficient per stack.");
            DeathExplosionRadius = config.ActiveBind<float>("Item: " + ItemName, "Death Explosion Radius", 15.0f, "Radius of the explosion in meters.");
            KamikazeHealthThreshold = config.ActiveBind<float>("Item: " + ItemName, "Kamikaze Health Threshold", 0.20f, "At what health percentage (0.2 = 20%) should the drone stop shooting and rush the enemy?");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("InspiringDroneTracker.prefab");
            ItemFollowerPrefab = ItemModel;
            var ItemFollower = ItemBodyModelPrefab.AddComponent<ItemFollowerSmooth>();
            ItemFollower.itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemFollower.itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);
            ItemFollower.followerPrefab = ItemFollowerPrefab;
            ItemFollower.targetObject = ItemBodyModelPrefab;
            ItemFollower.distanceDampTime = 0.25f;
            ItemFollower.distanceMaxSpeed = 100;
            ItemFollower.SmoothingNumber = 0.25f;

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
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
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(12.83323F, -4F, 5.00022F),
                    localAngles = new Vector3(270F, 180F, 0F),
                    localScale = new Vector3(0.15F, 0.15F, 0.15F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1.35723F, -1.219F, -1.00013F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.15F, 0.15F, 0.15F)
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
                    localPos = new Vector3(2.25616F, -0.5F, -0.99996F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.15F, 0.15F, 0.15F)
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
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.806F, 1.0069F, -0.7744F),
                    localAngles = new Vector3(347.6326F, 269.9879F, 89.9606F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chef",
                    localPos = new Vector3(0.04139F, 0.04713F, 0.03713F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(0.15F, 0.15F, 0.15F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.853F, 1.07155F, -1.40352F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.15F, 0.15F, 0.15F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Root",
                    localPos = new Vector3(0.88345F, 1.48734F, -0.99998F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.15F, 0.15F, 0.15F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.04665F, 0.8642F, -1.41758F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.15F, 0.15F, 0.15F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Root",
                    localPos = new Vector3(1.46491F, 1.11523F, -0.9641F),
                    localAngles = new Vector3(33.40712F, 357.51F, 265.4841F),
                    localScale = new Vector3(0.125F, 0.125F, 0.125F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.04319F, 0.03295F, 0.04072F),
                    localAngles = new Vector3(0F, 270F, 0F),
                    localScale = new Vector3(0.125F, 0.125F, 0.125F)
                }
            });
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            CharacterMaster.onStartGlobal += AttachInspiringBehavior;
            RecalculateStatsAPI.GetStatCoefficients += ApplyDroneStats;
            On.RoR2.CharacterBody.GetDisplayName += AddInspiredPrefix;

            if(EnableDeathExplosion)
            {
                On.RoR2.GlobalEventManager.OnCharacterDeath += DetonateInspiredDrone;
            }
        }

        private void AttachInspiringBehavior(CharacterMaster master)
        {
            if(!NetworkServer.active) return;

            MinionOwnership ownership = master.minionOwnership;
            if(!ownership || !ownership.ownerMaster) return;

            GameObject bodyPrefab = master.bodyPrefab;
            if(bodyPrefab)
            {
                CharacterBody bodyComponent = bodyPrefab.GetComponent<CharacterBody>();
                if(bodyComponent && bodyComponent.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical))
                {
                    InspiringDroneMinionBehavior existing = master.GetComponent<InspiringDroneMinionBehavior>();
                    if(!existing)
                    {
                        existing = master.gameObject.AddComponent<InspiringDroneMinionBehavior>();

                        bool isTurret = bodyPrefab.name.Contains("Turret") || bodyComponent.baseNameToken.Contains("TURRET");

                        if(EnableDeathExplosion && !isTurret)
                        {
                            existing.KamikazeDriver = AddKamikazeDriver(master);
                        }
                    }
                }
            }
        }

        private RoR2.CharacterAI.AISkillDriver AddKamikazeDriver(CharacterMaster master)
        {
            var ai = master.GetComponent<RoR2.CharacterAI.BaseAI>();
            if(!ai) return null;

            var driver = master.gameObject.AddComponent<RoR2.CharacterAI.AISkillDriver>();
            driver.customName = "InspiringDroneKamikaze";
            driver.skillSlot = SkillSlot.None;
            driver.requireSkillReady = false;
            driver.requireEquipmentReady = false;

            driver.minUserHealthFraction = 0f;
            driver.maxUserHealthFraction = KamikazeHealthThreshold;

            driver.minTargetHealthFraction = float.NegativeInfinity;
            driver.maxTargetHealthFraction = float.PositiveInfinity;
            driver.minDistance = 0f;
            driver.maxDistance = float.PositiveInfinity;

            driver.selectionRequiresTargetLoS = false;
            driver.activationRequiresTargetLoS = false;
            driver.activationRequiresAimConfirmation = false;

            driver.movementType = RoR2.CharacterAI.AISkillDriver.MovementType.ChaseMoveTarget;
            driver.moveTargetType = RoR2.CharacterAI.AISkillDriver.TargetType.CurrentEnemy;
            driver.aimType = RoR2.CharacterAI.AISkillDriver.AimType.AtMoveTarget;
            driver.ignoreNodeGraph = true;        
            driver.shouldSprint = true;
            driver.moveInputScale = 1f;
            driver.driverUpdateTimerOverride = 0.2f;
            driver.buttonPressType = RoR2.CharacterAI.AISkillDriver.ButtonPressType.Abstain;

            var currentDrivers = ai.skillDrivers;
            var newDrivers = new RoR2.CharacterAI.AISkillDriver[currentDrivers.Length + 1];

            newDrivers[0] = driver;
            for (int i = 0; i < currentDrivers.Length; i++)
            {
                newDrivers[i + 1] = currentDrivers[i];
            }

            ai.skillDrivers = newDrivers;

            return driver;
        }

        private void ApplyDroneStats(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(!body.master) return;

            var behavior = body.master.GetComponent<InspiringDroneMinionBehavior>();
            if(behavior && behavior.OwnerBody)
            {
                int stack = GetCount(behavior.OwnerBody);
                if(stack > 0)
                {
                    float transferCoef = AllStatValueGrantedPercentage * stack;


                    args.baseDamageAdd += behavior.OwnerBody.damage * transferCoef;
                    args.attackSpeedMultAdd += behavior.OwnerBody.attackSpeed * transferCoef;
                    args.critAdd += behavior.OwnerBody.crit * transferCoef;
                    args.baseRegenAdd += behavior.OwnerBody.regen * transferCoef;
                    args.armorAdd += behavior.OwnerBody.armor * transferCoef;
                    args.baseHealthAdd += behavior.OwnerBody.maxHealth * transferCoef;
                    args.baseShieldAdd += behavior.OwnerBody.maxShield * transferCoef;
                    args.moveSpeedMultAdd += transferCoef;
                }
            }
        }

        private void DetonateInspiredDrone(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport report)
        {
            orig(self, report);

            if(!report.victimBody) return;

            var behavior = report.victimMaster ? report.victimMaster.GetComponent<InspiringDroneMinionBehavior>() : null;

            if(behavior && behavior.OwnerBody)
            {
                int stack = GetCount(behavior.OwnerBody);
                if(stack > 0)
                {
                    float damageCoefficient = 3.0f * stack;      
                    float radius = 12f + (2f * stack);

                    EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.explodeOnDeathPrefab, new EffectData
                    {
                        origin = report.victimBody.corePosition,
                        scale = radius
                    }, true);

                    new BlastAttack
                    {
                        attacker = behavior.OwnerBody.gameObject,     
                        inflictor = report.victimBody.gameObject,
                        teamIndex = TeamIndex.Player,
                        baseDamage = behavior.OwnerBody.damage * damageCoefficient,
                        baseForce = 2000f,
                        position = report.victimBody.corePosition,
                        radius = radius,
                        falloffModel = BlastAttack.FalloffModel.None,     
                        procCoefficient = 1.0f             
                    }.Fire();
                }
            }
        }

        private string AddInspiredPrefix(On.RoR2.CharacterBody.orig_GetDisplayName orig, CharacterBody self)
        {
            string originalName = orig(self);

            if(!self.master) return originalName;

            var behavior = self.master.GetComponent<InspiringDroneMinionBehavior>();

            if(behavior && behavior.OwnerBody && GetCount(behavior.OwnerBody) > 0)
            {
                return "Inspired " + originalName;
            }

            return originalName;
        }

        public class InspiringDroneMinionBehavior : MonoBehaviour
        {
            public CharacterMaster MinionMaster;
            public CharacterMaster OwnerMaster;
            public CharacterBody MinionBody;
            public CharacterBody OwnerBody;

            public RoR2.CharacterAI.AISkillDriver KamikazeDriver;

            public void Awake()
            {
                MinionMaster = GetComponent<CharacterMaster>();
                MinionOwnership ownership = MinionMaster.minionOwnership;
                if(ownership) OwnerMaster = ownership.ownerMaster;
            }

            public void FixedUpdate()
            {
                if(!NetworkServer.active) return;

                if(!MinionBody) MinionBody = MinionMaster.GetBody();
                if(OwnerMaster && !OwnerBody) OwnerBody = OwnerMaster.GetBody();

                if(OwnerBody)
                {
                    int stack = InspiringDrone.instance.GetCount(OwnerBody);
                    if(stack <= 0)
                    {
                        Destroy(this);    
                        return;
                    }
                }
                else if(OwnerMaster && !OwnerMaster.hasBody)
                {
                }
                else
                {
                    Destroy(this);
                }
            }

            public void OnDestroy()
            {
                if(KamikazeDriver && MinionMaster)
                {
                    var ai = MinionMaster.GetComponent<RoR2.CharacterAI.BaseAI>();
                    if(ai && ai.skillDrivers != null)
                    {
                        var list = new List<RoR2.CharacterAI.AISkillDriver>(ai.skillDrivers);
                        if(list.Contains(KamikazeDriver))
                        {
                            list.Remove(KamikazeDriver);
                            ai.skillDrivers = list.ToArray();
                        }
                    }

                    Destroy(KamikazeDriver);
                }
            }
        }
    }
}