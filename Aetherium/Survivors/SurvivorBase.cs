using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Survivors
{
    public abstract class SurvivorBase
    {
        //Language Tokens and Text for Survivor
        public abstract string SurvivorName { get; }
        public abstract string SurvivorBodyName { get; } //Alphanumeric Only, no spaces.
        public abstract string SurvivorSubtitle { get; } //A super short description of the character. Literally as few words as possible. "Unholy Paragon" or something.
        public abstract string SurvivorLangToken { get; }
        public abstract string SurvivorDescription { get; }
        public abstract string SurvivorEndingSuccessText { get; } //If you complete the game.
        public abstract string SurvivorEndingFailureText { get; } //If you're dead and someone else does.

        //Need to set Base Values
        public abstract float SurvivorBaseMaxHealth { get; }
        public abstract float SurvivorBaseArmor { get; }
        public abstract float SurvivorBaseMoveSpeed { get; }
        public abstract int SurvivorBaseJumpCount { get; }

        //Optionally set Base Values
        public virtual float SurvivorBaseMaxShield { get; set; } = 0f;
        public virtual float SurvivorBaseRegen { get; set; } = 1f;
        public virtual float SurvivorBaseAcceleration { get; set; } = 80f;
        public virtual float SurvivorBaseJumpPower { get; set; } = 15f;
        public virtual float SurvivorBaseDamage { get; set; } = 12f;
        public virtual float SurvivorBaseCritChance { get; set; } = 1f;
        public virtual float SurvivorBaseAttackSpeed { get; set; } = 1f;
        public virtual float SurvivorSprintSpeedMultiplier { get; set; } = 1.45f;

        //Base Growth Stats (Optional)
        public virtual float SurvivorHealthGrowth { get; set; } = 100f * 0.3f;
        public virtual float SurvivorRegenGrowth { get; set; } = 1f * 0.2f;
        public virtual float SurvivorArmorGrowth { get; set; } = 0f;
        public virtual float SurvivorShieldGrowth { get; set; } = 0f;
        public virtual float SurvivorDamageGrowth { get; set; } = 12f * 0.2f;
        public virtual float SurvivorAttackSpeedGrowth { get; set; } = 0f;
        public virtual float SurvivorCritGrowth { get; set; } = 0f;
        public virtual float SurvivorMoveSpeedGrowth { get; set; } = 0f;
        public virtual float SurvivorJumpPowerGrowth { get; set; } = 0f;

        //Graphics
        public abstract GameObject SurvivorBodyModelPrefab { get; } //Playable Model
        public abstract GameObject SurvivorDisplayModelPrefab { get; } //Lobby Model
        public abstract Texture SurvivorPortraitIcon { get; }
        public virtual GameObject SurvivorCrosshair { get; set; } = null;
        public virtual GameObject SurvivorPodPrefab { get; set; } = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
        public virtual Color SurvivorLogbookColor { get; set; } = Color.white;

        //Camera and Positionings
        public virtual Vector3 ModelBasePosition { get; set; } = new Vector3(0f, -0.92f, 0f);
        public virtual Vector3 AimOriginPosition { get; set; } = new Vector3(0f, 1.6f, 0f);

        public CharacterCameraParams CharacterCameraParameters;
        public virtual Vector3 CameraPivotPosition { get; set; } = new Vector3(0f, 0.8f, 0);
        public virtual float CameraPivotVerticalOffset { get; set; } = 1.37f;
        public virtual float CameraZoomInOutDepth { get; set; } = -10f;

        //Character Entity States
        public abstract Type SurvivorMainState { get; }
        public virtual Type SurvivorSpawnState { get; }

        //Important
        public SurvivorDef SurvivorDef;
        public GameObject SurvivorBodyPrefab;
        public virtual string CharacterNameToClone { get; set; } = "Commando"; //Modding's favorite base character.
        public virtual UnlockableDef SurvivorUnlockDef { get; set; } = null;
        public virtual float DesiredSelectScreenSortPosition { get; set; } = 100f; //Desired place in the Character Select Screen
        public virtual CharacterBody.BodyFlags SurvivorBodyFlags { get; set; } = CharacterBody.BodyFlags.ImmuneToExecutes; //Used for specific condition immunity/vulnerability, and other things.
        public virtual HullClassification SurvivorHullClassification { get; set; } = HullClassification.Human;
        public PhysicMaterial RagdollMaterial;

        public abstract void Init(ConfigFile config);

        protected void CreateLang()
        {
            LanguageAPI.Add("SURVIVOR_" + SurvivorLangToken + "_NAME", SurvivorName);
            LanguageAPI.Add("SURVIVOR_" + SurvivorLangToken + "_BODY_NAME", SurvivorBodyName);
            LanguageAPI.Add("SURVIVOR_" + SurvivorLangToken + "_BODY_SUBTITLE", SurvivorSubtitle);
            LanguageAPI.Add("SURVIVOR_" + SurvivorLangToken + "_DESCRIPTION", SurvivorDescription);
            LanguageAPI.Add("SURVIVOR_" + SurvivorLangToken + "_OUTRO_TEXT", SurvivorEndingSuccessText);
            LanguageAPI.Add("SURVIVOR_" + SurvivorLangToken + "_OUTRO_FAILURE_TEXT", SurvivorEndingFailureText);
        }

        protected void CreateBodyAndDisplay()
        {
            SurvivorBodyPrefab = CreateCharacterPrefab();

            var CharacterBody = SurvivorBodyPrefab.GetComponent<CharacterBody>();
            if (!CharacterBody)
            {
                ModLogger.LogError($"Something went wrong with the CharacterPrefabSetup, we somehow do not have a Character Body component for {SurvivorBodyName}! Aborting!");
                return;
            }

            CreateDisplayPrefab();
        }

        protected void CreateSurvivor()
        {
            if (!SurvivorBodyPrefab)
            {
                ModLogger.LogError($"We were unable to find the Body Prefab for {SurvivorName}. Assuming that CharacterBodyPrefab setup has failed. Aborting registration of Def!");
                return;
            }

            var expansionRequirement = SurvivorBodyPrefab.AddComponent<ExpansionRequirementComponent>();
            expansionRequirement.requiredExpansion = AetheriumExpansionDef;
            R2API.ContentAddition.AddBody(SurvivorBodyPrefab);

            SurvivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            SurvivorDef.bodyPrefab = SurvivorBodyPrefab;
            SurvivorDef.displayPrefab = SurvivorDisplayModelPrefab;
            SurvivorDef.primaryColor = SurvivorLogbookColor;

            SurvivorDef.cachedName = SurvivorBodyModelPrefab.name.Replace("Body", "");
            SurvivorDef.displayNameToken = "SURVIVOR_" + SurvivorLangToken + "_NAME";
            SurvivorDef.descriptionToken = "SURVIVOR_" + SurvivorLangToken + "_DESCRIPTION";
            SurvivorDef.outroFlavorToken = "SURVIVOR_" + SurvivorLangToken + "_OUTRO_TEXT";
            SurvivorDef.mainEndingEscapeFailureFlavorToken = "SURVIVOR_" + SurvivorLangToken + "_OUTRO_FAILURE_TEXT";

            SurvivorDef.desiredSortPosition = DesiredSelectScreenSortPosition;

            if (SurvivorUnlockDef)
            {
                SurvivorDef.unlockableDef = SurvivorUnlockDef;
            }

            var survivorSuccess = R2API.ContentAddition.AddSurvivorDef(SurvivorDef);

        }

        public virtual void CreateCharacterMaster() { }

        public virtual void CreateEntityStateMachine()
        {
            if (SurvivorBodyPrefab)
            {
                SurvivorBodyPrefab.GetComponent<EntityStateMachine>().mainStateType = new EntityStates.SerializableEntityStateType(SurvivorMainState);
                R2API.ContentAddition.AddEntityState(SurvivorMainState, out var succeeded);

                if (SurvivorSpawnState != null)
                {
                    SurvivorBodyPrefab.GetComponent<EntityStateMachine>().initialStateType = new EntityStates.SerializableEntityStateType(SurvivorSpawnState);
                    R2API.ContentAddition.AddEntityState(SurvivorMainState, out var success);
                }
            }
        }

        public abstract void CreateSkills();

        public virtual void CreateHurtboxes() { }

        public virtual void CreateHitboxes() { }

        public virtual void CreateSkins() { }

        public abstract void CreateItemDisplays();

        public abstract void Hooks();

        #region Character Prefab Setup Methods
        /// <summary>
        /// This is called after creating the language tokens, and then the survivor definition.
        /// This sets up the survivor's character body prefab.
        /// </summary>
        public GameObject CreateCharacterPrefab()
        {

            GameObject ClonedBody = RoR2.LegacyResourcesAPI.Load<GameObject>($"Prefabs/CharacterBodies/{CharacterNameToClone}Body");
            if (!ClonedBody)
            {
                ModLogger.LogError($"{CharacterNameToClone}Body doesn't reference anything in the base game, aborting character set up.");
                return null;
            }

            GameObject NewBodyPrefab = PrefabAPI.InstantiateClone(ClonedBody, SurvivorBodyName); //Our Prefab for the Body, cloned from Commando's.

            //Character Body Setup
            CharacterBody CharacterBody = CharacterBodySetup(NewBodyPrefab);


            if (!SurvivorBodyModelPrefab)
            {
                ModLogger.LogError($"There's no model for this survivor, we can't continue. Aborting!");
                return null;
            }

            //Model Setup
            var model = PrefabAPI.InstantiateClone(SurvivorBodyModelPrefab, SurvivorBodyModelPrefab.name, false); //Our character model prefab.
            CharacterChildLocatorSwap(model);
            Transform ModelBaseTransform = ModelTransformSetup(NewBodyPrefab, model.transform, ModelBasePosition, CameraPivotPosition, AimOriginPosition);
            ModelLocator ModelLocator = ModelLocatorSetup(NewBodyPrefab, ModelBaseTransform, model.transform);
            CharacterDirection CharacterDirection = CharacterDirectionSetup(NewBodyPrefab, ModelBaseTransform, model.transform);
            FootstepHandler FootstepHandler = FootstepHandlerSetup(model);
            RagdollController ragdollController = RagdollSetup(model);
            SetupCharacterModel(NewBodyPrefab);

            //Camera and Aiming Setup
            CameraTargetParams CameraTargetParams = CameraTargetSetup(NewBodyPrefab);
            AimAnimator AimAnimator = AimAnimatorSetup(NewBodyPrefab, model);

            //Character Physics Setup
            CapsuleCollider CapsuleCollider = BasicCapsuleColliderSetup(NewBodyPrefab);
            HurtBoxGroup MainHurtboxGroup = MainHurtboxSetup(NewBodyPrefab, model);

            return NewBodyPrefab;
        }

        /// <summary>
        /// Creates the Character Body component for our survivor based on our defined fields in this class.
        /// </summary>
        /// <param name="bodyPrefab">The Body Prefab that should contain a premade character body component for us to modify.</param>
        /// <returns>The finished Character Body.</returns>
        public CharacterBody CharacterBodySetup(GameObject bodyPrefab)
        {
            var characterBody = bodyPrefab.GetComponent<CharacterBody>();
            if (!characterBody)
            {
                ModLogger.LogError($"{SurvivorBodyName} character body setup somehow couldn't get a character body from the referenced base model. Aborting!");
                return null;
            }

            //Character Identity
            characterBody.name = SurvivorName;
            characterBody.baseNameToken = "SURVIVOR_" + SurvivorLangToken + "_NAME";
            characterBody.subtitleNameToken = "SURVIVOR_" + SurvivorLangToken + "_BODY_SUBTITLE";
            characterBody.portraitIcon = SurvivorPortraitIcon;
            characterBody.bodyColor = SurvivorLogbookColor;

            ///Character Crosshair and Pod
            characterBody._defaultCrosshairPrefab = SurvivorCrosshair;
            characterBody.hideCrosshair = false;
            characterBody.preferredPodPrefab = SurvivorPodPrefab;

            //Character Stats
            characterBody.baseMaxHealth = SurvivorBaseMaxHealth;
            characterBody.baseMaxShield = SurvivorBaseMaxShield;
            characterBody.baseRegen = SurvivorBaseRegen;
            characterBody.baseArmor = SurvivorBaseArmor;
            characterBody.baseDamage = SurvivorBaseDamage;
            characterBody.baseAttackSpeed = SurvivorBaseAttackSpeed;
            characterBody.baseCrit = SurvivorBaseCritChance;
            characterBody.baseMoveSpeed = SurvivorBaseMoveSpeed;
            characterBody.baseJumpPower = SurvivorBaseJumpPower;

            //Miscellaneous Character Stats
            characterBody.baseAcceleration = SurvivorBaseAcceleration;
            characterBody.baseJumpCount = SurvivorBaseJumpCount;
            characterBody.sprintingSpeedMultiplier = SurvivorSprintSpeedMultiplier;

            //Other Miscellaneous Setups
            characterBody.bodyFlags = SurvivorBodyFlags;
            characterBody.hullClassification = SurvivorHullClassification;
            characterBody.rootMotionInMainState = false;
            characterBody.isChampion = false;

            return characterBody;
        }

        public void CharacterChildLocatorSwap(GameObject model)
        {
            var childLocatorCustom = model.GetComponent<ChildLocatorCustom>();
            if (childLocatorCustom)
            {
                var childLocator = model.AddComponent<ChildLocator>();
                childLocator.transformPairs = childLocatorCustom.transformPairs.Select(x => new ChildLocator.NameTransformPair(){ name = x.name, transform = x.transform }).ToArray();
                UnityEngine.Object.DestroyImmediate(childLocatorCustom);
            }

            return;
        }

        /// <summary>
        /// Sets up the transforms for our survivor prefab.
        /// </summary>
        /// <param name="basePrefab">The Body Prefab for our cloned body that we're going to edit.</param>
        /// <param name="modelTransform">The transform from the character model we cloned before calling this.</param>
        /// <param name="modelBasePosition">The base lower position of our character model.</param>
        /// <param name="cameraPivotPosition">Where the camera will rotate around on the character.</param>
        /// <param name="aimOriginPosition">Where we begin our aiming for projectiles and attacks from.</param>
        /// <returns>Completed model transform setup for the survivor prefab.</returns>
        public Transform ModelTransformSetup(GameObject bodyPrefab, Transform modelTransform, Vector3 modelBasePosition, Vector3 cameraPivotPosition, Vector3 aimOriginPosition)
        {
            for (int i = bodyPrefab.transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(bodyPrefab.transform.GetChild(i).gameObject); //Destroy the pre-existing commando transforms.
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

        /// <summary>
        /// Sets up the Model Locator component for our body prefab. This tells the game's components where our model transform is
        /// on the prefab for our character body. Used heavily with visual components later on in survivor setup.
        /// </summary>
        /// <param name="bodyPrefab">The prefab we set a CharacterBody up on.</param>
        /// <param name="modelBaseTransform">The result of the ModelTransformSetup method above.</param>
        /// <param name="modelTransform">The transform of the model we cloned before calling ModelTransformSetup.</param>
        /// <returns></returns>
        public ModelLocator ModelLocatorSetup(GameObject bodyPrefab, Transform modelBaseTransform, Transform modelTransform)
        {
            ModelLocator modelLocator = bodyPrefab.GetComponent<ModelLocator>();
            if (!modelLocator || !modelBaseTransform || !modelTransform)
            {
                ModLogger.LogError($"Couldn't find a model locator component or one of the associated transforms on the body prefab for {SurvivorBodyName}, aborting!");
                return null;
            }

            modelLocator.modelBaseTransform = modelBaseTransform;
            modelLocator.modelTransform = modelTransform;

            return modelLocator;
        }

        /// <summary>
        /// Character Direction controls the actual facing of the model, this sets that component up.
        /// </summary>
        /// <param name="prefab">The prefab we set the Character Body up on.</param>
        /// <param name="modelBaseTransform">The root transform we created in ModelTransformSetup.</param>
        /// <param name="modelTransform">The transform of the Survivor Model prefab.</param>
        public CharacterDirection CharacterDirectionSetup(GameObject prefab, Transform modelBaseTransform, Transform modelTransform)
        {
            if (!prefab.GetComponent<CharacterDirection>())
                return null;

            CharacterDirection characterDirection = prefab.GetComponent<CharacterDirection>();
            characterDirection.targetTransform = modelBaseTransform;
            characterDirection.overrideAnimatorForwardTransform = null;
            characterDirection.rootMotionAccumulator = null;
            characterDirection.modelAnimator = modelTransform.GetComponent<Animator>();
            characterDirection.driveFromRootRotation = false;
            characterDirection.turnSpeed = 720f;

            return characterDirection;
        }

        /// <summary>
        /// Sets up what happens when our character steps around the map, both visually and audially.
        /// </summary>
        /// <param name="model">Our character model, the one we cloned before calling ModelTransformSetup.</param>
        /// <returns>The footstep handler component, after it has been set up.</returns>
        public FootstepHandler FootstepHandlerSetup(GameObject model)
        {
            if (!model)
            {
                ModLogger.LogError($"No model object found while setting up the Footstep Handler for {SurvivorBodyName}! Aborting!");
                return null;
            }

            FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/GenericFootstepDust");

            return footstepHandler;
        }

        /// <summary>
        /// Sets up the ragdoll, used when we're not animating or forcing the model at all (E.g. On death)
        /// </summary>
        /// <param name="model">The model prefab that we cloned before ModelTransformSetup</param>
        /// <returns>The completed ragdoll controller for the survivor.</returns>
        public RagdollController RagdollSetup(GameObject model)
        {
            RagdollController ragdollController = model.GetComponent<RagdollController>();

            if (!ragdollController)
            {
                ModLogger.LogError($"No ragdoll controller was found within the model provided for {SurvivorBodyName} in RagdollSetup, aborting!");
                return null;
            }

            if (RagdollMaterial == null)
            {
                RagdollMaterial = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<RagdollController>().bones[1].GetComponent<Collider>().material;
            }

            foreach (Transform boneTransform in ragdollController.bones)
            {
                if (boneTransform)
                {
                    boneTransform.gameObject.layer = LayerIndex.ragdoll.intVal;
                    Collider boneCollider = boneTransform.GetComponent<Collider>();
                    if (boneCollider)
                    {
                        boneCollider.material = RagdollMaterial;
                        boneCollider.sharedMaterial = RagdollMaterial;
                    }
                }
            }

            return ragdollController;
        }

        /// <summary>
        /// Sets up the character's rendererinfos. These are used to display the materials on the character, as well as let other visuals affect them.
        /// </summary>
        /// <param name="characterPrefab">The character body prefab.</param>
        /// <param name="debugMode">Whether or not we're using the Material Controller Component to affect this at runtime.</param>
        public void SetupCharacterModel(GameObject characterPrefab, bool debugMode = false)
        {
            if (characterPrefab)
            {
                var modelLocator = characterPrefab.GetComponent<ModelLocator>();
                if (modelLocator)
                {
                    var modelTransform = modelLocator.modelTransform;
                    if (modelTransform)
                    {
                        var characterModel = modelTransform.gameObject.GetComponent<CharacterModel>();
                        if (!characterModel)
                        {
                            characterModel = modelTransform.gameObject.AddComponent<CharacterModel>();
                        }

                        characterModel.body = characterPrefab.GetComponent<CharacterBody>();
                        characterModel.autoPopulateLightInfos = true;
                        characterModel.invisibilityCount = 0;
                        characterModel.temporaryOverlays = new List<TemporaryOverlay>();
                        var modelObject = characterModel.gameObject;

                        characterModel.baseRendererInfos = SurvivorHelpers.CharacterRendererInfoSetup(modelObject);
                        return;
                    }
                }
            }

            ModLogger.LogError($"Character RenderInfos and Model setup cannot proceed as we're missing some necessary component!");
            return;
        }

        /// <summary>
        /// Sets up the target for the camera on the survivor. Handles the way the camera works in game.
        /// </summary>
        /// <param name="bodyPrefab">The prefab we set the Character Body up on.</param>
        /// <returns>A set up CameraTargetParams component for the survivor.</returns>
        public CameraTargetParams CameraTargetSetup(GameObject bodyPrefab)
        {
            if (!bodyPrefab || !bodyPrefab.transform.Find("CameraPivot"))
            {
                ModLogger.LogError($"Body Prefab for {SurvivorBodyName} is null or we have no Camera Pivot transform, we can't set the camera up! Aborting!");
                return null;
            }            

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
            else
            {
                ModLogger.LogError($"Failed to retrieve Camera Target Params for {SurvivorBodyName}, can't set camera up! Aborting!");
                return null;
            }

            return cameraTargetParams;
        }

        /// <summary>
        /// This sets up how our character actually looks and aims around the environment, visually.
        /// </summary>
        /// <param name="bodyPrefab">The prefab we set the character body up on.</param>
        /// <param name="model">The visual model prefab we cloned before calling ModelTransformSetup.</param>
        /// <returns>The character's aim animator.</returns>
        public AimAnimator AimAnimatorSetup(GameObject bodyPrefab, GameObject model)
        {
            if(!bodyPrefab || !model)
            {
                ModLogger.LogError($"No valid body prefab or model prefab was fed into Aim Animator setup for {SurvivorBodyName}! Aborting!");
                return null;
            }

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

        /// <summary>
        /// Sets up a basic capsule collider for the survivor.
        /// </summary>
        /// <param name="bodyPrefab">The prefab we set the Character Body up on.</param>
        /// <returns></returns>
        public CapsuleCollider BasicCapsuleColliderSetup(GameObject bodyPrefab)
        {
            CapsuleCollider capsuleCollider = bodyPrefab.GetComponent<CapsuleCollider>();
            if (!bodyPrefab)
            {
                ModLogger.LogError($"The body prefab for {SurvivorBodyName} has no Capsule Collider. Can't set up the physics for it! Aborting!");
                return null;
            }

            capsuleCollider.center = new Vector3(0f, 0f, 0f);
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.direction = 1;

            return capsuleCollider;
        }

        /// <summary>
        /// Create the main hurtbox group, or the boxes that will determine what hits us where on our character.
        /// </summary>
        /// <param name="bodyPrefab">The prefab we set the Character Body up on.</param>
        /// <param name="modelPrefab">The model we had cloned before calling ModelTransformSetup.</param>
        /// <returns>Our main hurtbox for the character.</returns>
        public HurtBoxGroup MainHurtboxSetup(GameObject bodyPrefab, GameObject modelPrefab)
        {
            ChildLocator childLocator = modelPrefab.GetComponent<ChildLocator>();

            if (!childLocator || !childLocator.FindChild("MainHurtbox"))
            {
                ModLogger.LogError($"Could not find a Child Locator component on {SurvivorBodyName}'s Model Prefab or couldn't find a transform called 'MainHurtbox' in its hierarchy! Aborting!");
                return null;
            }

            HurtBoxGroup hurtBoxGroup = modelPrefab.AddComponent<HurtBoxGroup>();
            HurtBox mainHurtbox = childLocator.FindChild("MainHurtbox").gameObject.AddComponent<HurtBox>();
            mainHurtbox.gameObject.layer = LayerIndex.entityPrecise.intVal;
            mainHurtbox.healthComponent = bodyPrefab.GetComponent<HealthComponent>();
            mainHurtbox.isBullseye = true;
            mainHurtbox.damageModifier = HurtBox.DamageModifier.Normal;
            mainHurtbox.hurtBoxGroup = hurtBoxGroup;
            mainHurtbox.indexInGroup = 0;

            hurtBoxGroup.hurtBoxes = new HurtBox[]
            {
                mainHurtbox
            };

            hurtBoxGroup.mainHurtBox = mainHurtbox;
            hurtBoxGroup.bullseyeCount = 1;

            return hurtBoxGroup;
        }
        #endregion

        #region Character Display Prefab Setup
        public void CreateDisplayPrefab()
        {
            if (SurvivorDisplayModelPrefab)
            {
                CharacterModel characterModel = SurvivorDisplayModelPrefab.GetComponent<CharacterModel>();
                if (!characterModel)
                {
                    characterModel = SurvivorDisplayModelPrefab.AddComponent<CharacterModel>();
                }
                characterModel.baseRendererInfos = SurvivorHelpers.CharacterRendererInfoSetup(SurvivorDisplayModelPrefab);
            }
        }
        #endregion

    }
}
