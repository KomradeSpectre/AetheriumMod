using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Utils.Components
{
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileWorldTargetImpact : MonoBehaviour, IProjectileImpactBehavior
    {
        private ProjectileController projectileController;
        public Collider projectileCollider;
        private bool alive = true;
        public GameObject impactEffect;
        public string hitSoundString;
        public string enemyHitSoundString;

        public void Awake()
        {
            this.projectileController = this.GetComponent<ProjectileController>();
        }

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            ModLogger.LogError($"Entering Projectile Impact");
            if (!this.alive)
                return;
            ModLogger.LogError($"We're Alive");
            Collider collider = impactInfo.collider;
            if (collider)
            {
                ModLogger.LogError($"We have a collider.");
                HurtBox hurtBox = collider.GetComponent<HurtBox>();
                if (!hurtBox)
                {
                    ModLogger.LogError($"There's no hurtbox on this collider.");
                    var colliderTopLevel = collider.gameObject.transform.root;
                    if (colliderTopLevel)
                    {
                        var colliderBody = colliderTopLevel.GetComponent<CharacterBody>();
                        if (!colliderBody)
                        {
                            ModLogger.LogError($"The collider did not belong to a characterbody hierarchy.");
                            this.alive = false;
                        }
                        else
                        {
                            Physics.IgnoreCollision(projectileCollider, collider);
                        }
                    }
                }
            }
            if (this.alive)
                return;

            if (NetworkServer.active && this.impactEffect)
            {
                EffectManager.SimpleImpactEffect(this.impactEffect, impactInfo.estimatedPointOfImpact, -this.transform.forward, !this.projectileController.isPrediction);
            }
            Util.PlaySound(this.hitSoundString, this.gameObject);

            UnityEngine.Object.Destroy(this.gameObject);
        }
    }
}
