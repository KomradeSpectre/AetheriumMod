using Aetherium.Utils;
using BepInEx.Configuration;
using On.RoR2;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using UnityEngine;
using ItemStats;
using ItemStats.Stat;
using ItemStats.ValueFormatters;

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Compatability.ModCompatability.ItemStatsModCompat;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static R2API.RecalculateStatsAPI;

namespace Aetherium.Items
{
    public class ShieldingCore : ItemBase<ShieldingCore>
    {
        public static ConfigOption<bool> UseNewIcons;
        public static ConfigOption<bool> EnableParticleEffects;
        public static ConfigOption<float> BaseShieldingCoreArmorGrant;
        public static ConfigOption<float> AdditionalShieldingCoreArmorGrant;
        public static ConfigOption<float> BaseGrantShieldMultiplier;

        public override string ItemName => "Shielding Core";

        public override string ItemLangTokenName => "SHIELDING_CORE";

        public override string ItemPickupDesc => "While shielded, gain a temporary boost in <style=cIsUtility>armor</style>.";

        public override string ItemFullDescription => $"You gain <style=cIsUtility>{BaseShieldingCoreArmorGrant}</style> <style=cStack>(+{AdditionalShieldingCoreArmorGrant} per stack)</style> <style=cIsUtility>armor</style> while <style=cIsUtility>BLUE shields</style> are active." +
            $" The first stack of this item will grant <style=cIsUtility>{FloatToPercentageString(BaseGrantShieldMultiplier)}</style> of your max health as shield on pickup.";

        public override string ItemLore => OrderManifestLoreFormatter(

            ItemName,

            "7/4/2091",

            "UES Backlight/Sector 667/Outer Rim",

            "667********",

            ItemPickupDesc,

            "Light / Liquid-Seal / DO NOT DRINK FROM EXHAUST",

            "\nEngineer's report:\n\n" +
            "   Let me preface this with a bit of honesty, I do not know what the green goo inside my little turbine is. " +
            "I bought an aftermarket resonator from one of the junk dealers our ship passed, because I was running low on parts to repair our shield generators. " +
            "As soon as I slotted this thing in, I'm covered in this gross liquid that seems to dissipate into these sparkly crystals when exposed to air. " +
            "Normally this wouldn't be much of an issue since I'm in a suit, but the stuff was constantly attempting to fill the container it occupied so I had to create a seal for it. " +
            "That's when my suit diagnostics alarmed me that my shield's efficacy hit the roof.\n\n" +
            "Eureka moment, and a few design drafts later.\n" +
            "Now I'm selling these things like hotcakes and making a profit. So here's one for you.\n\n" +
            "P.S. Don't expose your skin to this stuff, it may cause over 200 known forms of cancer. That's our secret though, right?");

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("ShieldingCore.prefab");
        public override Sprite ItemIcon => UseNewIcons ? MainAssets.LoadAsset<Sprite>("ShieldingCoreIconAlt.png") : MainAssets.LoadAsset<Sprite>("ShieldingCoreIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public Material OriginalShieldMaterial;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            UseNewIcons = config.ActiveBind<bool>("Item: " + ItemName, "Use Alternative Icon Art?", true, "If set to true, will use the new icon art drawn by WaltzingPhantom, else it will use the old icon art.");
            EnableParticleEffects = config.ActiveBind<bool>("Item: " + ItemName, "Enable Particle Effects", true, "Should the particle effects for the models be enabled?");
            BaseShieldingCoreArmorGrant = config.ActiveBind<float>("Item: " + ItemName, "First Shielding Core Bonus to Armor", 15f, "How much armor should the first Shielding Core grant?");
            AdditionalShieldingCoreArmorGrant = config.ActiveBind<float>("Item: " + ItemName, "Additional Shielding Cores Bonus to Armor", 10f, "How much armor should each additional Shielding Core grant?");
            BaseGrantShieldMultiplier = config.ActiveBind<float>("Item: " + ItemName, "First Shielding Core Bonus to Max Shield", 0.08f, "How much should the starting shield be upon receiving the item?");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);
            if (EnableParticleEffects) { ItemBodyModelPrefab.AddComponent<ShieldingCoreVisualCueController>(); }

