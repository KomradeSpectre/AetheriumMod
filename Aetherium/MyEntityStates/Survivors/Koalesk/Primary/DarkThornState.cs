using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RoR2;
using EntityStates;
using UnityEngine;
using Aetherium.Utils;
using RoR2.Projectile;

namespace Aetherium.MyEntityStates.Survivors.Koalesk
{
    internal class DarkThornState : BasicMelee
    {
        bool HasGrantedBuff = false;
        bool HasFiredHand = false;

        public override void OnEnter()
        {
            if (characterBody)
            {
                var darkThornTiming = characterBody.GetComponent<KoaleskMainState.DarkThornTimingAdjuster>();
                if (darkThornTiming)
                {
                    duration = darkThornTiming.baseDuration;
                    attackStartTime = darkThornTiming.attackStartTime;
                    attackEndTime = darkThornTiming.attackEndTime;
                    baseEarlyExitTime = darkThornTiming.baseEarlyExitTime;

                    damageCoefficient = 5f;
                    procCoefficient = 1;

                    hitboxName = "DarkThornHitbox";
                }
            }            

            base.OnEnter();

            if (!HasFiredHand)
            {
                FireProjectileInfo darkThornProjectile = new FireProjectileInfo()
                {
                    owner = characterBody.gameObject,
                    projectilePrefab = Aetherium.Survivors.Koalesk.KoaleskDarkThornProjectile,
                    speedOverride = 150.0f,
                    damage = characterBody.damage * 1.5f,
                    position = characterBody.corePosition,
                    rotation = Util.QuaternionSafeLookRotation(inputBank.aimDirection - characterBody.corePosition),
                    procChainMask = default
                };
                ProjectileManager.instance.FireProjectile(darkThornProjectile);

                HasFiredHand = true;
            }

        }

        protected override void PlayAttackAnimation()
        {
            base.PlayCrossfade("Gesture, Override", "Slash2", "Slash.playbackRate", this.duration, 0.1f * duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority)
            {

            }
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();

            if (!HasGrantedBuff)
            {
                HasGrantedBuff = true;
                if (characterBody.timedBuffs.Any(x => x.buffIndex == Aetherium.Survivors.Koalesk.BloodliquorBuff.buffIndex))
                {
                    ItemHelpers.RefreshTimedBuffs(characterBody, Aetherium.Survivors.Koalesk.BloodliquorBuff, 4, 1);
                }
                characterBody.AddTimedBuff(Aetherium.Survivors.Koalesk.BloodliquorBuff, 2);
            }

            
        }
    }
}
