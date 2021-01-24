using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static Aetherium.Utils.MathHelpers;

namespace Aetherium.Items
{
    public class Voidheart : ItemBase<Voidheart>
    {
        public static float VoidImplosionDamageMultiplier;
        public static float VoidImplosionBaseRadius;
        public static float VoidImplosionAdditionalRadius;
        public static float VoidHeartBaseTickingTimeBombHealthThreshold;
        public static float VoidHeartAdditionalTickingTimeBombHealthThreshold;
        public static float VoidHeartMaxTickingTimeBombHealthThreshold;
        public static float VoidHeartCooldownDebuffDuration;

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

        public override string ItemModelPath => "@Aetherium:Assets/Models/Prefabs/Item/Voidheart/Voidheart.prefab";
        public override string ItemIconPath => "@Aetherium:Assets/Textures/Icons/Item/VoidheartIcon.png";

        public static GameObject ItemBodyModelPrefab;

        public static BuffIndex VoidInstabilityDebuff { get; private set; }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateMaterials();
            CreateBuff();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            VoidImplosionDamageMultiplier = config.Bind<float>("Item: " + ItemName, "Void Implosion Damage Multiplier", 120f, "How high should the damage multiplier be for the void implosion? (120 for example, is currentDamage * 120))").Value;
            VoidImplosionBaseRadius = config.Bind<float>("Item: " + ItemName, "Base Radius of Void Implosion", 15f, "What should the first Heart of the Void pickup's void implosion radius be? (Default: 15 (That is to say, 15m))").Value;
            VoidImplosionAdditionalRadius = config.Bind<float>("Item: " + ItemName, "Additional Implosion Radius per Additional Heart of the Void", 7.5f, "What should additional Heart of the Void pickups increase the radius of the void implosion by? (Default: 7.5 (That is to say, 7.5m))").Value;
            VoidHeartBaseTickingTimeBombHealthThreshold = config.Bind<float>("Item: " + ItemName, "Ticking Time Bomb Threshold", 0.3f, "What percentage of health should the first Heart of the Void pickup have the ticking time bomb activation be? (Default: 0.3 (30%))").Value;
            VoidHeartAdditionalTickingTimeBombHealthThreshold = config.Bind<float>("Item: " + ItemName, "Percentage Raise in Ticking Time Bomb Threshold per Additional Heart of the Void", 0.05f, "How much additional percentage should we add to the ticking time bomb threshold per stack of Heart of the Void? (Default: 0.05 (5%))").Value;
            VoidHeartMaxTickingTimeBombHealthThreshold = config.Bind<float>("Item: " + ItemName, "Absolute Max Ticking Time Bomb Threshold", 0.99f, "How high should our maximum ticking time bomb health threshold be? (Default: 0.99 (99%))").Value;
            VoidHeartCooldownDebuffDuration = config.Bind<float>("Item: " + ItemName, "Duration of Heart of the Void Cooldown After Use", 30f, "How should long should our Heart of the Void usage cooldown duration be? (Default: 30 (30 seconds))").Value;
        }

        private void CreateMaterials()
        {
            

            var heartVeinsMaterial = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/Item/Voidheart/VoidheartVeins.mat");
            heartVeinsMaterial.shader = AetheriumPlugin.HopooShader;
            heartVeinsMaterial.SetFloat("_Smoothness", 1f);
            heartVeinsMaterial.SetColor("_EmColor", new Color(70, 0, 72));
            heartVeinsMaterial.SetFloat("_EmPower", 0.00001f);

            var blackHeartMaterial = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/Item/Voidheart/VoidheartBlack.mat");
            blackHeartMaterial.shader = AetheriumPlugin.HopooShader;
            blackHeartMaterial.SetFloat("_Smoothness", 1);
            blackHeartMaterial.SetColor("_EmColor", new Color(1, 1, 1));
            blackHeartMaterial.SetFloat("_EmPower", 0.0001f);

        }

