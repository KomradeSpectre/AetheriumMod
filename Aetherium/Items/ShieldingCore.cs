using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.CoreModules.StatHooks;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;

namespace Aetherium.Items
{
    public class ShieldingCore : ItemBase<ShieldingCore>
    {
        public static ConfigEntry<bool> UseNewIcons;
        public static ConfigEntry<float> BaseShieldingCoreArmorGrant;
        public static ConfigEntry<float> AdditionalShieldingCoreArmorGrant;
        public static ConfigEntry<float> BaseGrantShieldMultiplier;

        public override string ItemName => "Shielding Core";

        public override string ItemLangTokenName => "SHIELDING_CORE";

        public override string ItemPickupDesc => "While shielded, gain a temporary boost in <style=cIsUtility>armor</style>.";

        public override string ItemFullDescription => $"You gain <style=cIsUtility>{BaseShieldingCoreArmorGrant.Value}</style> <style=cStack>(+{AdditionalShieldingCoreArmorGrant.Value} per stack)</style> <style=cIsUtility>armor</style> while <style=cIsUtility>BLUE shields</style> are active." +
            $" The first stack of this item will grant <style=cIsUtility>{FloatToPercentageString(BaseGrantShieldMultiplier.Value)}</style> of your max health as shield on pickup.";

        public override string ItemLore => "A salvaged shield amplifier. These were used to harden shields, but were known to cause harmful mutations with prolonged exposure to the crossover field.";

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

        public override string ItemModelPath => "@Aetherium:Assets/Models/Prefabs/Item/ShieldingCore/ShieldingCore.prefab";
        public override string ItemIconPath => UseNewIcons.Value ? "@Aetherium:Assets/Textures/Icons/Item/ShieldingCoreIconAlt.png" : "@Aetherium:Assets/Textures/Icons/Item/shieldingCoreIcon.png";

        public static GameObject ItemBodyModelPrefab;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            UseNewIcons = config.Bind<bool>(ItemName, "Use Alternative Icon Art?", true, "If set to true, will use the new icon art drawn by WaltzingPhantom, else it will use the old icon art.");
            BaseShieldingCoreArmorGrant = config.Bind<float>(ItemName, "First Shielding Core Bonus to Armor", 15f, "How much armor should the first Shielding Core grant?");
            AdditionalShieldingCoreArmorGrant = config.Bind<float>(ItemName, "Additional Shielding Cores Bonus to Armor", 10f, "How much armor should each additional Shielding Core grant?");
            BaseGrantShieldMultiplier = config.Bind<float>(ItemName, "First Shielding Core Bonus to Max Shield", 0.04f, "How much should the starting shield be upon receiving the item?");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = Resources.Load<GameObject>(ItemModelPath);
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

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

        public override void Hooks()
        {
            GetStatCoefficients += GrantBaseShield;
            On.RoR2.CharacterBody.FixedUpdate += ShieldedCoreValidator;
            GetStatCoefficients += ShieldedCoreArmorCalc;
        }

        private void GrantBaseShield(CharacterBody sender, StatHookEventArgs args)
        {
            if (GetCount(sender) > 0)
            {
                HealthComponent healthC = sender.GetComponent<HealthComponent>();
                args.baseShieldAdd += healthC.fullHealth * BaseGrantShieldMultiplier.Value;
            }
        }

        //private void GrantBaseShield(ILContext il)
        //{
        //    //Provided by Harb from their HarbCrate mod. Thanks Harb!
        //    ILCursor c = new ILCursor(il);
        //    int shieldsLoc = 33;
        //    c.GotoNext(
        //        MoveType.Before,
        //        x => x.MatchLdloc(out shieldsLoc),
        //        x => x.MatchCallvirt<CharacterBody>("set_maxShield")
        //    );
        //    c.Emit(OpCodes.Ldloc, shieldsLoc);
        //    c.EmitDelegate<Func<CharacterBody, float, float>>((self, shields) =>
        //    {
        //        var InventoryCount = GetCount(self);
        //        if (InventoryCount > 0)
        //        {
        //            shields += self.maxHealth * 0.04f;
        //        }
        //        return shields;
        //    });
        //    c.Emit(OpCodes.Stloc, shieldsLoc);
        //    c.Emit(OpCodes.Ldarg_0);
        //}

        private void ShieldedCoreValidator(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);

            var shieldComponent = self.GetComponent<ShieldedCoreComponent>();
            if (!shieldComponent) { shieldComponent = self.gameObject.AddComponent<ShieldedCoreComponent>(); }

            var newInventoryCount = GetCount(self);
            var IsShielded = self.healthComponent.shield > 0 ? true : false;

            bool IsDifferent = false;
            if (shieldComponent.cachedInventoryCount != newInventoryCount)
            {
                IsDifferent = true;
                shieldComponent.cachedInventoryCount = newInventoryCount;
            }
            if (shieldComponent.cachedIsShielded != IsShielded)
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
                args.armorAdd += BaseShieldingCoreArmorGrant.Value + (AdditionalShieldingCoreArmorGrant.Value * (ShieldedCoreComponent.cachedInventoryCount - 1));
            }

        }

        public class ShieldedCoreComponent : MonoBehaviour
        {
            public int cachedInventoryCount = 0;
            public bool cachedIsShielded = false;
        }
    }
}
