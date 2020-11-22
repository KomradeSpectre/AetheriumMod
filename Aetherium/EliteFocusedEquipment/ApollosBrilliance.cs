using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using TILER2;
using EliteSpawningOverhaul;
using UnityEngine;
using R2API;
using R2API.Utils;
using RoR2.Projectile;
using Mono.Cecil;
using Aetherium.Utils;
using UnityEngine.Networking;
using TMPro;

namespace Aetherium.EliteFocusedEquipment
{
    class ApollosBrilliance : Equipment_V2<ApollosBrilliance>
    {
        public override string displayName => "Apollo's Brilliance";

        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Become an aspect of light.";

        protected override string GetDescString(string langID = null) => $"Enemies that are not in cover in a radius around you will be set on fire. Attackers outside of this radius will have a solar pillar form on them after a short delay.";

        protected override string GetLoreString(string langID = null) => $"";

        public static string EliteBuffName = "Affix_Radiant";
        public static string EliteBuffIconPath = "@Aetherium:Assets/Textures/Icons/ApollosBrillianceBuffIcon.png";

        public static string ElitePrefixName = "Radiant";
        public static string EliteModifierString = "AETHERIUM_ELITE_MODIFIER_RADIANT";
        public static int EliteTier = 2;

        public static EliteAffixCard EliteCard { get; set; }
        public static EliteIndex EliteIndex;
        public static BuffIndex EliteBuffIndex;

        public static Material EliteMaterial;

        public static GameObject RadiantProjectile;

        public static Xoroshiro128Plus random = new Xoroshiro128Plus((ulong)System.DateTime.Now.Ticks);

        public ApollosBrilliance()
        {
            modelResourcePath = "";
            iconResourcePath = "";
        }

        public override void SetupAttributes()
        {

            base.SetupAttributes();
            equipmentDef.canDrop = false;
            equipmentDef.enigmaCompatible = false;
            equipmentDef.cooldown = 40;

            var buffDef = new RoR2.BuffDef
            {
                name = EliteBuffName,
                buffColor = new Color32(255, 255, 255, byte.MaxValue),
                iconPath = EliteBuffIconPath,
                canStack = false,
            };
            buffDef.eliteIndex = EliteIndex;
            var buffIndex = new CustomBuff(buffDef);
            EliteBuffIndex = BuffAPI.Add(buffIndex);
            equipmentDef.passiveBuff = EliteBuffIndex;

            var eliteDef = new RoR2.EliteDef
            {
                name = ElitePrefixName,
                modifierToken = EliteModifierString,
                color = buffDef.buffColor,
            };
            eliteDef.eliteEquipmentIndex = equipmentDef.equipmentIndex;
            var eliteIndex = new CustomElite(eliteDef, EliteTier);
            EliteIndex = EliteAPI.Add(eliteIndex);

            var card = new EliteAffixCard
            {
                spawnWeight = 0.5f,
                costMultiplier = 30.0f,
                damageBoostCoeff = 2.0f,
                healthBoostCoeff = 4.5f,
                eliteOnlyScaling = 0.5f,
                eliteType = EliteIndex
            };
            EsoLib.Cards.Add(card);
            EliteCard = card;

            LanguageAPI.Add(eliteDef.modifierToken, ElitePrefixName + " {0}");

            RadiantProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/Thermite"), "RadiantProjectile", true);

            var damage = RadiantProjectile.GetComponent<RoR2.Projectile.ProjectileDamage>();
            damage.damageType = DamageType.IgniteOnHit;

            var simpleProjectile = RadiantProjectile.GetComponent<ProjectileSimple>();
            simpleProjectile.velocity = 100;
            simpleProjectile.oscillateMagnitude = 0;

            if (RadiantProjectile) PrefabAPI.RegisterNetworkPrefab(RadiantProjectile);

            RoR2.ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(RadiantProjectile);
            };

            //If we want to load a base game material, then we use this.
            /*GameObject worm = Resources.Load<GameObject>("Prefabs/characterbodies/ElectricWormBody");
            Debug.Log($"WORM: {worm}");
            var modelLocator = worm.GetComponent<ModelLocator>();
            Debug.Log($"MODEL LOCATOR: {modelLocator}");
            var model = modelLocator.modelTransform.GetComponent<CharacterModel>();
            Debug.Log($"MODEL: {model}");
            if (model)
            {
                var rendererInfos = model.baseRendererInfos;
                foreach (CharacterModel.RendererInfo renderer in rendererInfos)
                {
                    if (renderer.defaultMaterial.name == "matElectricWorm")
                    {
                        HyperchargedMaterial = renderer.defaultMaterial;
                    }
                }
            }*/

            //If we want to load our own, uncomment the one below.
            EliteMaterial = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/ApollosBrillianceMain.mat");
            //Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/hgstandard");


        }

        public override void Install()
        {
            base.Install();

            On.RoR2.CharacterBody.FixedUpdate += AddEliteMaterials;
            On.RoR2.HealthComponent.TakeDamage += PunishHeretics;
        }

