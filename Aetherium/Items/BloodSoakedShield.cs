using Aetherium.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TILER2;
using UnityEngine;
using static TILER2.MiscUtil;
using static TILER2.StatHooks;

namespace Aetherium.Items
{
    public class BloodSoakedShield : Item_V2<BloodSoakedShield>
    {
        [AutoConfig("If set to true, will use the new icon art drawn by WaltzingPhantom, else it will use the old icon art. Client only.", AutoConfigFlags.None)]
        public bool useNewIcons { get; private set; } = true;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("How much shield in percentage should be restored per kill? (Default: 0.1 (10%))", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float shieldPercentageRestoredPerKill { get; private set; } = 0.1f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("How much additional shield per kill should be granted with diminishing returns (hyperbolic scaling) on additional stacks? (Default: 0.1 (10%))", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float additionalShieldPercentageRestoredPerKillDiminishing { get; private set; } = 0.1f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What should our maximum percentage shield restored per kill be? (Default: 0.5 (50%))", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float maximumPercentageShieldRestoredPerKill { get; private set; } = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("How much should the starting shield be upon receiving the item? (Default: 0.08 (8%))", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float baseGrantShieldMultiplier { get; private set; } = 0.08f;

        public override string displayName => "Blood Soaked Shield";

        public override ItemTier itemTier => RoR2.ItemTier.Tier2;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Healing });
        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Killing an enemy <style=cIsHealing>restores</style> a small portion of <style=cIsHealing>shield</style>.";

        protected override string GetDescString(string langid = null) => $"Killing an enemy restores <style=cIsUtility>{Pct(shieldPercentageRestoredPerKill)} max shield</style> " +
            $"<style=cStack>(+{Pct(additionalShieldPercentageRestoredPerKillDiminishing)} per stack hyperbolically.)</style> " +
            $"This item will grant <style=cIsUtility>{Pct(baseGrantShieldMultiplier)}</style> of your max health as shield on pickup once.";

        protected override string GetLoreString(string langID = null) => "An old gladiatorial round shield. The bloody spikes and Greek lettering give you an accurate picture of what it was used to do. Somehow, holding it makes you feel empowered.";

        private static List<RoR2.CharacterBody> Playername = new List<RoR2.CharacterBody>();
        public static GameObject ItemBodyModelPrefab;

        public BloodSoakedShield()
        {
        }

        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(modelResourcePath);
                displayRules = GenerateItemDisplayRules();
            }
            base.SetupAttributes();
        }

        public override void SetupConfig()
        {
            base.SetupConfig();
            modelResourcePath = "@Aetherium:Assets/Models/Prefabs/BloodSoakedShield.prefab";
            iconResourcePath = useNewIcons ? "@Aetherium:Assets/Textures/Icons/BloodSoakedShieldIconAlt.png" : "@Aetherium:Assets/Textures/Icons/BloodSoakedShieldIcon.png";
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.3f, 0.3f, 0.3f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0, 0.23f, -0.05f),
                    localAngles = new Vector3(0, -180, -90),
                    localScale = generalScale
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(0, 0.2f, -0.05f),
                    localAngles = new Vector3(0, 180, -90),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0, 0, 0.65f),
                    localAngles = new Vector3(0, 0, 270),
                    localScale = new Vector3(2, 2, 2)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(-0.014f, 0.127f, -0.08f),
                    localAngles = new Vector3(0, 160, 180),
                    localScale = new Vector3(0.3f, 0.3f, 0.3f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0f, 0.15f, 0.07f),
                    localAngles = new Vector3(0, 0, 180),
                    localScale = new Vector3(0.32f, 0.32f, 0.32f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.036f, 0.21f, -0.041f),
                    localAngles = new Vector3(350, 180, 90),
                    localScale = generalScale
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "WeaponPlatform",
                    localPos = new Vector3(-0.16f, -0.1f, 0.1f),
                    localAngles = new Vector3(0, -90, -90),
                    localScale = new Vector3(0.5f, 0.5f, 0.5f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MechLowerArmL",
                    localPos = new Vector3(0, 0.2f, -0.09f),
                    localAngles = new Vector3(0, 180, 90),
                    localScale = new Vector3(0.32f, 0.32f, 0.32f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.7f, 3, 0.7f),
                    localAngles = new Vector3(0, 45, 270),
                    localScale = new Vector3(2, 2, 2)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.058f, 0.23f, 0),
                    localAngles = new Vector3(10, -90, 90),
                    localScale = new Vector3(0.3f, 0.3f, 0.3f)
                }
            });
            return rules;
        }

        public override void Install()
        {
            base.Install();
            //IL.RoR2.CharacterBody.RecalculateStats += GrantBaseShield;
            GetStatCoefficients += GrantBaseShield;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GrantShieldReward;
        }

        public override void Uninstall()
        {
            base.Uninstall();
            //IL.RoR2.CharacterBody.RecalculateStats -= GrantBaseShield;
            GetStatCoefficients -= GrantBaseShield;
            On.RoR2.GlobalEventManager.OnCharacterDeath -= GrantShieldReward;
        }

        private void GrantBaseShield(CharacterBody sender, StatHookEventArgs args)
        {
            if (GetCount(sender) > 0)
            {
                HealthComponent healthC = sender.GetComponent<HealthComponent>();
                args.baseShieldAdd += healthC.fullHealth * baseGrantShieldMultiplier;
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
        //            shields += self.maxHealth * 0.08f;
        //        }
        //        return shields;
        //    });
        //    c.Emit(OpCodes.Stloc, shieldsLoc);
        //    c.Emit(OpCodes.Ldarg_0);
        //}

        private void GrantShieldReward(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, RoR2.GlobalEventManager self, RoR2.DamageReport damageReport)
        {
            if (damageReport?.attackerBody)
            {
                int inventoryCount = GetCount(damageReport.attackerBody);
                if (inventoryCount > 0)
                {
                    var percentage = shieldPercentageRestoredPerKill + (maximumPercentageShieldRestoredPerKill - maximumPercentageShieldRestoredPerKill / (1 + additionalShieldPercentageRestoredPerKillDiminishing * (inventoryCount - 1)));
                    damageReport.attackerBody.healthComponent.RechargeShield(damageReport.attackerBody.healthComponent.fullShield * percentage);
                }
            }
            orig(self, damageReport);
        }

    }
}
