using Aetherium.Effect;
using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;

namespace Aetherium.Items.Tier2
{
    public class SharkTeeth : ItemBase<SharkTeeth>
    {
        public static ConfigOption<float> BaseDamageSpreadPercentage;
        public static ConfigOption<float> AdditionalDamageSpreadPercentage;
        public static ConfigOption<float> MaxDamageSpreadPercentage;
        public static ConfigOption<float> DurationOfDamageSpread;
        public static ConfigOption<bool> SharkTeethIsNonLethal;
        public static ConfigOption<bool> EnableCleanseOnHit;
        public static ConfigOption<float> CleansePercentage;

        public override string ItemName => "Shark Teeth";
        public override string ItemLangTokenName => "SHARK_TEETH";
        public override string ItemPickupDesc => "A portion of damage taken is distributed to you over time as <style=cIsDamage>bleed damage</style>.";
        public override string ItemFullDescription => $"<style=cIsDamage>{FloatToPercentageString(BaseDamageSpreadPercentage)}</style> of damage taken <style=cStack>(+{FloatToPercentageString(AdditionalDamageSpreadPercentage)} per stack)</style> is distributed to you over {DurationOfDamageSpread} second(s) as <style=cIsDamage>bleed damage</style>.";
        public override string ItemLore => "Order: Experimental Bio-Augment [Classified]\n" +
                   "Tracking Number: 44-KILO\n" +
                   "Estimated Delivery: 04/05/2056\n\n" +
                   "\"We strapped the sample to the test subject. The results were... visceral.\"\n\n" +
                   "\"The subject took a lethal kinetic impact. By all metrics, his ribcage should have collapsed instantly. Instead, the force was distributed. Delayed. He was bleeding profusely, yes, but he remained combat effective.\"\n\n" +
                   "\"Then came the anomaly. As the subject engaged the target, his vitals stabilized. It appears the augment creates a sympathetic link between aggression and survival. <style=cMono>Violence metabolizes the pain.</style> As long as he kept fighting, the deferred trauma simply... vanished.\"\n\n" +
                   "\"It turns a soldier into a shark. If they stop moving, they die. So they don't stop.\"";

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("SharkTeeth.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("SharkTeethIcon.png");

        public static GameObject ItemBodyModelPrefab;       
        public static GameObject SharkTeethInflictor;
        public static HealthBarAPI.BarOverlayIndex SharkTeethOverlayIndex;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateVisualBar();
            CreateNetworking();
            CreateLang();
            CreateItem();
            Hooks();

            SharkTeethInflictor = new GameObject("SharkTeethDamageSource");
            UnityEngine.Object.DontDestroyOnLoad(SharkTeethInflictor);
        }

        private void CreateVisualBar()
        {
            var overlayInfo = new HealthBarAPI.BarOverlayInfo
            {
                BodySpecific = true,

                BarInfo = new HealthBar.BarInfo
                {
                    color = new Color(0.8f, 0.8f, 0.8f, 0.70f),
                    imageType = UnityEngine.UI.Image.Type.Tiled,
                    enabled = true
                },

                ModifyBarInfo = (RoR2.UI.HealthBar healthBar, ref HealthBar.BarInfo barInfo) =>
                {
                    if(!healthBar.source || !healthBar.source.body)
                    {
                        barInfo.enabled = false;
                        return;
                    }

                    var behavior = healthBar.source.body.GetComponent<SharkTeethBehavior>();

                    if(!behavior || behavior.StoredBleedPool <= 0)
                    {
                        barInfo.enabled = false;
                        return;
                    }

                    float currentHealth = healthBar.source.health;
                    float maxHealth = healthBar.source.fullHealth;

                    if(maxHealth <= 0)
                    {
                        barInfo.enabled = false;
                        return;
                    }

                    float endFraction = currentHealth / maxHealth;
                    float startFraction = (currentHealth - behavior.StoredBleedPool) / maxHealth;
                    startFraction = Mathf.Max(0f, startFraction);

                    barInfo.enabled = true;
                    barInfo.normalizedXMin = startFraction;
                    barInfo.normalizedXMax = endFraction;
                }
            };

            SharkTeethOverlayIndex = HealthBarAPI.RegisterBarOverlay(overlayInfo);
        }

        private void CreateNetworking()
        {
            NetworkingAPI.RegisterMessageType<SyncBleedPool>();
        }

