using EliteSpawningOverhaul;
using R2API;
using RoR2;
using TILER2;
using UnityEngine;

namespace Aetherium.EliteFocusedEquipment
{
    class ApollosBrilliance : Equipment_V2<ApollosBrilliance>
    {
        public override string displayName => "Moirae's Bond";

        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Become an aspect of fate.";

        protected override string GetDescString(string langID = null) => $"You establish a link with the first enemy in range, gaining combat bonuses against them. On use, rip the souls out of enemies in a radius around you.";

        protected override string GetLoreString(string langID = null) => $"";

        public static string EliteBuffName = "Affix_Soul_Linked";
        public static string EliteBuffIconPath = "@Aetherium:Assets/Textures/Icons/MoiraesBondBuffIcon.png";

        public static string ElitePrefixName = "Soul Linked";
        public static string EliteModifierString = "AETHERIUM_ELITE_MODIFIER_SOUL_LINKED";
        public static int EliteTier = 2;

        public static EliteAffixCard EliteCard { get; set; }
        public static EliteIndex EliteIndex;
        public static BuffIndex EliteBuffIndex;

        public static Material EliteMaterial;

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

            var buffDef = new BuffDef
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

            var eliteDef = new EliteDef
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

            //If we want to load our own, uncomment the one below.
            EliteMaterial = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/ApollosBrillianceMain.mat");
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
            if (!slot.characterBody) { return false; }
            var body = slot.characterBody;

            return false;
        }

        private void AddEliteMaterials(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (self.HasBuff(EliteBuffIndex) && !self.GetComponent<SoulLinkedBuffTracker>())
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
                            var soulLinkedBuffTracker = self.gameObject.AddComponent<SoulLinkedBuffTracker>();
                            soulLinkedBuffTracker.Body = self;
                            TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                            overlay.duration = float.PositiveInfinity;
                            overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                            overlay.animateShaderAlpha = true;
                            overlay.destroyComponentOnEnd = true;
                            overlay.originalMaterial = EliteMaterial;
                            overlay.AddToCharacerModel(model);
                            soulLinkedBuffTracker.Overlay = overlay;
                        }
                    }
                }
            }
            orig(self);
        }


        public class SoulLinkedBuffTracker : MonoBehaviour
        {
            public TemporaryOverlay Overlay;
            public CharacterBody Body;

            public void FixedUpdate()
            {
                if (!Body.HasBuff(EliteBuffIndex))
                {
                    Destroy(Overlay);
                    Destroy(this);
                }
            }
        }
    }
}
