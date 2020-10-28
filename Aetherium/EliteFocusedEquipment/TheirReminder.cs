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

        public static EliteAffixCard HyperchargedEliteCard { get; set; }
        public static EliteIndex HyperchargedEliteIndex;
        public static BuffIndex HyperchargedBuffIndex;

        public static Material HyperchargedMaterial;

        public static GameObject HyperchargedProjectile;

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


            var hyperchargedEliteIndex = new CustomElite(
            new RoR2.EliteDef
            {
                name = "Hypercharged",
                modifierToken = "AETHERIUM_ELITE_MODIFIER_HYPERCHARGED",
                color = new Color32(128, 128, 128, 255),
                eliteEquipmentIndex = equipmentDef.equipmentIndex
            }, 1);
            HyperchargedEliteIndex = EliteAPI.Add(hyperchargedEliteIndex);
            LanguageAPI.Add(hyperchargedEliteIndex.EliteDef.modifierToken, "Hypercharged {0}");


            var hyperchargedEliteBuff = new CustomBuff(
            new RoR2.BuffDef
            {
                name = "Affix_Hypercharged",
                buffColor = new Color32(255, 255, 255, 255),
                iconPath = "@Aetherium:Assets/Textures/Icons/TheirReminderBuffIcon.png",
                eliteIndex = HyperchargedEliteIndex,
                canStack = false
            });
            HyperchargedBuffIndex = BuffAPI.Add(hyperchargedEliteBuff);
            equipmentDef.passiveBuff = HyperchargedBuffIndex;

            HyperchargedEliteCard = new EliteAffixCard
            {
                spawnWeight = 0.5f,
                costMultiplier = 30.0f,
                damageBoostCoeff = 2.0f,
                healthBoostCoeff = 4.5f,
                eliteOnlyScaling = 0.5f,
                eliteType = HyperchargedEliteIndex,
            };
            EsoLib.Cards.Add(HyperchargedEliteCard);


            //If we want to load a base game material, then we use this.
            GameObject worm = Resources.Load<GameObject>("Prefabs/characterbodies/ElectricWormBody");
            Debug.Log($"WORM: {worm}");
            var modelLocator = worm.GetComponent<ModelLocator>();
            Debug.Log($"MODEL LOCATOR: {modelLocator}");
            var model = modelLocator.modelTransform.GetComponent<CharacterModel>();
            Debug.Log($"MODEL: {model}");
            if (model)
            {
                var rendererInfos = model.baseRendererInfos;
                foreach(CharacterModel.RendererInfo renderer in rendererInfos)
                {
                    if (renderer.defaultMaterial.name == "matElectricWorm")
                    {
                        HyperchargedMaterial = renderer.defaultMaterial;
                    }
                }
            }

            //If we want to load our own, uncomment the one below.
            //HyperchargedMaterial = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/TheirReminder.mat");


            HyperchargedProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/LightningStake"), "HyperchargedProjectile", true);

            var impactExplosion = HyperchargedProjectile.GetComponent<RoR2.Projectile.ProjectileImpactExplosion>();
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

            On.RoR2.CharacterBody.FixedUpdate += AddHyperchargedMaterials;
            On.RoR2.GlobalEventManager.OnHitAll += SpawnLightningPillar;
        }

        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.CharacterBody.FixedUpdate -= AddHyperchargedMaterials;
            On.RoR2.GlobalEventManager.OnHitAll -= SpawnLightningPillar;
        }

        protected override bool PerformEquipmentAction(RoR2.EquipmentSlot slot)
        {
            return false;
        }

        private void SpawnLightningPillar(On.RoR2.GlobalEventManager.orig_OnHitAll orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, GameObject hitObject)
        {
            if(damageInfo.procCoefficient == 0f || damageInfo.rejected) { return; }

            var attacker = damageInfo.attacker;
            if (attacker)
            {
                var body = attacker.GetComponent<CharacterBody>();
                if (body)
                {
                    if (body.HasBuff(HyperchargedBuffIndex))
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

        private void AddHyperchargedMaterials(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (self.modelLocator && self.modelLocator.modelTransform && self.HasBuff(HyperchargedBuffIndex) && !self.GetComponent<HyperchargedBuffTracker>())
            {
                RoR2.TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                overlay.duration = float.PositiveInfinity;
                overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlay.animateShaderAlpha = true;
                overlay.destroyComponentOnEnd = true;
                overlay.originalMaterial = HyperchargedMaterial;
                overlay.AddToCharacerModel(self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());
                var hyperchargedBuffTracker = self.gameObject.AddComponent<HyperchargedBuffTracker>();
                hyperchargedBuffTracker.Overlay = overlay;
                hyperchargedBuffTracker.Body = self;
            }
            orig(self);
        }

        public class HyperchargedBuffTracker : MonoBehaviour
        {
            public RoR2.TemporaryOverlay Overlay;
            public RoR2.CharacterBody Body;

            public void FixedUpdate()
            {
                if (!Body.HasBuff(HyperchargedBuffIndex))
                {
                    UnityEngine.Object.Destroy(Overlay);
                    UnityEngine.Object.Destroy(this);
                }
            }
        }
    }
}
