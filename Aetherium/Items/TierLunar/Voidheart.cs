using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.MathHelpers;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace Aetherium.Items.TierLunar
{
    public class Voidheart : ItemBase<Voidheart>
    {
        public static ConfigOption<float> VoidImplosionCameraSpinSpeed;
        public static ConfigOption<float> VoidImplosionDamageMultiplier;
        public static ConfigOption<float> VoidImplosionBaseRadius;
        public static ConfigOption<float> VoidImplosionAdditionalRadius;
        public static ConfigOption<float> VoidHeartBaseTickingTimeBombHealthThreshold;
        public static ConfigOption<float> VoidHeartAdditionalTickingTimeBombHealthThreshold;
        public static ConfigOption<float> VoidHeartMaxTickingTimeBombHealthThreshold;
        public static ConfigOption<float> VoidHeartCooldownDebuffDuration;

        public override string ItemName => "Heart of the Void";

        public override string ItemLangTokenName => "HEART_OF_THE_VOID";

        public override string ItemPickupDesc => "On <style=cDeath>death</style>, cause a highly damaging void implosion that <style=cIsHealing>revives you if an enemy is killed by it</style> BUT at low health <style=cIsDamage>all healing is converted to damage</style>.";

        public override string ItemFullDescription => $"On <style=cDeath>death</style>, cause a highly damaging void implosion that is {VoidImplosionDamageMultiplier}x your damage with a radius of <style=cIsDamage>{VoidImplosionBaseRadius}m</style> <style=cStack>(+{VoidImplosionAdditionalRadius}m per stack)</style> that <style=cIsHealing>revives you if an enemy is killed by it</style> BUT at <style=cIsHealth>{FloatToPercentageString(VoidHeartBaseTickingTimeBombHealthThreshold)} health</style> <style=cStack>(+{FloatToPercentageString(VoidHeartAdditionalTickingTimeBombHealthThreshold)} per stack, max {FloatToPercentageString(VoidHeartMaxTickingTimeBombHealthThreshold)})</style> or lower, <style=cIsDamage>all kinds of healing are converted to damage</style>.";

        public override string ItemLore => "\n[INCIDENT NUMBER 511051]" +
            "\n[TRANSCRIPT TO FOLLOW]" +
            "\nElena: Hey uh...Robert...\n" +
            "\nRobert: Yeah?\n" +
            "\nElena: Remember that heart you told me to pick up from one of those dead crab things?\n" +
            "\nRobert: Yeah? What about it?\n" +
            "\nElena: Did you happen to see...where it went when I touched it a second ago?\n" +
            "\nRobert: Can't say I saw much other than a small flash of light when you did.\n" +
            "\nElena: Hand me the bioscanner really quick.\n" +
            "\n[Humming can be heard for a moment, followed by a loud BEEP.]\n" +
            "\nRobert: ...\n" +
            "\nElena: Robert, I think I know where the heart went. Please call for a medevac. I'm starting to feel unwell.\n" +
            "\nRobert: You'll be fine, we just have a bit more recon to do and we'll be back up on station in no time.\n" +
            "\n[A sound reminiscent of someone patting another on the back can be heard.]\n" +
            "\n[It is followed by a sound reminiscent to an alarm that goes on for a few seconds before it ends in a sharp pop a moment later.]\n" +
            "\n[TRANSCRIPT ENDS]" +
            "\n[ONE SURVIVOR FOUND IN PERFECTLY SPHERICAL CRATER.]";

        public override ItemTier Tier => ItemTier.Lunar;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Cleansable };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Voidheart.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("VoidheartIcon.png");

        public static GameObject ItemBodyModelPrefab;
        public static GameObject VoidPortal;
        public static DamageAPI.ModdedDamageType VoidHeartDamage;

        public RoR2.UI.HealthBarStyle HealthBarStyle;

        public static BuffDef VoidInstabilityDebuff { get; private set; }
        public static BuffDef VoidImmunityBuff { get; private set; }

        public List<string> RevivalItems = new List<string>()
        {
            "ExtraLife",
            "ExtraLifeVoid"
        };

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateUnlockable();
            CreateLang();
            CreateBuff();
            CreateDamageType();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            VoidImplosionCameraSpinSpeed = config.ActiveBind<float>("Item: " + ItemName, "Void Implosion Camera Spin Speed", 0.0125f, "How fast should the camera spin around the circle? (0.0125 = PI * 0.0125 or 1 half circle * 0.0125 per tick)");
            VoidImplosionDamageMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Void Implosion Damage Multiplier", 120f, "How high should the damage multiplier be for the void implosion? (120 for example, is currentDamage * 120))");
            VoidImplosionBaseRadius = config.ActiveBind<float>("Item: " + ItemName, "Base Radius of Void Implosion", 15f, "What should the first Heart of the Void pickup's void implosion radius be? (Default: 15 (That is to say, 15m))");
            VoidImplosionAdditionalRadius = config.ActiveBind<float>("Item: " + ItemName, "Additional Implosion Radius per Additional Heart of the Void", 7.5f, "What should additional Heart of the Void pickups increase the radius of the void implosion by? (Default: 7.5 (That is to say, 7.5m))");
            VoidHeartBaseTickingTimeBombHealthThreshold = config.ActiveBind<float>("Item: " + ItemName, "Ticking Time Bomb Threshold", 0.3f, "What percentage of health should the first Heart of the Void pickup have the ticking time bomb activation be? (Default: 0.3 (30%))");
            VoidHeartAdditionalTickingTimeBombHealthThreshold = config.ActiveBind<float>("Item: " + ItemName, "Percentage Raise in Ticking Time Bomb Threshold per Additional Heart of the Void", 0.05f, "How much additional percentage should we add to the ticking time bomb threshold per stack of Heart of the Void? (Default: 0.05 (5%))");
            VoidHeartMaxTickingTimeBombHealthThreshold = config.ActiveBind<float>("Item: " + ItemName, "Absolute Max Ticking Time Bomb Threshold", 0.99f, "How high should our maximum ticking time bomb health threshold be? (Default: 0.99 (99%))");
            VoidHeartCooldownDebuffDuration = config.ActiveBind("Item: " + ItemName, "Duration of Heart of the Void Cooldown After Use", 30f, "How should long should our Heart of the Void usage cooldown duration be? (Default: 30 (30 seconds))");

            var blacklistString = config.ActiveBind<string>("Item: " + ItemName, "Revival Items", "", "What revival items should be consumed before Heart of the Void activates? (generally no spaces, comma delimited) E.g.: ExtraLife,ExtraLifeConsumed");

            if (!string.IsNullOrWhiteSpace(blacklistString))
            {
                var blacklistedStringArray = blacklistString.ToString().Split(',');

                foreach (string blacklistedEntry in blacklistedStringArray)
                {
                    RevivalItems.Add(blacklistedEntry);
                }
            }
        }

        private void CreateUnlockable()
        {
            
        }

        private void CreateBuff()
        {
            VoidInstabilityDebuff = ScriptableObject.CreateInstance<BuffDef>();
            VoidInstabilityDebuff.name = "Aetherium: Void Instability Debuff";
            VoidInstabilityDebuff.buffColor = Color.magenta;
            VoidInstabilityDebuff.canStack = false;
            VoidInstabilityDebuff.isDebuff = false;
            VoidInstabilityDebuff.iconSprite = MainAssets.LoadAsset<Sprite>("VoidInstabilityDebuffIcon.png");

            VoidImmunityBuff = ScriptableObject.CreateInstance<BuffDef>();
            VoidImmunityBuff.name = "Aetherium: Voidheart Temporary Immunity";
            VoidImmunityBuff.buffColor = Color.gray;
            VoidImmunityBuff.canStack = false;
            VoidImmunityBuff.isDebuff = false;
            VoidImmunityBuff.iconSprite = MainAssets.LoadAsset<Sprite>("VoidInstabilityDebuffIcon.png");

            ContentAddition.AddBuffDef(VoidInstabilityDebuff);
            ContentAddition.AddBuffDef(VoidImmunityBuff);

        }

        public void CreateDamageType()
        {
            VoidHeartDamage = DamageAPI.ReserveDamageType();

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.15f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.1f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.5f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.9f, 0.9f, 0.9f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.14f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.11f, 0.11f, 0.11f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.08f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.07f, 0.07f, 0.07f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.09f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.05f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.4f, 0.4f, 0.4f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.1f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.1f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.1f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.11f, 0.11f, 0.11f)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.1989F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.071F, 0.071F, 0.071F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Body",
                    localPos = new Vector3(0F, 0.01217F, -0.00126F),
                    localAngles = new Vector3(356.9376F, 25.8988F, 14.69767F),
                    localScale = new Vector3(0.003F, 0.003F, 0.003F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00556F, 0.08616F, 0.00375F),
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
                    localPos = new Vector3(0F, 0.15193F, 0.02972F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.07023F, 0.07023F, 0.07023F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.14666F, 0F),
                    localAngles = new Vector3(0F, 270F, 0F),
                    localScale = new Vector3(0.10293F, 0.10293F, 0.10293F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.CharacterMaster.OnBodyDeath += VoidheartDeathInteraction;
            On.RoR2.HealthComponent.Heal += Voidheart30PercentTimebomb;
            On.RoR2.CharacterBody.FixedUpdate += VoidheartOverlayManager;
            On.RoR2.CharacterBody.Start += CacheHealthForVoidheart;
            On.RoR2.CharacterBody.Awake += PreventVoidheartFromKillingPlayer;
            On.RoR2.CharacterBody.OnInventoryChanged += VoidheartAnnihilatesItselfOnDeployables;
            //IL.RoR2.HealthComponent.TakeDamage += InterceptPlanula;
        }

        private void CacheHealthForVoidheart(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (self)
            {
                var inventoryCount = GetCount(self);
                if (inventoryCount > 0)
                {
                    self.AddTimedBuff(VoidImmunityBuff, 3f);
                }
            }
            var cacheComponent = self.GetComponent<VoidHeartCacheHealthComponent>();
            if (!cacheComponent) 
            {
                cacheComponent = self.gameObject.AddComponent<VoidHeartCacheHealthComponent>();
                cacheComponent.LastMaxHealth = self.maxHealth;
            }
        }

        private void VoidheartDeathInteraction(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, RoR2.CharacterMaster self, RoR2.CharacterBody body)
        {
            var InventoryCount = GetCount(body);
            if (InventoryCount > 0 && !body.healthComponent.killingDamageType.damageType.HasFlag(DamageType.VoidDeath) && !body.HasBuff(VoidInstabilityDebuff))
            {
                bool hasBlacklistedRevivalItems = false;
                foreach (var item in RevivalItems)
                {
                    var blackListedItem = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(item));
                    if (blackListedItem)
                    {
                        var itemCount = body.inventory.GetItemCount(blackListedItem);
                        if(itemCount > 0)
                        {
                            hasBlacklistedRevivalItems = true;
                        }
                    }
                }

                if (!hasBlacklistedRevivalItems)
                {
                    GameObject explosion = new GameObject();
                    explosion.transform.position = body.transform.position;

                    var componentVoidheartDeath = explosion.AddComponent<VoidheartDeath>();
                    componentVoidheartDeath.toReviveMaster = self;
                    componentVoidheartDeath.toReviveBody = body;
                    componentVoidheartDeath.voidExplosionRadius = VoidImplosionBaseRadius + (VoidImplosionAdditionalRadius * (InventoryCount - 1));
                    componentVoidheartDeath.voidHeartImplosionDamageMultiplier = VoidImplosionDamageMultiplier;
                    componentVoidheartDeath.voidInstabilityDebuff = VoidInstabilityDebuff;
                    componentVoidheartDeath.voidHeartCooldownDuration = VoidHeartCooldownDebuffDuration;
                    componentVoidheartDeath.intendedDuration = 4;
                    componentVoidheartDeath.shrinkDuration = 0.3f;
                    componentVoidheartDeath.Init();

                    var tempDestroyOnDeath = self.destroyOnBodyDeath;
                    self.destroyOnBodyDeath = false;
                    orig(self, body);
                    self.destroyOnBodyDeath = tempDestroyOnDeath;
                    self.preventGameOver = true;
                    return;
                }
            }
            orig(self, body);
        }

        private float Voidheart30PercentTimebomb(On.RoR2.HealthComponent.orig_Heal orig, RoR2.HealthComponent self, float amount, RoR2.ProcChainMask procChainMask, bool nonRegen)
        {
            var InventoryCount = GetCount(self.body);
            if (self.body && InventoryCount > 0)
            {
                var cacheComponent = self.body.GetComponent<VoidHeartCacheHealthComponent>();
                if (!cacheComponent)
                {
                    cacheComponent = self.body.gameObject.AddComponent<VoidHeartCacheHealthComponent>();
                    cacheComponent.LastMaxHealth = self.body.maxHealth;
                }
                else 
                {
                    if(cacheComponent.LastMaxHealth != self.body.maxHealth)
                    {
                        self.body.AddTimedBuffAuthority(VoidImmunityBuff.buffIndex, 0.1f);
                        cacheComponent.LastMaxHealth = self.body.maxHealth;
                    }
                }

                if (self.combinedHealth <= self.fullCombinedHealth * Mathf.Clamp((VoidHeartBaseTickingTimeBombHealthThreshold + (VoidHeartAdditionalTickingTimeBombHealthThreshold * InventoryCount - 1)), VoidHeartBaseTickingTimeBombHealthThreshold, VoidHeartMaxTickingTimeBombHealthThreshold) && self.GetComponent<VoidheartPrevention>().InternalTimer >= 7f && !self.body.HasBuff(VoidImmunityBuff))
                {
                    RoR2.DamageInfo damageInfo = new RoR2.DamageInfo
                    {
                        crit = false,
                        damage = amount,
                        force = Vector3.zero,
                        position = self.transform.position,
                        procChainMask = procChainMask,
                        procCoefficient = 0f,
                        damageColorIndex = DamageColorIndex.Default
                    };
                    DamageAPI.AddModdedDamageType(damageInfo, VoidHeartDamage);
                    self.TakeDamage(damageInfo);
                    return orig(self, 0, procChainMask, nonRegen);
                }
            }
            return orig(self, amount, procChainMask, nonRegen);
        }

        private void VoidheartOverlayManager(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {

            if (self.modelLocator && self.modelLocator.modelTransform && self.HasBuff(VoidInstabilityDebuff) && !self.GetComponent<VoidheartCooldown>())
            {
                var Meshes = Voidheart.ItemBodyModelPrefab.GetComponentsInChildren<MeshRenderer>();
                var overlay = TemporaryOverlayManager.AddOverlay(self.modelLocator.modelTransform.gameObject);
                overlay.duration = VoidHeartCooldownDebuffDuration;
                overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlay.animateShaderAlpha = true;
                overlay.destroyComponentOnEnd = true;
                overlay.originalMaterial = MainAssets.LoadAsset<Material>("VoidheartPlaceholderTexture.mat");
                overlay.AddToCharacterModel(self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());
                var VoidheartCooldownTracker = self.gameObject.AddComponent<Voidheart.VoidheartCooldown>();
                VoidheartCooldownTracker.Overlay = overlay;
                VoidheartCooldownTracker.Body = self;
            }
            orig(self);
        }

        /*private void InterceptPlanula(ILContext il)
        {
            var c = new ILCursor(il);
            ILLabel label = null;

            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(1),
                x => x.MatchLdflda<RoR2.HealthComponent>("itemCounts"),
                x => x.MatchLdfld<RoR2.HealthComponent.ItemCounts>("parentEgg"), 
                x => x.MatchLdcI4(0), 
                x => x.MatchBle(out label));

            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Func<DamageInfo, bool>>(damageInfo =>
                !DamageAPI.HasModdedDamageType(damageInfo, VoidHeartDamage));
            c.Emit(OpCodes.Brfalse, label);
        }*/

        public class VoidheartCooldown : MonoBehaviour
        {
            public RoR2.TemporaryOverlayInstance Overlay;
            public RoR2.CharacterBody Body;

            public void FixedUpdate()
            {
                if (!Body.HasBuff(VoidInstabilityDebuff))
                {
                    UnityEngine.Object.Destroy(this);
                    Overlay.CleanupEffect();
                }
            }
        }

        public class VoidHeartCacheHealthComponent : MonoBehaviour
        {
            public float LastMaxHealth;
        }

        private void VoidheartAnnihilatesItselfOnDeployables(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, RoR2.CharacterBody self)
        {
            orig(self);
            var InventoryCount = GetCount(self);
            if (InventoryCount > 0 && self.master)
            {
                /*if (self.master.teamIndex == TeamIndex.Player && !self.isPlayerControlled)
                {
                    //Unga bunga, voidheart not like deployables. POP!
                    self.inventory.RemoveItem(ItemDef, InventoryCount);
                }*/
            }
        }

        public class VoidheartPrevention : MonoBehaviour
        {
            public float InternalTimer = 0f;

            private void Update()
            {
                InternalTimer += Time.deltaTime;
            }

            public void ResetTimer()
            {
                InternalTimer = 0f;
            }
        }

        private void PreventVoidheartFromKillingPlayer(On.RoR2.CharacterBody.orig_Awake orig, RoR2.CharacterBody self)
        {
            //First just run the normal awake stuff
            orig(self);
            //If I somehow lack the Prevention, give me one
            if (!self.gameObject.GetComponent<VoidheartPrevention>())
            {
                self.gameObject.AddComponent<VoidheartPrevention>();
            }
            //And reset the timer
            self.gameObject.GetComponent<VoidheartPrevention>().ResetTimer();
        }
    }
}