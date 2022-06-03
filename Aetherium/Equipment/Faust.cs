using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Aetherium.Effect;
using static Aetherium.AetheriumPlugin;
using RoR2.CharacterSpeech;
using RoR2.Skills;
using Aetherium.Utils;
using System.Linq;
using RoR2.Projectile;
using UnityEngine.Networking;
using Aetherium.Utils.Components;
using RoR2.UI;

namespace Aetherium.Equipment
{
    internal class Faust : EquipmentBase<Faust>
    {
        public override string EquipmentName => "Faust";

        public override string EquipmentLangTokenName => "FAUST";

        public override string EquipmentPickupDesc => "On use, throw a homing hat that sticks to enemies. Steal gold and xp from them every hit, and seal one of their skills while they wear the hat.";

        public override string EquipmentFullDescription => "";

        public override string EquipmentLore => "";

        public override bool UseTargeting => true;

        public override GameObject EquipmentModel => MainAssets.LoadAsset<GameObject>("PickupFaust.prefab");

        public override Sprite EquipmentIcon => MainAssets.LoadAsset<Sprite>("FeatheredPlumeIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public static GameObject OrbFaust;

        public ItemDef FaustItem;

        public ItemDef DeactivatedFaustItem;

        public List<ItemDef> FaustVictimItems = new List<ItemDef>();

        public static SkillDef BrokenSkill;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateSkill();
            CreateTargeting();
            CreateProjectile();
            CreateEquipment();
            CreateFaustItem();
            CreateFaustDisplayRules();
            Hooks();
        }

