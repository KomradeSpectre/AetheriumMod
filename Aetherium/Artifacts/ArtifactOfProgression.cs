using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using R2API;
using static Aetherium.Utils.MiscHelpers;
using static Aetherium.AetheriumPlugin;
using UnityEngine.Networking;
using Aetherium.Utils;
using RoR2.Audio;
using System.Linq;

namespace Aetherium.Artifacts
{
    public class ArtifactOfProgression : ArtifactBase<ArtifactOfProgression>
    {
        public static ConfigOption<float> ProgressionInterval;
        public static ConfigOption<bool> EnableSounds;
        public static ConfigOption<bool> DoubleGoldAndExpOfProgressions;
        public static ConfigOption<string> BlacklistedEvolutionMastersString;

        public override string ArtifactName => "Artifact of Progression";

        public override string ArtifactLangTokenName => "ARTIFACT_OF_PROGRESSION";

        public override string ArtifactDescription => "Most enemies will evolve into stronger versions of themselves after a duration.";

        public override Sprite ArtifactEnabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfProgressionEnabledIcon.png");

        public override Sprite ArtifactDisabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfProgressionDisabledIcon.png");

        internal Dictionary<string, EvolutionData> ProgressionLookup = new Dictionary<string, EvolutionData>();

        public delegate bool EvolutionDataHandler(EvolutionData evolutionData);
        public event EvolutionDataHandler onProgressionLookupChanged;

        public static BuffDef ProgressionStartBuff;
        public static BuffDef ProgressionQuarterBuff;
        public static BuffDef ProgressionHalfBuff;
        public static BuffDef ProgressionThreeQuartersBuff;

        public static GameObject ProgressionSoundEffectHolder;

        public static NetworkSoundEventDef ProgressionSquelchEvent;

        public List<CharacterMaster> BlacklistedEvolutionMasters = new List<CharacterMaster>();

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateProgressionLookup();
            CreateLang();
            CreateSound();
            CreateBuff();
            CreateArtifact();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            ProgressionInterval = config.ActiveBind<float>("Artifact: " + ArtifactName, "Interval Between Each Progression", 60, "How long until a monster progresses to the next progression state if they have one?");
            EnableSounds = config.ActiveBind<bool>("Artifact:" + ArtifactName, "Enable Progression Sounds?", true, "Should a sound play each time a monster reaches a progression checkpoint?");
            DoubleGoldAndExpOfProgressions = config.ActiveBind<bool>("Artifact: " + ArtifactName, "Double Gold and Exp Reward of Progressions", true, "Should progressions spawned by the Progression effect have doubled money and exp?");
            BlacklistedEvolutionMastersString = config.ActiveBind<string>("Artifact: " + ArtifactName, "Blacklisted Evolution Masters", "", "Which enemies master components should be blacklisted from evolving? (Each entry should be separated by a comma, you must know their internal master name to blacklist them. E.g. impmaster,lemurianmaster will blacklist normal imps and lemurians.");
        }

        private void CreateSound()
        {
            ProgressionSquelchEvent = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            ProgressionSquelchEvent.eventName = "Aetherium_Progression_Squelch";

            R2API.ContentAddition.AddNetworkSoundEventDef(ProgressionSquelchEvent);
        }

