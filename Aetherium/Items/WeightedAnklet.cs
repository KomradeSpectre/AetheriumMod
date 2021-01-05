using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static Aetherium.CoreModules.StatHooks;
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Utils.ItemHelpers;
using System;

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

        public override ItemTier Tier => ItemTier.Lunar;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Cleansable };

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

        private void CreateBuff()
        {
            var limiterReleaseBuffDef = new RoR2.BuffDef()
            {
                buffColor = new Color(255, 255, 255),
                canStack = true,
                isDebuff = false,
                name = "Aetherium: Limiter Release",
                iconPath = ""
            };
            LimiterReleaseBuffIndex = BuffAPI.Add(new CustomBuff(limiterReleaseBuffDef));

            var limiterReleaseDodgeBuffDef = new RoR2.BuffDef()
            {
                buffColor = new Color(255, 255, 255),
                canStack = true,
                isDebuff = false,
                name = "Aetherium: Limiter Release Dodges",
                iconPath = ""
            };
            LimiterReleaseDodgeBuffIndex = BuffAPI.Add(new CustomBuff(limiterReleaseDodgeBuffDef));

            var limiterReleaseDodgeCooldownDebuffDef = new RoR2.BuffDef()
            {
                buffColor = new Color(255, 255, 255),
                canStack = false,
                isDebuff = true,
                name = "Aetherium: Limiter Release Dodge Cooldown",
                iconPath = ""
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
            GetStatCoefficients += MoveSpeedReduction;
            On.RoR2.CharacterMaster.OnInventoryChanged += ManageLimiter;
            On.RoR2.CharacterBody.FixedUpdate += ManageLimiterBuff;
            On.RoR2.BlastAttack.PerformDamageServer += ManageBlastDodge;
            //On.RoR2.BlastAttack.HandleHits += ManageBlastDodge;
            On.RoR2.OverlapAttack.PerformDamage += ManageOverlapDodge;
        }

        private void ManageBlastDodge(On.RoR2.BlastAttack.orig_PerformDamageServer orig, ValueType blastAttackDamageInfo)
        {
            Debug.Log(blastAttackDamageInfo.GetType());
            orig(blastAttackDamageInfo);
        }



        /*private void ManageBlastDodge(On.RoR2.BlastAttack.orig_HandleHits orig, RoR2.BlastAttack self, ValueType hitPoints)
        {
            Debug.Log(hitPoints);
            orig(self, hitPoints);
        }*/

        private void MoveSpeedReduction(RoR2.CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            if (InventoryCount > 0)
            {
                args.moveSpeedMultAdd -= Mathf.Min(InventoryCount * BaseMovementSpeedReductionPercentage.Value, MovementSpeedReductionPercentageCap.Value);
            }

        }

        private void ManageLimiter(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, RoR2.CharacterMaster self)
        {
            orig(self);
            var ankletTracker = self.GetComponent<AnkletTracker>();
            if (!ankletTracker) { ankletTracker = self.gameObject.AddComponent<AnkletTracker>();}

            var inventoryCount = GetCount(self);
            if(inventoryCount > ankletTracker.AnkletStacks)
            {
                ankletTracker.AnkletStacks = inventoryCount;
            }
            else if(inventoryCount < ankletTracker.AnkletStacks)
            {
                self.inventory.GiveItem(LimiterReleaseItemIndex, ankletTracker.AnkletStacks - inventoryCount);
                ankletTracker.AnkletStacks = inventoryCount;
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
                    for(int i = 1; i <= iterations; i++)
                    {
                        self.AddBuff(LimiterReleaseBuffIndex);
                        self.AddBuff(LimiterReleaseDodgeBuffIndex);
                    }
                }
            }
        }

        private void ManageOverlapDodge(On.RoR2.OverlapAttack.orig_PerformDamage orig, GameObject attacker, GameObject inflictor, float damage, bool isCrit, RoR2.ProcChainMask procChainMask, float procCoefficient, DamageColorIndex damageColorIndex, DamageType damageType, Vector3 forceVector, float pushAwayForce, object hitList)
        {
            var hitlist = (RoR2.OverlapAttack.OverlapInfo)hitList;
        }

        public class AnkletTracker : MonoBehaviour
        {
            public int AnkletStacks;
        }
    }
}