using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Equipment.EliteEquipment
{
    class AffixMagnetic : EliteEquipmentBase<AffixMagnetic>
    {
        public override string EliteEquipmentName => "The Unseen Hand";

        public override string EliteAffixToken => "AFFIX_MAGNETIC";

        public override string EliteEquipmentPickupDesc => "Become an aspect of magnetism.";

        public override string EliteEquipmentFullDescription => "";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Magnetic";

        public override GameObject EliteEquipmentModel => new GameObject();

        public override Sprite EliteEquipmentIcon => null;

        public override Sprite EliteBuffIcon => null;

        public static GameObject FieldEffect;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateEffect();
            CreateEquipment();
            CreateElite();
            Hooks();
        }

        private void CreateEffect()
        {
            FieldEffect = PrefabAPI.InstantiateClone(MainAssets.LoadAsset<GameObject>("BuffBrazierActiveField.prefab"), "FieldEffect", true);
            FieldEffect.AddComponent<NetworkIdentity>();

            var chargeSphereEffectComponent = FieldEffect.AddComponent<RoR2.EffectComponent>();
            chargeSphereEffectComponent.parentToReferencedTransform = true;
            chargeSphereEffectComponent.positionAtReferencedTransform = true;

            var chargeSphereTimer = FieldEffect.AddComponent<RoR2.DestroyOnTimer>();
            chargeSphereTimer.duration = 1f;

            var chargeSphereVfxAttributes = FieldEffect.AddComponent<RoR2.VFXAttributes>();
            chargeSphereVfxAttributes.vfxIntensity = RoR2.VFXAttributes.VFXIntensity.Low;
            chargeSphereVfxAttributes.vfxPriority = RoR2.VFXAttributes.VFXPriority.Medium;

            var scaleCurve = FieldEffect.AddComponent<ObjectScaleCurve>();
            scaleCurve.useOverallCurveOnly = true;
            scaleCurve.overallCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1f, 1));

            EffectAPI.AddEffect(FieldEffect);

            PrefabAPI.RegisterNetworkPrefab(FieldEffect);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnEquipmentGained += GiveMagneticController;
            On.RoR2.CharacterBody.OnEquipmentLost += RemoveMagneticController;
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }

        private void GiveMagneticController(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);
            if (equipmentDef == EliteEquipmentDef)
            {
                var magneticController = self.GetComponent<AffixMagneticController>();
                if (!magneticController)
                {
                    magneticController = self.gameObject.AddComponent<AffixMagneticController>();
                    magneticController.Body = self;
                }
            }
        }

        private void RemoveMagneticController(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EliteEquipmentDef)
            {
                var magneticController = self.GetComponent<AffixMagneticController>();
                if (magneticController)
                {
                    UnityEngine.Object.Destroy(magneticController);
                }
            }
            orig(self, equipmentDef);
        }

        public class AffixMagneticController : MonoBehaviour
        {
            public CharacterBody Body;
            public bool Repel;
            public float Stopwatch;
            public float TimeBetweenPulses = 1f;

            public void Start()
            {
                if (Run.instance)
                {
                    Repel = Run.instance.stageRng.nextBool;
                }
            }

            public void FixedUpdate()
            {
                Stopwatch += Time.fixedDeltaTime;
                if(Stopwatch >= TimeBetweenPulses)
                {
                    if (Body)
                    {
                        RoR2.TeamMask enemyTeams = RoR2.TeamMask.GetEnemyTeams(Body.teamComponent.teamIndex);
                        RoR2.HurtBox[] hurtBoxes = new RoR2.SphereSearch
                        {
                            radius = 20,
                            mask = RoR2.LayerIndex.entityPrecise.mask,
                            origin = Body.corePosition
                        }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                        foreach(HurtBox hurtBox in hurtBoxes)
                        {
                            if(hurtBox.healthComponent)
                            {
                                var hit = Util.CharacterRaycast(Body.gameObject, new Ray(Body.corePosition, hurtBox.transform.position - Body.corePosition), out RaycastHit raycastHit, Vector3.Distance(Body.corePosition, hurtBox.transform.position), LayerIndex.world.mask, QueryTriggerInteraction.Ignore);

                                if (!hit)
                                {
                                    var rigidBody = Body.rigidbody;
                                    if (rigidBody)
                                    {
                                        if (Repel)
                                        {
                                            var damageInfo = new DamageInfo();
                                            damageInfo.damage = 0;
                                            damageInfo.force = (Body.corePosition - hurtBox.transform.position) * 40;
                                            hurtBox.healthComponent.TakeDamage(damageInfo);
                                        }
                                        else
                                        {
                                            var damageInfo = new DamageInfo();
                                            damageInfo.damage = 0;
                                            damageInfo.force = (hurtBox.transform.position - Body.corePosition) * 40;
                                            hurtBox.healthComponent.TakeDamage(damageInfo);
                                        }
                                    }
                                }
                            }
                        }

                        if (Repel)
                        {
                            var effectData = new EffectData()
                            {
                                color = Color.red,
                                origin = Body.corePosition,
                            };
                            EffectManager.SpawnEffect(FieldEffect, effectData, true);
                        }
                        else
                        {
                            var effectData = new EffectData()
                            {
                                color = Color.blue,
                                origin = Body.corePosition,
                            };
                            EffectManager.SpawnEffect(FieldEffect, effectData, true);
                        }
                    }
                    Stopwatch = 0;
                }
            }

        }
    }
}
