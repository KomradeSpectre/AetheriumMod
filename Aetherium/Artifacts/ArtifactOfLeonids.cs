using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Aetherium.Utils;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Artifacts
{
    public class ArtifactOfLeonids : ArtifactBase<ArtifactOfLeonids>
    {
        public ConfigOption<float> MeteorShowerInterval;
        public ConfigOption<int> WavesPerMeteorStorm;

        public override string ArtifactName => "Artifact of Leonids";

        public override string ArtifactLangTokenName => "ARTIFACT_OF_LEONIDS";

        public override string ArtifactDescription => $"After every {MeteorShowerInterval} second(s) a meteor shower will occur that contains {WavesPerMeteorStorm} wave(s) of meteors.";

        public override Sprite ArtifactEnabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfLeonidsEnabledIcon.png");

        public override Sprite ArtifactDisabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfLeonidsDisabledIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            MeteorShowerInterval = config.ActiveBind<float>("Artifact: " + ArtifactName, "Interval Between Meteor Showers", 60, "How many seconds between each meteor shower?");
            WavesPerMeteorStorm = config.ActiveBind<int>("Artifact: " + ArtifactName, "Meteor Waves Per Shower", 6, "How many waves of meteors should be included in each meteor shower? Each wave contains 1 meteor per body on the map.");
        }

        public override void Hooks()
        {
            Run.onRunStartGlobal += AttachLeonidsController;
        }

        private void AttachLeonidsController(Run run)
        {
            if (NetworkServer.active && ArtifactEnabled)
            {
                var showerController = run.gameObject.AddComponent<LeonidsControllerComponent>();
                showerController.MeteorShowerInterval = MeteorShowerInterval;
                showerController.MeteorWaves = WavesPerMeteorStorm;
            }
        }

        public class LeonidsControllerComponent : MonoBehaviour
        {
            public Run Run;

            public float NextMeteorTime;

            public float MeteorShowerInterval;
            public int MeteorWaves;

            public void Start()
            {
                Run = gameObject.GetComponent<Run>();
                NextMeteorTime = MeteorShowerInterval;
            }

            public void FixedUpdate()
            {
                var currentTime = Run.GetRunStopwatch();

                if (currentTime > NextMeteorTime)
                {
                    //spawn meteors here
                    if (NetworkServer.active)
                    {
                        MeteorStormController component = UnityEngine.Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/MeteorStorm"), Vector3.zero, Quaternion.identity).GetComponent<MeteorStormController>();
                        component.owner = this.gameObject;
                        component.ownerDamage = 20 * Run.difficultyCoefficient;
                        component.waveCount = MeteorWaves;
                        NetworkServer.Spawn(component.gameObject);
                    }

                    NextMeteorTime = currentTime + MeteorShowerInterval;
                }
            }
        }
    }
}
