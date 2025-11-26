using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.MathHelpers;

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Aetherium.Items.Tier3
{
    public class WitchesRing : ItemBase<WitchesRing>
    {
        public static ConfigOption<float> WitchesRingTriggerThreshold;
        public static ConfigOption<float> BaseCooldownDuration;
        public static ConfigOption<float> AdditionalCooldownReduction;
        public static ConfigOption<bool> GlobalCooldownOnUse;

        public override string ItemName => "Witches Ring";

        public override string ItemLangTokenName => "WITCHES_RING";

        public override string ItemPickupDesc => $"Hits that deal <style=cIsDamage>high damage</style> will also trigger <style=cDeath>On Kill</style> effects. Enemies hit by this effect gain <style=cIsUtility>temporary immunity to it</style>.";

        public override string ItemFullDescription => $"Hits that deal <style=cIsDamage>{FloatToPercentageString(WitchesRingTriggerThreshold)} damage</style> or more will trigger <style=cDeath>On Kill</style> effects." +
            $" Upon success, {(GlobalCooldownOnUse ? "the target hit is <style=cIsUtility>granted immunity to this effect</style>" : "<style=cIsUtility>the ring must recharge</style>")} for <style=cIsUtility>{BaseCooldownDuration} second(s)</style> <style=cStack>(-{FloatToPercentageString(AdditionalCooldownReduction)} duration per stack, hyperbolically)</style>.";

        public override string ItemLore =>
            "\"The Raven calls and it commands\n" +
            "To each and every devotee\n" + 
            "The time has come, a cycle ends\n" +
            "Burn out, o sun, dry out, o sea\n\n" +

            "And here our coven gathered now\n" + 
            "Along with night wind's howling breath\n" +
            "We seal our pact with Her, this vow:\n" +
            "To bring life's long awaited death\"\n\n" +

            "Is this what's written on the ring or did you always know this poem...?";

        public override ItemTier Tier => ItemTier.Tier3;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("PickupWitchesRing.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("WitchesRingIcon.png");

        public static GameObject ItemBodyModelPrefab;
        public static GameObject CircleBodyModelPrefab;

        public static BuffDef WitchesRingImmunityBuffDef;

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
            WitchesRingTriggerThreshold = config.ActiveBind<float>("Item: " + ItemName, "Damage Multiplier Required in a Hit to Activate", 5f, "What threshold should damage have to pass to trigger the Witches Ring?");
            BaseCooldownDuration = config.ActiveBind<float>("Item: " + ItemName, "Duration of Cooldown After Use", 10f, "What should be the base duration of the Witches Ring cooldown?");
            AdditionalCooldownReduction = config.ActiveBind<float>("Item: " + ItemName, "Cooldown Duration Reduction per Additional Witches Ring (Diminishing)", 0.1f, "What percentage (hyperbolically) should each additional Witches Ring reduce the cooldown duration?");
            GlobalCooldownOnUse = config.ActiveBind<bool>("Item: " + ItemName, "Global Cooldown On Use", false, "Should the cooldown effect be applied in the same manner as Kjaro/Runald Bands, or on the victim of the effect?");
        }

        private void CreateBuff()
        {
            WitchesRingImmunityBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            WitchesRingImmunityBuffDef.name = "Aetherium: Witches Ring Immunity";
            WitchesRingImmunityBuffDef.buffColor = new Color(0, 80, 0);
            WitchesRingImmunityBuffDef.canStack = false;
            WitchesRingImmunityBuffDef.isDebuff = false;
            WitchesRingImmunityBuffDef.iconSprite = MainAssets.LoadAsset<Sprite>("WitchesRingBuffIcon.png");

            ContentAddition.AddBuffDef(WitchesRingImmunityBuffDef);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("DisplayWitchesRing.prefab");
            CircleBodyModelPrefab = MainAssets.LoadAsset<GameObject>("WitchesRingCircle.prefab");

            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            CircleBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            CircleBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(CircleBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.31f, 0.01f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(0.28f, 0.28f, 0.28f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0.00002F, 0.27F, 0F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.17F, 0.17F, 0.001F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0F, 0.18F, 0F),
                    localAngles = new Vector3(0F, 240F, 0F),
                    localScale = new Vector3(0.15761F, 0.15761F, 0.15761F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0F, 0.16F, 0F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.12061F, 0.12061F, 0.00071F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0F, 3.4F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(1.52995F, 1.52995F, 1.52995F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0F, 3.2F, 0F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(1.04152F, 1.04152F, 0.00069F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(-0.01043F, 0.19627F, 0.00471F),
                    localAngles = new Vector3(0F, 45F, 0F),
                    localScale = new Vector3(0.26772F, 0.26772F, 0.26772F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0F, 0.23035F, 0F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(0.14294F, 0.14294F, 0.00084F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00004F, -0.05823F, -0.00008F),
                    localAngles = new Vector3(10.54133F, 61.86488F, 18.8417F),
                    localScale = new Vector3(0.15706F, 0.15706F, 0.15706F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.33f, 0f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.001f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0f, 0.33f, 0f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.001f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0.00946F, 0.19897F, -0.00675F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.23704F, 0.23704F, 0.23704F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0F, 0.16861F, -0.00066F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(0.11845F, 0.11845F, 0.00099F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0F, -0.35896F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(1.95921F, 1.95921F, 1.95921F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0F, -0.63299F, 0F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(1F, 1F, 0.001F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MechLowerArmR",
                    localPos = new Vector3(0F, 0.615F, 0F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "MechLowerArmR",
                    localPos = new Vector3(0.00001F, 0.55343F, -0.00001F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.15478F, 0.15478F, 0.00091F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.03556F, 2.89761F, -0.04224F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(4.43531F, 3.10209F, 4.43531F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0F, 2.3602F, 0.00009F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(2.25F, 2.25F, 0.001F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HandL",
                    localPos = new Vector3(-0.00187F, -0.09568F, 0.00006F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.13668F, 0.13668F, 0.13668F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "HandL",
                    localPos = new Vector3(-0.00002F, -0.07795F, 0.00004F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.14467F, 0.14467F, 0.00103F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0F, 0.1467F, 0F),
                    localAngles = new Vector3(0F, 270F, 0F),
                    localScale = new Vector3(0.22237F, 0.22237F, 0.22237F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.00001F, 0.11486F, -0.00001F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.14F, 0.14F, 0.001F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LeftArm4",
                    localPos = new Vector3(0F, 0.00893F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.00613F, 0.00613F, 0.00613F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LeftArm4",
                    localPos = new Vector3(0F, 0.00809F, 0F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.00589F, 0.00589F, 0.00002F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(-0.0049F, 0.33232F, -0.0126F),
                    localAngles = new Vector3(0F, 68.94624F, 0F),
                    localScale = new Vector3(0.37154F, 0.37154F, 0.37154F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.00001F, 0.28591F, 0F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.21645F, 0.21645F, 0.00155F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.09453F, 0.11459F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(0.055F, 0.055F, 0.055F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.09117F, 0.14851F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.03658F, 0.03658F, 0.00026F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.32933F, 0F),
                    localAngles = new Vector3(0F, 60F, 0F),
                    localScale = new Vector3(0.16408F, 0.11731F, 0.16408F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "HandR",
                    localPos = new Vector3(0.01093F, 0.06846F, -0.01247F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.07012F, 0.07012F, 0.0005F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ShotgunItems",
                    localPos = new Vector3(0.16889F, -0.03765F, 0.00576F),
                    localAngles = new Vector3(339.5094F, 0F, 90F),
                    localScale = new Vector3(0.2429F, 0.2429F, 0.2429F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "SSGBarrelItems",
                    localPos = new Vector3(0.00005F, 0.18278F, 0.05299F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.29718F, 0.29718F, 0.29718F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HMGItems",
                    localPos = new Vector3(0.58483F, -0.05182F, -0.00003F),
                    localAngles = new Vector3(330F, 0F, 90F),
                    localScale = new Vector3(0.22982F, 0.22982F, 0.22982F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HammerBase",
                    localPos = new Vector3(0.00006F, 1.625F, -0.00012F),
                    localAngles = new Vector3(0F, 270F, 0F),
                    localScale = new Vector3(0.48864F, 0.48864F, 0.48864F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName =   "NeedlerItems",
                    localPos =    new Vector3(-0.15368F, 0.11949F, -0.01714F),
                    localAngles = new Vector3(343.9558F, 0F, 90F),
                    localScale =  new Vector3(0.23553F, 0.24472F, 0.23553F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "ShotgunItems",
                    localPos = new Vector3(1.00937F, -0.04993F, 0.00758F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(0.10296F, 0.10296F, 0.00001F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "SSGBarrelItems",
                    localPos = new Vector3(-0.03745F, 1.07244F, 0.07575F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(0.08F, 0.08F, 0F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "SSGBarrelItems",
                    localPos = new Vector3(0.03198F, 1.0725F, 0.07575F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(0.08F, 0.08F, 0F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "HMGItems",
                    localPos = new Vector3(1.26296F, -0.05222F, 0.01241F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(0.07741F, 0.07741F, 0.00001F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "HammerBase",
                    localPos = new Vector3(0.00004F, 1.51978F, -0.00007F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(0.33855F, 0.33855F, 0.00003F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName =   "NeedlerItems",
                    localPos =    new Vector3(0.22771F, 0.07286F, -0.00003F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale =  new Vector3(0.26724F, 0.26724F, 0.00002F)
                },
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MinigunBarrel",
                    localPos = new Vector3(0F, 0.02617F, 0F),
                    localAngles = new Vector3(0F, 60F, 0F),
                    localScale = new Vector3(0.01335F, 0.01335F, 0.01335F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hammer",
                    localPos = new Vector3(0F, 0.00288F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.00754F, 0.00754F, 0.00754F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "Hammer",
                    localPos = new Vector3(0F, 0.01181F, 0.006F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.01191F, 0.01191F, 0.00001F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "MinigunMuzzle",
                    localPos = new Vector3(0F, 0F, 0.0041F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.00384F, 0.00384F, 0F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GrantOnKillEffectsOnHighDamage;
        }

        private void GrantOnKillEffectsOnHighDamage(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, GameObject victim)
        {
            var attacker = damageInfo.attacker;
            if (attacker)
            {
                var body = attacker.GetComponent<CharacterBody>();
                var victimBody = victim.GetComponent<CharacterBody>();
                if (body && victimBody)
                {
                    var InventoryCount = GetCount(body);
                    if (InventoryCount > 0)
                    {
                        if (damageInfo.damage / body.damage >= WitchesRingTriggerThreshold)
                        {
                            if (GlobalCooldownOnUse && !body.HasBuff(WitchesRingImmunityBuffDef) || !GlobalCooldownOnUse && !victimBody.HasBuff(WitchesRingImmunityBuffDef))
                            {
                                if (NetworkServer.active)
                                {
                                    if (!GlobalCooldownOnUse)
                                    {
                                        victimBody.AddTimedBuffAuthority(WitchesRingImmunityBuffDef.buffIndex, BaseCooldownDuration / (1 + AdditionalCooldownReduction * (InventoryCount - 1)));
                                    }
                                    else
                                    {
                                        body.AddTimedBuffAuthority(WitchesRingImmunityBuffDef.buffIndex, BaseCooldownDuration / (1 + AdditionalCooldownReduction * (InventoryCount - 1)));
                                    }
                                    DamageReport damageReport = new DamageReport(damageInfo, victimBody.healthComponent, damageInfo.damage, victimBody.healthComponent.combinedHealth);
                                    GlobalEventManager.instance.OnCharacterDeath(damageReport);
                                }
                            }
                        }
                    }
                }
            }
            orig(self, damageInfo, victim);
        }
    }
}