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
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Utils.MiscUtils;
using RoR2.CharacterAI;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
using R2API.Networking;
using Aetherium.Utils;

namespace Aetherium.Equipment.EliteEquipment
{
    public class AffixSanguine : EliteEquipmentBase<AffixSanguine>
    {
        public static ConfigOption<float> BlinkStateDuration;
        public static ConfigOption<float> BlinkDistance;
        public static ConfigOption<float> CostMultiplierOfElite;
        public static ConfigOption<float> DamageMultiplierOfElite;
        public static ConfigOption<float> HealthMultiplierOfElite;
        public static ConfigOption<float> ForcedDurationBetweenPlayerBlinks;

        public override string EliteEquipmentName => "Bloody Fealty";

        public override string EliteAffixToken => "AFFIX_SANGUINE";

        public override string EliteEquipmentPickupDesc => "Become an aspect of the red plane.";

        public override string EliteEquipmentFullDescription => $"On use, teleport dash up to <style=cIsUtility>{BlinkDistance}m</style> in the direction you're going and gain slight invulnerability during the dash. Additionally, all of your attacks now cause <style=cDeath>bleeding</style>.";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Sanguine";

        public override GameObject EliteEquipmentModel => MainAssets.LoadAsset<GameObject>("PickupAffixAbyssal");
        public override Material EliteMaterial { get; set; } = MainAssets.LoadAsset<Material>("AffixAbyssalMat");

        public override Sprite EliteEquipmentIcon => MainAssets.LoadAsset<Sprite>("AffixAbyssalIcon.png");

        public override Sprite EliteBuffIcon => MainAssets.LoadAsset<Sprite>("AffixAbyssalBuffIcon.png");

        public override Color EliteBuffColor => new Color(195, 33, 72, 255);

        public static GameObject ItemBodyModelPrefab;

        public override float Cooldown => 2;

        public HashSet<string> NoAbyssalControllerForTheseBodies = new HashSet<string>()
        {
            "GrandParentBody",
            "EngiTurretBody",
            "MiniMushroomBody"
        };

        public HashSet<string> NoOverdriveForTheseBodies = new HashSet<string>
        {
            "HermitCrabBody",
            "VagrantBody",
            "SuperRoboBallBossBody",
            "RoboBallBossBody"
        };

        public static HashSet<Type> EntityStateBlacklist = new HashSet<Type>()
        {
            typeof(EntityStates.BrotherMonster.SpellBaseState),
            typeof(EntityStates.BrotherMonster.SpellChannelEnterState),
            typeof(EntityStates.BrotherMonster.SpellChannelExitState),
            typeof(EntityStates.BrotherMonster.SpellChannelState),
            typeof(EntityStates.BrotherMonster.FistSlam)
        };

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            RegisterEntityState();
            CreateNetworking();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            BlinkStateDuration = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Duration of the Abyssal Dash State", 0.2f, "How long (in second(s)) should it take for the abyssal dash of this equipment to fully complete?");
            BlinkDistance = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Max Distance to Cover in a Single Abyssal Dash", 10f, "How far out should the Abyssal Dash check for a node to teleport towards?");
            CostMultiplierOfElite = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Cost Multiplier of Elite", 2.5f, "How many times higher than the base elite cost should the cost of this elite be? (Do not set this to 0, only warning haha.)");
            DamageMultiplierOfElite = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Damage Multiplier of Elite", 1f, "How many times higher than the base elite damage boost should the damage of this elite be?");
            HealthMultiplierOfElite = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Health Multiplier of Elite", 2f, "How many times higher than the base elite health boost should the health of this elite be?");
            ForcedDurationBetweenPlayerBlinks = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Forced Cooldown Duration Between Player Blinks", 2f, "What should the duration of forced cooldown for the abyssal dash ability be for players?");
        }

        private void CreateEliteTiers()
        {
            CanAppearInEliteTiers = new CombatDirector.EliteTierDef[]
            {
                new CombatDirector.EliteTierDef()
                {
                    costMultiplier = CombatDirector.baseEliteCostMultiplier * CostMultiplierOfElite,
                    damageBoostCoefficient = CombatDirector.baseEliteDamageBoostCoefficient * DamageMultiplierOfElite,
                    healthBoostCoefficient = CombatDirector.baseEliteHealthBoostCoefficient * HealthMultiplierOfElite,
                    eliteTypes = Array.Empty<EliteDef>(),
                    isAvailable = SetAvailability
                }
            };
        }

