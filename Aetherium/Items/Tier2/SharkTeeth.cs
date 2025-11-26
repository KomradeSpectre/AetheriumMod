using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Compatability.ModCompatability;

using System.Runtime.CompilerServices;

namespace Aetherium.Items.Tier2
{
    public class SharkTeeth : ItemBase<SharkTeeth>
    {
        public static ConfigOption<float> BaseDamageSpreadPercentage;
        public static ConfigOption<float> AdditionalDamageSpreadPercentage;
        public static ConfigOption<float> MaxDamageSpreadPercentage;
        public static ConfigOption<float> DurationOfDamageSpread;
        public static ConfigOption<int> TicksOfDamageDuringDuration;
        public static ConfigOption<bool> SharkTeethIsNonLethal;

        public override string ItemName => "Shark Teeth";

        public override string ItemLangTokenName => "SHARK_TEETH";

        public override string ItemPickupDesc => "A portion of damage taken is distributed to you over <style=cIsUtility>5 seconds</style> as <style=cIsDamage>bleed damage</style>.";

        public override string ItemFullDescription => $"<style=cIsDamage>{FloatToPercentageString(BaseDamageSpreadPercentage)}</style> of damage taken <style=cStack>(+{FloatToPercentageString(AdditionalDamageSpreadPercentage)} per stack, hyperbolically)</style> is distributed to you over {DurationOfDamageSpread} second(s) as <style=cIsDamage>bleed damage</style>";