        private void CreateSkill()
        {
            BrokenSkill = new SkillDef
            {
                skillName = "BROKEN",
                skillNameToken = "AETHERIUM_EQUIPMENT_" + EquipmentLangTokenName + "_BROKEN_SKILL_NAME",
                skillDescriptionToken = "AETHERIUM_EQUIPMENT_" + EquipmentLangTokenName + "_BROKEN_DESCRIPTION",
                icon = MainAssets.LoadAsset<Sprite>("SealedSkill.png"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(MyEntityStates.Faust.BrokenSkillState)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = 1f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[] { },
            };
            R2API.ContentAddition.AddSkillDef(BrokenSkill);
        }

        private void CreateTargeting()
        {
            TargetingIndicatorPrefabBase = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/WoodSpriteIndicator"), "FaustHatIndicator", false);

            var bootlegAnimController = TargetingIndicatorPrefabBase.transform.Find("Holder").gameObject.AddComponent<BootlegAnimationController>();
            bootlegAnimController.Duration = 0.1f;
            bootlegAnimController.Sprites = new Sprite[]
            {
                MainAssets.LoadAsset<Sprite>("FaustHatReticle1.png"),
                MainAssets.LoadAsset<Sprite>("FaustHatReticle2.png"),
                MainAssets.LoadAsset<Sprite>("FaustHatReticle3.png"),
                MainAssets.LoadAsset<Sprite>("FaustHatReticle4.png"),
            };

            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().sprite = MainAssets.LoadAsset<Sprite>("FaustHatReticule.png");
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.identity;
            TargetingIndicatorPrefabBase.GetComponentInChildren<TMPro.TextMeshPro>().color = new Color(1f, 1, 1f);
        }

        private void CreateProjectile()
        {
            OrbFaust = PrefabAPI.InstantiateClone(MainAssets.LoadAsset<GameObject>("HatOrb.prefab"), "FaustOrb", false);

            var networkId = OrbFaust.AddComponent<NetworkIdentity>();

            var scaleTransform = OrbFaust.transform.Find("EmptyTransformer/ScaleTransform");
            scaleTransform.localScale = new Vector3(3, 3, 3);

            var orbEffect = OrbFaust.AddComponent<OrbEffect>();
            orbEffect.startVelocity1 = new Vector3(-12, 0, 8);
            orbEffect.startVelocity2 = new Vector3(12, 0, 16);
            orbEffect.endVelocity1 = new Vector3(0, 12, 0);
            orbEffect.endVelocity2 = new Vector3(0, 12, 0);
            orbEffect.movementCurve = AnimationCurve.Linear(0, 0, 1, 1);
            orbEffect.faceMovement = true;

            var detachParticles = OrbFaust.AddComponent<DetachParticleOnDestroyAndEndEmission>();
            detachParticles.particleSystem = OrbFaust.GetComponentInChildren<ParticleSystem>();

            var effectComponent = OrbFaust.AddComponent<EffectComponent>();

            var vfxAttributes = OrbFaust.AddComponent<VFXAttributes>();
            vfxAttributes.vfxIntensity = RoR2.VFXAttributes.VFXIntensity.Low;
            vfxAttributes.vfxPriority = RoR2.VFXAttributes.VFXPriority.Medium;

            PrefabAPI.RegisterNetworkPrefab(OrbFaust);
            R2API.ContentAddition.AddEffect(OrbFaust);
            OrbAPI.AddOrb(typeof(FaustOrb));
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("DisplayFaust.prefab");
            var itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab, true);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlAutimecia", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HatBone",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
            });
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.38538F, -0.00003F),
                    localAngles = new Vector3(0, 0F, 0F),
                    localScale = new Vector3(0.72529F, 0.72529F, 0.72529F)
                },
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.32967F, -0.07252F),
                    localAngles = new Vector3(345.3851F, 0F, 0F),
                    localScale = new Vector3(0.50198F, 0.50198F, 0.50198F)
                },
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.22734F, 2.94954F, 1.68321F),
                    localAngles = new Vector3(300F, 180F, 10F),
                    localScale = new Vector3(3.84434F, 3.84434F, 3.84434F)
                },
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0F, 0.14441F, 0.09523F),
                    localAngles = new Vector3(30F, 0F, 0F),
                    localScale = new Vector3(0.5261F, 0.5261F, 0.5261F)
                },
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.16698F, -0.06869F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.60452F, 0.60452F, 0.66428F)
                },
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.23322F, 0.06644F),
                    localAngles = new Vector3(25F, 0F, 0F),
                    localScale = new Vector3(0.59409F, 0.59409F, 0.59409F)
                },
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0F, 1.85292F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(3.10717F, 2.98671F, 3.10717F)
                },
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.01055F, 0.25472F, 0.03823F),
                    localAngles = new Vector3(10F, 0F, 350F),
                    localScale = new Vector3(0.60644F, 0.80859F, 0.60644F)
                },
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 3.87662F, 1.75981F),
                    localAngles = new Vector3(290.8887F, 0F, 180F),
                    localScale = new Vector3(3.81337F, 3.81337F, 3.81337F)
                },
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.01024F, 0.32544F, 0.04617F),
                    localAngles = new Vector3(316.9598F, 0F, 0F),
                    localScale = new Vector3(0.37438F, 0.56157F, 0.37438F)
                },
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.03697F, 0.22621F, 0.01845F),
                    localAngles = new Vector3(5F, 0F, 340F),
                    localScale = new Vector3(0.4525F, 0.4525F, 0.4525F)
                },
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.22797F, -0.02135F),
                    localAngles = new Vector3(10.7447F, 0F, 0F),
                    localScale = new Vector3(0.6425F, 0.6425F, 0.6425F)
                },
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00001F, 0.15809F, 0.00004F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(1.05937F, 1.05937F, 1.05937F)
                },
            });
            rules.Add("mdlBeetle", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.4935F, 0.65063F),
                    localAngles = new Vector3(322.3344F, 180F, 0F),
                    localScale = new Vector3(3.01554F, 3.01554F, 3.01554F)
                },
            });
            rules.Add("mdlBeetleGuard", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00001F, -0.7457F, 2.49654F),
                    localAngles = new Vector3(300F, 0.00001F, 180F),
                    localScale = new Vector3(3.33905F, 16.69523F, 3.33905F)
                },
            });
            rules.Add("mdlBeetleQueen", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 4.09684F, 0.65182F),
                    localAngles = new Vector3(3.24317F, 180F, 0F),
                    localScale = new Vector3(8.94569F, 8.94569F, 8.94569F)
                },
            });
            rules.Add("mdlBell", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chain",
                    localPos = new Vector3(0F, 0F, 0F),
                    localAngles = new Vector3(355.8196F, 240.6027F, 188.2107F),
                    localScale = new Vector3(10.40124F, 10.40124F, 10.40124F)
                },
            });
            rules.Add("mdlBison", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.08458F, 0.7156F),
                    localAngles = new Vector3(270.3444F, 0.00001F, 180F),
                    localScale = new Vector3(1.34895F, 1.34895F, 1.34895F)
                },
            });
            rules.Add("mdlBrother", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00001F, 0.07384F, 0.21033F),
                    localAngles = new Vector3(76.38378F, 0F, 0F),
                    localScale = new Vector3(0.51863F, 0.51863F, 0.51863F)
                },
            });
            rules.Add("mdlClayBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "PotLidTop",
                    localPos = new Vector3(0F, 0.6465F, 1.12905F),
                    localAngles = new Vector3(348.5426F, 0F, 0F),
                    localScale = new Vector3(6.51281F, 6.51281F, 6.51281F)
                },
            });
            rules.Add("mdlClayBruiser", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00001F, 0.47472F, 0.09709F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(1.60304F, 1.60304F, 1.60304F)
                },
            });
            rules.Add("mdlMagmaWorm", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.8173F, 0.94391F),
                    localAngles = new Vector3(84.25488F, 180F, 180F),
                    localScale = new Vector3(4.1909F, 4.1909F, 4.1909F)
                },
            });
            rules.Add("mdlGolem", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 1.25522F, -0.48824F),
                    localAngles = new Vector3(352.9799F, 0F, 0F),
                    localScale = new Vector3(5.43554F, 5.43554F, 5.43554F)
                },
            });
            rules.Add("mdlGrandparent", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00001F, -0.66301F, -0.57144F),
                    localAngles = new Vector3(14.06001F, 0F, 0F),
                    localScale = new Vector3(3.1499F, 3.93738F, 3.1499F)
                },
            });
            rules.Add("mdlGravekeeper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00001F, 2.8343F, -0.53367F),
                    localAngles = new Vector3(347.964F, 0F, 0F),
                    localScale = new Vector3(4.26751F, 4.552F, 4.26751F)
                },
            });
            rules.Add("mdlGreaterWisp", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MaskBase",
                    localPos = new Vector3(0F, 0.87668F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(4.28359F, 4.28359F, 4.28359F)
                },
            });
            rules.Add("mdlHermitCrab", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.12039F, 1.60811F, -0.15032F),
                    localAngles = new Vector3(352.0045F, 315.7086F, 0F),
                    localScale = new Vector3(2.45086F, 6.15805F, 2.45086F)
                },
            });
            rules.Add("mdlImp", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Neck",
                    localPos = new Vector3(0F, 0.11066F, 0.01507F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.37164F, 0.37164F, 0.37164F)
                },
            });
            rules.Add("mdlImpBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Neck",
                    localPos = new Vector3(0.00001F, 0.45082F, 0.05234F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(2.23938F, 2.23938F, 2.23938F)
                },
            });
            rules.Add("mdlJellyfish", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hull2",
                    localPos = new Vector3(0.41076F, 1.11547F, 0.16957F),
                    localAngles = new Vector3(10F, 0F, 340F),
                    localScale = new Vector3(4.06308F, 4.06308F, 4.06308F)
                },
            });
            rules.Add("mdlLemurian", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00001F, 2.60764F, -1.29676F),
                    localAngles = new Vector3(275F, 0.00001F, 0.00001F),
                    localScale = new Vector3(6F, 8F, 6F)
                },
            });
            rules.Add("mdlLemurianBruiser", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 1.92613F, 0.92026F),
                    localAngles = new Vector3(270F, 180F, 0F),
                    localScale = new Vector3(2.63389F, 2.63389F, 2.63389F)
                },
            });
            rules.Add("mdlLunarExploder", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MuzzleCore",
                    localPos = new Vector3(0.00701F, 0.85753F, -0.03562F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(3.50634F, 3.50634F, 3.50634F)
                },
            });
            rules.Add("mdlLunarGolem", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 1.26947F, 0.70541F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(2.15346F, 2.15346F, 2.15346F)
                },
            });
            rules.Add("mdlLunarWisp", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Mask",
                    localPos = new Vector3(0F, 0.49385F, 2.99716F),
                    localAngles = new Vector3(80.00003F, 0F, 0F),
                    localScale = new Vector3(6.98348F, 6.98348F, 6.98348F)
                },
            });
            rules.Add("mdlMiniMushroom", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.48783F, 0.29087F, 0F),
                    localAngles = new Vector3(84.63853F, 270F, 0F),
                    localScale = new Vector3(5.30475F, 5.30475F, 5.30475F)
                },
            });
            rules.Add("mdlNullifier", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Muzzle",
                    localPos = new Vector3(0F, 1.60683F, 0.29378F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(6.50117F, 6.50117F, 6.50117F)
                },
            });
            rules.Add("mdlParent", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-65.54737F, 116.3315F, -0.00005F),
                    localAngles = new Vector3(330F, 90F, 0F),
                    localScale = new Vector3(272.702F, 272.702F, 272.702F)
                },
            });
            rules.Add("mdlRoboBallBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Center",
                    localPos = new Vector3(0F, 1.06607F, -0.28356F),
                    localAngles = new Vector3(0.41209F, 0F, 0F),
                    localScale = new Vector3(5.38626F, 5.38626F, 5.38626F)
                },
            });
            rules.Add("mdlRoboBallMini", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Muzzle",
                    localPos = new Vector3(0F, 0.9528F, -1.37393F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(5.04865F, 5.04865F, 5.04865F)
                },
            });
            rules.Add("mdlScav", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 6.62639F, 2.57275F),
                    localAngles = new Vector3(323.1491F, 180F, 0F),
                    localScale = new Vector3(16.17946F, 16.17946F, 16.17946F)
                },
            });
            rules.Add("mdlTitan", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 6.33197F, 0.39897F),
                    localAngles = new Vector3(20F, 0F, 0F),
                    localScale = new Vector3(10.77975F, 12.9357F, 10.77975F)
                },
            });
            rules.Add("mdlVagrant", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hull",
                    localPos = new Vector3(0F, 1.56133F, -0.22038F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(6.85929F, 6.85929F, 6.85929F)
                },
            });
            rules.Add("mdlVulture", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00002F, 0.95108F, -1.15824F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(4.02466F, 4.02466F, 4.02466F)
                },
            });
            rules.Add("mdlWisp1Mouth", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, -0.08323F, 0.5547F),
                    localAngles = new Vector3(274.373F, 0.00006F, 180F),
                    localScale = new Vector3(2.75877F, 2.75877F, 2.75877F)
                },
            });
            rules.Add("mdlVoidRaidCrab", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00002F, 30.1623F, 0.00004F),
                    localAngles = new Vector3(5.84114F, 0F, 0F),
                    localScale = new Vector3(73.37091F, 73.37091F, 73.37091F)
                },
            });
            rules.Add("mdlFlyingVermin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Body",
                    localPos = new Vector3(0F, 1.36649F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(4.89432F, 4.89432F, 4.89432F)
                },
            });
            rules.Add("mdlClayGrenadier", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Torso",
                    localPos = new Vector3(0F, 0.44041F, 0.00001F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(1.12374F, 1.12374F, 1.12374F)
                },
            });
            rules.Add("mdlGup", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MainBody2",
                    localPos = new Vector3(0F, 0.97533F, -0.1173F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(4.46617F, 4.46617F, 4.46617F)
                },
            });
            rules.Add("mdlMegaConstruct", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Eye",
                    localPos = new Vector3(0F, 0F, 1.52945F),
                    localAngles = new Vector3(270F, 180F, 0F),
                    localScale = new Vector3(8.78305F, 8.78305F, 8.78305F)
                },
            });
            rules.Add("mdlMinorConstruct", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CapTop",
                    localPos = new Vector3(0F, 0.74455F, -0.09024F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(2.684F, 2.684F, 2.684F)
                },
            });
            rules.Add("mdlVermin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, -0.20875F, -0.47719F),
                    localAngles = new Vector3(308.5193F, 180F, 180F),
                    localScale = new Vector3(1.46343F, 1.46343F, 1.46343F)
                },
            });
            rules.Add("mdlVoidBarnacle", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00524F, 0F, -0.66232F),
                    localAngles = new Vector3(0F, 248.4958F, 90F),
                    localScale = new Vector3(2.37544F, 2.37544F, 2.37544F)
                },
            });
            rules.Add("mdlVoidJailer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.78154F, 0F, 0.40017F),
                    localAngles = new Vector3(0F, 8.95706F, 90F),
                    localScale = new Vector3(1.81469F, 1.81469F, 1.81469F)
                },
            });
            rules.Add("mdlVoidMegaCrab", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "BodyBase",
                    localPos = new Vector3(0F, 8.1396F, 2.18922F),
                    localAngles = new Vector3(14.97951F, 0F, 0F),
                    localScale = new Vector3(16.37576F, 16.37576F, 16.37576F)
                },
            });
            rules.Add("AcidLarva", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "BodyBase",
                    localPos = new Vector3(0F, 5.38907F, -1.65954F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(12.05802F, 12.05802F, 12.05802F)
                },
            });
            return rules;
        }

        private void CreateFaustItem()
        {
            LanguageAPI.Add("HIDDEN_ITEM_" + EquipmentLangTokenName + "_FAUST_DEBILITATOR_NAME", "Faust");
            LanguageAPI.Add("HIDDEN_ITEM_" + EquipmentLangTokenName + "_FAUST_DEBILITATOR_PICKUP", "This attire seems to be sealing your power! You feel much weaker!");
            LanguageAPI.Add("HIDDEN_ITEM_" + EquipmentLangTokenName + "_FAUST_DEBILITATOR_DESCRIPTION", $"Your power is sealed by a demonic hat. Each hit done to you will sap [x]% of your gold, [x]% of XP, and will seal [x] of your skills until removed.");

            FaustItem = ScriptableObject.CreateInstance<ItemDef>();
            FaustItem.name = "HIDDEN_ITEM_FAUST_DEBILITATOR";
            FaustItem.nameToken = "HIDDEN_ITEM_" + EquipmentLangTokenName + "_FAUST_DEBILITATOR_NAME";
            FaustItem.pickupToken = "HIDDEN_ITEM_" + EquipmentLangTokenName + "_FAUST_DEBILITATOR_PICKUP";
            FaustItem.descriptionToken = "HIDDEN_ITEM_" + EquipmentLangTokenName + "_FAUST_DEBILITATOR_DESCRIPTION";
            FaustItem.loreToken = "";
            FaustItem.canRemove = false;
            FaustItem.hidden = true;
            FaustItem.deprecatedTier = ItemTier.NoTier;

            DeactivatedFaustItem = ScriptableObject.CreateInstance<ItemDef>();
            DeactivatedFaustItem.name = "HIDDEN_ITEM_FAUST_DEBILITATOR_DEACTIVATED";
            DeactivatedFaustItem.nameToken = "HIDDEN_ITEM_FAUST_DEBILITATOR_DEACTIVATED_NAME";
            DeactivatedFaustItem.pickupToken = "HIDDEN_ITEM_FAUST_DEBILITATOR_DEACTIVATED_PICKUP";
            DeactivatedFaustItem.descriptionToken = "HIDDEN_ITEM_FAUST_DEBILITATOR_DEACTIVATED_DESCRIPTION";
            DeactivatedFaustItem.loreToken = "";
            DeactivatedFaustItem.canRemove = false;
            DeactivatedFaustItem.hidden = true;
            DeactivatedFaustItem.deprecatedTier = ItemTier.NoTier;

            ItemAPI.Add(new CustomItem(FaustItem, CreateFaustDisplayRules()));
            ItemAPI.Add(new CustomItem(DeactivatedFaustItem, new ItemDisplayRule[] { }));
            
        }

        private ItemDisplayRuleDict CreateFaustDisplayRules()
        {
            var victimBodyModelPrefab = MainAssets.LoadAsset<GameObject>("DisplayFaustVictim.prefab");
            var fedoraRainbow = victimBodyModelPrefab.transform.Find("EmptyTransformer/ScaleTransform/Hat").gameObject.AddComponent<RainbowComponent>();
            fedoraRainbow.changeEmission = true;
            fedoraRainbow.hueRate = 1f;
            fedoraRainbow.value = 1.3f;
            var itemDisplay = victimBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemHelpers.ItemDisplaySetup(victimBodyModelPrefab, true);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlAutimecia", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "HatBone",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
            });
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.38538F, -0.00003F),
                    localAngles = new Vector3(0, 0F, 0F),
                    localScale = new Vector3(0.72529F, 0.72529F, 0.72529F)
                },
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.32967F, -0.07252F),
                    localAngles = new Vector3(345.3851F, 0F, 0F),
                    localScale = new Vector3(0.50198F, 0.50198F, 0.50198F)
                },
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.22734F, 2.94954F, 1.68321F),
                    localAngles = new Vector3(300F, 180F, 10F),
                    localScale = new Vector3(3.84434F, 3.84434F, 3.84434F)
                },
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0F, 0.14441F, 0.09523F),
                    localAngles = new Vector3(30F, 0F, 0F),
                    localScale = new Vector3(0.5261F, 0.5261F, 0.5261F)
                },
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.16698F, -0.06869F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.60452F, 0.60452F, 0.66428F)
                },
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.23322F, 0.06644F),
                    localAngles = new Vector3(25F, 0F, 0F),
                    localScale = new Vector3(0.59409F, 0.59409F, 0.59409F)
                },
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0F, 1.85292F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(3.10717F, 2.98671F, 3.10717F)
                },
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.01055F, 0.25472F, 0.03823F),
                    localAngles = new Vector3(10F, 0F, 350F),
                    localScale = new Vector3(0.60644F, 0.80859F, 0.60644F)
                },
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 3.87662F, 1.75981F),
                    localAngles = new Vector3(290.8887F, 0F, 180F),
                    localScale = new Vector3(3.81337F, 3.81337F, 3.81337F)
                },
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.01024F, 0.32544F, 0.04617F),
                    localAngles = new Vector3(316.9598F, 0F, 0F),
                    localScale = new Vector3(0.37438F, 0.56157F, 0.37438F)
                },
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.03697F, 0.22621F, 0.01845F),
                    localAngles = new Vector3(5F, 0F, 340F),
                    localScale = new Vector3(0.4525F, 0.4525F, 0.4525F)
                },
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.22797F, -0.02135F),
                    localAngles = new Vector3(10.7447F, 0F, 0F),
                    localScale = new Vector3(0.6425F, 0.6425F, 0.6425F)
                },
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00001F, 0.15809F, 0.00004F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(1.05937F, 1.05937F, 1.05937F)
                },
            });
            rules.Add("mdlBeetle", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.4935F, 0.65063F),
                    localAngles = new Vector3(322.3344F, 180F, 0F),
                    localScale = new Vector3(3.01554F, 3.01554F, 3.01554F)
                },
            });
            rules.Add("mdlBeetleGuard", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00001F, -0.7457F, 2.49654F),
                    localAngles = new Vector3(300F, 0.00001F, 180F),
                    localScale = new Vector3(3.33905F, 16.69523F, 3.33905F)
                },
            });
            rules.Add("mdlBeetleQueen", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 4.09684F, 0.65182F),
                    localAngles = new Vector3(3.24317F, 180F, 0F),
                    localScale = new Vector3(8.94569F, 8.94569F, 8.94569F)
                },
            });
            rules.Add("mdlBell", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Chain",
                    localPos = new Vector3(0F, 0F, 0F),
                    localAngles = new Vector3(355.8196F, 240.6027F, 188.2107F),
                    localScale = new Vector3(10.40124F, 10.40124F, 10.40124F)
                },
            });
            rules.Add("mdlBison", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.08458F, 0.7156F),
                    localAngles = new Vector3(270.3444F, 0.00001F, 180F),
                    localScale = new Vector3(1.34895F, 1.34895F, 1.34895F)
                },
            });
            rules.Add("mdlBrother", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00001F, 0.07384F, 0.21033F),
                    localAngles = new Vector3(76.38378F, 0F, 0F),
                    localScale = new Vector3(0.51863F, 0.51863F, 0.51863F)
                },
            });
            rules.Add("mdlClayBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "PotLidTop",
                    localPos = new Vector3(0F, 0.6465F, 1.12905F),
                    localAngles = new Vector3(348.5426F, 0F, 0F),
                    localScale = new Vector3(6.51281F, 6.51281F, 6.51281F)
                },
            });
            rules.Add("mdlClayBruiser", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00001F, 0.47472F, 0.09709F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(1.60304F, 1.60304F, 1.60304F)
                },
            });
            rules.Add("mdlMagmaWorm", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.8173F, 0.94391F),
                    localAngles = new Vector3(84.25488F, 180F, 180F),
                    localScale = new Vector3(4.1909F, 4.1909F, 4.1909F)
                },
            });
            rules.Add("mdlGolem", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 1.25522F, -0.48824F),
                    localAngles = new Vector3(352.9799F, 0F, 0F),
                    localScale = new Vector3(5.43554F, 5.43554F, 5.43554F)
                },
            });
            rules.Add("mdlGrandparent", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00001F, -0.66301F, -0.57144F),
                    localAngles = new Vector3(14.06001F, 0F, 0F),
                    localScale = new Vector3(3.1499F, 3.93738F, 3.1499F)
                },
            });
            rules.Add("mdlGravekeeper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00001F, 2.8343F, -0.53367F),
                    localAngles = new Vector3(347.964F, 0F, 0F),
                    localScale = new Vector3(4.26751F, 4.552F, 4.26751F)
                },
            });
            rules.Add("mdlGreaterWisp", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "MaskBase",
                    localPos = new Vector3(0F, 0.87668F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(4.28359F, 4.28359F, 4.28359F)
                },
            });
            rules.Add("mdlHermitCrab", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.12039F, 1.60811F, -0.15032F),
                    localAngles = new Vector3(352.0045F, 315.7086F, 0F),
                    localScale = new Vector3(2.45086F, 6.15805F, 2.45086F)
                },
            });
            rules.Add("mdlImp", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Neck",
                    localPos = new Vector3(0F, 0.11066F, 0.01507F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.37164F, 0.37164F, 0.37164F)
                },
            });
            rules.Add("mdlImpBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Neck",
                    localPos = new Vector3(0.00001F, 0.45082F, 0.05234F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(2.23938F, 2.23938F, 2.23938F)
                },
            });
            rules.Add("mdlJellyfish", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Hull2",
                    localPos = new Vector3(0.41076F, 1.11547F, 0.16957F),
                    localAngles = new Vector3(10F, 0F, 340F),
                    localScale = new Vector3(4.06308F, 4.06308F, 4.06308F)
                },
            });
            rules.Add("mdlLemurian", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00001F, 2.60764F, -1.29676F),
                    localAngles = new Vector3(275F, 0.00001F, 0.00001F),
                    localScale = new Vector3(6F, 8F, 6F)
                },
            });
            rules.Add("mdlLemurianBruiser", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 1.92613F, 0.92026F),
                    localAngles = new Vector3(270F, 180F, 0F),
                    localScale = new Vector3(2.63389F, 2.63389F, 2.63389F)
                },
            });
            rules.Add("mdlLunarExploder", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "MuzzleCore",
                    localPos = new Vector3(0.00701F, 0.85753F, -0.03562F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(3.50634F, 3.50634F, 3.50634F)
                },
            });
            rules.Add("mdlLunarGolem", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 1.26947F, 0.70541F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(2.15346F, 2.15346F, 2.15346F)
                },
            });
            rules.Add("mdlLunarWisp", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Mask",
                    localPos = new Vector3(0F, 0.49385F, 2.99716F),
                    localAngles = new Vector3(80.00003F, 0F, 0F),
                    localScale = new Vector3(6.98348F, 6.98348F, 6.98348F)
                },
            });
            rules.Add("mdlMiniMushroom", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.48783F, 0.29087F, 0F),
                    localAngles = new Vector3(84.63853F, 270F, 0F),
                    localScale = new Vector3(5.30475F, 5.30475F, 5.30475F)
                },
            });
            rules.Add("mdlNullifier", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Muzzle",
                    localPos = new Vector3(0F, 1.60683F, 0.29378F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(6.50117F, 6.50117F, 6.50117F)
                },
            });
            rules.Add("mdlParent", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-65.54737F, 116.3315F, -0.00005F),
                    localAngles = new Vector3(330F, 90F, 0F),
                    localScale = new Vector3(272.702F, 272.702F, 272.702F)
                },
            });
            rules.Add("mdlRoboBallBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Center",
                    localPos = new Vector3(0F, 1.06607F, -0.28356F),
                    localAngles = new Vector3(0.41209F, 0F, 0F),
                    localScale = new Vector3(5.38626F, 5.38626F, 5.38626F)
                },
            });
            rules.Add("mdlRoboBallMini", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Muzzle",
                    localPos = new Vector3(0F, 0.9528F, -1.37393F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(5.04865F, 5.04865F, 5.04865F)
                },
            });
            rules.Add("mdlScav", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 6.62639F, 2.57275F),
                    localAngles = new Vector3(323.1491F, 180F, 0F),
                    localScale = new Vector3(16.17946F, 16.17946F, 16.17946F)
                },
            });
            rules.Add("mdlTitan", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 6.33197F, 0.39897F),
                    localAngles = new Vector3(20F, 0F, 0F),
                    localScale = new Vector3(10.77975F, 12.9357F, 10.77975F)
                },
            });
            rules.Add("mdlVagrant", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Hull",
                    localPos = new Vector3(0F, 1.56133F, -0.22038F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(6.85929F, 6.85929F, 6.85929F)
                },
            });
            rules.Add("mdlVulture", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00002F, 0.95108F, -1.15824F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(4.02466F, 4.02466F, 4.02466F)
                },
            });
            rules.Add("mdlWisp1Mouth", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, -0.08323F, 0.5547F),
                    localAngles = new Vector3(274.373F, 0.00006F, 180F),
                    localScale = new Vector3(2.75877F, 2.75877F, 2.75877F)
                },
            });
            rules.Add("mdlVoidRaidCrab", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00002F, 30.1623F, 0.00004F),
                    localAngles = new Vector3(5.84114F, 0F, 0F),
                    localScale = new Vector3(73.37091F, 73.37091F, 73.37091F)
                },
            });
            rules.Add("mdlFlyingVermin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Body",
                    localPos = new Vector3(0F, 1.36649F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(4.89432F, 4.89432F, 4.89432F)
                },
            });
            rules.Add("mdlClayGrenadier", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Torso",
                    localPos = new Vector3(0F, 0.44041F, 0.00001F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(1.12374F, 1.12374F, 1.12374F)
                },
            });
            rules.Add("mdlGup", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "MainBody2",
                    localPos = new Vector3(0F, 0.97533F, -0.1173F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(4.46617F, 4.46617F, 4.46617F)
                },
            });
            rules.Add("mdlMegaConstruct", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Eye",
                    localPos = new Vector3(0F, 0F, 1.52945F),
                    localAngles = new Vector3(270F, 180F, 0F),
                    localScale = new Vector3(8.78305F, 8.78305F, 8.78305F)
                },
            });
            rules.Add("mdlMinorConstruct", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "CapTop",
                    localPos = new Vector3(0F, 0.74455F, -0.09024F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(2.684F, 2.684F, 2.684F)
                },
            });
            rules.Add("mdlVermin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, -0.20875F, -0.47719F),
                    localAngles = new Vector3(308.5193F, 180F, 180F),
                    localScale = new Vector3(1.46343F, 1.46343F, 1.46343F)
                },
            });
            rules.Add("mdlVoidBarnacle", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00524F, 0F, -0.66232F),
                    localAngles = new Vector3(0F, 248.4958F, 90F),
                    localScale = new Vector3(2.37544F, 2.37544F, 2.37544F)
                },
            });
            rules.Add("mdlVoidJailer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.78154F, 0F, 0.40017F),
                    localAngles = new Vector3(0F, 8.95706F, 90F),
                    localScale = new Vector3(1.81469F, 1.81469F, 1.81469F)
                },
            });
            rules.Add("mdlVoidMegaCrab", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "BodyBase",
                    localPos = new Vector3(0F, 8.1396F, 2.18922F),
                    localAngles = new Vector3(14.97951F, 0F, 0F),
                    localScale = new Vector3(16.37576F, 16.37576F, 16.37576F)
                },
            });
            rules.Add("AcidLarva", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "BodyBase",
                    localPos = new Vector3(0F, 5.38907F, -1.65954F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(12.05802F, 12.05802F, 12.05802F)
                },
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.EquipmentSlot.Update += FilterOutHatWearers;
            On.RoR2.CharacterBody.OnEquipmentGained += GiveFaustController;
            On.RoR2.CharacterBody.OnEquipmentLost += RemoveFaustController;
        }

        private void GiveFaustController(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if(equipmentDef == EquipmentDef && self)
            {
                var faustController = self.gameObject.AddComponent<FaustControllerComponent>();
            }
            orig(self, equipmentDef);
        }

        private void RemoveFaustController(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EquipmentDef && self)
            {
                var faustController = self.gameObject.GetComponent<FaustControllerComponent>();
                if (faustController)
                {
                    UnityEngine.Object.Destroy(faustController);
                }
            }
            orig(self, equipmentDef);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.inputBank) { return false; }

            var targetComponent = slot.GetComponent<TargetingControllerComponent>();
            var displayTransform = slot.FindActiveEquipmentDisplay();


            if (targetComponent && targetComponent.TargetObject)
            {
                var chosenHurtbox = targetComponent.TargetFinder.GetResults().First();

                if (chosenHurtbox)
                {
                    var faustOrb = new FaustOrb()
                    {
                        attacker = slot.characterBody.gameObject,
                        origin = displayTransform ? displayTransform.position : slot.characterBody.corePosition,
                        target = chosenHurtbox,
                    };

                    OrbManager.instance.AddOrb(faustOrb);

                    return true;
                }
            }
            return false;
        }

        private void FilterOutHatWearers(On.RoR2.EquipmentSlot.orig_Update orig, EquipmentSlot self)
        {
            orig(self);
            if (self.equipmentIndex == EquipmentDef.equipmentIndex)
            {
                var targetingComponent = self.GetComponent<TargetingControllerComponent>();
                if (targetingComponent)
                {
                    targetingComponent.AdditionalBullseyeFunctionality = (bullseyeSearch) => bullseyeSearch.FilterOutItemWielders(DeactivatedFaustItem);
                }
            }
        }

        public class FaustControllerComponent : MonoBehaviour
        {
            public List<FaustComponent> activeBargains = new List<FaustComponent>();
            public int maxBargains = 4;

            public void AddBargain(FaustComponent faust)
            {
                if (activeBargains.Contains(faust) || activeBargains.Any(x => x.gameObject == faust.gameObject))
                {
                    return;
                }

                activeBargains.Add(faust);
                if (activeBargains.Count > maxBargains)
                {
                    var bargain = activeBargains[0];
                    bargain.BeginDestruction = true;
                    activeBargains.RemoveAt(0);
                }
                activeBargains.RemoveAll(x => !x.enabled);
            }

            internal void RemoveBargain(FaustComponent faustComponent)
            {
                activeBargains.Remove(faustComponent);
            }
        }

        public class FaustComponent : MonoBehaviour
        {
            public GameObject attacker;

            public CharacterBody characterBody;
            public FaustControllerComponent faustController;
            public SkillLocator skillLocator;
            public Inventory inventory;

            public float stopwatch;
            public float CheckDuration = 3;

            public int skillSealSeed;

            public bool BeginDestruction;

            public void OnEnable()
            {
                characterBody = gameObject.GetComponent<CharacterBody>();
                skillLocator = gameObject.GetComponent<SkillLocator>();
                if (characterBody)
                    inventory = characterBody.inventory;

                SecretKingDialog();

                skillSealSeed = UnityEngine.Random.Range(0, 10000);

                if (this.skillLocator)
                {
                    var skills = new List<GenericSkill>() { this.skillLocator.primary, this.skillLocator.secondary, this.skillLocator.special, this.skillLocator.utility };
                    skills.RemoveAll(x => !x);
                    if (skills.Count > 0)
                    {
                        var skill = skills[skillSealSeed % skills.Count];
                        skill.SetSkillOverride(this, BrokenSkill, GenericSkill.SkillOverridePriority.Replacement);
                    }
                }

                GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;

                if (inventory)
                {
                    inventory.GiveItem(Faust.instance.FaustItem);
                }
            }

            private void SecretKingDialog()
            {
                if (gameObject.name.ToLowerInvariant().StartsWith("brother"))
                {
                    var speechDriver = GameObject.FindObjectOfType<BrotherSpeechDriver>();

                    if (speechDriver)
                    {
                        bool isHurt = speechDriver.gameObject.name.ToLowerInvariant().Contains("hurt");
                        speechDriver.characterSpeechController.EnqueueSpeech(new CharacterSpeechController.SpeechInfo()
                        {
                            token = isHurt ? "BROTHERHURT_AUTIMECIA_FAUST" : "BROTHER_AUTIMECIA_FAUST",
                            duration = 2,
                            maxWait = 0.5f,
                            priority = 10000,
                            mustPlay = true,
                        });
                    }
                }
            }

            public void Start()
            {
                faustController = attacker.GetComponent<FaustControllerComponent>();

                if (faustController)
                {
                    faustController.AddBargain(this);
                }
            }

            private void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
            {
                if (damageReport.victim.gameObject == gameObject && attacker && damageReport.attackerBody && damageReport.attacker == attacker)
                {
                    DropExtraGold(damageReport.attackerBody);
                }
            }

            public void DropExtraGold(CharacterBody attackerBody)
            {
                var corePosition = characterBody.corePosition;
                var rewards = gameObject.GetComponent<DeathRewards>();

                if (rewards)
                {
                    var goldRewarded = (uint)Math.Max(1, rewards.goldReward * 1);
                    var expRewarded = (uint)Math.Max(0, rewards.expReward * 1);

                    ExperienceManager.instance.AwardExperience(corePosition, attackerBody, expRewarded);

                    if (attackerBody.master)
                    {
                        attackerBody.master.GiveMoney(goldRewarded);
                        EffectManager.SpawnEffect(DeathRewards.coinEffectPrefab, new EffectData
                        {
                            origin = corePosition,
                            genericFloat = goldRewarded,
                            scale = characterBody.radius
                        }, true);
                    }
                }
            }

            public void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;
                if(stopwatch >= CheckDuration && !BeginDestruction)
                {
                    if(faustController && !faustController.activeBargains.Contains(this))
                    {
                        BeginDestruction = true;
                    }

                    stopwatch = 0;
                }

                if(BeginDestruction)
                {
                    if (this.skillLocator)
                    {
                        var skills = new List<GenericSkill>() { this.skillLocator.primary, this.skillLocator.secondary, this.skillLocator.special, this.skillLocator.utility };
                        skills.RemoveAll(x => !x);
                        if (skills.Count > 0)
                        {
                            var skill = skills[skillSealSeed % skills.Count];
                            skill.UnsetSkillOverride(this, BrokenSkill, GenericSkill.SkillOverridePriority.Replacement);
                        }
                    }

                    GlobalEventManager.onServerDamageDealt -= GlobalEventManager_onServerDamageDealt;

                    if (inventory)
                    {
                        var inventoryCount = inventory.GetItemCount(Faust.instance.FaustItem);
                        if(inventoryCount > 0)
                        {
                            inventory.RemoveItem(Faust.instance.FaustItem, inventoryCount);
                        }
                    }

                    Destroy(this);
                }
            }

            public void OnDisable()
            {
                if (faustController)
                {
                    faustController.RemoveBargain(this);
                }

                if (this.skillLocator)
                {
                    var skills = new List<GenericSkill>() { this.skillLocator.primary, this.skillLocator.secondary, this.skillLocator.special, this.skillLocator.utility };
                    skills.RemoveAll(x => !x);
                    if (skills.Count > 0)
                    {
                        var skill = skills[skillSealSeed % skills.Count];
                        skill.UnsetSkillOverride(this, BrokenSkill, GenericSkill.SkillOverridePriority.Replacement);
                    }
                }

                GlobalEventManager.onServerDamageDealt -= GlobalEventManager_onServerDamageDealt;

                if (inventory)
                    inventory.RemoveItem(Faust.instance.FaustItem);
            }
        }
    }
}
