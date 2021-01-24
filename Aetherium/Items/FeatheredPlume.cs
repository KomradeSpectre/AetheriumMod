using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static Aetherium.CoreModules.StatHooks;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;

namespace Aetherium.Items
{
    public class FeatheredPlume : ItemBase<FeatheredPlume>
    {
        public static bool UseNewIcons;
        public static float BaseDurationOfBuffInSeconds;
        public static float AdditionalDurationOfBuffInSeconds;
        public static int BuffStacksPerFeatheredPlume;
        public static float MoveSpeedPercentageBonusPerBuffStack;

        public override string ItemName => "Feathered Plume";
        public override string ItemLangTokenName => "FEATHERED_PLUME";
        public override string ItemPickupDesc => "After taking damage, gain a boost in speed.";
        public override string ItemFullDescription => $"Gain a temporary <style=cIsUtility>{FloatToPercentageString(MoveSpeedPercentageBonusPerBuffStack)} speed boost</style> upon taking damage that stacks {BuffStacksPerFeatheredPlume} times for {BaseDurationOfBuffInSeconds} seconds. <style=cStack>(+{BuffStacksPerFeatheredPlume} stacks and +{AdditionalDurationOfBuffInSeconds} second duration per additional Feathered Plume.)</style>";
        public override string ItemLore => "A feather plucked from a legendary alloy vulture. Field testers have noted it to allow them to 'Haul Ass' away from conflict when they get injured.";

        public override ItemTier Tier => ItemTier.Tier1;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

        public override string ItemModelPath => "@Aetherium:Assets/Models/Prefabs/Item/FeatheredPlume/FeatheredPlume.prefab";
        public override string ItemIconPath => UseNewIcons ? "@Aetherium:Assets/Textures/Icons/Item/FeatheredPlumeIconAlt.png" : "@Aetherium:Assets/Textures/Icons/Item/FeatheredPlumeIcon.png";

        public static GameObject ItemBodyModelPrefab;

        public BuffIndex SpeedBuff { get; private set; }

        public FeatheredPlume()
        {
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateMaterials();
            CreateBuff();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            UseNewIcons = config.Bind<bool>("Item: " + ItemName, "Use Alternative Icon Art?", true, "If set to true, will use the new icon art drawn by WaltzingPhantom, else it will use the old icon art.").Value;
            BaseDurationOfBuffInSeconds = config.Bind<float>("Item: " + ItemName, "Base Duration of Buff with One Feathered Plume", 5f, "How many seconds should feathered plume's buff last with a single feathered plume?").Value;
            AdditionalDurationOfBuffInSeconds = config.Bind<float>("Item: " + ItemName, "Additional Duration of Buff per Feathered Plume Stack", 0.5f, "How many additional seconds of buff should each feathered plume after the first give?").Value;
            BuffStacksPerFeatheredPlume = config.Bind<int>("Item: " + ItemName, "Stacks of Buff per Feathered Plume", 3, "How many buff stacks should each feather give?").Value;
            MoveSpeedPercentageBonusPerBuffStack = config.Bind<float>("Item: " + ItemName, "Movement speed per Feathered Plume Buff Stack", 0.07f, "How much movement speed in percent should each stack of Feathered Plume's buff give? (0.07 = 7%)").Value;
        }

        private void CreateMaterials()
        {
            

            var featherMainMaterial = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/Item/FeatheredPlume/FeatheredPlumeFeather.mat");
            featherMainMaterial.shader = AetheriumPlugin.HopooShader;
            featherMainMaterial.SetTexture("_NormalTex", Resources.Load<Texture2D>("@Aetherium:Assets/Textures/Material Textures/FeatheredPlumeNormal.png"));
            featherMainMaterial.SetFloat("_NormalStrength", 5);
            featherMainMaterial.SetFloat("_Smoothness", 0.5f);

            var quillSubMaterial = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/Item/FeatheredPlume/FeatheredPlumeQuill.mat");
            quillSubMaterial.shader = AetheriumPlugin.HopooShader;
            quillSubMaterial.SetTexture("_NormalTex", Resources.Load<Texture2D>("@Aetherium:Assets/Textures/Material Textures/10538-normal.jpg"));
            quillSubMaterial.SetFloat("_NormalStrength", 5);
            quillSubMaterial.SetFloat("_Smoothness", 0.5f);

        }

        private void CreateBuff()
        {
            var speedBuff = new R2API.CustomBuff(
            new BuffDef
            {
                buffColor = Color.green,
                canStack = true,
                isDebuff = false,
                name = "Aetherium: Feathered Plume Speed",
                iconPath = "@Aetherium:Assets/Textures/Icons/Buff/FeatheredPlumeBuffIcon.png"
            });
            SpeedBuff = R2API.BuffAPI.Add(speedBuff);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = Resources.Load<GameObject>(ItemModelPath);
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
            var InventoryCount = GetCount(self.body);
            if (InventoryCount > 0 && self.body.GetBuffCount(SpeedBuff) < BuffStacksPerFeatheredPlume * InventoryCount)
            {
                self.body.AddTimedBuffAuthority(SpeedBuff, (BaseDurationOfBuffInSeconds + (AdditionalDurationOfBuffInSeconds * InventoryCount - 1)));
            }
            orig(self, damageInfo);
        }

        private void AddSpeedReward(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(SpeedBuff)) { args.moveSpeedMultAdd += MoveSpeedPercentageBonusPerBuffStack * sender.GetBuffCount(SpeedBuff); }
        }
    }
}