        private void CreateBuff()
        {
            var voidInstability = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = Color.magenta,
                canStack = false,
                isDebuff = true,
                name = "Aetherium: Void Instability Debuff",
                iconPath = "@Aetherium:Assets/Textures/Icons/Buff/VoidInstabilityDebuffIcon.png"
            });
            VoidInstabilityDebuff = R2API.BuffAPI.Add(voidInstability);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = Resources.Load<GameObject>(ItemModelPath);
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
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
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.CharacterMaster.OnBodyDeath += VoidheartDeathInteraction;
            On.RoR2.HealthComponent.Heal += Voidheart30PercentTimebomb;
            On.RoR2.CharacterBody.FixedUpdate += VoidheartOverlayManager;
            On.RoR2.CharacterBody.Awake += VoidheartPreventionInteraction;
            On.RoR2.CharacterBody.OnInventoryChanged += VoidheartAnnihilatesItselfOnDeployables;
        }

        private void VoidheartDeathInteraction(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, RoR2.CharacterMaster self, RoR2.CharacterBody body)
        {
            var InventoryCount = GetCount(body);
            if (InventoryCount > 0 && !body.healthComponent.killingDamageType.HasFlag(DamageType.VoidDeath) && !body.HasBuff(VoidInstabilityDebuff))
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
                componentVoidheartDeath.Init();

                var tempDestroyOnDeath = self.destroyOnBodyDeath;
                self.destroyOnBodyDeath = false;
                orig(self, body);
                self.destroyOnBodyDeath = tempDestroyOnDeath;
                self.preventGameOver = true;
                return;
            }
            orig(self, body);
        }

        private float Voidheart30PercentTimebomb(On.RoR2.HealthComponent.orig_Heal orig, RoR2.HealthComponent self, float amount, RoR2.ProcChainMask procChainMask, bool nonRegen)
        {
            var InventoryCount = GetCount(self.body);
            if (self.body && InventoryCount > 0)
            {
                if (self.combinedHealth <= self.fullCombinedHealth * Mathf.Clamp((VoidHeartBaseTickingTimeBombHealthThreshold + (VoidHeartAdditionalTickingTimeBombHealthThreshold * InventoryCount - 1)), VoidHeartBaseTickingTimeBombHealthThreshold, VoidHeartMaxTickingTimeBombHealthThreshold) &&
                    //This check is for the timer to determine time since spawn, at <= 10f it'll only activate after the tenth second
                    self.GetComponent<VoidHeartPrevention>().internalTimer >= 7f)
                {
                    RoR2.DamageInfo damageInfo = new RoR2.DamageInfo
                    {
                        crit = false,
                        damage = amount,
                        force = Vector3.zero,
                        position = self.transform.position,
                        procChainMask = procChainMask,
                        procCoefficient = 0f,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.Generic
                    };
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
                RoR2.TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                overlay.duration = VoidHeartCooldownDebuffDuration;
                overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlay.animateShaderAlpha = true;
                overlay.destroyComponentOnEnd = true;
                overlay.originalMaterial = Meshes[0].material;
                overlay.AddToCharacerModel(self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());
                var VoidheartCooldownTracker = self.gameObject.AddComponent<Voidheart.VoidheartCooldown>();
                VoidheartCooldownTracker.Overlay = overlay;
                VoidheartCooldownTracker.Body = self;
            }
            orig(self);
        }

        public class VoidheartCooldown : MonoBehaviour
        {
            public RoR2.TemporaryOverlay Overlay;
            public RoR2.CharacterBody Body;

            public void FixedUpdate()
            {
                if (!Body.HasBuff(VoidInstabilityDebuff))
                {
                    UnityEngine.Object.Destroy(Overlay);
                    UnityEngine.Object.Destroy(this);
                }
            }
        }

        //If I want to update the size of the meta-balls in the shader
        /*private void UpdateVoidheartVisual(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            orig(self);
            if (GetCount(self) > 0)
            {
                var scale = ruleLookup[self.modelLocator.modelTransform.name];
                ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos[0].defaultMaterial.SetFloat("_BlobScale", 3.16f / scale);
                ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos[0].defaultMaterial.SetFloat("_BlobDepth", 2.9f * scale);
                //ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos[0].defaultMaterial.SetFloat("_BlobScale", 3.16f + (1-scale));
                ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos[0].defaultMaterial.SetFloat("_BlobMoveSpeed", 6 * scale);
            }
        }*/

        private void VoidheartAnnihilatesItselfOnDeployables(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, RoR2.CharacterBody self)
        {
            orig(self);
            var InventoryCount = GetCount(self);
            if (InventoryCount > 0 && self.master)
            {
                if (self.master.teamIndex == TeamIndex.Player && !self.isPlayerControlled)
                {
                    //Unga bunga, voidheart not like deployables. POP!
                    self.inventory.RemoveItem(Index, InventoryCount);
                }
            }
        }

        public class VoidHeartPrevention : MonoBehaviour
        {
            public float internalTimer = 0f;

            private void Update()
            {
                internalTimer += Time.deltaTime;
            }

            public void ResetTimer()
            {
                internalTimer = 0f;
            }
        }

        private void VoidheartPreventionInteraction(On.RoR2.CharacterBody.orig_Awake orig, RoR2.CharacterBody self)
        {
            //First just run the normal awake stuff
            orig(self);
            //If I somehow lack the Prevention, give me one
            if (!self.gameObject.GetComponent<VoidHeartPrevention>())
            {
                self.gameObject.AddComponent<VoidHeartPrevention>();
            }
            //And reset the timer
            self.gameObject.GetComponent<VoidHeartPrevention>().ResetTimer();
        }
    }
}