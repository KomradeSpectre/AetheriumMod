using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Aetherium.Utils;
using ItemStats;
using ItemStats.Stat;
using ItemStats.ValueFormatters;

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Compatability.ModCompatability.BetterUICompat;
using static Aetherium.Compatability.ModCompatability.ItemStatsModCompat;

using RoR2.Projectile;
using UnityEngine.Networking;
using static Aetherium.Utils.MiscUtils;
using System.Runtime.CompilerServices;
using Aetherium.Utils.Components;

namespace Aetherium.Items
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

        public override string ItemFullDescription => UseAlternateImplementation ? $"After {NailBombDropDelay} second(s) <style=cStack>(-{FloatToPercentageString(DurationPercentageReducedByWithAdditionalStacks)} per stack)</style> you will drop a shrapnel grenade from your current position that explodes for {AmountOfNailsPerNailBomb}x{FloatToPercentageString(PercentDamagePerNailInNailBomb)} of your damage <style=cStack>(+{FloatToPercentageString(PercentDamageBonusOfAdditionalStacks)} more per stack). The shrapnel has a high chance to trigger On-Hit effects." : $"Attacks that deal {FloatToPercentageString(PercentDamageThresholdRequiredToActivate)} damage or more release a shrapnel grenade that explodes for {AmountOfNailsPerNailBomb}x{FloatToPercentageString(PercentDamagePerNailInNailBomb)} of your damage <style=cStack>(+{FloatToPercentageString(PercentDamageBonusOfAdditionalStacks)} more per stack)</style>. Enemies hit that launched a nail bomb are granted {NailBombAbsurdityLimiterCooldown} second(s) of immunity to the effect.";

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

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("NailBomb.prefab");

        public override Sprite ItemIcon => UseAlternateImplementation ? MainAssets.LoadAsset<Sprite>("NailBombTier2.png") : MainAssets.LoadAsset<Sprite>("NailBombTier1.png");

        public static GameObject ItemBodyModelPrefab;

        public static GameObject NailBombProjectileMain;

        public static GameObject NailBombNailEffect;

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

            BuffAPI.Add(new CustomBuff(NailBombCooldownDebuff));

            NailBombImmunityBuff = ScriptableObject.CreateInstance<BuffDef>();
            NailBombImmunityBuff.name = "Aetherium: Nail Bomb Immunity";
            NailBombImmunityBuff.buffColor = new Color(255, 255, 255);
            NailBombImmunityBuff.canStack = false;
            NailBombImmunityBuff.isDebuff = false;
            NailBombImmunityBuff.iconSprite = MainAssets.LoadAsset<Sprite>("NailBombNailCooldownIcon.png");

            BuffAPI.Add(new CustomBuff(NailBombImmunityBuff));

        }


        public void CreateEffect()
        {
            NailBombNailEffect = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/effects/impacteffects/ImpactNailgun"), "NailBombNailImpact");


            var nailImpactSoundDef = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            nailImpactSoundDef.eventName = "Aetherium_Nailbomb_Nail_Impact";
            SoundAPI.AddNetworkedSoundEvent(nailImpactSoundDef);

            var effectComponent = NailBombNailEffect.GetComponent<EffectComponent>();
            effectComponent.soundName = "Aetherium_Nailbomb_Nail_Impact";

            NailBombNailEffect.AddComponent<NetworkIdentity>();

            if (NailBombNailEffect) { PrefabAPI.RegisterNetworkPrefab(NailBombNailEffect); }
            EffectAPI.AddEffect(NailBombNailEffect);
        }

        private void CreateProjectile()
        {
            NailBombProjectileMain = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/EngiGrenadeProjectile"), "NailBombProjectile", true);

            var networkIdentityMain = NailBombProjectileMain.GetComponent<NetworkIdentity>();
            if (!networkIdentityMain) { NailBombProjectileMain.AddComponent<NetworkIdentity>(); }

            var model = MainAssets.LoadAsset<GameObject>("NailBombProjectile.prefab");
            model.AddComponent<ProjectileGhostController>();
            model.AddComponent<NetworkIdentity>();

            var scaleCurve = model.AddComponent<ObjectScaleCurve>();
            scaleCurve.useOverallCurveOnly = true;
            scaleCurve.overallCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.1f, 1));

            var projectileController = NailBombProjectileMain.GetComponent<ProjectileController>();
            projectileController.ghostPrefab = model;

            var velocityRandom = NailBombProjectileMain.AddComponent<VelocityRandomOnStart>();
            velocityRandom.coneAngle = 30;
            velocityRandom.directionMode = VelocityRandomOnStart.DirectionMode.Cone;
            velocityRandom.baseDirection = Vector3.up;
            velocityRandom.minSpeed = 15;
            velocityRandom.maxSpeed = 20;

            NailBombProjectileMain.AddComponent<ProjectileVelocityDetonate>();

            UnityEngine.Object.Destroy(NailBombProjectileMain.GetComponent<ProjectileImpactExplosion>());

            var impactExplosion = NailBombProjectileMain.AddComponent<ProjectileFixedImpactExplosion>();
            impactExplosion.ChildBulletAttack = true;
            impactExplosion.childTracerPrefab = Resources.Load<GameObject>("Prefabs/effects/tracers/TracerToolbotNails");
            impactExplosion.childHitEffectPrefab = NailBombNailEffect;
            impactExplosion.childrenCount = AmountOfNailsPerNailBomb;
            impactExplosion.explosionEffect = Resources.Load<GameObject>("Prefabs/effects/Omnieffect/OmniExplosionVFXCommandoGrenade");
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
            ProjectileAPI.Add(NailBombProjectileMain);

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.16759F, -0.07591F, 0.06936F),
                    localAngles = new Vector3(343.2889F, 299.2036F, 176.8172F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.14431F, -0.06466F, -0.03696F),
                    localAngles = new Vector3(355.1616F, 81.55997F, 180F),
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
                    localPos = new Vector3(0.08787F, 0.07478F, 1.04472F),
                    localAngles = new Vector3(354.9749F, 182.8028F, 237.0256F),
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
                    localPos = new Vector3(-0.20102F, 0.09445F, 0.16025F),
                    localAngles = new Vector3(15.50638F, 144.8099F, 180.4037F),
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
                    localPos = new Vector3(-0.17241F, -0.0089F, 0.02642F),
                    localAngles = new Vector3(5.28933F, 111.5028F, 190.532F),
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
                    localPos = new Vector3(0.16832F, 0.04282F, 0.06368F),
                    localAngles = new Vector3(355.8307F, 42.81982F, 185.1587F),
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
                    localPos = new Vector3(-0.6845F, -0.60707F, -0.05308F),
                    localAngles = new Vector3(349.4037F, 73.89225F, 346.442F),
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
                    localPos = new Vector3(-0.2442F, 0.04122F, 0.01506F),
                    localAngles = new Vector3(22.73106F, 289.1799F, 159.5365F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
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
                    localAngles = new Vector3(1.77184F, 278.9485F, 190.4101F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.21004F, -0.09095F, -0.09165F),
                    localAngles = new Vector3(0F, 60.43688F, 180F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.17925F, -0.02363F, -0.11047F),
                    localAngles = new Vector3(359.353F, 299.9855F, 169.6378F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            RoR2Application.onLoad += OnLoadModCompat;

            if (UseAlternateImplementation)
            {
                On.RoR2.CharacterBody.FixedUpdate += FireNailBombFromBody;
            }
            else
            {
                On.RoR2.GlobalEventManager.OnHitEnemy += FireNailBomb;
            }
        }

        private void OnLoadModCompat()
        {
            if (IsItemStatsModInstalled)
            {
                CreateNailBombStatDef();
            }

            if (IsBetterUIInstalled)
            {
                var bombCooldownDebuffInfo = CreateBetterUIBuffInformation($"{ItemLangTokenName}_BOMB_COOLDOWN", NailBombCooldownDebuff.name, "You've run out of materials to create another Nail Bomb, keep looking around!", false);
                RegisterBuffInfo(NailBombCooldownDebuff, bombCooldownDebuffInfo.Item1, bombCooldownDebuffInfo.Item2);
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
