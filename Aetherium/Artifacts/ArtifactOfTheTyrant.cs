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

        public override string ArtifactDescription => $"Any time Mithrix spawns they will be given {NumberOfEliteAffixesToGiveMithrix} random elite modifier(s).";

        public override Sprite ArtifactEnabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfTheTyrantEnabledIcon.png");

        public override Sprite ArtifactDisabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfTheTyrantDisabledIcon.png");

        public string[] MithrixNames = new string[] 
        {
            "BrotherBody",
            "BrotherGlassBody",
            "BrotherHauntBody",
            "BrotherHurtBody"
        };

        public List<string> BlacklistedAffixes = new List<string>()
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

            var blacklistString = config.ActiveBind<string>("Artifact: " + ArtifactName, "Blacklisted Affixes String", "", "What affixes should be blacklisted from Artifact of the Tyrant? (generally no spaces, comma delimited)");

            if (!String.IsNullOrWhiteSpace(blacklistString))
            {
                var blacklistedStringArray = blacklistString.ToString().Split(',');

                foreach(string blacklistedEntry in blacklistedStringArray)
                {
                    BlacklistedAffixes.Add(blacklistedEntry);
                }
            }
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
                    var eliteDefs = EliteCatalog.eliteDefs.Where(x => x.eliteEquipmentDef && x.eliteEquipmentDef.passiveBuffDef && !BlacklistedAffixes.Contains(x.eliteEquipmentDef.name));

                    var selectedEliteDefs = eliteDefs.Where(x => !body.HasBuff(x.eliteEquipmentDef.passiveBuffDef)).Shuffle(Run.instance.stageRng).Take(NumberOfEliteAffixesToGiveMithrix);

                    if (selectedEliteDefs.Any())
                    {
                        body.inventory.GiveEquipmentString(selectedEliteDefs.First().eliteEquipmentDef.name);

                        foreach (EliteDef eliteDef in selectedEliteDefs.Skip(1))
                        {
                            body.AddBuff(eliteDef.eliteEquipmentDef.passiveBuffDef);
                        }
                    }
                }
            }
        }
    }
}
