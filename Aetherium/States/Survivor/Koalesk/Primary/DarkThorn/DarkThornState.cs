using KoaleskSurvivor = Aetherium.Survivors.Koalesk;
using System.Collections.Generic;
using UnityEngine;

namespace Aetherium.States.Survivor.Koalesk.Primary
{
    public class DarkThornState : KoaleskMeleeBase
    {
        public override void OnEnter()
        {
            if (characterBody && characterBody.GetBuffCount(KoaleskSurvivor.DarkblightBuff) > 0)
            {
                characterBody.SetBuffCount(KoaleskSurvivor.DarkblightBuff.buffIndex, characterBody.GetBuffCount(KoaleskSurvivor.DarkblightBuff) - 1);
            }

            baseDuration = 1.25f;
            attackStartPercent = 0.33f;
            attackEndPercent = 0.46f;
            damageCoefficient = 4.0f;   
            pushForce = -2500f;  
            hitboxName = "DarkThornHitbox";
            animationName = "DarkThornClawSwipe";

            base.OnEnter();
        }

        protected override void OnHitEnemyAuthority()
        {
            if (characterBody && !hasHit) KoaleskSurvivor.AddBloodliquorStacks(characterBody, 1);
        }
    }
}