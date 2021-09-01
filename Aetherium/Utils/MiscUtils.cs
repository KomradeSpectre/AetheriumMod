using RoR2;
using RoR2.Navigation;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Aetherium.Utils;
using UnityEngine.Networking;
using R2API;

namespace Aetherium.Utils
{
    public static class MiscUtils
    {
        //Sourced from source code, couldn't access because it was private, modified a little
        public static Vector3? RaycastToDirection(Vector3 position, float maxDistance, Vector3 direction)
        {
            if (Physics.Raycast(new Ray(position, direction), out RaycastHit raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                return raycastHit.point;
            }
            return null;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> toShuffle, Xoroshiro128Plus random)
        {
            List<T> shuffled = new List<T>();
            foreach (T value in toShuffle)
            {
                shuffled.Insert(random.RangeInt(0, shuffled.Count + 1), value);
            }
            return shuffled;
        }

        public static Vector3 FindClosestGroundNodeToPosition(Vector3 position, HullClassification hullClassification)
        {
            Vector3 ResultPosition;

            NodeGraph groundNodes = SceneInfo.instance.groundNodes;

            var closestNode = groundNodes.FindClosestNode(position, hullClassification);

            if(closestNode != NodeGraph.NodeIndex.invalid)
            {
                groundNodes.GetNodePosition(closestNode, out ResultPosition);
                return ResultPosition;
            }

            Debug.LogWarning($"No closest node to be found for XYZ: {position}, returning 0,0,0");
            return Vector3.zero;
        }

        public static Vector3? AboveTargetVectorFromDamageInfo(DamageInfo damageInfo, float distanceAboveTarget)
        {
            if (damageInfo.rejected || !damageInfo.attacker) { return null; }

            var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();

            if (attackerBody)
            {
                RoR2.TeamMask enemyTeams = RoR2.TeamMask.GetEnemyTeams(attackerBody.teamComponent.teamIndex);
                HurtBox hurtBox = new RoR2.SphereSearch
                {
                    radius = 1,
                    mask = RoR2.LayerIndex.entityPrecise.mask,
                    origin = damageInfo.position
                }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().FirstOrDefault();

                if (hurtBox)
                {
                    if(hurtBox.healthComponent && hurtBox.healthComponent.body)
                    {
                        var body = hurtBox.healthComponent.body;

                        var closestPointOnBounds = body.mainHurtBox.collider.ClosestPointOnBounds(body.transform.position + new Vector3(0, 10000, 0));

                        var raycastPoint = RaycastToDirection(closestPointOnBounds, distanceAboveTarget, Vector3.up);
                        if (raycastPoint.HasValue)
                        {
                            return raycastPoint.Value;
                        }
                        else
                        {
                            return closestPointOnBounds + (Vector3.up * distanceAboveTarget);
                        }
                    }
                    
                }
            }

            return null;
        }

        public static Vector3? AboveTargetBody(CharacterBody body, float distanceAbove)
        {
            if (!body) { return null; }

            var closestPointOnBounds = body.mainHurtBox.collider.ClosestPointOnBounds(body.transform.position + new Vector3(0, 10000, 0));

            var raycastPoint = RaycastToDirection(closestPointOnBounds, distanceAbove, Vector3.up);
            if (raycastPoint.HasValue)
            {
                return raycastPoint.Value;
            }
            else
            {
                return closestPointOnBounds + (Vector3.up * distanceAbove);
            }
        }
    }
}
