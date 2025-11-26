using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using UnityEngine;

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Compatability.ModCompatability;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static R2API.RecalculateStatsAPI;
using RoR2.Achievements;
using Aetherium.Achievements;

namespace Aetherium.Items.Tier2
{
    public class ShieldingCore : ItemBase<ShieldingCore>
    {
        public static ConfigOption<bool> EnableParticleEffects;
        public static ConfigOption<float> BaseShieldingCoreArmorGrant;
        public static ConfigOption<float> AdditionalShieldingCoreArmorGrant;
        public static ConfigOption<float> BaseGrantShieldMultiplier;

        public override string ItemName => "Shielding Core";

        public override string ItemLangTokenName => "SHIELDING_CORE";

        public override string ItemPickupDesc => "While shielded, gain a temporary boost in <style=cIsUtility>armor</style>.";

        public override string ItemFullDescription => $"You gain <style=cIsUtility>{BaseShieldingCoreArmorGrant}</style> <style=cStack>(+{AdditionalShieldingCoreArmorGrant} per stack)</style> <style=cIsUtility>armor</style> while <style=cIsUtility>BLUE shields</style> are active." +
            $" The first stack of this item will grant <style=cIsUtility>{FloatToPercentageString(BaseGrantShieldMultiplier)}</style> of your max health as shield on pickup.";

        public override string ItemLore => 

            "\nEngineer's report:\n\n" +

            "   Let me preface this with a bit of honesty, I do not know what the green goo inside my little turbine is. " +
            "I bought an aftermarket resonator from one of the junk dealers our ship passed, because I was running low on parts to repair our shield generators. " +
            "As soon as I slotted this thing in, I'm covered in this gross liquid that seems to dissipate into these sparkly crystals when exposed to air. " +
            "Normally this wouldn't be much of an issue since I'm in a suit, but the stuff was constantly attempting to fill the container it occupied so I had to create a seal for it. " +
            "That's when my suit diagnostics alarmed me that my shield's efficacy hit the roof.\n\n" +
            "Eureka moment, and a few design drafts later.\n" +
            "Now I'm selling these things like hotcakes and making a profit. So here's one for you.\n\n" +
            "P.S. Don't expose your skin to this stuff, it may cause over 200 known forms of cancer. That's our secret though, right?";

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("PickupShieldingCore.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("ShieldingCoreIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public Material OriginalShieldMaterial;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            //CreateAchievement();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            EnableParticleEffects = config.ActiveBind<bool>("Item: " + ItemName, "Enable Particle Effects", true, "Should the particle effects for the models be enabled?");
            BaseShieldingCoreArmorGrant = config.ActiveBind<float>("Item: " + ItemName, "First Shielding Core Bonus to Armor", 15f, "How much armor should the first Shielding Core grant?");
            AdditionalShieldingCoreArmorGrant = config.ActiveBind<float>("Item: " + ItemName, "Additional Shielding Cores Bonus to Armor", 10f, "How much armor should each additional Shielding Core grant?");
            BaseGrantShieldMultiplier = config.ActiveBind<float>("Item: " + ItemName, "First Shielding Core Bonus to Max Shield", 0.08f, "How much should the starting shield be upon receiving the item?");
        }

