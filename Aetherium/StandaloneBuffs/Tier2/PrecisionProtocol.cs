using BepInEx.Configuration;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Aetherium.Interactables.BuffBrazier;
using static Aetherium.AetheriumPlugin;
using System.Linq;
using Aetherium.Utils.Components;

namespace Aetherium.StandaloneBuffs.Tier2
{
    internal class PrecisionProtocol : BuffBase<PrecisionProtocol>
    {
        public override string BuffName => "Precision Protocol";
        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("FeatheredPlumeIcon.png");
        public override Color Color => new Color(255, 0, 255);

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.BulletAttack.Fire += ReduceBulletAttackSpreadAndAddCrit;
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += NormalizeSpeedAndAddCrit;
        }

        private void ReduceBulletAttackSpreadAndAddCrit(On.RoR2.BulletAttack.orig_Fire orig, RoR2.BulletAttack self)
        {
            if(self.owner)
            {
                var body = self.owner.GetComponent<CharacterBody>();
                if(body && body.HasBuff(BuffDef))
                {
                    self.minSpread = 0;
                    self.maxSpread = 0;
                    self.spreadPitchScale = 0;
                    self.spreadYawScale = 0;
                    self.isCrit = true;
                }
            }

            orig(self);
        }

        private void NormalizeSpeedAndAddCrit(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, RoR2.Projectile.ProjectileManager self, FireProjectileInfo fireProjectileInfo)
        {
            if (fireProjectileInfo.owner)
            {
                var body = fireProjectileInfo.owner.GetComponent<CharacterBody>();
                if (body && body.HasBuff(BuffDef) && body.teamComponent)
                {
                    if (fireProjectileInfo.projectilePrefab)
                    {
                        fireProjectileInfo.useSpeedOverride = true;
                        fireProjectileInfo.speedOverride = 150f;
                        fireProjectileInfo.crit = true;
                    }
                }
            }

            orig(self, fireProjectileInfo);
        }
    }
}
