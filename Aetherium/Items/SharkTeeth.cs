using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using System;
using KomradeSpectre.Aetherium;

namespace Aetherium.Items
{
    class SharkTeeth : Item<SharkTeeth>
    {
        public override string displayName => "Shark Teeth";

        public override ItemTier itemTier => RoR2.ItemTier.Tier2;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "A portion of damage taken is distributed to you over <style=cIsUtiltiy>5 seconds</style> as <style=cIsDamage>bleed damage</style>.";

        protected override string NewLangDesc(string langid = null) => "<style=cIsDamage>25%</style> of damage taken <style=cStack>(+25% per stack, hyperbolically)</style> is distributed to you over 5 seconds as <style=cIsDamage>bleed damage</style>";

        protected override string NewLangLore(string langID = null) => "A pair of what seems to be normal shark teeth. However, field testing has shown them to be capable of absorbing a portion of any kind of force applied to them, and redirecting it to be a simple flesh wound on their wielder.";

        private static List<RoR2.CharacterBody> Playername = new List<RoR2.CharacterBody>();
        public static GameObject ItemBodyModelPrefab;

        public GameObject BleedInflictor = new GameObject("Shark Teeth Damage");

        public SharkTeeth()
        {
            modelPathName = "@Aetherium:Assets/Models/Prefabs/SharkTeeth.prefab";
            iconPathName = "@Aetherium:Assets/Textures/Icons/SharkTeethIcon.png";

        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = AetheriumPlugin.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.3f, 0.3f, 0.3f);
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

        protected override void LoadBehavior()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = regDef.pickupModelPrefab;
                regItem.ItemDisplayRules = GenerateItemDisplayRules();

            }
            On.RoR2.HealthComponent.TakeDamage += TakeDamage;
            On.RoR2.CharacterBody.FixedUpdate += TickDamage;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.HealthComponent.TakeDamage -= TakeDamage;
            On.RoR2.CharacterBody.FixedUpdate -= TickDamage;
        }

        private void TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            var bleedComponent = self.GetComponent<BleedTrackerComponent>();
            if (!bleedComponent) { bleedComponent = self.gameObject.AddComponent<BleedTrackerComponent>(); }
            var inventoryCount = GetCount(self.body.inventory);
            //var healthBefore = self.health; //debug
            if (damageInfo.inflictor != SharkTeeth.instance.BleedInflictor && inventoryCount > 0)
            {
                //Chat.AddMessage($"Damage Before: {damageInfo.damage}"); //debug
                var percentage = 0.25f + (0.75f - 0.75f / (1 + 0.25f * (inventoryCount - 1)));
                var damage = damageInfo.damage * percentage;
                var time = 5f;
                var segments = 10;
                damageInfo.damage -= damage;
                bleedComponent.BleedStacks.Add(new BleedStack(time / segments, damage / segments, segments, damageInfo.attacker, damageInfo.damageType));
            }

            orig(self, damageInfo);
            //if (inventoryCount > 0) {Chat.AddMessage($"Actual Damage: {healthBefore - self.health}");} //debug
        }

        private void TickDamage(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            orig(self);

            var bleedComponent = self.GetComponent<BleedTrackerComponent>();
            if (!bleedComponent) { bleedComponent = self.gameObject.AddComponent<BleedTrackerComponent>(); }

            foreach(BleedStack stack in bleedComponent.BleedStacks)
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
            public float DamagePerTick;
            public float TicksLeft;
            public GameObject Attacker;
            public float DamageDealt;
            public DamageType DamageType;

            public BleedStack(float timeLeft, float damagePerTick, float ticksLeft, GameObject attacker, DamageType damageType)
            {
                TimeLeft = timeLeft;
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
                    TimeLeft += 0.25f;
                    TicksLeft -= 1;

                    DamageDealt += DamagePerTick;

                    if (DamageDealt >= 1)
                    {
                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.attacker = Attacker;
                        damageInfo.crit = false;
                        damageInfo.damage = DamageDealt;
                        damageInfo.force = Vector3.zero;
                        damageInfo.inflictor = SharkTeeth.instance.BleedInflictor;
                        damageInfo.position = characterBody.corePosition;
                        damageInfo.procCoefficient = 0f;
                        damageInfo.damageColorIndex = DamageColorIndex.Bleed;
                        damageInfo.damageType = DamageType;
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
