using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static Aetherium.CoreModules.StatHooks;
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Utils.ItemHelpers;
using System;
using System.Collections.Generic;
using static RoR2.Navigation.MapNodeGroup;

namespace Aetherium.Items
{
    public class WeightedAnklet : ItemBase<WeightedAnklet>
    {
        public static ConfigEntry<float> BaseKnockbackReductionPercentage;
        public static ConfigEntry<float> BaseMovementSpeedReductionPercentage;
        public static ConfigEntry<float> MovementSpeedReductionPercentageCap;

        public override string ItemName => "Weighted Anklet";

        public override string ItemLangTokenName => "WEIGHTED_ANKLET";

        public override string ItemPickupDesc => "A collection of weights slow you down, but finding a way to remove them could greatly benefit you.";

        public override string ItemFullDescription => $"Gain a {FloatToPercentageString(BaseKnockbackReductionPercentage.Value)} reduction to knockback from attacks <style=cStack>(+{FloatToPercentageString(BaseKnockbackReductionPercentage.Value)} per stack (up to 100%) linearly)</style>. Lose {FloatToPercentageString(BaseMovementSpeedReductionPercentage.Value)} move speed <style=cStack>(+{FloatToPercentageString(BaseMovementSpeedReductionPercentage.Value)} per stack (up to {FloatToPercentageString(1 - MovementSpeedReductionPercentageCap.Value)}) linearly)</style>.";

        public override string ItemLore => OrderManifestLoreFormatter(
            ItemName, 

            "7/17/2056",

            "Neptune's Gym and Grill\nEurytrades\nNeptune", 

            "405********", 

            ItemPickupDesc, 

            "Heavy  / Support Equipment Needed / Superdense [Do Not Drop]", 

            "A strange anklet lined with superdense crystals. It's hard to move around in these, but scanners show that the muscle mass of people wearing them increases exponentially.");

        public override ItemTier Tier => ItemTier.Tier1;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Cleansable | ItemTag.AIBlacklist};

        public override string ItemModelPath => "@Aetherium:Assets/Models/Prefabs/Item/WeightedAnklet/WeightedAnklet.prefab";
        public override string ItemIconPath => "@Aetherium:Assets/Textures/Icons/Item/WeightedAnkletIcon.png";

        public static GameObject ItemBodyModelPrefab;

        public static ItemIndex LimiterReleaseItemIndex;

