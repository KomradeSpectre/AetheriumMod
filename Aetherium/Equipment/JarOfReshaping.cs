using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using TILER2;
using UnityEngine;
using UnityEngine.Networking;

namespace Aetherium.Equipment
{
    public class JarOfReshaping : Equipment<JarOfReshaping>
    {
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("What radius should the jar devour bullets? (Default: 20m)", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseRadiusGranted { get; private set; } = 20f;

        public override string displayName => "Jar of Reshaping";

        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangDesc(string langID = null) => "";

        protected override string NewLangLore(string langID = null) => "";

        protected override string NewLangPickup(string langID = null) => "";

        public static GameObject JarProjectile;

        public JarOfReshaping()
        {

        }

        protected override void LoadBehavior()
        {
            JarProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/Rocket"), "SwordProjectile", true);

            var model = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/BlasterSwordProjectile.prefab");
            model.AddComponent<NetworkIdentity>();
            model.AddComponent<RoR2.Projectile.ProjectileGhostController>();

            var controller = JarProjectile.GetComponent<RoR2.Projectile.ProjectileController>();
            controller.procCoefficient = 0.5f;
            controller.ghostPrefab = model;

            JarProjectile.GetComponent<RoR2.TeamFilter>().teamIndex = TeamIndex.Player;

            var damage = JarProjectile.GetComponent<RoR2.Projectile.ProjectileDamage>();
            damage.damageType = DamageType.CrippleOnHit;
            damage.damage = 0;

            var impactExplosion = JarProjectile.GetComponent<RoR2.Projectile.ProjectileImpactExplosion>();
            impactExplosion.impactEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/VagrantCannonExplosion");
            impactExplosion.blastRadius = 2;
            impactExplosion.blastProcCoefficient = 0.2f;

            // register it for networking
            if (JarProjectile) PrefabAPI.RegisterNetworkPrefab(JarProjectile);

            // add it to the projectile catalog or it won't work in multiplayer
            RoR2.ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(JarProjectile);
            };

        }

        protected override void UnloadBehavior()
        {
        }

        protected override bool OnEquipUseInner(EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.teamComponent) return false;
            var body = slot.characterBody;

            var bulletTracker = body.GetComponent<JarBulletTracker>();
            if (!bulletTracker) { body.gameObject.AddComponent<JarBulletTracker>(); }
            if (bulletTracker.jarBullets.Count <= 0)
            {

                List<ProjectileController> bullets = new List<ProjectileController>();
                new RoR2.SphereSearch
                {
                    radius = baseRadiusGranted,
                    mask = RoR2.LayerIndex.entityPrecise.mask,
                    origin = body.corePosition
                }.RefreshCandidates().FilterCandidatesByProjectileControllers().GetProjectileControllers(bullets);
                foreach (ProjectileController controller in bullets)
                {
                    var controllerOwner = controller.owner;
                    if (controllerOwner)
                    {
                        var ownerBody = controllerOwner.GetComponent<CharacterBody>();
                        if (ownerBody)
                        {
                            if (ownerBody.teamComponent.teamIndex == body.teamComponent.teamIndex)
                            {
                                continue;
                            }
                            var projectileDamage = controller.gameObject.GetComponent<ProjectileDamage>();
                            if (projectileDamage)
                            {
                                bulletTracker.jarBullets.Add(new JarBullet(projectileDamage.damage, projectileDamage.damageColorIndex, projectileDamage.damageType));
                            }
                        }
                    }
                }
                slot.subcooldownTimer = 5;
                return true;
            }
            else
            {
                foreach(JarBullet bullet in bulletTracker.jarBullets)
                {
                    FireProjectileInfo
                }
            }
        }

        public class JarBullet
        {
            public float Damage;
            public DamageColorIndex DamageColorIndex;
            public DamageType DamageType;

            public JarBullet(float damage, DamageColorIndex damageColorIndex, DamageType damageType)
            {
                Damage = damage;
                DamageColorIndex = damageColorIndex;
                DamageType = damageType;
            }

        }

        public class JarBulletTracker : MonoBehaviour
        {
            public List<JarBullet> jarBullets = new List<JarBullet>();

        }
    }
}
