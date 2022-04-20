using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Utils.Components
{
    internal class ProjectileTransformVictim : MonoBehaviour
    {
        public ProjectileImpactExplosion ProjectileImpactExplosion;
        public ProjectileController ProjectileController;
        public CharacterBody OwnerBody;

        public GameObject DetonationEffect;

        public float SearchRadius = 1;
        public bool HasTransformed;
        public bool Decay;

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
            if (!OwnerBody && ProjectileController)
            {
                if (ProjectileController.owner)
                {
                    OwnerBody = ProjectileController.owner.GetComponent<CharacterBody>();
                }
            }

            if(!HasTransformed && ProjectileImpactExplosion && ProjectileImpactExplosion.hasImpact && ProjectileController && ProjectileController.teamFilter && OwnerBody)
            {
                RoR2.TeamMask enemyTeams = RoR2.TeamMask.GetEnemyTeams(ProjectileController.teamFilter.teamIndex);
                RoR2.HurtBox[] hurtBoxes = new RoR2.SphereSearch
                {
                    radius = SearchRadius,
                    mask = RoR2.LayerIndex.entityPrecise.mask,
                    origin = transform.position,
                }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                if(hurtBoxes.Length > 0)
                {
                    var enemyHit = hurtBoxes[0];
                    if(enemyHit.healthComponent && enemyHit.healthComponent.body)
                    {
                        var body = enemyHit.healthComponent.body;
                        var master = enemyHit.healthComponent.body.master;
                        if (master)
                        {
                            var baseAI = enemyHit.healthComponent.body.master.GetComponent<BaseAI>();
                            if (baseAI)
                            {
                                master.teamIndex = ProjectileController.teamFilter.teamIndex;
                                body.teamComponent.teamIndex = ProjectileController.teamFilter.teamIndex;
                                UnityEngine.Object.Destroy(body.teamComponent.indicator);
                                body.teamComponent.SetupIndicator();

                                baseAI.currentEnemy.Reset();
                                baseAI.ForceAcquireNearestEnemyIfNoCurrentEnemy();
                            }
                        }
                        HasTransformed = true;
                        if (Decay)
                        {
                            body.inventory.GiveItem(RoR2Content.Items.HealthDecay);
                        }
                    }
                }
            }
        }
    }
}
