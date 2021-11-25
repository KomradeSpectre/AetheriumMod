using static Aetherium.AetheriumPlugin;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using MonoMod.Cil;

namespace Aetherium.Artifacts
{
    public class ArtifactOfTheJourney : ArtifactBase<ArtifactOfTheJourney>
    {
        public override string ArtifactName => "Artifact of the Journey";

        public override string ArtifactLangTokenName => "ARTIFACT_OF_THE_JOURNEY";

        public override string ArtifactDescription => "Most stages will be randomly picked. After a certain amount, a primordial teleporter will be placed.";

        public override Sprite ArtifactEnabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfTheJourneyEnabledIcon.png");

        public override Sprite ArtifactDisabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfTheJourneyDisabledIcon.png");

        public List<string> TeleporterNames = new List<string>
        {
            "spawncards/interactablespawncard/iscLunarTeleporter",
            "spawncards/interactablespawncard/iscTeleporter"
        };

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.Run.Start += ChangeStageOrderToRandom;
            On.RoR2.BazaarController.SetUpSeerStations += ChangeSeersToRandom;
            On.RoR2.SceneDirector.PlaceTeleporter += ChangeTeleporter;
        }

        private void ChangeTeleporter(On.RoR2.SceneDirector.orig_PlaceTeleporter orig, SceneDirector self)
        {
            if (ArtifactEnabled)
            {
                if(self.teleporterSpawnCard != null)
                {
                    if (Run.instance.NetworkstageClearCount % Run.stagesPerLoop == Run.stagesPerLoop - 1)
                    {
                        self.teleporterSpawnCard = Resources.Load<InteractableSpawnCard>(TeleporterNames[0]);
                    }
                    else
                    {
                        self.teleporterSpawnCard = Resources.Load<InteractableSpawnCard>(TeleporterNames[1]);
                    }
                }
            }
            orig(self);
        }

        private void ChangeSeersToRandom(On.RoR2.BazaarController.orig_SetUpSeerStations orig, BazaarController self)
        {
            if (ArtifactEnabled)
            {
                SceneDef nextStageScene = Run.instance.nextStageScene;
                List<SceneDef> list = new List<SceneDef>();
                if (nextStageScene != null)
                {
                    int stageOrder = nextStageScene.stageOrder;
                    foreach (SceneDef sceneDef in SceneCatalog.allSceneDefs)
                    {
                        if (sceneDef.destinations.Length > 0)
                        {
                            list.Add(sceneDef);
                        }
                    }
                }
                foreach (SeerStationController seerStationController in self.seerStations)
                {
                    if (list.Count == 0)
                    {
                        seerStationController.GetComponent<PurchaseInteraction>().SetAvailable(false);
                    }
                    else
                    {
                        Util.ShuffleList<SceneDef>(list, self.rng);
                        int index = list.Count - 1;
                        SceneDef targetScene = list[index];
                        list.RemoveAt(index);
                        if (self.rng.nextNormalizedFloat < 0.05f)
                        {
                            targetScene = SceneCatalog.GetSceneDefFromSceneName("goldshores");
                        }
                        seerStationController.SetTargetScene(targetScene);
                    }
                }
            }
            else
            {
                orig(self);
            }
        }

        private void ChangeStageOrderToRandom(On.RoR2.Run.orig_Start orig, RoR2.Run self)
        {
            if (ArtifactEnabled)
            {
                self.ruleBook.ApplyChoice(RuleCatalog.FindChoiceDef("Misc.StageOrder.Random"));
            }
            orig(self);
        }
    }
}
