using System;
using UnityEngine;

namespace Aetherium.Utils
{
    public class MathHelpers
    {
        /// <summary>
        /// Converts a float number to a percentage string. Default is base 100, so 2 = 200%.
        /// </summary>
        /// <param name="number">The number you wish to convert to a percentage.</param>
        /// <param name="numberBase">The multiplier or base of the number.</param>
        /// <returns>A string representing the percentage value of the number converted using our number base.</returns>
        public static string FloatToPercentageString(float number, float numberBase = 100)
        {
            return (number * numberBase).ToString("##0") + "%";
        }

        /// <summary>
        /// Returns a Vector3 representing the closest point on a sphere to another point at a set radius.
        /// </summary>
        /// <param name="origin">The starting position.</param>
        /// <param name="radius">The radius of the sphere.</param>
        /// <param name="targetPosition">The target's position.</param>
        /// <returns>The point on the sphere closest to the target position.</returns>
        public static Vector3 ClosestPointOnSphereToPoint(Vector3 origin, float radius, Vector3 targetPosition)
        {
            Vector3 differenceVector = targetPosition - origin;
            differenceVector = Vector3.Normalize(differenceVector);
            differenceVector *= radius;

            return origin + differenceVector;
        }
    }
}