using RoR2;
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
    }
}