        private void CreateConfig(ConfigFile config)
        {
            BaseDamageSpreadPercentage = config.ActiveBind<float>("Item: " + ItemName, "Base Damage Spread", 0.25f, "Percentage of damage to delay.");
            AdditionalDamageSpreadPercentage = config.ActiveBind<float>("Item: " + ItemName, "Spread Per Stack", 0.25f, "Hyperbolic scaling per stack.");
            MaxDamageSpreadPercentage = config.ActiveBind<float>("Item: " + ItemName, "Max Spread Cap", 0.75f, "Hard cap on percentage delayed.");
            DurationOfDamageSpread = config.ActiveBind<float>("Item: " + ItemName, "Spread Duration", 5f, "Seconds to spread damage over.");
            SharkTeethIsNonLethal = config.ActiveBind<bool>("Item: " + ItemName, "Non-Lethal Bleed", false, "If true, bleed ticks cannot kill you (leaves you at 1 HP).");

            EnableCleanseOnHit = config.ActiveBind<bool>("Item: " + ItemName, "Berserker Mode (Cleanse on Hit)", true, "If true, dealing damage reduces your stored bleed amount. Distinguishes this item from Warped Echo.");
            CleansePercentage = config.ActiveBind<float>("Item: " + ItemName, "Cleanse Percentage", 0.1f, "Percentage of stored bleed removed when you deal damage.");
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
            On.RoR2.CharacterBody.OnInventoryChanged += ManageComponent;
            On.RoR2.HealthComponent.TakeDamage += MitigateDamage;

            if(EnableCleanseOnHit)
            {
                On.RoR2.GlobalEventManager.OnHitEnemy += CleanseBleed;
            }
        }

        private void ManageComponent(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if(!self.inventory) return;

            int count = self.inventory.GetItemCount(ItemDef);
            var behavior = self.GetComponent<SharkTeethBehavior>();

            if(count > 0)
            {
                if(!behavior) behavior = self.gameObject.AddComponent<SharkTeethBehavior>();
                behavior.StackCount = count;
            }
            else if(behavior)
            {
                UnityEngine.Object.Destroy(behavior);
            }
        }

        private void MitigateDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if(damageInfo.rejected || damageInfo.damage <= 0 || !self.body)
            {
                orig(self, damageInfo);
                return;
            }

            if(damageInfo.inflictor == SharkTeethInflictor)
            {
                orig(self, damageInfo);
                return;
            }

            var behavior = self.GetComponent<SharkTeethBehavior>();
            if(behavior && behavior.StackCount > 0)
            {
                float flatThreshold = 5f;
                float percentThreshold = self.fullHealth * 0.02f;     
                float effectiveThreshold = Mathf.Min(flatThreshold, percentThreshold);

                if(damageInfo.damage >= effectiveThreshold)
                {
                    float ratio = BaseDamageSpreadPercentage + (MaxDamageSpreadPercentage - MaxDamageSpreadPercentage / (1f + AdditionalDamageSpreadPercentage * (behavior.StackCount - 1)));
                    ratio = Mathf.Clamp(ratio, 0f, 0.9f);

                    float damageToDelay = damageInfo.damage * ratio;
                    float damageImmediate = damageInfo.damage - damageToDelay;

                    behavior.AddToPool(damageToDelay, damageInfo.attacker);
                    damageInfo.damage = damageImmediate;
                }
            }

            orig(self, damageInfo);
        }

        private void CleanseBleed(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if(!damageInfo.attacker) return;

            DamageSource allowedSources = DamageSource.SkillMask;

            if((damageInfo.damageType.damageSource & allowedSources) == DamageSource.NoneSpecified)
            {
                return;
            }

            var behavior = damageInfo.attacker.GetComponent<SharkTeethBehavior>();
            if(behavior)
            {
                behavior.Cleanse(CleansePercentage);
            }
        }

        public class SharkTeethBehavior : MonoBehaviour
        {
            public int StackCount;
            public CharacterBody Body;

            public float StoredBleedPool;

            private GameObject lastAttacker;

            private float tickTimer;
            private const float TickInterval = 0.5f;

            private bool _hasAddedOverlay;
            private bool _isBleedSyncDirty;

            public void Awake()
            {
                Body = GetComponent<CharacterBody>();
            }

            private void Start()
            {
                if(Body)
                {
                    HealthBarAPI.AddOverlayToBody(Body, SharkTeeth.SharkTeethOverlayIndex);
                    _hasAddedOverlay = true;
                }
            }

