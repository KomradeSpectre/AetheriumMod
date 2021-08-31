using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Utils.Components
{
    public class ProjectileFixedImpactExplosion : ProjectileImpactExplosion
    {
        public float MinDeviationAngle;
        public float MaxDeviationAngle;
        public Vector3 Direction;

        public Matrix4x4 LookAt(Vector3 dir, Vector3 up, Vector3 right)
        {
            if (Mathf.Abs(Vector3.Dot(dir, up) / (dir.magnitude * up.magnitude)) > 1 - 0.000001)
            {
                up = right;
            }
            return Matrix4x4.LookAt(Vector3.zero, dir, up);
        }

        public override Vector3 GetRandomDirectionForChild()
        {
            var matrixLocal = Matrix4x4.Rotate(Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.forward)) * Matrix4x4.Rotate(Quaternion.AngleAxis(UnityEngine.Random.Range(MinDeviationAngle, MaxDeviationAngle), Vector3.up));

            Matrix4x4 matrixDirection = new Matrix4x4();

            switch (transformSpace)
            {
                case (TransformSpace.World):
                    matrixDirection = LookAt(Direction, Vector3.up, Vector3.right) * matrixLocal;
                    break;

                case (TransformSpace.Normal):
                    matrixDirection = LookAt(impactNormal, Vector3.up, Vector3.right) * LookAt(Direction, Vector3.up, Vector3.right) * matrixLocal;
                    break;

                case (TransformSpace.Local):
                    matrixDirection = LookAt(base.transform.forward, Vector3.up, Vector3.right) * LookAt(Direction, Vector3.up, Vector3.right) * matrixLocal;
                    break;
            }
            return matrixDirection.MultiplyPoint(Vector3.forward);
        }
    }
}
