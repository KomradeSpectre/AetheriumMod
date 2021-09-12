using static Aetherium.AetheriumPlugin;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Aetherium.Utils;
using static Aetherium.Utils.ItemHelpers;
using System.Linq;
using UnityEngine.Networking;

namespace Aetherium.Items
{
    public class ZenithAccelerator : ItemBase<ZenithAccelerator>
    { 
        public override string ItemName => "Zenith Accelerator";

        public override string ItemLangTokenName => "ZENITH_ACCELERATOR";

        public override string ItemPickupDesc => "Each hit grants you a temporary attack speed bonus stack up to a cap, but you";

        public override string ItemFullDescription => "";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Lunar;

        public override bool AIBlacklisted => true;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("ZenithAccelerator.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("NailBombIcon.png");

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
        }

        private void CreateBuff()
        {
            ZenithAccelerationBuff = ScriptableObject.CreateInstance<BuffDef>();
            ZenithAccelerationBuff.name = "Aetherium: Zenith Acceleration";
            ZenithAccelerationBuff.buffColor = new Color(255, 255, 255);
            ZenithAccelerationBuff.canStack = true;
            ZenithAccelerationBuff.isDebuff = false;
            ZenithAccelerationBuff.iconSprite = MainAssets.LoadAsset<Sprite>("NailBombNailCooldownIcon.png");

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
                    childName = "Pelvis",
                    localPos = new Vector3(-0.16759F, -0.07591F, 0.06936F),
                    localAngles = new Vector3(343.2889F, 299.2036F, 176.8172F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.14431F, -0.06466F, -0.03696F),
                    localAngles = new Vector3(355.1616F, 81.55997F, 180F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
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
            R2API.RecalculateStatsAPI.GetStatCoefficients += CalculateAttackSpeedStat;
            On.RoR2.GlobalEventManager.OnHitEnemy += AddAttackSpeedBuff;
        }

        private void CalculateAttackSpeedStat(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var inventoryCount = GetCount(sender);
            if (inventoryCount > 0)
            {
                var buffCount = sender.GetBuffCount(ZenithAccelerationBuff);
                var Base = 1.0 / Math.Pow(2, inventoryCount);
                //var Increase = 0.10 + (inventoryCount - 1) * 0.05;
                var Increase = 0.10;
                var Max = 3.0 + (inventoryCount - 1) * 1.0;
                var SpeedMult = (float)Math.Min(Base + Increase * buffCount, Max);

                args.attackSpeedMultAdd += SpeedMult - 1.0f;
                
            }
        }

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
                        var baseAmount = 1.0 / Math.Pow(2, inventoryCount);
                        //var increase = 0.10 + (inventoryCount - 1) * 0.05;
                        var increase = 0.10;
                        var max = 3.0 + (inventoryCount - 1) * 1.0;
                        int maxBuffCount = (int)Math.Ceiling((max - baseAmount) / increase);

                        RefreshTimedBuffs(body, ZenithAccelerationBuff, 1 + (0.5f * (inventoryCount - 1)), 0.25f / inventoryCount);

                        if (buffCount < maxBuffCount)
                        {
                            body.AddTimedBuff(ZenithAccelerationBuff, 3);
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

            public int CalculateMaxBuffStacks()
            {
                var inventoryCount = OwnerBody.inventory.GetItemCount(ZenithAccelerator.instance.ItemDef);
                var baseAmount = 1.0 / Math.Pow(2, inventoryCount);
                var increase = 0.10;
                var max = 3.0 + (inventoryCount - 1) * 1.0;
                int maxBuffCount = (int)Math.Ceiling((max - baseAmount) / increase);

                return maxBuffCount;
            }

            public float CalculateSpeedMult()
            {
                var buffCount = OwnerBody.GetBuffCount(ZenithAccelerationBuff);
                var inventoryCount = OwnerBody.inventory.GetItemCount(ZenithAccelerator.instance.ItemDef);
                var Base = 1.0f / Mathf.Pow(2, inventoryCount);
                var Increase = 0.10f;
                var Max = 3.0f + (inventoryCount - 1) * 1.0f;
                var SpeedMult = Mathf.Min(Base + Increase * buffCount, Max);

                return SpeedMult;
            }

            public float CalculateWindUp()
            {
                var buffCount = OwnerBody.GetBuffCount(ZenithAccelerationBuff);
                var inventoryCount = OwnerBody.inventory.GetItemCount(ZenithAccelerator.instance.ItemDef);
                var Base = 1.0f / Mathf.Pow(2, inventoryCount);
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
                        if (OwnerBody.inventory.GetItemCount(ZenithAccelerator.instance.ItemDef) > 0 && CalculateMaxBuffStacks() == OwnerBody.GetBuffCount(ZenithAccelerationBuff))
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
                    Animator.speed = CalculateSpeedMult();
                }
            }

            public void Update()
            {
                foreach(ZenithGlowController zenithGlowController in GlowControllers)
                {
                    zenithGlowController.Update();
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


                //TODO: Lerp the buff count to make the fading happen more smoothly.
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
