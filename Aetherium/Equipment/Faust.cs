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
                icon = MainAssets.LoadAsset<Sprite>("FeatheredPlumeIcon.png"),
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
                    localPos = new Vector3(0F, 0.31581F, -0.00002F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(2F, 2F, 2F)
                },
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.268F, -0.00002F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(2F, 2F, 2F)
                },
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.18332F, 2.82469F, 1.46699F),
                    localAngles = new Vector3(300F, 180F, 10F),
                    localScale = new Vector3(10F, 10F, 10F)
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
                    localScale = new Vector3(1F, 1F, 1F)
                },
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.02416F, 0.14425F, 0.00324F),
                    localAngles = new Vector3(5F, 0F, 10F),
                    localScale = new Vector3(3F, 3F, 3F)
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
                    localScale = new Vector3(1.5F, 1.5F, 1.5F)
                },
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0.15719F, 1.54412F, 0.16778F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(14F, 25F, 10F)
                },
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00232F, 0.20873F, 0.03012F),
                    localAngles = new Vector3(10F, 0F, 350F),
                    localScale = new Vector3(1.5F, 2F, 1.5F)
                },
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 3.91489F, 1.65952F),
                    localAngles = new Vector3(278.3512F, 0F, 180F),
                    localScale = new Vector3(10F, 10F, 10F)
                },
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.01024F, 0.26534F, -0.02854F),
                    localAngles = new Vector3(326.567F, 0F, 0F),
                    localScale = new Vector3(2F, 3F, 2F)
                },
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.03061F, 0.2088F, 0.01693F),
                    localAngles = new Vector3(5F, 0F, 340F),
                    localScale = new Vector3(1F, 1F, 1F)
                },
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.18217F, -0.00261F),
                    localAngles = new Vector3(10.7447F, 0F, 0F),
                    localScale = new Vector3(1.46132F, 1.46132F, 1.46132F)
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
                    localScale = new Vector3(2.18048F, 2.18048F, 2.18048F)
                },
            });
            rules.Add("mdlBeetle", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.50721F, 0.43642F),
                    localAngles = new Vector3(334.1324F, 180F, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
            });
            rules.Add("mdlBeetleGuard", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00001F, -0.43915F, 2.30031F),
                    localAngles = new Vector3(300F, 0.00001F, 180F),
                    localScale = new Vector3(8F, 40F, 8F)
                },
            });
            rules.Add("mdlBeetleQueen", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 3.84075F, 0.71141F),
                    localAngles = new Vector3(10F, 180F, 0F),
                    localScale = new Vector3(20F, 20F, 20F)
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
                    localScale = new Vector3(20F, 20F, 20F)
                },
            });
            rules.Add("mdlBison", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.14374F, 0.63656F),
                    localAngles = new Vector3(277.6974F, 180F, 0F),
                    localScale = new Vector3(3F, 3F, 3F)
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
                    localAngles = new Vector3(76.38377F, 0F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                },
            });
            rules.Add("mdlClayBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "PotLidTop",
                    localPos = new Vector3(0F, 0.65998F, 1.30412F),
                    localAngles = new Vector3(5F, 0F, 0F),
                    localScale = new Vector3(24F, 24F, 24F)
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
                    localScale = new Vector3(4F, 4F, 4F)
                },
            });
            rules.Add("mdlMagmaWorm", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.48357F, 0.52811F),
                    localAngles = new Vector3(84.25481F, 180F, 180F),
                    localScale = new Vector3(12F, 12F, 12F)
                },
            });
            rules.Add("mdlGolem", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.84154F, -0.34857F),
                    localAngles = new Vector3(337.5F, 0F, 0F),
                    localScale = new Vector3(15F, 15F, 15F)
                },
            });
            rules.Add("mdlGrandparent", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00001F, 0.19052F, -1.51033F),
                    localAngles = new Vector3(14.06001F, 0F, 0F),
                    localScale = new Vector3(40F, 50F, 40F)
                },
            });
            rules.Add("mdlGravekeeper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00001F, 2.56513F, -0.72038F),
                    localAngles = new Vector3(347.964F, 0F, 0F),
                    localScale = new Vector3(15F, 16F, 15F)
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
                    localScale = new Vector3(12F, 12F, 12F)
                },
            });
            rules.Add("mdlHermitCrab", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.01153F, 1.97345F, -0.03873F),
                    localAngles = new Vector3(10F, 315.7086F, 0F),
                    localScale = new Vector3(4F, 4F, 4F)
                },
            });
            rules.Add("mdlImp", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Neck",
                    localPos = new Vector3(0.07122F, 0.07336F, -0.02511F),
                    localAngles = new Vector3(10F, 180F, 10F),
                    localScale = new Vector3(3F, 3F, 3F)
                },
            });
            rules.Add("mdlImpBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Neck",
                    localPos = new Vector3(0F, -0.4062F, 0.09444F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(20F, 20F, 20F)
                },
            });
            rules.Add("mdlJellyfish", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hull2",
                    localPos = new Vector3(0.30717F, 0.74014F, 0.16873F),
                    localAngles = new Vector3(10F, 0F, 340F),
                    localScale = new Vector3(12F, 12F, 12F)
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
                    localPos = new Vector3(0F, 1.628F, 0.92033F),
                    localAngles = new Vector3(270F, 180F, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
            });
            rules.Add("mdlLunarExploder", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MuzzleCore",
                    localPos = new Vector3(0.00701F, 0.79941F, 0.00746F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(8F, 8F, 8F)
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
                    localScale = new Vector3(6F, 6F, 6F)
                },
            });
            rules.Add("mdlLunarWisp", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Mask",
                    localPos = new Vector3(0F, 0F, 2.55695F),
                    localAngles = new Vector3(80.00003F, 0F, 0F),
                    localScale = new Vector3(20F, 20F, 20F)
                },
            });
            rules.Add("mdlMiniMushroom", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.24393F, 0F, 0F),
                    localAngles = new Vector3(90F, 270F, 0F),
                    localScale = new Vector3(12F, 12F, 12F)
                },
            });
            rules.Add("mdlNullifier", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Muzzle",
                    localPos = new Vector3(0F, 1.29621F, 0.23904F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(20F, 20F, 20F)
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
                    localScale = new Vector3(750F, 750F, 750F)
                },
            });
            rules.Add("mdlRoboBallBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Center",
                    localPos = new Vector3(0F, 0.79644F, 0F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(12F, 12F, 12F)
                },
            });
            rules.Add("mdlRoboBallMini", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Muzzle",
                    localPos = new Vector3(0F, 0.70609F, -1.13701F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(12F, 12F, 12F)
                },
            });
            rules.Add("mdlScav", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 6.14461F, 0.40801F),
                    localAngles = new Vector3(340F, 180F, 0F),
                    localScale = new Vector3(65F, 65F, 65F)
                },
            });
            rules.Add("mdlTitan", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 6.16367F, 0.86146F),
                    localAngles = new Vector3(20F, 0F, 0F),
                    localScale = new Vector3(25F, 30F, 25F)
                },
            });
            rules.Add("mdlVagrant", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hull",
                    localPos = new Vector3(0F, 1.28064F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(20F, 20F, 20F)
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
                    localScale = new Vector3(20F, 20F, 20F)
                },
            });
            rules.Add("mdlWisp1Mouth", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0F, 0.56107F),
                    localAngles = new Vector3(290F, 180F, 0.00001F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
            });
            rules.Add("mdlVoidRaidCrab", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00002F, 22.41177F, 0.00002F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(196.3254F, 196.3254F, 196.3254F)
                },
            });
            rules.Add("mdlFlyingVermin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Body",
                    localPos = new Vector3(0F, 1.31394F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(8.0078F, 8.0078F, 8.0078F)
                },
            });
            rules.Add("mdlClayGrenadier", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Torso",
                    localPos = new Vector3(0F, 0.40911F, 0.00001F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(2.70638F, 2.70638F, 2.70638F)
                },
            });
            rules.Add("mdlGup", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MainBody2",
                    localPos = new Vector3(0F, 0.97533F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(6.24664F, 6.24664F, 6.24664F)
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
                    localPos = new Vector3(0F, 0.74455F, 0F),
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
                    localScale = new Vector3(3.74382F, 3.74382F, 3.74382F)
                },
            });
            rules.Add("mdlVoidBarnacle", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.35798F, 0F, -0.66401F),
                    localAngles = new Vector3(0F, 283.0353F, 90F),
                    localScale = new Vector3(3.71252F, 3.71252F, 3.71252F)
                },
            });
            rules.Add("mdlVoidJailer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.71118F, 0F, 0.38908F),
                    localAngles = new Vector3(0F, 8.95706F, 90F),
                    localScale = new Vector3(4.2451F, 4.2451F, 4.2451F)
                },
            });
            rules.Add("mdlVoidMegaCrab", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "BodyBase",
                    localPos = new Vector3(0F, 7.40641F, 1.99304F),
                    localAngles = new Vector3(14.97951F, 0F, 0F),
                    localScale = new Vector3(35.96554F, 35.96554F, 35.96554F)
                },
            });
            rules.Add("AcidLarva", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "BodyBase",
                    localPos = new Vector3(0F, 4.91773F, -1.65954F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(29.75583F, 29.75583F, 29.75583F)
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
            FaustItem.tier = ItemTier.NoTier;

            ItemAPI.Add(new CustomItem(FaustItem, CreateFaustDisplayRules()));
        }

        private ItemDisplayRuleDict CreateFaustDisplayRules()
        {
            var victimBodyModelPrefab = MainAssets.LoadAsset<GameObject>("DisplayFaustVictim.prefab");
            var fedoraRainbow = victimBodyModelPrefab.transform.Find("EmptyTransformer/ScaleTransform/Hat").gameObject.AddComponent<RainbowComponent>();
            fedoraRainbow.changeTexture = true;
            fedoraRainbow.hueRate = 1f;
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
                    localPos = new Vector3(0F, 0.31581F, -0.00002F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(2F, 2F, 2F)
                },
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.268F, -0.00002F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(2F, 2F, 2F)
                },
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.18332F, 2.82469F, 1.46699F),
                    localAngles = new Vector3(300F, 180F, 10F),
                    localScale = new Vector3(10F, 10F, 10F)
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
                    localScale = new Vector3(1F, 1F, 1F)
                },
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.02416F, 0.14425F, 0.00324F),
                    localAngles = new Vector3(5F, 0F, 10F),
                    localScale = new Vector3(3F, 3F, 3F)
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
                    localScale = new Vector3(1.5F, 1.5F, 1.5F)
                },
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0.15719F, 1.54412F, 0.16778F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(14F, 25F, 10F)
                },
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00232F, 0.20873F, 0.03012F),
                    localAngles = new Vector3(10F, 0F, 350F),
                    localScale = new Vector3(1.5F, 2F, 1.5F)
                },
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 3.91489F, 1.65952F),
                    localAngles = new Vector3(278.3512F, 0F, 180F),
                    localScale = new Vector3(10F, 10F, 10F)
                },
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.01024F, 0.26534F, -0.02854F),
                    localAngles = new Vector3(326.567F, 0F, 0F),
                    localScale = new Vector3(2F, 3F, 2F)
                },
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.03061F, 0.2088F, 0.01693F),
                    localAngles = new Vector3(5F, 0F, 340F),
                    localScale = new Vector3(1F, 1F, 1F)
                },
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.18217F, -0.00261F),
                    localAngles = new Vector3(10.7447F, 0F, 0F),
                    localScale = new Vector3(1.46132F, 1.46132F, 1.46132F)
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
                    localScale = new Vector3(2.18048F, 2.18048F, 2.18048F)
                },
            });
            rules.Add("mdlBeetle", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.50721F, 0.43642F),
                    localAngles = new Vector3(334.1324F, 180F, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
            });
            rules.Add("mdlBeetleGuard", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00001F, -0.43915F, 2.30031F),
                    localAngles = new Vector3(300F, 0.00001F, 180F),
                    localScale = new Vector3(8F, 40F, 8F)
                },
            });
            rules.Add("mdlBeetleQueen", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 3.84075F, 0.71141F),
                    localAngles = new Vector3(10F, 180F, 0F),
                    localScale = new Vector3(20F, 20F, 20F)
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
                    localScale = new Vector3(20F, 20F, 20F)
                },
            });
            rules.Add("mdlBison", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.14374F, 0.63656F),
                    localAngles = new Vector3(277.6974F, 180F, 0F),
                    localScale = new Vector3(3F, 3F, 3F)
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
                    localAngles = new Vector3(76.38377F, 0F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                },
            });
            rules.Add("mdlClayBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "PotLidTop",
                    localPos = new Vector3(0F, 0.65998F, 1.30412F),
                    localAngles = new Vector3(5F, 0F, 0F),
                    localScale = new Vector3(24F, 24F, 24F)
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
                    localScale = new Vector3(4F, 4F, 4F)
                },
            });
            rules.Add("mdlMagmaWorm", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.48357F, 0.52811F),
                    localAngles = new Vector3(84.25481F, 180F, 180F),
                    localScale = new Vector3(12F, 12F, 12F)
                },
            });
            rules.Add("mdlGolem", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.84154F, -0.34857F),
                    localAngles = new Vector3(337.5F, 0F, 0F),
                    localScale = new Vector3(15F, 15F, 15F)
                },
            });
            rules.Add("mdlGrandparent", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00001F, 0.19052F, -1.51033F),
                    localAngles = new Vector3(14.06001F, 0F, 0F),
                    localScale = new Vector3(40F, 50F, 40F)
                },
            });
            rules.Add("mdlGravekeeper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00001F, 2.56513F, -0.72038F),
                    localAngles = new Vector3(347.964F, 0F, 0F),
                    localScale = new Vector3(15F, 16F, 15F)
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
                    localScale = new Vector3(12F, 12F, 12F)
                },
            });
            rules.Add("mdlHermitCrab", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.01153F, 1.97345F, -0.03873F),
                    localAngles = new Vector3(10F, 315.7086F, 0F),
                    localScale = new Vector3(4F, 4F, 4F)
                },
            });
            rules.Add("mdlImp", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Neck",
                    localPos = new Vector3(0.07122F, 0.07336F, -0.02511F),
                    localAngles = new Vector3(10F, 180F, 10F),
                    localScale = new Vector3(3F, 3F, 3F)
                },
            });
            rules.Add("mdlImpBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Neck",
                    localPos = new Vector3(0F, -0.4062F, 0.09444F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(20F, 20F, 20F)
                },
            });
            rules.Add("mdlJellyfish", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Hull2",
                    localPos = new Vector3(0.30717F, 0.74014F, 0.16873F),
                    localAngles = new Vector3(10F, 0F, 340F),
                    localScale = new Vector3(12F, 12F, 12F)
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
                    localPos = new Vector3(0F, 1.628F, 0.92033F),
                    localAngles = new Vector3(270F, 180F, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
            });
            rules.Add("mdlLunarExploder", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "MuzzleCore",
                    localPos = new Vector3(0.00701F, 0.79941F, 0.00746F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(8F, 8F, 8F)
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
                    localScale = new Vector3(6F, 6F, 6F)
                },
            });
            rules.Add("mdlLunarWisp", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Mask",
                    localPos = new Vector3(0F, 0F, 2.55695F),
                    localAngles = new Vector3(80.00003F, 0F, 0F),
                    localScale = new Vector3(20F, 20F, 20F)
                },
            });
            rules.Add("mdlMiniMushroom", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.24393F, 0F, 0F),
                    localAngles = new Vector3(90F, 270F, 0F),
                    localScale = new Vector3(12F, 12F, 12F)
                },
            });
            rules.Add("mdlNullifier", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Muzzle",
                    localPos = new Vector3(0F, 1.29621F, 0.23904F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(20F, 20F, 20F)
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
                    localScale = new Vector3(750F, 750F, 750F)
                },
            });
            rules.Add("mdlRoboBallBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Center",
                    localPos = new Vector3(0F, 0.79644F, 0F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(12F, 12F, 12F)
                },
            });
            rules.Add("mdlRoboBallMini", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Muzzle",
                    localPos = new Vector3(0F, 0.70609F, -1.13701F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(12F, 12F, 12F)
                },
            });
            rules.Add("mdlScav", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 6.14461F, 0.40801F),
                    localAngles = new Vector3(340F, 180F, 0F),
                    localScale = new Vector3(65F, 65F, 65F)
                },
            });
            rules.Add("mdlTitan", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 6.16367F, 0.86146F),
                    localAngles = new Vector3(20F, 0F, 0F),
                    localScale = new Vector3(25F, 30F, 25F)
                },
            });
            rules.Add("mdlVagrant", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Hull",
                    localPos = new Vector3(0F, 1.28064F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(20F, 20F, 20F)
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
                    localScale = new Vector3(20F, 20F, 20F)
                },
            });
            rules.Add("mdlWisp1Mouth", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0F, 0.56107F),
                    localAngles = new Vector3(290F, 180F, 0.00001F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
            });
            rules.Add("mdlVoidRaidCrab", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00002F, 22.41177F, 0.00002F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(196.3254F, 196.3254F, 196.3254F)
                },
            });
            rules.Add("mdlFlyingVermin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Body",
                    localPos = new Vector3(0F, 1.31394F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(8.0078F, 8.0078F, 8.0078F)
                },
            });
            rules.Add("mdlClayGrenadier", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Torso",
                    localPos = new Vector3(0F, 0.40911F, 0.00001F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(2.70638F, 2.70638F, 2.70638F)
                },
            });
            rules.Add("mdlGup", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "MainBody2",
                    localPos = new Vector3(0F, 0.97533F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(6.24664F, 6.24664F, 6.24664F)
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
                    localPos = new Vector3(0F, 0.74455F, 0F),
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
                    localScale = new Vector3(3.74382F, 3.74382F, 3.74382F)
                },
            });
            rules.Add("mdlVoidBarnacle", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.35798F, 0F, -0.66401F),
                    localAngles = new Vector3(0F, 283.0353F, 90F),
                    localScale = new Vector3(3.71252F, 3.71252F, 3.71252F)
                },
            });
            rules.Add("mdlVoidJailer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.71118F, 0F, 0.38908F),
                    localAngles = new Vector3(0F, 8.95706F, 90F),
                    localScale = new Vector3(4.2451F, 4.2451F, 4.2451F)
                },
            });
            rules.Add("mdlVoidMegaCrab", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "BodyBase",
                    localPos = new Vector3(0F, 7.40641F, 1.99304F),
                    localAngles = new Vector3(14.97951F, 0F, 0F),
                    localScale = new Vector3(35.96554F, 35.96554F, 35.96554F)
                },
            });
            rules.Add("AcidLarva", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = victimBodyModelPrefab,
                    childName = "BodyBase",
                    localPos = new Vector3(0F, 4.91773F, -1.65954F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(29.75583F, 29.75583F, 29.75583F)
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
                    targetingComponent.AdditionalBullseyeFunctionality = (bullseyeSearch) => bullseyeSearch.FilterOutItemWielders(FaustItem);
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
                    inventory.GiveItem(Faust.instance.FaustItem);
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
