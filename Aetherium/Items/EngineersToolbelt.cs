using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Aetherium.Utils;
using static Aetherium.AetheriumPlugin;
using RoR2.CharacterAI;

namespace Aetherium.Items
{
    public class EngineersToolbelt : ItemBase<EngineersToolbelt>
    {
        public ConfigOption<float> BaseRevivalPercentChance;
        public ConfigOption<float> AdditionalRevivalPercentChance;
        public ConfigOption<float> MaximumRevivalPercentChance;

        public override string ItemName => "Engineers Toolbelt";

        public override string ItemLangTokenName => "ENGINEERS_TOOLBELT";

        public override string ItemPickupDesc => "You have a chance to revive drones and turrets when they die.";

        public override string ItemFullDescription => "You have a <style=cIsUtility>10%</style> chance <style=cStack>(+10% hyperbolically)</style> to revive drones and turrets when they <style=cDeath>die</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("EngineersToolbelt.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("EngineersToolbelt.png");

        public static GameObject ItemBodyModelPrefab;

        private static readonly List<string> DronesList = new List<string>
        {
            "DroneBackup",
            "Drone1",
            "Drone2",
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
            BaseRevivalPercentChance = config.ActiveBind<float>("Item: " + ItemName, "Base Revival Percentage Chance", 0.1f, "What chance in percentage should a drone or turret have of reviving on death with the first stack of this?");
            AdditionalRevivalPercentChance = config.ActiveBind<float>("Item: " + ItemName, "Additional Revival Percentage Chance", 0.1f, "What chance in percentage should a drone or turret have of reviving on death per additional stack?");
            MaximumRevivalPercentChance = config.ActiveBind<float>("Item: " + ItemName, "Maximum Revival Percentage Chance", 1f, "What is the maximum percent chance that a drone or turret should have of reviving on death?");
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
                    childName = "Pelvis",
                    localPos = new Vector3(0F, -0.043F, 0F),
                    localAngles = new Vector3(0F, 90F, 180F),
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
                    localPos = new Vector3(0F, -0.4395F, 0F),
                    localAngles = new Vector3(0F, 0F, 180F),
                    localScale = new Vector3(1.1F, 1.1F, 1.1F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0F, -0.006F, 0F),
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
                    localAngles = new Vector3(359.5742F, 79.7867F, 184.4904F),
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
                    childName = "Head",
                    localPos = new Vector3(0.0439F, 0.4547F, -0.5683F),
                    localAngles = new Vector3(19.1091F, 66.0667F, 27.681F),
                    localScale = new Vector3(2.1F, 2.1F, 2.1F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.0007F, -0.1231F, -0.0208F),
                    localAngles = new Vector3(353.1227F, 107.3045F, 179.6526F),
                    localScale = new Vector3(0.23F, 0.23F, 0.23F)
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
            On.RoR2.CharacterAI.BaseAI.OnBodyDeath += ReviveDronesAndTurrets;
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
                                    var shouldWeRevive = Util.CheckRoll((BaseRevivalPercentChance + (MaximumRevivalPercentChance - MaximumRevivalPercentChance / (1 + AdditionalRevivalPercentChance * (inventoryCount - 1)))) * 100, self.master.minionOwnership.ownerMaster);
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

        public class EngineersToolbeltRevivalComponent : MonoBehaviour
        {
            public CharacterMaster Owner;
            public CharacterMaster Master;

            public void FixedUpdate()
            {
                if(Master && Master.hasBody && Master.GetBody().healthComponent.alive)
                {
                    Master.destroyOnBodyDeath = true;

                    foreach(BaseAI ai in Master.aiComponents)
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
