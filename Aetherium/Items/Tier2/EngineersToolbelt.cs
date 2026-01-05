using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Audio;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Utils.MiscHelpers;

namespace Aetherium.Items.Tier2
{
    public class EngineersToolbelt : ItemBase<EngineersToolbelt>
    {
        public static ConfigOption<bool> EnableSounds;

        public static ConfigOption<float> BaseDuplicationPercentChance;
        public static ConfigOption<float> AdditionalDuplicationPercentChance;

        public static ConfigOption<float> BaseRevivalPercentChance;
        public static ConfigOption<float> AdditionalRevivalPercentChance;

        public override string ItemName => "Engineers Toolbelt";

        public override string ItemLangTokenName => "ENGINEERS_TOOLBELT";

        public override string ItemPickupDesc => "Gain a small chance to duplicate drones and turrets on purchase. Drones and turrets have a small chance to revive themselves on death.";

        public override string ItemFullDescription => $"You have a <style=cIsUtility>{FloatToPercentageString(BaseDuplicationPercentChance)}</style> chance <style=cStack>(+{FloatToPercentageString(AdditionalDuplicationPercentChance)} " +
            $"hyperbolically up to a maximum of 100% chance)</style> to duplicate drones and turrets <style=cIsUtility>on purchase</style>.\n" +
            $"Additionally, you have a <style=cIsUtility>{FloatToPercentageString(BaseRevivalPercentChance)}</style> chance <style=cStack>(+{FloatToPercentageString(AdditionalRevivalPercentChance)} " +
            $"hyperbolically up to a maximum of 100% chance)</style> to revive drones and turrets when they <style=cDeath>die</style>.";

        public override string ItemLore => OrderManifestLoreFormatter(
            ItemName,

            "9/9/2079",

            "UES Safe Travels/Unmarked Sector/Outer Rim",

            "667********",

            ItemPickupDesc,

            "Next Day Delivery / Common Industrial / Small",

            "Hey Pal,\n" +
            "\nJust got your delivery request for a replacement toolbelt. Couldn't believe that the Safe Travels doesn't have a single one of them on board. " +
            "We've been running low on some supplies here so all this stuff is what I had on hand. Included in the belt there's everything your standard Drone and Turret repair technician would need." +
            "\n\nYou've got: \n\n" +
            "<indent=5%>- A flathead screwdriver</indent>\n" +
            "<indent=5%>- A entire pouch of Ionocell AA batteries (lucky you)</indent>\n" +
            "<indent=5%>- A drive ratchet</indent>\n" +
            "<indent=5%>- My daughter's Zebra print duct tape that she won't miss</indent>\n" +
            "<indent=5%>- A pouch filled to the brim with all the loose screws I could find.</indent>\n" +
            "<indent=5%>- Some nuts and bolts in a few pouches.</indent>\n" +
            "<indent=5%>- And you'll enjoy this, if you open any of the remaining pouches, you'll find I packed you some snacks in there. No telling if they'll be good to eat when this arrives.</indent>\n" +
            "\nBest Regards,\n" +
            "A Humble Technician\n" +
            "\nP.S. some Ionocell batteries might not survive the trip into hyperspace, but with the amount I've included if they don't make those bots pop up, try another pair.");

