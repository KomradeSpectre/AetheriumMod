using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Utils.MiscHelpers;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static R2API.RecalculateStatsAPI;

namespace Aetherium.Items.Tier1
{
    public class FeatheredPlume : ItemBase<FeatheredPlume>
    {
        public static ConfigOption<bool> UseNewIcons;
        public static ConfigOption<float> BaseDurationOfBuffInSeconds;
        public static ConfigOption<float> AdditionalDurationOfBuffInSeconds;
        public static ConfigOption<int> BuffStacksPerFeatheredPlume;
        public static ConfigOption<float> MoveSpeedPercentageBonusPerBuffStack;

        public override string ItemName => "Feathered Plume";
        public override string ItemLangTokenName => "FEATHERED_PLUME";
        public override string ItemPickupDesc => "After taking damage, gain a boost in speed.";
        public override string ItemFullDescription => $"Gain a temporary <style=cIsUtility>{FloatToPercentageString(MoveSpeedPercentageBonusPerBuffStack)} speed boost</style> upon taking damage that stacks {BuffStacksPerFeatheredPlume} times for {BaseDurationOfBuffInSeconds} seconds. <style=cStack>(+{BuffStacksPerFeatheredPlume} stacks and +{AdditionalDurationOfBuffInSeconds} second duration per additional Feathered Plume.)</style>";
        public override string ItemLore => OrderManifestLoreFormatter(
            ItemName,
            "05/05/2077",
            "Unmarked Drop Point #951",
            "591********",
            ItemPickupDesc,
            "Bio-metal / Bio-Hazardous / Light",

            "A couple feathers from a legendary alloy vulture. Scans show that the quills generate a powerful muscle stimulant upon taking an impact. Our field testers have also reported this," +
            " albeit their exact words is that the feathers allow them to 'haul ass' out of danger, and that they had a need.\n" +
            "A need for speed.");

        public override ItemTier Tier => ItemTier.Tier1;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

        public override string CorruptsItem => "ITEM_NAIL_BOMB_NAME";

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("FeatheredPlume.prefab");
        public override Sprite ItemIcon => UseNewIcons ? MainAssets.LoadAsset<Sprite>("FeatheredPlumeIconAlt.png") : MainAssets.LoadAsset<Sprite>("FeatheredPlumeIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public BuffDef SpeedBuff { get; private set; }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuff();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            UseNewIcons = config.ActiveBind<bool>("Item: " + ItemName, "Use Alternative Icon Art?", false, "If set to true, will use the new icon art drawn by WaltzingPhantom, else it will use the old icon art.");
            BaseDurationOfBuffInSeconds = config.ActiveBind<float>("Item: " + ItemName, "Base Duration of Buff with One Feathered Plume", 5f, "How many seconds should feathered plume's buff last with a single feathered plume?");
            AdditionalDurationOfBuffInSeconds = config.ActiveBind<float>("Item: " + ItemName, "Additional Duration of Buff per Feathered Plume Stack", 0.5f, "How many additional seconds of buff should each feathered plume after the first give?");
            BuffStacksPerFeatheredPlume = config.ActiveBind<int>("Item: " + ItemName, "Stacks of Buff per Feathered Plume", 3, "How many buff stacks should each feather give?");
            MoveSpeedPercentageBonusPerBuffStack = config.ActiveBind<float>("Item: " + ItemName, "Movement speed per Feathered Plume Buff Stack", 0.07f, "How much movement speed in percent should each stack of Feathered Plume's buff give? (0.07 = 7%)");
        }

