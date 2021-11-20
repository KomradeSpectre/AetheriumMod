using static Aetherium.AetheriumPlugin;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Aetherium.Utils;
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Utils.ItemHelpers;
using System.Linq;
using UnityEngine.Networking;

namespace Aetherium.Items
{
    public class ZenithAccelerator : ItemBase<ZenithAccelerator>
    {
        public ConfigOption<float> ExponentialBaseMultiplier;
        public ConfigOption<float> PercentageIncreasePerHit;
        public ConfigOption<float> BaseMaximumPercentageCap;
        public ConfigOption<float> AdditionalMaximumPercentageCap;
        public ConfigOption<float> DurationOfTimedBuffPerStack;

        public override string ItemName => "Zenith Accelerator";

        public override string ItemLangTokenName => "ZENITH_ACCELERATOR";

        public override string ItemPickupDesc => $"Hits increase your attack speed... <style=cDeath>BUT it's reduced by {FloatToPercentageString(ExponentialBaseMultiplier)} initially.</style>";

        public override string ItemFullDescription => $"<style=cIsDamage>Hitting enemies</style> grants you a <style=cIsDamage>temporary attack speed buff</style> that increases attack speed <style=cIsDamage>{FloatToPercentageString(PercentageIncreasePerHit)}</style> per buff stack linearly up to <style=cIsDamage>+{FloatToPercentageString(BaseMaximumPercentageCap)}</style> <style=cStack>(+{FloatToPercentageString(AdditionalMaximumPercentageCap)} per item stack linearly)</style>, " +
            $"but you lose up to <style=cDeath>{FloatToPercentageString(ExponentialBaseMultiplier)} attack speed</style> <style=cStack>(+{FloatToPercentageString(ExponentialBaseMultiplier)} per item stack exponentially)</style> the closer you are to <style=cDeath>0</style> stacks of the buff.";

        public override string ItemLore => "The others are not the masters of this universe, brother. They create clocks to count the time, we create clocks to create our own time. Watch.\n\n" +
            "We create an axle for stability, power, and to release the energies trapped within. A rounded center is key to its purpose.\n\n" +
            "The exhausts send the energies spiraling out. This, we harness with a simple bit of stone. We require less of the essence if we create smaller portions here, and here.\n\n" +
            "Now it is complete. The hotter it burns, the more time we have created. This time allows us a quickening.\n\n" +
            "I do not need to remind you of the importance of the quickening. Remember brother.\n\n" +
            "Fuel is Blood.\n\n" +
            "Speed is War.";

        public override ItemTier Tier => ItemTier.Lunar;

        public override bool AIBlacklisted => true;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("ZenithAccelerator.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("ZenithAcceleratorIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public static BuffDef ZenithAccelerationBuff;

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
            ExponentialBaseMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Exponential Base Multiplier", 0.5f, "How far down should we reduce attack speed by at 0 stacks of the Zenith Acceleration Buff? (0.5f results in reducing attack speed by half, an additional pickup of the item reduces it twice as much as the first.)");
            PercentageIncreasePerHit = config.ActiveBind<float>("Item: " + ItemName, "Percentage Increase per Hit", 0.1f, "How much bonus in percentage towards our cap should we get per hit?");
            BaseMaximumPercentageCap = config.ActiveBind<float>("Item: " + ItemName, "Base Percentage Max of Zenith Acceleration Attack Speed Bonus", 3f, "What should be our base cap for the attack speed bonus granted to us at full Zenith Acceleration stacks?");
            AdditionalMaximumPercentageCap = config.ActiveBind<float>("Item: " + ItemName, "Additional Percentage Cap per Additional Stack", 1f, "How much higher should our base percentage cap go with additional Zenith Accelerator stacks?");
            DurationOfTimedBuffPerStack = config.ActiveBind<float>("Item: " + ItemName, "Duration of Zenith Acceleration Stack", 3f, "How long should the base Zenith Acceleration stack be?");
        }

