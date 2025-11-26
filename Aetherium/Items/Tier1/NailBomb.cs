using Aetherium.Achievements;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Aetherium.Utils;

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.MathHelpers;

using RoR2.Projectile;
using UnityEngine.Networking;
using static Aetherium.Utils.MiscHelpers;
using System.Runtime.CompilerServices;
using Aetherium.Utils.Components;
using System.Linq;

namespace Aetherium.Items.Tier1
{
    public class NailBomb : ItemBase<NailBomb>
    {
        public static ConfigOption<bool> UseAlternateImplementation;

        public static ConfigOption<Vector3> NailBombChildDirectionVector;
        public static ConfigOption<float> NailBombChildMinSpreadAngle;
        public static ConfigOption<float> NailBombChildMaxSpreadAngle;

        public static ConfigOption<float> NailBombAbsurdityLimiterCooldown;
        public static ConfigOption<float> PercentDamageThresholdRequiredToActivate;
        public static ConfigOption<int> AmountOfNailsPerNailBomb;
        public static ConfigOption<float> PercentDamagePerNailInNailBomb;
        public static ConfigOption<float> PercentDamageBonusOfAdditionalStacks;
        public static ConfigOption<float> NailBombDropDelay;
        public static ConfigOption<float> DurationPercentageReducedByWithAdditionalStacks;


        public override string ItemName => "Nail Bomb";

        public override string ItemLangTokenName => "NAIL_BOMB";

        public override string ItemPickupDesc => UseAlternateImplementation ? $"Occasionally drop a shrapnel grenade from your position that explodes after a delay." : $"Attacks that deal <style=cIsDamage>high damage</style> release a shrapnel grenade that explodes after a delay.";

        public override string ItemFullDescription => UseAlternateImplementation ? $"After <style=cIsDamage>{NailBombDropDelay}</style> second(s) <style=cStack>(-{FloatToPercentageString(DurationPercentageReducedByWithAdditionalStacks)} per stack)</style> you will drop <style=cIsDamage>a shrapnel grenade from your current position</style> that explodes for <style=cIsDamage>{AmountOfNailsPerNailBomb}x{FloatToPercentageString(PercentDamagePerNailInNailBomb)} of your damage</style> <style=cStack>(+{FloatToPercentageString(PercentDamageBonusOfAdditionalStacks)} more per stack)</style>. The shrapnel has a high chance to trigger <style=cIsDamage>On-Hit</style> effects." : $"Attacks that deal <style=cIsDamage>{FloatToPercentageString(PercentDamageThresholdRequiredToActivate)} damage or more</style> release a <style=cIsDamage>shrapnel grenade</style> that explodes for <style=cIsDamage>{AmountOfNailsPerNailBomb}x{FloatToPercentageString(PercentDamagePerNailInNailBomb)} of your damage</style> <style=cStack>(+{FloatToPercentageString(PercentDamageBonusOfAdditionalStacks)} more per stack)</style>. Enemies hit that launched a nail bomb are granted <style=cIsUtility>{NailBombAbsurdityLimiterCooldown} second(s) of immunity</style> to the effect.";

        public override string ItemLore => "[Attached to this box is a strange note covered in letters cut from various sources.]\n\n" +
            "Hello there!\n\n" +

            "If you're reading this, then the mail service has done their job in sending this parcel to the right person. I just want you to know the following: Screw you! " +
            "Not only did you steal my job, you took almost all my possessions from me before fleeing to some deep sector of space and now I'm giving you what you forgot to take!\n\n" +

            "That's right, open up the package! See that? You probably did shortly before it went off, but now I imagine you're not reading this anymore if my device worked. If you're not the person I sent this to, and " +
            "you're only finding the note next to some poor schmuck covered in nails, they got what was coming to them. I've attached the blueprints on how I built this thing in a secret compartment inside the bottom " +
            "of the box.\n\n" +

            "Bury the body, take the design, and stay quiet about this. It can be our little secret.\n\n" +
            
            "Sincerely,\n" +
            "Jeb Labinsky";

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public override ItemTier Tier => UseAlternateImplementation ? ItemTier.Tier2 : ItemTier.Tier1;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("PickupNailBomb.prefab");

