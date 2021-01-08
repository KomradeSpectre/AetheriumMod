using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.Utils.MathHelpers;

namespace Aetherium.Items
{
    public class WitchesRing : ItemBase<WitchesRing>
    {
        public static float WitchesRingTriggerThreshold;
        public static float BaseCooldownDuration;
        public static float AdditionalCooldownReduction;
        public static bool GlobalCooldownOnUse;

        public override string ItemName => "Witches Ring";

        public override string ItemLangTokenName => "WITCHES_RING";

        public override string ItemPickupDesc => $"Hits that deal <style=cIsDamage>high damage</style> will also trigger <style=cDeath>On Kill</style> effects. Enemies hit by this effect gain <style=cIsUtility>temporary immunity to it</style>.";

        public override string ItemFullDescription => $"Hits that deal <style=cIsDamage>{FloatToPercentageString(WitchesRingTriggerThreshold)} damage</style> or more will trigger <style=cDeath>On Kill</style> effects." +
            $" Upon success, {(GlobalCooldownOnUse ? "the target hit is <style=cIsUtility>granted immunity to this effect</style>" : "<style=cIsUtility>the ring must recharge</style>")} for <style=cIsUtility>{BaseCooldownDuration} second(s)</style> <style=cStack>(-{FloatToPercentageString(AdditionalCooldownReduction)} duration per stack, hyperbolically)</style>.";

        public override string ItemLore => "A strange ring found next to a skeleton wearing a dark green robe with light green trim.\n" +
            "The markings on it roughly translate to the following: \n" +
            "\n<color=#00AA00>She rewards us, for our service to the cycle she maintains is vital.</color>" +
            "\n<color=#008800>She teaches us, so our hands may always bring forth her sermons in combat.</color>" +
            "\n<color=#00AA00>We are empowered by the cycle of</color> <color=#000000>death</color><color=#00AA00>, so that we may keep balance over an unchecked cycle of</color> <color=#FFFFFF>life</color>.";

        public override ItemTier Tier => ItemTier.Tier3;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

        public override string ItemModelPath => "@Aetherium:Assets/Models/Prefabs/Item/WitchesRing/WitchesRing.prefab";
        public override string ItemIconPath => "@Aetherium:Assets/Textures/Icons/Item/WitchesRingIcon.png";

        public static GameObject ItemBodyModelPrefab;
        public static GameObject CircleBodyModelPrefab;

        public static BuffIndex WitchesRingImmunityBuff;

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
            WitchesRingTriggerThreshold = config.Bind<float>("Item: " + ItemName, "Damage Multiplier Required in a Hit to Activate", 5f, "What threshold should damage have to pass to trigger the Witches Ring?").Value;
            BaseCooldownDuration = config.Bind<float>("Item: " + ItemName, "Duration of Cooldown After Use", 5f, "What should be the base duration of the Witches Ring cooldown?").Value;
            AdditionalCooldownReduction = config.Bind<float>("Item: " + ItemName, "Cooldown Duration Reduction per Additional Witches Ring (Diminishing)", 0.1f, "What percentage (hyperbolically) should each additional Witches Ring reduce the cooldown duration?").Value;
            GlobalCooldownOnUse = config.Bind<bool>("Item: " + ItemName, "Global Cooldown On Use", false, "Should the cooldown effect be applied in the same manner as Kjaro/Runald Bands, or on the victim of the effect?").Value;
        }

        private void CreateBuff()
        {
            var witchesRingImmunityBuff = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = new Color(0, 80, 0),
                canStack = false,
                isDebuff = false,
                name = "Aetherium: Witches Ring Immunity",
                iconPath = "@Aetherium:Assets/Textures/Icons/Buff/WitchesRingBuffIcon.png"
            });
            WitchesRingImmunityBuff = R2API.BuffAPI.Add(witchesRingImmunityBuff);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = Resources.Load<GameObject>(ItemModelPath);
            CircleBodyModelPrefab = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/Item/WitchesRing/WitchesRingCircle.prefab");

            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            CircleBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            CircleBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(CircleBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.31f, 0.01f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(0.28f, 0.28f, 0.28f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.3f, 0f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.001f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.18f, 0f),
                    localAngles = new Vector3(0f, -120f, 0f),
                    localScale = new Vector3(0.28f, 0.28f, 0.28f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.16f, 0f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.001f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0f, 3.4f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(3f, 3f, 3f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0f, 3.2f, 0f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(1.5f, 1.5f, 0.001f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0.01f, 0.28f, 0f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(0.3f, 0.3f, 0.3f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.27f, 0f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.001f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, -0.05f, 0f),
                    localAngles = new Vector3(20f, 0f, 0f),
                    localScale = new Vector3(0.28f, 0.28f, 0.28f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.33f, 0f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.001f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0f, 0.33f, 0f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.001f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.27f, 0f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(0.28f, 0.28f, 0.28f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.25f, 0f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.12f, 0.12f, 0.001f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0f, -0.6f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(3f, 3f, 3f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0f, -0.7f, 0f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 0.001f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MechLowerArmR",
                    localPos = new Vector3(0f, 0.615f, 0f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(0.4f, 0.4f, 0.4f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "MechLowerArmR",
                    localPos = new Vector3(0f, 0.6f, 0f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.001f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 3.7f, 0f),
                    localAngles = new Vector3(0f, -50f, 355f),
                    localScale = new Vector3(6f, 6f, 6f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 3.5f, 0f),
                    localAngles = new Vector3(-85f, 0f, 0f),
                    localScale = new Vector3(2.25f, 2.25f, 0.001f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.17f, 0f),
                    localAngles = new Vector3(0f, -90f, 0f),
                    localScale = new Vector3(0.28f, 0.28f, 0.28f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CircleBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.15f, 0f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.14f, 0.14f, 0.001f)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GrantOnKillEffectsOnHighDamage;
        }

        private void GrantOnKillEffectsOnHighDamage(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, GameObject victim)
        {
            var attacker = damageInfo.attacker;
            if (attacker)
            {
                var body = attacker.GetComponent<CharacterBody>();
                var victimBody = victim.GetComponent<CharacterBody>();
                if (body && victimBody)
                {
                    var InventoryCount = GetCount(body);
                    if (InventoryCount > 0)
                    {
                        if (damageInfo.damage / body.damage >= WitchesRingTriggerThreshold && !victimBody.HasBuff(WitchesRingImmunityBuff))
                        {
                            if (NetworkServer.active)
                            {
                                if (!GlobalCooldownOnUse)
                                {
                                    victimBody.AddTimedBuffAuthority(WitchesRingImmunityBuff, BaseCooldownDuration / (1 + AdditionalCooldownReduction * (InventoryCount - 1)));
                                }
                                else
                                {
                                    body.AddTimedBuffAuthority(WitchesRingImmunityBuff, BaseCooldownDuration / (1 + AdditionalCooldownReduction * (InventoryCount - 1)));
                                }
                                DamageReport damageReport = new DamageReport(damageInfo, victimBody.healthComponent, damageInfo.damage, victimBody.healthComponent.combinedHealth);
                                GlobalEventManager.instance.OnCharacterDeath(damageReport);
                            }
                        }
                    }
                }
            }
            orig(self, damageInfo, victim);
        }
    }
}