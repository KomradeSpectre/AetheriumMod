using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Linq;
using System.Collections.Generic;
using static Aetherium.AetheriumPlugin;
using UnityEngine;
using UnityEngine.Networking;
using Aetherium.Utils;

namespace Aetherium.Equipment
{
    public class SoulPin : EquipmentBase<SoulPin>
    {
        public override string EquipmentName => "Soul Pin";
        public override string EquipmentLangTokenName => "SOUL_PIN";
        public override string EquipmentPickupDesc => "On use, fire the pin at a target. If they are elite and you kill them, this equipment transforms into their Aspect.";
        public override string EquipmentFullDescription => "On use, fire a pin that marks an elite enemy for <style=cIsDamage>60 seconds</style>. If the marked elite dies to you, this equipment <style=cIsUtility>transforms into their Aspect</style>.";
        public override string EquipmentLore => $"Found on a scrap of paper in an ornate case along with the device: \"[...] at that point, the binding process will become automatic, " +
            $"and all the user needs to do is sever the specimen's connection to its soul.\"";

        public override GameObject EquipmentModel => MainAssets.LoadAsset<GameObject>("SoulPinMagicCircle.prefab");
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
            CreateProjectile(); // Moved up to ensure it exists before creating equipment
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

            ContentAddition.AddBuffDef(SoulConversionDebuff);
        }

        private void CreateTargetingIndicator()
        {
            TargetingIndicatorPrefabBase = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/WoodSpriteIndicator"), "SoulPinIndicator", false);
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().sprite = MainAssets.LoadAsset<Sprite>("SoulPinReticuleIcon.png");
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.identity;
            TargetingIndicatorPrefabBase.GetComponentInChildren<TMPro.TextMeshPro>().color = new Color(0.423f, 1, 0.749f);
        }

        private void CreateProjectile()
        {
            // Use MageIceboltExpanded as a base for a reliable, straight-flying projectile
            SoulConversionProjectile = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/MageIceboltExpanded"), "SoulConversionProjectile", true);

            // Add Ghost (Visuals)
            var model = MainAssets.LoadAsset<GameObject>("SoulPinProjectile.prefab");
            model.AddComponent<NetworkIdentity>();
            model.AddComponent<ProjectileGhostController>();

            var controller = SoulConversionProjectile.GetComponent<ProjectileController>();
            controller.ghostPrefab = model;

            // Setup Damage (0 damage, purely for debuff application)
            var damage = SoulConversionProjectile.GetComponent<ProjectileDamage>();
            damage.damageType = DamageType.Generic;
            damage.damage = 0f;

            // Setup Buff Application
            var buffApplier = SoulConversionProjectile.AddComponent<ProjectileInflictTimedBuff>();
            buffApplier.buffDef = SoulConversionDebuff;
            buffApplier.duration = 60f;

            // Setup Movement (Fast and Straight)
            var simple = SoulConversionProjectile.GetComponent<ProjectileSimple>();
            simple.enableVelocityOverLifetime = false;
            simple.desiredForwardSpeed = 80f;

            // Setup Homing (Crucial for reliability)
            var steer = SoulConversionProjectile.AddComponent<ProjectileSteerTowardTarget>();
            steer.rotationSpeed = 100f; // High turn rate to ensure it hits

            var targetComponent = SoulConversionProjectile.AddComponent<ProjectileTargetComponent>();

            PrefabAPI.RegisterNetworkPrefab(SoulConversionProjectile);
            ContentAddition.AddProjectile(SoulConversionProjectile);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = EquipmentModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            // OPTIMIZED: Uses the new caching display handler
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
            On.RoR2.EquipmentSlot.Update += RemoveNonElitesFromTargeting;
        }

        private void RemoveNonElitesFromTargeting(On.RoR2.EquipmentSlot.orig_Update orig, EquipmentSlot self)
        {
            orig(self);
            if (self.equipmentIndex == EquipmentDef.equipmentIndex)
            {
                var targetingComponent = self.GetComponent<TargetingControllerComponent>();
                if (targetingComponent)
                {
                    // Filter targeting to only lock onto Elites
                    targetingComponent.AdditionalBullseyeFunctionality = (bullseyeSearch) => bullseyeSearch.FilterElites();
                }
            }
        }

