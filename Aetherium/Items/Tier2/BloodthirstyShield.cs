using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using System.Linq;

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static R2API.RecalculateStatsAPI;

namespace Aetherium.Items.Tier2
{
    public class BloodthirstyShield : ItemBase<BloodthirstyShield>
    {
        public static ConfigOption<bool> UseNewIcons;
        public static ConfigOption<float> ShieldPercentageRestoredPerKill;
        public static ConfigOption<float> AdditionalShieldPercentageRestoredPerKillDiminishing;
        public static ConfigOption<float> MaximumPercentageShieldRestoredPerKill;
        public static ConfigOption<float> BaseGrantShieldMultiplier;

        public override string ItemName => "Bloodthirsty Shield";
        public override string ItemLangTokenName => "BLOODTHIRSTY_SHIELD";
        public override string ItemPickupDesc => "Killing an enemy <style=cIsHealing>restores</style> a small portion of <style=cIsHealing>shield</style>.";

        public override string ItemFullDescription => $"Killing an enemy restores <style=cIsUtility>{FloatToPercentageString(ShieldPercentageRestoredPerKill)} max shield</style> " +
            $"<style=cStack>(+{FloatToPercentageString(AdditionalShieldPercentageRestoredPerKillDiminishing)} per stack hyperbolically.)</style> " +
            $"This item will grant <style=cIsUtility>{FloatToPercentageString(BaseGrantShieldMultiplier)}</style> of your max health as shield on pickup once.";

        public override string ItemLore =>

            "Ballad of Letheseus, Hero of the Isles - Act II, Scene III\n\n" +

            "The sound of metal tearing flesh ripples through the air; the soldiers within seemingly unaffected by the macabre orchestra around them. " +
            "All that matters to them is their own survival, regardless of whose blood that survival costs. One after another, the Xiphos meets its mark until there is only one man left.\n\n" +

            "The sound dies down. Letheseus kneels upon the crimson soil, worn out from his deadly dance. His Xiphos shall serve him no longer. " +
            "His Aspis, albeit cracked, shall continue to protect him as long as he is able to pay it a tribute he can only find in the fury of battle.";

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Healing };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("BloodthirstyShield.prefab");
        public override Sprite ItemIcon => UseNewIcons ? MainAssets.LoadAsset<Sprite>("BloodSoakedShieldIconAlt.png") : MainAssets.LoadAsset<Sprite>("BloodthirstyShieldIcon.png");

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
            UseNewIcons = config.ActiveBind<bool>("Item: " + ItemName, "Use Alternative Icon Art?", false, "If set to true, will use the new icon art drawn by WaltzingPhantom, else it will use the old icon art.");
            ShieldPercentageRestoredPerKill = config.ActiveBind<float>("Item: " + ItemName, "Percentage of Shield Restored per Kill", 0.1f, "How much shield in percentage should be restored per kill? 0.1 = 10%");
            AdditionalShieldPercentageRestoredPerKillDiminishing = config.ActiveBind<float>("Item: " + ItemName, "Additional Shield Restoration Percentage per Additional BSS Stack (Diminishing)", 0.1f, "How much additional shield per kill should be granted with diminishing returns (hyperbolic scaling) on additional stacks? 0.1 = 10%");
            MaximumPercentageShieldRestoredPerKill = config.ActiveBind<float>("Item: " + ItemName, "Absolute Maximum Shield Restored per Kill", 0.5f, "What should our maximum percentage shield restored per kill be? 0.5 = 50%");
            BaseGrantShieldMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Shield Granted on First BSS Stack", 0.08f, "How much should the starting shield be upon receiving the item? 0.08 = 8%");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;

            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.3f, 0.3f, 0.3f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
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
                    localPos = new Vector3(-0.01392F, 0.15062F, -0.07997F),
                    localAngles = new Vector3(0.00001F, 160F, 270.2119F),
                    localScale = new Vector3(0.3F, 0.3F, 0.3F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.00001F, 0.20747F, 0.09967F),
                    localAngles = new Vector3(0F, 0F, 263.2974F),
                    localScale = new Vector3(0.15937F, 0.15937F, 0.15937F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.01003F, 0.13759F, -0.07581F),
                    localAngles = new Vector3(357.1992F, 180F, 90.00002F),
                    localScale = new Vector3(0.22186F, 0.22186F, 0.22324F)
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
                    localPos = new Vector3(0F, 0.2F, -0.11967F),
                    localAngles = new Vector3(0F, 180F, 90F),
                    localScale = new Vector3(0.32F, 0.32F, 0.32F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.7F, 3F, 0.7F),
                    localAngles = new Vector3(-0.00001F, 45F, 14.43579F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MuzzleGun",
                    localPos = new Vector3(-0.02823F, 0.06544F, -0.10462F),
                    localAngles = new Vector3(282.3442F, 270F, 0F),
                    localScale = new Vector3(0.19405F, 0.19405F, 0.19405F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.0718F, 0.0361F, -0.0153F),
                    localAngles = new Vector3(28.3891F, 105.4747F, 280.3181F),
                    localScale = new Vector3(0.1533F, 0.1533F, 0.1533F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LeftArm4",
                    localPos = new Vector3(0F, 0.00373F, -0.0032F),
                    localAngles = new Vector3(0F, 180F, 90F),
                    localScale = new Vector3(0.008F, 0.008F, 0.01F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0.05551F, 0.27008F, -0.17225F),
                    localAngles = new Vector3(0F, 172.5358F, 90F),
                    localScale = new Vector3(0.3F, 0.3F, 0.3F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HandR",
                    localPos = new Vector3(0.03136F, 0.0477F, 0.01365F),
                    localAngles = new Vector3(357.0265F, 56.73941F, 0F),
                    localScale = new Vector3(0.03674F, 0.03674F, 0.03674F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.05742F, 0.2146F, 0.00018F),
                    localAngles = new Vector3(359.2144F, 90F, 270F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Shield",
                    localPos = new Vector3(0.36339F, -0.11669F, 0.01F),
                    localAngles = new Vector3(10.25951F, 127.3924F, 352.6849F),
                    localScale = new Vector3(0.35804F, 0.35804F, 0.35804F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00738F, 0.00257F, 0F),
                    localAngles = new Vector3(11.14201F, 270F, 0F),
                    localScale = new Vector3(0.00435F, 0.00435F, 0.00435F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            GetStatCoefficients += GrantBaseShield;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GrantShieldReward;
            RoR2Application.onLoad += OnLoadModCompat;
        }

        private void OnLoadModCompat()
        {
        }

        private void GrantBaseShield(CharacterBody sender, StatHookEventArgs args)
        {
            if (GetCount(sender) > 0)
            {
                HealthComponent healthC = sender.GetComponent<HealthComponent>();
                args.baseShieldAdd += healthC.fullHealth * BaseGrantShieldMultiplier;
            }
        }

        private void GrantShieldReward(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, RoR2.GlobalEventManager self, RoR2.DamageReport damageReport)
        {
            if (damageReport?.attackerBody)
            {
                int inventoryCount = GetCount(damageReport.attackerBody);
                if (inventoryCount > 0)
                {
                    var percentage = InverseHyperbolicScaling(ShieldPercentageRestoredPerKill, AdditionalShieldPercentageRestoredPerKillDiminishing, MaximumPercentageShieldRestoredPerKill, inventoryCount);
                    damageReport.attackerBody.healthComponent.RechargeShield(damageReport.attackerBody.healthComponent.fullShield * percentage);
                }
            }
            orig(self, damageReport);
        }
    }
}