        private void CreateBuff()
        {
            ProgressionStartBuff = ScriptableObject.CreateInstance<BuffDef>();
            ProgressionStartBuff.name = "Aetherium: Progression Start Buff";
            ProgressionStartBuff.buffColor = new Color(255, 255, 255);
            ProgressionStartBuff.canStack = false;
            ProgressionStartBuff.isDebuff = false;
            ProgressionStartBuff.iconSprite = MainAssets.LoadAsset<Sprite>("ProgressionStartBuffIcon.png");
            ContentAddition.AddBuffDef(ProgressionStartBuff);

            ProgressionQuarterBuff = ScriptableObject.CreateInstance<BuffDef>();
            ProgressionQuarterBuff.name = "Aetherium: Progression Quarter Buff";
            ProgressionQuarterBuff.buffColor = new Color(255, 255, 255);
            ProgressionQuarterBuff.canStack = false;
            ProgressionQuarterBuff.isDebuff = false;
            ProgressionQuarterBuff.iconSprite = MainAssets.LoadAsset<Sprite>("ProgressionQuarterBuffIcon.png");
            ContentAddition.AddBuffDef(ProgressionQuarterBuff);

            ProgressionHalfBuff = ScriptableObject.CreateInstance<BuffDef>();
            ProgressionHalfBuff.name = "Aetherium: Progression Half Buff";
            ProgressionHalfBuff.buffColor = new Color(255, 255, 255);
            ProgressionHalfBuff.canStack = false;
            ProgressionHalfBuff.isDebuff = false;
            ProgressionHalfBuff.iconSprite = MainAssets.LoadAsset<Sprite>("ProgressionHalfBuffIcon.png");
            ContentAddition.AddBuffDef(ProgressionHalfBuff);

            ProgressionThreeQuartersBuff = ScriptableObject.CreateInstance<BuffDef>();
            ProgressionThreeQuartersBuff.name = "Aetherium: Progression Three Quarters Buff";
            ProgressionThreeQuartersBuff.buffColor = new Color(255, 255, 255);
            ProgressionThreeQuartersBuff.canStack = false;
            ProgressionThreeQuartersBuff.isDebuff = false;
            ProgressionThreeQuartersBuff.iconSprite = MainAssets.LoadAsset<Sprite>("ProgressionThreeQuartersBuffIcon.png");
            ContentAddition.AddBuffDef(ProgressionThreeQuartersBuff);
        }

        private void CreateProgressionLookup()
        {
            new EvolutionData("BeetleMaster", "BeetleGuardMaster").Register();
            new EvolutionData("BeetleGuardMaster", "BeetleQueenMaster").Register();

            new EvolutionData("ImpMaster", "ImpBossMaster").Register();

            new EvolutionData("JellyfishMaster", "VagrantMaster").Register();

            new EvolutionData("LemurianMaster", "LemurianBruiserMaster").Register();

            new EvolutionData("WispMaster", "GreaterWispMaster").Register();

            new EvolutionData("ParentMaster", "GrandparentMaster").Register();

            new EvolutionData("ClayBruiserMaster", "ClayBossMaster").Register();

            new EvolutionData("GolemMaster", "TitanMaster").Register();

            new EvolutionData("VultureMaster", "SuperRoboBallBossMaster").Register();

            new EvolutionData("RoboBallMiniMaster", "RoboBallBossMaster").Register();

            new EvolutionData("LunarExploderMaster", "LunarGolemMaster").Register();
            new EvolutionData("LunarGolemMaster", "LunarWispMaster").Register();

            new EvolutionData("HermitCrabMaster", "NullifierMaster").Register();
        }

        public override void Hooks()
        {
            On.RoR2.Run.Start += BlacklistSpecificEvolutions;
            On.RoR2.CharacterAI.BaseAI.OnBodyStart += AddEvolutionComponent;
        }

        private void BlacklistSpecificEvolutions(On.RoR2.Run.orig_Start orig, Run self)
        {
            string testString = BlacklistedEvolutionMastersString;
            var testStringArray = testString.Split(',');
            if (testStringArray.Length > 0)
            {
                foreach (string stringToTest in testStringArray)
                {
                    var master = Array.Find<CharacterMaster>(RoR2.MasterCatalog.masterPrefabMasterComponents, x => x.name.ToLowerInvariant() == stringToTest.ToLowerInvariant());
                    if (!master) { continue; }

                    BlacklistedEvolutionMasters.Add(master);
                }
            }

            orig(self);
        }

