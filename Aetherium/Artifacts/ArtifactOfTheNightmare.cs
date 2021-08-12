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
    public class ArtifactOfTheNightmare : ArtifactBase<ArtifactOfTheNightmare>
    {
        public override string ArtifactName => "Artifact of the Nightmare";

        public override string ArtifactLangTokenName => "ARTIFACT_OF_THE_NIGHTMARE";

        public override string ArtifactDescription => "When enabled, non-boss enemies are granted 1-4 of the Lunar Heresy items when they spawn.";

        public override Sprite ArtifactEnabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfTheNightmareEnabledIcon.png");

        public override Sprite ArtifactDisabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfTheNightmareDisabledIcon.png");

        public List<ItemDef> HeresyItemList = new List<ItemDef>()
        {
            RoR2Content.Items.LunarPrimaryReplacement,
            RoR2Content.Items.LunarSecondaryReplacement,
            RoR2Content.Items.LunarUtilityReplacement,
            RoR2Content.Items.LunarSpecialReplacement
        };

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterMaster.OnBodyStart += GiveLunarHeresyItems;
        }

        private void GiveLunarHeresyItems(On.RoR2.CharacterMaster.orig_OnBodyStart orig, RoR2.CharacterMaster self, RoR2.CharacterBody body)
        {
            orig(self, body);

            if (NetworkServer.active && ArtifactEnabled)
            {
                if (!self.isBoss && self.teamIndex != TeamIndex.Player && self.inventory)
                {
                    var itemsToGiveEnemy = HeresyItemList.Where(x => body.inventory.GetItemCount(x) <= 0).Shuffle(Run.instance.stageRng).Take(Run.instance.stageRng.RangeInt(1, 5));

                    foreach(ItemDef item in itemsToGiveEnemy)
                    {
                        self.inventory.GiveItem(item);
                    }
                }
            }
        }
    }
}
