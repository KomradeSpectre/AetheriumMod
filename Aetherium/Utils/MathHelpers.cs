using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using R2API;
using RoR2;

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

        /// <summary>
        /// A method to create a list of points evenly spread around a sphere at a set radius.
        /// </summary>
        /// <param name="points">The number of points to spread around a sphere.</param>
        /// <param name="radius">The radius of our sphere.</param>
        /// <param name="origin">The point of reference for our sphere.</param>
        /// <returns>A list of points evenly distributed around the desired sphere.</returns>
        public static List<Vector3> DistributePointsEvenlyAroundSphere(int points, float radius, Vector3 origin)
        {
            List<Vector3> pointArray = new List<Vector3>();

            var phi = Math.PI * (3 - Math.Sqrt(5));

            for (int i = 0; i < points; i++)
            {
                var yCoord = 1 - (i / (points - 1)) * 2;
                var radiusCoord = Math.Sqrt(1 - yCoord * yCoord);
                var theta = phi * i;

                var xCoord = (float)(Math.Cos(theta) * radiusCoord);
                var zCoord = (float)(Math.Sin(theta) * radiusCoord);

                var calculatedPoint = origin + new Vector3(xCoord, yCoord, zCoord);

                pointArray.Add(calculatedPoint * radius);
            }

            return pointArray;

        }

        public static List<Vector3> DistributePointsEvenlyOnSphereCap(float radius, float capHeight, Vector3 center, Quaternion rotation)
        {
            List<Vector3> positions = new List<Vector3>();
            float thetaMax = Mathf.Acos(1 - capHeight / radius); // Calculate thetaMax based on cap height
            int rows = (int)Mathf.Ceil((Mathf.PI / 2) / thetaMax); // Calculate number of rows based on cap height and distribution

            for (int i = 0; i < rows; i++)
            {
                float theta = thetaMax * i / (rows - 1);
                int pointsInThisRow = (int)Mathf.Pow(2, i);

                for (int j = 0; j < pointsInThisRow; j++)
                {
                    float phi = 2 * Mathf.PI * j / pointsInThisRow;
                    float x = radius * Mathf.Sin(theta) * Mathf.Cos(phi);
                    float y = radius * Mathf.Sin(theta) * Mathf.Sin(phi);
                    float z = radius * Mathf.Cos(theta);

                    Vector3 point = new Vector3(x, y, z);
                    point = rotation * point; // Apply rotation
                    point += center; // Adjust for center position

                    positions.Add(point);
                }
            }

            return positions;
        }

        /// <summary>
        /// A method to create a list of points evenly spread around a circle at a set radius. Three points make a triangle, four make a square, and so on.
        /// </summary>
        /// <param name="points">The number of points to spread around a circle.</param>
        /// <param name="radius">The radius of our circle.</param>
        /// <param name="origin">The point of reference for our circle.</param>
        /// <param name="angleOffset">How far along the circle should we shift all our points?</param>
        /// <returns>A list of points evenly distributed around the desired circle.</returns>
        public static List<Vector3> DistributePointsEvenlyAroundCircle(int points, float radius, Vector3 origin, float angleLength = Mathf.PI * 2, float angleOffset = 0)
        {
            List<Vector3> pointsList = new List<Vector3>();
            for (int i = 0; i < points; i++)
            {
                var theta = (angleLength) / points;
                var angle = theta * i + angleOffset;
                Vector3 positionChosen;

                positionChosen = new Vector3((float)(radius * Math.Cos(angle) + origin.x), origin.y, (float)(radius * Math.Sin(angle) + origin.z));

                pointsList.Add(positionChosen);
            }

            return pointsList;
        }

        public static Vector3 GetPointOnUnitSphereCap(Quaternion targetDirection, float angle)
        {
            var angleInRad = UnityEngine.Random.Range(0.0f, angle) * Mathf.Deg2Rad;
            var PointOnCircle = (UnityEngine.Random.insideUnitCircle.normalized) * Mathf.Sin(angleInRad);
            var V = new Vector3(PointOnCircle.x, PointOnCircle.y, Mathf.Cos(angleInRad));
            return targetDirection * V;
        }

        public static Vector3[] DistributeEvenlyAlongRainbowArc(int numberOfProjectiles, float radius, float arcHeight, Vector3 startPosition)
        {
            Vector3[] positions = new Vector3[numberOfProjectiles];

            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float angle = Mathf.PI * i / (numberOfProjectiles - 1);
                float x = radius * Mathf.Sin(angle);
                float y = arcHeight; // Height of the arc
                float z = radius * Mathf.Cos(angle);

                // Adjusting the position relative to the character's position
                positions[i] = new Vector3(x + startPosition.x, y + startPosition.y, z + startPosition.z);
            }

            return positions;
        }

        public static Vector3 GetPointOnUnitSphereCap(Vector3 targetDirection, float angle)
        {
            return GetPointOnUnitSphereCap(Quaternion.LookRotation(targetDirection), angle);
        }

        public static Vector3 RandomPointOnCircle(Vector3 origin, float radius, Xoroshiro128Plus random)
        {
            float angle = random.RangeFloat(0, 2f * Mathf.PI);
            return origin + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        }

        public static Vector3 GetRandomPointOnSphere(float radius, Xoroshiro128Plus random, Vector3 origin)
        {
            float theta = 2 * Mathf.PI * random.nextNormalizedFloat; // Random angle between 0 and 2π
            float phi = Mathf.Acos(2 * random.nextNormalizedFloat - 1); // Inverse cosine for uniform distribution

            float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta) + origin.x;
            float y = radius * Mathf.Sin(phi) * Mathf.Sin(theta) + origin.y;
            float z = radius * Mathf.Cos(phi) + origin.z;

            return new Vector3(x, y, z);
        }

        // Method to get a random point on the surface of a tube with Quaternion rotation
        public static Vector3 GetRandomPointOnTube(Vector3 center, float radius, float height, Quaternion rotation, Xoroshiro128Plus random)
        {
            // Randomize the angle for the circular part of the tube
            float angle = random.RangeFloat(0f, 2 * Mathf.PI);

            // Randomize the height along the tube
            float randomHeight = random.RangeFloat(-height / 2, height / 2);

            // Calculate the point on the tube before rotation
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);

            // Apply the Quaternion rotation
            Vector3 point = rotation * new Vector3(x, randomHeight, z);

            // Return the point in 3D space, considering the tube's center
            return center + point;
        }

        /// <summary>
        /// Calculates inverse hyperbolic scaling (diminishing) for the parameters passed in, and returns the result.
        /// <para>Uses the formula: baseValue + (maxValue - baseValue) * (1 - 1 / (1 + additionalValue * (itemCount - 1)))</para>
        /// </summary>
        /// <param name="baseValue">The starting value of the function.</param>
        /// <param name="additionalValue">The value that is added per additional itemCount</param>
        /// <param name="maxValue">The maximum value that the function can possibly be.</param>
        /// <param name="itemCount">The amount of items/stacks that increments our function.</param>
        /// <returns>A float representing the inverse hyperbolic scaling of the parameters.</returns>
        public static float InverseHyperbolicScaling(float baseValue, float additionalValue, float maxValue, int itemCount)
        {
            return baseValue + (maxValue - baseValue) * (1 - 1 / (1 + additionalValue * (itemCount - 1)));
        }
    }
}