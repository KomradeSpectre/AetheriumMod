using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using TILER2;
using EliteSpawningOverhaul;
using UnityEngine;
using R2API;
using R2API.Utils;
using UnityEngine.Networking;
using RoR2.Projectile;
using Aetherium.Items;
using Aetherium.Utils;

namespace Aetherium.EliteFocusedEquipment
{
    class HisVengeance : Equipment_V2<HisVengeance>
    {
        public override string displayName => "His Vengeance";

        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Release shards when you are hit.";

        protected override string GetDescString(string langID = null) => $"";

        protected override string GetLoreString(string langID = null) => $"";

        public static string EliteBuffName = "Affix_Splintering";
        public static string EliteBuffIconPath = "@Aetherium:Assets/Textures/Icons/HisVengeanceBuffIcon.png";

        public static string ElitePrefixName = "Splintering";
        public static string EliteModifierString = "AETHERIUM_ELITE_MODIFIER_SPLINTERING";
        public static int EliteTier = 1;

        public static EliteAffixCard EliteCard { get; set; }
        public static EliteIndex EliteIndex;
        public static BuffIndex EliteBuffIndex;

        public static Material EliteMaterial;

        public static GameObject SplinteringProjectile;
        public static GameObject ItemBodyModelPrefab;
        public static GameObject SecondaryItemBodyModelPrefab;

        public static Xoroshiro128Plus random = new Xoroshiro128Plus((ulong)System.DateTime.Now.Ticks);

        public HisVengeance()
        {
            modelResourcePath = "@Aetherium:Assets/Models/Prefabs/HisVengeance.prefab";
            iconResourcePath = "@Aetherium:Assets/Textures/Icons/SplinteringSoulIcon.png";
        }

        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/HisVengeanceDisplay.prefab");
                SecondaryItemBodyModelPrefab = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/HisVengeanceCluster.prefab");
                displayRules = GenerateItemDisplayRules();
            }
            base.SetupAttributes();
            equipmentDef.canDrop = false;
            equipmentDef.enigmaCompatible = false;

            var buffDef = new RoR2.BuffDef
            {
                name = EliteBuffName,
                buffColor = new Color32(255, 255, 255, byte.MaxValue),
                iconPath = EliteBuffIconPath,
                canStack = false,
            };
            buffDef.eliteIndex = EliteIndex;
            var buffIndex = new CustomBuff(buffDef);
            EliteBuffIndex = BuffAPI.Add(buffIndex);
            equipmentDef.passiveBuff = EliteBuffIndex;

            var eliteDef = new RoR2.EliteDef
            {
                name = ElitePrefixName,
                modifierToken = EliteModifierString,
                color = buffDef.buffColor,
            };
            eliteDef.eliteEquipmentIndex = equipmentDef.equipmentIndex;
            var eliteIndex = new CustomElite(eliteDef, EliteTier);
            EliteIndex = EliteAPI.Add(eliteIndex);

            var card = new EliteAffixCard
            {
                spawnWeight = 0.5f,
                costMultiplier = 15.0f,
                damageBoostCoeff = 2.0f,
                healthBoostCoeff = 2.0f,
                eliteOnlyScaling = 0.5f,
                eliteType = EliteIndex
            };
            EsoLib.Cards.Add(card);
            EliteCard = card;

            LanguageAPI.Add(eliteDef.modifierToken, ElitePrefixName + " {0}");
            
            EliteMaterial = Resources.Load<Material>("@Aetherium:Assets/Textures/Materials/HisVengeance.mat");

            SplinteringProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/ImpVoidspikeProjectile"), "SplinteringProjectile", true);

            var model = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/HisVengeanceProjectile.prefab");
            model.AddComponent<NetworkIdentity>();
            var ghost = model.AddComponent<RoR2.Projectile.ProjectileGhostController>();

            var controller = SplinteringProjectile.GetComponent<RoR2.Projectile.ProjectileController>();
            controller.procCoefficient = 0.25f;
            controller.ghostPrefab = model;

