using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RoR2;
using EntityStates;
using UnityEngine;
using Aetherium.Utils;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Survivors.Koalesk;
using RoR2.Projectile;
using RoR2.Skills;

namespace Aetherium.States.Survivor.Koalesk.Primary
{
    internal class DarkThornState : BaseMeleeAttack
    {
        public BuffDef BloodliquorBuff => Aetherium.Survivors.Koalesk.BloodliquorBuff;
        public BuffDef DarkblightBuff => Aetherium.Survivors.Koalesk.DarkblightBuff;

        float BloodliquorBuffDuration = 4f;
        bool HasGrantedBuff = false;
        bool HasFiredHand = false;
        float fireDarkThornTime;

        public override void OnEnter()
        {
            if (!BloodliquorBuff || !DarkblightBuff)
            {
                base.OnExit();
                return;
            }

            hitboxGroupName = "DarkThornHitbox";

            baseDuration = 1.25f;
            attackStartPercentTime = 30f / 90;
            attackEndPercentTime = 42f / 90;
            earlyExitPercentTime = 0.85f;

            fireDarkThornTime = 40f / 90;

            damageCoefficient = 1.5f;
            procCoefficient = 0.5f;

            base.OnEnter();

        }

        protected override void PlayAttackAnimation()
        {
            base.PlayCrossfade("Gesture, Override", "DarkThornClawSwipe", "Slash.playbackRate", this.duration, 0.1f * duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority)
            {
                if(stopwatch >= duration * fireDarkThornTime && !HasFiredHand)
                {
                    HasFiredHand = true;

                    var handChildTransform = GetModelChildLocator().FindChild("HandL");

                    FireProjectileInfo darkThornProjectile = new FireProjectileInfo()
                    {
                        owner = characterBody.gameObject,
                        projectilePrefab = Aetherium.Survivors.Koalesk.KoaleskDarkThornProjectile,
                        speedOverride = 150.0f,
                        damage = characterBody.damage * 0.5f,
                        position = handChildTransform ? handChildTransform.position : characterBody.corePosition,
                        rotation = Util.QuaternionSafeLookRotation(inputBank.aimDirection),
                        procChainMask = default
                    };
                    ProjectileManager.instance.FireProjectile(darkThornProjectile);
                }
            }
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();

            if (!HasGrantedBuff && isAuthority)
            {
                HasGrantedBuff = true;
                Survivors.Koalesk.AddBloodliquorStacks(characterBody, 1, BloodliquorBuffDuration, 2);
            }
            
        }
    }
}
