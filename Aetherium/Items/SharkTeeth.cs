using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;

namespace Aetherium.Items
{
    public class SharkTeeth : ItemBase<SharkTeeth>
    {
        public ConfigOption<bool> UseNewIcons;
        //public static bool UseNewIcons;
        public static ConfigOption<float> BaseDamageSpreadPercentage;
        public static ConfigOption<float> AdditionalDamageSpreadPercentage;
        public static ConfigOption<float> MaxDamageSpreadPercentage;
        public static ConfigOption<float> DurationOfDamageSpread;
        public static ConfigOption<int> TicksOfDamageDuringDuration;

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
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(-0.05f, 0.45f, 0f),
                    localAngles = new Vector3(0, 0, -25),
                    localScale = new Vector3(0.4f, 0.4f, 0.3f)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0.45f, 0f),
                    localAngles = new Vector3(-30, 0, -45),
                    localScale = new Vector3(0.4f, 0.4f, 0.3f)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 2.5f, 0f),
                    localAngles = new Vector3(0, 90, -45),
                    localScale = new Vector3(3, 4, 3)
                }
            });

            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0.4f, 0f),
                    localAngles = new Vector3(0, 0, -45),
                    localScale = new Vector3(0.6f, 0.6f, 0.4f)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0.5f, 0.05f),
                    localAngles = new Vector3(0, 0, -45),
                    localScale = new Vector3(0.4f, 0.4f, 0.3f)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0.5f, 0f),
                    localAngles = new Vector3(0, 0, -45),
                    localScale = new Vector3(0.5f, 0.5f, 0.5f)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfBackL",
                    localPos = new Vector3(0f, 0.7f, 0f),
                    localAngles = new Vector3(0, 0, -45),
                    localScale = new Vector3(0.8f, 0.8f, 0.55f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0.7f, 0f),
                    localAngles = new Vector3(0, 90, -55),
                    localScale = new Vector3(0.6f, 0.6f, 0.3f)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 2f, 0f),
                    localAngles = new Vector3(0, 0, -45),
                    localScale = new Vector3(6, 6, 4)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0.5f, 0f),
                    localAngles = new Vector3(0, 0, -45),
                    localScale = new Vector3(0.7f, 0.7f, 0.4f)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += TakeDamage;
            On.RoR2.CharacterBody.FixedUpdate += TickDamage;
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
                        characterBody.healthComponent.TakeDamage(damageInfo);
                        DamageDealt = 0;
                        //Chat.AddMessage($"Actual Tick Damage: {healthBefore - characterBody.healthComponent.health}"); //debug
                    }
                }
            }
        }
    }
}