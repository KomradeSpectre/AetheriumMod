using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using AK.Wwise;

using static Aetherium.AetheriumPlugin;
using static Aetherium.CoreModules.ItemHelperModule;
using static Aetherium.Compatability.ModCompatability.BetterAPICompat;
using UnityEngine.Networking;
using Aetherium.Utils;

namespace Aetherium.Equipment
{
    public class SoulPin : EquipmentBase<SoulPin>
    {
        public override string EquipmentName => "Soul Pin";

        public override string EquipmentLangTokenName => "SOUL_PIN";

        public override string EquipmentPickupDesc => "On use, fire the pin out towards an enemy. If that enemy is an elite and dies to you, the pin will transform into its aspect.";

        public override string EquipmentFullDescription => "On use, you can mark an elite enemy. If that elite enemy dies to you, this equipment transforms into its Aspect.";

        public override string EquipmentLore =>
            
            $"Found on a scrap of paper in an ornate case along with the device: \"[...] at that point, the binding process will become automatic, " +
            $"and all the user needs to do is sever the specimen's connection to its soul.\"";

        public override GameObject EquipmentModel => MainAssets.LoadAsset<GameObject>("SoulPin.prefab");

        public override Sprite EquipmentIcon => MainAssets.LoadAsset<Sprite>("FeatheredPlumeIcon.png");

        public override bool UseTargeting => true;

        public static GameObject ItemBodyModelPrefab;

        public static GameObject SoulConversionProjectile;

        public static BuffDef SoulConversionDebuff;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateBuff();
            CreateTargetingIndicator();

            CreateProjectile();

            CreateEquipment();
            Hooks();
        }

        private void CreateBuff()
        {
            SoulConversionDebuff = ScriptableObject.CreateInstance<BuffDef>();
            SoulConversionDebuff.name = "Aetherium: Soul Conversion Debuff";
            SoulConversionDebuff.buffColor = Color.white;
            SoulConversionDebuff.canStack = false;
            SoulConversionDebuff.isDebuff = true;
            SoulConversionDebuff.iconSprite = MainAssets.LoadAsset<Sprite>("AccursedPotionSipCooldownDebuffIcon.png");

            BuffAPI.Add(new CustomBuff(SoulConversionDebuff));
        }

        private void CreateTargetingIndicator()
        {
            TargetingIndicatorPrefabBase = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/WoodSpriteIndicator"), "SoulPinIndicator", false);
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().sprite = MainAssets.LoadAsset<Sprite>("SoulPinReticuleIcon.png");
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.identity;
            TargetingIndicatorPrefabBase.GetComponentInChildren<TMPro.TextMeshPro>().color = new Color(0.423f, 1, 0.749f);
        }

        private void CreateProjectile()
        {
            SoulConversionProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/LunarShardProjectile"), "SoulConversionProjectile", true);

            var model = MainAssets.LoadAsset<GameObject>("SoulPinProjectile.prefab");
            model.AddComponent<NetworkIdentity>();
            model.AddComponent<RoR2.Projectile.ProjectileGhostController>();

            var projectileController = SoulConversionProjectile.GetComponent<ProjectileController>();
            projectileController.ghostPrefab = model;

            var projectileDamage = SoulConversionProjectile.GetComponent<ProjectileDamage>();
            projectileDamage.damageType = DamageType.Generic;

            var projectileInflictDebuff = SoulConversionProjectile.AddComponent<ProjectileInflictTimedBuff>();
            projectileInflictDebuff.buffDef = SoulConversionDebuff;
            projectileInflictDebuff.duration = 60;

            var projectileStickOnImpact = SoulConversionProjectile.AddComponent<ProjectileStickOnImpact>();
            projectileStickOnImpact.alignNormals = false;
            projectileStickOnImpact.ignoreCharacters = false;
            projectileStickOnImpact.ignoreWorld = true;
            projectileStickOnImpact.stickSoundString = "Play_treeBot_m1_impact";

            var projectileSteerTowardsTarget = SoulConversionProjectile.GetComponent<ProjectileSteerTowardTarget>();
            projectileSteerTowardsTarget.rotationSpeed = 50;

            UnityEngine.Object.Destroy(SoulConversionProjectile.GetComponent<ProjectileImpactExplosion>());

            PrefabAPI.RegisterNetworkPrefab(SoulConversionProjectile);
            ProjectileAPI.Add(SoulConversionProjectile);
        }

