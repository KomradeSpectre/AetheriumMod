using Aetherium.Utils;
using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Artifacts
{
    public class ArtifactOfRegression : ArtifactBase<ArtifactOfRegression>
    {
        public ConfigOption<int> QueenToGuardSplitNumber;
        public ConfigOption<int> GuardToBeetleSplitNumber;
        public ConfigOption<int> CrystalGuardToCrystalBeetleSplitNumber;

        public ConfigOption<int> AncientWispToArchWispSplitNumber;
        public ConfigOption<int> ArchWispToGreaterWispSplitNumber;
        public ConfigOption<int> GrovetenderToGreaterWispSplitNumber;
        public ConfigOption<int> GreaterWispToWispSplitNumber;

        public ConfigOption<int> VagrantToJellyfishSplitNumber;

        public ConfigOption<int> ElderLemurianToLemurianSplitNumber;

        public ConfigOption<int> LunarWispToLunarGolemSplitNumber;
        public ConfigOption<int> LunarGolemToLunarExploderSplitNumber;

        public ConfigOption<int> TitanToGolemSplitNumber;

        public ConfigOption<int> GrandparentToParentSplitNumber;

        public ConfigOption<int> AlloyWorshipUnitToVultureSplitNumber;
        public ConfigOption<int> SolusControlUnitToSolusProbeSplitNumber;

        public ConfigOption<int> ClayDunestriderToClayTemplarSplitNumber;

        public ConfigOption<int> ImpOverlordToImpSplitNumber;

        public ConfigOption<int> VoidReaverToHermitCrabSplitNumber;

        public override string ArtifactName => "Artifact of Regression";

        public override string ArtifactLangTokenName => "ARTIFACT_OF_REGRESSION";

        public override string ArtifactDescription => $"When enabled, if a monster is in an evolved form, it will split into a group of its lesser form when it dies.";

        public override Sprite ArtifactEnabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfRegressionEnabledIcon.png");

        public override Sprite ArtifactDisabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfRegressionDisabledIcon.png");

        public Dictionary<string, Tuple<string, ConfigOption<int>>> RegressionLookup;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateRegressionLookup();
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            QueenToGuardSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Queen to Guard Split Amount", 2, "How many Beetle Guards should appear when the Beetle Queen has regressed?");
            GuardToBeetleSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Guard to Beetle Split Amount", 4, "How many Beetles should appear when the Beetle Guard has regressed?");
            CrystalGuardToCrystalBeetleSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Crystal Guard to Crystal Beetle Split Amount", 4, "How many Crystal Beetles should appear when the Crystal Beetle Guard has regressed?");

            AncientWispToArchWispSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Ancient Wisp To Arch Wisp Split Amount", 2, "How many Arch Wisps should appear when the Ancient Wisp has regressed?");
            ArchWispToGreaterWispSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Arch Wisp To Greater Wisp Split Amount", 2, "How many Greater Wisps should appear when the Arch Wisp has regressed?");
            GrovetenderToGreaterWispSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Grovetender To Greater Wisp Split Amount", 2, "How many Greater Wisps should appear when the Grovetender has regressed?");
            GreaterWispToWispSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Greater Wisp To Wisp Split Amount", 4, "How many Wisps should appear when the Greater Wisp has regressed?");

            VagrantToJellyfishSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Wandering Vagrant To Jellyfish Split Amount", 8, "How many Jellyfish should appear when the Wandering Vagrant has regressed?");

            ElderLemurianToLemurianSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Elder Lemurian To Lemurian Split Amount", 5, "How many Lemurians should appear when the Elder Lemurian has regressed?");

            LunarWispToLunarGolemSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Lunar Wisp To Lunar Chimera Split Amount", 2, "How many Lunar Chimeras should appear when the Lunar Wisp has regressed?");
            LunarGolemToLunarExploderSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Lunar Chimera To Lunar Exploder Split Amount", 5, "How many Lunar Exploders should appear when the Lunar Chimera has regressed?");

            TitanToGolemSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Titan To Golem Split Amount", 4, "How many Golems should appear when the Titan has regressed?");

            GrandparentToParentSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Grandparent To Parent Split Amount", 5, "How many Parents should appear when the Grandparent has regressed?");

            AlloyWorshipUnitToVultureSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Alloy Worship Unit To Vulture Split Amount", 6, "How many Vultures should appear when the Alloy Worship Unit has regressed?");
            SolusControlUnitToSolusProbeSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Solus Control Unit To Solus Probe Split Amount", 6, "How many Solus Probes should appear when the Solus Control Unit has regressed?");

            ClayDunestriderToClayTemplarSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Clay Dunestrider To Clay Templar Split Amount", 4, "How many Clay Templars should appear when the Clay Dunestrider has regressed?");

            ImpOverlordToImpSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Imp Overlord To Imp Split Amount", 5, "How many Imps should appear when the Imp Overlord has regressed?");

            VoidReaverToHermitCrabSplitNumber = config.ActiveBind<int>("Artifact: " + ArtifactName, "Void Reaver To Hermit Crab Split Amount", 4, "How many Hermit Crabs should appear when the Void Reaver has regressed?");
        }

        private void CreateRegressionLookup()
        {
            RegressionLookup = new Dictionary<string, Tuple<string, ConfigOption<int>>>()
            {
                {"BeetleQueenMaster", Tuple.Create("BeetleGuardMaster", QueenToGuardSplitNumber)},
                {"BeetleGuardMaster", Tuple.Create("BeetleMaster", GuardToBeetleSplitNumber)},
                {"BeetleGuardMasterCrystal", Tuple.Create("BeetleCrystalMaster", CrystalGuardToCrystalBeetleSplitNumber)},

                {"AncientWispMaster", Tuple.Create("ArchWispMaster", AncientWispToArchWispSplitNumber)},
                {"GravekeeperMaster", Tuple.Create("GreaterWispMaster", ArchWispToGreaterWispSplitNumber)},
                {"ArchWispMaster", Tuple.Create("GreaterWispMaster", GrovetenderToGreaterWispSplitNumber)},
                {"GreaterWispMaster", Tuple.Create("WispMaster", GreaterWispToWispSplitNumber)},

                {"VagrantMaster", Tuple.Create("JellyfishMaster", VagrantToJellyfishSplitNumber)},

                {"LemurianBruiserMaster", Tuple.Create("LemurianMaster", ElderLemurianToLemurianSplitNumber)},
                {"LemurianBruiserMasterFire", Tuple.Create("LemurianMaster", ElderLemurianToLemurianSplitNumber)},
                {"LemurianBruiserMasterHaunted", Tuple.Create("LemurianMaster", ElderLemurianToLemurianSplitNumber)},
                {"LemurianBruiserMasterIce", Tuple.Create("LemurianMaster", ElderLemurianToLemurianSplitNumber)},
                {"LemurianBruiserMasterPoison", Tuple.Create("LemurianMaster", ElderLemurianToLemurianSplitNumber)},

                {"LunarWispMaster", Tuple.Create("LunarGolemMaster", LunarWispToLunarGolemSplitNumber)},
                {"LunarGolemMaster", Tuple.Create("LunarExploderMaster", LunarGolemToLunarExploderSplitNumber)},

                {"TitanMaster", Tuple.Create("GolemMaster", TitanToGolemSplitNumber)},

                {"GrandparentMaster", Tuple.Create("ParentMaster", GrandparentToParentSplitNumber)},

                {"SuperRoboBallBossMaster", Tuple.Create("VultureMaster", AlloyWorshipUnitToVultureSplitNumber)},

                {"RoboBallBossMaster", Tuple.Create("RoboBallMiniMaster", SolusControlUnitToSolusProbeSplitNumber)},

                {"ClayBossMaster", Tuple.Create("ClayBruiserMaster", ClayDunestriderToClayTemplarSplitNumber)},

                {"ImpBossMaster", Tuple.Create("ImpMaster", ImpOverlordToImpSplitNumber)},

                {"NullifierMaster", Tuple.Create("HermitCrabMaster", VoidReaverToHermitCrabSplitNumber)}
            };
        }

        public override void Hooks()
        {
            On.RoR2.CharacterAI.BaseAI.OnBodyDeath += RegressAIToLowerForm;
        }

        private void RegressAIToLowerForm(On.RoR2.CharacterAI.BaseAI.orig_OnBodyDeath orig, RoR2.CharacterAI.BaseAI self, CharacterBody characterBody)
        {
            if (NetworkServer.active && ArtifactEnabled)
            {
                if (self.master && self.master.IsDeadAndOutOfLivesServer() && (self.master.teamIndex == TeamIndex.Monster || self.master.teamIndex == TeamIndex.Lunar))
                {
                    var masterName = self.name.Replace("(Clone)", "");
                    if (RegressionLookup.ContainsKey(masterName))
                    {
                        var regressionMasterName = RegressionLookup[masterName].Item1;
                        var regressionSpawnAmount = RegressionLookup[masterName].Item2;

                        var masterPrefab = Resources.Load<GameObject>($"Prefabs/CharacterMasters/{regressionMasterName}");

                        if (masterPrefab)
                        {
                            for(int i = 0; i < regressionSpawnAmount; i++)
                            {
                                var theta = (Math.PI * 2) / regressionSpawnAmount;
                                var angle = theta * i;
                                var radius = regressionSpawnAmount;
                                var positionChosen = new Vector3((float)(radius * Math.Cos(angle) + characterBody.corePosition.x), characterBody.corePosition.y + 2f, (float)(radius * Math.Sin(angle) + characterBody.corePosition.z));

                                CharacterMaster summonedThing = new MasterSummon()
                                {
                                    masterPrefab = masterPrefab,
                                    position = positionChosen,
                                    rotation = characterBody.transform.rotation,
                                    summonerBodyObject = characterBody.gameObject,
                                    ignoreTeamMemberLimit = true,
                                    inventoryToCopy = characterBody.inventory ? characterBody.inventory : null                                    

                                }.Perform();

                                if (summonedThing)
                                {
                                    EffectManager.SimpleEffect(Resources.Load<GameObject>("prefabs/effects/CombatShrineSpawnEffect"), summonedThing.transform.position, summonedThing.transform.rotation, true);
                                }
                            }
                        }
                    }
                }
            }
            orig(self, characterBody);
        }
    }
}
