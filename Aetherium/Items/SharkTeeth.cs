using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using ItemStats;
using ItemStats.Stat;
using ItemStats.ValueFormatters;

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Compatability.ModCompatability.ItemStatsModCompat;

using System.Runtime.CompilerServices;

namespace Aetherium.Items
{
    public class SharkTeeth : ItemBase<SharkTeeth>
    {
        public static ConfigOption<bool> UseNewIcons;
        //public static bool UseNewIcons;
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
        public override Sprite ItemIcon => UseNewIcons ? MainAssets.LoadAsset<Sprite>("SharkTeethIconAlt.png") : MainAssets.LoadAsset<Sprite>("SharkTeethIcon.png");

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
            UseNewIcons = config.ActiveBind<bool>("Item: " + ItemName, "Use Alternative Icon Art?", true, "If set to true, will use the new icon art drawn by WaltzingPhantom, else it will use the old icon art.");
            BaseDamageSpreadPercentage = config.ActiveBind<float>("Item: " + ItemName, "Base Damage Spread Percentage", 0.25f, "How much damage in percentage should be spread out over time?");
            AdditionalDamageSpreadPercentage = config.ActiveBind<float>("Item: " + ItemName, "Damage Spread Percentage Gained Per Stack (Diminishing)", 0.25f, "How much damage in percentage should be spread out over time with diminishing returns (hyperbolic scaling) on additional stacks?");
            MaxDamageSpreadPercentage = config.ActiveBind<float>("Item: " + ItemName, "Absolute Maximum Damage Spread Percentage", 0.75f, "What should our maximum percentage damage spread over time be?");
            DurationOfDamageSpread = config.ActiveBind<float>("Item: " + ItemName, "Damage Spread Duration", 5f, "How many seconds should the damage be spread out over?");
            TicksOfDamageDuringDuration = config.ActiveBind<int>("Item: " + ItemName, "Damage Spread Ticks (Segments)", 5, "How many ticks of damage during our duration (as in how divided is our damage)?");
            SharkTeethIsNonLethal = config.ActiveBind<bool>("Item: " + ItemName, "Shark Teeth Effect is Non-Lethal", false, "Should the Shark Teeth's bleed ticks be unable to kill you? If this is set to true, it will void any damage from segments that would kill you.");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab, true);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(-0.07953F, 0.32931F, -0.11062F),
                    localAngles = new Vector3(-0.00002F, 315F, 262.2341F),
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
                    localPos = new Vector3(-0.10813F, 0.32903F, 0.04613F),
                    localAngles = new Vector3(350.2299F, 355.2263F, 245.7051F),
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
                    localPos = new Vector3(-1.18736F, 2.24384F, -0.4462F),
                    localAngles = new Vector3(338.5363F, 165.216F, 83.74396F),
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
                    localPos = new Vector3(0.12838F, 0.22183F, -0.02369F),
                    localAngles = new Vector3(3.12242F, 180.6776F, 261.7126F),
                    localScale = new Vector3(0.13F, 0.16F, 0.17F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(-0.07793F, 0.42336F, 0.04161F),
                    localAngles = new Vector3(350.0682F, -0.00001F, 259.9016F),
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
                    localPos = new Vector3(-0.14659F, 0.32297F, 0.009F),
                    localAngles = new Vector3(0.02192F, 359.0269F, 260.3308F),
                    localScale = new Vector3(0.14F, 0.17F, 0.16F)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfBackL",
                    localPos = new Vector3(0.00007F, 0.50545F, -0.23588F),
                    localAngles = new Vector3(0F, 90F, 71.00894F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.14673F, 0.16713F, 0.02241F),
                    localAngles = new Vector3(3.4232F, 348.1147F, 255.454F),
                    localScale = new Vector3(0.16F, 0.16F, 0.14F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.09202F, 1.95922F, 0.87672F),
                    localAngles = new Vector3(0F, 90F, 250.1263F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(-0.1528F, 0.35756F, 0.00491F),
                    localAngles = new Vector3(354.5349F, 1.39827F, 255.625F),
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
                    localPos = new Vector3(-0.13255F, 0.39298F, -0.00043F),
                    localAngles = new Vector3(358.2062F, 355.4815F, 240.507F),
                    localScale = new Vector3(0.11F, 0.13F, 0.12F)
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

            On.RoR2.HealthComponent.TakeDamage += TakeDamage;
            On.RoR2.CharacterBody.FixedUpdate += TickDamage;
        }

        private void ItemStatsModCompat()
        {
            CreateSharkTeethStatDef();
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