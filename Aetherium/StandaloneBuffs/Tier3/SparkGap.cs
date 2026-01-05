using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BepInEx.Configuration;
using RoR2;
using UnityEngine.Networking;
using RoR2.Orbs;
using static Aetherium.AetheriumPlugin;
using Aetherium.StandaloneBuffs; // Assuming namespace match

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
            orig(self, buffDef);
            if (NetworkServer.active && self && buffDef == BuffDef) // Only add controller on Server
            {
                var component = self.GetComponent<SparkGapController>();
                if (!component)
                {
                    component = self.gameObject.AddComponent<SparkGapController>();
                    component.OwnerBody = self;
                    component.SparkGapBuff = buffDef;
                }
            }
        }

        private void RemoveSparkController(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (NetworkServer.active && self && buffDef == BuffDef)
            {
                var component = self.GetComponent<SparkGapController>();
                if (component)
                {
                    UnityEngine.Object.Destroy(component);
                }
            }
        }

        public class SparkGapController : NetworkBehaviour
        {
            public CharacterBody OwnerBody;
            public BuffDef SparkGapBuff;

            public float IntervalTimer;
            public float SparkInterval = 1f; // Increased slightly to prevent spam lag

            // Cached lists/objects to avoid GC allocation
            private List<CharacterBody> _hitEnemiesBuffer = new List<CharacterBody>();
            private BulletAttack _bulletAttack;
            private LightningOrb _lightningOrb;

            public void Start()
            {
                // Initialize cached objects once
                _bulletAttack = new BulletAttack
                {
                    stopperMask = LayerIndex.world.mask,
                    falloffModel = BulletAttack.FalloffModel.None,
                    force = 20f,
                    bulletCount = 1,
                    radius = 1f,
                    tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerRailgunLight"),
                    hitEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniImpactVFXLightning"),
                    filterCallback = FilterTeammates,
                    hitCallback = IncrementHitEnemies
                };

                _lightningOrb = new LightningOrb
                {
                    lightningType = LightningOrb.LightningType.MageLightning,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = 0.2f
                };
                _lightningOrb.procChainMask.AddProc(ProcType.ChainLightning);
            }

            public bool IncrementHitEnemies(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
            {
                var hurtbox = hitInfo.hitHurtBox;
                if (hurtbox && hurtbox.healthComponent && hurtbox.healthComponent.body)
                {
                    _hitEnemiesBuffer.Add(hurtbox.healthComponent.body);
                    return true;
                }
                return false;
            }

            public bool FilterTeammates(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
            {
                // Default filter works, but we add custom logic to be safe
                if (!hitInfo.hitHurtBox) return false;

                // Don't hit ourselves or the target we are connecting to
                // (Note: The connection target is technically a teammate, so we filter by team)
                if (hitInfo.hitHurtBox.teamIndex == OwnerBody.teamComponent.teamIndex) return false;

                return true;
            }

            public void FixedUpdate()
            {
                // CRITICAL: Only run damage logic on the server
                if (!NetworkServer.active || !OwnerBody) return;

                IntervalTimer += Time.fixedDeltaTime;
                if (IntervalTimer > SparkInterval)
                {
                    // Scan for teammates
                    // Optimization: GetTeamMembers is cheaper than SphereSearch for small teams (players), 
                    // but expensive for drone armies.
                    var teamMembers = TeamComponent.GetTeamMembers(OwnerBody.teamComponent.teamIndex);

                    foreach (var member in teamMembers)
                    {
                        if (!member.body || member.body == OwnerBody) continue;

                        // CONNECTION LOGIC:
                        // 1. Must have buff
                        // 2. Must be in range
                        // 3. (Crucial) My NetID < Their NetID. 
                        //    This prevents A firing at B AND B firing at A. Only the "smaller" ID fires.
                        if (member.body.HasBuff(SparkGapBuff) &&
                            Vector3.Distance(OwnerBody.corePosition, member.body.corePosition) < 40 &&
                            OwnerBody.netId.Value < member.body.netId.Value)
                        {
                            FireSparkConnection(member.body);
                        }
                    }
                    IntervalTimer = 0;
                }
            }

            private void FireSparkConnection(CharacterBody targetBody)
            {
                _hitEnemiesBuffer.Clear();

                // Setup BulletAttack from cache
                _bulletAttack.origin = OwnerBody.corePosition;
                _bulletAttack.owner = OwnerBody.gameObject;
                _bulletAttack.aimVector = targetBody.corePosition - OwnerBody.corePosition;
                _bulletAttack.maxDistance = Vector3.Distance(OwnerBody.corePosition, targetBody.corePosition);

                // Fire (Triggers IncrementHitEnemies callback)
                _bulletAttack.Fire();

                // Process hits
                if (_hitEnemiesBuffer.Count > 0)
                {
                    foreach (var body in _hitEnemiesBuffer)
                    {
                        if (body && body.healthComponent)
                        {
                            // Setup LightningOrb from cache
                            _lightningOrb.origin = body.corePosition;
                            _lightningOrb.attacker = OwnerBody.gameObject;
                            _lightningOrb.teamIndex = OwnerBody.teamComponent.teamIndex;
                            _lightningOrb.damageValue = OwnerBody.damage * 2f; // 200% damage per spark
                            _lightningOrb.bouncesRemaining = 2; // Fixed bounces to prevent infinite chains
                            _lightningOrb.range = 20f; // Reset range default

                            // IMPORTANT: We must explicitly tell the orb "Don't bounce back to the guy we just hit"
                            _lightningOrb.bouncedObjects = new List<HealthComponent> { body.healthComponent };

                            // Find a target nearby the hit enemy
                            var hurtBox = _lightningOrb.PickNextTarget(body.corePosition);
                            if (hurtBox)
                            {
                                _lightningOrb.target = hurtBox;
                                OrbManager.instance.AddOrb(_lightningOrb);
                            }
                        }
                    }
                }
            }
        }
    }
}