            var damage = SplinteringProjectile.GetComponent<RoR2.Projectile.ProjectileDamage>();
            damage.damageType = DamageType.Generic;

            var impactExplosion = SplinteringProjectile.GetComponent<ProjectileImpactExplosion>();
            impactExplosion.impactEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactCrowbar");

            var stickOnImpact = SplinteringProjectile.GetComponent<RoR2.Projectile.ProjectileStickOnImpact>();
            stickOnImpact.stickSoundString = "Play_bellBody_attackLand";

            if (SplinteringProjectile) PrefabAPI.RegisterNetworkPrefab(SplinteringProjectile);

            RoR2.ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(SplinteringProjectile);
            };
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            SecondaryItemBodyModelPrefab.AddComponent<ItemDisplay>();
            SecondaryItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(SecondaryItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.3f, 0.3f, 0.3f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.188f, 0.451f, -0.268f),
                    localAngles = new Vector3(-45, -135, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.188f, 0.451f, -0.268f),
                    localAngles = new Vector3(-45, 135, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.3018f, 0.1697f),
                    localAngles = new Vector3(35, 180, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.158f, 0f),
                    localAngles = new Vector3(90, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SecondaryItemBodyModelPrefab,
                    childName = "UpperArmL",
                    localPos = new Vector3(0.049f, 0.169f, 0.064f),
                    localAngles = new Vector3(0, 30, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SecondaryItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(-0.049f, 0.169f, 0.064f),
                    localAngles = new Vector3(0, -30, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0f, 0.158f, 0f),
                    localAngles = new Vector3(90, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.138f, 0.036f),
                    localAngles = new Vector3(90, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0f, 0.138f, 0.036f),
                    localAngles = new Vector3(90, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.15f, 0.17f),
                    localAngles = new Vector3(-45, 0, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.089f, 0.3065f, -0.1954f),
                    localAngles = new Vector3(211, 0, 0),
                    localScale = new Vector3(0.015f, 0.015f, 0.11f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.089f, 0.3065f, -0.1954f),
                    localAngles = new Vector3(211, 0, 0),
                    localScale = new Vector3(0.015f, 0.015f, 0.11f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.2566f, 0.0698f),
                    localAngles = new Vector3(10, 180, 0),
                    localScale = new Vector3(0.03f, 0.03f, 0.05f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.159f, 0.111f),
                    localAngles = new Vector3(10, 180, 0),
                    localScale = new Vector3(0.03f, 0.03f, 0.05f)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-2.5f, 3f, -2f),
                    localAngles = new Vector3(-45, -135, 0),
                    localScale = new Vector3(0.7f, 0.7f, 2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(2.5f, 3f, -2f),
                    localAngles = new Vector3(-45, 135, 0),
                    localScale = new Vector3(0.7f, 0.7f, 2f)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.083f, 0.215f),
                    localAngles = new Vector3(-20, 180, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.2101f, 0.298f, 0.18f),
                    localAngles = new Vector3(155, 0, 0),
                    localScale = new Vector3(0.04f, 0.04f, 0.05f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.2101f, 0.298f, 0.18f),
                    localAngles = new Vector3(155, 0, 0),
                    localScale = new Vector3(0.04f, 0.04f, 0.05f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.0815f, 0.2384f, 0.2304f),
                    localAngles = new Vector3(155, 0, 0),
                    localScale = new Vector3(0.04f, 0.04f, 0.05f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.0815f, 0.2384f, 0.2304f),
                    localAngles = new Vector3(155, 0, 0),
                    localScale = new Vector3(0.04f, 0.04f, 0.05f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.083f, -0.078f, 0.193f),
                    localAngles = new Vector3(180, 45, 0),
                    localScale = new Vector3(0.03f, 0.03f, 0.04f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.083f, -0.078f, 0.193f),
                    localAngles = new Vector3(180, -45, 0),
                    localScale = new Vector3(0.03f, 0.03f, 0.04f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.14f, 0.466f, -0.15f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.04f, 0.04f, 0.04f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.14f, 0.466f, -0.15f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.04f, 0.04f, 0.04f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CannonHeadL",
                    localPos = new Vector3(0.223f, 0.233f, 0.229f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(0.03f, 0.03f, 0.15f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CannonHeadR",
                    localPos = new Vector3(-0.223f, 0.233f, 0.229f),
                    localAngles = new Vector3(0, -45, 0),
                    localScale = new Vector3(0.03f, 0.03f, 0.15f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CannonHeadL",
                    localPos = new Vector3(0.223f, 0.384f, 0.233f),
                    localAngles = new Vector3(0, 45, 0),
                    localScale = new Vector3(0.03f, 0.03f, 0.15f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CannonHeadR",
                    localPos = new Vector3(-0.223f, 0.384f, 0.233f),
                    localAngles = new Vector3(0, -45, 0),
                    localScale = new Vector3(0.03f, 0.03f, 0.15f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SecondaryItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0f, -0.054f, 0f),
                    localAngles = new Vector3(-70, 0, 0),
                    localScale = new Vector3(0.1f, 0.12f, 0.12f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.15f, 0f),
                    localAngles = new Vector3(-90, 0, 0),
                    localScale = new Vector3(0.12f, 0.12f, 0.1f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0f, 0.15f, 0f),
                    localAngles = new Vector3(-90, 0, 0),
                    localScale = new Vector3(0.11f, 0.11f, 0.1f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.299f, 0.044f),
                    localAngles = new Vector3(-90, 0, 0),
                    localScale = new Vector3(0.12f, 0.12f, 0.1f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0f, 0.299f, 0.044f),
                    localAngles = new Vector3(-90, 0, 0),
                    localScale = new Vector3(0.12f, 0.12f, 0.1f)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.15f, 0.15f),
                    localAngles = new Vector3(-45, 0, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.193f, -0.126f),
                    localAngles = new Vector3(0, 180, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.08f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0f, 0.193f, 0.126f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.08f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.113f, 0.14f, -0.083f),
                    localAngles = new Vector3(-45f, 160, 0),
                    localScale = new Vector3(0.02f, 0.02f, 0.15f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.113f, 0.14f, -0.083f),
                    localAngles = new Vector3(-45f, -160, 0),
                    localScale = new Vector3(0.02f, 0.02f, 0.15f)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.084f, 0.171f),
                    localAngles = new Vector3(0, 180, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.213f, 0.302f, -0.213f),
                    localAngles = new Vector3(-45, 135, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.213f, 0.302f, -0.213f),
                    localAngles = new Vector3(-45, -135, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.2f)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FootFrontL",
                    localPos = new Vector3(0f, 0.697f, 0f),
                    localAngles = new Vector3(-90, 0, 0),
                    localScale = new Vector3(0.2f, 0.2f, 0.55f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FootFrontR",
                    localPos = new Vector3(0f, 0.697f, 0f),
                    localAngles = new Vector3(-90, 0, 0),
                    localScale = new Vector3(0.2f, 0.2f, 0.55f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FootBackL",
                    localPos = new Vector3(0f, 0.697f, 0f),
                    localAngles = new Vector3(-90, 0, 0),
                    localScale = new Vector3(0.2f, 0.2f, 0.55f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FootBackR",
                    localPos = new Vector3(0f, 0.697f, 0f),
                    localAngles = new Vector3(-90, 0, 0),
                    localScale = new Vector3(0.2f, 0.2f, 0.55f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0.7f, 1.2f, -0.3f),
                    localAngles = new Vector3(-45, 135, 0),
                    localScale = new Vector3(0.25f, 0.25f, 1f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(-0.7f, 1.2f, -0.3f),
                    localAngles = new Vector3(-45, -135, 0),
                    localScale = new Vector3(0.25f, 0.25f, 1f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MechLowerArmL",
                    localPos = new Vector3(0f, 0.254f, 0f),
                    localAngles = new Vector3(90, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.4f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MechLowerArmR",
                    localPos = new Vector3(0f, 0.254f, 0f),
                    localAngles = new Vector3(90, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.4f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.146f, 0.149f),
                    localAngles = new Vector3(22.5f, 180, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MechUpperArmL",
                    localPos = new Vector3(0f, 0f, -0.168f),
                    localAngles = new Vector3(0, 180, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MechUpperArmR",
                    localPos = new Vector3(0f, 0f, -0.168f),
                    localAngles = new Vector3(0, 180, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.2f)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, -1.87f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1f, 1f, 1f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(2.29f, 2.67f, 3.76f),
                    localAngles = new Vector3(-22.5f, 45, 0),
                    localScale = new Vector3(0.7f, 0.7f, 2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-2.29f, 2.67f, 3.76f),
                    localAngles = new Vector3(-22.5f, -45, 0),
                    localScale = new Vector3(0.7f, 0.7f, 2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 2.76f, -0.4f),
                    localAngles = new Vector3(-90f, 0, 0),
                    localScale = new Vector3(0.5f, 0.5f, 2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-1.41f, 1.9f, 0.43f),
                    localAngles = new Vector3(0f, 90, 0),
                    localScale = new Vector3(0.25f, 0.25f, 0.2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(1.41f, 1.9f, 0.43f),
                    localAngles = new Vector3(0f, -90, 0),
                    localScale = new Vector3(0.25f, 0.25f, 0.2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(1.232f, 2.784f, 0.43f),
                    localAngles = new Vector3(0f, -90, 0),
                    localScale = new Vector3(0.25f, 0.25f, 0.2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-1.232f, 2.784f, 0.43f),
                    localAngles = new Vector3(0f, 90, 0),
                    localScale = new Vector3(0.25f, 0.25f, 0.2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 2.92f, 0f),
                    localAngles = new Vector3(90f, 0, 0),
                    localScale = new Vector3(1.3f, 1.3f, 2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0f, 2.92f, 0f),
                    localAngles = new Vector3(90f, 0, 0),
                    localScale = new Vector3(1.3f, 1.3f, 2f)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.13f, 0.179f),
                    localAngles = new Vector3(0, 180, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.05f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.08f, -0.169f),
                    localAngles = new Vector3(-22.5f, 180, 0),
                    localScale = new Vector3(0.03f, 0.03f, 0.07f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.403f, -0.272f),
                    localAngles = new Vector3(-45, 180, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.158f, -0.259f),
                    localAngles = new Vector3(-45, 180, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, -0.099f, -0.204f),
                    localAngles = new Vector3(-45, 180, 0),
                    localScale = new Vector3(0.05f, 0.05f, 0.2f)
                }
            });
            rules.Add("mdlBeetle", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.5f, 0.35f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.3f, 0.3f, 0.3f)
                }
            });
            rules.Add("mdlBeetleGuard", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-1f, -0.3f, 0.9f),
                    localAngles = new Vector3(30, -45, 0),
                    localScale = new Vector3(0.3f, 0.3f, 2)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(1f, -0.3f, 0.9f),
                    localAngles = new Vector3(30, 45, 0),
                    localScale = new Vector3(0.3f, 0.3f, 2)
                }
            });
            rules.Add("mdlBeetleQueen", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 3.5f, -1f),
                    localAngles = new Vector3(-45, 180, 0),
                    localScale = new Vector3(0.4f, 0.4f, 2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-2f, 3f, -0.5f),
                    localAngles = new Vector3(-45, -135, 0),
                    localScale = new Vector3(0.4f, 0.4f, 2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(2f, 3f, -0.5f),
                    localAngles = new Vector3(-45, 135, 0),
                    localScale = new Vector3(0.4f, 0.4f, 2f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Butt",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1.5f, 1.5f, 2f)
                }
            });
            rules.Add("mdlBell", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ShieldL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(4.3f, 4.3f, 4.3f)
                },
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ShieldR",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(4.3f, 4.3f, 4.3f)
                }
            });
            rules.Add("mdlBison", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.3f, 0.3f, 0.3f)
                }
            });
            return rules;
        }

        public override void Install()
        {
            base.Install();

            On.RoR2.CharacterBody.FixedUpdate += AddEliteMaterials;
            On.RoR2.HealthComponent.TakeDamage += FireSplinter;
        }

        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.CharacterBody.FixedUpdate -= AddEliteMaterials;
            On.RoR2.HealthComponent.TakeDamage -= FireSplinter;
        }

        protected override bool PerformEquipmentAction(RoR2.EquipmentSlot slot)
        {
            return false;
        }

        private void AddEliteMaterials(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (self.HasBuff(EliteBuffIndex) && !self.GetComponent<SplinteringBuffTracker>())
            {
                var modelLocator = self.modelLocator;
                if (modelLocator)
                {
                    var modelTransform = self.modelLocator.modelTransform;
                    if (modelTransform)
                    {
                        var model = self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>();
                        if (model)
                        {
                            var splinteringBuffTracker = self.gameObject.AddComponent<SplinteringBuffTracker>();
                            splinteringBuffTracker.Body = self;
                            TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                            overlay.duration = float.PositiveInfinity;
                            overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                            overlay.animateShaderAlpha = true;
                            overlay.destroyComponentOnEnd = true;
                            overlay.originalMaterial = EliteMaterial;
                            overlay.AddToCharacerModel(model);
                            splinteringBuffTracker.Overlay = overlay;
                        }
                    }
                }
            }
            orig(self);
        }

        private void FireSplinter(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            var body = self.body;
            if (body)
            {
                if (body.HasBuff(EliteBuffIndex))
                {
                    var newProjectileInfo = new FireProjectileInfo();
                    newProjectileInfo.owner = body.gameObject;
                    newProjectileInfo.projectilePrefab = SplinteringProjectile;
                    newProjectileInfo.speedOverride = 50.0f;
                    newProjectileInfo.damage = body.damage;
                    newProjectileInfo.damageTypeOverride = null;
                    newProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                    newProjectileInfo.procChainMask = default(RoR2.ProcChainMask);
                    //var theta = (Math.PI * 2) / swordsPerFlower;
                    //var angle = theta * i;
                    //var radius = 3;
                    //var positionChosen = new Vector3((float)(radius * Math.Cos(angle) + damageInfo.position.x), damageInfo.position.y + 3, (float)(radius * Math.Sin(angle) + damageInfo.position.z));
                    Vector3 PositionChosen;
                    if (damageInfo.attacker)
                    {
                        var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        if (attackerBody)
                        {
                            PositionChosen = attackerBody.corePosition;
                        }
                        else
                        {
                            PositionChosen = body.transform.position;
                        }
                    }
                    else
                    {
                        PositionChosen = body.transform.position;
                    }
                    if (random.nextNormalizedFloat > 0.15f)
                    {
                        var randomOffset = new Vector3(random.RangeFloat(-1, 1), random.RangeFloat(-1, 1), random.RangeFloat(-1, 1));
                        if (randomOffset.Equals(Vector3.zero))
                        {
                            randomOffset = Vector3.one;
                        }
                        PositionChosen += randomOffset.normalized * random.RangeFloat(0, 10);
                    }
                    newProjectileInfo.position = damageInfo.position;
                    newProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(PositionChosen - damageInfo.position);
                    if (NetworkServer.active) { EffectManager.SimpleEffect(Resources.Load<GameObject>("Prefabs/Effects/BrittleDeath"), newProjectileInfo.position, newProjectileInfo.rotation, true); }                    
                    ProjectileManager.instance.FireProjectile(newProjectileInfo);

                }
            }
            orig(self, damageInfo);
        }

        public class SplinteringBuffTracker : MonoBehaviour
        {
            public RoR2.TemporaryOverlay Overlay;
            public RoR2.CharacterBody Body;

            public void FixedUpdate()
            {
                if (!Body.HasBuff(EliteBuffIndex))
                {
                    UnityEngine.Object.Destroy(Overlay);
                    UnityEngine.Object.Destroy(this);
                }
            }
        }

    }
}
