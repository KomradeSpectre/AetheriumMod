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

namespace Aetherium.EliteFocusedEquipment
{
    class ApollosBrilliance : Equipment_V2<ApollosBrilliance>
    {
        public override string displayName => "Apollo's Brilliance";

        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Enemies that are not in cover in a radius around you will be set on fire. Attackers outside of this radius will have a solar pillar form on them after a short delay.";

        protected override string GetDescString(string langID = null) => $"";

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
            equipmentDef.cooldown = 60;

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
            //EliteMaterial.shader = hotpoo;
            //EliteMaterial = new Material(Shader.Find("Standard"));


        }

        public override void Install()
        {
            base.Install();

            On.RoR2.CharacterBody.FixedUpdate += AddEliteMaterials;
        }

        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.CharacterBody.FixedUpdate -= AddEliteMaterials;
        }

        //Sourced from source code, couldn't access because it was private, modified a little

        protected override bool PerformEquipmentAction(RoR2.EquipmentSlot slot)
        {
            if (!slot.characterBody) { return false; }
            var body = slot.characterBody;
            return true;
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
            public TemporaryOverlay Overlay;
            public CharacterBody Body;
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
                        TeamMask enemyTeams = RoR2.TeamMask.GetEnemyTeams(Body.teamComponent.teamIndex);
                        HurtBox[] hurtBoxes = new RoR2.SphereSearch
                        {
                            radius = 40,
                            mask = RoR2.LayerIndex.entityPrecise.mask,
                            origin = Body.corePosition
                        }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();
                        foreach (HurtBox enemyHurtbox in hurtBoxes)
                        {
                            var enemyHealthComponent = enemyHurtbox.healthComponent;
                            if (enemyHealthComponent)
                            {
                                var enemyBody = enemyHealthComponent.body;
                                if (enemyBody)
                                {
                                    RaycastHit raycastHit;
                                    var rayCast = Util.CharacterRaycast(Body.gameObject, new Ray(Body.corePosition, enemyBody.corePosition - Body.corePosition), out raycastHit, 40, LayerIndex.entityPrecise.mask | LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
                                    if (rayCast && raycastHit.collider)
                                    {
                                        var hurtbox = raycastHit.collider.GetComponent<HurtBox>();
                                        if (hurtbox && hurtbox.healthComponent)
                                        {
                                            var body = hurtbox.healthComponent.body;
                                            if(body == enemyBody)
                                            {
                                                if (NetworkServer.active)
                                                {
                                                    EffectManager.SimpleEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/IgniteExplosionVFX"), enemyBody.corePosition, Util.QuaternionSafeLookRotation(Body.corePosition - enemyBody.corePosition), true);
                                                }
                                                DotController.InflictDot(enemyBody.gameObject, Body.gameObject, DotController.DotIndex.Burn, 2);
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