        public static BuffIndex LimiterReleaseBuffIndex;
        public static BuffIndex LimiterReleaseDodgeBuffIndex;
        public static BuffIndex LimiterReleaseDodgeCooldownDebuffIndex;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateMaterials();
            CreateBuff();
            CreateItem();
            CreatePowerupItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            BaseKnockbackReductionPercentage = config.Bind<float>("Item: " + ItemName, "Base Knockback Reduction Percentage", 0.25f, "How much knockback reduction in percentage should be given for each Weighted Anklet?");
            BaseMovementSpeedReductionPercentage = config.Bind<float>("Item: " + ItemName, "Base Movement Speed Reduction Percentage", 0.1f, "How much movement speed in percentage should be reduced per Weighted Anklet?");
            MovementSpeedReductionPercentageCap = config.Bind<float>("Item: " + ItemName, "Absolute Lowest Movement Speed Reduction Percentage", 0.1f, "What should be the lowest percentage of movement speed reduction be?");
        }

        private void CreateMaterials()
        {
            var hopooShader = Resources.Load<Shader>("shaders/deferred/hgstandard");
            var crystalNormal = Resources.Load<Texture2D>("@Aetherium:Assets/Textures/Material Textures/BlasterSwordCoreGlassTexure.png");

            var weightMain = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/Item/WeightedAnklet/WeightedAnkletWeight.mat");
            weightMain.shader = hopooShader;
            weightMain.SetTexture("_NormalTex", crystalNormal);
            weightMain.SetFloat("_NormalStrength", 5);
            weightMain.SetFloat("_RampInfo", 4);
            weightMain.SetFloat("_Smoothness", 0.591f);
            weightMain.SetFloat("_SpecularStrength", 1);
            weightMain.SetFloat("_SpecularExponent", 10);
            weightMain.SetFloat("_ForceSpecOn", 1);

            var weightRing = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/Item/WeightedAnklet/WeightedAnkletSecondary.mat");
            weightRing.shader = hopooShader;
            weightRing.SetTexture("_NormalTex", Resources.Load<Texture2D>("@Aetherium:Assets/Textures/Material Textures/BlasterSwordTexture.png"));
            weightRing.SetFloat("_NormalStrength", 5f);
            weightRing.SetFloat("_Smoothness", 0.5F);
            weightRing.SetFloat("_ForceSpecOn", 1);

        }

        private void CreateBuff()
        {
            var limiterReleaseBuffDef = new RoR2.BuffDef()
            {
                buffColor = new Color(48, 255, 48),
                canStack = true,
                isDebuff = false,
                name = "Aetherium: Limiter Release",
                iconPath = "@Aetherium:Assets/Textures/Icons/Buff/WeightedAnkletLimiterReleaseBuffIcon.png"
            };
            LimiterReleaseBuffIndex = BuffAPI.Add(new CustomBuff(limiterReleaseBuffDef));

            var limiterReleaseDodgeBuffDef = new RoR2.BuffDef()
            {
                buffColor = new Color(48, 255, 48),
                canStack = true,
                isDebuff = false,
                name = "Aetherium: Limiter Release Dodges",
                iconPath = "@Aetherium:Assets/Textures/Icons/Buff/WeightedAnkletLimiterReleaseDodgeBuffIcon.png"
            };
            LimiterReleaseDodgeBuffIndex = BuffAPI.Add(new CustomBuff(limiterReleaseDodgeBuffDef));

            var limiterReleaseDodgeCooldownDebuffDef = new RoR2.BuffDef()
            {
                buffColor = new Color(48, 255, 48),
                canStack = false,
                isDebuff = true,
                name = "Aetherium: Limiter Release Dodge Cooldown",
                iconPath = "@Aetherium:Assets/Textures/Icons/Buff/WeightedAnkletLimiterReleaseDodgeCooldownDebuffIcon.png"
            };
            LimiterReleaseDodgeCooldownDebuffIndex = BuffAPI.Add(new CustomBuff(limiterReleaseDodgeCooldownDebuffDef));


        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = Resources.Load<GameObject>(ItemModelPath);
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.32f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.4f, 0.02f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 3f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(2, 2, 2)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.25f, 0f),
                    localAngles = new Vector3(-19f, 0f, -4f),
                    localScale = new Vector3(0.28f, 0.28f, 0.28f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.41f, 0.02f),
                    localAngles = new Vector3(-5f, 0f, 0f),
                    localScale = new Vector3(0.19f, 0.19f, 0.19f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.32f, 0.025f),
                    localAngles = new Vector3(-10f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FootFrontL",
                    localPos = new Vector3(0f, 1f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.4f, 0.4f, 0.4f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.01f, 0.39f, 0.02f),
                    localAngles = new Vector3(-6f, 0f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 3f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1.5f, 1.5f, 1.5f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.39f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            return rules;
        }

        private void CreatePowerupItem()
        {
            LanguageAPI.Add("HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_NAME", "Weighted Anklet Limiter Release");
            LanguageAPI.Add("HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_PICKUP", "You feel much lighter, and your senses keener.");
            LanguageAPI.Add("HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_DESCRIPTION", "You gain [x] movement speed (+[x] per stack), [x] attack speed (+[x] per stack), and [x] damage bonus (+[x] per stack). Gain the ability to dodge [x] times out of the way of close ranged attacks and behind the attacker before entering a cooldown period.");

            var limiterReleaseItemDef = new RoR2.ItemDef()
            {
                name = "HIDDEN_ITEM_WEIGHTED_ANKLET_LIMITER_RELEASE",
                nameToken = "HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_NAME",
                pickupToken = "HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_PICKUP",
                descriptionToken = "HIDDEN_ITEM_" + ItemLangTokenName + "_LIMITER_RELEASE_DESCRIPTION",
                loreToken = "",
                pickupModelPath = "",
                pickupIconPath = "",
                hidden = true,
                canRemove = false,
                tier = ItemTier.NoTier
            };
            LimiterReleaseItemIndex = ItemAPI.Add(new CustomItem(limiterReleaseItemDef, new RoR2.ItemDisplayRule[] { }));
        }

        public override void Hooks()
        {
            GetStatCoefficients += ManageBonusesAndPenalties;
            On.RoR2.CharacterMaster.OnInventoryChanged += ManageLimiter;
            On.RoR2.CharacterBody.FixedUpdate += ManageLimiterBuff;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += ManageLimiterBuffCooldown;

            var methodBlast = typeof(RoR2.BlastAttack).GetMethod("HandleHits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            new MonoMod.RuntimeDetour.Hook(methodBlast, new Action<Action<RoR2.BlastAttack, RoR2.BlastAttack.HitPoint[]>, RoR2.BlastAttack, RoR2.BlastAttack.HitPoint[]>((orig, self, hitPoints) =>
            {
                List<RoR2.CharacterBody> DodgedBodies = new List<RoR2.CharacterBody>();
                List<RoR2.BlastAttack.HitPoint> HitPointList = new List<RoR2.BlastAttack.HitPoint>();
                foreach(RoR2.BlastAttack.HitPoint hitpoint in hitPoints)
                {
                    var hurtbox = hitpoint.hurtBox;
                    if (hurtbox && hurtbox.healthComponent && hurtbox.healthComponent.body)
                    {
                        var body = hurtbox.healthComponent.body;
                        if (body.HasBuff(LimiterReleaseDodgeBuffIndex))
                        {
                            if (!DodgedBodies.Contains(body)) { DodgedBodies.Add(body); }
                            continue;
                        }

                    }
                    HitPointList.Add(hitpoint);
                }
                if(DodgedBodies.Count > 0)
                {
                    foreach(RoR2.CharacterBody dodgeBody in DodgedBodies)
                    {
                        if (self.attacker) 
                        {
                            var attackerBody = self.attacker.GetComponent<RoR2.CharacterBody>();
                            if (attackerBody)
                            {
                                TeleportBody(dodgeBody, self.attacker.transform.position, dodgeBody.isFlying ? GraphType.Air : GraphType.Ground);

                                var teleportCameraComponent = dodgeBody.GetComponent<LimiterDodgeCameraTrackPostTeleport>();
                                if (!teleportCameraComponent) { teleportCameraComponent = dodgeBody.gameObject.AddComponent<LimiterDodgeCameraTrackPostTeleport>(); }

                                teleportCameraComponent.dodgeBody = dodgeBody;
                                teleportCameraComponent.attackerBody = attackerBody;
                                teleportCameraComponent.Timer = 0.1f;
                            }

                        }

                        dodgeBody.RemoveBuff(LimiterReleaseDodgeBuffIndex);
                        if (dodgeBody.GetBuffCount(LimiterReleaseDodgeBuffIndex) <= 0)
                        {
                            dodgeBody.AddTimedBuff(LimiterReleaseDodgeCooldownDebuffIndex, 10);
                        }

                    }
                }
                orig(self, HitPointList.ToArray());
                
            }));

            var methodOverlap = typeof(RoR2.OverlapAttack).GetMethod("ProcessHits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            new MonoMod.RuntimeDetour.Hook(methodOverlap, new Action<Action<RoR2.OverlapAttack, List<RoR2.OverlapAttack.OverlapInfo>>, RoR2.OverlapAttack, List<RoR2.OverlapAttack.OverlapInfo>>((orig, self, hitList) =>
            {
                List<RoR2.CharacterBody> DodgedBodies = new List<RoR2.CharacterBody>();
                List<RoR2.OverlapAttack.OverlapInfo> HitPointList = new List<RoR2.OverlapAttack.OverlapInfo>();
                foreach (RoR2.OverlapAttack.OverlapInfo hitpoint in hitList)
                {
                    var hurtbox = hitpoint.hurtBox;
                    if (hurtbox && hurtbox.healthComponent && hurtbox.healthComponent.body)
                    {
                        var body = hurtbox.healthComponent.body;
                        if (body.HasBuff(LimiterReleaseDodgeBuffIndex))
                        {
                            if (!DodgedBodies.Contains(body)) { DodgedBodies.Add(body); }
                            continue;
                        }

                    }
                    HitPointList.Add(hitpoint);
                }
                if (DodgedBodies.Count > 0)
                {
                    foreach (RoR2.CharacterBody dodgeBody in DodgedBodies)
                    {
                        if (self.attacker)
                        {
                            var attackerBody = self.attacker.GetComponent<RoR2.CharacterBody>();
                            if (attackerBody)
                            {
                                var teleportBool = TeleportBody(dodgeBody, self.attacker.transform.position, dodgeBody.isFlying ? GraphType.Air : GraphType.Ground);

                                var teleportCameraComponent = dodgeBody.GetComponent<LimiterDodgeCameraTrackPostTeleport>();
                                if (!teleportCameraComponent) { teleportCameraComponent = dodgeBody.gameObject.AddComponent<LimiterDodgeCameraTrackPostTeleport>(); }

                                teleportCameraComponent.dodgeBody = dodgeBody;
                                teleportCameraComponent.attackerBody = attackerBody;
                                teleportCameraComponent.Timer = 0.1f;
                            }
                        }

                        dodgeBody.RemoveBuff(LimiterReleaseDodgeBuffIndex);
                        if (dodgeBody.GetBuffCount(LimiterReleaseDodgeBuffIndex) <= 0)
                        {
                            dodgeBody.AddTimedBuff(LimiterReleaseDodgeCooldownDebuffIndex, 10);
                        }

                    }
                }
                orig(self, HitPointList);

            }));

        }

        private void ManageBonusesAndPenalties(RoR2.CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            if (InventoryCount > 0)
            {
                args.moveSpeedMultAdd -= Mathf.Min(InventoryCount * BaseMovementSpeedReductionPercentage.Value, MovementSpeedReductionPercentageCap.Value);
                args.attackSpeedMultAdd -= Mathf.Min(InventoryCount * 0.1f, MovementSpeedReductionPercentageCap.Value);
            }

            var LimiterReleaseCount = GetCountSpecific(sender, LimiterReleaseItemIndex);
            if (LimiterReleaseCount > 0)
            {
                args.baseAttackSpeedAdd += LimiterReleaseCount * 0.25f;
                args.baseMoveSpeedAdd += LimiterReleaseCount;
                args.damageMultAdd += LimiterReleaseCount * 0.05f;
            }

        }

        private void ManageLimiter(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, RoR2.CharacterMaster self)
        {
            orig(self);
            var ankletTracker = self.GetComponent<AnkletTracker>();
            if (!ankletTracker) { ankletTracker = self.gameObject.AddComponent<AnkletTracker>(); }

            var inventoryCount = GetCount(self);
            if (inventoryCount > ankletTracker.AnkletStacks)
            {
                ankletTracker.AnkletStacks = inventoryCount;
            }
            else if (inventoryCount < ankletTracker.AnkletStacks)
            {
                var calculatedStacks = ankletTracker.AnkletStacks - inventoryCount;
                ankletTracker.AnkletStacks = inventoryCount;
                self.inventory.GiveItem(LimiterReleaseItemIndex, calculatedStacks);
            }
        }

        private void ManageLimiterBuff(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {

            orig(self);
            if (self.inventory)
            {
                var inventoryCount = self.inventory.GetItemCount(LimiterReleaseItemIndex);
                var buffCount = self.GetBuffCount(LimiterReleaseBuffIndex);
                if (buffCount < inventoryCount)
                {
                    var iterations = inventoryCount - buffCount;
                    for (int i = 1; i <= iterations; i++)
                    {
                        self.AddBuff(LimiterReleaseBuffIndex);
                        self.AddBuff(LimiterReleaseDodgeBuffIndex);
                    }
                }
            }
        }

        private void ManageLimiterBuffCooldown(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, RoR2.CharacterBody self, RoR2.BuffDef buffDef)
        {
            if(buffDef == BuffCatalog.GetBuffDef(LimiterReleaseDodgeCooldownDebuffIndex))
            {
                var ankletTracker = self.master.GetComponent<AnkletTracker>();
                if (ankletTracker)
                {
                    for(int i = 1; i <= self.GetBuffCount(LimiterReleaseBuffIndex); i++)
                    {
                        self.AddBuff(LimiterReleaseDodgeBuffIndex);
                    }
                }
            }

            orig(self, buffDef);
        }

        private bool TeleportBody(RoR2.CharacterBody body, Vector3 desiredPosition, GraphType nodeGraphType)
        {
            RoR2.SpawnCard spawnCard = ScriptableObject.CreateInstance<RoR2.SpawnCard>();
            spawnCard.hullSize = body.hullClassification;
            spawnCard.nodeGraphType = nodeGraphType;
            spawnCard.prefab = Resources.Load<GameObject>("SpawnCards/HelperPrefab");
            GameObject gameObject = RoR2.DirectorCore.instance.TrySpawnObject(new RoR2.DirectorSpawnRequest(spawnCard, new RoR2.DirectorPlacementRule
            {
                placementMode = RoR2.DirectorPlacementRule.PlacementMode.Approximate,
                position = desiredPosition,
                minDistance = 10,
                maxDistance = 20
            }, RoR2.RoR2Application.rng));
            if (gameObject)
            {
                RoR2.TeleportHelper.TeleportBody(body, gameObject.transform.position);
                GameObject teleportEffectPrefab = RoR2.Run.instance.GetTeleportEffectPrefab(body.gameObject);
                if (teleportEffectPrefab)
                {
                    RoR2.EffectManager.SimpleEffect(teleportEffectPrefab, gameObject.transform.position, Quaternion.identity, true);
                }
                UnityEngine.Object.Destroy(gameObject);
                UnityEngine.Object.Destroy(spawnCard);
                return true;
            }
            else
            {
                UnityEngine.Object.Destroy(spawnCard);
                return false;
            }
        }

        public class AnkletTracker : MonoBehaviour
        {
            public int AnkletStacks;
        }

        public class LimiterDodgeCameraTrackPostTeleport : MonoBehaviour
        {
            public CharacterBody dodgeBody;
            public CharacterBody attackerBody;
            public float Timer = 1;

            public void FixedUpdate()
            {
                if(!dodgeBody || !attackerBody)
                {
                    UnityEngine.Object.Destroy(this);
                }

                Timer -= Time.fixedDeltaTime;
                if(Timer <= 0)
                {
                    if (dodgeBody.master.playerCharacterMasterController && dodgeBody.master.playerCharacterMasterController.networkUser && dodgeBody.master.playerCharacterMasterController.networkUser.cameraRigController)
                    {
                        var Camera = dodgeBody.master.playerCharacterMasterController.networkUser.cameraRigController;
                        Camera.SetPitchYawFromLookVector(attackerBody.corePosition - dodgeBody.corePosition);
                    }
                    UnityEngine.Object.Destroy(this);
                }
            }
        }
    }
}