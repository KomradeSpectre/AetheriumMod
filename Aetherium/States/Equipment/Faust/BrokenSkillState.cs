using EntityStates;
using RoR2;
using UnityEngine;

namespace Aetherium.States.Equipment.Faust
{
    public class BrokenSkillState : BaseSkillState
    {
        public static float baseDuration = 0.1f;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;

            if(base.characterBody)
            {
                Util.PlaySound("Play_UI_insufficient_funds", base.gameObject);
            }

            if(base.characterBody)
            {
                EffectManager.SimpleMuzzleFlash(
                    Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2ResetEffect"),
                    base.gameObject,
                    "Head",
                    false
                );
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }
}