using Aetherium.Utils;
using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Artifacts
{
    public class ArtifactOfIndecision : ArtifactBase<ArtifactOfIndecision>
    {
        public static ConfigOption<float> EliteChangeInterval;

        public override string ArtifactName => "Artifact of Indecision";

        public override string ArtifactLangTokenName => "ARTIFACT_OF_INDECISION";

        public override string ArtifactDescription => $"After every {EliteChangeInterval} second(s) the powers of all active elites will shift.";

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
            EliteChangeInterval = config.ActiveBind<float>("Artifact: " + ArtifactName, "Interval Between Elite Shifts", 60, "How many seconds between each elite type shift?");
        }

        public override void Hooks()
        {
            Run.onRunStartGlobal += AttachEliteShifter;
        }

        private void AttachEliteShifter(Run run)
        {
            if (NetworkServer.active && ArtifactEnabled)
            {
                var shifterController = run.gameObject.AddComponent<EliteShifterComponent>();
                shifterController.EliteShiftInterval = EliteChangeInterval;
            }
        }

        public class EliteShifterComponent : MonoBehaviour
        {
            public Run Run;

            public float NextEliteShiftTime;

            public float EliteShiftInterval;

            public void Start()
            {
                Run = gameObject.GetComponent<Run>();
                NextEliteShiftTime = EliteShiftInterval;
            }

            public void FixedUpdate()
            {
                if (Stage.instance.sceneDef.cachedName == "Bazaar")
                {
                    return;
                }
                var currentTime = Run.GetRunStopwatch();

                if (currentTime > NextEliteShiftTime)
                {
                    if (NetworkServer.active)
                    {
                        var mastersToTransform = CharacterMaster.readOnlyInstancesList;

                        foreach (CharacterMaster master in mastersToTransform)
                        {
                            var body = master.GetBody();
                            if (body && body.inventory && body.eliteBuffCount > 0)
                            {
                                var slotCount = body.inventory.GetEquipmentSlotCount();
                                List<EliteDef> currentlyOwnedEliteDefinitions = new List<EliteDef>();
                                for(int i = 0; i < slotCount; i++)
                                {
                                    var equipmentDef = body.inventory.GetEquipment((uint)i).equipmentDef;
                                    if (equipmentDef && equipmentDef.passiveBuffDef && equipmentDef.passiveBuffDef.eliteDef)
                                    {
                                      body.inventory.SetEquipmentIndex(EquipmentCatalog.equipmentDefs.Where(x => x.passiveBuffDef && x.passiveBuffDef.eliteDef;
                                    }
                                }                                
                            }
                        }
                    }

                    NextEliteShiftTime = currentTime + EliteShiftInterval;
                }
            }
        }

    }
}
