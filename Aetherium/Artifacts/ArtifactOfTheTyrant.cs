using System;
using R2API;
using RoR2;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using BepInEx.Configuration;
using Aetherium.Utils;

using static Aetherium.AetheriumPlugin;
using UnityEngine.Networking;

namespace Aetherium.Artifacts
{
    public class ArtifactOfTheTyrant : ArtifactBase<ArtifactOfTheTyrant>
    {
        public ConfigOption<int> NumberOfEliteAffixesToGiveMithrix;

        public override string ArtifactName => "Artifact of the Tyrant";

        public override string ArtifactLangTokenName => "ARTIFACT_OF_THE_TYRANT";

        public override string ArtifactDescription => $"When enabled, any time Mithrix spawns they will be given {NumberOfEliteAffixesToGiveMithrix} random elite modifier(s).";

        public override Sprite ArtifactEnabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfTheTyrantEnabledIcon.png");

        public override Sprite ArtifactDisabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfTheTyrantDisabledIcon.png");

        public string[] MithrixNames = new string[] 
        {
            "BrotherBody",
            "BrotherGlassBody",
            "BrotherHauntBody",
            "BrotherHurtBody"
        };

        public string[] BlacklistedAffixes = new string[]
        {
            "AffixEcho"
        };

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            NumberOfEliteAffixesToGiveMithrix = config.ActiveBind<int>("Artifact: " + ArtifactName, "Number of Elite Affixes to Give Mithrix", 1, "How many elite statuses should Mithrix be granted by us?");
        }

        public override void Hooks()
        {
            On.RoR2.CharacterMaster.OnBodyStart += GiveMithrixEliteAffix;
        }

        private void GiveMithrixEliteAffix(On.RoR2.CharacterMaster.orig_OnBodyStart orig, RoR2.CharacterMaster self, RoR2.CharacterBody body)
        {
            orig(self, body);

            if (ArtifactEnabled && NetworkServer.active && body)
            {
                if (MithrixNames.Any(x => body.name.Contains(x)))
                {
                    var affixBuffs = BuffCatalog.buffDefs.Where(x => x.name.Contains("Affix") && !BlacklistedAffixes.Contains(x.name)).ToList();

                    var selectedBuffs = affixBuffs.Where(x => !body.HasBuff(x)).Shuffle(Run.instance.stageRng).Take(NumberOfEliteAffixesToGiveMithrix);

                    foreach (BuffDef affixBuff in selectedBuffs)
                    {
                        body.AddBuff(affixBuff);
                    }
                }
            }
        }
    }
}