        private void RemoveBuffFromNonElites(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            // Safety check: if a non-elite gets the buff (e.g. via explosion radius), remove it.
            if (buffDef == SoulConversionDebuff && !self.isElite)
            {
                self.RemoveBuff(SoulConversionDebuff);
            }
        }

        private void MorphEquipmentIntoAffix(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport); // Always run original logic first

            if (!NetworkServer.active) return; // Server only logic

            if (damageReport.attackerMaster && damageReport.victimBody)
            {
                // Check if attacker has the Pin active AND victim has the Debuff
                if (damageReport.attackerMaster.inventory.currentEquipmentIndex == EquipmentDef.equipmentIndex && damageReport.victimBody.HasBuff(SoulConversionDebuff))
                {
                    var victimEquipmentIndex = damageReport.victimBody.inventory.GetEquipmentIndex();
                    var victimEquipmentDef = EquipmentCatalog.GetEquipmentDef(victimEquipmentIndex);

                    // Check if the victim's equipment is an Elite Aspect
                    if (victimEquipmentDef && victimEquipmentDef.passiveBuffDef && victimEquipmentDef.passiveBuffDef.isElite)
                    {
                        // Transform the Soul Pin into the Aspect
                        damageReport.attackerMaster.inventory.SetEquipmentIndex(victimEquipmentIndex);

                        // Visual feedback for the transformation
                        EffectManager.SimpleEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/LevelUpEffect"), damageReport.attackerBody.corePosition, Quaternion.identity, true);
                    }
                }
            }
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.inputBank) { return false; }

            var targetComponent = slot.GetComponent<TargetingControllerComponent>();

            // Ensure we have a valid target from the targeting system
            if (targetComponent && targetComponent.TargetObject)
            {
                var targetHurtbox = targetComponent.TargetObject.GetComponent<HurtBox>();
                if (!targetHurtbox) return false;

                // FIX: Fire a homing projectile instead of a "Sky Bullet"
                // This ensures it hits enemies indoors or under cover.
                ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                {
                    projectilePrefab = SoulConversionProjectile,
                    position = slot.characterBody.inputBank.aimOrigin,
                    rotation = Util.QuaternionSafeLookRotation(slot.characterBody.inputBank.aimDirection),
                    owner = slot.characterBody.gameObject,
                    damage = 0f,
                    force = 0f,
                    crit = false,
                    target = targetComponent.TargetObject // Sets the homing target
                });

                return true;
            }
            return false;
        }

        // OPTIMIZED: Caches components to avoid GetComponentInParent every frame
        public class SoulPinDisplayHandler : MonoBehaviour
        {
            public RoR2.ItemDisplay ItemDisplay;
            public RoR2.CharacterBody OwnerBody;
            private CharacterModel.RendererInfo[] rendererInfos;

            public void Start()
            {
                ItemDisplay = GetComponentInParent<RoR2.ItemDisplay>();
                if (ItemDisplay)
                {
                    rendererInfos = ItemDisplay.rendererInfos;
                    var model = ItemDisplay.GetComponentInParent<RoR2.CharacterModel>();
                    if (model && model.body)
                    {
                        OwnerBody = model.body;
                    }
                }
            }

            public void FixedUpdate()
            {
                if (OwnerBody && ItemDisplay && rendererInfos != null && rendererInfos.Length > 0)
                {
                    bool shouldBeVisible = false;

                    // Logic: Visible if we have stock (equipment is ready)
                    if (OwnerBody.equipmentSlot && OwnerBody.equipmentSlot.stock > 0)
                    {
                        shouldBeVisible = true;
                    }

                    // Only toggle if state changes (Optimization)
                    if (rendererInfos[0].renderer.enabled != shouldBeVisible)
                    {
                        for (int i = 0; i < rendererInfos.Length; i++)
                        {
                            rendererInfos[i].renderer.enabled = shouldBeVisible;
                        }
                    }
                }
            }
        }
    }
}