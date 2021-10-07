using Aetherium.Utils;
using BepInEx.Configuration;
using ItemStats;
using ItemStats.Stat;
using R2API;
using RoR2;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Compatability.ModCompatability.ItemStatsModCompat;

namespace Aetherium.Items
{
    public class AlienMagnet : ItemBase<AlienMagnet>
    {
        public static float StartingForceMultiplier;
        public static float AdditionalForceMultiplier;
        public static float MinimumForceMultiplier;
        public static float MaximumForceMultiplier;

        public override string ItemName => "Alien Magnet";
        public override string ItemLangTokenName => "ALIEN_MAGNET";
        public override string ItemPickupDesc => "Your attacks pull enemies towards you.";
        public override string ItemFullDescription => $"Enemies hit by your attacks will be pulled towards you, starting at {StartingForceMultiplier}x force <style=cStack>(+{AdditionalForceMultiplier}x force multiplier, up to {MaximumForceMultiplier}x total force. The effect is more noticeable on higher health enemies.)</style>";
        public override string ItemLore => 

            "[BEGINNING LOG]\n\n" +

            "Experiment Log #476951-b\n" +
            "Subject Matter: Unknown Omni-Magnetic Xenomineral" +

            "\n<indent=5%>I'm recoding this video log in complete awe of the discovery before us. What we understood about magnetic properties of minerals had generally revealed to us that there a variety" +
            "of materials that were not able to be affected in any large scale manner by standard magnetics. One of these would be flesh. If I drop this neodymium magnet on my arm like so...You notice that" +
            "nothing of note happens. The magnet just falls right onto the table. This is what we've known as our normal for magnetics, but this mineral seems to rewrite that constant.\n" +

            "\n<indent=5%>Now we'll repeat our last test, but using a chunk of this mineral. I've been reassured that it is completely safe to test on us, and has no radioactive properties to it. So here we" +
            "go. We just drop this right past the arm like so...*THUNK* *CRACK* *SQUELCH*\n" +

            "AAAAAAAAAAAAGH!!! AAAAAAAAAAAAAAA!!!\n" +
            "[END OF LOG]" +

            "[TRAUMA TEAM HAS BEEN DISPATCHED TO LAB 5]";

        public override ItemTier Tier => ItemTier.Lunar;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.Cleansable };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("AlienMagnet.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("AlienMagnetIcon.png");

        public override bool CanRemove => false;

        public override GameObject ItemBodyModelPrefab { get; internal set; }

        public static GameObject ItemFollowerPrefab;

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
            ItemModel.AddComponent<AlienMagnetAnimationController>();
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);
            /*ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("AlienMagnetTracker.prefab");
            ItemFollowerPrefab = ItemModel;

            var ItemFollower = ItemBodyModelPrefab.AddComponent<ItemFollowerSmooth>();
            ItemFollower.itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemFollower.itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab, true);
            ItemFollower.followerPrefab = ItemFollowerPrefab;
            ItemFollower.targetObject = ItemBodyModelPrefab;
            ItemFollower.distanceDampTime = 0.10f;
            ItemFollower.distanceMaxSpeed = 100;
            ItemFollower.SmoothingNumber = 0.25f;*/

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
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
                    localAngles = new Vector3(-90f, 0f, 0f),
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
                    localPos = new Vector3(-9.05238F, -2F, 5.00013F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.28301F, -0.34921F, -1.00009F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.15F, 0.15F, 0.15F)
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
                    localAngles = new Vector3(-90f, 0f, 0f),
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
                    localAngles = new Vector3(-90f, 0f, 0f),
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
                    localAngles = new Vector3(-90f, 0f, 0f),
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
                    localAngles = new Vector3(-90f, 0f, 0f),
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
                    localPos = new Vector3(5F, 0F, 10F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.70726F, -0.17282F, -1.00137F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.15F, 0.15F, 0.15F)
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
                    localAngles = new Vector3(-90f, 0F, 0F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += GetOverHere;
            RoR2Application.onLoad += OnLoadModCompat;
        }

        private void OnLoadModCompat()
        {
            if (IsItemStatsModInstalled)
            {
                CreateAlienMagnetStatDef();
            }
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

        public class AlienMagnetAnimationController : MonoBehaviour
        {
            public void Start()
            {
                var animators = gameObject.GetComponentsInChildren<Animator>();
                foreach(Animator animator in animators)
                {
                    switch (animator.gameObject.name)
                    {
                        case ("AnimationControl"):
                            animator.Play("BaseLayer.AlienMagnetMain");
                            break;
                        case ("Torus.002"):
                            animator.Play("BaseLayer.AlienMagnetBlueTorus");
                            break;
                        case ("Torus.003"):
                            animator.Play("BaseLayer.AlienMagnetRedTorus");
                            break;
                    }
                }
            }
        }
    }
}