        public override Sprite ItemIcon => UseAlternateImplementation ? MainAssets.LoadAsset<Sprite>("NailBombIconTier2.png") : MainAssets.LoadAsset<Sprite>("NailBombIconTier1.png");

        public static GameObject ItemBodyModelPrefab;

        public static GameObject NailBombProjectileMain;

        public static GameObject NailBombNailEffect;
        public static GameObject NailBombNailTracerEffect;
        public static GameObject NailBombShrapnelEffect;

        public static BuffDef NailBombCooldownDebuff;
        public static BuffDef NailBombImmunityBuff;

        public static NetworkSoundEventDef NailBombTracerSound;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            //CreateAchievement();
            CreateLang();
            CreateBuff();
            CreateEffect();
            CreateProjectile();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            UseAlternateImplementation = config.ActiveBind<bool>("Item: " + ItemName, "Use Alternate Item Implementation?", false, "If true, Nail Bomb drops from your position after a delay.");

            NailBombChildDirectionVector = config.ActiveBind<Vector3>("Item: " + ItemName, "Nail Bomb Child Direction Vector", Vector3.down, "What world relative vector should we fire the Nail Bomb's child projectiles?");
            NailBombChildMinSpreadAngle = config.ActiveBind<float>("Item: " + ItemName, "Nail Bomb Child Min Spread Angle", 0, "What should be the most minimal spread angle of the Nail Bomb's child projectiles?");
            NailBombChildMaxSpreadAngle = config.ActiveBind<float>("Item: " + ItemName, "Nail Bomb Child Max Spread Angle", 45, "What should be the most maximal spread angle of the Nail Bomb's child projectiles?");

            PercentDamageThresholdRequiredToActivate = config.ActiveBind<float>("Item: " + ItemName, "Percent Damage Threshold Required to Activate Effect", 1.2f, "What percentage of damage should we deal in a single hit to activate the effect of this item?");
            AmountOfNailsPerNailBomb = config.ActiveBind<int>("Item: " + ItemName, "Amount of Nails per Nail Bomb", 20, "How many nails should get released upon explosion of the projectile?");
            PercentDamagePerNailInNailBomb = config.ActiveBind<float>("Item: " + ItemName, "Percent Damage per Nail in Nail Bomb", 0.3f, "What percentage of damage should each nail in the nail bomb deal?");
            PercentDamageBonusOfAdditionalStacks = config.ActiveBind<float>("Item: " + ItemName, "Percent Damage Bonus of Additional Stacks", 0.5f, "What additional percentage of the body's damage should be given per additional stacks of Nail Bomb?");
            NailBombAbsurdityLimiterCooldown = config.ActiveBind<float>("Item: " + ItemName, "Cooldown for Nail Bomb Absurdity Limiter", 2, "What should be the immunity duration to the effect of Nail Bomb implementation 1 for enemies? (if 0, you will regret its absurdity)");
            NailBombDropDelay = config.ActiveBind<float>("Item: " + ItemName, "Delay Between Nail Bomb Drops in Alternate Implementation", 10, "How many seconds should we wait between Nail Bomb drops for the first stack?");
            DurationPercentageReducedByWithAdditionalStacks = config.ActiveBind<float>("Item: " + ItemName, "Duration Percentage is Reduced By With Additional Stacks", 0.2f, "What percentage should we reduce the cooldown duration of Nail Bomb Alternate Implementation? (hyperbolically).");
        }

        private void CreateBuff()
        {
            NailBombCooldownDebuff = ScriptableObject.CreateInstance<BuffDef>();
            NailBombCooldownDebuff.name = "Aetherium: Nail Bomb Cooldown Debuff";
            NailBombCooldownDebuff.buffColor = new Color(255, 255, 255);
            NailBombCooldownDebuff.canStack = false;
            NailBombCooldownDebuff.isDebuff = true;
            NailBombCooldownDebuff.iconSprite = MainAssets.LoadAsset<Sprite>("NailBombNailCooldownIcon.png");

            ContentAddition.AddBuffDef(NailBombCooldownDebuff);

            NailBombImmunityBuff = ScriptableObject.CreateInstance<BuffDef>();
            NailBombImmunityBuff.name = "Aetherium: Nail Bomb Immunity";
            NailBombImmunityBuff.buffColor = new Color(255, 255, 255);
            NailBombImmunityBuff.canStack = false;
            NailBombImmunityBuff.isDebuff = false;
            NailBombImmunityBuff.iconSprite = MainAssets.LoadAsset<Sprite>("NailBombNailCooldownIcon.png");

            ContentAddition.AddBuffDef(NailBombImmunityBuff);
        }


