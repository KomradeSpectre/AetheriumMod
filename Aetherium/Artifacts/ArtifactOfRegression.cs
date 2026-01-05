using Aetherium.Utils;
using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Artifacts
{
    public class ArtifactOfRegression : ArtifactBase<ArtifactOfRegression>
    {
        public ConfigOption<int> RegressionSplitMonsterCap;
        public ConfigOption<bool> ReduceGoldAndExpOfChildren;
        public ConfigOption<float> ChildImmunityDuration;

        public override string ArtifactName => "Artifact of Regression";
        public override string ArtifactLangTokenName => "ARTIFACT_OF_REGRESSION";
        public override string ArtifactDescription => "When an evolved monster dies, it splits into its lesser forms.";
        public override Sprite ArtifactEnabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfRegressionEnabledIcon.png");
        public override Sprite ArtifactDisabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfRegressionDisabledIcon.png");

        private Dictionary<MasterCatalog.MasterIndex, List<RegressionChild>> RegressionCache = new Dictionary<MasterCatalog.MasterIndex, List<RegressionChild>>();

        private struct RegressionChild
        {
            public GameObject MasterPrefab;
            public int Count;
        }

        private List<RegressionDefinition> pendingDefinitions = new List<RegressionDefinition>();
        private struct RegressionDefinition
        {
            public string ParentName;
            public string ChildName;
            public ConfigOption<int> CountConfig;
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            // Global Settings
            RegressionSplitMonsterCap = config.ActiveBind<int>("Artifact: " + ArtifactName, "Monster Cap", 48, "At what monster population should we not be able to split anymore?");
            ReduceGoldAndExpOfChildren = config.ActiveBind<bool>("Artifact: " + ArtifactName, "Reduce Gold and Exp Reward of Children", true, "Should children spawned by the Regression effect have halved money and exp?");
            ChildImmunityDuration = config.ActiveBind<float>("Artifact: " + ArtifactName, "Duration of Child Immunity", 2f, "How long in seconds should children of regression splits be immune?");

            // --- WORM FAMILY (Overloading -> Magma -> Scorch) ---
            AddDefinition("ElectricWormMaster", "MagmaWormMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Overloading Worm -> Magma Worms", 1, "Amount of Magma Worms spawned."));
            AddDefinition("MagmaWormMaster", "ScorchWurmMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Magma Worm -> Scorch Wurms", 2, "Amount of Scorch Wurms spawned."));
            AddDefinition("ScorchWurmMaster", "LemurianMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Scorch Wurm -> Lemurians", 3, "Amount of Lemurians spawned."));

            // --- PARENT FAMILY (Grandparent -> Parent -> Child) ---
            AddDefinition("GrandparentMaster", "ParentMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Grandparent -> Parents", 2, "Amount of Parents spawned."));
            AddDefinition("ParentMaster", "ChildMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Parent -> Children", 3, "Amount of Children spawned."));

            // --- LEMURIAN FAMILY ---
            AddDefinition("LemurianBruiserMaster", "LemurianMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Elder Lemurian -> Lemurians", 4, "Amount of Lemurians spawned."));

            // --- BEETLE FAMILY ---
            AddDefinition("BeetleQueenMaster", "BeetleGuardMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Queen -> Guards", 2, "Amount of Beetle Guards spawned."));
            AddDefinition("BeetleGuardMaster", "BeetleMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Guard -> Beetles", 3, "Amount of Beetles spawned."));

            // --- IMP FAMILY ---
            AddDefinition("ImpBossMaster", "ImpMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Imp Overlord -> Imps", 5, "Amount of Imps spawned."));

            // --- STONE FAMILY ---
            AddDefinition("TitanMaster", "GolemMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Stone Titan -> Golems", 2, "Amount of Stone Golems spawned."));

            // --- CLAY FAMILY ---
            AddDefinition("ClayBossMaster", "ClayBruiserMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Dunestrider -> Templars", 2, "Amount of Clay Templars spawned."));
            AddDefinition("ClayBruiserMaster", "ClayManMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Templar -> Apothecaries", 2, "Amount of Clay Apothecaries spawned."));

            // --- JELLYFISH FAMILY ---
            AddDefinition("VagrantMaster", "JellyfishMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Wandering Vagrant -> Jellyfish", 6, "Amount of Jellyfish spawned."));

            // --- WISP FAMILY ---
            AddDefinition("GravekeeperMaster", "GreaterWispMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Grovetender -> Greater Wisps", 2, "Amount of Greater Wisps spawned."));
            AddDefinition("ArchWispMaster", "GreaterWispMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Arch Wisp -> Greater Wisps", 2, "Amount of Greater Wisps spawned."));
            AddDefinition("GreaterWispMaster", "WispMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Greater Wisp -> Lesser Wisps", 3, "Amount of Lesser Wisps spawned."));

            // --- ROBOT FAMILY ---
            AddDefinition("SuperRoboBallBossMaster", "RoboBallMiniMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Alloy Worship Unit -> Solus Probes", 5, "Amount of Solus Probes spawned."));
            AddDefinition("RoboBallBossMaster", "RoboBallMiniMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Solus Control Unit -> Solus Probes", 4, "Amount of Solus Probes spawned."));

            // --- LUNAR FAMILY ---
            AddDefinition("LunarWispMaster", "LunarGolemMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Lunar Wisp -> Lunar Golems", 2, "Amount of Lunar Golems spawned."));
            AddDefinition("LunarGolemMaster", "LunarExploderMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Lunar Golem -> Lunar Exploders", 4, "Amount of Lunar Exploders spawned."));

            // --- DLC1 & DLC2 EXTRAS ---
            AddDefinition("MegaConstructMaster", "MajorConstructMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Xi Construct -> Alpha Constructs", 2, "Amount of Alpha Constructs spawned."));
            AddDefinition("FalseSonBossMaster", "HalcyoniteMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "False Son -> Halcyonites", 2, "Amount of Halcyonites spawned."));
            AddDefinition("TitanGoldMaster", "HalcyoniteMaster",
                config.ActiveBind("Artifact: " + ArtifactName, "Aurelionite -> Halcyonites", 2, "Amount of Halcyonites spawned."));

            // Void Chain
            AddDefinition("VoidRaidCrabMaster", "VoidMegaCrabMaster", config.ActiveBind("Artifact: " + ArtifactName, "Voidling -> Devastators", 3, ""));
            AddDefinition("VoidMegaCrabMaster", "VoidJailerMaster", config.ActiveBind("Artifact: " + ArtifactName, "Devastator -> Jailers", 2, ""));
            AddDefinition("VoidJailerMaster", "NullifierMaster", config.ActiveBind("Artifact: " + ArtifactName, "Jailer -> Reavers", 2, ""));
            AddDefinition("NullifierMaster", "VoidBarnacleMaster", config.ActiveBind("Artifact: " + ArtifactName, "Reaver -> Barnacles", 3, ""));
        }

        private void AddDefinition(string parent, string child, ConfigOption<int> count)
        {
            pendingDefinitions.Add(new RegressionDefinition { ParentName = parent, ChildName = child, CountConfig = count });
        }

        public override void Hooks()
        {
            RoR2Application.onLoad += BuildRegressionCache;
            On.RoR2.CharacterAI.BaseAI.OnBodyDeath += OnBodyDeath;
        }

        private void BuildRegressionCache()
        {
            RegressionCache.Clear();

            foreach (var def in pendingDefinitions)
            {
                var parentIndex = MasterCatalog.FindMasterIndex(def.ParentName);
                if(parentIndex == MasterCatalog.MasterIndex.none) continue;

                var childPrefab = MasterCatalog.FindMasterPrefab(def.ChildName);
                if(!childPrefab) continue;

                if(!RegressionCache.ContainsKey(parentIndex))
                {
                    RegressionCache[parentIndex] = new List<RegressionChild>();
                }

                RegressionCache[parentIndex].Add(new RegressionChild
                {
                    MasterPrefab = childPrefab,
                    Count = def.CountConfig
                });
            }

            pendingDefinitions.Clear();
            AetheriumPlugin.ModLogger.LogInfo($"Artifact of Regression: Cached {RegressionCache.Count} parent types.");
        }

        private void OnBodyDeath(On.RoR2.CharacterAI.BaseAI.orig_OnBodyDeath orig, RoR2.CharacterAI.BaseAI self, CharacterBody body)
        {
            orig(self, body);

            if(!NetworkServer.active || !ArtifactEnabled || !self.master) return;

            int monsterCount = TeamComponent.GetTeamMembers(TeamIndex.Monster).Count + TeamComponent.GetTeamMembers(TeamIndex.Lunar).Count;
            if(monsterCount >= RegressionSplitMonsterCap) return;

            var masterIndex = self.master.masterIndex;
            if(RegressionCache.TryGetValue(masterIndex, out List<RegressionChild> children))
            {
                SpawnChildren(body, children);
            }
        }

        private void SpawnChildren(CharacterBody parentBody, List<RegressionChild> children)
        {
            int totalCount = children.Sum(x => x.Count);
            if(totalCount <= 0) return;

            float angleStep = 360f / totalCount;
            float currentAngle = 0f;
            float radius = 2f + parentBody.radius;

            foreach (var childDef in children)
            {
                for (int i = 0; i < childDef.Count; i++)
                {
                    Vector3 offset = Quaternion.Euler(0f, currentAngle, 0f) * Vector3.forward * radius;
                    Vector3 spawnPos = parentBody.corePosition + offset;

                    if(Physics.Raycast(spawnPos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 20f, LayerIndex.world.mask))
                    {
                        spawnPos = hit.point;
                    }

                    MasterSummon summon = new MasterSummon
                    {
                        masterPrefab = childDef.MasterPrefab,
                        position = spawnPos,
                        rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f),
                        teamIndexOverride = parentBody.teamComponent.teamIndex,
                        ignoreTeamMemberLimit = true,
                        summonerBodyObject = parentBody.gameObject,
                        inventoryToCopy = parentBody.inventory,
                        useAmbientLevel = true
                    };

                    CharacterMaster childMaster = summon.Perform();

                    if(childMaster)
                    {
                        CharacterBody childBody = childMaster.GetBody();
                        if(childBody)
                        {
                            childBody.AddTimedBuff(RoR2Content.Buffs.Immune, ChildImmunityDuration);

                            EffectManager.SimpleEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/CombatShrineSpawnEffect"), childBody.corePosition, Quaternion.identity, true);

                            if(ReduceGoldAndExpOfChildren)
                            {
                                var rewards = childBody.GetComponent<DeathRewards>();
                                if(rewards)
                                {
                                    rewards.goldReward = (uint)(rewards.goldReward * 0.5f);
                                    rewards.expReward = (uint)(rewards.expReward * 0.5f);
                                }
                            }
                        }
                    }

                    currentAngle += angleStep;
                }
            }
        }
    }
}