        private void CreateBuff()
        {
            SpeedBuff = ScriptableObject.CreateInstance<BuffDef>();
            SpeedBuff.name = "Aetherium: Feathered Plume Speed";
            SpeedBuff.buffColor = Color.green;
            SpeedBuff.canStack = true;
            SpeedBuff.isDebuff = false;
            SpeedBuff.iconSprite = MainAssets.LoadAsset<Sprite>("FeatheredPlumeBuffIcon.png");

            ContentAddition.AddBuffDef(SpeedBuff);

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab, true);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.42142F, -0.10234F),
                    localAngles = new Vector3(351.1655F, 45.64202F, 351.1029F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.35414F, -0.14761F),
                    localAngles = new Vector3(356.5505F, 45.08208F, 356.5588F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0, 2.46717F, 2.64379F),
                    localAngles = new Vector3(315.5635F, 233.7695F, 325.0397F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0, 0.24722F, -0.01662F),
                    localAngles = new Vector3(10.68209F, 46.03322F, 11.01807F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.24128F, -0.14951F),
                    localAngles = new Vector3(6.07507F, 45.37084F, 6.11489F),
                    localScale = new Vector3(0.17F, 0.17F, 0.17F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.31304F, -0.00747F),
                    localAngles = new Vector3(359.2931F, 45.00048F, 359.2912F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0, 1.94424F, -0.47558F),
                    localAngles = new Vector3(20.16552F, 48.87548F, 21.54582F),
                    localScale = new Vector3(1.5F, 1.5F, 1.5F)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.30118F, -0.0035F),
                    localAngles = new Vector3(8.31363F, 45.67525F, 8.41428F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, -0.65444F, 1.64345F),
                    localAngles = new Vector3(326.1803F, 277.2657F, 249.9269F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.0068F, 0.3225F, -0.03976F),
                    localAngles = new Vector3(0F, 45F, 0F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.14076F, 0.15542F, -0.04648F),
                    localAngles = new Vector3(356.9802F, 81.10978F, 353.687F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hat",
                    localPos = new Vector3(0F, 0.01217F, -0.00126F),
                    localAngles = new Vector3(356.9376F, 25.8988F, 14.69767F),
                    localScale = new Vector3(0.01F, 0.01F, 0.01F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00042F, 0.46133F, 0.01385F),
                    localAngles = new Vector3(355.2848F, 47.55381F, 355.0908F),
                    localScale = new Vector3(0.20392F, 0.20392F, 0.20392F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00076F, -0.0281F, 0.09539F),
                    localAngles = new Vector3(338.9489F, 145.7505F, 217.6883F),
                    localScale = new Vector3(0.05402F, 0.05402F, 0.05402F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, -0.00277F, -0.13259F),
                    localAngles = new Vector3(322.1495F, 124.8318F, 235.476F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.32104F, 0F),
                    localAngles = new Vector3(0F, 321.2954F, 0F),
                    localScale = new Vector3(0.24027F, 0.24027F, 0.24027F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0.00216F, 0.01033F, 0F),
                    localAngles = new Vector3(0F, 323.6887F, 355.1232F),
                    localScale = new Vector3(0.00551F, 0.00551F, 0.00551F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += CalculateSpeedReward;
            GetStatCoefficients += AddSpeedReward;
        }

        private void CalculateSpeedReward(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            var body = self.body;
            if (body)
            {
                var buffCount = body.GetBuffCount(SpeedBuff);
                var InventoryCount = GetCount(body);

                if (InventoryCount > 0)
                {
                    var stackTime = BaseDurationOfBuffInSeconds + (AdditionalDurationOfBuffInSeconds * (InventoryCount - 1));
                    if (buffCount < BuffStacksPerFeatheredPlume * InventoryCount)
                    {
                        ItemHelpers.RefreshTimedBuffs(body, SpeedBuff, stackTime);
                        body.AddTimedBuffAuthority(SpeedBuff.buffIndex, stackTime);
                    }
                    if(buffCount >= BuffStacksPerFeatheredPlume * InventoryCount)
                    {
                        ItemHelpers.RefreshTimedBuffs(body, SpeedBuff, stackTime);
                    }
                }
            }
            orig(self, damageInfo);
        }

        private void AddSpeedReward(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(SpeedBuff)) { args.moveSpeedMultAdd += MoveSpeedPercentageBonusPerBuffStack * sender.GetBuffCount(SpeedBuff); }
        }
    }
}