            Vector3 generalScale = new Vector3(0.2f, 0.2f, 0.2f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.2f, -0.22f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.17f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.05f, 0.14f, -0.12f),
                    localAngles = new Vector3(0f, 160f, -20f),
                    localScale = new Vector3(0.14f, 0.14f, 0.14f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 2.5f, -2.5f),
                    localAngles = new Vector3(0f, -180f, 0f),
                    localScale = new Vector3(1.5f, 1.5f, 1.5f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.22f, -0.3f),
                    localAngles = new Vector3(0f, 180, 180f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.1f, -0.35f),
                    localAngles = new Vector3(-10f, 180f, 180f),
                    localScale = new Vector3(0.14f, 0.14f, 0.14f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.19f, -0.32f),
                    localAngles = new Vector3(-15f, -180f, 180f),
                    localScale = new Vector3(0.17f, 0.17f, 0.17f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0f, 0.9f, 0f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.7f, 0.7f, 0.7f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.25f, -0.4f),
                    localAngles = new Vector3(-10f, 180, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 3.5f, 3.5f),
                    localAngles = new Vector3(-45f, 0f, 0f),
                    localScale = new Vector3(2, 2, 2)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.2f, -0.25f),
                    localAngles = new Vector3(0f, -180f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.1737F, -0.2124F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.1239F, 0.1239F, 0.1239F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            if (IsItemStatsModInstalled)
            {
                RoR2.RoR2Application.onLoad += ItemStatsModCompat;
            }

            GetStatCoefficients += GrantBaseShield;
            On.RoR2.CharacterBody.FixedUpdate += ShieldedCoreValidator;
            GetStatCoefficients += ShieldedCoreArmorCalc;
        }

        private void ItemStatsModCompat()
        {
            CreateShieldingCoreStatDef();
        }

        private void GrantBaseShield(RoR2.CharacterBody sender, StatHookEventArgs args)
        {
            if (GetCount(sender) > 0)
            {
                RoR2.HealthComponent healthC = sender.GetComponent<RoR2.HealthComponent>();
                args.baseShieldAdd += healthC.fullHealth * BaseGrantShieldMultiplier;
            }
        }

        //private void GrantBaseShield(ILContext il)
        //{
        //    //Provided by Harb from their HarbCrate mod. Thanks Harb!
        //    ILCursor c = new ILCursor(il);
        //    int shieldsLoc = 33;
        //    c.GotoNext(
        //        MoveType.Before,
        //        x => x.MatchLdloc(out shieldsLoc),
        //        x => x.MatchCallvirt<CharacterBody>("set_maxShield")
        //    );
        //    c.Emit(OpCodes.Ldloc, shieldsLoc);
        //    c.EmitDelegate<Func<CharacterBody, float, float>>((self, shields) =>
        //    {
        //        var InventoryCount = GetCount(self);
        //        if (InventoryCount > 0)
        //        {
        //            shields += self.maxHealth * 0.04f;
        //        }
        //        return shields;
        //    });
        //    c.Emit(OpCodes.Stloc, shieldsLoc);
        //    c.Emit(OpCodes.Ldarg_0);
        //}

        private void ShieldedCoreValidator(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            orig(self);

            var shieldComponent = self.GetComponent<ShieldedCoreComponent>();
            if (!shieldComponent) { shieldComponent = self.gameObject.AddComponent<ShieldedCoreComponent>(); }

            var newInventoryCount = GetCount(self);
            var IsShielded = self.healthComponent.shield > 0;

            bool IsDifferent = false;
            if (shieldComponent.cachedInventoryCount != newInventoryCount)
            {
                IsDifferent = true;
                shieldComponent.cachedInventoryCount = newInventoryCount;
            }
            if (shieldComponent.cachedIsShielded != IsShielded)
            {
                IsDifferent = true;
                shieldComponent.cachedIsShielded = IsShielded;
            }

            if (!IsDifferent) return;

            self.statsDirty = true;
        }

        private void ShieldedCoreArmorCalc(RoR2.CharacterBody sender, StatHookEventArgs args)
        {
            var ShieldedCoreComponent = sender.GetComponent<ShieldedCoreComponent>();
            if (ShieldedCoreComponent && ShieldedCoreComponent.cachedIsShielded && ShieldedCoreComponent.cachedInventoryCount > 0)
            {
                args.armorAdd += BaseShieldingCoreArmorGrant + (AdditionalShieldingCoreArmorGrant * (ShieldedCoreComponent.cachedInventoryCount - 1));
            }
        }

        public class ShieldedCoreComponent : MonoBehaviour
        {
            public int cachedInventoryCount = 0;
            public bool cachedIsShielded = false;
        }

        public class ShieldingCoreVisualCueController : MonoBehaviour
        {
            public RoR2.ItemDisplay ItemDisplay;
            public ParticleSystem[] ParticleSystem;
            public RoR2.CharacterMaster OwnerMaster;
            public RoR2.CharacterBody OwnerBody;
            public void FixedUpdate()
            {

                if (!OwnerMaster || !ItemDisplay || ParticleSystem.Length != 3)
                {
                    ItemDisplay = this.GetComponentInParent<RoR2.ItemDisplay>();
                    if (ItemDisplay)
                    {
                        ParticleSystem = ItemDisplay.GetComponentsInChildren<ParticleSystem>();
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
                        if (ParticleSystem.Length == 3)
                        {
                            for(int i = 0; i < ParticleSystem.Length; i++)
                            {
                                UnityEngine.Object.Destroy(ParticleSystem[i]);
                            }
                        }
                        UnityEngine.Object.Destroy(this);
                    }
                }

                if (OwnerBody && ParticleSystem.Length == 3)
                {
                    foreach (ParticleSystem particleSystem in ParticleSystem)
                    {
                        if (OwnerBody.healthComponent.shield > 0)
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
                }
            }
        }
    }
}