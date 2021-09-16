using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;

namespace Aetherium.Equipment.EliteEquipment
{
    public class AffixAbyssal : EliteEquipmentBase<AffixAbyssal>
    {
        public override string EliteEquipmentName => "Her Cruelty";

        public override string EliteAffixToken => "AFFIX_ABYSSAL";

        public override string EliteEquipmentPickupDesc => "Become an aspect of the red plane.";

        public override string EliteEquipmentFullDescription => "";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Abyssal";

        public override GameObject EliteEquipmentModel => new GameObject();
        public override Material EliteMaterial { get; set; } = MainAssets.LoadAsset<Material>("AffixAbyssalMat");

        public override Sprite EliteEquipmentIcon => null;

        public override Sprite EliteBuffIcon => null;

        public static GameObject ItemBodyModelPrefab;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            CreateElite();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {

            ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("AffixAbyssalDisplay");
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);


            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-0, -0, -2),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.25f, 0.25f, 0.25f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0F, 0.21926F, -1.33465F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.08787F, 0.07478F, 1.04472F),
                    localAngles = new Vector3(354.9749F, 182.8028F, 237.0256F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.20102F, 0.09445F, 0.16025F),
                    localAngles = new Vector3(15.50638F, 144.8099F, 180.4037F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.17241F, -0.0089F, 0.02642F),
                    localAngles = new Vector3(5.28933F, 111.5028F, 190.532F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.16832F, 0.04282F, 0.06368F),
                    localAngles = new Vector3(355.8307F, 42.81982F, 185.1587F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(-0.6845F, -0.60707F, -0.05308F),
                    localAngles = new Vector3(349.4037F, 73.89225F, 346.442F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.2442F, 0.04122F, 0.01506F),
                    localAngles = new Vector3(22.73106F, 289.1799F, 159.5365F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hip",
                    localPos = new Vector3(-2.2536F, 1.10779F, 0.45293F),
                    localAngles = new Vector3(1.77184F, 278.9485F, 190.4101F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.21004F, -0.09095F, -0.09165F),
                    localAngles = new Vector3(0F, 60.43688F, 180F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.17925F, -0.02363F, -0.11047F),
                    localAngles = new Vector3(359.353F, 299.9855F, 169.6378F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnEquipmentGained += GiveAbyssalController;
            On.RoR2.CharacterBody.OnEquipmentLost += RemoveAbyssalController;
            On.RoR2.GlobalEventManager.OnHitEnemy += BleedOnHit;
        }

        private void BleedOnHit(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.attacker && !damageInfo.rejected)
            {
                var body = damageInfo.attacker.GetComponent<CharacterBody>();
                if (body && body.inventory && (body.inventory.currentEquipmentIndex == EliteEquipmentDef.equipmentIndex || body.inventory.alternateEquipmentIndex == EliteEquipmentDef.equipmentIndex))
                {
                    if(damageInfo.damageType != DamageType.BleedOnHit)
                    {
                        damageInfo.damageType |= DamageType.BleedOnHit;
                    }

                }
            }
            orig(self, damageInfo, victim);
        }

        private void GiveAbyssalController(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);
            if(equipmentDef == EliteEquipmentDef && !self.isPlayerControlled)
            {
                var abyssalController = self.GetComponent<AbyssalController>();
                if (!abyssalController)
                {
                    abyssalController = self.gameObject.AddComponent<AbyssalController>();
                }
            }
        }

        private void RemoveAbyssalController(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EliteEquipmentDef)
            {
                var abyssalController = self.GetComponent<AbyssalController>();
                if (abyssalController)
                {
                    UnityEngine.Object.Destroy(abyssalController);
                }
            }
            orig(self, equipmentDef);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }

        public class AbyssalController : MonoBehaviour
        {
            public EntityStateMachine BodyStateMachine;
            public float Stopwatch;
            public float TimeBetweenJumps = 2;

            public void Start()
            {
                BodyStateMachine = GetComponents<EntityStateMachine>().Where(x => x.customName == "Body").FirstOrDefault();
            }

            public void FixedUpdate()
            {
                Stopwatch += Time.fixedDeltaTime;
                if(Stopwatch >= TimeBetweenJumps)
                {
                    if (BodyStateMachine)
                    {
                        BodyStateMachine.SetInterruptState(new EntityStates.ImpMonster.BlinkState(), EntityStates.InterruptPriority.Any);
                    }
                    Stopwatch = 0;
                }
            }
        }
    }
}