        public void CreateEffect()
        {
            NailBombNailEffect = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/effects/impacteffects/ImpactNailgun"), "NailBombNailImpact");

            NailBombTracerSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            NailBombTracerSound.eventName = "Aetherium_Nailbomb_Nail_Impact";
            R2API.ContentAddition.AddNetworkSoundEventDef(NailBombTracerSound);

            var normalEffectComponent = NailBombNailEffect.GetComponent<EffectComponent>();
            normalEffectComponent.soundName = "Aetherium_Nailbomb_Nail_Impact";

            NailBombNailEffect.AddComponent<NetworkIdentity>();

            NailBombShrapnelEffect = MainAssets.LoadAsset<GameObject>("NailBombShrapnelEffect.prefab");
            NailBombShrapnelEffect.AddComponent<NetworkIdentity>();

            var shrapnelEffectComponent = NailBombShrapnelEffect.AddComponent<EffectComponent>();
            shrapnelEffectComponent.applyScale = true;

            var particleKiller = NailBombShrapnelEffect.AddComponent<DestroyOnParticleEnd>();
            particleKiller.trackedParticleSystem = NailBombShrapnelEffect.GetComponent<ParticleSystem>();

            var shrapnelVFXComponent = NailBombShrapnelEffect.AddComponent<VFXAttributes>();
            shrapnelVFXComponent.vfxIntensity = VFXAttributes.VFXIntensity.Low;
            shrapnelVFXComponent.vfxPriority = VFXAttributes.VFXPriority.Medium;

            NailBombNailTracerEffect = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/tracers/TracerToolbotNails"), "NailBombNailTracer");

            var vfxComponent = NailBombNailTracerEffect.AddComponent<VFXAttributes>();
            vfxComponent.vfxIntensity = VFXAttributes.VFXIntensity.Low;
            vfxComponent.vfxPriority = VFXAttributes.VFXPriority.Medium;

            var smokeLine = NailBombNailTracerEffect.transform.Find("SmokeLine").gameObject;
            if (smokeLine) { UnityEngine.Object.Destroy(smokeLine); }

            NailBombNailTracerEffect.AddComponent<NetworkIdentity>();

            if (NailBombNailEffect) { PrefabAPI.RegisterNetworkPrefab(NailBombNailEffect); }
            ContentAddition.AddEffect(NailBombNailEffect);

            if (NailBombShrapnelEffect) { PrefabAPI.RegisterNetworkPrefab(NailBombShrapnelEffect); }
            ContentAddition.AddEffect(NailBombShrapnelEffect);

            if (NailBombNailTracerEffect) { PrefabAPI.RegisterNetworkPrefab(NailBombNailTracerEffect); }
            ContentAddition.AddEffect(NailBombNailTracerEffect);
        }

