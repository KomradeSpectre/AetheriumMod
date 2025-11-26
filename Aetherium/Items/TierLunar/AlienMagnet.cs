using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;

namespace Aetherium.Items.TierLunar
{
    public class AlienMagnet : ItemBase<AlienMagnet>
    {
        public static ConfigOption<float> OldStartingForceMultiplier;
        public static ConfigOption<float> OldAdditionalForceMultiplier;
        public static ConfigOption<float> OldMinimumForceMultiplier;
        public static ConfigOption<float> OldMaximumForceMultiplier;
        public static ConfigOption<float> NewStartingForceMultiplier;
        public static ConfigOption<float> NewAdditionalForceMultiplier;
        public static ConfigOption<bool> UseOldFunctionality;

        public override string ItemName => "Alien Magnet";
        public override string ItemLangTokenName => "ALIEN_MAGNET";
        public override string ItemPickupDesc => "Your attacks pull enemies towards you.";
        public override string ItemFullDescription => $"Enemies hit by your attacks will be pulled towards you, starting at {(UseOldFunctionality ? OldStartingForceMultiplier : NewStartingForceMultiplier)}x force <style=cStack>(+{(UseOldFunctionality ? OldAdditionalForceMultiplier : NewAdditionalForceMultiplier)}x force multiplier{(UseOldFunctionality ? $", up to {OldMaximumForceMultiplier}x total force. The effect is more noticeable on higher health enemies." : ".")}</style>";
        public override string ItemLore => 

            "[BEGINNING LOG]\n\n" +

            "Experiment Log #476951-b\n" +
            "Subject Matter: Unknown Omni-Magnetic Xenomineral" +

            "\n<indent=5%>I'm recording this video log in complete awe of the discovery before us. What we understood about magnetic properties of minerals had generally revealed to us that there a variety" +
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

        public static GameObject ItemBodyModelPrefab;
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
            NewStartingForceMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Starting Pull Force Multiplier", 20f, "What should the starting pull force multiplier of the Alien Magnet's pull be?");
            NewAdditionalForceMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Additional Pull Force Multiplier per Stack", 5f, "How much additional force multiplier should be granted per Alien Magnet stack?");

            UseOldFunctionality = config.ActiveBind<bool>("Item: " + ItemName, "Use Old Alien Magnet Functionality?", false, "Should we use the old, chaotic functionality of Alien Magnet?");
            OldStartingForceMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Starting Pull Force Multiplier", 3f, "What should the starting pull force multiplier of the Alien Magnet's pull be? (OLD FUNCTIONALITY ONLY)");
            OldAdditionalForceMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Additional Pull Force Multiplier per Stack", 2f, "How much additional force multiplier should be granted per Alien Magnet stack? (OLD FUNCTIONALITY ONLY)");
            OldMinimumForceMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Minimum Pull Force Multiplier", 3f, "What should the minimum force multiplier be for the Alien Magnet? (OLD FUNCTIONALITY ONLY)");
            OldMaximumForceMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Maximum Pull Force Multiplier", 10f, "What should the maximum force multiplier be for the Alien Magnet? (OLD FUNCTIONALITY ONLY)");
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
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chef",
                    localPos = new Vector3(0.03661F, 0.01391F, 0.03791F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(0.00424F, 0.00424F, 0.00424F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.34614F, 1.44644F, -0.56914F),
                    localAngles = new Vector3(4.32037F, 93.26095F, 357.2377F),
                    localScale = new Vector3(0.14018F, 0.14018F, 0.14018F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Root",
                    localPos = new Vector3(0.741F, 1.24812F, -0.85671F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.09F, 0.09F, 0.09F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.41153F, 0.6322F, -0.27579F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.05383F, 0.05383F, 0.05383F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Root",
                    localPos = new Vector3(0.53893F, -0.16848F, 0.68904F),
                    localAngles = new Vector3(275.9082F, 269.7766F, 180.526F),
                    localScale = new Vector3(0.14648F, 0.14648F, 0.14648F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.01511F, 0.03003F, 0.03958F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.00216F, 0.00216F, 0.00216F)
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
        }

        private void GetOverHere(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            if (damageInfo?.attacker)
            {
                if (self.body)
                {
                    var attackerBody = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
                    if (attackerBody)
                    {
                        int ItemCount = GetCount(attackerBody);
                        if (ItemCount > 0)
                        {
                            if (UseOldFunctionality)
                            {
                                //Thanks Chen for fixing this.
                                float mass;
                                if (self.body.characterMotor) mass = (self.body.characterMotor as IPhysMotor).mass;
                                else if (self.body.rigidbody) mass = self.body.rigidbody.mass;
                                else mass = 1f;

                                var forceCalc = Mathf.Clamp(OldStartingForceMultiplier + (OldAdditionalForceMultiplier * (ItemCount - 1)), OldMinimumForceMultiplier, OldMaximumForceMultiplier);
                                damageInfo.force += Vector3.Normalize(attackerBody.corePosition - self.body.corePosition) * forceCalc * mass;
                            }
                            else
                            {
                                var forceCalc = (NewStartingForceMultiplier + (NewAdditionalForceMultiplier * (ItemCount - 1)));

                                if (self)
                                {
                                    if (self.body)
                                    {
                                        if (self.body.rigidbody)
                                        {
                                            if (self.body.rigidbody.velocity != null)
                                            {
                                                Vector3 directionToPlayer = (attackerBody.corePosition - self.body.corePosition).normalized;
                                                Vector3 projectedVelocity = Vector3.Project(self.body.rigidbody.velocity, directionToPlayer);

                                                float maxVelocity = 5 * ItemCount;

                                                float velocityDifference = Mathf.Max(0, maxVelocity - projectedVelocity.magnitude);
                                                if (velocityDifference != null)
                                                {
                                                    var desiredForce = directionToPlayer * velocityDifference;

                                                    if (desiredForce != null)
                                                    {
                                                        var physInfo = new PhysForceInfo()
                                                        {
                                                            massIsOne = true,
                                                            disableAirControlUntilCollision = true,
                                                            ignoreGroundStick = true,
                                                            force = desiredForce,
                                                        };

                                                        var characterMotor = self.body.characterMotor;
                                                        if (characterMotor && characterMotor != null)
                                                        {
                                                            characterMotor.ApplyForceImpulseFixed(physInfo);
                                                            orig(self, damageInfo);
                                                            return;
                                                        }

                                                        var rigidBodyMotor = self.body.GetComponent<RigidbodyMotor>();
                                                        if (rigidBodyMotor && rigidBodyMotor != null)
                                                        {
                                                            rigidBodyMotor.ApplyForceImpulse(physInfo);
                                                            orig(self, damageInfo);
                                                            return;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
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