        private void AddEvolutionComponent(On.RoR2.CharacterAI.BaseAI.orig_OnBodyStart orig, RoR2.CharacterAI.BaseAI self, CharacterBody newBody)
        {
            orig(self, newBody);
            if(NetworkServer.active && ArtifactEnabled)
            {
                if (self.master && self.master.teamIndex != TeamIndex.Player && newBody && !newBody.isBoss)
                {
                    var masterName = self.master.name.Replace("(Clone)", "");
                    if (!BlacklistedEvolutionMasters.Any(master => master.name.ToLowerInvariant() == masterName.ToLowerInvariant()) && ProgressionLookup.ContainsKey(masterName))
                    {
                        List<EvolutionData.EvolvedStateData> evolutionData = ProgressionLookup[masterName].PossibleNextEvolutions;

                        GameObject choice;

                        if(evolutionData.Count > 1)
                        {
                            choice = evolutionData[Run.instance.stageRng.RangeInt(0, evolutionData.Count)].Resource;
                        }
                        else
                        {
                            choice = evolutionData[0].Resource;
                        }

                        if (!choice)
                        {
                            Debug.LogError($"Registered next evolutionary stage for {self.body.name} has an invalid resource. Aborting evolution.");
                            return;
                        }

                        var evolutionManagerComponent = newBody.GetComponent<EvolutionManagerComponent>();
                        if (!evolutionManagerComponent)
                        {
                            evolutionManagerComponent = newBody.gameObject.AddComponent<EvolutionManagerComponent>();
                            evolutionManagerComponent.EvolutionInterval = ProgressionInterval;
                            evolutionManagerComponent.EvolutionMasterPrefab = choice;
                        }
                    }
                }
            }
        }

        public class EvolutionManagerComponent : NetworkBehaviour
        {
            private CharacterBody Body;

            [SyncVar]
            public GameObject EvolutionMasterPrefab;

            public List<Material> MaterialsOfBody;

            private CharacterMaster Master;

            [SyncVar]
            public float Timer;

            public float EvolutionInterval;

            public void Start()
            {
                Body = gameObject.GetComponent<CharacterBody>();
                Master = Body.master;

                if (NetworkServer.active)
                {
                    Body.AddBuff(ProgressionStartBuff);
                    PlayEvolutionaryStepVFX(1, Color.red);
                }
            }

            public void FixedUpdate()
            {
                Timer += Time.fixedDeltaTime;

                if (NetworkServer.active)
                {
                    if (Timer > EvolutionInterval * 0.25f && Timer <= EvolutionInterval * 0.5f && !Body.HasBuff(ProgressionQuarterBuff))
                    {
                        if (Body.HasBuff(ProgressionStartBuff)) { Body.RemoveBuff(ProgressionStartBuff); }
                        Body.AddBuff(ProgressionQuarterBuff);
                        PlayEvolutionaryStepVFX(2, Color.yellow);
                    }

                    if (Timer > EvolutionInterval * 0.5f && Timer <= EvolutionInterval * 0.75f && !Body.HasBuff(ProgressionHalfBuff))
                    {
                        if (Body.HasBuff(ProgressionQuarterBuff)) { Body.RemoveBuff(ProgressionQuarterBuff); }
                        Body.AddBuff(ProgressionHalfBuff);
                        PlayEvolutionaryStepVFX(3, Color.green);
                    }

                    if (Timer > EvolutionInterval * 0.75f && Timer <= EvolutionInterval && !Body.HasBuff(ProgressionThreeQuartersBuff))
                    {
                        if (Body.HasBuff(ProgressionHalfBuff)) { Body.RemoveBuff(ProgressionHalfBuff); }
                        Body.AddBuff(ProgressionThreeQuartersBuff);
                        PlayEvolutionaryStepVFX(4, Color.white);
                    }

                    if (Timer > EvolutionInterval && Master)
                    {
                        Destroy(this);

                        CharacterMaster summonedThing = new MasterSummon()
                        {
                            masterPrefab = EvolutionMasterPrefab,
                            position = Body.corePosition,
                            rotation = Body.transform.rotation,
                            summonerBodyObject = Body.gameObject,
                            ignoreTeamMemberLimit = true,
                            inventoryToCopy = Body.inventory ? Body.inventory : null
                        }.Perform();

                        var summonBody = summonedThing.GetBody();
                        if (summonBody)
                        {
                            summonBody.AddTimedBuff(RoR2Content.Buffs.Immune, 2);
                            var summonDeathRewards = summonBody.GetComponent<DeathRewards>();
                            var originalBodyDeathRewards = Body.GetComponent<DeathRewards>();

                            if (summonDeathRewards && originalBodyDeathRewards)
                            {
                                summonDeathRewards.expReward = DoubleGoldAndExpOfProgressions ? originalBodyDeathRewards.expReward * 2 : originalBodyDeathRewards.expReward;
                                summonDeathRewards.goldReward = DoubleGoldAndExpOfProgressions ? originalBodyDeathRewards.goldReward * 2 : originalBodyDeathRewards.goldReward;
                            }
                        }

                        Master.TrueKill();
                    }
                }
            }

