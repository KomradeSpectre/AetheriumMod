using Aetherium.OrbVisuals;
using Aetherium.Utils;
using EntityStates;
using KomradeSpectre.Aetherium;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RewiredConsts;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using TILER2;
using static TILER2.MiscUtil;
using UnityEngine;
using UnityEngine.Networking;
using JetBrains.Annotations;

namespace Aetherium.Items
{
    public class WitchesRing : Item_V2<WitchesRing>
    {
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What threshold should damage have to pass to trigger the Witches Ring? (Default: 5 (500%))", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float witchesRingTriggerThreshold { get; private set; } = 5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What should be the base duration of the Witches Ring cooldown? (Default: 5 (5 seconds))", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseCooldownDuration { get; private set; } = 5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What percentage (hyperbolically) should each additional Witches Ring reduce the cooldown duration? (Default: 0.1 (10% hyperbolically))", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float additionalCooldownReduction { get; private set; } = 0.1f;

        public override string displayName => "Witches Ring";

        public override ItemTier itemTier => RoR2.ItemTier.Tier3;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.OnKillEffect });
        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => $"Hits that deal <style=cIsDamage>high damage</style> will also trigger <style=cDeath>On Kill</style> effects. Enemies hit by this effect gain <style=cIsUtility>temporary immunity to it</style>.";

        protected override string GetDescString(string langid = null) => $"Hits that deal <style=cIsDamage>{Pct(witchesRingTriggerThreshold)} damage</style> or more will trigger <style=cDeath>On Kill</style> effects." +
            $" Upon success, the target hit is <style=cIsUtility>granted immunity to this effect</style> for <style=cIsUtility>{baseCooldownDuration} second(s)</style> <style=cStack>(-{Pct(additionalCooldownReduction)} duration per stack, hyperbolically)</style>.";

        protected override string GetLoreString(string langID = null) => "A strange ring found next to a skeleton wearing a dark green robe with light green trim.\n" +
            "The markings on it roughly translate to the following: \n" +
            "\n<color=#00AA00>She rewards us, for our service to the cycle she maintains is vital.</color>" +
            "\n<color=#008800>She teaches us, so our hands may always bring forth her sermons in combat.</color>" +
            "\n<color=#00AA00>We are empowered by the cycle of</color> <color=#000000>death</color><color=#00AA00>, so that we may keep balance over an unchecked cycle of</color> <color=#FFFFFF>life</color>.";

        private static List<RoR2.CharacterBody> Playername = new List<RoR2.CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        public static GameObject CircleBodyModelPrefab;

        public static BuffIndex WitchesRingImmunityBuff;

        public WitchesRing()
        {
            modelResourcePath = "@Aetherium:Assets/Models/Prefabs/WitchesRing.prefab";
            iconResourcePath = "@Aetherium:Assets/Textures/Icons/WitchesRingIcon.png";
        }

        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(modelResourcePath);
                CircleBodyModelPrefab = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/WitchesRingCircle.prefab");
                displayRules = GenerateItemDisplayRules();
            }
            base.SetupAttributes();

            var witchesRingImmunityBuff = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = new Color(0, 80, 0),
                canStack = false,
                isDebuff = false,
                name = "ATHRM Witches Ring Immunity",
                iconPath = "@Aetherium:Assets/Textures/Icons/WitchesRingBuffIcon.png"
            });
            WitchesRingImmunityBuff = R2API.BuffAPI.Add(witchesRingImmunityBuff);
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            CircleBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            CircleBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(CircleBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.2f, 0.2f, 0.2f);
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

        public override void Install()
        {
            base.Install();

            On.RoR2.GlobalEventManager.OnHitEnemy += GrantOnKillEffectsOnHighDamage;

        }

        public override void Uninstall()
        {
            base.Uninstall();

            On.RoR2.GlobalEventManager.OnHitEnemy -= GrantOnKillEffectsOnHighDamage;

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
                        if (damageInfo.damage / body.damage >= witchesRingTriggerThreshold && !victimBody.HasBuff(WitchesRingImmunityBuff))
                        {
                            if (NetworkServer.active)
                            {
                                victimBody.AddTimedBuffAuthority(WitchesRingImmunityBuff, baseCooldownDuration / (1 + additionalCooldownReduction * (InventoryCount - 1)));
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