        private void PunishHeretics(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            if(damageInfo == null || damageInfo.rejected) { return; }

            var body = self.body;
            if (body && body.HasBuff(EliteBuffIndex) && damageInfo.attacker)
            {
                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if(attackerBody && (attackerBody.corePosition - body.corePosition).sqrMagnitude > 40 * 40)
                {
                    var Ray = new Ray(attackerBody.corePosition, Vector3.up);
                    RaycastHit HitResults;
                    var RayHit = Util.CharacterRaycast(damageInfo.attacker, Ray, out HitResults, 1000, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
                    var newProjectileInfo = new FireProjectileInfo();
                    newProjectileInfo.owner = body.gameObject;
                    newProjectileInfo.projectilePrefab = RadiantProjectile;
                    newProjectileInfo.damage = body.damage * 4;
                    newProjectileInfo.procChainMask = default(RoR2.ProcChainMask);
                    if (RayHit)
                    {
                        Debug.Log("Terrain Detected");
                        newProjectileInfo.position = HitResults.point;
                        newProjectileInfo.rotation = Util.QuaternionSafeLookRotation(Vector3.down);
                    }
                    else
                    {
                        Debug.Log("Sky Detected");
                        newProjectileInfo.position = attackerBody.corePosition + (Vector3.up * 100);
                        newProjectileInfo.rotation = Util.QuaternionSafeLookRotation(Vector3.down);
                    }
                    if (NetworkServer.active) 
                    {
                        EffectManager.SimpleEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/ExplosionVFX"), newProjectileInfo.position, newProjectileInfo.rotation, true);
                    }
                    ProjectileManager.instance.FireProjectile(newProjectileInfo);
                }
            }
            orig(self, damageInfo);
        }

        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.CharacterBody.FixedUpdate -= AddEliteMaterials;
            On.RoR2.HealthComponent.TakeDamage -= PunishHeretics;
        }

        protected override bool PerformEquipmentAction(RoR2.EquipmentSlot slot)
        {
            if (!slot.characterBody) { return false; }
            var body = slot.characterBody;

            if (NetworkServer.active)
            {
                var supernova = new RoR2.BlastAttack()
                {
                    radius = 40,
                    procCoefficient = 0.25f,
                    position = body.corePosition,
                    attacker = body.gameObject,
                    crit = RoR2.Util.CheckRoll(body.crit, body.master),
                    baseDamage = body.damage * 4,
                    falloffModel = RoR2.BlastAttack.FalloffModel.None,
                    baseForce = body.damage * 100,
                    teamIndex = !body.teamComponent.teamIndex.Equals(TeamIndex.None) ? body.teamComponent.teamIndex : TeamIndex.None,
                    damageType = DamageType.IgniteOnHit,
                    attackerFiltering = AttackerFiltering.NeverHit
                };
                supernova.Fire();
                RoR2.EffectData effectData = new RoR2.EffectData();
                effectData.origin = body.corePosition;
                effectData.scale = 40f;
                RoR2.EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/ExplosionVFX"), effectData, true);
                return true;
            }
            return false;
        }

        private void AddEliteMaterials(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (self.HasBuff(EliteBuffIndex) && !self.GetComponent<RadiantBuffTracker>())
            {
                var modelLocator = self.modelLocator;
                if (modelLocator)
                {
                    var modelTransform = self.modelLocator.modelTransform;
                    if (modelTransform)
                    {
                        var model = self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>();
                        if (model)
                        {
                            var radiantBuffTracker = self.gameObject.AddComponent<RadiantBuffTracker>();
                            radiantBuffTracker.Body = self;
                            RoR2.TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                            overlay.duration = float.PositiveInfinity;
                            overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                            overlay.animateShaderAlpha = true;
                            overlay.destroyComponentOnEnd = true;
                            overlay.originalMaterial = EliteMaterial;
                            overlay.AddToCharacerModel(model);
                            radiantBuffTracker.Overlay = overlay;
                            radiantBuffTracker.PulseTimer = 1;
                        }
                    }
                }
            }
            orig(self);
        }


        public class RadiantBuffTracker : MonoBehaviour
        {
            public RoR2.TemporaryOverlay Overlay;
            public RoR2.CharacterBody Body;
            public float PulseTimer;

            public void FixedUpdate()
            {
                if (!Body.HasBuff(EliteBuffIndex))
                {
                    UnityEngine.Object.Destroy(Overlay);
                    UnityEngine.Object.Destroy(this);
                }
                if (PulseTimer > 0)
                {
                    PulseTimer -= Time.fixedDeltaTime;
                    if (PulseTimer <= 0)
                    {
                        RoR2.TeamMask enemyTeams = RoR2.TeamMask.GetEnemyTeams(Body.teamComponent.teamIndex);
                        RoR2.HurtBox[] hurtBoxes = new RoR2.SphereSearch
                        {
                            radius = 40,
                            mask = RoR2.LayerIndex.entityPrecise.mask,
                            origin = Body.corePosition
                        }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();
                        foreach (RoR2.HurtBox enemyHurtbox in hurtBoxes)
                        {
                            var enemyHealthComponent = enemyHurtbox.healthComponent;
                            if (enemyHealthComponent)
                            {
                                var enemyBody = enemyHealthComponent.body;
                                if (enemyBody)
                                {
                                    RaycastHit raycastHit;
                                    var rayCast = RoR2.Util.CharacterRaycast(Body.gameObject, new Ray(Body.corePosition, enemyBody.corePosition - Body.corePosition), out raycastHit, 40, RoR2.LayerIndex.entityPrecise.mask | RoR2.LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
                                    if (rayCast && raycastHit.collider)
                                    {
                                        var hurtbox = raycastHit.collider.GetComponent<RoR2.HurtBox>();
                                        if (hurtbox && hurtbox.healthComponent)
                                        {
                                            var body = hurtbox.healthComponent.body;
                                            if(body == enemyBody)
                                            {
                                                if (NetworkServer.active)
                                                {
                                                    RoR2.EffectManager.SimpleEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/IgniteExplosionVFX"), enemyBody.corePosition, RoR2.Util.QuaternionSafeLookRotation(Body.corePosition - enemyBody.corePosition), true);
                                                }
                                                RoR2.DotController.InflictDot(enemyBody.gameObject, Body.gameObject, RoR2.DotController.DotIndex.Burn, 2);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        PulseTimer = 1;
                    }
                }
            }
        }
    }
}
