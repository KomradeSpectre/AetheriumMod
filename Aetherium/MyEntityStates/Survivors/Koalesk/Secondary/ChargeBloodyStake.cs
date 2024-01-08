using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.MyEntityStates.Survivors.Koalesk
{
    public class ChargeBloodyStake : BaseSkillState
    {
        public float Stopwatch;
        public float StackConsumptionDuration = 0.25f;
        public int StacksConsumed;
        public List<GameObject> vfxCreated = new List<GameObject> ();

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Stopwatch += Time.fixedDeltaTime;
            if(Stopwatch > StackConsumptionDuration)
            {
                ModLogger.LogError("Past stopwatch check.");
                Stopwatch -= StackConsumptionDuration;

                if (characterBody && base.isAuthority)
                {
                    ModLogger.LogError("Made past the characterbody and is authority check.");
                    var stacksAvailable = characterBody.GetBuffCount(Aetherium.Survivors.Koalesk.BloodliquorBuff);
                    var stacksToBeConsumed = Math.Min(1, stacksAvailable);
                    characterBody.SetBuffCount(Aetherium.Survivors.Koalesk.BloodliquorBuff.buffIndex, stacksAvailable - stacksToBeConsumed);

                    StacksConsumed += stacksToBeConsumed;

                    if(vfxCreated.Count < StacksConsumed + 1)
                    {
                        var toBeCreatedCount = (StacksConsumed + 1) - vfxCreated.Count;

                        for(int i = 1; i <= toBeCreatedCount; i++)
                        {
                            var stakeProjectileVFX = UnityEngine.Object.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
                            vfxCreated.Add(stakeProjectileVFX);
                        }
                    }
                }
            }

            if (!inputBank.skill2.down)
            {
                outer.SetNextState(new FireBloodyStake() { ProjectilesToGenerate = StacksConsumed + 1});
            }
        }

        public override void Update()
        {
            base.Update();

            var vfxDistributionPoints = Utils.MathHelpers.DistributePointsEvenlyOnSphereCap(0.25f * vfxCreated.Count, 1, characterBody.corePosition + inputBank.aimDirection, RoR2.Util.QuaternionSafeLookRotation(inputBank.aimDirection));

            for (int i = 0; i < vfxCreated.Count; i++)
            {
                if (vfxCreated[i] != null)
                {
                    vfxCreated[i].transform.position = vfxDistributionPoints[i];
                    vfxCreated[i].transform.rotation = RoR2.Util.QuaternionSafeLookRotation(inputBank.aimDirection);
                }
            }
        }

        public override void OnExit()
        {
            ModLogger.LogError($"We are currently going to destroy {vfxCreated.Count} temporary effects.");

            foreach(var vfx in vfxCreated)
            {
                if (vfx != null)
                {
                    UnityEngine.Object.Destroy(vfx.gameObject);
                }
            }

            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