        public override ItemTier Tier => ItemTier.Tier2;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.AIBlacklist, ItemTag.Utility, ItemTag.InteractableRelated };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("EngineersToolbelt.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("EngineersToolbeltIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public static NetworkSoundEventDef ToolbeltRepairSound;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateSound();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            EnableSounds = config.ActiveBind<bool>("Item: " + ItemName, "Enable Sounds?", true, "Should this item be able to emit sounds in certain conditions?");

            BaseDuplicationPercentChance = config.ActiveBind<float>("Item: " + ItemName, "Base Duplication Percent Chance", 0.2f, "What chance in percentage should a drone or turret have of duplicating on purchase with the first stack of this?");
            AdditionalDuplicationPercentChance = config.ActiveBind<float>("Item: " + ItemName, "Additional Duplication Percentage Chance", 0.2f, "What chance in percentage should a drone or turret have of duplicating on purchase per additional stack? (hyperbolically)");

            BaseRevivalPercentChance = config.ActiveBind<float>("Item: " + ItemName, "Base Revival Percentage Chance", 0.1f, "What chance in percentage should a drone or turret have of reviving on death with the first stack of this?");
            AdditionalRevivalPercentChance = config.ActiveBind<float>("Item: " + ItemName, "Additional Revival Percentage Chance", 0.1f, "What chance in percentage should a drone or turret have of reviving on death per additional stack?");
        }

