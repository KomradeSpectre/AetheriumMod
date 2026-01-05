using Aetherium.Utils;
using BepInEx.Configuration;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Artifacts
{
    public class ArtifactOfTheTyrant : ArtifactBase<ArtifactOfTheTyrant>
    {
        public ConfigOption<int> NumberOfEliteAffixesToGiveMithrix;
        public ConfigOption<string> BlacklistedAffixesString;      

        public override string ArtifactName => "Artifact of the Tyrant";
        public override string ArtifactLangTokenName => "ARTIFACT_OF_THE_TYRANT";
        public override string ArtifactDescription => $"Any time a route-ending boss spawns they will be given {NumberOfEliteAffixesToGiveMithrix} random elite modifier(s).";

        public override Sprite ArtifactEnabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfTheTyrantEnabledIcon.png");
        public override Sprite ArtifactDisabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfTheTyrantDisabledIcon.png");

        public string[] RouteEnderBossNames = new string[]
        {
            "BrotherBody", "BrotherGlassBody", "BrotherHauntBody", "BrotherHurtBody",
            "MiniVoidRaidCrabBody", "MiniVoidRaidCrabBodyBase", "MiniVoidRaidCrabBodyPhase1",
            "MiniVoidRaidCrabBodyPhase2", "MiniVoidRaidCrabBodyPhase3",
            "ScavLunar1Body", "ScavLunar2Body", "ScavLunar3Body", "ScavLunar4Body",
            "FalseSonBossBody", "FalseSonBossBodyLunarShard", "FalseSonBossBodyBrokenLunarShard",
        };

        private HashSet<BodyIndex> ValidBossIndices = new HashSet<BodyIndex>();

        private List<EliteDef> ValidEliteDefs = new List<EliteDef>();

        private List<string> BlacklistedAffixes = new List<string> { "AffixEcho" };

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            NumberOfEliteAffixesToGiveMithrix = config.ActiveBind<int>("Artifact: " + ArtifactName, "Number of Elite Affixes", 1, "How many elite statuses should Route-Ending bosses be granted?");
            BlacklistedAffixesString = config.ActiveBind<string>("Artifact: " + ArtifactName, "Blacklisted Affixes String", "", "Comma-separated list of ignored EliteDef names.");
        }

        public override void Hooks()
        {
            RoR2Application.onLoad += BuildCaches;
            On.RoR2.CharacterMaster.OnBodyStart += GiveBossEliteAffix;
        }

        private void BuildCaches()
        {
            ValidBossIndices.Clear();
            foreach (var bodyPrefab in BodyCatalog.allBodyPrefabs)
            {
                if(!bodyPrefab) continue;
                string prefabName = bodyPrefab.name;

                if(RouteEnderBossNames.Any(target => prefabName.Contains(target)))
                {
                    BodyIndex index = BodyCatalog.FindBodyIndex(bodyPrefab);
                    if(index != BodyIndex.None) ValidBossIndices.Add(index);
                }
            }

            ValidEliteDefs.Clear();

            if(BlacklistedAffixesString != null && !string.IsNullOrWhiteSpace(BlacklistedAffixesString.ToString()))
            {
                var split = BlacklistedAffixesString.ToString().Split(',');
                foreach (var s in split) BlacklistedAffixes.Add(s.Trim());
            }

            foreach (var elite in EliteCatalog.eliteDefs)
            {
                if(!elite.eliteEquipmentDef) continue;
                if(!elite.eliteEquipmentDef.passiveBuffDef) continue;

                if(BlacklistedAffixes.Contains(elite.name)) continue;
                if(BlacklistedAffixes.Contains(elite.eliteEquipmentDef.name)) continue;
                if(BlacklistedAffixes.Contains(elite.eliteEquipmentDef.passiveBuffDef.name)) continue;

                ValidEliteDefs.Add(elite);
            }
        }

        private void GiveBossEliteAffix(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
        {
            orig(self, body);

            if(!NetworkServer.active || !ArtifactEnabled || !body) return;

            if(ValidBossIndices.Contains(body.bodyIndex))
            {
                if(ValidEliteDefs.Count == 0) return;

                var candidates = ValidEliteDefs
                    .Where(x => !body.HasBuff(x.eliteEquipmentDef.passiveBuffDef))
                    .ToList();

                Util.ShuffleList(candidates, Run.instance.stageRng);

                int count = NumberOfEliteAffixesToGiveMithrix;
                for (int i = 0; i < count && i < candidates.Count; i++)
                {
                    EliteDef elite = candidates[i];

                    if(i == 0)
                    {
                        body.inventory.SetEquipmentIndex(elite.eliteEquipmentDef.equipmentIndex, false);
                    }
                    else
                    {
                        body.AddBuff(elite.eliteEquipmentDef.passiveBuffDef);
                    }
                }
            }
        }
    }
}