            private void PlayEvolutionaryStepVFX(float scale, Color color)
            {
                EffectData effectData = new EffectData()
                {
                    color = color,
                    origin = Body.transform.position,
                    rotation = Body.transform.rotation,
                    scale = scale                    
                };

                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/LevelUpEffectEnemy"), effectData, true);
                EntitySoundManager.EmitSoundServer(ProgressionSquelchEvent.akId, Body.gameObject);
            }
        }
    }

    /// <summary>
    /// A data structure useful for storing next evolution data of a master and registering them.
    /// </summary>
    public struct EvolutionData
    {
        public string CurrentEvolutionName;
        internal List<EvolvedStateData> PossibleNextEvolutions;

        /// <summary>
        /// Constructor with no next evolutions assigned.
        /// </summary>
        /// <param name="currentEvolutionName">Name of the current evolution State. In other words, the current master name.</param>
        public EvolutionData(string currentEvolutionName)
        {
            CurrentEvolutionName = currentEvolutionName;
            PossibleNextEvolutions = new List<EvolvedStateData>();
        }

        /// <summary>
        /// Constructor with an assignment of one next evolution while specifying a resource.
        /// Pass null in resource to specify a vanilla resource.
        /// If it is a custom resource, load it from the bundle.
        /// </summary>
        /// <param name="currentEvolutionName">The name of the current evolution state. In other words, the current master name.</param>
        /// <param name="nextEvolutionName">The name of the next evolution state. In other words, the next master name.</param>
        /// <param name="resource">Prefab of the next body state.</param>
        public EvolutionData(string currentEvolutionName, string nextEvolutionName, GameObject resource) : this(currentEvolutionName)
        {
            Store(nextEvolutionName, resource);
        }

        /// <summary>
        /// Constructor with an assignment of one next evolution without specifying a resource, which means it will use a vanilla resource.
        /// </summary>
        /// <param name="currentEvolutionName">The name of the current evolution state. In other words, the current master name.</param>
        /// <param name="nextEvolutionName">The name of the next evolution state. In other words, the next master name.</param>
        public EvolutionData(string currentEvolutionName, string nextEvolutionName) : this(currentEvolutionName, nextEvolutionName, null) { }

        /// <summary>
        /// Add or Modify a next evolution for the current evolution specified within the data structure.
        /// Pass null in resource to specify a vanilla resource.
        /// If it is a custom resource, load it from the bundle.
        /// </summary>
        /// <param name="nextEvolutionName">The name of the next evolution state. In other words, the next master name.</param>
        /// <param name="resource">Prefab of the next master state.</param>
        public void Store(string nextEvolutionName, GameObject resource)
        {
            if (!resource)
            {
                resource = LegacyResourcesAPI.Load<GameObject>($"Prefabs/CharacterMasters/{nextEvolutionName}");
            }
            PossibleNextEvolutions.Add(new EvolvedStateData(nextEvolutionName, resource));
        }

        public void Register()
        {
            ArtifactOfProgression.instance.ProgressionLookup[CurrentEvolutionName] = this;
        }

        internal struct EvolvedStateData
        {
            public string EvolvedStateName;
            public GameObject Resource;

            public EvolvedStateData(string evolvedStateName, GameObject resource)
            {
                EvolvedStateName = evolvedStateName;
                Resource = resource;
            }
        }
    }
}