        public override string ItemLore => "A pair of what seems to be normal shark teeth. However, field testing has shown them to be capable of absorbing a portion of any kind of force applied to them, and redirecting it to be minor flesh wounds on their wielder.";

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("SharkTeeth.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("SharkTeethIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public GameObject BleedInflictor = new GameObject("Shark Teeth Damage");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            BaseDamageSpreadPercentage = config.ActiveBind<float>("Item: " + ItemName, "Base Damage Spread Percentage", 0.25f, "How much damage in percentage should be spread out over time?");
            AdditionalDamageSpreadPercentage = config.ActiveBind<float>("Item: " + ItemName, "Damage Spread Percentage Gained Per Stack (Diminishing)", 0.25f, "How much damage in percentage should be spread out over time with diminishing returns (hyperbolic scaling) on additional stacks?");
            MaxDamageSpreadPercentage = config.ActiveBind<float>("Item: " + ItemName, "Absolute Maximum Damage Spread Percentage", 0.75f, "What should our maximum percentage damage spread over time be?");
            DurationOfDamageSpread = config.ActiveBind<float>("Item: " + ItemName, "Damage Spread Duration", 2f, "How many seconds should the damage be spread out over?");
            TicksOfDamageDuringDuration = config.ActiveBind<int>("Item: " + ItemName, "Damage Spread Ticks (Segments)", 5, "How many ticks of damage during our duration (as in how divided is our damage)?");
            SharkTeethIsNonLethal = config.ActiveBind<bool>("Item: " + ItemName, "Shark Teeth Effect is Non-Lethal", false, "Should the Shark Teeth's bleed ticks be unable to kill you? If this is set to true, it will void any damage from segments that would kill you.");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.00778F, 0.31246F, -0.02331F),
                    localAngles = new Vector3(278.1251F, 62.21039F, 162.9523F),
                    localScale = new Vector3(0.14F, 0.14F, 0.11F)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(-0.00609F, 0.28318F, 0.06258F),
                    localAngles = new Vector3(294.4476F, 93.50946F, 168.4398F),
                    localScale = new Vector3(0.12F, 0.12F, 0.12F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.13037F, 2.35654F, -0.12139F),
                    localAngles = new Vector3(288.1113F, 105.642F, 157.9526F),
                    localScale = new Vector3(1F, 1.4F, 1F)
                }
            });

            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.0121F, 0.20365F, -0.01354F),
                    localAngles = new Vector3(282.0218F, 318.0565F, 133.7099F),
                    localScale = new Vector3(0.1819F, 0.16F, 0.13294F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(-0.00421F, 0.4105F, 0.04387F),
                    localAngles = new Vector3(286.7192F, 142.5261F, 126.8952F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.02316F, 0.33854F, -0.00004F),
                    localAngles = new Vector3(281.9041F, 125.1079F, 144.5135F),
                    localScale = new Vector3(0.15246F, 0.17F, 0.16F)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfBackL",
                    localPos = new Vector3(0.00008F, 0.55991F, -0.07756F),
                    localAngles = new Vector3(71.0061F, 1.02794F, 0.97197F),
                    localScale = new Vector3(0.13045F, 0.19932F, 0.19932F)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.01029F, 0.20659F, 0.02518F),
                    localAngles = new Vector3(286.7435F, 109.5904F, 150.4877F),
                    localScale = new Vector3(0.13885F, 0.13885F, 0.12149F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.09203F, 1.75747F, 0.29914F),
                    localAngles = new Vector3(304.1629F, 187.799F, 172.0363F),
                    localScale = new Vector3(0.88287F, 0.88287F, 0.88287F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(-0.02029F, 0.32377F, 0.00491F),
                    localAngles = new Vector3(291.4364F, 139.4888F, 132.5494F),
                    localScale = new Vector3(0.15F, 0.18F, 0.15F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(-0.0233F, 0.33094F, 0.01016F),
                    localAngles = new Vector3(302.1523F, 58.52777F, 202.3831F),
                    localScale = new Vector3(0.11F, 0.13F, 0.12F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LeftLeg",
                    localPos = new Vector3(0F, 0.01236F, 0.00006F),
                    localAngles = new Vector3(81.90417F, 0F, 0F),
                    localScale = new Vector3(0.00232F, 0.00232F, 0.00232F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.02789F, 0.3097F, 0.03044F),
                    localAngles = new Vector3(77.06367F, 8.05409F, 337.8984F),
                    localScale = new Vector3(0.13029F, 0.29853F, 0.16657F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HandR",
                    localPos = new Vector3(-0.02619F, 0.05078F, 0.03219F),
                    localAngles = new Vector3(6.32559F, 147.086F, 275.4325F),
                    localScale = new Vector3(0.02207F, 0.02207F, 0.02207F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0.00076F, 0.27952F, -0.01956F),
                    localAngles = new Vector3(63.571F, 2.0089F, 3.43756F),
                    localScale = new Vector3(0.06306F, 0.06306F, 0.06306F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0F, 0.30428F, -0.01296F),
                    localAngles = new Vector3(71.25261F, 313.1178F, 314.6782F),
                    localScale = new Vector3(0.18079F, 0.26736F, 0.18079F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "KneeL",
                    localPos = new Vector3(0F, 0.00768F, 0.00007F),
                    localAngles = new Vector3(88.39465F, 182.8047F, 180F),
                    localScale = new Vector3(0.00332F, 0.00416F, 0.00307F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += TakeDamage;
            On.RoR2.CharacterBody.FixedUpdate += TickDamage;
            RoR2Application.onLoad += OnLoadModCompat;
        }

        private void OnLoadModCompat()
        {
        }

        private void TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            if (!damageInfo.rejected || damageInfo == null)
            {
                var bleedComponent = self.GetComponent<BleedTrackerComponent>();
                if (!bleedComponent) { bleedComponent = self.gameObject.AddComponent<BleedTrackerComponent>(); }
                var inventoryCount = GetCount(self.body);
                //var healthBefore = self.health; //debug
                if (damageInfo.inflictor != SharkTeeth.instance.BleedInflictor && inventoryCount > 0)
                {
                    //Chat.AddMessage($"Damage Before: {damageInfo.damage}"); //debug
                    var percentage = BaseDamageSpreadPercentage + (MaxDamageSpreadPercentage - MaxDamageSpreadPercentage / (1 + AdditionalDamageSpreadPercentage * (inventoryCount - 1)));
                    var damage = damageInfo.damage * percentage;
                    var time = DurationOfDamageSpread;
                    var segments = TicksOfDamageDuringDuration;
                    damageInfo.damage -= damage;
                    bleedComponent.BleedStacks.Add(new BleedStack(time / segments, damage / segments, segments, damageInfo.attacker, damageInfo.damageType));
                }
            }

            orig(self, damageInfo);
            //if (inventoryCount > 0) {Chat.AddMessage($"Actual Damage: {healthBefore - self.health}");} //debug
        }

        private void TickDamage(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            orig(self);

            var bleedComponent = self.GetComponent<BleedTrackerComponent>();
            if (!bleedComponent) { bleedComponent = self.gameObject.AddComponent<BleedTrackerComponent>(); }

            foreach (BleedStack stack in bleedComponent.BleedStacks)
            {
                stack.FixedUpdate(self);
            }
            bleedComponent.BleedStacks.RemoveAll(x => x.TicksLeft <= 0);
        }

        public class BleedTrackerComponent : MonoBehaviour
        {
            public List<BleedStack> BleedStacks = new List<BleedStack>();
        }

        public class BleedStack
        {
            public float TimeLeft;
            public float StashedTimeLeft;
            public float DamagePerTick;
            public float TicksLeft;
            public GameObject Attacker;
            public float DamageDealt;
            public DamageType DamageType;

            public BleedStack(float timeLeft, float damagePerTick, float ticksLeft, GameObject attacker, DamageType damageType)
            {
                TimeLeft = timeLeft;
                StashedTimeLeft = timeLeft;
                DamagePerTick = damagePerTick;
                TicksLeft = ticksLeft;
                Attacker = attacker;
                DamageType = damageType;
            }

            public void FixedUpdate(RoR2.CharacterBody characterBody)
            {
                TimeLeft -= Time.fixedDeltaTime;

                if (TimeLeft <= 0)
                {
                    TimeLeft += StashedTimeLeft;
                    TicksLeft -= 1;

                    DamageDealt += DamagePerTick;

                    if (DamageDealt >= 1)
                    {
                        DamageInfo damageInfo = new DamageInfo
                        {
                            attacker = Attacker,
                            crit = false,
                            damage = DamageDealt,
                            force = Vector3.zero,
                            inflictor = SharkTeeth.instance.BleedInflictor,
                            position = characterBody.corePosition,
                            procCoefficient = 0f,
                            damageColorIndex = DamageColorIndex.Bleed,
                            damageType = DamageType
                        };
                        //var healthBefore = characterBody.healthComponent.health; //debug
                        if (!SharkTeethIsNonLethal || SharkTeethIsNonLethal && characterBody.healthComponent.health > damageInfo.damage)
                        {
                            characterBody.healthComponent.TakeDamage(damageInfo);
                        }
                        DamageDealt = 0;
                        //Chat.AddMessage($"Actual Tick Damage: {healthBefore - characterBody.healthComponent.health}"); //debug
                    }
                }
            }
        }
    }
}