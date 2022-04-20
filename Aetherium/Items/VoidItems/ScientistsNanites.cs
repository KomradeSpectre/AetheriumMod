using Aetherium.Utils.Components;
using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Items.VoidItems
{
    internal class ScientistsNanites : ItemBase<ScientistsNanites>
    {
        public override string ItemName => "Scientists Nanites";

        public override string ItemLangTokenName => "SCIENTISTS_NANITES";

        public override string ItemPickupDesc => "On purchase, chance to send out a few homing nanite swarms that convert enemies touched into decaying drones or turrets. Corrupts Engineer's Toolbelts.";

        public override string ItemFullDescription => "On purchase, gain a [x] percent chance to fire out [x] nanite swarm projectiles towards nearby enemies. When the nanites impact an enemy, they transform into a drone/turret.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("BlasterSword.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("EngineersToolbeltIcon.png");

        public override string CorruptsItem => "ITEM_ENGINEERS_TOOLBELT_NAME";

        public static GameObject SwordProjectile;

        private static readonly List<string> DronesList = new List<string>
        {
            "DroneBackup",
            "Drone1",
            "Drone2",
            "EquipmentDrone",
            "EmergencyDrone",
            "FlameDrone",
            "MegaDrone",
            "DroneMissile",
            "Turret1"
        };

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateProjectile();
            CreateItem();
            Hooks();
        }

        public void CreateProjectile()
        {
            SwordProjectile = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/FMJ"), "NaniteProjectile", true);

            var model = MainAssets.LoadAsset<GameObject>("BlasterSwordProjectile.prefab");
            model.AddComponent<NetworkIdentity>();
            model.AddComponent<RoR2.Projectile.ProjectileGhostController>();

            var controller = SwordProjectile.GetComponent<RoR2.Projectile.ProjectileController>();
            controller.procCoefficient = 0.5f;
            controller.ghostPrefab = model;

            SwordProjectile.GetComponent<RoR2.TeamFilter>().teamIndex = TeamIndex.Player;

            var damage = SwordProjectile.GetComponent<RoR2.Projectile.ProjectileDamage>();
            damage.damageType = DamageType.CrippleOnHit;
            damage.damage = 0;

            var intervalController = SwordProjectile.GetComponent<ProjectileIntervalOverlapAttack>();
            UnityEngine.Object.Destroy(intervalController);

            var impactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/VagrantCannonExplosion");

            var projectileTarget = SwordProjectile.AddComponent<ProjectileTargetComponent>();

            var projectileDirectionalTargetFinder = SwordProjectile.AddComponent<ProjectileDirectionalTargetFinder>();
            projectileDirectionalTargetFinder.lookRange = 40;
            projectileDirectionalTargetFinder.lookCone = 35;
            projectileDirectionalTargetFinder.targetSearchInterval = 0.1f;
            projectileDirectionalTargetFinder.onlySearchIfNoTarget = true;
            projectileDirectionalTargetFinder.allowTargetLoss = false;
            projectileDirectionalTargetFinder.testLoS = false;
            projectileDirectionalTargetFinder.ignoreAir = false;
            projectileDirectionalTargetFinder.flierAltitudeTolerance = float.PositiveInfinity;
            projectileDirectionalTargetFinder.targetComponent = projectileTarget;

            var projectileHoming = SwordProjectile.AddComponent<ProjectileSteerTowardTarget>();
            projectileHoming.targetComponent = projectileTarget;
            projectileHoming.rotationSpeed = 90;
            projectileHoming.yAxisOnly = false;

            var projectileSimple = SwordProjectile.GetComponent<ProjectileSimple>();
            projectileSimple.enableVelocityOverLifetime = true;
            projectileSimple.updateAfterFiring = true;
            projectileSimple.velocityOverLifetime = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(2, 70) });

            var impactExplosion = SwordProjectile.AddComponent<RoR2.Projectile.ProjectileImpactExplosion>();
            impactExplosion.impactEffect = impactEffect;
            impactExplosion.blastRadius = 2;
            impactExplosion.blastProcCoefficient = 0f;
            impactExplosion.lifetimeAfterImpact = 1.5f;
            impactExplosion.timerAfterImpact = true;
            impactExplosion.blastDamageCoefficient = 1;

            SwordProjectile.AddComponent<ProjectileTransformVictim>();

            R2API.ContentAddition.AddProjectile(SwordProjectile);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.PurchaseInteraction.OnInteractionBegin += FireNaniteMissiles;
        }

        private void FireNaniteMissiles(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, RoR2.PurchaseInteraction self, RoR2.Interactor activator)
        {
            if (NetworkServer.active && activator && activator.gameObject && self.GetInteractability(activator) == Interactability.Available)
            {
                var characterBody = activator.gameObject.GetComponent<CharacterBody>();

                if (characterBody)
                {
                    var inventoryCount = GetCount(characterBody);

                    if (inventoryCount > 0)
                    {
                        var characterMaster = characterBody.master;

                        if (characterMaster)
                        {
                            var summonMasterBehavior = self.gameObject.GetComponent<SummonMasterBehavior>();

                            if (summonMasterBehavior)
                            {
                                var masterPrefab = summonMasterBehavior.masterPrefab;

                                if (masterPrefab)
                                {
                                    var masterPrefabMaster = masterPrefab.GetComponent<CharacterMaster>();

                                    if (masterPrefabMaster && IsDroneSupported(masterPrefabMaster.name))
                                    {
                                        var masterPrefabBodyPrefab = masterPrefabMaster.bodyPrefab;

                                        if (masterPrefabBodyPrefab)
                                        {
                                            var duplicationBody = masterPrefabBodyPrefab.GetComponent<CharacterBody>();

                                            if (duplicationBody)
                                            {
                                                /*if (Util.CheckRoll(InverseHyperbolicScaling(BaseDuplicationPercentChance, AdditionalDuplicationPercentChance, 1, inventoryCount) * 100, characterBody.master))
                                                {

                                                }*/

                                                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo();
                                                fireProjectileInfo.owner = characterBody.gameObject;
                                                fireProjectileInfo.position = MiscUtils.AboveTargetBody(duplicationBody, 3f).Value;
                                                fireProjectileInfo.rotation = Quaternion.identity;
                                                fireProjectileInfo.damage = 0;
                                                fireProjectileInfo.projectilePrefab = SwordProjectile;
                                                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            orig(self, activator);
        }

        private bool IsDroneSupported(CharacterMaster botMaster)
        {
            return IsDroneSupported(botMaster.name);
        }

        private bool IsDroneSupported(string botMasterName)
        {
            return DronesList.Exists((droneSubstring) => { return botMasterName.Contains(droneSubstring); });
        }

        /// <summary>
        /// Allows a custom drone to be revived by Engineer's Toolbelt.
        /// </summary>
        /// <param name="bodyName">The CharacterBody name of the custom drone.</param>
        /// <returns>True if the custom drone is now supported. False if the custom drone is already supported.</returns>
        public bool AddCustomDrone(string bodyName)
        {
            if (DronesList.Exists(item => item == bodyName)) return false;
            DronesList.Add(bodyName);
            return true;
        }
    }
}