        private void CreateAchievement()
        {
            if (RequireUnlock)
            {
                var achievement = new ShieldingCoreAchievement();
                achievement.Init();
                if (achievement.UnlockableDef)
                {
                    ItemUnlockableDef = achievement.UnlockableDef;
                }
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("DisplayShieldingCore.prefab");
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab); ;
            //if (EnableParticleEffects) { ItemBodyModelPrefab.AddComponent<ShieldingCoreVisualCueController>(); }

            Vector3 generalScale = new Vector3(0.2f, 0.2f, 0.2f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00037F, 0.34304F, -0.16813F),
                    localAngles = new Vector3(59.76984F, 180.0994F, 0.12059F),
                    localScale = new Vector3(0.3F, 0.3F, 0.3F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00006F, 0.1931F, -0.11347F),
                    localAngles = new Vector3(68.04121F, 232.2084F, 53.81786F),
                    localScale = new Vector3(0.3F, 0.3F, 0.3F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.73453F, -1.60691F),
                    localAngles = new Vector3(90F, 180F, 0F),
                    localScale = new Vector3(2.5F, 2.5F, 2.5F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00479F, 0.14605F, -0.28389F),
                    localAngles = new Vector3(90F, 180F, 0F),
                    localScale = new Vector3(0.43F, 0.43F, 0.43F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00046F, 0.05743F, -0.13412F),
                    localAngles = new Vector3(38.05768F, 357.8512F, 178.9748F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.18383F, -0.27827F),
                    localAngles = new Vector3(75.11288F, 180F, 0F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0F, 0.30485F, 0F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(2.5F, 2.5F, 2.5F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.22424F, -0.24409F),
                    localAngles = new Vector3(81.89301F, 180F, 0F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00001F, 0.10571F, 4.09442F),
                    localAngles = new Vector3(72.38651F, 180F, 180F),
                    localScale = new Vector3(3F, 4F, 3F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00002F, 0.1999F, -0.18968F),
                    localAngles = new Vector3(90F, 180F, 0F),
                    localScale = new Vector3(0.6F, 0.6F, 0.6F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00047F, 0.21561F, -0.15879F),
                    localAngles = new Vector3(90F, 180F, 0F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Body",
                    localPos = new Vector3(0F, 0.0123F, -0.00927F),
                    localAngles = new Vector3(83.47623F, 0.00001F, 180F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, -0.11317F, -0.16661F),
                    localAngles = new Vector3(87.0937F, 0.00001F, 180F),
                    localScale = new Vector3(0.55F, 0.55F, 0.55F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.12598F, -0.09886F),
                    localAngles = new Vector3(77.63712F, 180F, 0F),
                    localScale = new Vector3(0.25657F, 0.25657F, 0.25657F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.17618F, 0.09889F),
                    localAngles = new Vector3(55.16992F, 0F, 0F),
                    localScale = new Vector3(0.15795F, 0.15795F, 0.15795F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.28736F, 0.13478F, 0F),
                    localAngles = new Vector3(86.65497F, 270F, 180F),
                    localScale = new Vector3(0.54194F, 0.54194F, 0.54194F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.00502F, 0.00483F, 0F),
                    localAngles = new Vector3(277.5818F, 270F, 0F),
                    localScale = new Vector3(0.01F, 0.01357F, 0.01F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            GetStatCoefficients += GrantBaseShield;
            On.RoR2.CharacterBody.FixedUpdate += ShieldedCoreValidator;
            GetStatCoefficients += ShieldedCoreArmorCalc;
            RoR2.RoR2Application.onLoad += OnLoadModCompat;
        }

        private void OnLoadModCompat()
        {
        }

        private void GrantBaseShield(RoR2.CharacterBody sender, StatHookEventArgs args)
        {
            if (GetCount(sender) > 0)
            {
                RoR2.HealthComponent healthC = sender.GetComponent<RoR2.HealthComponent>();
                args.baseShieldAdd += healthC.fullHealth * BaseGrantShieldMultiplier;
            }
        }

        private void ShieldedCoreValidator(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            orig(self);

            if(self && self.healthComponent)
            {
                var shieldComponent = self.GetComponent<ShieldedCoreComponent>();
                if (!shieldComponent) { shieldComponent = self.gameObject.AddComponent<ShieldedCoreComponent>(); }

                var newInventoryCount = GetCount(self);
                var IsShielded = self.healthComponent.shield > 0;

                bool IsDifferent = false;
                if (shieldComponent.cachedInventoryCount != newInventoryCount)
                {
                    IsDifferent = true;
                    shieldComponent.cachedInventoryCount = newInventoryCount;
                }
                if (shieldComponent.cachedIsShielded != IsShielded)
                {
                    IsDifferent = true;
                    shieldComponent.cachedIsShielded = IsShielded;
                }

                if (!IsDifferent) return;

                self.statsDirty = true;
            }
        }

        private void ShieldedCoreArmorCalc(RoR2.CharacterBody sender, StatHookEventArgs args)
        {
            var ShieldedCoreComponent = sender.GetComponent<ShieldedCoreComponent>();
            if (ShieldedCoreComponent && ShieldedCoreComponent.cachedIsShielded && ShieldedCoreComponent.cachedInventoryCount > 0)
            {
                args.armorAdd += BaseShieldingCoreArmorGrant + (AdditionalShieldingCoreArmorGrant * (ShieldedCoreComponent.cachedInventoryCount - 1));
            }
        }

        public class ShieldedCoreComponent : MonoBehaviour
        {
            public int cachedInventoryCount = 0;
            public bool cachedIsShielded = false;
        }

        public class ShieldingCoreVisualCueController : MonoBehaviour
        {
            public RoR2.ItemDisplay ItemDisplay;
            public ParticleSystem[] ParticleSystem;
            public RoR2.CharacterMaster OwnerMaster;
            public RoR2.CharacterBody OwnerBody;
            public void FixedUpdate()
            {

                if (!OwnerMaster || !ItemDisplay || ParticleSystem.Length != 3)
                {
                    ItemDisplay = this.GetComponentInParent<RoR2.ItemDisplay>();
                    if (ItemDisplay)
                    {
                        ParticleSystem = ItemDisplay.GetComponentsInChildren<ParticleSystem>();
                        //Debug.Log("Found ItemDisplay: " + itemDisplay);
                        var characterModel = ItemDisplay.GetComponentInParent<RoR2.CharacterModel>();

                        if (characterModel)
                        {
                            var body = characterModel.body;
                            if (body)
                            {
                                OwnerMaster = body.master;
                            }
                        }
                    }
                }

                if (OwnerMaster && !OwnerBody)
                {
                    var body = OwnerMaster.GetBody();
                    if (body)
                    {
                        OwnerBody = body;
                    }
                    if (!body)
                    {
                        if (ParticleSystem.Length == 3)
                        {
                            for(int i = 0; i < ParticleSystem.Length; i++)
                            {
                                UnityEngine.Object.Destroy(ParticleSystem[i]);
                            }
                        }
                        UnityEngine.Object.Destroy(this);
                    }
                }

                if (OwnerBody && ParticleSystem.Length == 3)
                {
                    foreach (ParticleSystem particleSystem in ParticleSystem)
                    {
                        if (OwnerBody.healthComponent.shield > 0)
                        {
                            if (!particleSystem.isPlaying && ItemDisplay.visibilityLevel != VisibilityLevel.Invisible)
                            {
                                particleSystem.Play();
                            }
                            else
                            {
                                if (particleSystem.isPlaying && ItemDisplay.visibilityLevel == VisibilityLevel.Invisible)
                                {
                                    particleSystem.Stop();
                                    particleSystem.Clear();
                                }
                            }
                        }
                        else
                        {
                            if (particleSystem.isPlaying)
                            {
                                particleSystem.Stop();
                            }
                        }
                    }
                }
            }
        }
    }
}