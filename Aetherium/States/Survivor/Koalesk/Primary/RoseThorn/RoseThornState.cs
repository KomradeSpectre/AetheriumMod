using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RoR2;
using EntityStates;
using UnityEngine;
using UnityEngine.Networking;
using Aetherium.Utils;
using Aetherium.States.Survivor;

namespace Aetherium.States.Survivor.Koalesk.Primary.RoseThorn
{
    internal class RoseThornState : BaseMeleeAttack
    {
        public BuffDef BloodliquorBuff => Aetherium.Survivors.Koalesk.BloodliquorBuff;
        public BuffDef DarkblightBuff => Aetherium.Survivors.Koalesk.DarkblightBuff;

        bool HasGrantedBuff = false;

        float DoubleSlashStartTime;
        float DoubleSlashEndTime;
        OverlapAttack DoubleSlashAttack;

        bool HasConsumedRequiredAmountForDoubleHit = false;
        int RequiredStacksToDoubleHit = 1;

        public override void OnEnter()
        {
            if (!BloodliquorBuff || !DarkblightBuff) 
            {
                base.OnExit();
                return;
            }

            DoubleSlashAttack = new OverlapAttack()
            {
                attacker = gameObject,
                teamIndex = GetTeam(),
                inflictor = gameObject,
                hitBoxGroup = FindHitBoxGroup("DoubleSlashHitbox"),
                damage = characterBody.damage,
                isCrit = RollCrit(),
                procCoefficient = 0.5f
            };

            hitEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniImpactExecute");
            switch (swingIndex)
            {
                case 0:
                    baseDuration = 1.10f;
                    attackStartPercentTime = 29f / 90;
                    attackEndPercentTime = 36f / 90;
                    earlyExitPercentTime = 0.85f;

                    DoubleSlashStartTime = 34f / 90;
                    DoubleSlashEndTime = 41f / 90;
                    DoubleSlashAttack.damage *= 1.5f;

                    hitStopDuration = 0.1f;
                    hitHopVelocity = 6;
                    attackRecoil = 1;

                    damageCoefficient = 3f;
                    procCoefficient = 1;

                    pushForce = 250f;
                    break;
                
                case 1:
                    baseDuration = 1.10f;
                    attackStartPercentTime = 30f / 90;
                    attackEndPercentTime = 37f / 90;
                    earlyExitPercentTime = 0.85f;

                    DoubleSlashStartTime = 35f / 90;
                    DoubleSlashEndTime = 42f / 90;
                    DoubleSlashAttack.damage *= 1.5f;

                    hitStopDuration = 0.1f;
                    hitHopVelocity = 6;
                    attackRecoil = 1;

                    damageCoefficient = 3f;
                    procCoefficient = 1;

                    pushForce = 250f;
                    break;

                case 2:
                    baseDuration = 1.85f;
                    attackStartPercentTime = 45f / 120;
                    attackEndPercentTime = 52f / 120;
                    earlyExitPercentTime = 0.85f;

                    DoubleSlashStartTime = 50f / 120;
                    DoubleSlashEndTime = 57f / 120;
                    DoubleSlashAttack.damage *= 2.5f;

                    hitStopDuration = 0.15f;
                    hitHopVelocity = 12;
                    attackRecoil = 2.5f;

                    damageCoefficient = 5f;
                    procCoefficient = 1;

                    pushForce = 750f;
                    break;
            }

            hitboxGroupName = "RoseThornHitbox";

            var bloodLiquorCount = characterBody.GetBuffCount(Aetherium.Survivors.Koalesk.BloodliquorBuff.buffIndex);
            if (bloodLiquorCount > 0 && bloodLiquorCount - RequiredStacksToDoubleHit >= 0)
            {
                characterBody.SetBuffCount(Aetherium.Survivors.Koalesk.BloodliquorBuff.buffIndex, bloodLiquorCount - RequiredStacksToDoubleHit);
                HasConsumedRequiredAmountForDoubleHit = true;
            }

            base.OnEnter();

            base.StartAimMode(0.5f + this.duration, false);

        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (HasConsumedRequiredAmountForDoubleHit)
            {
                if (stopwatch >= DoubleSlashStartTime * duration && stopwatch <= DoubleSlashEndTime * duration)
                {
                    if (isAuthority)
                    {
                        DoubleSlashAttack.Fire();
                    }
                }
            }
        }

        protected override void PlayAttackAnimation()
        {
            base.PlayCrossfade("Gesture, Override", "RoseThornSlash" + (swingIndex + 1), "Slash.playbackRate", this.duration, 0.1f * duration);
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();

            /*

            if (HasConsumedRequiredAmountForDoubleHit)
            {
                attack?.ResetIgnoredHealthComponents();
                attack?.Fire();
            }*/

            if (!HasGrantedBuff && isAuthority)
            {
                HasGrantedBuff = true;
                Survivors.Koalesk.AddDarkblightStacks(characterBody, 1);
            }
        }

    }
}
