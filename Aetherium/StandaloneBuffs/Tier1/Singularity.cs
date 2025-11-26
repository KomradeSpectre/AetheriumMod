using Aetherium.Utils;
using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Interactables.BuffBrazier;

using System.Linq;
using UnityEngine.Networking;

namespace Aetherium.StandaloneBuffs.Tier1
{
    internal class Singularity : BuffBase<Singularity>
    {
        public override string BuffName => "Singularity";

        public override Color Color => new Color32(48, 25, 52, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("SingularityDebuffIcon.png");

        public override bool IsDebuff => true;

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.HoldoutZoneController.Update += PullTowardsCentralPoint;
        }

        private void PullTowardsCentralPoint(On.RoR2.HoldoutZoneController.orig_Update orig, HoldoutZoneController self)
        {
            if (NetworkServer.active)
            {
                HurtBox[] HurtBoxes = new SphereSearch()
                {
                    origin = self.transform.position,
                    mask = LayerIndex.entityPrecise.mask,
                    radius = self.currentRadius
                }.RefreshCandidates().OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();


                foreach (HurtBox hurtBox in HurtBoxes)
                {
                    if (hurtBox && hurtBox.healthComponent)
                    {
                        var body = hurtBox.healthComponent.body;
                        if (body && body.HasBuff(BuffDef))
                        {

                            Vector3 directionToCenterOfRadius = (self.gameObject.transform.position - body.transform.position).normalized;
                            Vector3 projectedVelocity = Vector3.Project(body.rigidbody.velocity, directionToCenterOfRadius);

                            float maxVelocity = 3;

                            float velocityDifference = Mathf.Max(0, maxVelocity - projectedVelocity.magnitude);
                            var desiredForce = directionToCenterOfRadius * velocityDifference;

                            var physInfo = new PhysForceInfo()
                            {
                                massIsOne = true,
                                disableAirControlUntilCollision = true,
                                ignoreGroundStick = true,
                                force = desiredForce,
                            };

                            var characterMotor = body.characterMotor;
                            if (characterMotor)
                            {
                                characterMotor.ApplyForceImpulseFixed(physInfo);
                            }
                            else
                            {
                                var rigidBodyMotor = body.GetComponent<RigidbodyMotor>();
                                if (rigidBodyMotor)
                                {
                                    rigidBodyMotor.ApplyForceImpulse(physInfo);
                                }
                            }
                        }
                    }
                }
            }
            orig(self);
        }
    }
}
