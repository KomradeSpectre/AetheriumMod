using System;
using System.Collections.Generic;
using System.Text;
using static Aetherium.Interactables.BuffBrazier;
using static Aetherium.AetheriumPlugin;
using UnityEngine;
using BepInEx.Configuration;
using RoR2;
using UnityEngine.Networking;
using RoR2.Projectile;
using RoR2.Orbs;
using static Aetherium.Utils.ExtensionMethods;

namespace Aetherium.StandaloneBuffs.Tier3
{
    internal class SparkGap : BuffBase<SparkGap>
    {
        public override string BuffName => "Spark Gap";

        public override Color Color => new Color32(1, 234, 255, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("DoubleGoldDoubleXPBuffIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnBuffFirstStackGained += AddSparkController;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += RemoveSparkController;
        }

        private void AddSparkController(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            if(self && buffDef == BuffDef)
            {
                var component = self.GetComponent<SparkGapController>();
                if (!component)
                {
                    component = self.gameObject.AddComponent<SparkGapController>();
                    component.OwnerBody = self;
                    component.SparkGapBuff = buffDef;
                    component.SparkInterval = 1;
                }
            }
            orig(self, buffDef);
        }

        private void RemoveSparkController(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            if(self && buffDef == BuffDef)
            {
                var component = self.GetComponent<SparkGapController>();
                if (component)
                {
                    UnityEngine.Object.Destroy(component);
                }
            }
            orig(self, buffDef);
        }

        public class SparkGapController : NetworkBehaviour
        {
            public CharacterBody OwnerBody;
            public BuffDef SparkGapBuff;

            public float IntervalTimer;
            public float SparkInterval = 0.5f;
            public bool HasFiredSpark;
            public List<CharacterBody> HitEnemies = new List<CharacterBody>();

            public bool IncrementHitEnemies(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
            {
                var hurtbox = hitInfo.hitHurtBox;
                if (hurtbox)
                {
                    var healthComponent = hurtbox.healthComponent;
                    if (healthComponent)
                    {
                        var body = healthComponent.body;
                        if (body)
                        {
                            HitEnemies.Add(body);
                            return true;
                        }
                    }
                }
                return false;
            }

            public bool FilterTeammates(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
            {
                var hurtBox = hitInfo.hitHurtBox;
                if (hurtBox)
                {
                    var hurtBoxTeam = hurtBox.teamIndex;
                    var owner = bulletAttack.owner;
                    if (owner)
                    {
                        var ownerBody = owner.GetComponent<CharacterBody>();
                        if(ownerBody && ownerBody.teamComponent && ownerBody.teamComponent.teamIndex == hurtBoxTeam)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            public void FixedUpdate()
            {
                IntervalTimer += Time.fixedDeltaTime;
                if(IntervalTimer > SparkInterval)
                {
                    HasFiredSpark = false;
                    if (OwnerBody && OwnerBody.teamComponent && SparkGapBuff && !HasFiredSpark)
                    {
                        var teamMembers = TeamComponent.GetTeamMembers(OwnerBody.teamComponent.teamIndex);

                        foreach(var member in teamMembers)
                        {
                            if(member.body && member.body != OwnerBody && member.body.HasBuff(SparkGapBuff) && Vector3.Distance(OwnerBody.corePosition, member.body.corePosition) < 40)
                            {
                                HitEnemies.Clear();
                                BulletAttack bulletAttack = new BulletAttack();
                                bulletAttack.stopperMask = LayerIndex.world.mask;
                                bulletAttack.maxDistance = Vector3.Distance(OwnerBody.corePosition, member.body.corePosition);
                                bulletAttack.bulletCount = 1;
                                bulletAttack.force = 20f;
                                bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                                bulletAttack.origin = OwnerBody.corePosition;
                                bulletAttack.owner = OwnerBody.gameObject;
                                bulletAttack.filterCallback = FilterTeammates;
                                bulletAttack.hitCallback = IncrementHitEnemies;
                                bulletAttack.aimVector = member.body.corePosition - OwnerBody.corePosition;
                                bulletAttack.radius = 1f;
                                bulletAttack.tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerRailgunLight");
                                bulletAttack.hitEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniImpactVFXLightning");

                                bulletAttack.Fire();

                                foreach(CharacterBody body in HitEnemies)
                                {
                                    if(body && body.healthComponent)
                                    {
                                        LightningOrb lightningOrb = new LightningOrb();
                                        lightningOrb.bouncesRemaining = HitEnemies.Count;
                                        lightningOrb.bouncedObjects = new List<HealthComponent>() {body.healthComponent};
                                        lightningOrb.origin = body.corePosition;
                                        lightningOrb.damageValue = body.damage;
                                        lightningOrb.canBounceOnSameTarget = false;
                                        lightningOrb.attacker = OwnerBody.gameObject;
                                        lightningOrb.range += 2 * HitEnemies.Count;
                                        lightningOrb.teamIndex = OwnerBody.teamComponent.teamIndex;
                                        lightningOrb.lightningType = LightningOrb.LightningType.MageLightning;
                                        lightningOrb.procChainMask = default(ProcChainMask);
                                        lightningOrb.procChainMask.AddProc(ProcType.ChainLightning);
                                        lightningOrb.procCoefficient = 0.2f;

                                        var hurtBox = lightningOrb.PickNextTarget(body.corePosition);
                                        if (hurtBox)
                                        {
                                            lightningOrb.target = hurtBox;
                                            OrbManager.instance.AddOrb(lightningOrb);
                                        }
                                    }
                                }

                            }
                        }
                        HasFiredSpark = true;
                    }

                    IntervalTimer = 0;
                }
            }
        }
    }
}
