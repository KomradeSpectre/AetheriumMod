using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Utils
{
    public static class MiscUtils
    {
        //Sourced from source code, couldn't access because it was private, modified a little
        public static Vector3? RaycastToFloor(Vector3 position, float maxDistance)
        {
            if (Physics.Raycast(new Ray(position, Vector3.down), out RaycastHit raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
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

        public class ProjectileRotateTowardsVelocity : MonoBehaviour
        {
            public bool InvertVelocity;

            private ProjectileNetworkTransform NetworkTransform;
            private Rigidbody Rigidbody;

            private Vector3 LastVelocity;

            public void Start()
            {
                NetworkTransform = gameObject.GetComponent<ProjectileNetworkTransform>();
                Rigidbody = gameObject.GetComponent<Rigidbody>();
            }

            public void FixedUpdate()
            {
                if (NetworkTransform && Rigidbody)
                {
                    if (Rigidbody.velocity != Vector3.zero && Rigidbody.velocity != LastVelocity)
                    {
                        NetworkTransform.transform.rotation = Quaternion.LookRotation(InvertVelocity ? -Rigidbody.velocity : Rigidbody.velocity);
                    }

                    LastVelocity = Rigidbody.velocity;
                }
            }
        }
    }
}
