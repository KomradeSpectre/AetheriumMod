using Aetherium.Achievements;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Aetherium.Utils;
using RoR2.Projectile;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Utils.MiscHelpers;

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
        public override string ItemLore => "Bury the body, take the design, and stay quiet about this. It can be our little secret.\n\nSincerely,\nJeb Labinsky";

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

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
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
            AmountOfNailsPerNailBomb = config.ActiveBind<int>("Item: " + ItemName, "Amount of Nails per Nail Bomb", 32, "How many nails should get released upon explosion of the projectile?");
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
            NailBombNailEffect.AddComponent<NetworkIdentity>();

            var effectComponent = NailBombNailEffect.GetComponent<EffectComponent>();
            effectComponent.soundName = "";

            NailBombShrapnelEffect = MainAssets.LoadAsset<GameObject>("NailBombShrapnelEffect.prefab");
            NailBombShrapnelEffect.AddComponent<NetworkIdentity>();
            var shrapnelEffectComponent = NailBombShrapnelEffect.AddComponent<EffectComponent>();
            shrapnelEffectComponent.applyScale = true;
            shrapnelEffectComponent.soundName = "Aetherium_Nailbomb_Nail_Impact";

            var particleKiller = NailBombShrapnelEffect.AddComponent<DestroyOnParticleEnd>();
            particleKiller.trackedParticleSystem = NailBombShrapnelEffect.GetComponent<ParticleSystem>();
            var shrapnelVFXComponent = NailBombShrapnelEffect.AddComponent<VFXAttributes>();
            shrapnelVFXComponent.vfxIntensity = VFXAttributes.VFXIntensity.Low;
            shrapnelVFXComponent.vfxPriority = VFXAttributes.VFXPriority.Medium;

            NailBombNailTracerEffect = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/tracers/TracerToolbotNails"), "NailBombNailTracer");
            var vfxComponent = NailBombNailTracerEffect.AddComponent<VFXAttributes>();
            vfxComponent.vfxIntensity = VFXAttributes.VFXIntensity.Low;
            vfxComponent.vfxPriority = VFXAttributes.VFXPriority.Medium;
            var smokeLine = NailBombNailTracerEffect.transform.Find("SmokeLine")?.gameObject;
            if(smokeLine) UnityEngine.Object.Destroy(smokeLine);
            NailBombNailTracerEffect.AddComponent<NetworkIdentity>();

            ContentAddition.AddEffect(NailBombNailEffect);
            ContentAddition.AddEffect(NailBombShrapnelEffect);
            ContentAddition.AddEffect(NailBombNailTracerEffect);
        }

        private void CreateProjectile()
        {
            NailBombProjectileMain = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/EngiGrenadeProjectile"), "NailBombProjectile", true);

            var networkIdentityMain = NailBombProjectileMain.GetComponent<NetworkIdentity>();
            if(!networkIdentityMain) NailBombProjectileMain.AddComponent<NetworkIdentity>();

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

            UnityEngine.Object.Destroy(NailBombProjectileMain.GetComponent<ProjectileImpactExplosion>());

            var detonator = NailBombProjectileMain.AddComponent<NailBombDetonator>();
            detonator.DetonationEffect = NailBombShrapnelEffect;
            detonator.TracerEffect = NailBombNailTracerEffect;
            detonator.HitEffect = NailBombNailEffect;
            detonator.NailCount = AmountOfNailsPerNailBomb;
            detonator.DamageCoefficient = PercentDamagePerNailInNailBomb;
            detonator.MinSpread = NailBombChildMinSpreadAngle;
            detonator.MaxSpread = NailBombChildMaxSpreadAngle;
            detonator.Direction = NailBombChildDirectionVector;
            detonator.Lifetime = 0.5f;

            PrefabAPI.RegisterNetworkPrefab(NailBombProjectileMain);
            ContentAddition.AddProjectile(NailBombProjectileMain);
        }

        public class NailBombDetonator : NetworkBehaviour
        {
            public GameObject DetonationEffect;
            public GameObject TracerEffect;
            public GameObject HitEffect;

            public int NailCount = 20;
            public float DamageCoefficient = 0.3f;
            public float MinSpread = 0;
            public float MaxSpread = 90;
            public Vector3 Direction = Vector3.down;
            public float Lifetime = 2f;

            private ProjectileController _projectileController;
            private ProjectileDamage _projectileDamage;
            private bool _hasDetonated;

            public void Start()
            {
                _projectileController = GetComponent<ProjectileController>();
                _projectileDamage = GetComponent<ProjectileDamage>();
            }

            public void FixedUpdate()
            {
                if(!NetworkServer.active) return;

                Lifetime -= Time.fixedDeltaTime;
                if(Lifetime <= 0 && !_hasDetonated)
                {
                    Detonate();
                }
            }

            private void Detonate()
            {
                _hasDetonated = true;

                if(DetonationEffect)
                {
                    EffectManager.SpawnEffect(DetonationEffect, new EffectData
                    {
                        origin = transform.position,
                        scale = 1f
                    }, true);
                }

                if(_projectileController && _projectileController.owner)
                {
                    Vector3 fireOrigin = transform.position + (Vector3.up * 0.5f);

                    BulletAttack attack = new BulletAttack
                    {
                        owner = _projectileController.owner,
                        weapon = gameObject,
                        origin = fireOrigin,                        

                        aimVector = new Vector3(0.001f, -1f, 0.001f).normalized,

                        minSpread = 0,
                        maxSpread = 45,
                        spreadPitchScale = 1f,
                        spreadYawScale = 1f,

                        bulletCount = (uint)NailCount,
                        damage = _projectileDamage.damage * DamageCoefficient,
                        force = 300f,
                        tracerEffectPrefab = TracerEffect,
                        hitEffectPrefab = HitEffect,
                        procCoefficient = 0.5f,
                        isCrit = _projectileDamage.crit,
                        radius = 1f,
                        smartCollision = true,
                        
                        maxDistance = 20f     
                    };

                    attack.Fire();
                }

                Destroy(gameObject);
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
            if(UseAlternateImplementation)
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
            orig(self);
            if(!self || !self.HasBuff(NailBombCooldownDebuff)) return;

            var inventoryCount = GetCount(self);
            if(inventoryCount > 0)
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

        private void FireNailBomb(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, GameObject victim)
        {
            if(damageInfo.rejected || damageInfo.procCoefficient <= 0 || !damageInfo.attacker)
            {
                orig(self, damageInfo, victim);
                return;
            }

            var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            if(attackerBody)
            {
                var inventoryCount = GetCount(attackerBody);
                if(inventoryCount > 0)
                {
                    var victimBody = victim.GetComponent<CharacterBody>();
                    if(victimBody && !victimBody.HasBuff(NailBombImmunityBuff))
                    {
                        if(damageInfo.damage / attackerBody.damage >= PercentDamageThresholdRequiredToActivate)
                        {
                            var positionChosen = AboveTargetVectorFromDamageInfo(damageInfo, 3);

                            ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                            {
                                projectilePrefab = NailBombProjectileMain,
                                owner = attackerBody.gameObject,
                                damage = attackerBody.damage + (attackerBody.damage * (PercentDamageBonusOfAdditionalStacks * (inventoryCount - 1))),
                                position = positionChosen ?? damageInfo.position,
                                crit = attackerBody.RollCrit()
                            });

                            victimBody.AddTimedBuff(NailBombImmunityBuff, NailBombAbsurdityLimiterCooldown);
                        }
                    }
                }
            }
            orig(self, damageInfo, victim);
        }
    }
}