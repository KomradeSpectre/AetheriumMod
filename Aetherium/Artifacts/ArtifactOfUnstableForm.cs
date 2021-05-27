using Aetherium.Utils;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;

using static Aetherium.AetheriumPlugin;
using UnityEngine.Networking;
using System.Linq;

namespace Aetherium.Artifacts
{
    public class ArtifactOfUnstableForm : ArtifactBase<ArtifactOfUnstableForm>
    {
        public ConfigOption<float> TimeBetweenShapeshifts;

        public override string ArtifactName => "Artifact of Unstable Form";

        public override string ArtifactLangTokenName => "ARTIFACT_OF_UNSTABLE_FORM";

        public override string ArtifactDescription => $"When enabled, non-boss enemies will shapeshift every {TimeBetweenShapeshifts} second(s).";

        public override Sprite ArtifactEnabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfTheTyrantEnabledIcon.png");

        public override Sprite ArtifactDisabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfTheTyrantDisabledIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            TimeBetweenShapeshifts = config.ActiveBind<float>("Artifact: " + ArtifactName, "Interval Between Shapeshifts", 60, "How long should the duration between shapeshifts be?");
        }

        public override void Hooks()
        {
            Run.onRunStartGlobal += AttachShapeshifterComponent;
        }

        private void AttachShapeshifterComponent(Run run)
        {
            if(NetworkServer.active && ArtifactEnabled)
            {
                var shapeshiftController = run.gameObject.AddComponent<ShapeshifterMaster>();
                shapeshiftController.ShapeshiftInterval = TimeBetweenShapeshifts;
            }
        }

        public class ShapeshifterMaster : MonoBehaviour
        {
            public Run Run;

            public float NextShapeshiftTime;
            public float ShapeshiftInterval;

            public void Start()
            {
                Run = gameObject.GetComponent<Run>();
                NextShapeshiftTime = ShapeshiftInterval;
            }

            public void FixedUpdate()
            {
                if (Stage.instance.sceneDef.cachedName == "Bazaar")
                {
                    return;
                }
                var currentTime = Run.GetRunStopwatch();

                if (currentTime > NextShapeshiftTime)
                {
                    if (NetworkServer.active)
                    {
                        var mastersToTransform = CharacterMaster.readOnlyInstancesList.Where(x => !x.GetComponent<PlayerCharacterMasterController>() && x.GetBody() && x.teamIndex != TeamIndex.Player && !x.isBoss).ToList();

                        foreach(CharacterMaster master in mastersToTransform)
                        {
                            var bodyString = BodyCatalog.bodyPrefabs[Run.instance.stageRng.RangeInt(0, BodyCatalog.bodyPrefabs.Length)].name;
                            master.TransformBody(bodyString);
                        }
                    }

                    NextShapeshiftTime = currentTime + ShapeshiftInterval;
                }
            }
        }
    }
}
