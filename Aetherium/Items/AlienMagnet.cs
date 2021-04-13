using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;

namespace Aetherium.Items
{
    public class AlienMagnet : ItemBase<AlienMagnet>
    {
        public ConfigOption<float> TimeToDecayLiftStacks;
        public ConfigOption<float> BaseDurationOfLevitationDebuff;
        public ConfigOption<float> AdditionalDurationOfLevitationDebuff;
        public ConfigOption<bool> UseOldAlienMagnet;
        public ConfigOption<float> StartingForceMultiplier;
        public ConfigOption<float> AdditionalForceMultiplier;
        public ConfigOption<float> MinimumForceMultiplier;
        public ConfigOption<float> MaximumForceMultiplier;

        public override string ItemName => "Alien Magnet";
        public override string ItemLangTokenName => "ALIEN_MAGNET";
        public override string ItemPickupDesc => "Your attacks pull enemies towards you.";
        public override string ItemFullDescription => $"Enemies hit by your attacks will be pulled towards you, starting at {StartingForceMultiplier}x force <style=cStack>(+{AdditionalForceMultiplier}x force multiplier, up to {MaximumForceMultiplier}x total force. The effect is more noticeable on higher health enemies.)</style>";
        public override string ItemLore => "A strange pylon that seems to bring enemies towards the wielder when their attacks hit. Only the truly brave or insane bring the fight to themselves.";

