using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RoR2;
using EntityStates;
using UnityEngine;
using UnityEngine.Networking;
using Aetherium.Utils;

namespace Aetherium.MyEntityStates.Survivors.Koalesk
{
    internal class RoseThornState : BasicMelee
    {
        bool HasGrantedBuff = false;
        bool HasConsumedRequiredAmountForDoubleHit = false;
        bool HasFiredDoublehit = false;
        int RequiredStacksToDoubleHit = 1;
        float doubleAttackStartTime = 0.45f;
        float doubleAttackEndTime = 0.46f;

        public override void OnEnter()
        {

            baseDuration = 1f;
            attackStartTime = 0.35f;
            attackEndTime = 0.36f;
            baseEarlyExitTime = 1f;

            damageCoefficient = 3.5f;
            procCoefficient = 1;

            hitboxName = "RoseThornHitbox";


            /*if (characterBody)
            {
                var roseTiming = characterBody.GetComponent<KoaleskMainState.RoseThornTimingAdjuster>();
                if (roseTiming)
                {
                    baseDuration = roseTiming.baseDuration;
                    attackStartTime = roseTiming.attackStartTime;
                    attackEndTime = roseTiming.attackEndTime;
                    baseEarlyExitTime = roseTiming.baseEarlyExitTime;

                    damageCoefficient = 3.5f;
                    procCoefficient = 1;

                    hitboxName = "RoseThornHitbox";
                }
            }*/

            base.OnEnter();

            var bloodLiquorCount = characterBody.GetBuffCount(Aetherium.Survivors.Koalesk.BloodliquorBuff.buffIndex);
            if (bloodLiquorCount > 0 && bloodLiquorCount - RequiredStacksToDoubleHit >= 0)
            {
                characterBody.SetBuffCount(Aetherium.Survivors.Koalesk.BloodliquorBuff.buffIndex, bloodLiquorCount - RequiredStacksToDoubleHit);
                HasConsumedRequiredAmountForDoubleHit = true;
            }

        }

        public override void OnExit()
        {
            base.OnExit();
            if(!HasFiredDoublehit && HasConsumedRequiredAmountForDoubleHit)
            {
                attack?.ResetIgnoredHealthComponents();
                attack?.Fire();
                HasFiredDoublehit = true;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.stopwatch > doubleAttackStartTime * duration && base.stopwatch <= doubleAttackEndTime * duration)
            {
                if (HasConsumedRequiredAmountForDoubleHit)
                {
                    attack?.ResetIgnoredHealthComponents();
                    attack?.Fire();
                    HasFiredDoublehit = true;
                }
            }
        }

        protected override void PlayAttackAnimation()
        {
            base.PlayCrossfade("Gesture, Override", "Slash1", "Slash.playbackRate", this.duration, 0.1f * duration);
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();

            if (!HasGrantedBuff)
            {
                HasGrantedBuff = true;
                if (characterBody.timedBuffs.Any(x => x.buffIndex == Aetherium.Survivors.Koalesk.DarkblightBuff.buffIndex))
                {
                    ItemHelpers.RefreshTimedBuffs(characterBody, Aetherium.Survivors.Koalesk.DarkblightBuff, 4, 1);
                }
                characterBody.AddTimedBuff(Aetherium.Survivors.Koalesk.DarkblightBuff, 2);
            }
        }

    }
}
