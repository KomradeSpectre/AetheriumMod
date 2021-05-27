using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Utils.Components
{
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
