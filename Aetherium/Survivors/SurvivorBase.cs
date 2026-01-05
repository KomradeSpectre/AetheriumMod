using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Survivors
{
    public abstract class SurvivorBase<T> : SurvivorBase where T : SurvivorBase<T>
    {
        public static T instance { get; private set; }

        public SurvivorBase()
        {
            if(instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting SurvivorBase was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class SurvivorBase
    {
        public abstract string SurvivorName { get; }
        public abstract string SurvivorBodyName { get; }
        public abstract string SurvivorSubtitle { get; }
        public abstract string SurvivorLangToken { get; }
        public abstract string SurvivorDescription { get; }
        public abstract string SurvivorEndingSuccessText { get; }
        public abstract string SurvivorEndingFailureText { get; }

        public abstract float SurvivorBaseMaxHealth { get; }
        public abstract float SurvivorBaseArmor { get; }
        public abstract float SurvivorBaseMoveSpeed { get; }
        public abstract int SurvivorBaseJumpCount { get; }

        public virtual float SurvivorBaseMaxShield { get; set; } = 0f;
        public virtual float SurvivorBaseRegen { get; set; } = 1f;
        public virtual float SurvivorBaseAcceleration { get; set; } = 80f;
        public virtual float SurvivorBaseJumpPower { get; set; } = 15f;
        public virtual float SurvivorBaseDamage { get; set; } = 12f;
        public virtual float SurvivorBaseCritChance { get; set; } = 1f;
        public virtual float SurvivorBaseAttackSpeed { get; set; } = 1f;
        public virtual float SurvivorSprintSpeedMultiplier { get; set; } = 1.45f;

        public abstract GameObject SurvivorBodyModelPrefab { get; }
        public abstract GameObject SurvivorDisplayModelPrefab { get; }
        public abstract Texture SurvivorPortraitIcon { get; }
        public virtual GameObject SurvivorCrosshair { get; set; } = null;
        public virtual GameObject SurvivorPodPrefab { get; set; } = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab").WaitForCompletion();
        public virtual Color SurvivorLogbookColor { get; set; } = Color.white;

        public virtual Vector3 ModelBasePosition { get; set; } = new Vector3(0f, -0.92f, 0f);
        public virtual Vector3 AimOriginPosition { get; set; } = new Vector3(0f, 1.6f, 0f);
        public CharacterCameraParams CharacterCameraParameters;
        public virtual Vector3 CameraPivotPosition { get; set; } = new Vector3(0f, 0.8f, 0);
        public virtual float CameraPivotVerticalOffset { get; set; } = 1.37f;
        public virtual float CameraZoomInOutDepth { get; set; } = -10f;

        public abstract Type SurvivorMainState { get; }
        public virtual Type SurvivorSpawnState { get; }

        public SurvivorDef SurvivorDef;
        public GameObject SurvivorBodyPrefab;
        public CharacterBody SurvivorCharacterBody;
        public ChildLocator SurvivorChildLocator;

        public virtual string CharacterNameToClone { get; set; } = "Commando";
        public virtual string CloneBodyAddressablePath { get; set; } = "RoR2/Base/Commando/CommandoBody.prefab";

        public virtual UnlockableDef SurvivorUnlockDef { get; set; } = null;
        public virtual float DesiredSelectScreenSortPosition { get; set; } = 100f;
        public virtual CharacterBody.BodyFlags SurvivorBodyFlags { get; set; } = CharacterBody.BodyFlags.ImmuneToExecutes;
        public virtual HullClassification SurvivorHullClassification { get; set; } = HullClassification.Human;
        public PhysicMaterial RagdollMaterial;

        public abstract void Init(ConfigFile config);

        protected void CreateLang()
        {
            Language.Language.Add("SURVIVOR_" + SurvivorLangToken + "_NAME", SurvivorName);
            Language.Language.Add("SURVIVOR_" + SurvivorLangToken + "_BODY_NAME", SurvivorBodyName);
            Language.Language.Add("SURVIVOR_" + SurvivorLangToken + "_BODY_SUBTITLE", SurvivorSubtitle);
            Language.Language.Add("SURVIVOR_" + SurvivorLangToken + "_DESCRIPTION", SurvivorDescription);
            Language.Language.Add("SURVIVOR_" + SurvivorLangToken + "_OUTRO_TEXT", SurvivorEndingSuccessText);
            Language.Language.Add("SURVIVOR_" + SurvivorLangToken + "_OUTRO_FAILURE_TEXT", SurvivorEndingFailureText);
        }

        protected void CreateBodyAndDisplay()
        {
            SurvivorBodyPrefab = CreateCharacterPrefab();
            if(!SurvivorBodyPrefab || !SurvivorBodyPrefab.GetComponent<CharacterBody>()) return;
            CreateDisplayPrefab();
        }

        protected void CreateSurvivor()
        {
            if(!SurvivorBodyPrefab) return;

            var expansionRequirement = SurvivorBodyPrefab.AddComponent<ExpansionRequirementComponent>();
            expansionRequirement.requiredExpansion = AetheriumExpansionDef;
            R2API.ContentAddition.AddBody(SurvivorBodyPrefab);

            SurvivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            SurvivorDef.bodyPrefab = SurvivorBodyPrefab;
            SurvivorDef.displayPrefab = SurvivorDisplayModelPrefab;
            SurvivorDef.primaryColor = SurvivorLogbookColor;
            SurvivorDef.cachedName = SurvivorBodyName;
            SurvivorDef.displayNameToken = "SURVIVOR_" + SurvivorLangToken + "_NAME";
            SurvivorDef.descriptionToken = "SURVIVOR_" + SurvivorLangToken + "_DESCRIPTION";
            SurvivorDef.outroFlavorToken = "SURVIVOR_" + SurvivorLangToken + "_OUTRO_TEXT";
            SurvivorDef.mainEndingEscapeFailureFlavorToken = "SURVIVOR_" + SurvivorLangToken + "_OUTRO_FAILURE_TEXT";
            SurvivorDef.desiredSortPosition = DesiredSelectScreenSortPosition;

            if(SurvivorUnlockDef) SurvivorDef.unlockableDef = SurvivorUnlockDef;

            R2API.ContentAddition.AddSurvivorDef(SurvivorDef);
        }

        public virtual void CreateCharacterMaster() { }

        public virtual void CreateEntityStateMachine()
        {
            if(SurvivorBodyPrefab)
            {
                SurvivorBodyPrefab.GetComponent<EntityStateMachine>().mainStateType = new EntityStates.SerializableEntityStateType(SurvivorMainState);
                R2API.ContentAddition.AddEntityState(SurvivorMainState, out var succeeded);

                if(SurvivorSpawnState != null)
                {
                    SurvivorBodyPrefab.GetComponent<EntityStateMachine>().initialStateType = new EntityStates.SerializableEntityStateType(SurvivorSpawnState);
                    R2API.ContentAddition.AddEntityState(SurvivorMainState, out var success);
                }
            }
        }

        protected void CreateSkillFamily(GameObject targetPrefab, GenericSkill skillSlot)
        {
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (newFamily as ScriptableObject).name = targetPrefab.name + skillSlot.skillName + "Family";
            newFamily.variants = new SkillFamily.Variant[0];
            skillSlot._skillFamily = newFamily;
            R2API.ContentAddition.AddSkillFamily(newFamily);
        }

        protected GenericSkill CreateGenericSkill(GameObject targetPrefab, string skillName)
        {
            GenericSkill skill = targetPrefab.AddComponent<GenericSkill>();
            skill.skillName = skillName;
            CreateSkillFamily(targetPrefab, skill);
            return skill;
        }
        public abstract void CreateSkills();
        public virtual void CreateHurtboxes() { }
        public virtual void CreateHitboxes() { }
        public virtual void CreateSkins() { }
        public abstract void CreateItemDisplays();
        public abstract void Hooks();

        public GameObject CreateCharacterPrefab()
        {
            GameObject ClonedBody = Addressables.LoadAssetAsync<GameObject>(CloneBodyAddressablePath).WaitForCompletion();
            if(!ClonedBody) return null;

            GameObject NewBodyPrefab = PrefabAPI.InstantiateClone(ClonedBody, SurvivorBodyName);
            SurvivorCharacterBody = CharacterBodySetup(NewBodyPrefab);

            if(!this.SurvivorBodyModelPrefab) return null;

            SurvivorChildLocator = CharacterChildLocatorSwap(SurvivorBodyModelPrefab);
            Transform ModelBaseTransform = ModelTransformSetup(NewBodyPrefab, SurvivorBodyModelPrefab.transform, ModelBasePosition, CameraPivotPosition, AimOriginPosition);
            ModelLocatorSetup(NewBodyPrefab, ModelBaseTransform, SurvivorBodyModelPrefab.transform);
            CharacterDirectionSetup(NewBodyPrefab, ModelBaseTransform, SurvivorBodyModelPrefab.transform);
            FootstepHandlerSetup(SurvivorBodyModelPrefab);
            RagdollSetup(SurvivorBodyModelPrefab);
            SetupCharacterModel(NewBodyPrefab);
            CameraTargetSetup(NewBodyPrefab);
            AimAnimatorSetup(NewBodyPrefab, SurvivorBodyModelPrefab);
            BasicCapsuleColliderSetup(NewBodyPrefab);
            MainHurtboxSetup(NewBodyPrefab, SurvivorBodyModelPrefab);

            return NewBodyPrefab;
        }

        public CharacterBody CharacterBodySetup(GameObject bodyPrefab)
        {
            var characterBody = bodyPrefab.GetComponent<CharacterBody>();
            if(!characterBody) return null;

            characterBody.name = SurvivorBodyName;
            characterBody.baseNameToken = "SURVIVOR_" + SurvivorLangToken + "_NAME";
            characterBody.subtitleNameToken = "SURVIVOR_" + SurvivorLangToken + "_BODY_SUBTITLE";
            characterBody.portraitIcon = SurvivorPortraitIcon;
            characterBody.bodyColor = SurvivorLogbookColor;
            characterBody._defaultCrosshairPrefab = SurvivorCrosshair;
            characterBody.hideCrosshair = false;
            characterBody.preferredPodPrefab = SurvivorPodPrefab;
            characterBody.baseMaxHealth = SurvivorBaseMaxHealth;
            characterBody.baseMaxShield = SurvivorBaseMaxShield;
            characterBody.baseRegen = SurvivorBaseRegen;
            characterBody.baseArmor = SurvivorBaseArmor;
            characterBody.baseDamage = SurvivorBaseDamage;
            characterBody.baseAttackSpeed = SurvivorBaseAttackSpeed;
            characterBody.baseCrit = SurvivorBaseCritChance;
            characterBody.baseMoveSpeed = SurvivorBaseMoveSpeed;
            characterBody.baseJumpPower = SurvivorBaseJumpPower;
            characterBody.baseAcceleration = SurvivorBaseAcceleration;
            characterBody.baseJumpCount = SurvivorBaseJumpCount;
            characterBody.sprintingSpeedMultiplier = SurvivorSprintSpeedMultiplier;
            characterBody.bodyFlags = SurvivorBodyFlags;
            characterBody.hullClassification = SurvivorHullClassification;
            characterBody.rootMotionInMainState = false;
            characterBody.isChampion = false;

            return characterBody;
        }

        public ChildLocator CharacterChildLocatorSwap(GameObject model)
        {
            var childLocatorCustom = model.GetComponent<ChildLocatorCustom>();
            if(childLocatorCustom)
            {
                var childLocator = model.AddComponent<ChildLocator>();

                childLocator.transformPairs = childLocatorCustom.transformPairs
                    .Select(x => new ChildLocator.NameTransformPair
                    {
                        name = x.name,
                        transform = x.transform
                    })
                    .ToArray();

                UnityEngine.Object.DestroyImmediate(childLocatorCustom);
                return childLocator;
            }

            return model.GetComponent<ChildLocator>();
        }

        public Transform ModelTransformSetup(GameObject bodyPrefab, Transform modelTransform, Vector3 modelBasePosition, Vector3 cameraPivotPosition, Vector3 aimOriginPosition)
        {
            for (int i = bodyPrefab.transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(bodyPrefab.transform.GetChild(i).gameObject);
            }

            Transform modelBase = new GameObject("ModelBase").transform;
            modelBase.parent = bodyPrefab.transform;
            modelBase.localPosition = modelBasePosition;
            modelBase.localRotation = Quaternion.identity;

            modelTransform.parent = modelBase.transform;
            modelTransform.localPosition = Vector3.zero;
            modelTransform.localRotation = Quaternion.identity;

            GameObject cameraPivot = new GameObject("CameraPivot");
            cameraPivot.transform.parent = bodyPrefab.transform;
            cameraPivot.transform.localPosition = cameraPivotPosition;
            cameraPivot.transform.localRotation = Quaternion.identity;

            GameObject aimOrigin = new GameObject("AimOrigin");
            aimOrigin.transform.parent = bodyPrefab.transform;
            aimOrigin.transform.localPosition = aimOriginPosition;
            aimOrigin.transform.localRotation = Quaternion.identity;
            bodyPrefab.GetComponent<CharacterBody>().aimOriginTransform = aimOrigin.transform;

            return modelBase.transform;
        }

        public ModelLocator ModelLocatorSetup(GameObject bodyPrefab, Transform modelBaseTransform, Transform modelTransform)
        {
            ModelLocator modelLocator = bodyPrefab.GetComponent<ModelLocator>();
            modelLocator.modelBaseTransform = modelBaseTransform;
            modelLocator.modelTransform = modelTransform;
            return modelLocator;
        }

        public CharacterDirection CharacterDirectionSetup(GameObject prefab, Transform modelBaseTransform, Transform modelTransform)
        {
            CharacterDirection characterDirection = prefab.GetComponent<CharacterDirection>();
            if(characterDirection)
            {
                characterDirection.targetTransform = modelBaseTransform;
                characterDirection.overrideAnimatorForwardTransform = null;
                characterDirection.rootMotionAccumulator = null;
                characterDirection.modelAnimator = modelTransform.GetComponent<Animator>();
                characterDirection.driveFromRootRotation = false;
                characterDirection.turnSpeed = 720f;
            }
            return characterDirection;
        }

        public FootstepHandler FootstepHandlerSetup(GameObject model)
        {
            if(!model) return null;
            FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
            return footstepHandler;
        }

        public RagdollController RagdollSetup(GameObject model)
        {
            RagdollController ragdollController = model.GetComponent<RagdollController>();
            if(!ragdollController) return null;

            if(RagdollMaterial == null)
            {
                GameObject commando = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion();
                RagdollMaterial = commando.GetComponentInChildren<RagdollController>().bones[1].GetComponent<Collider>().material;
            }

            foreach (Transform boneTransform in ragdollController.bones)
            {
                if(boneTransform)
                {
                    boneTransform.gameObject.layer = LayerIndex.ragdoll.intVal;
                    Collider boneCollider = boneTransform.GetComponent<Collider>();
                    if(boneCollider)
                    {
                        boneCollider.material = RagdollMaterial;
                        boneCollider.sharedMaterial = RagdollMaterial;
                    }
                }
            }
            return ragdollController;
        }

        public void SetupCharacterModel(GameObject characterPrefab)
        {
            var modelLocator = characterPrefab.GetComponent<ModelLocator>();
            if(modelLocator)
            {
                var modelTransform = modelLocator.modelTransform;
                if(modelTransform)
                {
                    var characterModel = modelTransform.gameObject.GetComponent<CharacterModel>();
                    if(!characterModel) characterModel = modelTransform.gameObject.AddComponent<CharacterModel>();

                    characterModel.body = characterPrefab.GetComponent<CharacterBody>();
                    characterModel.autoPopulateLightInfos = true;
                    characterModel.invisibilityCount = 0;
                    characterModel.temporaryOverlays = new List<TemporaryOverlayInstance>();
                    characterModel.baseRendererInfos = SurvivorHelpers.CharacterRendererInfoSetup(characterModel.gameObject);
                }
            }
        }

        public CameraTargetParams CameraTargetSetup(GameObject bodyPrefab)
        {
            if(CharacterCameraParameters == null)
            {
                CharacterCameraParameters = ScriptableObject.CreateInstance<CharacterCameraParams>();
                CharacterCameraParameters.data.minPitch = -70;
                CharacterCameraParameters.data.maxPitch = 70;
                CharacterCameraParameters.data.wallCushion = 0.1f;
                CharacterCameraParameters.data.pivotVerticalOffset = CameraPivotVerticalOffset;
                CharacterCameraParameters.data.idealLocalCameraPos = new Vector3(0, 0, CameraZoomInOutDepth);
            }

            CameraTargetParams cameraTargetParams = bodyPrefab.GetComponent<CameraTargetParams>();
            if(cameraTargetParams)
            {
                cameraTargetParams.cameraParams = CharacterCameraParameters;
                cameraTargetParams.cameraPivotTransform = bodyPrefab.transform.Find("CameraPivot");
            }
            return cameraTargetParams;
        }

        public AimAnimator AimAnimatorSetup(GameObject bodyPrefab, GameObject model)
        {
            AimAnimator aimAnimator = model.AddComponent<AimAnimator>();
            aimAnimator.directionComponent = bodyPrefab.GetComponent<CharacterDirection>();
            aimAnimator.pitchRangeMax = 60f;
            aimAnimator.pitchRangeMin = -60f;
            aimAnimator.yawRangeMin = -80f;
            aimAnimator.yawRangeMax = 80f;
            aimAnimator.pitchGiveupRange = 30f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 3f;
            aimAnimator.inputBank = bodyPrefab.GetComponent<InputBankTest>();
            return aimAnimator;
        }

        public CapsuleCollider BasicCapsuleColliderSetup(GameObject bodyPrefab)
        {
            CapsuleCollider capsuleCollider = bodyPrefab.GetComponent<CapsuleCollider>();
            capsuleCollider.center = new Vector3(0f, 0f, 0f);
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.direction = 1;
            return capsuleCollider;
        }

        public HurtBoxGroup MainHurtboxSetup(GameObject bodyPrefab, GameObject modelPrefab)
        {
            ChildLocator childLocator = modelPrefab.GetComponent<ChildLocator>();
            if(!childLocator || !childLocator.FindChild("MainHurtbox")) return null;

            HurtBoxGroup hurtBoxGroup = modelPrefab.AddComponent<HurtBoxGroup>();
            HurtBox mainHurtbox = childLocator.FindChild("MainHurtbox").gameObject.AddComponent<HurtBox>();
            mainHurtbox.gameObject.layer = LayerIndex.entityPrecise.intVal;
            mainHurtbox.healthComponent = bodyPrefab.GetComponent<HealthComponent>();
            mainHurtbox.isBullseye = true;
            mainHurtbox.damageModifier = HurtBox.DamageModifier.Normal;
            mainHurtbox.hurtBoxGroup = hurtBoxGroup;
            mainHurtbox.indexInGroup = 0;
            hurtBoxGroup.hurtBoxes = new HurtBox[] { mainHurtbox };
            hurtBoxGroup.mainHurtBox = mainHurtbox;
            hurtBoxGroup.bullseyeCount = 1;

            return hurtBoxGroup;
        }

        public void CreateDisplayPrefab()
        {
            if(SurvivorDisplayModelPrefab)
            {
                CharacterModel characterModel = SurvivorDisplayModelPrefab.GetComponent<CharacterModel>();
                if(!characterModel) characterModel = SurvivorDisplayModelPrefab.AddComponent<CharacterModel>();
                characterModel.baseRendererInfos = SurvivorHelpers.CharacterRendererInfoSetup(SurvivorDisplayModelPrefab);
            }
        }
    }
}