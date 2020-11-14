using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Utils
{
    public class MiscUtils
    {
        //Sourced from source code, couldn't access because it was private, modified a little
        public static Vector3? RaycastToFloor(Vector3 position, float maxDistance)
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(new Ray(position, Vector3.down), out raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                return raycastHit.point;
            }
            return null;
        }
    }
}
