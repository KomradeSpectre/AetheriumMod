using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Aetherium.Utils;
using ItemStats;
using ItemStats.Stat;
using ItemStats.ValueFormatters;
using AK.Wwise;

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MathHelpers;
using static Aetherium.Utils.MiscUtils;
using static Aetherium.Compatability.ModCompatability.ItemStatsModCompat;
using RoR2.CharacterAI;
using System.Runtime.CompilerServices;

namespace Aetherium.Items
{
    public class EngineersToolbelt : ItemBase<EngineersToolbelt>
    {
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

        public override bool AIBlacklisted => true;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("EngineersToolbelt.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("EngineersToolbeltIcon.png");

        public static GameObject ItemBodyModelPrefab;

        private static readonly List<string> DronesList = new List<string>
        {
            "DroneBackup",
            "Drone1",
            "Drone2",
            "EquipmentDrone",
            "EmergencyDrone",
            "FlameDrone",
            "MegaDrone",
            "DroneMissile",
            "Turret1"
        };

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            BaseDuplicationPercentChance = config.ActiveBind<float>("Item: " + ItemName, "Base Duplication Percent Chance", 0.2f, "What chance in percentage should a drone or turret have of duplicating on purchase with the first stack of this?");
            AdditionalDuplicationPercentChance = config.ActiveBind<float>("Item: " + ItemName, "Additional Duplication Percentage Chance", 0.2f, "What chance in percentage should a drone or turret have of duplicating on purchase per additional stack? (hyperbolically)");

            BaseRevivalPercentChance = config.ActiveBind<float>("Item: " + ItemName, "Base Revival Percentage Chance", 0.1f, "What chance in percentage should a drone or turret have of reviving on death with the first stack of this?");
            AdditionalRevivalPercentChance = config.ActiveBind<float>("Item: " + ItemName, "Additional Revival Percentage Chance", 0.1f, "What chance in percentage should a drone or turret have of reviving on death per additional stack?");
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
            return rules;
        }

        public override void Hooks()
        {
            if (IsItemStatsModInstalled)
            {
                RoR2Application.onLoad += ItemStatsModCompat;
            }

            On.RoR2.PurchaseInteraction.OnInteractionBegin += DuplicateDronesAndTurrets;
            On.RoR2.CharacterAI.BaseAI.OnBodyDeath += ReviveDronesAndTurrets;
        }

        private void ItemStatsModCompat()
        {
            CreateEngineersToolbeltStatDef();
        }

        private void DuplicateDronesAndTurrets(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            GameObject duplicatedDrone = null;

            if (NetworkServer.active)
            {
                if (self.name.Contains("Drone") || self.name.Contains("Turret"))
                {
                    var droneName = self.name.Replace("Broken", "").Replace("(Clone)", "") + "Master";

                    var masterPrefab = Resources.Load<GameObject>($"Prefabs/CharacterMasters/" + droneName);

                    if (masterPrefab)
                    {
                        if (activator && activator.gameObject && self.GetInteractability(activator) == Interactability.Available)
                        {
                            var characterBody = activator.gameObject.GetComponent<CharacterBody>();
                            var inventoryCount = GetCount(characterBody);

                            if (characterBody && characterBody.master && inventoryCount > 0)
                            {
                                if (Util.CheckRoll(InverseHyperbolicScaling(BaseDuplicationPercentChance, AdditionalDuplicationPercentChance, 1, inventoryCount) * 100, characterBody.master))
                                {
                                    Vector3 chosenPosition = Vector3.zero;

                                    var masterPrefabMaster = masterPrefab.GetComponent<CharacterMaster>();
                                    if (masterPrefabMaster && masterPrefabMaster.bodyPrefab)
                                    {
                                        var duplicationBody = masterPrefabMaster.bodyPrefab.GetComponent<CharacterBody>();
                                        if (duplicationBody)
                                        {
                                            chosenPosition = FindClosestGroundNodeToPosition(RandomPointOnCircle(self.transform.position, 5, Run.instance.stageRng), duplicationBody.hullClassification);
                                        }
                                    }

                                    CharacterMaster summonedDrone = new MasterSummon()
                                    {
                                        masterPrefab = masterPrefab,
                                        position = chosenPosition != Vector3.zero ? chosenPosition : self.transform.position,
                                        rotation = self.transform.rotation,
                                        summonerBodyObject = activator.gameObject,
                                        ignoreTeamMemberLimit = true,
                                    }.Perform();

                                    if (droneName == "EquipmentDroneMaster")
                                    {
                                        summonedDrone.inventory.CopyEquipmentFrom(characterBody.inventory);
                                    }

                                    duplicatedDrone = summonedDrone.gameObject;
                                }
                            }
                        }
                    }
                }
            }

            if (duplicatedDrone)
            {
                AkSoundEngine.PostEvent(2596808706, duplicatedDrone);
            }

            orig(self, activator);
        }

