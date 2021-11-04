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

        public static Vector3 FindClosestNodeToPosition(Vector3 position, HullClassification hullClassification, bool checkAirNodes = false)
        {
            Vector3 ResultPosition;

            NodeGraph nodesToCheck = checkAirNodes ? SceneInfo.instance.airNodes : SceneInfo.instance.groundNodes;

            var closestNode = nodesToCheck.FindClosestNode(position, hullClassification);

            if(closestNode != NodeGraph.NodeIndex.invalid)
            {
                nodesToCheck.GetNodePosition(closestNode, out ResultPosition);
                return ResultPosition;
            }

            Debug.LogWarning($"No closest node to be found for XYZ: {position}, returning 0,0,0");
            return Vector3.zero;
        }

        public static bool TeleportBody(CharacterBody characterBody, GameObject target, GameObject teleportEffect, HullClassification hullClassification, Xoroshiro128Plus rng, float minDistance = 20, float maxDistance = 45, bool teleportAir = false)
        {
            if (!characterBody){ return false; }

            SpawnCard spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
            spawnCard.hullSize = hullClassification;
            spawnCard.nodeGraphType = teleportAir ? MapNodeGroup.GraphType.Air : MapNodeGroup.GraphType.Ground;
            spawnCard.prefab = Resources.Load<GameObject>("SpawnCards/HelperPrefab");
            GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                position = target.transform.position,
                minDistance = minDistance,
                maxDistance = maxDistance
            }, rng));
            if (gameObject)
            {
                TeleportHelper.TeleportBody(characterBody, gameObject.transform.position);
                GameObject teleportEffectPrefab = teleportEffect;
                if (teleportEffectPrefab)
                {
                    EffectManager.SimpleEffect(teleportEffectPrefab, gameObject.transform.position, Quaternion.identity, true);
                }
                UnityEngine.Object.Destroy(gameObject);
                UnityEngine.Object.Destroy(spawnCard);
                return true;
            }
            else
            {
                UnityEngine.Object.Destroy(spawnCard);
                return false;
            }
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

        public static Dictionary<string, Vector3> GetAimSurfaceAlignmentInfo(Ray ray, int layerMask, float distance)
        {
            Dictionary<string, Vector3> SurfaceAlignmentInfo = new Dictionary<string, Vector3>();

            var didHit = Physics.Raycast(ray, out RaycastHit raycastHit, distance, layerMask, QueryTriggerInteraction.Ignore);

            if (!didHit)
            {
                AetheriumPlugin.ModLogger.LogError($"GetAimSurfaceAlignmentInfo did not hit anything in the aim direction on the specified layer.");
                return null;
            }

            var point = raycastHit.point;
            var right = Vector3.Cross(ray.direction, Vector3.up);
            var up = Vector3.ProjectOnPlane(raycastHit.normal, right);
            var forward = Vector3.Cross(right, up);

            SurfaceAlignmentInfo.Add("Position", point);
            SurfaceAlignmentInfo.Add("Right", right);
            SurfaceAlignmentInfo.Add("Forward", forward);
            SurfaceAlignmentInfo.Add("Up", up);

            return SurfaceAlignmentInfo;
        }
    }
}
