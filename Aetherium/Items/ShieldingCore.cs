using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using System;
using KomradeSpectre.Aetherium;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Aetherium.Items
{
    public class ShieldingCore : Item<ShieldingCore>
    {
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("If set to true, will use the new icon art drawn by WaltzingPhantom, else it will use the old icon art. Client only.", AutoItemConfigFlags.None)]
        public bool useNewIcons { get; private set; } = true;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("How much armor should the first Shielding Core grant? (Default: 15)", AutoItemConfigFlags.PreventNetMismatch)]
        public float baseShieldingCoreArmorGrant { get; private set; } = 15f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("How much armor should each additional Shielding Core grant? (Default: 10)", AutoItemConfigFlags.PreventNetMismatch)]
        public float additionalShieldingCoreArmorGrant { get; private set; } = 10f;

        public BuffIndex shieldedCoreArmorBuff { get; private set; }
        public override string displayName => "Shielding Core";

        public override ItemTier itemTier => RoR2.ItemTier.Tier2;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "On being hit while shielded, gain a temporary boost in <style=cIsUtility>armor</style>.";

        protected override string NewLangDesc(string langid = null) => $"You gain {baseShieldingCoreArmorGrant} <style=cStack>(+{additionalShieldingCoreArmorGrant} per stack)</style> <style=cIsUtility>armor</style> while <style=cIsUtility>BLUE shields</style> are active.";

        protected override string NewLangLore(string langID = null) => "A salvaged shield amplifier. These were used to harden shields, but were known to cause harmful mutations with prolonged exposure to the crossover field.";

        private static List<CharacterBody> Playername = new List<CharacterBody>();

        public static GameObject ItemBodyModelPrefab;



        public ShieldingCore()
        {
            postConfig += (configFile) =>
            {
                modelPathName = "@Aetherium:Assets/Models/Prefabs/ShieldingCore.prefab";
                iconPathName = useNewIcons ? "@Aetherium:Assets/Textures/Icons/ShieldingCoreIconAlt.png" : "@Aetherium:Assets/Textures/Icons/shieldingCoreIcon.png";
            };
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

            IL.RoR2.CharacterBody.RecalculateStats += GrantBaseShield;
            On.RoR2.CharacterBody.FixedUpdate += ShieldedCoreValidator;
            GetStatCoefficients += ShieldedCoreArmorCalc;
        }

        protected override void UnloadBehavior()
        {
            IL.RoR2.CharacterBody.RecalculateStats -= GrantBaseShield;
            On.RoR2.CharacterBody.FixedUpdate -= ShieldedCoreValidator;
            GetStatCoefficients -= ShieldedCoreArmorCalc;
        }

        private void GrantBaseShield(ILContext il)
        {
            //Provided by Harb from their HarbCrate mod. Thanks Harb!
            ILCursor c = new ILCursor(il);
            int shieldsLoc = 33;
            c.GotoNext(
                MoveType.Before,
                x => x.MatchLdloc(out shieldsLoc),
                x => x.MatchCallvirt<CharacterBody>("set_maxShield")
            );
            c.Emit(OpCodes.Ldloc, shieldsLoc);
            c.EmitDelegate<Func<CharacterBody, float, float>>((self, shields) =>
            {
                var InventoryCount = GetCount(self);
                if (InventoryCount > 0)
                {
                    shields += self.maxHealth * 0.04f;
                }
                return shields;
            });
            c.Emit(OpCodes.Stloc, shieldsLoc);
            c.Emit(OpCodes.Ldarg_0);
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
                args.armorAdd += baseShieldingCoreArmorGrant + (additionalShieldingCoreArmorGrant * (ShieldedCoreComponent.cachedInventoryCount-1));
            }

        }

        public class ShieldedCoreComponent : MonoBehaviour
        {
            public int cachedInventoryCount = 0;
            public bool cachedIsShielded = false;
        }
    }
}
