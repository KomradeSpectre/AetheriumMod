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

namespace Aetherium.EliteFocusedEquipment
{
    class CallToWorship : Equipment_V2<CallToWorship>
    {
        public override string displayName => "Their Reminder";

        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Enemies around you will have their gravity intensified.";

        protected override string GetDescString(string langID = null) => $"";

        protected override string GetLoreString(string langID = null) => $"";

        public static string EliteBuffName = "Affix_Pressurizing";
        public static string EliteBuffIconPath = "";

        public static string ElitePrefixName = "Pressurizing";
        public static string EliteModifierString = "AETHERIUM_ELITE_MODIFIER_PRESSURIZING";
        public static int EliteTier = 2;

        public static EliteAffixCard EliteCard { get; set; }
        public static EliteIndex EliteIndex;
        public static BuffIndex EliteBuffIndex;

        public static Material EliteMaterial;

        public static GameObject HyperchargedProjectile;

        public static Xoroshiro128Plus random = new Xoroshiro128Plus((ulong)System.DateTime.Now.Ticks);

        public CallToWorship()
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
            EliteMaterial = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/TheirReminder.mat");

        }

        public override void Install()
        {
            base.Install();

            On.RoR2.CharacterBody.FixedUpdate += AddEliteMaterials;
            On.RoR2.CharacterBody.FixedUpdate += ApplyWeightToVictims;
        }

        private void ApplyWeightToVictims(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
        }

        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.CharacterBody.FixedUpdate -= AddEliteMaterials;
            On.RoR2.CharacterBody.FixedUpdate -= ApplyWeightToVictims;
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
            if (self.HasBuff(EliteBuffIndex) && !self.GetComponent<HyperchargedBuffTracker>())
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
                            var hyperchargedBuffTracker = self.gameObject.AddComponent<HyperchargedBuffTracker>();
                            hyperchargedBuffTracker.Body = self;
                            RoR2.TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
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