        private bool SetAvailability(SpawnCard.EliteRules arg)
        {
            return Run.instance.ambientLevel >= 10 && arg == SpawnCard.EliteRules.Default;
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
                    childName = "Head",
                    localPos = new Vector3(0.11358F, 0.30196F, -0.03545F),
                    localAngles = new Vector3(345F, 350F, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.11358F, 0.30196F, -0.03545F),
                    localAngles = new Vector3(345F, 10F, 0F),
                    localScale = new Vector3(-6F, 6F, 6F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.08291F, 0.15131F, -0.00038F),
                    localAngles = new Vector3(345F, 350F, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.08291F, 0.15131F, -0.00038F),
                    localAngles = new Vector3(345F, 10F, 0F),
                    localScale = new Vector3(-6F, 6F, 6F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-1.08295F, 1.56853F, 0.61364F),
                    localAngles = new Vector3(310F, 180F, 0F),
                    localScale = new Vector3(40F, 40F, 40F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(1.08295F, 1.56853F, 0.61364F),
                    localAngles = new Vector3(310F, 180F, 3.16676F),
                    localScale = new Vector3(-40F, 40F, 40F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0.08293F, 0.13092F, -0.00368F),
                    localAngles = new Vector3(345F, 350F, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(-0.08293F, 0.13092F, -0.00368F),
                    localAngles = new Vector3(345F, 10F, 0F),
                    localScale = new Vector3(-6F, 6F, 6F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.07638F, 0.08456F, -0.0278F),
                    localAngles = new Vector3(345F, 350F, 0F),
                    localScale = new Vector3(4F, 4F, 4F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.07638F, 0.08456F, -0.0278F),
                    localAngles = new Vector3(345F, 10F, 0F),
                    localScale = new Vector3(-4F, 4F, 4F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.08291F, 0.15131F, -0.00038F),
                    localAngles = new Vector3(345F, 350F, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.08291F, 0.15131F, -0.00038F),
                    localAngles = new Vector3(345F, 10F, 0F),
                    localScale = new Vector3(-6F, 6F, 6F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0.51618F, 0.52009F, -0.39326F),
                    localAngles = new Vector3(22.5F, 0F, 0F),
                    localScale = new Vector3(20F, 20F, 20F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(-0.51618F, 0.52009F, -0.39326F),
                    localAngles = new Vector3(22.5F, 0F, 0F),
                    localScale = new Vector3(-20F, 20F, 20F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.08291F, 0.15131F, -0.00038F),
                    localAngles = new Vector3(345F, 350F, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.08291F, 0.15131F, -0.00038F),
                    localAngles = new Vector3(345F, 10F, 0F),
                    localScale = new Vector3(-6F, 6F, 6F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.95548F, 0.76718F, 1.31338F),
                    localAngles = new Vector3(305F, 0.00001F, 180F),
                    localScale = new Vector3(40F, 40F, 40F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.95548F, 0.76718F, 1.31338F),
                    localAngles = new Vector3(305F, 0.00001F, 180F),
                    localScale = new Vector3(-40F, 40F, 40F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.08291F, 0.15131F, -0.00038F),
                    localAngles = new Vector3(345F, 350F, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.08291F, 0.15131F, -0.00038F),
                    localAngles = new Vector3(345F, 10F, 0F),
                    localScale = new Vector3(-6F, 6F, 6F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.08053F, 0.10021F, 0.01311F),
                    localAngles = new Vector3(345F, 350F, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.08053F, 0.10021F, 0.01311F),
                    localAngles = new Vector3(345F, 10F, 0F),
                    localScale = new Vector3(-6F, 6F, 6F)
                }
            });
            rules.Add("mdlBeetle", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.38917F, 0.38536F, -0.06125F),
                    localAngles = new Vector3(337.5F, 180F, 0F),
                    localScale = new Vector3(12F, 12F, 12F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.38917F, 0.38536F, -0.06125F),
                    localAngles = new Vector3(337.5F, 180F, 0F),
                    localScale = new Vector3(-12F, 12F, 12F)
                }
            });
            rules.Add("mdlBeetleGuard", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.64273F, 0.49881F, 0.22102F),
                    localAngles = new Vector3(300F, 180F, 0F),
                    localScale = new Vector3(36F, 36F, 36F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.64273F, 0.49881F, 0.22102F),
                    localAngles = new Vector3(300F, 180F, 0F),
                    localScale = new Vector3(-36F, 36F, 36F)
                }
            });
            rules.Add("mdlBeetleQueen", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-1.62821F, 2.00708F, -0.06132F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(60F, 60F, 60F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(1.62821F, 2.00708F, -0.06132F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(-60F, 60F, 60F)
                }
            });
            rules.Add("mdlBell", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ShieldR",
                    localPos = new Vector3(-0.15542F, -0.53686F, 0.17545F),
                    localAngles = new Vector3(0F, 0F, 180F),
                    localScale = new Vector3(36F, 36F, 36F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ShieldL",
                    localPos = new Vector3(0.15542F, -0.53686F, 0.17545F),
                    localAngles = new Vector3(0F, 0F, 180F),
                    localScale = new Vector3(-36F, 36F, 36F)
                }
            });
            rules.Add("mdlBison", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.37546F, 0.22873F, 0.0157F),
                    localAngles = new Vector3(315F, 180F, 0F),
                    localScale = new Vector3(24F, 24F, 24F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.37546F, 0.22873F, 0.0157F),
                    localAngles = new Vector3(315F, 180F, 0F),
                    localScale = new Vector3(-24F, 24F, 24F)
                }
            });
            rules.Add("mdlBrother", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.11815F, 0.06693F, 0.07572F),
                    localAngles = new Vector3(45F, 0F, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.11815F, 0.06693F, 0.07572F),
                    localAngles = new Vector3(45F, 0F, 0F),
                    localScale = new Vector3(-6F, 6F, 6F)
                }
            });
            rules.Add("mdlClayBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "PotLidTop",
                    localPos = new Vector3(1.21543F, 0.00426F, 1.32384F),
                    localAngles = new Vector3(337.5F, 0F, 0F),
                    localScale = new Vector3(48F, 48F, 48F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "PotLidTop",
                    localPos = new Vector3(-1.21543F, 0.00426F, 1.32384F),
                    localAngles = new Vector3(337.5F, 0F, 0F),
                    localScale = new Vector3(-48F, 48F, 48F)
                }
            });
            rules.Add("mdlClayBruiser", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.21046F, 0.17845F, 0.11614F),
                    localAngles = new Vector3(337.5F, 0F, 0F),
                    localScale = new Vector3(12F, 12F, 12F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.21046F, 0.17845F, 0.11614F),
                    localAngles = new Vector3(337.5F, 0F, 0F),
                    localScale = new Vector3(-12F, 12F, 12F)
                }
            });
            rules.Add("mdlMagmaWorm", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.4459F, 0.38536F, -0.06125F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(24F, 24F, 24F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.4459F, 0.38536F, -0.06125F),
                    localAngles = new Vector3(90F, 0F, 0F),
                    localScale = new Vector3(-24F, 24F, 24F)
                }
            });
            rules.Add("mdlGolem", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.45874F, 0.86819F, -0.06124F),
                    localAngles = new Vector3(337.5F, 0F, 0F),
                    localScale = new Vector3(20F, 20F, 20F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.45874F, 0.86819F, -0.06124F),
                    localAngles = new Vector3(337.5F, 0F, 0F),
                    localScale = new Vector3(-20F, 20F, 20F)
                }
            });
            rules.Add("mdlGrandparent", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.99862F, -0.5785F, -1.48208F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(120F, 120F, 120F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.99862F, -0.5785F, -1.48208F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(-120F, 120F, 120F)
                }
            });
            rules.Add("mdlGravekeeper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.66876F, 0.90583F, -0.15146F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(60F, 60F, 60F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.66876F, 0.90583F, -0.15146F),
                    localAngles = new Vector3(10F, 0F, 0F),
                    localScale = new Vector3(-60F, 60F, 60F)
                }
            });
            rules.Add("mdlGreaterWisp", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MaskBase",
                    localPos = new Vector3(0.61798F, 0.56288F, 0.36733F),
                    localAngles = new Vector3(10F, 0F, 340F),
                    localScale = new Vector3(24F, 24F, 24F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MaskBase",
                    localPos = new Vector3(-0.61798F, 0.56288F, 0.36733F),
                    localAngles = new Vector3(10F, 0F, 20F),
                    localScale = new Vector3(-24F, 24F, 24F)
                }
            });
            rules.Add("mdlHermitCrab", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.10288F, 0.38539F, 0.32161F),
                    localAngles = new Vector3(0F, 315.7086F, 0F),
                    localScale = new Vector3(24F, 24F, 24F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-0.38689F, 0.38539F, 0.01365F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(-24F, 24F, 24F)
                }
            });
            rules.Add("mdlImp", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Neck",
                    localPos = new Vector3(-0.07728F, -0.00684F, -0.03139F),
                    localAngles = new Vector3(22.5F, 180F, 0F),
                    localScale = new Vector3(6F, 6F, 6F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Neck",
                    localPos = new Vector3(0.07728F, 0.03594F, -0.06488F),
                    localAngles = new Vector3(22.5F, 180F, 0F),
                    localScale = new Vector3(-6F, 6F, 6F)
                }
            });
            rules.Add("mdlImpBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Neck",
                    localPos = new Vector3(-0.30381F, -0.40225F, -0.41471F),
                    localAngles = new Vector3(60F, 180F, 0F),
                    localScale = new Vector3(48F, 48F, 48F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Neck",
                    localPos = new Vector3(0.30381F, -0.40225F, -0.41471F),
                    localAngles = new Vector3(60F, 180F, 0F),
                    localScale = new Vector3(-48F, 48F, 48F)
                }
            });
            rules.Add("mdlJellyfish", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hull2",
                    localPos = new Vector3(0.85484F, 0.50523F, 0.19185F),
                    localAngles = new Vector3(10F, 0F, 340F),
                    localScale = new Vector3(24F, 24F, 24F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hull2",
                    localPos = new Vector3(-0.85484F, 0.50523F, 0.19185F),
                    localAngles = new Vector3(10F, 0F, 20F),
                    localScale = new Vector3(-24F, 24F, 24F)
                }
            });
            rules.Add("mdlLemurian", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.78298F, 1.21665F, -0.43465F),
                    localAngles = new Vector3(290F, 0F, 0F),
                    localScale = new Vector3(48F, 48F, 48F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.78298F, 1.21665F, -0.43465F),
                    localAngles = new Vector3(290F, 0F, 0F),
                    localScale = new Vector3(-48F, 48F, 48F)
                }
            });
            rules.Add("mdlLemurianBruiser", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.91089F, 0.38538F, -0.06132F),
                    localAngles = new Vector3(290F, 180F, 0F),
                    localScale = new Vector3(60F, 60F, 60F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.91089F, 0.38538F, -0.06132F),
                    localAngles = new Vector3(290F, 180F, 0F),
                    localScale = new Vector3(-60F, 60F, 60F)
                }
            });
            rules.Add("mdlMiniMushroom", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.0704F, -0.28646F, 0.76564F),
                    localAngles = new Vector3(90F, 270F, 0F),
                    localScale = new Vector3(24F, 24F, 24F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.0704F, -0.28646F, -0.76564F),
                    localAngles = new Vector3(90F, 270F, 0F),
                    localScale = new Vector3(-24F, 24F, 24F)
                }
            });
            rules.Add("mdlNullifier", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Muzzle",
                    localPos = new Vector3(0.71386F, 0.84448F, 0.26824F),
                    localAngles = new Vector3(45.00001F, 0F, 340F),
                    localScale = new Vector3(36F, 36F, 36F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Muzzle",
                    localPos = new Vector3(-0.71386F, 0.84448F, 0.26824F),
                    localAngles = new Vector3(45.00001F, 0F, 20F),
                    localScale = new Vector3(-36F, 36F, 36F)
                }
            });
            rules.Add("mdlParent", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-23.3459F, 59.561F, -42.465F),
                    localAngles = new Vector3(345.8339F, 90F, 350F),
                    localScale = new Vector3(2000F, 2000F, 2000F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-23.3459F, 59.561F, 42.465F),
                    localAngles = new Vector3(345.8339F, 90F, 10F),
                    localScale = new Vector3(-2000F, 2000F, 2000F)
                }
            });
            rules.Add("mdlRoboBallBoss", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MainEyeMuzzle",
                    localPos = new Vector3(0.53861F, 0.27738F, -0.39097F),
                    localAngles = new Vector3(70.00002F, 0.00001F, 0.00001F),
                    localScale = new Vector3(12F, 12F, 12F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MainEyeMuzzle",
                    localPos = new Vector3(-0.53861F, 0.27738F, -0.39097F),
                    localAngles = new Vector3(70.00002F, 5.00895F, 5.00896F),
                    localScale = new Vector3(-12F, 12F, 12F)
                }
            });
            rules.Add("mdlRoboBallMini", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Muzzle",
                    localPos = new Vector3(0.60576F, 0.29241F, -0.57041F),
                    localAngles = new Vector3(45F, 0F, 0F),
                    localScale = new Vector3(12F, 12F, 12F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Muzzle",
                    localPos = new Vector3(-0.60576F, 0.29241F, -0.57041F),
                    localAngles = new Vector3(45F, 0F, 0F),
                    localScale = new Vector3(-12F, 12F, 12F)
                }
            });
            rules.Add("mdlScav", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-2.26237F, 5.07269F, -2.12799F),
                    localAngles = new Vector3(337.5F, 180F, 0F),
                    localScale = new Vector3(180F, 180F, 180F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(2.26237F, 5.07269F, -2.12799F),
                    localAngles = new Vector3(337.5F, 180F, 0F),
                    localScale = new Vector3(-180F, 180F, 180F)
                }
            });
            rules.Add("mdlTitan", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(1.23464F, 2.94563F, 0.86964F),
                    localAngles = new Vector3(45F, 0F, 340F),
                    localScale = new Vector3(60F, 60F, 60F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-1.23464F, 2.94563F, 0.86964F),
                    localAngles = new Vector3(45.00001F, 6.03709F, 20F),
                    localScale = new Vector3(-60F, 60F, 60F)
                }
            });
            rules.Add("mdlVagrant", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hull",
                    localPos = new Vector3(0.80985F, 0.71921F, -0.06125F),
                    localAngles = new Vector3(45.00001F, 0F, 340F),
                    localScale = new Vector3(24F, 24F, 24F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hull",
                    localPos = new Vector3(-0.80985F, 0.71921F, -0.06125F),
                    localAngles = new Vector3(45.00002F, 0F, 20F),
                    localScale = new Vector3(-24F, 24F, 24F)
                }
            });
            rules.Add("mdlVulture", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.72263F, 0.38541F, -0.06115F),
                    localAngles = new Vector3(337.5F, 0F, 0F),
                    localScale = new Vector3(48F, 48F, 48F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.72263F, 0.38541F, -0.06115F),
                    localAngles = new Vector3(337.5F, 0F, 0F),
                    localScale = new Vector3(-48F, 48F, 48F)
                }
            });
            rules.Add("mdlWisp1Mouth", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.38917F, 0.13952F, 0.11711F),
                    localAngles = new Vector3(337.5F, 180F, 0F),
                    localScale = new Vector3(12F, 12F, 12F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.38917F, 0.13952F, 0.11711F),
                    localAngles = new Vector3(337.5F, 180F, 0F),
                    localScale = new Vector3(-12F, 12F, 12F)
                }
            });
            return rules;
        }

        private void RegisterEntityState()
        {
            LoadoutAPI.StateTypeOf<MyEntityStates.AbyssalDash>();
        }

        private void CreateNetworking()
        {
            NetworkingAPI.RegisterMessageType<AbyssalDashMessage>();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnEquipmentGained += GiveSanguineController;
            On.RoR2.CharacterBody.OnEquipmentLost += RemoveSanguineController;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += GiveSanguineControllerOnBuff;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += RemoveSanguineControllerOnBuff;
            On.RoR2.GlobalEventManager.OnHitEnemy += BleedOnHit;
        }


        private void BleedOnHit(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.attacker && !damageInfo.rejected)
            {
                var body = damageInfo.attacker.GetComponent<CharacterBody>();
                if (body && body.inventory && (body.inventory.currentEquipmentIndex == EliteEquipmentDef.equipmentIndex || body.inventory.alternateEquipmentIndex == EliteEquipmentDef.equipmentIndex || body.HasBuff(EliteBuffDef)))
                {
                    if(!damageInfo.damageType.HasFlag(DamageType.BleedOnHit))
                    {
                        damageInfo.damageType |= DamageType.BleedOnHit;
                    }

                }
            }
            orig(self, damageInfo, victim);
        }

        private void GiveSanguineController(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);
            if(equipmentDef == EliteEquipmentDef && !self.isPlayerControlled)
            {
                var abyssalController = self.GetComponent<SanguineController>();
                if (!abyssalController && !NoAbyssalControllerForTheseBodies.Any(x => self.name.Contains(x)))
                {
                    abyssalController = self.gameObject.AddComponent<SanguineController>();
                }
            }
        }

        private void RemoveSanguineController(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EliteEquipmentDef)
            {
                var abyssalController = self.GetComponent<SanguineController>();
                if (abyssalController)
                {
                    UnityEngine.Object.Destroy(abyssalController);
                }
            }
            orig(self, equipmentDef);
        }

        private void GiveSanguineControllerOnBuff(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == EliteBuffDef && !self.isPlayerControlled)
            {
                var abyssalController = self.GetComponent<SanguineController>();
                if (!abyssalController && !NoAbyssalControllerForTheseBodies.Any(x => self.name.Contains(x)))
                {
                    abyssalController = self.gameObject.AddComponent<SanguineController>();
                }
            }
        }

        private void RemoveSanguineControllerOnBuff(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            if (buffDef == EliteBuffDef)
            {
                var abyssalController = self.GetComponent<SanguineController>();
                if (abyssalController)
                {
                    UnityEngine.Object.Destroy(abyssalController);
                }
            }
            orig(self, buffDef);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            var bodyStateMachine = slot.characterBody.GetComponents<EntityStateMachine>().Where(x => x.customName == "Body").FirstOrDefault();
            if (bodyStateMachine)
            {
                if (NetworkServer.active)
                {
                    var blinkState = new MyEntityStates.AbyssalDash();
                    blinkState.duration = BlinkStateDuration;
                    blinkState.blinkDistance = BlinkDistance;
                    bodyStateMachine.SetInterruptState(blinkState, EntityStates.InterruptPriority.Any);

                    var bodyIdentity = slot.characterBody.gameObject.GetComponent<NetworkIdentity>();
                    if (bodyIdentity)
                    {
                        new AbyssalDashMessage(bodyIdentity.netId).Send(NetworkDestination.Clients);
                    }

                    slot.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, BlinkStateDuration);
                }
                if (slot.subcooldownTimer != Cooldown) { slot.subcooldownTimer = ForcedDurationBetweenPlayerBlinks; }
                return true;
            }
            return false;
        }

        public class AbyssalDashMessage : INetMessage
        {
            private NetworkInstanceId BodyID;

            public AbyssalDashMessage()
            {
            }

            public AbyssalDashMessage(NetworkInstanceId bodyID)
            {
                BodyID = bodyID;
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(BodyID);
            }

            public void Deserialize(NetworkReader reader)
            {
                BodyID = reader.ReadNetworkId();
            }

            public void OnReceived()
            {
                if (NetworkServer.active) return;
                var playerGameObject = RoR2.Util.FindNetworkObject(BodyID);
                if (playerGameObject)
                {
                    var body = playerGameObject.GetComponent<RoR2.CharacterBody>();
                    if (body)
                    {
                        var bodyStateMachine = body.GetComponents<EntityStateMachine>().Where(x => x.customName == "Body").FirstOrDefault();
                        if (bodyStateMachine)
                        {
                            var blinkState = new MyEntityStates.AbyssalDash();
                            blinkState.duration = BlinkStateDuration;
                            blinkState.blinkDistance = BlinkDistance;
                            bodyStateMachine.SetInterruptState(blinkState, EntityStates.InterruptPriority.Any);
                        }
                    }
                }
            }
        }

        public class SanguineController : MonoBehaviour
        {
            public CharacterBody Body;
            public EntityStateMachine BodyStateMachine;
            public float Stopwatch;
            public float TimeBetweenJumps = 2;

            public bool EnableExhaustionAndOverdriveMechanic = true;
            public int ExhaustionStacks;
            public int ExhaustionLimit = 20;
            public float ExhaustionStacksStopwatch;
            public float ExhaustionStackReductionDelay = 0.5f;

            public bool IsFlyer;

            public void Start()
            {
                BodyStateMachine = GetComponents<EntityStateMachine>().Where(x => x.customName == "Body").FirstOrDefault();
                Body = GetComponent<CharacterBody>();
                if (Body)
                {
                    EnableExhaustionAndOverdriveMechanic = !AffixSanguine.instance.NoOverdriveForTheseBodies.Any(x => Body.name.Contains(x));
                }
            }

            public CharacterBody GetCurrentTarget()
            {
                if (Body && Body.master)
                {
                    var aiComponent = Body.master.GetComponent<BaseAI>();
                    if (aiComponent)
                    {                        
                        var enemy = aiComponent.currentEnemy;
                        if (enemy != null)
                        {
                            var enemyBody = enemy.characterBody;
                            if (enemyBody)
                            {
                                return enemyBody;
                            }
                        }
                    }
                }


                return null;
            }

            public void DecrementOverdriveStacksOverTime()
            {
                if (EnableExhaustionAndOverdriveMechanic)
                {
                    if (ExhaustionStacks > ExhaustionLimit || ExhaustionStacks < 0)
                    {
                        ExhaustionStacks = Mathf.Clamp(ExhaustionStacks, 0, ExhaustionLimit);
                    }
                    if (ExhaustionStacks > 0)
                    {
                        ExhaustionStacksStopwatch += Time.fixedDeltaTime;
                        if (ExhaustionStacksStopwatch >= ExhaustionStackReductionDelay)
                        {
                            ExhaustionStacks--;
                            ExhaustionStacksStopwatch = 0;
                        }
                    }
                }
            }

            public void FixedUpdate()
            {
                if(Body && Body.isPlayerControlled) { Destroy(this); }

                Stopwatch += Time.fixedDeltaTime;
                DecrementOverdriveStacksOverTime();
                if(Stopwatch >= TimeBetweenJumps)
                {
                    if (BodyStateMachine && !EntityStateBlacklist.Contains(BodyStateMachine.state.GetType()))
                    {
                        if (Body)
                        {
                            var target = GetCurrentTarget();
                            if (target)
                            {
                                if (EnableExhaustionAndOverdriveMechanic)
                                {
                                    if (ExhaustionStacks == 0)
                                    {
                                        TimeBetweenJumps = BlinkStateDuration >= 2f ? BlinkStateDuration : 2f;
                                    }
                                    if (ExhaustionStacks < ExhaustionLimit)
                                    {
                                        var distance = Vector3.Distance(target.corePosition, Body.corePosition);
                                        if (distance >= 40)
                                        {
                                            TimeBetweenJumps = BlinkStateDuration <= 0.1f ? BlinkStateDuration : 0.1f; ;
                                            ExhaustionStacks++;
                                        }
                                        else
                                        {
                                            TimeBetweenJumps = BlinkStateDuration >= 2f ? BlinkStateDuration : 2f;
                                        }
                                    }
                                    else
                                    {
                                        TimeBetweenJumps = BlinkStateDuration >= 10f ? BlinkStateDuration : 10f;
                                        Body.AddTimedBuff(RoR2Content.Buffs.Slow80, BlinkStateDuration >= 10f ? BlinkStateDuration : 10f);
                                        ExhaustionStacks = 0;
                                    }
                                }

                                var blinkState = new MyEntityStates.AbyssalDash();
                                blinkState.duration = BlinkStateDuration;
                                blinkState.blinkDistance = BlinkDistance;
                                var jumped = BodyStateMachine.SetInterruptState(blinkState, EntityStates.InterruptPriority.Any);

                                if (!jumped && ExhaustionStacks > 0)
                                {
                                    ExhaustionStacks--;
                                }
                            }
                            else
                            {
                                TimeBetweenJumps = 0.5f;
                            }
                        }
                    }
                    Stopwatch = 0;
                }
            }
        }
    }
}
