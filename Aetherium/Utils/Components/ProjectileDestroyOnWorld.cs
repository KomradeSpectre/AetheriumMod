using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Utils.Components
{
    internal class ProjectileDestroyOnWorld : MonoBehaviour, IProjectileImpactBehavior
    {
        public GameObject impactEffect;
        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            AetheriumPlugin.ModLogger.LogError($"We've hit something with the projectile.");
            if (impactInfo.collider && !impactInfo.collider.GetComponent<HurtBox>())
            {
                AetheriumPlugin.ModLogger.LogError($"We're in the hit world check.");
                if (impactEffect)
                {
                    EffectManager.SpawnEffect(impactEffect, new EffectData
                    {
                        origin = base.transform.position,
                        scale = 1
                    }, transmit: true);
                }
                Destroy(base.gameObject);
            }
        }
    }
}
