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

namespace Aetherium.Items
{
    public class NailBomb : ItemBase<NailBomb>
    {
        public ConfigOption<float> PercentDamageThresholdRequiredToActivate;
        public ConfigOption<int> AmountOfNailsPerNailBomb;
        public ConfigOption<float> PercentDamagePerNailInNailBomb;

        public override string ItemName => "Nail Bomb";

        public override string ItemLangTokenName => "NAIL_BOMB";

        public override string ItemPickupDesc => $"Attacks that deal <style=cIsDamage>high damage</style> release a shrapnel grenade that explodes after a delay.";
        public override string ItemFullDescription => $"Attacks that deal {FloatToPercentageString(PercentDamageThresholdRequiredToActivate)} damage or more release a shrapnel grenade that explodes for {AmountOfNailsPerNailBomb}x{FloatToPercentageString(PercentDamagePerNailInNailBomb)} damage. Cooldown of 8s (-15% per stack).";

        public override string ItemLore => "";

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("NailBomb.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("FeatheredPlumeIcon.png");

        public static GameObject NailBombProjectileMain;
        public static GameObject NailBombProjectileSub;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateProjectile();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            PercentDamageThresholdRequiredToActivate = config.ActiveBind<float>("Item: " + ItemName, "Percent Damage Threshold Required to Activate Effect", 3f, "What percentage of damage should we deal in a single hit to activate the effect of this item?");
            AmountOfNailsPerNailBomb = config.ActiveBind<int>("Item: " + ItemName, "Amount of Nails per Nail Bomb", 10, "How many nails should get released upon explosion of the projectile?");
            PercentDamagePerNailInNailBomb = config.ActiveBind<float>("Item: " + ItemName, "Percent Damage per Nail in Nail Bomb", 0.3f, "What percentage of damage should each nail in the nail bomb deal?");
        }

        private void CreateProjectile()
        {
            NailBombProjectileMain = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/Funball"), "NailBombProjectile");

            var networkIdentityMain = NailBombProjectileMain.GetComponent<NetworkIdentity>();
            if (!networkIdentityMain) { NailBombProjectileMain.AddComponent<NetworkIdentity>(); }

            var model = MainAssets.LoadAsset<GameObject>("NailBomb.prefab");
            model.AddComponent<ProjectileGhostController>();
            model.AddComponent<NetworkIdentity>();

            var projectileController = NailBombProjectileMain.GetComponent<ProjectileController>();
            projectileController.ghostPrefab = model;

            var damage = NailBombProjectileMain.AddComponent<ProjectileDamage>();

            var funballBehaviour = NailBombProjectileMain.GetComponent<ProjectileFunballBehavior>();
            funballBehaviour.blastDamage = 0;

            var velocityRandom = NailBombProjectileMain.GetComponent<VelocityRandomOnStart>();
            velocityRandom.coneAngle = 30;
            velocityRandom.directionMode = VelocityRandomOnStart.DirectionMode.Hemisphere;
            velocityRandom.baseDirection = Vector3.up;
            velocityRandom.minSpeed = 25;
            velocityRandom.maxSpeed = 50;

            NailBombProjectileSub = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/SyringeProjectile"), "NailBombProjectileSub");

            var networkIdentitySub = NailBombProjectileSub.GetComponent<NetworkIdentity>();
            if(!networkIdentitySub) { NailBombProjectileSub.AddComponent<NetworkIdentity>(); }

            var impactExplosion = NailBombProjectileMain.AddComponent<ProjectileImpactExplosion>();
            impactExplosion.childrenProjectilePrefab = NailBombProjectileSub;
            impactExplosion.childrenCount = AmountOfNailsPerNailBomb;
            impactExplosion.childrenDamageCoefficient = PercentDamagePerNailInNailBomb;
            impactExplosion.minAngleOffset = new Vector3(-180, -180, -180);
            impactExplosion.maxAngleOffset = new Vector3(180, 180, 180);
            impactExplosion.fireChildren = true;

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
            On.RoR2.GlobalEventManager.OnHitEnemy += FireNailBomb;
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
                            var calculatedUpPosition = victimBody.mainHurtBox.collider.ClosestPointOnBounds(victimBody.transform.position + new Vector3(0, 10000, 0)) + (Vector3.up * 3);
                            FireProjectileInfo newProjectileLaunch = new FireProjectileInfo()
                            {
                                projectilePrefab = NailBombProjectileMain,
                                owner = body.gameObject,
                                damage = body.damage,
                                position = calculatedUpPosition,
                                damageTypeOverride = null,
                                damageColorIndex = DamageColorIndex.Default,
                                procChainMask = default
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
