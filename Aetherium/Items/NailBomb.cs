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
        public static ConfigOption<float> PercentDamageThresholdRequiredToActivate;
        public static ConfigOption<int> AmountOfNailsPerNailBomb;
        public static ConfigOption<float> PercentDamagePerNailInNailBomb;
        public static ConfigOption<float> PercentDamageBonusOfAdditionalStacks;
        public static ConfigOption<bool> UseAlternateImplementation;
        public static ConfigOption<float> NailBombDropDelay;
        public static ConfigOption<float> DurationPercentageReducedByWithAdditionalStacks;

        public override string ItemName => "Nail Bomb";

        public override string ItemLangTokenName => "NAIL_BOMB";

        public override string ItemPickupDesc => UseAlternateImplementation ? $"Occasionally drop a shrapnel grenade from your position that explodes after a delay." : $"Attacks that deal <style=cIsDamage>high damage</style> release a shrapnel grenade that explodes after a delay.";

        public override string ItemFullDescription => UseAlternateImplementation ? $"After {NailBombDropDelay} second(s) <style=cStack>(-{FloatToPercentageString(DurationPercentageReducedByWithAdditionalStacks)} per stack)</style> you will drop a shrapnel grenade from your current position that explodes for {AmountOfNailsPerNailBomb}x{FloatToPercentageString(PercentDamagePerNailInNailBomb)} of your damage <style=cStack>(+{FloatToPercentageString(PercentDamageBonusOfAdditionalStacks)} more per stack). The shrapnel has a high chance to trigger On-Hit effects." : $"Attacks that deal {FloatToPercentageString(PercentDamageThresholdRequiredToActivate)} damage or more release a shrapnel grenade that explodes for {AmountOfNailsPerNailBomb}x{FloatToPercentageString(PercentDamagePerNailInNailBomb)} of your damage <style=cStack>(+{FloatToPercentageString(PercentDamageBonusOfAdditionalStacks)} more per stack)</style>.";

        public override string ItemLore => "";

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public override ItemTier Tier => UseAlternateImplementation ? ItemTier.Tier2 : ItemTier.Tier1;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("NailBomb.prefab");

        public override Sprite ItemIcon => UseAlternateImplementation ? MainAssets.LoadAsset<Sprite>("NailBombTier2.png") : MainAssets.LoadAsset<Sprite>("NailBombTier1.png");

        public static GameObject NailBombProjectileMain;
        public static GameObject NailBombProjectileSub;

        public static BuffDef NailBombCooldownDebuff;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuff();
            CreateProjectile();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            PercentDamageThresholdRequiredToActivate = config.ActiveBind<float>("Item: " + ItemName, "Percent Damage Threshold Required to Activate Effect", 1.2f, "What percentage of damage should we deal in a single hit to activate the effect of this item?");
            AmountOfNailsPerNailBomb = config.ActiveBind<int>("Item: " + ItemName, "Amount of Nails per Nail Bomb", 20, "How many nails should get released upon explosion of the projectile?");
            PercentDamagePerNailInNailBomb = config.ActiveBind<float>("Item: " + ItemName, "Percent Damage per Nail in Nail Bomb", 0.3f, "What percentage of damage should each nail in the nail bomb deal?");
            PercentDamageBonusOfAdditionalStacks = config.ActiveBind<float>("Item: " + ItemName, "Percent Damage Bonus of Additional Stacks", 0.5f, "What additional percentage of the body's damage should be given per additional stacks of Nail Bomb?");
            UseAlternateImplementation = config.ActiveBind<bool>("Item: " + ItemName, "Use Alternate Item Implementation?", false, "If true, Nail Bomb drops from your position after a delay.");
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
            NailBombCooldownDebuff.iconSprite = MainAssets.LoadAsset<Sprite>("AccursedPotionSipCooldownDebuffIcon.png");

            BuffAPI.Add(new CustomBuff(NailBombCooldownDebuff));

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

            NailBombProjectileSub = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/SyringeProjectile"), "NailBombProjectileSub", true);

            var networkIdentitySub = NailBombProjectileSub.GetComponent<NetworkIdentity>();
            if (!networkIdentitySub) { NailBombProjectileSub.AddComponent<NetworkIdentity>(); }

            var modelSub = MainAssets.LoadAsset<GameObject>("NailBombSubProjectile.prefab");
            modelSub.AddComponent<ProjectileGhostController>();
            modelSub.AddComponent<NetworkIdentity>();

            var projectileControllerSub = NailBombProjectileSub.GetComponent<ProjectileController>();
            projectileControllerSub.procCoefficient = UseAlternateImplementation ? 3.5f : 1f;
            projectileControllerSub.ghostPrefab = modelSub;

            var projectileSimple = NailBombProjectileSub.GetComponent<ProjectileSimple>();
            projectileSimple.desiredForwardSpeed = 150;

            UnityEngine.Object.Destroy(NailBombProjectileMain.GetComponent<ProjectileImpactExplosion>());

            var impactExplosion = NailBombProjectileMain.AddComponent<ProjectileFixedImpactExplosion>();
            impactExplosion.childrenProjectilePrefab = NailBombProjectileSub;
            impactExplosion.childrenCount = AmountOfNailsPerNailBomb;
            impactExplosion.explosionEffect = Resources.Load<GameObject>("Prefabs/effects/Omnieffect/OmniExplosionVFX");
            impactExplosion.childrenDamageCoefficient = PercentDamagePerNailInNailBomb;
            impactExplosion.fireChildren = true;
            impactExplosion.MinDeviationAngle = 0;
            impactExplosion.MaxDeviationAngle = 45;
            impactExplosion.Direction = Vector3.down;
            impactExplosion.transformSpace = ProjectileImpactExplosion.TransformSpace.World;
            impactExplosion.destroyOnEnemy = false;
            impactExplosion.destroyOnWorld = false;
            impactExplosion.lifetime = 2;
            impactExplosion.lifetimeAfterImpact = 0.2f;

            PrefabAPI.RegisterNetworkPrefab(NailBombProjectileMain);
            ProjectileAPI.Add(NailBombProjectileMain);

            PrefabAPI.RegisterNetworkPrefab(NailBombProjectileSub);
            ProjectileAPI.Add(NailBombProjectileSub);

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
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
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo()
                    {
                        projectilePrefab = NailBombProjectileMain,
                        owner = self.gameObject,
                        damage = self.damage + (self.damage * (PercentDamageBonusOfAdditionalStacks * (inventoryCount - 1))),
                        position = self.corePosition != Vector3.zero ? self.corePosition : self.transform.position,
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
            var attacker = damageInfo.attacker;
            if (attacker)
            {
                var body = attacker.GetComponent<CharacterBody>();
                var victimBody = victim.GetComponent<CharacterBody>();
                if (body && victimBody)
                {
                    var InventoryCount = GetCount(body);
                    if (InventoryCount > 0)
                    {
                        if (damageInfo.damage / body.damage >= PercentDamageThresholdRequiredToActivate)
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
                        }
                    }
                }
            }
            orig(self, damageInfo, victim);
        }
    }
}
