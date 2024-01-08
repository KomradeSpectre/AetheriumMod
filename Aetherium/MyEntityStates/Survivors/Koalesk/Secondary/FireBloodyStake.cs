using EntityStates;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Aetherium.Utils;

namespace Aetherium.MyEntityStates.Survivors.Koalesk
{
    internal class FireBloodyStake : BaseState
    {
        public int ProjectilesToGenerate;

        public override void OnEnter()
        {
            base.OnEnter();

            var vectorsToSpawnProjectiles = Utils.MathHelpers.DistributePointsEvenlyOnSphereCap(0.25f * ProjectilesToGenerate, 1, characterBody.corePosition + inputBank.aimDirection, RoR2.Util.QuaternionSafeLookRotation(inputBank.aimDirection));
            for (int i = 1; i <= ProjectilesToGenerate; i++)
            {
                var hasValidAim = inputBank.GetAimRaycast(500, out var rayCastHit);
                FireProjectileInfo Stake = new FireProjectileInfo()
                {
                    owner = characterBody.gameObject,
                    projectilePrefab = Aetherium.Items.BlasterSword.SwordProjectile,
                    speedOverride = 150.0f,
                    damage = 50 * ProjectilesToGenerate,
                    damageTypeOverride = null,
                    damageColorIndex = DamageColorIndex.Default,
                    position = vectorsToSpawnProjectiles[i - 1],
                    rotation = RoR2.Util.QuaternionSafeLookRotation(hasValidAim ? rayCastHit.point - vectorsToSpawnProjectiles[i - 1] : inputBank.GetAimRay().direction),
                    procChainMask = default
                };
                ProjectileManager.instance.FireProjectile(Stake);
            }

            outer.SetNextStateToMain();
        }
    }
}
