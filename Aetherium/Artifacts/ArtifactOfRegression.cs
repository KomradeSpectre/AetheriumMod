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

        internal Dictionary<string, RegressData> RegressionLookup = new Dictionary<string, RegressData>();

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
            new RegressData("BeetleQueenMaster", "BeetleGuardMaster", QueenToGuardSplitNumber).Register();
            new RegressData("BeetleGuardMaster", "BeetleMaster", GuardToBeetleSplitNumber).Register();
            new RegressData("BeetleGuardMasterCrystal", "BeetleCrystalMaster", CrystalGuardToCrystalBeetleSplitNumber).Register();

            new RegressData("AncientWispMaster", "ArchWispMaster", AncientWispToArchWispSplitNumber).Register();
            new RegressData("GravekeeperMaster", "GreaterWispMaster", GrovetenderToGreaterWispSplitNumber).Register();
            new RegressData("ArchWispMaster", "GreaterWispMaster", ArchWispToGreaterWispSplitNumber).Register();
            new RegressData("GreaterWispMaster", "WispMaster", GreaterWispToWispSplitNumber).Register();

            new RegressData("VagrantMaster", "JellyfishMaster", VagrantToJellyfishSplitNumber).Register();

            new RegressData("LemurianBruiserMaster", "LemurianMaster", ElderLemurianToLemurianSplitNumber).Register();
            new RegressData("LemurianBruiserMasterFire", "LemurianMaster", ElderLemurianToLemurianSplitNumber).Register();
            new RegressData("LemurianBruiserMasterHaunted", "LemurianMaster", ElderLemurianToLemurianSplitNumber).Register();
            new RegressData("LemurianBruiserMasterIce", "LemurianMaster", ElderLemurianToLemurianSplitNumber).Register();
            new RegressData("LemurianBruiserMasterPoison", "LemurianMaster", ElderLemurianToLemurianSplitNumber).Register();

            new RegressData("LunarWispMaster", "LunarGolemMaster", LunarWispToLunarGolemSplitNumber).Register();
            new RegressData("LunarGolemMaster", "LunarExploderMaster", LunarGolemToLunarExploderSplitNumber).Register();

            new RegressData("TitanMaster", "GolemMaster", TitanToGolemSplitNumber).Register();

            new RegressData("GrandparentMaster", "ParentMaster", GrandparentToParentSplitNumber).Register();

            new RegressData("SuperRoboBallBossMaster", "VultureMaster", GrandparentToParentSplitNumber).Register();

            new RegressData("RoboBallBossMaster", "RoboBallMiniMaster", SolusControlUnitToSolusProbeSplitNumber).Register();

            new RegressData("ClayBossMaster", "ClayBruiserMaster", ClayDunestriderToClayTemplarSplitNumber).Register();

            new RegressData("ImpBossMaster", "ImpMaster", ImpOverlordToImpSplitNumber).Register();

            new RegressData("NullifierMaster", "HermitCrabMaster", VoidReaverToHermitCrabSplitNumber).Register();

            //Sample Registration of multiple children.
            //RegressData beetleQueenRegression = new RegressData("BeetleQueenMaster");
            //beetleQueenRegression.Store("BeetleGuardMaster", QueenToGuardSplitNumber / 2);
            //beetleQueenRegression.Store("BeetleMaster", QueenToGuardSplitNumber / 2);
            //beetleQueenRegression.Register();

            //Sample modded registration.
            //new RegressData("ParentMaster", "ModdedChildMaster", myConfigSpawnAmount, myBundle.LoadAsset<GameObject>("my/modded/path.prefab"));

            LogRegisteredRegressions();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterAI.BaseAI.OnBodyDeath += RegressAIToLowerForm;
        }

        private void RegressAIToLowerForm(On.RoR2.CharacterAI.BaseAI.orig_OnBodyDeath orig, RoR2.CharacterAI.BaseAI self, CharacterBody characterBody)
        {
            CharacterMaster master = self.master;
            if (ArtifactEnabled && NetworkServer.active && IsExisting(master) && IsAnEnemy(master))
            {
                string masterName = master.name.Replace("(Clone)", "");
                if (RegressionLookup.ContainsKey(masterName))
                {
                    List<RegressData.ChildData> childrenList = RegressionLookup[masterName].children;
                    int totalChildrenCount = childrenList.Sum(child => child.count);
                    float theta = (float)Math.PI * 2 / totalChildrenCount;
                    int radius = totalChildrenCount;
                    int angleCounter = 0;
                    foreach (RegressData.ChildData child in RegressionLookup[masterName].children)
                    {
                        if (child.resource)
                        {
                            for (int i = 0; i < child.count; i++)
                            {
                                float angle = theta * ++angleCounter;
                                Vector3 positionChosen = new Vector3((float)(radius * Math.Cos(angle) + characterBody.corePosition.x),
                                                                     characterBody.corePosition.y + 2f,
                                                                     (float)(radius * Math.Sin(angle) + characterBody.corePosition.z));

                                CharacterMaster summonedThing = new MasterSummon()
                                {
                                    masterPrefab = child.resource,
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

        private bool IsExisting(CharacterMaster master)
        {
            return master && master.IsDeadAndOutOfLivesServer();
        }

        private bool IsAnEnemy(CharacterMaster master)
        {
            return master.teamIndex == TeamIndex.Monster || master.teamIndex == TeamIndex.Lunar;
        }

        private void LogRegisteredRegressions()
        {
            ModLogger.LogMessage("Artifact of Regression Lookup Table:");
            foreach (var pair in RegressionLookup)
            {
                ModLogger.LogMessage($"-> {pair.Key}");
                foreach (RegressData.ChildData child in pair.Value.children)
                {
                    ModLogger.LogMessage($"  -> {child.count} {child.name}");
                }
            }
        }
    }

    /// <summary>
    /// A data structure useful for storing children data of a parent and registering them.
    /// </summary>
    public struct RegressData
    {
        private readonly string parentName;
        internal List<ChildData> children;

        /// <summary>
        /// Constructor with no children assigned.
        /// </summary>
        /// <param name="parentName">Parent's master name</param>
        public RegressData(string parentName)
        {
            this.parentName = parentName;
            children = new List<ChildData>();
        }

        /// <summary>
        /// Constructor with an assignment of one child while specifying a resource.
        /// Pass null in resource to specify a vanilla resource.
        /// If it is a custom resource, load it from the bundle.
        /// </summary>
        /// <param name="parentName">Parent's master name</param>
        /// <param name="childName">Child's master name</param>
        /// <param name="count">Children amount to be produced by regression</param>
        /// <param name="resource">Prefab of the child object</param>
        public RegressData(string parentName, string childName, int count, GameObject resource) : this(parentName)
        {
            Store(childName, count, resource);
        }

        /// <summary>
        /// Constructor with an assignment of one child without specifying a resource, which means it will use a vanilla resource.
        /// </summary>
        /// <param name="parentName">Parent's master name</param>
        /// <param name="childName">Child's master name</param>
        /// <param name="count">Children amount to be produced by regression</param>
        public RegressData(string parentName, string childName, int count) : this(parentName, childName, count, null) { }

        /// <summary>
        /// Add or Modify a child for the parent specified within the data structure.
        /// Pass null in resource to specify a vanilla resource.
        /// If it is a custom resource, load it from the bundle.
        /// </summary>
        /// <param name="childName">Child's master name</param>
        /// <param name="count">Amount to be produced by regression</param>
        /// <param name="resource">Resource prefab of the child</param>
        public void Store(string childName, int count, GameObject resource)
        {
            if (!resource)
            {
                resource = Resources.Load<GameObject>($"Prefabs/CharacterMasters/{childName}");
            }
            children.Add(new ChildData(childName, count, resource));
        }

        /// <summary>
        /// Registers the regression data of the parent and children into the Regression Lookup used for implementing regression behavior.
        /// </summary>
        public void Register()
        {
            ArtifactOfRegression.instance.RegressionLookup[parentName] = this;
        }

        internal struct ChildData
        {
            public string name;
            public int count;
            public GameObject resource;

            public ChildData(string name, int count, GameObject resource)
            {
                this.name = name;
                this.count = count;
                this.resource = resource;
            }
        }
    }
}