        private void CreateBuff()
        {
            ZenithAccelerationBuff = ScriptableObject.CreateInstance<BuffDef>();
            ZenithAccelerationBuff.name = "Aetherium: Zenith Acceleration";
            ZenithAccelerationBuff.buffColor = new Color(195, 61, 100, 255);
            ZenithAccelerationBuff.canStack = true;
            ZenithAccelerationBuff.isDebuff = false;
            ZenithAccelerationBuff.iconSprite = MainAssets.LoadAsset<Sprite>("ZenithAccelerationBuffIcon.png");

            BuffAPI.Add(new CustomBuff(ZenithAccelerationBuff));
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);
            ItemBodyModelPrefab.AddComponent<ZenithAcceleratorManager>();

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00195F, 0.19555F, -0.29803F),
                    localAngles = new Vector3(9.01745F, 0.00455F, 0.05957F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00009F, 0.10958F, -0.21047F),
                    localAngles = new Vector3(9.01745F, 0.00455F, 0.05957F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00047F, 1.88769F, -2.93752F),
                    localAngles = new Vector3(9.01745F, 0.00455F, 0.05957F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00201F, 0.14603F, -0.40206F),
                    localAngles = new Vector3(9.01745F, 0.00455F, 0.05957F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00206F, 0.08224F, -0.43672F),
                    localAngles = new Vector3(13.96509F, 0.00984F, 0.06063F),
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
                    localPos = new Vector3(0.00194F, 0.20398F, -0.34966F),
                    localAngles = new Vector3(18.7297F, 0.01516F, 0.06212F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "WeaponPlatform",
                    localPos = new Vector3(0.01724F, -0.31024F, 0.66479F),
                    localAngles = new Vector3(325.6529F, 87.44426F, 98.67111F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.0026F, 0.22369F, -0.39605F),
                    localAngles = new Vector3(6.51947F, 0.00243F, 0.04324F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.33546F, 4.95571F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.2863F, -0.29111F),
                    localAngles = new Vector3(22.03487F, 0F, 0F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00013F, 0.16106F, -0.25889F),
                    localAngles = new Vector3(9.01745F, 0.00455F, 0.05957F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Body",
                    localPos = new Vector3(0F, 0.01034F, -0.01241F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.00753F, 0.00753F, 0.00753F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00556F, 0.18566F, -0.41187F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.13048F, 0.13048F, 0.13048F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00452F, -0.09225F, -0.0105F),
                    localAngles = new Vector3(18.17866F, 359.5338F, 359.6518F),
                    localScale = new Vector3(0.09489F, 0.09489F, 0.09489F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.10674F, 0F),
                    localAngles = new Vector3(33.57753F, 0F, 0F),
                    localScale = new Vector3(0.01786F, 0.01786F, 0.01786F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.25555F, -0.27727F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.09293F, 0.09293F, 0.09293F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.38683F, -0.01213F, -0.01357F),
                    localAngles = new Vector3(0F, 30F, 0F),
                    localScale = new Vector3(0.14554F, 0.14554F, 0.14554F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Model",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            //R2API.RecalculateStatsAPI.GetStatCoefficients += CalculateAttackSpeedStat;
            On.RoR2.CharacterBody.RecalculateStats += AddAttackSpeedMult;
            On.RoR2.GlobalEventManager.OnHitEnemy += AddAttackSpeedBuff;
        }

        private void AddAttackSpeedMult(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            var inventoryCount = GetCount(self);
            if (inventoryCount > 0)
            {
                var speedMult = CalculateSpeedMult(self);
                self.attackSpeed *= speedMult;
            }
        }

        public int CalculateMaxBuffStacks(CharacterBody characterBody)
        {
            var inventoryCount = characterBody.inventory.GetItemCount(instance.ItemDef);
            var baseAmount = Math.Pow(ExponentialBaseMultiplier, inventoryCount);
            var increase = PercentageIncreasePerHit;
            var max = BaseMaximumPercentageCap + (inventoryCount - 1) * AdditionalMaximumPercentageCap;
            int maxBuffCount = (int)Math.Ceiling((max - baseAmount) / increase);

            return maxBuffCount;
        }

        public float CalculateSpeedMult(CharacterBody characterBody)
        {
            var buffCount = characterBody.GetBuffCount(ZenithAccelerationBuff);
            var inventoryCount = characterBody.inventory.GetItemCount(instance.ItemDef);
            var Base = Mathf.Pow(ExponentialBaseMultiplier, inventoryCount);
            var Increase = PercentageIncreasePerHit;
            var Max = BaseMaximumPercentageCap + (inventoryCount - 1) * AdditionalMaximumPercentageCap;
            var SpeedMult = Mathf.Min(Base + Increase * buffCount, Max);

            return SpeedMult;
        }

        /*public void CalculateAttackSpeedStat(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var inventoryCount = GetCount(sender);
            if (inventoryCount > 0)
            {
                var speedMult = CalculateSpeedMult(sender);
                args.attackSpeedMultAdd += speedMult - 1.0f;            
            }
        }*/

        private void AddAttackSpeedBuff(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            var attacker = damageInfo.attacker;
            if (attacker)
            {
                var body = attacker.GetComponent<CharacterBody>();
                var victimBody = victim.GetComponent<CharacterBody>();
                if(body && victimBody)
                {
                    var inventoryCount = GetCount(body);
                    if(inventoryCount > 0)
                    {
                        var buffCount = body.GetBuffCount(ZenithAccelerationBuff);
                        var maxBuffCount = CalculateMaxBuffStacks(body);

                        RefreshTimedBuffs(body, ZenithAccelerationBuff, 1 + (0.5f * (inventoryCount - 1)), 0.25f / inventoryCount);

                        if (buffCount < maxBuffCount)
                        {
                            body.AddTimedBuff(ZenithAccelerationBuff, DurationOfTimedBuffPerStack);
                        }
                    }
                }
            }
            orig(self, damageInfo, victim);
        }

        public class ZenithAcceleratorManager : MonoBehaviour
        {
            public RoR2.ItemDisplay ItemDisplay;
            public Animator Animator;
            public ParticleSystem[] ParticleSystem;

            public List<ZenithFirePlumeController> Plumes = new List<ZenithFirePlumeController>();
            public List<ZenithGlowController> GlowControllers = new List<ZenithGlowController>();

            public RoR2.CharacterMaster OwnerMaster;
            public RoR2.CharacterBody OwnerBody;

            public float CalculateWindUp()
            {
                var buffCount = OwnerBody.GetBuffCount(ZenithAccelerationBuff);
                var inventoryCount = OwnerBody.inventory.GetItemCount(ZenithAccelerator.instance.ItemDef);
                var Base = Mathf.Pow(0.5f, inventoryCount);
                var Increase = 0.10f;
                var Max = 3.0f + (inventoryCount - 1) * 1.0f;
                
                return Mathf.Min(Increase * buffCount / (Max - Base), 1);

            }

            public void OnDestroy()
            {
                foreach (ParticleSystem particleSystems in ParticleSystem)
                {
                    UnityEngine.Object.Destroy(particleSystems);
                }

                foreach (ZenithFirePlumeController zenithFirePlumeController in Plumes)
                {
                    UnityEngine.Object.Destroy(zenithFirePlumeController.Plume);
                }
            }

            public void FixedUpdate()
            {

                if (!OwnerMaster || !ItemDisplay || !ParticleSystem.Any() || !Plumes.Any() || !Animator || !GlowControllers.Any())
                {
                    ItemDisplay = this.GetComponentInParent<RoR2.ItemDisplay>();
                    if (ItemDisplay)
                    {
                        Animator = GetComponentInChildren<Animator>();
                        //ParticleSystem = GetComponentsInChildren<ParticleSystem>().Where(x => !x.gameObject.name.Contains("FirePlume")).ToArray();
                        ParticleSystem = GetComponentsInChildren<ParticleSystem>().Where(x => !x.gameObject.name.Contains("FirePlume")).ToArray();
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

                        var plumes = GetComponentsInChildren<ParticleSystem>().Where(x => x.gameObject.name.Contains("FirePlume")).ToArray();

                        foreach(ParticleSystem particleSystem in plumes)
                        {
                            Plumes.Add(new ZenithFirePlumeController(particleSystem, 0));
                        }

                        var glowControllers = GetComponentsInChildren<MeshRenderer>().Where(x => x.material.shader.name.Contains("Hopoo")).ToArray();

                        foreach(Renderer renderer in glowControllers)
                        {
                            GlowControllers.Add(new ZenithGlowController(this, renderer));
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
                        UnityEngine.Object.Destroy(this);
                    }
                }

                if (OwnerBody)
                {
                    foreach(ParticleSystem particleSystem in ParticleSystem)
                    {
                        if (OwnerBody.inventory.GetItemCount(ZenithAccelerator.instance.ItemDef) > 0 && ZenithAccelerator.instance.CalculateMaxBuffStacks(OwnerBody) == OwnerBody.GetBuffCount(ZenithAccelerationBuff))
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

                    foreach (ZenithFirePlumeController zenithFirePlumeController in Plumes)
                    {
                        zenithFirePlumeController.Update();
                    }

                }



                if(OwnerBody && Animator)
                {
                    Animator.speed = ZenithAccelerator.instance.CalculateSpeedMult(OwnerBody);
                }
            }

            public void Update()
            {
                if (OwnerBody)
                {
                    foreach (ZenithGlowController zenithGlowController in GlowControllers)
                    {
                        zenithGlowController.Update();
                    }
                }
            }
        }

        public class ZenithGlowController
        {
            public ZenithAcceleratorManager ManagerComponent;
            public Renderer Renderer;
            public float NoiseOffset;
            public float GlowCounter;

            public ZenithGlowController(ZenithAcceleratorManager managerComponent, Renderer renderer)
            {
                ManagerComponent = managerComponent;
                Renderer = renderer;
                NoiseOffset = UnityEngine.Random.Range(0, 1000);
            }

            public void Update()
            {
                var t = Time.time / 1f + NoiseOffset;
                var value = (Mathf.Sin(t * 7) * Mathf.Sin(t * 13) * Mathf.Sin(t * 43) * Mathf.Sin(t * 71) * Mathf.Sin(t * 101)) * 0.5f + 0.5f;
                var windupCutoff = 0.5f;
                var windup = Mathf.Clamp01(ManagerComponent.CalculateWindUp() - windupCutoff) / (1 - windupCutoff);


                Renderer.materials[0].SetFloat("_EmPower", EasingFunction.EaseInOutQuad(0, 10 - value * 7, windup));
            }

        }

        public class ZenithFirePlumeController
        {
            public ParticleSystem Plume;
            public float Stopwatch;

            public ZenithFirePlumeController(ParticleSystem plume, float stopwatch)
            {
                Plume = plume;
                Stopwatch = stopwatch;
            }

            public void Update()
            {
                Stopwatch -= Time.fixedDeltaTime;
                if(Stopwatch <= 0)
                {
                    Stopwatch += UnityEngine.Random.Range(1, 10);

                    if (!Plume.IsAlive())
                    {
                        var plumeMain = Plume.main;
                        plumeMain.duration = UnityEngine.Random.Range(1, 3);

                        Plume.Play();
                    }
                }
            }
        }
    }
}
