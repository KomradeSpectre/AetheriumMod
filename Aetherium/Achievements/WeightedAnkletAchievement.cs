using R2API;
using RoR2;
using RoR2.Achievements;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Achievements
{
    public class WeightedAnkletAchievement : ModdedUnlockable
    {
        public override string AchievementIdentifier => "AETHERIUM_ACHIEVEMENT_WEIGHTED_ANKLET_ID";

        public override string UnlockableIdentifier => "AETHERIUM_UNLOCKABLE_WEIGHTED_ANKLET_REWARD_ID";

        public override string PrerequisiteUnlockableIdentifier => "";

        public override string AchievementNameToken => "AETHERIUM_WEIGHTED_ANKLET_ACHIEVEMENT_NAME";

        public override string AchievementDescToken => "AETHERIUM_WEIGHTED_ANKLET_ACHIEVEMENT_DESC";

        public override string UnlockableNameToken => "AETHERIUM_WEIGHTED_ANKLET_UNLOCKABLE_NAME";

        public override Sprite Sprite => MainAssets.LoadAsset<Sprite>("ShieldingCoreAchievementIcon.png");

        public override Func<string> GetHowToUnlock { get; } = () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT",
            new object[]
            {
                Language.GetString("AETHERIUM_WEIGHTED_ANKLET_ACHIEVEMENT_NAME"),
                Language.GetString("AETHERIUM_WEIGHTED_ANKLET_ACHIEVEMENT_DESC"),
            });

        public override Func<string> GetUnlocked { get; } = () => Language.GetStringFormatted("UNLOCKED_FORMAT",
            new object[]
            {
                Language.GetString("AETHERIUM_WEIGHTED_ANKLET_ACHIEVEMENT_NAME"),
                Language.GetString("AETHERIUM_WEIGHTED_ANKLET_ACHIEVEMENT_DESC"),
            });

        public static void RegisterLanguage()
        {
            LanguageAPI.Add("AETHERIUM_WEIGHTED_ANKLET_ACHIEVEMENT_NAME", "Restraining Order");
            LanguageAPI.Add("AETHERIUM_WEIGHTED_ANKLET_ACHIEVEMENT_DESC", "Get thrown out of the bazaar.");
        }

        public override void OnInstall()
        {
            base.OnInstall();
            base.SetServerTracked(true);
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
        }

        public class WeightedAnkletServerAchievementTracker : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                On.RoR2.MapZone.TeleportBody += KickFromStoreGrant;
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
                On.RoR2.MapZone.TeleportBody -= KickFromStoreGrant;
            }

            private void KickFromStoreGrant(On.RoR2.MapZone.orig_TeleportBody orig, MapZone self, CharacterBody characterBody)
            {
                orig(self, characterBody);
                if(self.triggerType == MapZone.TriggerType.TriggerEnter && self.zoneType == MapZone.ZoneType.KickOutPlayers && RoR2.SceneCatalog.mostRecentSceneDef == RoR2.SceneCatalog.GetSceneDefFromSceneName("bazaar"))
                {
                    if (characterBody && base.IsCurrentBody(characterBody))
                    {
                        if (characterBody.healthComponent && characterBody.healthComponent.alive && characterBody.inventory)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                characterBody.inventory.GiveItem(Items.WeightedAnklet.instance.ItemDef);
                            }
                        }
                        base.Grant();
                    }
                }
            }
        }
    }
}
