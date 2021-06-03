using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using R2API;
using static Aetherium.AetheriumPlugin;
using UnityEngine.Networking;

namespace Aetherium.Artifacts
{
    public class ArtifactOfProgression : ArtifactBase<ArtifactOfProgression>
    {
        public override string ArtifactName => "Artifact of Progression";

        public override string ArtifactLangTokenName => "ARTIFACT_OF_PROGRESSION";

        public override string ArtifactDescription => "When enabled, most enemies will evolve into stronger versions of themselves after a duration.";

        public override Sprite ArtifactEnabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfRegressionEnabledIcon.png");

        public override Sprite ArtifactDisabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfRegressionDisabledIcon.png");

        internal Dictionary<string, EvolutionData> ProgressionLookup = new Dictionary<string, EvolutionData>();

        public static BuffDef ProgressionStartBuff;
        public static BuffDef ProgressionQuarterBuff;
        public static BuffDef ProgressionHalfBuff;
        public static BuffDef ProgressionThreeQuartersBuff;

        public override void Init(ConfigFile config)
        {
            CreateProgressionLookup();
            CreateLang();
            CreateBuff();
            CreateArtifact();
            Hooks();
        }

        private void CreateBuff()
        {
            ProgressionStartBuff = ScriptableObject.CreateInstance<BuffDef>();
            ProgressionStartBuff.name = "Aetherium: Progression Start Buff";
            ProgressionStartBuff.buffColor = new Color(255, 255, 255);
            ProgressionStartBuff.canStack = false;
            ProgressionStartBuff.isDebuff = false;
            ProgressionStartBuff.iconSprite = MainAssets.LoadAsset<Sprite>("ProgressionStartBuffIcon.png");
            BuffAPI.Add(new CustomBuff(ProgressionStartBuff));

            ProgressionQuarterBuff = ScriptableObject.CreateInstance<BuffDef>();
            ProgressionQuarterBuff.name = "Aetherium: Progression Quarter Buff";
            ProgressionQuarterBuff.buffColor = new Color(255, 255, 255);
            ProgressionQuarterBuff.canStack = false;
            ProgressionQuarterBuff.isDebuff = false;
            ProgressionQuarterBuff.iconSprite = MainAssets.LoadAsset<Sprite>("ProgressionQuarterBuffIcon.png");
            BuffAPI.Add(new CustomBuff(ProgressionQuarterBuff));

            ProgressionHalfBuff = ScriptableObject.CreateInstance<BuffDef>();
            ProgressionHalfBuff.name = "Aetherium: Progression Half Buff";
            ProgressionHalfBuff.buffColor = new Color(255, 255, 255);
            ProgressionHalfBuff.canStack = false;
            ProgressionHalfBuff.isDebuff = false;
            ProgressionHalfBuff.iconSprite = MainAssets.LoadAsset<Sprite>("ProgressionHalfBuffIcon.png");
            BuffAPI.Add(new CustomBuff(ProgressionHalfBuff));

            ProgressionThreeQuartersBuff = ScriptableObject.CreateInstance<BuffDef>();
            ProgressionThreeQuartersBuff.name = "Aetherium: Progression Three Quarters Buff";
            ProgressionThreeQuartersBuff.buffColor = new Color(255, 255, 255);
            ProgressionThreeQuartersBuff.canStack = false;
            ProgressionThreeQuartersBuff.isDebuff = false;
            ProgressionThreeQuartersBuff.iconSprite = MainAssets.LoadAsset<Sprite>("ProgressionThreeQuartersBuffIcon.png");
            BuffAPI.Add(new CustomBuff(ProgressionThreeQuartersBuff));
        }

        private void CreateProgressionLookup()
        {
            new EvolutionData("BeetleBody", "BeetleGuardBody").Register();
            new EvolutionData("BeetleGuardBody", "BeetleQueen2Body").Register();

            new EvolutionData("ImpBody", "ImpBossBody").Register();

            new EvolutionData("JellyfishBody", "VagrantBody").Register();

            new EvolutionData("LemurianBody", "LemurianBruiserBody").Register();

            new EvolutionData("WispBody", "GreaterWispBody").Register();

            new EvolutionData("ParentBody", "GrandparentBody").Register();

            new EvolutionData("ClayBruiserBody", "ClayBossBody").Register();

            new EvolutionData("GolemBody", "TitanBody").Register();

            new EvolutionData("VultureBody", "SuperRoboBallBossBody").Register();

            new EvolutionData("RoboBallMiniBody", "RoboBallBossBody").Register();

            new EvolutionData("LunarExploderBody", "LunarGolemBody").Register();
            new EvolutionData("LunarGolemBody", "LunarWispBody").Register();

            new EvolutionData("HermitCrabBody", "NullifierBody").Register();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterAI.BaseAI.OnBodyStart += AddEvolutionComponent;
        }

