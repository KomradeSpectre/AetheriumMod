using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using System.Linq;
using static Aetherium.AetheriumPlugin;
using UnityEngine.Networking;
using RoR2.Orbs;
using R2API;

namespace Aetherium.StandaloneBuffs
{
    public class SoulLinked : BuffBase<SoulLinked>
    {
        public override string BuffName => "Soul Linked";

        public override Color Color => new Color32(125, 75, 255, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("SoulLinkedDebuffIcon.png");

        public override bool IsDebuff => true;

        public static bool RecursionPrevention;

        public static GameObject SoulLinkedOrbEffect;
        public static GameObject SoulLinkedHeartEffect;

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            CreateOrb();
            Hooks();
        }

        private void CreateOrb()
        {
            SoulLinkedHeartEffect = MainAssets.LoadAsset<GameObject>("SoulLinkedHeartEffect.prefab");

            var heartEffectComponent = SoulLinkedHeartEffect.AddComponent<EffectComponent>();
            heartEffectComponent.parentToReferencedTransform = true;

            var heartVfxAttributes = SoulLinkedHeartEffect.AddComponent<VFXAttributes>();
            heartVfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
            heartVfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Medium;

            SoulLinkedHeartEffect.AddComponent<NetworkIdentity>();

            if (SoulLinkedHeartEffect) PrefabAPI.RegisterNetworkPrefab(SoulLinkedHeartEffect);
            ContentAddition.AddEffect(SoulLinkedHeartEffect);

            SoulLinkedOrbEffect = MainAssets.LoadAsset<GameObject>("SoulLinkedTrailEffect.prefab");

            var effectComponent = SoulLinkedOrbEffect.AddComponent<EffectComponent>();

            var vfxAttributes = SoulLinkedOrbEffect.AddComponent<VFXAttributes>();
            vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
            vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Medium;

            SoulLinkedOrbEffect.AddComponent<NetworkIdentity>();

            var orbEffect = SoulLinkedOrbEffect.AddComponent<OrbEffect>();
            orbEffect.startVelocity1 = new Vector3(-10, 10, -10);
            orbEffect.startVelocity2 = new Vector3(10, 13, 10);
            orbEffect.endVelocity1 = new Vector3(-10, 0, -10);
            orbEffect.endVelocity2 = new Vector3(10, 5, 10);
            orbEffect.movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            var trailDetach = SoulLinkedOrbEffect.AddComponent<DetachTrailOnDestroy>();
            trailDetach.targetTrailRenderers = SoulLinkedOrbEffect.GetComponents<TrailRenderer>();

            if (SoulLinkedOrbEffect) PrefabAPI.RegisterNetworkPrefab(SoulLinkedOrbEffect);
            ContentAddition.AddEffect(SoulLinkedOrbEffect);

            OrbAPI.AddOrb(typeof(Effect.SoulLinkedOrb));
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += SpreadDamage;
        }

        private void SpreadDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            if(!RecursionPrevention && self.alive && self.body && self.body.HasBuff(BuffDef))
            {
                RecursionPrevention = true;
                var teamMembers = TeamComponent.GetTeamMembers(self.body.teamComponent.teamIndex).Where(x => x.body != self.body);

                if(teamMembers.Any(x => x.body && x.body.HasBuff(BuffDef)))
                {
                    var effectData = new EffectData
                    {
                        origin = self.body.corePosition,
                        rotation = Quaternion.identity,
                        scale = self.body.bestFitRadius,
                        rootObject = self.body.gameObject
                    };
                    EffectManager.SpawnEffect(SoulLinkedHeartEffect, effectData, true);
                }

                foreach(TeamComponent teamComponent in teamMembers)
                {
                    if(teamComponent.body && teamComponent.body.HasBuff(BuffDef))
                    {
                        DamageInfo clonedDamageInfo = new DamageInfo()
                        {
                            damage = damageInfo.damage * 0.05f,
                            damageType = DamageType.Generic,
                            damageColorIndex = DamageColorIndex.Default,
                            procCoefficient = 0,
                            position = teamComponent.body.corePosition
                        };
                        teamComponent.body.healthComponent.TakeDamage(clonedDamageInfo);

                        var orb = new Effect.SoulLinkedOrb()
                        {
                            Target = teamComponent.body.gameObject,
                            origin = self.body.corePosition
                        };
                        OrbManager.instance.AddOrb(orb);
                    }
                }
                RecursionPrevention = false;
            }
            orig(self, damageInfo);
        }
    }
}
