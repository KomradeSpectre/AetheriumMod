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

        public override GameObject ItemModel => new GameObject();

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("FeatheredPlume");

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
            return new ItemDisplayRuleDict();
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
