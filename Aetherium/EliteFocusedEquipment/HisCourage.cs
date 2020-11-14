using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using TILER2;
using EliteSpawningOverhaul;
using UnityEngine;
using R2API;
using R2API.Utils;
using UnityEngine.Networking;
using RoR2.Projectile;
using Aetherium.Items;

namespace Aetherium.EliteFocusedEquipment
{
    class HisCourage : Equipment_V2<HisCourage>
    {
        public override string displayName => "His Courage";

        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Starting at 100% health, each 20% of health lost grants combat bonuses. Upon death, grant nearby allies a permanent combat bonus.";

        protected override string GetDescString(string langID = null) => $"";

        protected override string GetLoreString(string langID = null) => $"";

        public static string EliteBuffName = "Affix_Heroic";
        public static string EliteBuffIconPath = "@Aetherium:Assets/Textures/Icons/HisCourageBuffIcon.png";

        public static string ElitePrefixName = "Heroic";
        public static string EliteModifierString = "AETHERIUM_ELITE_MODIFIER_HEROIC";
        public static int EliteTier = 1;

        public static EliteAffixCard EliteCard { get; set; }
        public static EliteIndex EliteIndex;
        public static BuffIndex EliteBuffIndex;

        public static Material EliteMaterial;

        public static Xoroshiro128Plus random = new Xoroshiro128Plus((ulong)System.DateTime.Now.Ticks);

        public HisCourage()
        {
            modelResourcePath = "@Aetherium:Assets/Models/Prefabs/HisCourage.prefab";
            iconResourcePath = "@Aetherium:Assets/Textures/Icons/HisCourageIcon.png";
        }

        public override void SetupAttributes()
        {
            base.SetupAttributes();
            equipmentDef.canDrop = false;
            equipmentDef.enigmaCompatible = false;

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
                costMultiplier = 15.0f,
                damageBoostCoeff = 2.0f,
                healthBoostCoeff = 2.0f,
                eliteOnlyScaling = 0.5f,
                eliteType = EliteIndex
            };
            EsoLib.Cards.Add(card);
            EliteCard = card;

            LanguageAPI.Add(eliteDef.modifierToken, ElitePrefixName + " {0}");

            EliteMaterial = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/HisCourage.mat");

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

        protected override bool PerformEquipmentAction(RoR2.EquipmentSlot slot)
        {
            return false;
        }

        private void AddEliteMaterials(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (self.HasBuff(EliteBuffIndex) && !self.GetComponent<SplinteringBuffTracker>())
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
                            var splinteringBuffTracker = self.gameObject.AddComponent<SplinteringBuffTracker>();
                            splinteringBuffTracker.Body = self;
                            TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                            overlay.duration = float.PositiveInfinity;
                            overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                            overlay.animateShaderAlpha = true;
                            overlay.destroyComponentOnEnd = true;
                            overlay.originalMaterial = EliteMaterial;
                            overlay.AddToCharacerModel(model);
                            splinteringBuffTracker.Overlay = overlay;
                        }
                    }
                }
            }
            orig(self);
        }

        public class SplinteringBuffTracker : MonoBehaviour
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
