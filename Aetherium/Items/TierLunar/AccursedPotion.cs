using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MiscHelpers;
using static Aetherium.Utils.MathHelpers;
using RoR2.ContentManagement;

namespace Aetherium.Items.TierLunar
{
    public class AccursedPotion : ItemBase<AccursedPotion>
    {
        public static ConfigOption<bool> EnableSounds;
        public static ConfigOption<float> BaseSipCooldownDuration;
        public static ConfigOption<float> AdditionalStackSipCooldownReductionPercentage;
        public static ConfigOption<float> BaseRadiusGranted;
        public static ConfigOption<float> AdditionalRadiusGranted;
        public static ConfigOption<int> MaxEffectsAccrued;
        public static ConfigOption<string> BlacklistedBuffsAndDebuffsString;

        public override string ItemName => "Accursed Potion";
        public override string ItemLangTokenName => "ACCURSED_POTION";
        public override string ItemPickupDesc => "Every so often you are forced to drink a strange potion, sharing its effects with enemies around you.";
        public override string ItemFullDescription => $"Every <style=cIsUtility>{BaseSipCooldownDuration}</style> seconds <style=cStack>(reduced by {FloatToPercentageString(1 - AdditionalStackSipCooldownReductionPercentage)} per stack)</style> you are forced to drink a strange potion, sharing its effects with enemies in a <style=cIsUtility>{BaseRadiusGranted}m radius</style> <style=cStack>(+{AdditionalRadiusGranted}m per stack)</style>.";
        public override string ItemLore => "<style=cMono>LOG: Transcribed Audio - 'The Blue Place' SUBJECT: Unknown Survivor</style>\n" +

        "The air here... it tastes like static and old dust.\n " +
        "I found a merchant in the back of the cave. Not the big lizard, something else. Something wrapped in rags and crystals. It was stirring a vat of..." +
        "I don't know. It looked like liquid starlight mixed with oil.\n " +

        "\"There is no true survival in order\", the thing chattered. Its voice sounded like glass grinding.\n" +
        "\"Your 'tactics', your 'formations'... <style=cMono>Flimsy</style>. <style=cMono>Static</style>. To live forever, you must become akin to a storm.\"\n" +

        "It rolled a glass sphere across the stone table. Inside, the liquid shifted colors: blood—red, green, then void-purple; every glance a new color.\n" +

        "\"Chaos is the only armor that cannot be pierced\", it said, offering the vial. \"Drink. Let the wheel spin. If fate decides you burn, then let your enemies choke on the smoke.\"\n" +

        "I shouldn't touch it. I know I shouldn't. But I am no longer in the safety of that place, and the monsters outside are getting louder.\n" +
        "I'm thirsty.";

        public override ItemTier Tier => ItemTier.Lunar;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.Cleansable };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("AccursedPotion.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("AccursedPotionIcon.png");

        public static BuffDef AccursedPotionSipCooldownBuff;
        public static NetworkSoundEventDef AccursedPotionGulp;
        public static GameObject ItemBodyModelPrefab;

        public static List<BuffDef> ValidBuffList = new List<BuffDef>();
        public static HashSet<string> BlacklistedBuffNames = new HashSet<string>();

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateSound();
            CreateBuff();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            EnableSounds = config.ActiveBind<bool>("Item: " + ItemName, "Enable Sounds", true, "Should this item emit sounds?");
            BaseSipCooldownDuration = config.ActiveBind<float>("Item: " + ItemName, "Base Sip Cooldown", 30f, "Base cooldown in seconds.");
            AdditionalStackSipCooldownReductionPercentage = config.ActiveBind<float>("Item: " + ItemName, "Cooldown Reduction Per Stack", 0.75f, "Reduction multiplier per stack (0.75 = 25% reduction).");
            BaseRadiusGranted = config.ActiveBind<float>("Item: " + ItemName, "Base Radius", 20f, "Sharing radius in meters.");
            AdditionalRadiusGranted = config.ActiveBind<float>("Item: " + ItemName, "Radius Per Stack", 5f, "Additional radius per stack.");
            MaxEffectsAccrued = config.ActiveBind<int>("Item: " + ItemName, "Max Effects", 8, "Max buffs/debuffs allowed at once.");
            BlacklistedBuffsAndDebuffsString = config.ActiveBind<string>("Item: " + ItemName, "Blacklisted Buffs", "", "Comma-separated list of BuffDefs to exclude.");
        }

