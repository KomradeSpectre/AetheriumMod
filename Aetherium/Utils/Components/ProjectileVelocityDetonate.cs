using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2.Projectile;

namespace Aetherium.Utils.Components
{
    public class ProjectileVelocityDetonate : MonoBehaviour
    {
        public ProjectileFixedImpactExplosion ProjectileFixedImpactExplosion;
        public ProjectileController ProjectileController;
        public void Start()
        {
            var impactExplosion = GetComponent<ProjectileFixedImpactExplosion>();
            if (impactExplosion)
            {
                ProjectileFixedImpactExplosion = impactExplosion;
                ProjectileController = impactExplosion.projectileController;
            }
        }

        public void FixedUpdate()
        {
            if(ProjectileController && ProjectileFixedImpactExplosion)
            {
                if(ProjectileController.rigidbody && ProjectileController.rigidbody.velocity.y < 0)
                {
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
