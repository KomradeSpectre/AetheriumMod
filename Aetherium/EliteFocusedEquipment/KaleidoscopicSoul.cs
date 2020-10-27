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
    class KaleidoscopicSoul : Equipment_V2<KaleidoscopicSoul>
    {
        public override string displayName => "Kaleidoscopic Soul";

        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => "The type of damage you deal randomizes each time you are hit.";

        protected override string GetDescString(string langID = null) => $"";

        protected override string GetLoreString(string langID = null) => $"";

        public static EliteAffixCard KaleidoscopicEliteCard { get; set; }
        public static EliteIndex KaleidoscopicEliteIndex;
        public static BuffIndex KaleidoscopicBuffIndex;

        public static Material KaleidoscopicMaterial;

        public KaleidoscopicSoul()
        {
            modelResourcePath = "@Aetherium:Assets/Models/Prefabs/KaleidoscopicSoul.prefab";
            iconResourcePath = "@Aetherium:Assets/Textures/Icons/KaleidoscopicSoulIcon.png";
        }

        public override void SetupAttributes()
        {
            base.SetupAttributes();
            equipmentDef.canDrop = false;
            equipmentDef.enigmaCompatible = false;


            var kaleidoscopicEliteDef = new CustomElite(
            new RoR2.EliteDef
            {
                name = "Kaleidoscopic",
                modifierToken = "AETHERIUM_ELITE_MODIFIER_KALEIDOSCOPIC",
                color = new Color32(255, 255, 255, 255),
                eliteEquipmentIndex = equipmentDef.equipmentIndex
            }, 1);
            KaleidoscopicEliteIndex = EliteAPI.Add(kaleidoscopicEliteDef);
            LanguageAPI.Add(kaleidoscopicEliteDef.EliteDef.modifierToken, "Kaleidoscopic {0}");


            var kaleidoscopicBuffDef = new CustomBuff(
            new RoR2.BuffDef
            {
                name = "Affix_Kaleidoscopic",
                buffColor = new Color32(255, 255, 255, 255),
                iconPath = "@Aetherium:Assets/Textures/Icons/KaleidoscopicSoulBuffIcon.png",
                eliteIndex = KaleidoscopicEliteIndex,
                canStack = false
            });
            KaleidoscopicBuffIndex = BuffAPI.Add(kaleidoscopicBuffDef);
            equipmentDef.passiveBuff = KaleidoscopicBuffIndex;

            KaleidoscopicEliteCard = new EliteAffixCard
            {
                spawnWeight = 0.8f,
                costMultiplier = 15.0f,
                damageBoostCoeff = 2.0f,
                healthBoostCoeff = 4.0f,
                eliteOnlyScaling = 0.5f,
                eliteType = KaleidoscopicEliteIndex,
            };
            EsoLib.Cards.Add(KaleidoscopicEliteCard);

            KaleidoscopicMaterial = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/KaleidoscopicSoul.mat");
        }

        public override void Install()
        {
            base.Install();

            On.RoR2.CharacterBody.FixedUpdate += AddKaleidoscopicMaterials;
            On.RoR2.HealthComponent.TakeDamage += ShiftDamageType;
        }

        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.CharacterBody.FixedUpdate -= AddKaleidoscopicMaterials;
            On.RoR2.HealthComponent.TakeDamage -= ShiftDamageType;
        }

        protected override bool PerformEquipmentAction(RoR2.EquipmentSlot slot)
        {
            return false;
        }

        private void AddKaleidoscopicMaterials(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (self.modelLocator && self.modelLocator.modelTransform && self.HasBuff(KaleidoscopicBuffIndex) && !self.GetComponent<KaleidoscopicBuffChecker>())
            {
                RoR2.TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                overlay.duration = float.PositiveInfinity;
                overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlay.animateShaderAlpha = true;
                overlay.destroyComponentOnEnd = true;
                overlay.originalMaterial = KaleidoscopicMaterial;
                overlay.AddToCharacerModel(self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());
                var kaleidoscopicBuffTracker = self.gameObject.AddComponent<KaleidoscopicBuffChecker>();
                kaleidoscopicBuffTracker.Overlay = overlay;
                kaleidoscopicBuffTracker.Body = self;
            }
            orig(self);
        }

        private void ShiftDamageType(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            orig(self, damageInfo);
        }

        public class KaleidoscopicBuffChecker : MonoBehaviour
        {
            public RoR2.TemporaryOverlay Overlay;
            public RoR2.CharacterBody Body;

            public void FixedUpdate()
            {
                if (!Body.HasBuff(KaleidoscopicBuffIndex))
                {
                    UnityEngine.Object.Destroy(Overlay);
                    UnityEngine.Object.Destroy(this);
                }
            }
        }

    }
}