        public void DestroyProjectileParticleSystem()
        {

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = EquipmentModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab, true);
            ItemBodyModelPrefab.AddComponent<SoulPinDisplayHandler>();

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(-90, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(8f, -4, 8f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-2f, 0, -2f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-8f, 0, 8f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnBuffFirstStackGained += RemoveBuffFromNonElites;
            On.RoR2.GlobalEventManager.OnCharacterDeath += MorphEquipmentIntoAffix;
        }

        private void RemoveBuffFromNonElites(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            if (self.HasBuff(SoulConversionDebuff) && !self.isElite)
            {
                self.RemoveBuff(SoulConversionDebuff);
            }
        }

        private void MorphEquipmentIntoAffix(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            if(damageReport.attackerMaster && damageReport.victimBody)
            {
                if(damageReport.attackerMaster.inventory.currentEquipmentIndex == EquipmentDef.equipmentIndex && damageReport.victimBody.HasBuff(SoulConversionDebuff))
                {
                    var victimEquipmentDef = EquipmentCatalog.GetEquipmentDef(damageReport.victimBody.inventory.GetEquipmentIndex());

                    if (victimEquipmentDef && EliteEquipmentDefs.Any(x => x == victimEquipmentDef))
                    {
                        damageReport.attackerMaster.inventory.GiveEquipmentString(victimEquipmentDef.name);
                    }
                }
            }

            orig(self, damageReport);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if(!slot.characterBody || !slot.characterBody.inputBank) { return false; }

            var targetComponent = slot.GetComponent<TargetingControllerComponent>();
            var displayTransform = slot.FindActiveEquipmentDisplay();


            if (targetComponent && targetComponent.TargetObject)
            {
                var aimray = slot.characterBody.inputBank.GetAimRay();

                FireProjectileInfo SoulConversionInfo = new FireProjectileInfo()
                {
                    owner = slot.characterBody.gameObject,
                    position = displayTransform ? displayTransform.position : aimray.origin,
                    rotation = aimray.direction == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(aimray.direction),
                    target = targetComponent.TargetObject,
                    damage = slot.characterBody.damage,
                    projectilePrefab = SoulConversionProjectile,
                    speedOverride = 100,
                    procChainMask = default(ProcChainMask),
                };
                ProjectileManager.instance.FireProjectile(SoulConversionInfo);
                return true;
            }
            return false;
        }

        public class SoulPinDisplayHandler : MonoBehaviour
        {
            public RoR2.ItemDisplay ItemDisplay;
            public RoR2.CharacterMaster OwnerMaster;
            public RoR2.CharacterBody OwnerBody;
            public void FixedUpdate()
            {

                if (!OwnerMaster || !ItemDisplay)
                {
                    ItemDisplay = this.GetComponentInParent<RoR2.ItemDisplay>();
                    if (ItemDisplay)
                    {
                        //Debug.Log("Found ItemDisplay: " + itemDisplay);
                        var characterModel = ItemDisplay.GetComponentInParent<RoR2.CharacterModel>();

                        if (characterModel)
                        {
                            var body = characterModel.body;
                            if (body)
                            {
                                OwnerMaster = body.master;
                            }
                        }
                    }
                }

                if (OwnerMaster && !OwnerBody)
                {
                    var body = OwnerMaster.GetBody();
                    if (body)
                    {
                        OwnerBody = body;
                    }
                    if (!body)
                    {
                        UnityEngine.Object.Destroy(this);
                    }
                }

                if (OwnerBody && ItemDisplay)
                {
                    var slot = OwnerBody.equipmentSlot;
                    if (slot)
                    {
                        if(slot.stock > 0)
                        {
                            foreach(CharacterModel.RendererInfo rendererInfo in ItemDisplay.rendererInfos)
                            {
                                if (!rendererInfo.renderer.enabled && ItemDisplay.visibilityLevel != VisibilityLevel.Invisible)
                                {
                                    rendererInfo.renderer.enabled = true;
                                }
                            }
                        }
                        else
                        {
                            foreach(CharacterModel.RendererInfo rendererInfo in ItemDisplay.rendererInfos)
                            {
                                if (rendererInfo.renderer.enabled)
                                {
                                    rendererInfo.renderer.enabled = false;
                                }
                            }
                        }
                    }
                }

            }
        }
    }
}
