using RoR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using System.Linq;
using BepInEx.Configuration;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.EliteEquipment
{
    public abstract class EliteEquipmentBase<T> : EliteEquipmentBase where T : EliteEquipmentBase<T>
    {
        public static T instance { get; private set; }

        public EliteEquipmentBase()
        {
            if(instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting EquipmentBoilerplate/Equipment was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class EliteEquipmentBase
    {
        public abstract string EliteEquipmentName { get; }
        public abstract string EliteAffixToken { get; }
        public abstract string EliteEquipmentPickupDesc { get; }
        public abstract string EliteEquipmentFullDescription { get; }
        public abstract string EliteEquipmentLore { get; }
        public abstract string EliteModifier { get; }

        public virtual bool AppearsInSinglePlayer { get; } = true;

        public virtual bool AppearsInMultiPlayer { get; } = true;

        public virtual bool CanDrop { get; } = false;

        public virtual float Cooldown { get; } = 60f;

        public virtual bool EnigmaCompatible { get; } = false;

        public virtual bool IsBoss { get; } = false;

        public virtual bool IsLunar { get; } = false;

        public enum EliteTier
        {
            Invalid,
            T1,
            T1Honor,
            T1Upgrade,
            T1UpgradeHonor,
            T2,
            Lunar
        }

        public abstract EliteTier EliteTierDef { get; }

        public abstract Color EliteColor { get; }

        public abstract Texture2D EliteRamp { get; }

        public virtual CustomElite CustomEliteDef { get; set; }
        public virtual CustomElite CustomEliteDefHonor { get; set; }

        public abstract GameObject EliteEquipmentModel { get; }
        public abstract Sprite EliteEquipmentIcon { get; }

        public EquipmentDef EliteEquipmentDef;

        /// <summary>
        /// Implement before calling CreateEliteEquipment.
        /// </summary>
        public BuffDef EliteBuffDef;

        public abstract Sprite EliteBuffIcon { get; }

        public virtual Color EliteBuffColor { get; set; } = new Color32(255, 255, 255, byte.MaxValue);

        /// <summary>
        /// If not overriden, the elite can spawn in all tiers defined.
        /// </summary>
        public virtual CombatDirector.EliteTierDef[] CanAppearInEliteTiers { get; set; } = EliteAPI.GetCombatDirectorEliteTiers();

        public virtual Material EliteMaterial { get; set; } = null;

        public EliteDef EliteDef;

        public virtual float HealthMultiplier { get; set; } = 1;

        public virtual float DamageMultiplier { get; set; } = 1;

        public abstract void Init(ConfigFile config);

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        protected void CreateLang()
        {
            Language.Language.Add("AETHERIUM_ELITE_EQUIPMENT_" + EliteAffixToken + "_NAME", EliteEquipmentName);
            Language.Language.Add("AETHERIUM_ELITE_EQUIPMENT_" + EliteAffixToken + "_PICKUP", EliteEquipmentPickupDesc);
            Language.Language.Add("AETHERIUM_ELITE_EQUIPMENT_" + EliteAffixToken + "_DESCRIPTION", EliteEquipmentFullDescription);
            Language.Language.Add("AETHERIUM_ELITE_EQUIPMENT_" + EliteAffixToken + "_LORE", EliteEquipmentLore);
            Language.Language.Add("AETHERIUM_ELITE_" + EliteAffixToken + "_MODIFIER", EliteModifier + " {0}");

        }

        protected void CreateEquipment()
        {
            EliteBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            EliteBuffDef.name = EliteAffixToken;
            EliteBuffDef.canStack = false;
            EliteBuffDef.isCooldown = false;
            EliteBuffDef.isDebuff = false;
            EliteBuffDef.buffColor = EliteColor;
            EliteBuffDef.iconSprite = EliteBuffIcon;

            ContentAddition.AddBuffDef(EliteBuffDef);

            EliteEquipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();
            EliteEquipmentDef.name = "AETHERIUM_ELITE_EQUIPMENT_" + EliteAffixToken;
            EliteEquipmentDef.nameToken = "AETHERIUM_ELITE_EQUIPMENT_" + EliteAffixToken + "_NAME";
            EliteEquipmentDef.pickupToken = "AETHERIUM_ELITE_EQUIPMENT_" + EliteAffixToken + "_PICKUP";
            EliteEquipmentDef.descriptionToken = "AETHERIUM_ELITE_EQUIPMENT_" + EliteAffixToken + "_DESCRIPTION";
            EliteEquipmentDef.loreToken = "AETHERIUM_ELITE_EQUIPMENT_" + EliteAffixToken + "_LORE";
            EliteEquipmentDef.pickupModelPrefab = EliteEquipmentModel;
            EliteEquipmentDef.pickupIconSprite = EliteEquipmentIcon;
            EliteEquipmentDef.appearsInSinglePlayer = AppearsInSinglePlayer;
            EliteEquipmentDef.appearsInMultiPlayer = AppearsInMultiPlayer;
            EliteEquipmentDef.canDrop = CanDrop;
            EliteEquipmentDef.cooldown = Cooldown;
            EliteEquipmentDef.enigmaCompatible = EnigmaCompatible;
            EliteEquipmentDef.isBoss = IsBoss;
            EliteEquipmentDef.isLunar = IsLunar;
            EliteEquipmentDef.passiveBuffDef = EliteBuffDef;
            EliteEquipmentDef.requiredExpansion = AetheriumExpansionDef;

            ItemAPI.Add(new CustomEquipment(EliteEquipmentDef, CreateItemDisplayRules()));

            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;

            if(UseTargeting && TargetingIndicatorPrefabBase)
            {
                On.RoR2.EquipmentSlot.Update += UpdateTargeting;
            }

            if(EliteMaterial)
            {
                On.RoR2.CharacterBody.FixedUpdate += OverlayManager;
            }
        }

        /*private void CharacterModel_UpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
        {
            orig(self);
            if(self.body && self.body.inventory)
            {
                if(self.activeOverlayCount >= CharacterModel.maxOverlays) return;
                if(self.body.HasBuff(EliteBuffDef))
                {
                    Material[] array = self.currentOverlays;
                    int num = self.activeOverlayCount;
                    self.activeOverlayCount = num + 1;
                    array[num] = EliteMaterial;
                }
            }
        }*/

        private void OverlayManager(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            if(self.modelLocator && self.modelLocator.modelTransform && self.HasBuff(EliteBuffDef) && !self.GetComponent<EliteOverlayManager>())
            {
                var overlay = TemporaryOverlayManager.AddOverlay(self.modelLocator.modelTransform.gameObject);
                overlay.duration = float.PositiveInfinity;
                overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlay.animateShaderAlpha = true;
                overlay.destroyComponentOnEnd = true;
                overlay.originalMaterial = EliteMaterial;
                overlay.AddToCharacterModel(self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());
                var EliteOverlayManager = self.gameObject.AddComponent<EliteOverlayManager>();
                EliteOverlayManager.Overlay = overlay;
                EliteOverlayManager.Body = self;
                EliteOverlayManager.EliteBuffDef = EliteBuffDef;
            }
            orig(self);
        }

        public class EliteOverlayManager : MonoBehaviour
        {
            public RoR2.TemporaryOverlayInstance Overlay;
            public RoR2.CharacterBody Body;
            public BuffDef EliteBuffDef;

            public void FixedUpdate()
            {
                if(!Body.HasBuff(EliteBuffDef))
                {
                    UnityEngine.Object.Destroy(this);
                    Overlay.CleanupEffect();
                }
            }
        }

        protected void CreateElite()
        {
            EliteDef = ScriptableObject.CreateInstance<EliteDef>();
            EliteDef.name = "AETHERIUM_ELITE_" + EliteAffixToken;
            EliteDef.modifierToken = "AETHERIUM_ELITE_" + EliteAffixToken + "_MODIFIER";
            EliteDef.eliteEquipmentDef = EliteEquipmentDef;
            EliteDef.healthBoostCoefficient = HealthMultiplier;
            EliteDef.damageBoostCoefficient = DamageMultiplier;

            var tierDefs = GetVanillaEliteTierDef(EliteTierDef);
            if(tierDefs is null)
            {
                ModLogger.LogError("Failed to get vanilla elite tier definitions.");
                return;
            }

            var customElite = new CustomElite("AETHERIUM_ELITE_" + EliteAffixToken, EliteEquipmentDef, EliteColor, "AETHERIUM_ELITE_" + EliteAffixToken + "_MODIFIER", tierDefs, EliteRamp);

            if(EliteTierDef < EliteTier.T2)
            {
                customElite.EliteDef.healthBoostCoefficient = 4f;
                customElite.EliteDef.damageBoostCoefficient = 2f;
            }
            else
            {
                customElite.EliteDef.healthBoostCoefficient = 18f;
                customElite.EliteDef.damageBoostCoefficient = 6f;
            }

            EliteAPI.Add(customElite);

            EliteBuffDef.eliteDef = customElite.EliteDef;
            ContentAddition.AddBuffDef(EliteBuffDef);
        }


        /*public virtual CustomElite SetupElite()
        {
            var tierDefs = this.GetVanillaEliteTierDef(this.EliteTierDef);
            if(tierDefs is null)
                return null;

            var customElite = new CustomElite("AETHERIUM_ELITE_" + this.EliteAffixToken, this.EliteEquipmentDef, this.EliteColor, "AETHERIUM_ELITE_" + this.EliteAffixToken + "_MODIFIER", tierDefs, this.EliteRamp);
            if(this.EliteTierDef < EliteTier.T2)
            {
                tierDefs = this.GetVanillaEliteTierDef(this.EliteTierDef + 1);
                if(tierDefs != null)
                {
                    var customHonorElite = new CustomElite("AETHERIUM_ELITE_" + this.EliteAffixToken +"_HONOR", this.EliteEquipmentDef, this.EliteColor, "AETHERIUM_ELITE_" + this.EliteAffixToken + "_MODIFIER", tierDefs, this.EliteRamp);

                    customHonorElite.EliteDef.healthBoostCoefficient = 2.5f;
                    customHonorElite.EliteDef.damageBoostCoefficient = 1.5f;

                    EliteAPI.Add(customHonorElite);
                }

                customElite.EliteDef.healthBoostCoefficient = 4f;
                customElite.EliteDef.damageBoostCoefficient = 2f;
            }
            else
            {
                customElite.EliteDef.healthBoostCoefficient = 18f;
                customElite.EliteDef.damageBoostCoefficient = 6f;
            }

            EliteAPI.Add(customElite);

            CustomEliteDef.EliteDef.modifierToken = "AETHERIUM_ELITE_" + EliteAffixToken + "_MODIFIER";
            CustomEliteDef.EliteDef = CustomEliteDef.EliteDef;
            ContentAddition.AddBuffDef(EliteBuffDef);

            return customElite;
        }*/

        private IEnumerable<CombatDirector.EliteTierDef> GetVanillaEliteTierDef(EliteTier tier)
        {
            // 0 - none
            // 1 - t1
            // 2 - t1 honor
            // 3 - t1 + gold
            // 4 - t1 + gold honor
            // 5 - t2
            // 6 - lunar

            if(tier == EliteTier.Invalid)
            {
                ModLogger.LogError("Invalid tier");
                ModLogger.LogDebug(new System.Diagnostics.StackTrace());

                return null;
            }

            List<CombatDirector.EliteTierDef> tierDefs = new List<CombatDirector.EliteTierDef>() { EliteAPI.VanillaEliteTiers[(int)tier] };

            if(this.EliteTierDef is EliteTier.T1 or EliteTier.T1Honor)
                tierDefs.Add(EliteAPI.VanillaEliteTiers[(int)tier + 2]);

            return tierDefs;
        }

        protected bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if(equipmentDef == EliteEquipmentDef)
            {
                return ActivateEquipment(self);
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }

        protected abstract bool ActivateEquipment(EquipmentSlot slot);

        public abstract void Hooks();

        #region Targeting Setup
        //Targeting Support
        public virtual bool UseTargeting { get; } = false;
        public GameObject TargetingIndicatorPrefabBase = null;
        public enum TargetingType
        {
            Enemies,
            Friendlies,
        }
        public virtual TargetingType TargetingTypeEnum { get; } = TargetingType.Enemies;

        //Based on MysticItem's targeting code.
        protected void UpdateTargeting(On.RoR2.EquipmentSlot.orig_Update orig, EquipmentSlot self)
        {
            orig(self);

            if(self.equipmentIndex == EliteEquipmentDef.equipmentIndex)
            {
                var targetingComponent = self.GetComponent<TargetingControllerComponent>();
                if(!targetingComponent)
                {
                    targetingComponent = self.gameObject.AddComponent<TargetingControllerComponent>();
                    targetingComponent.VisualizerPrefab = TargetingIndicatorPrefabBase;
                }

                if(self.stock > 0)
                {
                    switch (TargetingTypeEnum)
                    {
                        case (TargetingType.Enemies):
                            targetingComponent.ConfigureTargetFinderForEnemies(self);
                            break;
                        case (TargetingType.Friendlies):
                            targetingComponent.ConfigureTargetFinderForFriendlies(self);
                            break;
                    }
                }
                else
                {
                    targetingComponent.Invalidate();
                    targetingComponent.Indicator.active = false;
                }
            }
        }

        public class TargetingControllerComponent : MonoBehaviour
        {
            public GameObject TargetObject;
            public GameObject VisualizerPrefab;
            public Indicator Indicator;
            public BullseyeSearch TargetFinder;
            public Action<BullseyeSearch> AdditionalBullseyeFunctionality = (search) => { };

            public void Awake()
            {
                Indicator = new Indicator(gameObject, null);
            }

            public void OnDestroy()
            {
                Invalidate();
            }

            public void Invalidate()
            {
                TargetObject = null;
                Indicator.targetTransform = null;
            }

            public void ConfigureTargetFinderBase(EquipmentSlot self)
            {
                if(TargetFinder == null) TargetFinder = new BullseyeSearch();
                TargetFinder.teamMaskFilter = TeamMask.allButNeutral;
                TargetFinder.teamMaskFilter.RemoveTeam(self.characterBody.teamComponent.teamIndex);
                TargetFinder.sortMode = BullseyeSearch.SortMode.Angle;
                TargetFinder.filterByLoS = true;
                float num;
                Ray ray = CameraRigController.ModifyAimRayIfApplicable(self.GetAimRay(), self.gameObject, out num);
                TargetFinder.searchOrigin = ray.origin;
                TargetFinder.searchDirection = ray.direction;
                TargetFinder.maxAngleFilter = 10f;
                TargetFinder.viewer = self.characterBody;
            }

            public void ConfigureTargetFinderForEnemies(EquipmentSlot self)
            {
                ConfigureTargetFinderBase(self);
                TargetFinder.teamMaskFilter = TeamMask.GetUnprotectedTeams(self.characterBody.teamComponent.teamIndex);
                TargetFinder.RefreshCandidates();
                TargetFinder.FilterOutGameObject(self.gameObject);
                AdditionalBullseyeFunctionality(TargetFinder);
                PlaceTargetingIndicator(TargetFinder.GetResults());
            }

            public void ConfigureTargetFinderForFriendlies(EquipmentSlot self)
            {
                ConfigureTargetFinderBase(self);
                TargetFinder.teamMaskFilter = TeamMask.none;
                TargetFinder.teamMaskFilter.AddTeam(self.characterBody.teamComponent.teamIndex);
                TargetFinder.RefreshCandidates();
                TargetFinder.FilterOutGameObject(self.gameObject);
                AdditionalBullseyeFunctionality(TargetFinder);
                PlaceTargetingIndicator(TargetFinder.GetResults());

            }

            public void PlaceTargetingIndicator(IEnumerable<HurtBox> TargetFinderResults)
            {
                HurtBox hurtbox = TargetFinderResults.Any() ? TargetFinderResults.First() : null;

                if(hurtbox)
                {
                    TargetObject = hurtbox.healthComponent.gameObject;
                    Indicator.visualizerPrefab = VisualizerPrefab;
                    Indicator.targetTransform = hurtbox.transform;
                }
                else
                {
                    Invalidate();
                }
                Indicator.active = hurtbox;
            }
        }

        #endregion Targeting Setup
    }
}
