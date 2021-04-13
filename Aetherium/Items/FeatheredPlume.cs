using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static Aetherium.AetheriumPlugin;
using static Aetherium.CoreModules.StatHooks;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;

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

        public FeatheredPlume()
        {
        }

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
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.4f, 0.4f, 0.4f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.35f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.30f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 2.3f, 2f),
                    localAngles = new Vector3(90f, 0f, 0f),
                    localScale = generalScale * 6
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0f, 0.15f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.15f, -0.05f),
                    localAngles = new Vector3(-22.5f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.25f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0f, 1.4f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale * 5
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.2f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0f, 0.75f),
                    localAngles = new Vector3(115f, 0f, 0f),
                    localScale = generalScale * 8
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.20f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
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