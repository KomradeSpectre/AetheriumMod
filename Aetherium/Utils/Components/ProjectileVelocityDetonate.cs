using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2.Projectile;

namespace Aetherium.Utils.Components
{
    public class ProjectileVelocityDetonate : MonoBehaviour
    {
        public ProjectileImpactExplosion ProjectileImpactExplosion;
        public ProjectileController ProjectileController;
        public void Start()
        {
            var impactExplosion = GetComponent<ProjectileImpactExplosion>();
            if (impactExplosion)
            {
                ProjectileImpactExplosion = impactExplosion;
                ProjectileController = impactExplosion.projectileController;
            }
        }

        public void FixedUpdate()
        {
            if(ProjectileController && ProjectileImpactExplosion)
            {
                if(ProjectileController.rigidbody && ProjectileController.rigidbody.velocity.y < 0)
                {
                    ProjectileImpactExplosion.Detonate();
                }
            }
            else
            {
                Destroy(this);
            }
        }
    }
}