        private void ReviveDronesAndTurrets(On.RoR2.CharacterAI.BaseAI.orig_OnBodyDeath orig, RoR2.CharacterAI.BaseAI self, CharacterBody characterBody)
        {
            if (NetworkServer.active)
            {
                if (characterBody && !characterBody.isPlayerControlled)
                {
                    if (self.master && self.master.IsDeadAndOutOfLivesServer() && self.master.minionOwnership && self.master.minionOwnership.ownerMaster && self.master.minionOwnership.ownerMaster.GetBody())
                    {
                        var ownerBody = self.master.minionOwnership.ownerMaster.GetBody();
                        var inventoryCount = GetCount(ownerBody);
                        if (inventoryCount > 0)
                        {
                            foreach (string droneName in DronesList)
                            {
                                if (characterBody.name.Contains(droneName))
                                {
                                    var shouldWeRevive = Util.CheckRoll(InverseHyperbolicScaling(BaseRevivalPercentChance, AdditionalRevivalPercentChance, 1, inventoryCount) * 100, self.master.minionOwnership.ownerMaster);
                                    if (shouldWeRevive)
                                    {
                                        var originalOwner = self.master.minionOwnership.ownerMaster;

                                        var engineerRevivalComponent = self.gameObject.GetComponent<EngineersToolbeltRevivalComponent>();
                                        if (!engineerRevivalComponent) { engineerRevivalComponent = self.gameObject.AddComponent<EngineersToolbeltRevivalComponent>(); }

                                        engineerRevivalComponent.Owner = originalOwner;
                                        engineerRevivalComponent.Master = self.master;

                                        self.master.destroyOnBodyDeath = false;
                                        self.master.RespawnExtraLife();

                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            orig(self, characterBody);
        }

        /// <summary>
        /// Allows a custom drone to be revived by Engineer's Toolbelt.
        /// </summary>
        /// <param name="bodyName">The CharacterBody name of the custom drone.</param>
        /// <returns>True if the custom drone is now supported. False if the custom drone is already supported.</returns>
        public bool AddCustomDrone(string bodyName)
        {
            if (DronesList.Exists(item => item == bodyName)) return false;
            DronesList.Add(bodyName);
            return true;
        }

        public class EngineersToolbeltRevivalComponent : MonoBehaviour
        {
            public CharacterMaster Owner;
            public CharacterMaster Master;

            public void FixedUpdate()
            {
                if (Master && Master.hasBody && Master.GetBody().healthComponent.alive)
                {
                    Master.destroyOnBodyDeath = true;

                    foreach (BaseAI ai in Master.aiComponents)
                    {
                        ai.leader.gameObject = Owner.gameObject;
                    }

                    var aiOwnership = Master.GetComponent<AIOwnership>();
                    aiOwnership.ownerMaster = Owner;

                    Master.minionOwnership.SetOwner(Owner);
                    Master.teamIndex = Owner.teamIndex;
                    UnityEngine.Object.Destroy(this);
                }
            }
        }
    }
}