        private void CreateSound()
        {
            ToolbeltRepairSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            ToolbeltRepairSound.eventName = "Aetherium_Duplicate_Bot";

            R2API.ContentAddition.AddNetworkSoundEventDef(ToolbeltRepairSound);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.3f, 0.3f, 0.3f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, -0.043F, 0F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(0.22F, 0.22F, 0.22F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0F, -0.0624F, 0.0029F),
                    localAngles = new Vector3(0F, 90F, 180F),
                    localScale = new Vector3(0.16F, 0.16F, 0.16F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hip",
                    localPos = new Vector3(0F, 0.2173F, 0F),
                    localAngles = new Vector3(0F, 0F, 180F),
                    localScale = new Vector3(1.5F, 1.5F, 1.5F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0F, -0.0269F, 0F),
                    localAngles = new Vector3(0F, 90F, 180F),
                    localScale = new Vector3(0.28F, 0.28F, 0.28F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0F, -0.0505F, -0.0143F),
                    localAngles = new Vector3(0F, 90F, 194.7163F),
                    localScale = new Vector3(0.21F, 0.21F, 0.21F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0F, 0.0107F, -0.0013F),
                    localAngles = new Vector3(357.9873F, 79.9759F, 175.6149F),
                    localScale = new Vector3(0.22F, 0.22F, 0.22F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0F, -0.1437F, 0F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0F, 0.1073F, 0.0172F),
                    localAngles = new Vector3(0F, 90F, 180F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hip",
                    localPos = new Vector3(0.0439F, 0.4383F, -0.5598F),
                    localAngles = new Vector3(-0.0002F, 89.9997F, 165.5597F),
                    localScale = new Vector3(2F, 2F, 2F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0.0007F, 0.1481F, 0.0258F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(0.22F, 0.22F, 0.22F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0F, 0F, 0F),
                    localAngles = new Vector3(0F, 90F, 180F),
                    localScale = new Vector3(0.22F, 0.22F, 0.22F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Body",
                    localPos = new Vector3(0F, 0.00347F, -0.00126F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(0.01241F, 0.01241F, 0.01241F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0F, 0.32527F, 0.04552F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(0.32056F, 0.33391F, 0.34077F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00168F, -0.06608F, -0.01318F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(0.16335F, 0.16335F, 0.16335F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0F, -0.06584F, 0F),
                    localAngles = new Vector3(0F, 90F, 0F),
                    localScale = new Vector3(0.1456F, 0.1456F, 0.1456F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName =  "Pelvis",
                    localPos =   new Vector3(0.00784F, 0.05264F, 0.00931F), 
                    localAngles = new Vector3(8.1354F, 273.3271F, 181.3306F),
                    localScale = new Vector3(0.3227F, 0.31414F, 0.29262F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.00055F, 0.00145F, 0F),
                    localAngles = new Vector3(0F, 0F, 180F),
                    localScale = new Vector3(0.00849F, 0.00849F, 0.00849F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.PurchaseInteraction.OnInteractionBegin += DuplicateDronesAndTurrets;
            On.RoR2.CharacterMaster.OnBodyDeath += ReviveDronesAndTurrets;
        }

        private void ReviveDronesAndTurrets(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster master, CharacterBody characterBody)
        {
            if(NetworkServer.active && master && characterBody && master.IsDeadAndOutOfLivesServer())
            {
                if(!characterBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical))
                {
                    orig(master, characterBody);
                    return;
                }

                var minionOwnership = master.minionOwnership;
                if(minionOwnership?.ownerMaster)
                {
                    var ownerBody = minionOwnership.ownerMaster.GetBody();
                    if(ownerBody)
                    {
                        int inventoryCount = GetCount(ownerBody);
                        if(inventoryCount > 0)
                        {
                            float revivalChance = InverseHyperbolicScaling(BaseRevivalPercentChance, AdditionalRevivalPercentChance, 1, inventoryCount) * 100;

                            if(Util.CheckRoll(revivalChance, minionOwnership.ownerMaster))
                            {
                                master.inventory.GiveItem(RoR2Content.Items.ExtraLife);

                                if(!characterBody.GetComponent<EngineersToolbeltRevivalFlag>())
                                {
                                    characterBody.gameObject.AddComponent<EngineersToolbeltRevivalFlag>();
                                }
                            }
                        }
                    }
                }
            }

            orig(master, characterBody);
        }

        private void DuplicateDronesAndTurrets(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);

            if(!NetworkServer.active) return;
            if(!activator?.gameObject) return;
            if(self.GetInteractability(activator) != Interactability.Available) return;

            var characterBody = activator.gameObject.GetComponent<CharacterBody>();
            if(!characterBody) return;

            var inventoryCount = GetCount(characterBody);
            if(inventoryCount <= 0) return;

            var characterMaster = characterBody.master;
            if(!characterMaster) return;

            var summonMasterBehavior = self.gameObject.GetComponent<SummonMasterBehavior>();
            if(!summonMasterBehavior?.masterPrefab) return;

            var masterPrefabMaster = summonMasterBehavior.masterPrefab.GetComponent<CharacterMaster>();
            if(!masterPrefabMaster?.bodyPrefab) return;

            var duplicationBody = masterPrefabMaster.bodyPrefab.GetComponent<CharacterBody>();
            if(!duplicationBody) return;

            if(!duplicationBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical)) return;

            if(!Util.CheckRoll(InverseHyperbolicScaling(BaseDuplicationPercentChance, AdditionalDuplicationPercentChance, 1, inventoryCount) * 100, characterBody.master))
                return;

            Vector3 chosenPosition = FindClosestNodeToPosition(
                RandomPointOnCircle(self.transform.position, 5, Run.instance.stageRng),
                duplicationBody.hullClassification
            );

            CharacterMaster summonedDrone = new MasterSummon()
            {
                masterPrefab = summonMasterBehavior.masterPrefab,
                position = chosenPosition,
                rotation = self.transform.rotation,
                summonerBodyObject = activator.gameObject,
                ignoreTeamMemberLimit = true,
            }.Perform();

            if(!summonedDrone) return;

            if(summonMasterBehavior.masterPrefab.name.Contains("EquipmentDrone"))
            {
                summonedDrone.inventory.CopyEquipmentFrom(characterBody.inventory);
            }

            if(EnableSounds)
            {
                EntitySoundManager.EmitSoundServer(ToolbeltRepairSound.akId, summonedDrone.gameObject);
            }
        }


        public class EngineersToolbeltRevivalFlag : MonoBehaviour
        {
            public CharacterMaster ownerMaster;
            public CharacterMaster droneMaster;
            public CharacterBody droneBody;
            public CharacterBody ownerBody;

            private void Awake()
            {
                droneBody = gameObject.GetComponent<CharacterBody>();
                droneMaster = droneBody.master;
                ownerMaster = droneMaster.minionOwnership.ownerMaster;
                ownerBody = ownerMaster.GetBody();
            }
        }
    }
}