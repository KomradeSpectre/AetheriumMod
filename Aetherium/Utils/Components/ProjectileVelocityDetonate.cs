using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2.Projectile;
using UnityEngine.Networking;
using RoR2;

namespace Aetherium.Utils.Components
{
    public class ProjectileVelocityDetonate : MonoBehaviour
    {
        public ProjectileFixedImpactExplosion ProjectileFixedImpactExplosion;
        public ProjectileController ProjectileController;

        public GameObject DetonationEffect;

        public void Start()
        {
            var impactExplosion = GetComponent<ProjectileFixedImpactExplosion>();
            if (impactExplosion)
            {
                ProjectileFixedImpactExplosion = impactExplosion;
                ProjectileController = impactExplosion.projectileController;
            }
        }

        public void CallDetonationEffect()
        {
            if (DetonationEffect && NetworkServer.active)
            {
                var effectData = new EffectData
                {
                    origin = gameObject.transform.position,
                    rootObject = gameObject
                };
                EffectManager.SpawnEffect(DetonationEffect, effectData, true);
            }
        }

        public void FixedUpdate()
        {
            if(ProjectileController && ProjectileFixedImpactExplosion)
            {
                if(ProjectileController.rigidbody && ProjectileController.rigidbody.velocity.y < 0)
                {
                    CallDetonationEffect();
                    ProjectileFixedImpactExplosion.Detonate();
                }
            }
            else
            {
                Destroy(this);
            }
        }
    }
}
