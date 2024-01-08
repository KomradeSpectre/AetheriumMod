using System.Collections;
using UnityEngine;
using RoR2.Projectile;

namespace Aetherium.Utils.Components
{
    public class NewBehaviourScript1 : MonoBehaviour, IProjectileImpactBehavior
    {
        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            if(impactInfo.collider)
            {

            }
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}