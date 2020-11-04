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

namespace Aetherium.EliteFocusedEquipment
{
    class TheirReminder : Equipment_V2<TheirReminder>
    {
        public override string displayName => "Their Reminder";

        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Attacks will detonate into lightning bolts.";

        protected override string GetDescString(string langID = null) => $"";

        protected override string GetLoreString(string langID = null) => $"";

        public static string EliteBuffName = "Affix_Hypercharged";
        public static string EliteBuffIconPath = "@Aetherium:Assets/Textures/Icons/TheirReminderBuffIcon.png";

        public static string ElitePrefixName = "Hypercharged";
        public static string EliteModifierString = "AETHERIUM_ELITE_MODIFIER_HYPERCHARGED";
        public static int EliteTier = 2;

        public static EliteAffixCard EliteCard { get; set; }
        public static EliteIndex EliteIndex;
        public static BuffIndex EliteBuffIndex;

        public static Material EliteMaterial;

        public static GameObject HyperchargedProjectile;

        public static Xoroshiro128Plus random = new Xoroshiro128Plus((ulong)System.DateTime.Now.Ticks);

        public TheirReminder()
        {
            modelResourcePath = "@Aetherium:Assets/Models/Prefabs/TheirReminder.prefab";
            iconResourcePath = "@Aetherium:Assets/Textures/Icons/TheirReminderIcon.png";
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
            EliteMaterial = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/TheirReminder.mat");

            HyperchargedProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/LightningStake"), "HyperchargedProjectile", true);

            var controller = HyperchargedProjectile.GetComponent<ProjectileController>();
            controller.startSound = "Play_titanboss_shift_shoot";

            var impactExplosion = HyperchargedProjectile.GetComponent<RoR2.Projectile.ProjectileImpactExplosion>();
            impactExplosion.lifetime = 0.5f;
            impactExplosion.impactEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/LightningStrikeImpact");
            impactExplosion.blastRadius = 7f;
            impactExplosion.bonusBlastForce = new Vector3(0, 750, 0);

            // register it for networking
            if (HyperchargedProjectile) PrefabAPI.RegisterNetworkPrefab(HyperchargedProjectile);

            // add it to the projectile catalog or it won't work in multiplayer 
            RoR2.ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(HyperchargedProjectile);
            };
        }

        public override void Install()
        {
            base.Install();

            On.RoR2.CharacterBody.FixedUpdate += AddEliteMaterials;
            On.RoR2.GlobalEventManager.OnHitAll += SpawnLightningPillar;
        }

        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.CharacterBody.FixedUpdate -= AddEliteMaterials;
            On.RoR2.GlobalEventManager.OnHitAll -= SpawnLightningPillar;
        }

        //Sourced from source code, couldn't access because it was private, modified a little
        private Vector3? RaycastToFloor(Vector3 position, float maxDistance)
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(new Ray(position, Vector3.down), out raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                return raycastHit.point;
            }
            return null;
        }

        protected override bool PerformEquipmentAction(RoR2.EquipmentSlot slot)
        {
            if (!slot.characterBody) { return false; }

            var body = slot.characterBody;
            for(int i = 1; i <= 16; i++)
            {
                var newProjectileInfo = new FireProjectileInfo();
                newProjectileInfo.owner = body.gameObject;
                newProjectileInfo.projectilePrefab = HyperchargedProjectile;
                newProjectileInfo.speedOverride = 150.0f;
                newProjectileInfo.damage = body.damage;
                newProjectileInfo.damageTypeOverride = null;
                newProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                newProjectileInfo.procChainMask = default(RoR2.ProcChainMask);
                var theta = (Math.PI * 2) / 16;
                var angle = theta * i;
                var radius = 20 + random.RangeFloat(-15, 15);
                var positionChosen = new Vector3((float)(radius * Math.Cos(angle) + body.corePosition.x), body.corePosition.y + 1, (float)(radius * Math.Sin(angle) + body.corePosition.z));
                var raycastedChosen = RaycastToFloor(positionChosen, 1000f);
                if(raycastedChosen != null)
                {
                    positionChosen = raycastedChosen.Value;
                }
                newProjectileInfo.position = positionChosen;
                newProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(positionChosen + Vector3.down);
                ProjectileManager.instance.FireProjectile(newProjectileInfo);
            }
            return true;
        }

        private void SpawnLightningPillar(On.RoR2.GlobalEventManager.orig_OnHitAll orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, GameObject hitObject)
        {
            if (damageInfo.procCoefficient == 0f || damageInfo.rejected) { return; }

            var attacker = damageInfo.attacker;
            if (attacker)
            {
                var body = attacker.GetComponent<CharacterBody>();
                if (body)
                {
                    if (body.HasBuff(EliteBuffIndex))
                    {
                        float damageCoefficient2 = 0.5f;
                        float damage = Util.OnHitProcDamage(damageInfo.damage, body.damage, damageCoefficient2);
                        float force = 0f;
                        Vector3 position = damageInfo.position;
                        ProjectileManager.instance.FireProjectile(HyperchargedProjectile, position, Quaternion.identity, damageInfo.attacker, damage, force, damageInfo.crit, DamageColorIndex.Item, null, -1f);
                    }
                }
            }
        }

        private void AddEliteMaterials(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (self.HasBuff(EliteBuffIndex) && !self.GetComponent<HyperchargedBuffTracker>())
            {
                var modelLocator = self.modelLocator;
                if (modelLocator)
                {
                    var modelTransform = self.modelLocator.modelTransform;
                    if (modelTransform)
                    {
                        var model = self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>();
                        if (model) {
                            var hyperchargedBuffTracker = self.gameObject.AddComponent<HyperchargedBuffTracker>();
                            hyperchargedBuffTracker.Body = self;
                            TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                            overlay.duration = float.PositiveInfinity;
                            overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                            overlay.animateShaderAlpha = true;
                            overlay.destroyComponentOnEnd = true;
                            overlay.originalMaterial = EliteMaterial;
                            overlay.AddToCharacerModel(model);
                            hyperchargedBuffTracker.Overlay = overlay;
                        }
                    }
                }
            }
            orig(self);
        }


        public class HyperchargedBuffTracker : MonoBehaviour
        {
            public RoR2.TemporaryOverlay Overlay;
            public RoR2.CharacterBody Body;

            public void FixedUpdate()
            {
                if (!Body.HasBuff(EliteBuffIndex))
                {
                    UnityEngine.Object.Destroy(Overlay);
                    UnityEngine.Object.Destroy(this);
                }
            }
        }
    }
}
