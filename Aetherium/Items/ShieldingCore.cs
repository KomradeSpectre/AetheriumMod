using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using System;
using KomradeSpectre.Aetherium;

namespace Aetherium.Items
{
    class ShieldingCore : Item<ShieldingCore>
    {
        public BuffIndex shieldedCoreArmorBuff { get; private set; }
        public override string displayName => "Shielding Core";

        public override ItemTier itemTier => RoR2.ItemTier.Tier2;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "On being hit while shielded, gain a temporary boost in <style=cIsUtility>armor</style>.";

        protected override string NewLangDesc(string langid = null) => "You gain 10 <style=cStack>(+5 per stack)</style> <style=cIsUtility>armor</style> while <style=cIsUtility>BLUE shields</style> are active.";

        protected override string NewLangLore(string langID = null) => "A salvaged shield amplifier. These were used to harden shields, but were known to cause harmful mutations with prolonged exposure to the crossover field.";

        private static List<CharacterBody> Playername = new List<CharacterBody>();

        public static GameObject ItemBodyModelPrefab;



        public ShieldingCore()
        {
            modelPathName = "@Aetherium:Assets/Models/Prefabs/ShieldingCore.prefab";
            iconPathName = "@Aetherium:Assets/Textures/Icons/shieldingCoreIcon.png";
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = AetheriumPlugin.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.2f, 0.2f, 0.2f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.2f, -0.22f),
                    localAngles = new Vector3(0f, -90f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.17f)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.05f, 0.15f, -0.12f),
                    localAngles = new Vector3(0f, -90f, 0f),
                    localScale = new Vector3(0.14f, 0.14f, 0.14f)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(-2f, 6f, 0f),
                    localAngles = new Vector3(45f, -90f, 0f),
                    localScale = generalScale * 10
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.22f, -0.28f),
                    localAngles = new Vector3(0f, -90, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.15f, -0.12f),
                    localAngles = new Vector3(0f, -90f, 0f),
                    localScale = new Vector3(0.14f, 0.14f, 0.14f)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.19f, -0.22f),
                    localAngles = new Vector3(0f, -90f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.17f)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "WeaponPlatform",
                    localPos = new Vector3(0.2f, 0.05f, 0.2f),
                    localAngles = new Vector3(0f, -180f, 0f),
                    localScale = generalScale * 2
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.22f, -0.26f),
                    localAngles = new Vector3(0f, -90, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 4.4f),
                    localAngles = new Vector3(0f, 90f, 0f),
                    localScale = generalScale * 4
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.2f, -0.22f),
                    localAngles = new Vector3(0f, -90f, 0f),
                    localScale = generalScale
                }
            });
            return rules;
        }
        protected override void LoadBehavior()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = regDef.pickupModelPrefab;
                regItem.ItemDisplayRules = GenerateItemDisplayRules();
            }
            On.RoR2.CharacterBody.FixedUpdate += ShieldedCoreValidator;
            GetStatCoefficients += ShieldedCoreArmorCalc;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.CharacterBody.FixedUpdate -= ShieldedCoreValidator;
            GetStatCoefficients -= ShieldedCoreArmorCalc;
        }

        private void ShieldedCoreValidator(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);

            var shieldComponent = self.GetComponent<ShieldedCoreComponent>();
            if (!shieldComponent) {shieldComponent = self.gameObject.AddComponent<ShieldedCoreComponent>();}

            var newInventoryCount = GetCount(self);
            var IsShielded = self.healthComponent.shield > 0 ? true : false;

            bool IsDifferent = false;
            if(shieldComponent.cachedInventoryCount != newInventoryCount)
            {
                IsDifferent = true;
                shieldComponent.cachedInventoryCount = newInventoryCount;
            }
            if(shieldComponent.cachedIsShielded != IsShielded)
            {
                IsDifferent = true;
                shieldComponent.cachedIsShielded = IsShielded;
            }

            if (!IsDifferent) return;

            self.statsDirty = true;
        }

        private void ShieldedCoreArmorCalc(CharacterBody sender, StatHookEventArgs args)
        {
            var ShieldedCoreComponent = sender.GetComponent<ShieldedCoreComponent>();
            if (ShieldedCoreComponent && ShieldedCoreComponent.cachedIsShielded && ShieldedCoreComponent.cachedInventoryCount > 0)
            {
                args.armorAdd += 5 + 5 * ShieldedCoreComponent.cachedInventoryCount;
            }

        }

        public class ShieldedCoreComponent : MonoBehaviour
        {
            public int cachedInventoryCount = 0;
            public bool cachedIsShielded = false;
        }
    }
}
