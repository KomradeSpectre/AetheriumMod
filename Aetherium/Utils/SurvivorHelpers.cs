using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Utils
{
    internal class SurvivorHelpers
    {
        public static CharacterModel.RendererInfo[] CharacterRendererInfoSetup(GameObject model, bool debugmode = true)
        {
            List<Renderer> AllRenderers = new List<Renderer>();

            var meshRenderers = model.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers.Length > 0) { AllRenderers.AddRange(meshRenderers); }

            var skinnedMeshRenderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers.Length > 0) { AllRenderers.AddRange(skinnedMeshRenderers); }

            CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[AllRenderers.Count];

            for (int i = 0; i < AllRenderers.Count; i++)
            {
                if (debugmode)
                {
                    var controller = AllRenderers[i].gameObject.AddComponent<MaterialControllerComponents.HGControllerFinder>();
                    controller.Renderer = AllRenderers[i];
                }

                renderInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = AllRenderers[i] is SkinnedMeshRenderer ? AllRenderers[i].sharedMaterial : AllRenderers[i].material,
                    renderer = AllRenderers[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false //We allow the mesh to be affected by overlays like OnFire or PredatoryInstinctsCritOverlay.
                };
            }

            return renderInfos;
        }

        public static SkillLocator CreateBasicSkillFamilies(GameObject bodyPrefab, string characterLangToken)
        {
            if (!bodyPrefab)
            {
                ModLogger.LogError($"We were fed a bad prefab while setting up a basic skill family! Aborting!");
                return null;
            }
            foreach (GenericSkill obj in bodyPrefab.GetComponentsInChildren<GenericSkill>())
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }

            SkillLocator skillLocator = bodyPrefab.GetComponent<SkillLocator>();
            if (!skillLocator)
            {
                ModLogger.LogError($"We have no skill locator, and cannot proceed with basic skill creation of {bodyPrefab.name}");
                return null;
            }

            //Primary Skill Family Setup
            GenericSkill primarySkill = bodyPrefab.AddComponent<GenericSkill>();
            primarySkill.skillName = "Primary";
            primarySkill.hideInCharacterSelect = false;

            SkillFamily primaryFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (primaryFamily as ScriptableObject).name = "AETHERIUM_SKILL_FAMILY_" + characterLangToken + "_PRIMARY";
            primaryFamily.variants = new SkillFamily.Variant[0];

            primarySkill._skillFamily = primaryFamily;

            //Secondary Skill Family Setup
            GenericSkill secondarySkill = bodyPrefab.AddComponent<GenericSkill>();
            secondarySkill.skillName = "Secondary";
            secondarySkill.hideInCharacterSelect = false;

            SkillFamily secondaryFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (secondaryFamily as ScriptableObject).name = "AETHERIUM_SKILL_FAMILY_" + characterLangToken + "_SECONDARY";
            secondaryFamily.variants = new SkillFamily.Variant[0];

            secondarySkill._skillFamily = secondaryFamily;

            //Utility Skill Family Setup
            GenericSkill utilitySkill = bodyPrefab.AddComponent<GenericSkill>();
            utilitySkill.skillName = "Utility";
            utilitySkill.hideInCharacterSelect = false;

            SkillFamily utilityFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (utilityFamily as ScriptableObject).name = "AETHERIUM_SKILL_FAMILY_" + characterLangToken + "_UTILITY";
            utilityFamily.variants = new SkillFamily.Variant[0];

            utilitySkill._skillFamily = utilityFamily;

            //Special Skill Family Setup
            GenericSkill specialSkill = bodyPrefab.AddComponent<GenericSkill>();
            specialSkill.skillName = "Special";
            specialSkill.hideInCharacterSelect = false;

            SkillFamily specialFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (specialFamily as ScriptableObject).name = "AETHERIUM_SKILL_FAMILY_" + characterLangToken + "_SPECIAL";
            specialFamily.variants = new SkillFamily.Variant[0];

            specialSkill._skillFamily = specialFamily;

            skillLocator.primary = primarySkill;
            skillLocator.secondary = secondarySkill;
            skillLocator.utility = utilitySkill;
            skillLocator.special = specialSkill;

            return skillLocator;
        }

        public static void SetupAttackHitbox(GameObject prefab, Transform hitboxTransform, string hitboxName)
        {
            ModLogger.LogError($"Prefab: {prefab}\nHitBox Transform: {hitboxTransform}\nHitBox Name: {hitboxName}");
            HitBoxGroup hitBoxGroup = prefab.AddComponent<HitBoxGroup>();

            HitBox hitBox = hitboxTransform.gameObject.AddComponent<HitBox>();
            hitboxTransform.gameObject.layer = LayerIndex.projectile.intVal;

            hitBoxGroup.hitBoxes = new HitBox[]
            {
                hitBox
            };

            hitBoxGroup.groupName = hitboxName;
            ModLogger.LogError($"HitboxGroup: {hitBoxGroup}\nHitBox: {hitBox}");
        }

        public static void AddSkillToFamily(SkillFamily skillFamily, SkillDef skillDef, UnlockableDef unlockableDef = null)
        {
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);

            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableDef = unlockableDef,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        /// <summary>
        /// This method gets the dot product of the move vector and aim direction to determine the player's input direction if possible.
        /// </summary>
        /// <param name="inputBank">The InputBank of the character.</param>
        /// <returns>The normalized dot product of the movement direction. From this we can determine movement input direction.</returns>
        public static string InputForwardOrBack(InputBankTest inputBank)
        {
            if (!inputBank)
            {
                ModLogger.LogError($"We have no inputBank, we can't determine the dot product from a non-existing move vector!");
                return "null";
            }

            var dotProduct = Vector3.Dot(inputBank.aimDirection.normalized, inputBank.moveVector.normalized);
            if (dotProduct >= 0)
            {
                return "Forward";
            }
            else if (dotProduct < 0)
            {
                return "Back";
            }
            else
            {
                return "null";
            }
        }
    }
}