            public void OnDestroy()
            {
                if(_hasAddedOverlay && Body)
                {
                    HealthBarAPI.RemoveOverlayFromBody(Body, SharkTeeth.SharkTeethOverlayIndex);
                }
            }

            private void SendBleedSync()
            {
                if(!NetworkServer.active) return;

                new SyncBleedPool(
                    SyncBleedPool.MessageType.CachedBloodPool,
                    Body.netId,
                    StoredBleedPool
                ).Send(NetworkDestination.Clients);
            }

            public void AddToPool(float damage, GameObject attacker)
            {
                StoredBleedPool += damage;
                if(attacker) lastAttacker = attacker;
                _isBleedSyncDirty = true;
            }

            public void Cleanse(float percentage)
            {
                if(StoredBleedPool > 0)
                {
                    float amountToRemove = StoredBleedPool * percentage;
                    StoredBleedPool -= amountToRemove;
                    _isBleedSyncDirty = true;

                }
            }

            public void FixedUpdate()
            {
                // FIX: Stop the client from running this logic!
                if(!NetworkServer.active) return;

                // 1. Logic: Process the bleed tick if we have a pool
                if(StoredBleedPool > 0)
                {
                    tickTimer -= Time.fixedDeltaTime;
                    if(tickTimer <= 0)
                    {
                        tickTimer = TickInterval;
                        ProcessTick();
                    }
                }

                // 2. Networking: Batch the updates
                if(_isBleedSyncDirty)
                {
                    SendBleedSync();
                    _isBleedSyncDirty = false;
                }
            }

            private void ProcessTick()
            {
                float bleedDuration = SharkTeeth.DurationOfDamageSpread;
                float fractionToTake = TickInterval / bleedDuration;

                float damageThisTick = Mathf.Max(StoredBleedPool * fractionToTake, 1f);

                damageThisTick = Mathf.Min(damageThisTick, StoredBleedPool);

                if(Body.healthComponent)
                {
                    if(SharkTeeth.SharkTeethIsNonLethal && Body.healthComponent.health <= damageThisTick)
                    {
                        StoredBleedPool = 0;
                        _isBleedSyncDirty = true;
                    }
                    else
                    {
                        DamageInfo bleedInfo = new DamageInfo
                        {
                            damage = damageThisTick,
                            attacker = lastAttacker,
                            inflictor = SharkTeeth.SharkTeethInflictor,
                            position = Body.corePosition,
                            damageColorIndex = DamageColorIndex.Bleed,
                            damageType = (SharkTeethIsNonLethal ? DamageType.NonLethal : DamageType.Generic) | DamageType.Silent,   
                            procCoefficient = 0f,
                            
                        };

                        Body.healthComponent.TakeDamage(bleedInfo);
                        StoredBleedPool -= damageThisTick;
                        _isBleedSyncDirty = true;
                    }
                }
            }
        }

        public class SyncBleedPool : INetMessage
        {
            private MessageType TypeOfMessage;
            private NetworkInstanceId PlayerBody;
            private float StoredBleedPool;

            public SyncBleedPool()
            {
            }

            public SyncBleedPool(MessageType messageType, NetworkInstanceId playerbody, float storedBleedPool)
            {
                TypeOfMessage = messageType;
                PlayerBody = playerbody;
                StoredBleedPool = storedBleedPool;
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write((byte)TypeOfMessage);
                writer.Write(PlayerBody);
                writer.Write(StoredBleedPool);
            }

            public void Deserialize(NetworkReader reader)
            {
                TypeOfMessage = (MessageType)reader.ReadByte();
                PlayerBody = reader.ReadNetworkId();
                StoredBleedPool = reader.ReadSingle();
            }

            public void OnReceived()
            {
                if(NetworkServer.active) return;

                GameObject playerGameObject = RoR2.Util.FindNetworkObject(PlayerBody);
                if(playerGameObject)
                {
                    var behavior = playerGameObject.GetComponent<SharkTeethBehavior>();

                    if(!behavior)
                    {
                        behavior = playerGameObject.AddComponent<SharkTeethBehavior>();
                    }
                    if(behavior)
                    {
                        behavior.StoredBleedPool = StoredBleedPool;
                    }
                }
            }

            public enum MessageType : byte
            {
                CachedBloodPool
            }
        }
    }
}