using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using TILER2;
using UnityEngine;

namespace Aetherium.Equipment
{
    public class JarOfReshaping : Equipment<JarOfReshaping>
    {
        public override string displayName => "Jar of Reshaping";

        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangDesc(string langID = null) => "";

        protected override string NewLangLore(string langID = null) => "";

        protected override string NewLangPickup(string langID = null) => "";

        public JarOfReshaping()
        {

        }

        protected override void LoadBehavior()
        {
        }

        protected override void UnloadBehavior()
        {
        }

        protected override bool OnEquipUseInner(EquipmentSlot slot)
        {
            var body = slot.characterBody;
            if (body)
            {
                var bulletTracker = body.GetComponent<JarBulletTracker>();
                if (!bulletTracker) { body.gameObject.AddComponent<JarBulletTracker>(); }
                if(bulletTracker.BulletsGrabbed <= 0)
                {
                    RoR2.TeamMask enemyTeams = RoR2.TeamMask.GetEnemyTeams(body.teamComponent.teamIndex);
                    RoR2.Projectile.ProjectileController[] bullets = new RoR2.SphereSearch
                    {
                        radius = baseRadiusGranted,
                        mask = RoR2.LayerIndex.entityPrecise.mask,
                        origin = body.corePosition
                    }.RefreshCandidates().FilterCandidatesByProjectileControllers();
                }
                else
                {

                }
            }
        }

        public class JarBulletTracker : MonoBehaviour
        {
            public int BulletsGrabbed;

            public void FixedUpdate()
            {
                if(BulletsGrabbed <= 0)
                {
                    UnityEngine.Object.Destroy(this);
                }
            }
        }
    }
}