        private void CreateProjectile()
        {
            NailBombProjectileMain = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/EngiGrenadeProjectile"), "NailBombProjectile", true);

            var networkIdentityMain = NailBombProjectileMain.GetComponent<NetworkIdentity>();
            if (!networkIdentityMain) { NailBombProjectileMain.AddComponent<NetworkIdentity>(); }

            var model = MainAssets.LoadAsset<GameObject>("NailBombProjectile.prefab");
            model.AddComponent<ProjectileGhostController>();
            model.AddComponent<NetworkIdentity>();

            var scaleCurve = model.AddComponent<ObjectScaleCurve>();
            scaleCurve.useOverallCurveOnly = true;
            scaleCurve.overallCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.05f, 1));

            var projectileController = NailBombProjectileMain.GetComponent<ProjectileController>();
            projectileController.ghostPrefab = model;

            var velocityRandom = NailBombProjectileMain.AddComponent<VelocityRandomOnStart>();
            velocityRandom.coneAngle = 30;
            velocityRandom.directionMode = VelocityRandomOnStart.DirectionMode.Cone;
            velocityRandom.baseDirection = Vector3.up;
            velocityRandom.minSpeed = 15;
            velocityRandom.maxSpeed = 20;

            var velocityDetonate = NailBombProjectileMain.AddComponent<ProjectileVelocityDetonate>();
            velocityDetonate.DetonationEffect = NailBombShrapnelEffect;

            /*var flicker = model.AddComponent<FlickerHGStandardEmission>();
            flicker.renderers = new Renderer[]
            {
                model.transform.Find("_mdlNailBomb/Display").GetComponent<Renderer>()
            };
            flicker.StartIntensity = 6;
            flicker.Interval = 0.01f;*/


            UnityEngine.Object.Destroy(NailBombProjectileMain.GetComponent<ProjectileImpactExplosion>());

            var impactExplosion = NailBombProjectileMain.AddComponent<ProjectileFixedImpactExplosion>();
            impactExplosion.ChildBulletAttack = true;
            impactExplosion.childTracerPrefab = NailBombNailTracerEffect;
            impactExplosion.childHitEffectPrefab = NailBombNailEffect;
            impactExplosion.childrenCount = AmountOfNailsPerNailBomb;
            //impactExplosion.explosionEffect = Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFX.prefab");
            impactExplosion.blastRadius = 2;
            impactExplosion.childrenDamageCoefficient = PercentDamagePerNailInNailBomb;
            impactExplosion.fireChildren = true;
            impactExplosion.MinDeviationAngle = NailBombChildMinSpreadAngle;
            impactExplosion.MaxDeviationAngle = NailBombChildMaxSpreadAngle;
            impactExplosion.Direction = NailBombChildDirectionVector;
            impactExplosion.transformSpace = ProjectileFixedImpactExplosion.TransformSpace.World;
            impactExplosion.destroyOnEnemy = false;
            impactExplosion.destroyOnWorld = false;
            impactExplosion.lifetime = 2;
            impactExplosion.lifetimeAfterImpact = 0.2f;

            PrefabAPI.RegisterNetworkPrefab(NailBombProjectileMain);
            ContentAddition.AddProjectile(NailBombProjectileMain);

        }

        private void CreateAchievement()
        {
            if (RequireUnlock)
            {
                var achievement = new NailBombAchievement();
                achievement.Init();
                if (achievement.UnlockableDef)
                {
                    ItemUnlockableDef = achievement.UnlockableDef;
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("DisplayNailBomb.prefab");
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.17296F, 0.20893F),
                    localAngles = new Vector3(80.00002F, 180F, 180F),
                    localScale = new Vector3(0.08412F, 0.06451F, 0.06451F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.14163F, -0.08349F, -0.04923F),
                    localAngles = new Vector3(276.0963F, 326.358F, 115.3274F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.07755F, 0.09307F, 0.83626F),
                    localAngles = new Vector3(345.215F, 91.4967F, 95.18412F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.19213F, 0.09219F, 0.14767F),
                    localAngles = new Vector3(289.9124F, 184.1818F, 327.7321F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.17761F, -0.00051F, 0.01399F),
                    localAngles = new Vector3(304.7539F, 286.6039F, 164.8734F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.20272F, 0.04168F, -0.03243F),
                    localAngles = new Vector3(280.6105F, 73.61681F, 189.5143F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(-0.66656F, -0.57055F, -0.05392F),
                    localAngles = new Vector3(85.20335F, 269.2286F, 7.29045F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.23379F, 0.04902F, 0.01696F),
                    localAngles = new Vector3(312.1915F, 295.248F, 152.045F),
                    localScale = new Vector3(0.06197F, 0.06197F, 0.06197F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hip",
                    localPos = new Vector3(-2.2536F, 1.10779F, 0.45293F),
                    localAngles = new Vector3(295.8574F, 206.614F, 251.7372F),
                    localScale = new Vector3(0.62931F, 0.62931F, 0.62931F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.2187F, -0.12313F, -0.09153F),
                    localAngles = new Vector3(273.8412F, 23.54453F, 36.83049F),
                    localScale = new Vector3(0.07509F, 0.07509F, 0.07509F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.17676F, -0.03541F, -0.11162F),
                    localAngles = new Vector3(283.1234F, 65.28964F, 241.054F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Body",
                    localPos = new Vector3(0F, 0.00988F, 0.00401F),
                    localAngles = new Vector3(85.90677F, 0F, 0F),
                    localScale = new Vector3(0.00424F, 0.00424F, 0.00424F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.27217F, 0.15341F, -0.02928F),
                    localAngles = new Vector3(81.40253F, 74.9142F, 327.2595F),
                    localScale = new Vector3(0.09084F, 0.09084F, 0.09084F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.18981F, 0.12625F),
                    localAngles = new Vector3(57.61138F, 0F, 0F),
                    localScale = new Vector3(0.04767F, 0.04767F, 0.04767F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.13045F, -0.07622F, 0.05581F),
                    localAngles = new Vector3(75.11233F, 93.31087F, 26.67048F),
                    localScale = new Vector3(0.05383F, 0.05383F, 0.05383F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Shield",
                    localPos = new Vector3(0.46832F, -0.53825F, 0.44098F),
                    localAngles = new Vector3(54.47169F, 20.14517F, 255.0181F),
                    localScale = new Vector3(0.21227F, 0.21227F, 0.21227F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00631F, 0.00766F, 0.00022F),
                    localAngles = new Vector3(73.33791F, 90F, 180F),
                    localScale = new Vector3(0.00278F, 0.00278F, 0.00278F)
                }
            });
            return rules;
        }


        public override void Hooks()
        {
            if (UseAlternateImplementation)
            {
                On.RoR2.CharacterBody.FixedUpdate += FireNailBombFromBody;
            }
            else
            {
                On.RoR2.GlobalEventManager.OnHitEnemy += FireNailBomb;
            }
        }

        private void FireNailBombFromBody(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            var inventoryCount = GetCount(self);
            if(inventoryCount > 0 && self)
            {
                if (!self.HasBuff(NailBombCooldownDebuff))
                {
                    var chosenPosition = AboveTargetBody(self, 3);
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo()
                    {
                        projectilePrefab = NailBombProjectileMain,
                        owner = self.gameObject,
                        damage = self.damage + (self.damage * (PercentDamageBonusOfAdditionalStacks * (inventoryCount - 1))),
                        position = chosenPosition.Value,
                        damageTypeOverride = null,
                        damageColorIndex = DamageColorIndex.Default,
                        procChainMask = default
                    };

                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);

                    self.AddTimedBuff(NailBombCooldownDebuff, NailBombDropDelay / (1 + DurationPercentageReducedByWithAdditionalStacks * (inventoryCount - 1)));
                }
            }

            orig(self);
        }

        private void FireNailBomb(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.rejected || damageInfo.procCoefficient <= 0)
            {
                orig(self, damageInfo, victim);
                return;
            }

            var attacker = damageInfo.attacker;
            if (attacker)
            {
                var body = attacker.GetComponent<CharacterBody>();
                var victimBody = victim.GetComponent<CharacterBody>();
                if (body && victimBody)
                {
                    if(victimBody.HasBuff(NailBombImmunityBuff))
                    {
                        orig(self, damageInfo, victim);
                        return;
                    }

                    var InventoryCount = GetCount(body);
                    if (InventoryCount > 0)
                    {
                        if (damageInfo.damage / body.damage >= PercentDamageThresholdRequiredToActivate )
                        {
                            var positionChosen = AboveTargetVectorFromDamageInfo(damageInfo, 3);

                            FireProjectileInfo newProjectileLaunch = new FireProjectileInfo()
                            {
                                projectilePrefab = NailBombProjectileMain,
                                owner = body.gameObject,
                                damage = body.damage + (body.damage * (PercentDamageBonusOfAdditionalStacks * (InventoryCount - 1))),
                                position = positionChosen.HasValue ? positionChosen.Value : damageInfo.position,
                                damageTypeOverride = null,
                                damageColorIndex = DamageColorIndex.Default,
                                procChainMask = default,
                            };

                            ProjectileManager.instance.FireProjectile(newProjectileLaunch);

                            victimBody.AddTimedBuff(NailBombImmunityBuff, NailBombAbsurdityLimiterCooldown);
                        }
                    }
                }
            }
            orig(self, damageInfo, victim);
        }
    }
}
