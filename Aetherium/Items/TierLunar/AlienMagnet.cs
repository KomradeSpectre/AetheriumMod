using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Items.TierLunar
{
    public class AlienMagnet : ItemBase<AlienMagnet>
    {
        public static ConfigOption<int> HitsRequiredToLevitate;
        public static ConfigOption<float> LevitationDuration;
        public static ConfigOption<float> BasePullForce;
        public static ConfigOption<float> MaxPullDistance;
        public static ConfigOption<float> SelfPullReduction;
        public static ConfigOption<float> PullStrengthIncrease;
        public static ConfigOption<float> PullCooldownPerEnemy;
        public static ConfigOption<float> AirControlDuringPull;
        public static ConfigOption<bool> AffectsBosses;
        public static ConfigOption<float> BossMassMultiplier;

        public override string ItemName => "Alien Magnet";
        public override string ItemLangTokenName => "ALIEN_MAGNET";

        public override string ItemPickupDesc => "Consecutive hits <style=cIsUtility>destabilize gravity</style> on enemies. Pulling enemies <style=cDeath>pulls you toward them</style>.";

        public override string ItemFullDescription =>
            $"Hitting an enemy applies <style=cIsUtility>Magnetic Charge</style>. At <style=cIsUtility>{HitsRequiredToLevitate} stacks</style>, the enemy <style=cIsUtility>levitates</style> for <style=cIsUtility>{LevitationDuration}s</style> <style=cStack>(+{LevitationDuration}s per stack)</style>.\n" +
            $"Attacking levitated enemies creates a <style=cIsUtility>magnetic pull</style> between you and them. <style=cDeath>Heavier enemies pull YOU more than you pull them</style>.";

        public override string ItemLore =>
            "Order: Alien Magnet\n" +
            "Tracking Number: 402-AM******\n" +
            "Estimated Delivery: Who knows at this point\n" +
            "Shipping Method: FRAGILE - GRAVITATIONAL ANOMALY\n\n" +

            "WARNING: Contents may cause localized spacetime distortion.\n\n" +

            "To whoever finds this:\n\n" +

            "DO NOT activate near planetary bodies. DO NOT activate while holding anything valuable. " +
            "DO NOT activate if you value your current position in space.\n\n" +

            "Our 'research team' (poor bastards) discovered that biological matter and this device " +
            "have a... reciprocal relationship with gravity. When you pull something toward you, " +
            "well, you get pulled right back. Newton would be thrilled. Our insurance company is not.\n\n" +

            "Test Subject #4 pulled a boulder toward himself. He traveled 40 meters. The boulder traveled 2. " +
            "Test Subject #7 tried to pull a small creature. It worked great until she realized she was now " +
            "standing exactly where she didn't want to be.\n\n" +

            "The device is yours now. We're done with it. Please sign the liability waiver.\n\n" +

            "Oh, and if you see Test Subject #4, tell him we found his boot.\n\n" +

            "P.S. - Don't aim it at anything heavier than you are. Trust us on this one.";

        public override ItemTier Tier => ItemTier.Lunar;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.Damage };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("AlienMagnet.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("AlienMagnetIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public static BuffDef MagneticChargeBuff;
        public static BuffDef ZeroGBuff;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuffs();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            HitsRequiredToLevitate = config.ActiveBind<int>("Item: " + ItemName, "Hits Required to Levitate", 5, "How many hits does it take to trigger the zero-gravity effect?");
            LevitationDuration = config.ActiveBind<float>("Item: " + ItemName, "Levitation Duration", 3f, "Base levitation duration in seconds");

            BasePullForce = config.ActiveBind<float>("Item: " + ItemName, "Base Pull Force", 2500f, "Base force applied during magnetic pull");
            MaxPullDistance = config.ActiveBind<float>("Item: " + ItemName, "Max Pull Distance", 40f, "Maximum distance for pull to be effective");

            SelfPullReduction = config.ActiveBind<float>("Item: " + ItemName, "Self Pull Reduction Per Stack", 0.12f, "How much each additional stack reduces the force pulling you (0.12 = 12% reduction per stack)");
            PullStrengthIncrease = config.ActiveBind<float>("Item: " + ItemName, "Pull Strength Increase Per Stack", 0.35f, "How much each additional stack increases total pull strength (0.35 = 35% stronger per stack)");

            PullCooldownPerEnemy = config.ActiveBind<float>("Item: " + ItemName, "Pull Cooldown Per Enemy", 0.5f, "Cooldown in seconds between pulls on the same enemy");
            AirControlDuringPull = config.ActiveBind<float>("Item: " + ItemName, "Air Control During Pull Multiplier", 0.25f, "Air control multiplier while being pulled (0.25 = 25% of normal control)");

            AffectsBosses = config.ActiveBind<bool>("Item: " + ItemName, "Affects Bosses", true, "If true, bosses can be levitated (WARNING: Pulling bosses is extremely dangerous!)");
            BossMassMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Boss Mass Multiplier", 6f, "Mass multiplier for bosses in pull calculations (higher = you get pulled more toward them)");
        }

        private void CreateBuffs()
        {
            MagneticChargeBuff = ScriptableObject.CreateInstance<BuffDef>();
            MagneticChargeBuff.name = "Aetherium: Magnetic Charge";
            MagneticChargeBuff.buffColor = Color.cyan;
            MagneticChargeBuff.canStack = true;
            MagneticChargeBuff.isDebuff = true;
            MagneticChargeBuff.iconSprite = MainAssets.LoadAsset<Sprite>("AlienMagnetLiftDebuffIcon.png");
            ContentAddition.AddBuffDef(MagneticChargeBuff);

            ZeroGBuff = ScriptableObject.CreateInstance<BuffDef>();
            ZeroGBuff.name = "Aetherium: Zero-G";
            ZeroGBuff.buffColor = Color.magenta;
            ZeroGBuff.canStack = false;
            ZeroGBuff.isDebuff = true;
            ZeroGBuff.iconSprite = MainAssets.LoadAsset<Sprite>("AlienMagnetLevitationDebuffIcon.png");
            ContentAddition.AddBuffDef(ZeroGBuff);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = Aetherium.Utils.ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-9.05238F, -2F, 5.00013F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.28301F, -0.34921F, -1.00009F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.15F, 0.15F, 0.15F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(5F, 0F, 10F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.70726F, -0.17282F, -1.00137F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.15F, 0.15F, 0.15F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.039F, -0.8778F, -0.5109F),
                    localAngles = new Vector3(-90f, 0F, 0F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chef",
                    localPos = new Vector3(0.03661F, 0.01391F, 0.03791F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(0.00424F, 0.00424F, 0.00424F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.34614F, 1.44644F, -0.56914F),
                    localAngles = new Vector3(4.32037F, 93.26095F, 357.2377F),
                    localScale = new Vector3(0.14018F, 0.14018F, 0.14018F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Root",
                    localPos = new Vector3(0.741F, 1.24812F, -0.85671F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.09F, 0.09F, 0.09F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.41153F, 0.6322F, -0.27579F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.05383F, 0.05383F, 0.05383F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Root",
                    localPos = new Vector3(0.53893F, -0.16848F, 0.68904F),
                    localAngles = new Vector3(275.9082F, 269.7766F, 180.526F),
                    localScale = new Vector3(0.14648F, 0.14648F, 0.14648F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.01511F, 0.03003F, 0.03958F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.00216F, 0.00216F, 0.00216F)
                }
            });

            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += ApplyMagnetism;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += AddLevitationComponent;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += RemoveLevitationComponent;
        }

        private void ApplyMagnetism(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if(!NetworkServer.active || !damageInfo.attacker) return;

            var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            var victimBody = victim?.GetComponent<CharacterBody>();

            if(!attackerBody || !victimBody) return;

            int count = GetCount(attackerBody);
            if(count <= 0) return;

            if(victimBody.HasBuff(ZeroGBuff))
            {
                var controller = victimBody.GetComponent<ZeroGController>();
                if(controller && controller.CanBePulled())
                {
                    PullEnemy(attackerBody, victimBody, count);
                    controller.OnPulled();
                }
            }
            else
            {
                if(!AffectsBosses && victimBody.isBoss) return;

                victimBody.AddBuff(MagneticChargeBuff);

                if(victimBody.GetBuffCount(MagneticChargeBuff) >= HitsRequiredToLevitate)
                {
                    victimBody.ClearTimedBuffs(MagneticChargeBuff);

                    float duration = LevitationDuration + (LevitationDuration * (count - 1));

                    if(victimBody.isBoss)
                    {
                        duration *= 0.5f;
                    }

                    victimBody.AddTimedBuff(ZeroGBuff, duration);
                }
            }
        }

        private void PullEnemy(CharacterBody attacker, CharacterBody victim, int itemCount)
        {
            if(!attacker || !victim) return;

            Vector3 pullVector = attacker.corePosition - victim.corePosition;
            float distance = pullVector.magnitude;

            if(distance > MaxPullDistance || distance < 1f) return;

            Vector3 direction = pullVector.normalized;

            float attackerMass = 100f;    
            if(attacker.characterMotor) attackerMass = attacker.characterMotor.mass;
            else if(attacker.rigidbody) attackerMass = attacker.rigidbody.mass;

            float victimMass = 100f;   
            if(victim.characterMotor) victimMass = victim.characterMotor.mass;
            else if(victim.rigidbody) victimMass = victim.rigidbody.mass;

            if(victim.isBoss)
            {
                victimMass *= BossMassMultiplier;
            }

            float totalMass = attackerMass + victimMass;

            float victimRatio = attackerMass / totalMass;       
            float attackerRatio = victimMass / totalMass;        

            float pullMultiplier = 1f + (itemCount - 1) * PullStrengthIncrease;     
            float selfPullMultiplier = 1f / (1f + (itemCount - 1) * SelfPullReduction);     

            float distanceFactor = Mathf.Clamp01(distance / MaxPullDistance);
            float velocityMultiplier = Mathf.Lerp(0.4f, 1f, distanceFactor);

            float finalPullForce = BasePullForce * pullMultiplier * velocityMultiplier;

            Vector3 victimForce = direction * finalPullForce * victimRatio;
            Vector3 attackerForce = -direction * finalPullForce * attackerRatio * selfPullMultiplier;

            if(victim.healthComponent)
            {
                DamageInfo victimPull = new DamageInfo
                {
                    attacker = attacker.gameObject,
                    damage = 0,
                    force = victimForce,
                    position = victim.corePosition
                };
                victim.healthComponent.TakeDamageForce(victimPull, true, true);
            }

            if(attacker.characterMotor)
            {
                var pullTracker = attacker.GetComponent<MagneticPullTracker>();
                if(!pullTracker)
                {
                    pullTracker = attacker.gameObject.AddComponent<MagneticPullTracker>();
                }
                pullTracker.StartPull(attacker.characterMotor, AirControlDuringPull);

                attacker.characterMotor.ApplyForce(attackerForce, false, false);
            }
            else if(attacker.rigidbody)
            {
                attacker.rigidbody.AddForce(attackerForce, ForceMode.Impulse);
            }
        }

        private void AddLevitationComponent(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            if(buffDef == ZeroGBuff)
            {
                var controller = self.gameObject.AddComponent<ZeroGController>();
                controller.body = self;
            }
        }

        private void RemoveLevitationComponent(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            if(buffDef == ZeroGBuff)
            {
                var controller = self.GetComponent<ZeroGController>();
                if(controller)
                {
                    UnityEngine.Object.Destroy(controller);
                }
            }
        }

        public class MagneticPullTracker : MonoBehaviour
        {
            private float pullDuration = 0.5f;
            private float pullTimer = 0f;
            private CharacterMotor motor;
            private float originalAirControl;
            private float targetAirControl;

            public void StartPull(CharacterMotor characterMotor, float airControlMultiplier)
            {
                if(!motor)
                {
                    motor = characterMotor;
                    originalAirControl = motor.airControl;
                }

                targetAirControl = originalAirControl * airControlMultiplier;
                motor.airControl = targetAirControl;
                pullTimer = pullDuration;
            }

            public void FixedUpdate()
            {
                if(pullTimer > 0f)
                {
                    pullTimer -= Time.fixedDeltaTime;

                    if(pullTimer <= 0f && motor)
                    {
                        motor.airControl = originalAirControl;
                    }
                }
            }

            public void OnDestroy()
            {
                if(motor)
                {
                    motor.airControl = originalAirControl;
                }
            }
        }

        public class ZeroGController : MonoBehaviour
        {
            public CharacterBody body;

            private float lastPullTime = -999f;

            public float HoverHeight = 4.0f;
            public float SpringStrength = 30.0f;
            public float DampingStrength = 5.0f;

            private bool wasGravityEnabled;
            private float originalAirControl;

            private CharacterMotor rorMotor;
            private Rigidbody rb;

            public bool CanBePulled()
            {
                return Time.time - lastPullTime >= AlienMagnet.PullCooldownPerEnemy;
            }

            public void OnPulled()
            {
                lastPullTime = Time.time;
            }

            public void Start()
            {
                if(!body) return;

                rorMotor = body.characterMotor;
                rb = body.rigidbody;

                if(rorMotor)
                {
                    wasGravityEnabled = rorMotor.useGravity;
                    originalAirControl = rorMotor.airControl;

                    rorMotor.useGravity = false;
                    rorMotor.airControl = 0.05f;

                    if(rorMotor.isGrounded)
                    {
                        rorMotor.Motor.ForceUnground();
                        rorMotor.velocity.y = 10f;
                    }
                }
                else if(rb)
                {
                    wasGravityEnabled = rb.useGravity;
                    rb.useGravity = false;
                }
            }

            public void FixedUpdate()
            {
                if(!body) return;

                Vector3 rayOrigin = body.footPosition + (Vector3.up * 1.0f);
                float currentVelY = 0f;
                float distToGround = 0f;
                bool groundFound = false;

                if(rorMotor) currentVelY = rorMotor.velocity.y;
                else if(rb) currentVelY = rb.velocity.y;

                RaycastHit hit;
                if(Physics.Raycast(rayOrigin, Vector3.down, out hit, 50f, LayerIndex.world.mask))
                {
                    distToGround = hit.distance - 1.0f;
                    groundFound = true;
                }

                if(groundFound)
                {
                    float heightError = HoverHeight - distToGround;

                    float massFactor = (rb ? rb.mass : (rorMotor ? rorMotor.mass : 1f));
                    massFactor = Mathf.Clamp(massFactor / 50f, 1f, 5f);

                    float targetAccelY = ((heightError * SpringStrength) - (currentVelY * DampingStrength)) * massFactor;

                    if(rorMotor)
                    {
                        rorMotor.velocity.y += targetAccelY * Time.fixedDeltaTime;

                        if(rorMotor.isGrounded && targetAccelY > 5f)
                        {
                            rorMotor.Motor.ForceUnground();
                        }

                        rorMotor.useGravity = false;
                    }
                    else if(rb)
                    {
                        Vector3 force = Vector3.up * targetAccelY * rb.mass;
                        rb.AddForce(force, ForceMode.Force);
                    }
                }
                else
                {
                    if(rorMotor)
                    {
                        rorMotor.velocity.y = Mathf.MoveTowards(rorMotor.velocity.y, 0f, Time.fixedDeltaTime * 10f);
                    }
                    else if(rb)
                    {
                        rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(rb.velocity.x, 0, rb.velocity.z), Time.fixedDeltaTime);
                    }
                }
            }

            public void OnDestroy()
            {
                if(rorMotor)
                {
                    rorMotor.useGravity = wasGravityEnabled;
                    rorMotor.airControl = originalAirControl;
                }
                else if(rb)
                {
                    rb.useGravity = wasGravityEnabled;
                }
            }
        }
    }
}