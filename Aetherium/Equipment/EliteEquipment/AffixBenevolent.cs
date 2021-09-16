using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using RoR2.Orbs;
using Aetherium.Utils;

namespace Aetherium.Equipment.EliteEquipment
{
    public class AffixBenevolent : EliteEquipmentBase<AffixBenevolent>
    {
        public override string EliteEquipmentName => "Her Sacrifice";

        public override string EliteAffixToken => "AFFIX_BENEVOLENT";

        public override string EliteEquipmentPickupDesc => "Become an aspect of self-sacrifice.";

        public override string EliteEquipmentFullDescription => throw new NotImplementedException();

        public override string EliteEquipmentLore => throw new NotImplementedException();

        public override string EliteModifier => "Benevolent";

        public override GameObject EliteEquipmentModel => new GameObject();

        public override Sprite EliteEquipmentIcon => null;

        public override Sprite EliteBuffIcon => null;

        public static GameObject ItemBodyModelPrefab;
        public static GameObject ItemFollowerPrefab;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {

        }

        private void CreateEliteTiers()
        {
            CanAppearInEliteTiers = new CombatDirector.EliteTierDef[]
            {
                new CombatDirector.EliteTierDef()
                {
                    costMultiplier = CombatDirector.baseEliteCostMultiplier,
                    damageBoostCoefficient = CombatDirector.baseEliteDamageBoostCoefficient,
                    healthBoostCoefficient = CombatDirector.baseEliteHealthBoostCoefficient * 8,
                    eliteTypes = Array.Empty<EliteDef>(),
                    isAvailable = SetAvailability
                }
            };
        }

        private bool SetAvailability(SpawnCard.EliteRules arg)
        {
            return Run.instance.loopClearCount > 0 && arg == SpawnCard.EliteRules.Default;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {

            ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("BenevolentDisplay");
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
                    localPos = new Vector3(-0, -0, 2),
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
            On.RoR2.CharacterBody.OnEquipmentGained += PlaceManagerComponentOnElite;
            On.RoR2.CharacterBody.OnEquipmentLost += RemoveManagerComponentOnElite;
        }

        private void RemoveManagerComponentOnElite(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EliteEquipmentDef)
            {
                var benevolenceManager = self.GetComponent<BenevolenceManager>();
                if (benevolenceManager)
                {
                    UnityEngine.Object.Destroy(benevolenceManager);
                }
            }
            orig(self, equipmentDef);
        }

        private void PlaceManagerComponentOnElite(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);
            if(equipmentDef == EliteEquipmentDef)
            {
                var benevolenceManager = self.GetComponent<BenevolenceManager>();
                if (!benevolenceManager)
                {
                    benevolenceManager = self.gameObject.AddComponent<BenevolenceManager>();
                    benevolenceManager.Body = self;
                }
            }
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }

        public class BenevolenceManager : MonoBehaviour
        {
            public CharacterBody Body;
            public float Timer;
            public float TimeBetweenPulses = 1;

            public void FixedUpdate()
            {
                if (!Body)
                {
                    Destroy(this);
                }

                Timer -= Time.fixedDeltaTime;
                if(Timer <= 0)
                {
                    FireDebuffStealingPulse();
                    Timer = TimeBetweenPulses;
                }
            }

            public void FireDebuffStealingPulse()
            {
                RoR2.HurtBox[] hurtBoxes = new RoR2.SphereSearch
                {
                    radius = 60,
                    mask = RoR2.LayerIndex.entityPrecise.mask,
                    origin = Body.corePosition
                }.RefreshCandidates().OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                foreach(HurtBox hurtBox in hurtBoxes)
                {
                    if(hurtBox.healthComponent && hurtBox.healthComponent.body)
                    {
                        var body = hurtBox.healthComponent.body;
                        if(body.teamComponent.teamIndex == Body.teamComponent.teamIndex && !body.HasBuff(AffixBenevolent.instance.EliteBuffDef) && body != Body)
                        {
                            bool removedADebuff = false;
                            List<Tuple<BuffDef, float>> TimedBuffsToAdd = new List<Tuple<BuffDef, float>>();
                            foreach(CharacterBody.TimedBuff timedBuff in body.timedBuffs)
                            {
                                var buffDef = BuffCatalog.GetBuffDef(timedBuff.buffIndex);
                                if (buffDef.isDebuff)
                                {
                                    TimedBuffsToAdd.Add(Tuple.Create(buffDef, timedBuff.timer));
                                }
                            }

                            foreach(Tuple<BuffDef, float> timedBuff in TimedBuffsToAdd)
                            {
                                var buffCount = body.GetBuffCount(timedBuff.Item1);
                                body.ClearTimedBuffs(timedBuff.Item1);
                                removedADebuff = true;
                                Body.AddTimedBuff(timedBuff.Item1, timedBuff.Item2);
                            }


                            foreach(DotController dotController in DotController.instancesList)
                            {
                                if(dotController.victimBody == body)
                                {
                                    dotController.victimObject = Body.gameObject;
                                    removedADebuff = true;
                                }
                            }

                            if (removedADebuff)
                            {
                                HealOrb healOrb = new HealOrb();
                                healOrb.origin = body.transform.position;
                                healOrb.target = Body.mainHurtBox;
                                healOrb.healValue = 0;
                                healOrb.overrideDuration = 0.3f;
                                OrbManager.instance.AddOrb(healOrb);
                            }
                        }
                    }
                }
            }
        }
    }
}