        private void AddEvolutionComponent(On.RoR2.CharacterAI.BaseAI.orig_OnBodyStart orig, RoR2.CharacterAI.BaseAI self, CharacterBody newBody)
        {
            orig(self, newBody);
            if(NetworkServer.active && ArtifactEnabled)
            {
                if (self.master && self.master.teamIndex != TeamIndex.Player && newBody && !newBody.isBoss)
                {
                    var bodyName = self.body.name.Replace("(Clone)", "");
                    if (ProgressionLookup.ContainsKey(bodyName))
                    {
                        List<EvolutionData.EvolvedStateData> evolutionData = ProgressionLookup[bodyName].PossibleNextEvolutions;

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
                            evolutionManagerComponent.EvolutionInterval = 30;
                            evolutionManagerComponent.EvolutionBodyName = choice.name;
                        }
                    }
                }
            }
        }

        public class EvolutionManagerComponent : MonoBehaviour
        {
            private CharacterBody Body;
            public string EvolutionBodyName;

            private CharacterMaster Master;

            public float Timer;
            public float EvolutionInterval;

            public void Start()
            {
                Body = gameObject.GetComponent<CharacterBody>();
                Master = Body.master;

                if (NetworkServer.active)
                {
                    Body.AddBuff(ProgressionStartBuff);
                    PlayEvolutionaryStepVFX(1);
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
                        PlayEvolutionaryStepVFX(2);
                    }

                    if (Timer > EvolutionInterval * 0.5f && Timer <= EvolutionInterval * 0.75f && !Body.HasBuff(ProgressionHalfBuff))
                    {
                        if (Body.HasBuff(ProgressionQuarterBuff)) { Body.RemoveBuff(ProgressionQuarterBuff); }
                        Body.AddBuff(ProgressionHalfBuff);
                        PlayEvolutionaryStepVFX(3);
                    }

                    if (Timer > EvolutionInterval * 0.75f && Timer <= EvolutionInterval && !Body.HasBuff(ProgressionThreeQuartersBuff))
                    {
                        if (Body.HasBuff(ProgressionHalfBuff)) { Body.RemoveBuff(ProgressionHalfBuff); }
                        Body.AddBuff(ProgressionThreeQuartersBuff);
                        PlayEvolutionaryStepVFX(4);
                    }
                }

                if (Timer > EvolutionInterval && Master)
                {
                    Destroy(this);
                    Master.TransformBody(EvolutionBodyName);
                }
            }

            private void PlayEvolutionaryStepVFX(float scale)
            {
                EffectData effectData = new EffectData()
                {
                    origin = Body.transform.position,
                    rotation = Body.transform.rotation,
                    scale = scale
                };

                EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/ShrineUseEffect"), effectData, true);
            }
        }
    }

    /// <summary>
    /// A data structure useful for storing next evolution data of a body and registering them.
    /// </summary>
    public struct EvolutionData
    {
        public string CurrentEvolutionName;
        internal List<EvolvedStateData> PossibleNextEvolutions;

        /// <summary>
        /// Constructor with no next evolutions assigned.
        /// </summary>
        /// <param name="currentEvolutionName">Name of the current evolution State. In other words, the current body name.</param>
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
        /// <param name="currentEvolutionName">The name of the current evolution state. In other words, the current body name.</param>
        /// <param name="nextEvolutionName">The name of the next evolution state. In other words, the next body name.</param>
        /// <param name="resource">Prefab of the next body state.</param>
        public EvolutionData(string currentEvolutionName, string nextEvolutionName, GameObject resource) : this(currentEvolutionName)
        {
            Store(nextEvolutionName, resource);
        }

        /// <summary>
        /// Constructor with an assignment of one next evolution without specifying a resource, which means it will use a vanilla resource.
        /// </summary>
        /// <param name="currentEvolutionName">The name of the current evolution state. In other words, the current body name.</param>
        /// <param name="nextEvolutionName">The name of the next evolution state. In other words, the next body name.</param>
        public EvolutionData(string currentEvolutionName, string nextEvolutionName) : this(currentEvolutionName, nextEvolutionName, null) { }

        /// <summary>
        /// Add or Modify a next evolution for the current evolution specified within the data structure.
        /// Pass null in resource to specify a vanilla resource.
        /// If it is a custom resource, load it from the bundle.
        /// </summary>
        /// <param name="nextEvolutionName">The name of the next evolution state. In other words, the next body name.</param>
        /// <param name="resource">Prefab of the next body state.</param>
        public void Store(string nextEvolutionName, GameObject resource)
        {
            if (!resource)
            {
                resource = Resources.Load<GameObject>($"Prefabs/characterbodies/{nextEvolutionName}");
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
