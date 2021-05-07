using Aetherium.Utils;
using BepInEx.Configuration;
using ItemStats;
using ItemStats.Stat;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;

namespace Aetherium.Items
{
    public class AlienMagnet : ItemBase<AlienMagnet>
    {
        public float StartingForceMultiplier;
        public float AdditionalForceMultiplier;
        public float MinimumForceMultiplier;
        public float MaximumForceMultiplier;

        public override string ItemName => "Alien Magnet";
        public override string ItemLangTokenName => "ALIEN_MAGNET";
        public override string ItemPickupDesc => "Your attacks pull enemies towards you.";
        public override string ItemFullDescription => $"Enemies hit by your attacks will be pulled towards you, starting at {StartingForceMultiplier}x force <style=cStack>(+{AdditionalForceMultiplier}x force multiplier, up to {MaximumForceMultiplier}x total force. The effect is more noticeable on higher health enemies.)</style>";
        public override string ItemLore => "A strange pylon that seems to bring enemies towards the wielder when their attacks hit. Only the truly brave or insane bring the fight to themselves.";

        public override ItemTier Tier => ItemTier.Lunar;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.Cleansable };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("AlienMagnet.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("AlienMagnetIcon.png");

        public override bool CanRemove => false;

        public static GameObject ItemBodyModelPrefab;
        public static GameObject ItemFollowerPrefab;

        public AlienMagnet()
        {
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            StartingForceMultiplier = config.Bind<float>("Item: " + ItemName, "Starting Pull Force Multiplier", 3f, "What should the starting pull force multiplier of the Alien Magnet's pull be?").Value;
            AdditionalForceMultiplier = config.Bind<float>("Item: " + ItemName, "Additional Pull Force Multiplier per Stack", 2f, "How much additional force multiplier should be granted per Alien Magnet stack?").Value;
            MinimumForceMultiplier = config.Bind<float>("Item: " + ItemName, "Minimum Pull Force Multiplier", 3f, "What should the minimum force multiplier be for the Alien Magnet?").Value;
            MaximumForceMultiplier = config.Bind<float>("Item: " + ItemName, "Maximum Pull Force Multiplier", 10f, "What should the maximum force multiplier be for the Alien Magnet?").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("AlienMagnetTracker.prefab");
            ItemFollowerPrefab = ItemModel;

            var ItemFollower = ItemBodyModelPrefab.AddComponent<ItemFollowerSmooth>();
            ItemFollower.itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemFollower.itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);
            ItemFollower.followerPrefab = ItemFollowerPrefab;
            ItemFollower.targetObject = ItemBodyModelPrefab;
            ItemFollower.distanceDampTime = 0.10f;
            ItemFollower.distanceMaxSpeed = 100;
            ItemFollower.SmoothingNumber = 0.25f;

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            //ruleLookup.Add("mdlHuntress", 0.1f);
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-4f, -2f, 5f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(5f, 0f, 10f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.039F, -0.8778F, -0.5109F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            if (IsItemStatsModInstalled)
            {
                RoR2Application.onLoad += ItemStatsModCompat;
            }

            On.RoR2.HealthComponent.TakeDamage += GetOverHere;
        }

        private void ItemStatsModCompat()
        {
            ItemStatDef AlienMagnetStatDefs = new ItemStatDef
            {
                Stats = new List<ItemStat>()
                {
                    new ItemStat
                    (
                        (itemCount, ctx) => Mathf.Clamp(StartingForceMultiplier + (AdditionalForceMultiplier * (itemCount - 1)), MinimumForceMultiplier, MaximumForceMultiplier),
                        (value, ctx) => $"Pull Force Multiplier: {value}"
                    )
                }
            };
            ItemStatsMod.AddCustomItemStatDef(ItemDef.itemIndex, AlienMagnetStatDefs);
        }

        private void GetOverHere(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            if (damageInfo?.attacker)
            {
                var attackerBody = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
                if (attackerBody)
                {
                    int ItemCount = GetCount(attackerBody);
                    if (ItemCount > 0)
                    {
                        //Thanks Chen for fixing this.
                        float mass;
                        if (self.body.characterMotor) mass = self.body.characterMotor.mass;
                        else if (self.body.rigidbody) mass = self.body.rigidbody.mass;
                        else mass = 1f;

                        var forceCalc = Mathf.Clamp(StartingForceMultiplier + (AdditionalForceMultiplier * (ItemCount - 1)), MinimumForceMultiplier, MaximumForceMultiplier);
                        damageInfo.force += Vector3.Normalize(attackerBody.corePosition - self.body.corePosition) * forceCalc * mass;
                    }
                }
            }
            orig(self, damageInfo);
        }
    }
}