        public override ItemTier Tier => ItemTier.Lunar;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.Cleansable };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("AlienMagnet.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("AlienMagnetIcon.png");

        public override bool CanRemove => false;

        public static BuffIndex LiftStackDebuff;
        public static Sprite LiftStackDebuffIcon;
        public static BuffIndex LevitationDebuff;

        public static GameObject ItemBodyModelPrefab;
        public static GameObject ItemFollowerPrefab;

        public AlienMagnet()
        {
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateAdditionalAssets();
            CreateBuff();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            TimeToDecayLiftStacks = config.ActiveBind<float>("Item: " + ItemName, "Time Until Lift Stack Debuff Decays", 4f, "Time until a Lift stack removes itself from the victim.");
            BaseDurationOfLevitationDebuff = config.ActiveBind<float>("Item: " + ItemName, "Base Duration of Levitation Debuff", 15f, "How long should the duration of the Levitation debuff be?");
            AdditionalDurationOfLevitationDebuff = config.ActiveBind<float>("Item: " + ItemName, "Additional Duration of Levitation Debuff per Alien Magnet Stack", 5f, "How long should each additional Alien Magnet grant the levitation debuff duration?");
            UseOldAlienMagnet = config.ActiveBind<bool>("Item: " + ItemName, "Use Old Alien Magnet Functionality", false, "Should we use the old functionality of wonky pulling?");
            StartingForceMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Starting Pull Force Multiplier", 3f, "What should the starting pull force multiplier of the Alien Magnet's pull be?");
            AdditionalForceMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Additional Pull Force Multiplier per Stack", 2f, "How much additional force multiplier should be granted per Alien Magnet stack?");
            MinimumForceMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Minimum Pull Force Multiplier", 3f, "What should the minimum force multiplier be for the Alien Magnet?");
            MaximumForceMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Maximum Pull Force Multiplier", 10f, "What should the maximum force multiplier be for the Alien Magnet?");
        }

        private void CreateAdditionalAssets()
        {

        }

        private void CreateBuff()
        {
            var liftStackDebuff = new RoR2.BuffDef
            {
                name = "Aetherium: Alien Magnet Lift Stacks Debuff",
                buffColor = new Color(255, 255, 255),
                isDebuff = true,
                canStack = true,
                iconPath = "@Aetherium:Assets/Textures/Icons/Buff/AlienMagnetLiftDebuffIcon.png"
            };
            LiftStackDebuff = BuffAPI.Add(new CustomBuff(liftStackDebuff));

            var levitationDebuff = new RoR2.BuffDef
            {
                name = "Aetherium: Alien Magnet Levitation Debuff",
                buffColor = new Color(255, 255, 255),
                isDebuff = true,
                canStack = false,
                iconPath = "@Aetherium:Assets/Textures/Icons/Buff/AlienMagnetLevitationDebuffIcon.png"
            };
            LevitationDebuff = BuffAPI.Add(new CustomBuff(levitationDebuff));
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/Item/AlienMagnet/AlienMagnetTracker.prefab");
            ItemFollowerPrefab = Resources.Load<GameObject>(ItemModelPath);

            var ItemFollower = ItemBodyModelPrefab.AddComponent<ItemFollowerSmooth>();
            ItemFollower.itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemFollower.itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);
            ItemFollower.followerPrefab = ItemFollowerPrefab;
            ItemFollower.targetObject = ItemBodyModelPrefab;
            ItemFollower.distanceDampTime = 0.10f;
            ItemFollower.distanceMaxSpeed = 100;
            ItemFollower.SmoothingNumber = 0.25f;

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(0f, 0f, 0f),
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
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            //ruleLookup.Add("mdlHuntress", 0.1f);
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-4f, -2f, 5f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
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
                    localAngles = new Vector3(0f, 0f, 0f),
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
                    localAngles = new Vector3(0f, 0f, 0f),
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
                    localAngles = new Vector3(0f, 0f, 0f),
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
                    localAngles = new Vector3(0f, 0f, 0f),
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
                    localPos = new Vector3(5f, 0f, 10f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.5f, 0f, -1f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += GetOverHere;
            On.RoR2.CharacterBody.FixedUpdate += LevitationDebuffManager;
        }

        private void LevitationDebuffManager(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (self.HasBuff(LevitationDebuff))
            {
                var heightConstant = 10;
                var rayHit = MiscUtils.RaycastToFloor(self.footPosition, heightConstant * 2);
                if(rayHit != null)
                {
                    var magnitude = (self.footPosition - rayHit.Value).magnitude;
                    var speed = ((1 - (magnitude / heightConstant)) * 3) - Physics.gravity.y;
                    var characterMotor = self.GetComponent<CharacterMotor>();
                    var rigidbody = self.rigidbody;

                    if (characterMotor)
                    {
                        characterMotor.Motor.ForceUnground();
                        characterMotor.velocity += Vector3.up * speed * Time.deltaTime;
                    }
                    else if (rigidbody)
                    {
                        rigidbody.velocity += Vector3.up * speed * Time.deltaTime;
                    }
                }
            }
            orig(self);
        }

        private void GetOverHere(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            if (UseOldAlienMagnet)
            {
                if (damageInfo?.attacker)
                {
                    var attackerBody = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
                    if (attackerBody)
                    {
                        int ItemCount = GetCount(attackerBody);
                        if (ItemCount > 0)
                        {
                            //Thanks Chen for fixing this.
                            float mass;
                            if (self.body.characterMotor) mass = self.body.characterMotor.mass;
                            else if (self.body.rigidbody) mass = self.body.rigidbody.mass;
                            else mass = 1f;

                            var forceCalc = Mathf.Clamp(StartingForceMultiplier + (AdditionalForceMultiplier * (ItemCount - 1)), MinimumForceMultiplier, MaximumForceMultiplier);
                            damageInfo.force += Vector3.Normalize(attackerBody.corePosition - self.body.corePosition) * forceCalc * mass;
                        }
                    }
                }
            }
            else
            {
                if (damageInfo?.attacker)
                {
                    var attackerBody = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
                    var body = self.body;

                    if (attackerBody && body)
                    {
                        var inventoryCount = GetCount(attackerBody);

                        if(inventoryCount > 0)
                        {
                            var mass = body.rigidbody.mass;

                            int AmountOfStacksToLift = (int) (mass * (0.05f));

                            AmountOfStacksToLift = Mathf.Clamp(AmountOfStacksToLift - (inventoryCount - 1), 1, int.MaxValue);
                            if (!body.HasBuff(LevitationDebuff))
                            {
                                if (body.GetBuffCount(LiftStackDebuff) < AmountOfStacksToLift)
                                {
                                    if (!body.HasBuff(LevitationDebuff))
                                    {
                                        var liftAmountCurrentlyOnBody = body.GetBuffCount(LiftStackDebuff);
                                        if (liftAmountCurrentlyOnBody > 0)
                                        {
                                            foreach(var buff in body.timedBuffs)
                                            {
                                                if(buff.buffIndex == LiftStackDebuff)
                                                {
                                                    buff.timer = TimeToDecayLiftStacks;
                                                }
                                            }
                                        }
                                        body.AddTimedBuff(LiftStackDebuff, TimeToDecayLiftStacks);
                                    }
                                }
                                else if (body.GetBuffCount(LiftStackDebuff) >= AmountOfStacksToLift)
                                {
                                    body.ClearTimedBuffs(LiftStackDebuff);
                                    body.AddTimedBuff(LevitationDebuff, BaseDurationOfLevitationDebuff + (AdditionalDurationOfLevitationDebuff * (inventoryCount - 1)));
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                }
            }
            orig(self, damageInfo);
        }

        public class AlienMagnetGravityComponent : MonoBehaviour
        {
            public float OriginalMass;
            public float OriginalDrag;
        }
    }
}