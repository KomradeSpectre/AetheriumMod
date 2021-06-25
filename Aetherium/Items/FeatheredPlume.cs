using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using ItemStats;
using ItemStats.Stat;
using ItemStats.ValueFormatters;

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Compatability.ModCompatability.BetterUICompat;
using static Aetherium.Compatability.ModCompatability.ItemStatsModCompat;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static R2API.RecalculateStatsAPI;

namespace Aetherium.Items
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
            UseNewIcons = config.ActiveBind<bool>("Item: " + ItemName, "Use Alternative Icon Art?", true, "If set to true, will use the new icon art drawn by WaltzingPhantom, else it will use the old icon art.");
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

            BuffAPI.Add(new CustomBuff(SpeedBuff));

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab, true);

            Vector3 generalScale = new Vector3(0.4f, 0.4f, 0.4f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00712F, 0.32361F, -0.07863F),
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
                    localPos = new Vector3(0.01572F, 0.25567F, -0.15293F),
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
                    localPos = new Vector3(-0.20158F, 1.96813F, 1.90146F),
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
                    localPos = new Vector3(0.00026F, 0.12435F, -0.03981F),
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
                    localPos = new Vector3(-0.00076F, 0.14591F, -0.04721F),
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
                    localPos = new Vector3(0.01096F, 0.21896F, -0.00683F),
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
                    localPos = new Vector3(0.14919F, 1.2111F, -0.47559F),
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
                    localPos = new Vector3(0.00979F, 0.21382F, -0.00342F),
                    localAngles = new Vector3(8.31363F, 45.67526F, 8.41428F),
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
                    localPos = new Vector3(0.0014F, -0.13153F, 0.92116F),
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
                    localPos = new Vector3(0.03226F, 0.21641F, -0.0457F),
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
                    localPos = new Vector3(-0.12881F, 0.06795F, -0.04607F),
                    localAngles = new Vector3(357.1273F, 75.26414F, 354.6212F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += CalculateSpeedReward;
            GetStatCoefficients += AddSpeedReward;
            RoR2Application.onLoad += OnLoadModCompat;
        }

        private void OnLoadModCompat()
        {
            if (IsItemStatsModInstalled)
            {
                CreateFeatheredPlumeStatDef();
            }

            if (IsBetterUIInstalled)
            {
                var speedBuffInfo = CreateBetterUIBuffInformation($"{ItemLangTokenName}_SPEED_BUFF", SpeedBuff.name, "The pain triggers a release of adrenaline in your veins from the feathers. You feel quicker!");

                RegisterBuffInfo(SpeedBuff, speedBuffInfo.Item1, speedBuffInfo.Item2);
            }
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
                        RefreshTimedBuffs(body, SpeedBuff, stackTime);
                        body.AddTimedBuffAuthority(SpeedBuff.buffIndex, stackTime);
                    }
                    if(buffCount >= BuffStacksPerFeatheredPlume * InventoryCount)
                    {
                        RefreshTimedBuffs(body, SpeedBuff, stackTime);
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