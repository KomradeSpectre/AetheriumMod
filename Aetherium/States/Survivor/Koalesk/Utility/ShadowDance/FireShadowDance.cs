using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using Aetherium.Utils;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.States.Survivor.Koalesk.Utility.ShadowDance
{
    internal class FireShadowDance : BaseSkillState
    {
        public Vector3? ChosenHitPosition;
        public override void OnEnter()
        {
            var childLocator = GetModelChildLocator();
            if (childLocator)
            {
                var arm = childLocator.FindChild("HandL");
                var hitPoint = Utils.MiscHelpers.RaycastToDirection(arm ? arm.position : characterBody.corePosition, 500, GetAimRay().direction, LayerIndex.world.intVal);
                if (hitPoint.HasValue)
                {
                    ChosenHitPosition = hitPoint;
                }
                else
                {
                    outer.SetNextStateToMain();
                }
            }

            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!IsKeyDownAuthority())
            {
                outer.SetNextStateToMain();
            }

            if (ChosenHitPosition.HasValue)
            {
                ModLogger.LogError($"D");
                var closestPointOnSphere = Utils.MathHelpers.ClosestPointOnSphereToPoint(ChosenHitPosition.Value, 10, characterBody.corePosition);

                Vector3 directionToPlayer = (closestPointOnSphere - characterBody.corePosition).normalized;
                Vector3 projectedVelocity = Vector3.Project(characterBody.rigidbody.velocity, directionToPlayer);

                float velocityDifference = Mathf.Max(0, 5 - projectedVelocity.magnitude);
                var desiredForce = directionToPlayer * velocityDifference;

                var physInfo = new PhysForceInfo()
                {
                    massIsOne = true,
                    disableAirControlUntilCollision = true,
                    ignoreGroundStick = true,
                    force = desiredForce,
                };

                var characterMotor = characterBody.characterMotor;
                if (characterMotor)
                {
                    characterMotor.ApplyForceImpulseFixed(physInfo);
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
    }
}
