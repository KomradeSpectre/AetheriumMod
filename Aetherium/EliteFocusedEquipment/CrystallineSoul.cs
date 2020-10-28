using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using TILER2;
using EliteSpawningOverhaul;
using UnityEngine;
using R2API;
using R2API.Utils;

namespace Aetherium.EliteFocusedEquipment
{
    class CrystallineSoul : Equipment_V2<CrystallineSoul>
    {
        public override string displayName => "Crystalline Soul";

        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Release shards when you are hit.";

        protected override string GetDescString(string langID = null) => $"";

        protected override string GetLoreString(string langID = null) => $"";

        public static EliteAffixCard CrystallineEliteCard { get; set; }
        public static EliteIndex CrystallineEliteIndex;
        public static BuffIndex CrystallineBuffIndex;

        public static Material CrystallineMaterial;

        public CrystallineSoul()
        {
            modelResourcePath = "@Aetherium:Assets/Models/Prefabs/CrystallineSoul.prefab";
            iconResourcePath = "@Aetherium:Assets/Textures/Icons/CrystallineSoulIcon.png";
        }

        public override void SetupAttributes()
        {
            base.SetupAttributes();
            equipmentDef.canDrop = false;
            equipmentDef.enigmaCompatible = false;


            var crystallineEliteDef = new CustomElite(
            new RoR2.EliteDef
            {
                name = "Crystalline",
                modifierToken = "AETHERIUM_ELITE_MODIFIER_CRYSTALLINE",
                color = new Color32(0, 0, 0, 255),
                eliteEquipmentIndex = equipmentDef.equipmentIndex
            }, 1);
            CrystallineEliteIndex = EliteAPI.Add(crystallineEliteDef);
            LanguageAPI.Add(crystallineEliteDef.EliteDef.modifierToken, "Crystalline {0}");


            var crystallineEliteBuff = new CustomBuff(
            new RoR2.BuffDef
            {
                name = "Affix_Kaleidoscopic",
                buffColor = new Color32(255, 255, 255, 255),
                iconPath = "@Aetherium:Assets/Textures/Icons/CrystallineSoulBuffIcon.png",
                eliteIndex = CrystallineEliteIndex,
                canStack = false
            });
            CrystallineBuffIndex = BuffAPI.Add(crystallineEliteBuff);
            equipmentDef.passiveBuff = CrystallineBuffIndex;

            CrystallineEliteCard = new EliteAffixCard
            {
                spawnWeight = 0.8f,
                costMultiplier = 15.0f,
                damageBoostCoeff = 2.0f,
                healthBoostCoeff = 4.0f,
                eliteOnlyScaling = 0.5f,
                eliteType = CrystallineEliteIndex,
            };
            EsoLib.Cards.Add(CrystallineEliteCard);

            CrystallineMaterial = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/CrystallineSoul.mat");
        }

        public override void Install()
        {
            base.Install();

            On.RoR2.CharacterBody.FixedUpdate += AddCrystallineMaterials;
            On.RoR2.HealthComponent.TakeDamage += ShiftDamageType;
        }

        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.CharacterBody.FixedUpdate -= AddCrystallineMaterials;
            On.RoR2.HealthComponent.TakeDamage -= ShiftDamageType;
        }

        protected override bool PerformEquipmentAction(RoR2.EquipmentSlot slot)
        {
            return false;
        }

        private void AddCrystallineMaterials(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (self.modelLocator && self.modelLocator.modelTransform && self.HasBuff(CrystallineBuffIndex) && !self.GetComponent<CrystallineBuffTracker>())
            {
                RoR2.TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                overlay.duration = float.PositiveInfinity;
                overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlay.animateShaderAlpha = true;
                overlay.destroyComponentOnEnd = true;
                overlay.originalMaterial = CrystallineMaterial;
                overlay.AddToCharacerModel(self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());
                var kaleidoscopicBuffTracker = self.gameObject.AddComponent<CrystallineBuffTracker>();
                kaleidoscopicBuffTracker.Overlay = overlay;
                kaleidoscopicBuffTracker.Body = self;
            }
            orig(self);
        }

        private void ShiftDamageType(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            orig(self, damageInfo);
        }

        public class CrystallineBuffTracker : MonoBehaviour
        {
            public RoR2.TemporaryOverlay Overlay;
            public RoR2.CharacterBody Body;

            public void FixedUpdate()
            {
                if (!Body.HasBuff(CrystallineBuffIndex))
                {
                    UnityEngine.Object.Destroy(Overlay);
                    UnityEngine.Object.Destroy(this);
                }
            }
        }

    }
}
