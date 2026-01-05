using KoaleskSurvivor = Aetherium.Survivors.Koalesk;
using Aetherium.Survivors.Components; 
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Aetherium.States.Survivor.Koalesk.Primary
{
    public class RoseThornState : KoaleskMeleeBase
    {
        private bool isEmpowered = false;
        private int currentStep = 0;           

        public override void OnEnter()
        {
            var passive = GetComponent<KoaleskPassive>();
            if (passive)
            {
                currentStep = passive.RoseThornStepIndex;
            }

            if (characterBody && characterBody.GetBuffCount(KoaleskSurvivor.BloodliquorBuff) > 0)
            {
                isEmpowered = true;
                characterBody.SetBuffCount(KoaleskSurvivor.BloodliquorBuff.buffIndex, characterBody.GetBuffCount(KoaleskSurvivor.BloodliquorBuff) - 1);
            }

            switch (currentStep)
            {
                case 0:
                    baseDuration = 1.1f;
                    attackStartPercent = 0.32f;
                    attackEndPercent = 0.40f;
                    damageCoefficient = 3.0f;
                    animationName = "RoseThornSlash1";
                    break;
                case 1:
                    baseDuration = 1.1f;
                    attackStartPercent = 0.33f;
                    attackEndPercent = 0.41f;
                    damageCoefficient = 3.0f;
                    animationName = "RoseThornSlash2";
                    break;
                case 2:
                    baseDuration = 1.85f;
                    attackStartPercent = 0.37f;
                    attackEndPercent = 0.43f;
                    damageCoefficient = 5.0f;
                    animationName = "RoseThornSlash3";
                    break;
            }

            hitboxName = "RoseThornHitbox";

            base.OnEnter();

            if (passive)
            {
                passive.RoseThornStepIndex = (passive.RoseThornStepIndex + 1) % 3;
            }
        }

        protected override void OnHitEnemyAuthority()
        {
            if (characterBody && !hasHit) KoaleskSurvivor.AddDarkblightStacks(characterBody, 1);

            if (isEmpowered)
            {
                FireAfterImage();
                isEmpowered = false;
            }
        }

        private void FireAfterImage()
        {
            new BlastAttack
            {
                attacker = gameObject,
                baseDamage = damageCoefficient * damageStat * 0.5f,
                position = transform.position + transform.forward * 2f,
                radius = 6f,
                teamIndex = GetTeam(),
                falloffModel = BlastAttack.FalloffModel.None
            }.Fire();
        }
    }
}