        private void CreateSound()
        {
            AccursedPotionGulp = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            AccursedPotionGulp.eventName = "Aetherium_Gulp";
            ContentAddition.AddNetworkSoundEventDef(AccursedPotionGulp);
        }

        private void CreateBuff()
        {
            AccursedPotionSipCooldownBuff = ScriptableObject.CreateInstance<BuffDef>();
            AccursedPotionSipCooldownBuff.name = "Aetherium: Accursed Potion Cooldown";
            AccursedPotionSipCooldownBuff.buffColor = new Color(0.2f, 0f, 0.2f);
            AccursedPotionSipCooldownBuff.canStack = false;
            AccursedPotionSipCooldownBuff.isDebuff = true;
            AccursedPotionSipCooldownBuff.iconSprite = MainAssets.LoadAsset<Sprite>("AccursedPotionSipCooldownDebuffIcon.png");
            ContentAddition.AddBuffDef(AccursedPotionSipCooldownBuff);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.14626F, 0.09623F, 0.05736F),
                    localAngles = new Vector3(0F, 237.9584F, 180F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.13893F, 0.1018F, 0.05313F),
                    localAngles = new Vector3(355.1616F, 81.55997F, 180F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.12508F, 0.52882F, 1.05645F),
                    localAngles = new Vector3(2.58644F, 15.4937F, 144.3143F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.25473F, 0.13682F, 0.02785F),
                    localAngles = new Vector3(6.06942F, 86.23837F, 170.9439F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.17959F, 0.02055F, -0.07736F),
                    localAngles = new Vector3(4.91717F, 72.05508F, 179.4397F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.14068F, 0.09356F, 0.04851F),
                    localAngles = new Vector3(353.4428F, 85.47656F, 180.9776F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(-0.5854F, -0.70073F, -0.27005F),
                    localAngles = new Vector3(357.1272F, 53.86685F, 346.9289F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.13296F, 0.14329F, 0.04933F),
                    localAngles = new Vector3(0F, 266.8607F, 180F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(1.55242F, 0.82693F, -0.12087F),
                    localAngles = new Vector3(1.77184F, 278.9485F, 211.384F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.11077F, 0.1393F, 0.05012F),
                    localAngles = new Vector3(0F, 90.62645F, 180F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.22591F, 0.0032F, 0.0343F),
                    localAngles = new Vector3(351.5648F, 261.5977F, 172.71F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.01245F, -0.00126F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.00339F, 0.00339F, 0.00339F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.27697F, -0.32539F, -0.0984F),
                    localAngles = new Vector3(0F, 48.7075F, 0F),
                    localScale = new Vector3(0.03752F, 0.03752F, 0.03752F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.1634F, -0.06965F, -0.00002F),
                    localAngles = new Vector3(1.8743F, 269.9206F, 0.62295F),
                    localScale = new Vector3(0.02054F, 0.02054F, 0.02054F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.13865F, -0.04514F, -0.03566F),
                    localAngles = new Vector3(11.17084F, 282.0667F, 0F),
                    localScale = new Vector3(0.02965F, 0.02965F, 0.02965F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos =    new Vector3(-0.0809F, 0.0543F, 0.19989F),
                    localAngles = new Vector3(356.8972F, 183.9839F, 175.9352F),
                    localScale =  new Vector3(0.07479F, 0.07479F, 0.07479F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.00583F, 0.00067F, 0.00583F),
                    localAngles = new Vector3(358.4079F, 152.9051F, 176.8915F),
                    localScale = new Vector3(0.00084F, 0.00084F, 0.00084F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            RoR2Application.onLoad += BuildBuffList;

            On.RoR2.CharacterBody.OnInventoryChanged += ManageComponent;
        }

        private void BuildBuffList()
        {
            string[] blacklist = BlacklistedBuffsAndDebuffsString.ToString().Split(',');
            foreach (string s in blacklist) BlacklistedBuffNames.Add(s.Trim());

            BlacklistedBuffNames.Add(RoR2Content.Buffs.Immune.name);
            BlacklistedBuffNames.Add(RoR2Content.Buffs.HiddenInvincibility.name);
            BlacklistedBuffNames.Add("bdBearVoidReady");

            foreach (BuffDef buff in ContentManager.buffDefs)
            {
                if(!buff) continue;
                if(buff.iconSprite == null) continue;
                if(BlacklistedBuffNames.Contains(buff.name)) continue;

                ValidBuffList.Add(buff);
            }

            AetheriumPlugin.ModLogger.LogInfo($"Accursed Potion: Cached {ValidBuffList.Count} valid buffs.");
        }

        private void ManageComponent(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if(!self.inventory) return;

            int count = self.inventory.GetItemCount(ItemDef);
            var behavior = self.GetComponent<AccursedPotionBehavior>();

            if(count > 0)
            {
                if(!behavior) behavior = self.gameObject.AddComponent<AccursedPotionBehavior>();
                behavior.StackCount = count;
            }
            else if(behavior)
            {
                UnityEngine.Object.Destroy(behavior);
            }
        }

        public class AccursedPotionBehavior : MonoBehaviour
        {
            public CharacterBody Body;
            public int StackCount;

            private float cooldownTimer;

            public void Awake()
            {
                Body = GetComponent<CharacterBody>();
            }

            public void FixedUpdate()
            {
                if(!NetworkServer.active) return;

                if(Body.HasBuff(AccursedPotionSipCooldownBuff)) return;

                TrySipPotion();
            }

            private void TrySipPotion()
            {
                if(Body.activeBuffsListCount >= MaxEffectsAccrued) return;

                if(ValidBuffList.Count == 0) return;
                BuffDef randomBuff = ValidBuffList[Run.instance.stageRng.RangeInt(0, ValidBuffList.Count)];

                float radius = BaseRadiusGranted + (AdditionalRadiusGranted * (StackCount - 1));
                int buffStacks = randomBuff.canStack ? StackCount : 1;
                float duration = Run.instance.stageRng.RangeFloat(10f, 20f);

                Body.AddTimedBuff(randomBuff, duration, buffStacks);

                SphereSearch search = new SphereSearch
                {
                    radius = radius,
                    mask = LayerIndex.entityPrecise.mask,
                    origin = Body.corePosition
                };

                TeamMask enemyTeams = TeamMask.GetEnemyTeams(Body.teamComponent.teamIndex);
                var hurtBoxes = search.RefreshCandidates()
                                      .FilterCandidatesByHurtBoxTeam(enemyTeams)
                                      .FilterCandidatesByDistinctHurtBoxEntities()
                                      .GetHurtBoxes();

                foreach (var box in hurtBoxes)
                {
                    if(box.healthComponent && box.healthComponent.body)
                    {
                        box.healthComponent.body.AddTimedBuff(randomBuff, duration, buffStacks);
                    }
                }

                float reductionMult = (float)Math.Pow(AdditionalStackSipCooldownReductionPercentage, StackCount - 1);
                float cooldown = BaseSipCooldownDuration * reductionMult;

                Body.AddTimedBuff(AccursedPotionSipCooldownBuff, cooldown);

                if(EnableSounds)
                {
                    EntitySoundManager.EmitSoundServer(AccursedPotionGulp.akId, Body.gameObject);
                }
            }
        }
    }
}