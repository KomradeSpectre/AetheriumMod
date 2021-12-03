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
    public class UnstableDesignAchievement : ModdedUnlockable
    {
        public override string AchievementIdentifier => "AETHERIUM_ACHIEVEMENT_UNSTABLE_DESIGN_ID";

        public override string UnlockableIdentifier => "AETHERIUM_UNLOCKABLE_UNSTABLE_DESIGN_REWARD_ID";

        public override string PrerequisiteUnlockableIdentifier => "";

        public override string AchievementNameToken => "AETHERIUM_UNSTABLE_DESIGN_ACHIEVEMENT_NAME";

        public override string AchievementDescToken => "AETHERIUM_UNSTABLE_DESIGN_ACHIEVEMENT_DESC";

        public override string UnlockableNameToken => "AETHERIUM_UNSTABLE_DESIGN_UNLOCKABLE_NAME";

        public override Sprite Sprite => MainAssets.LoadAsset<Sprite>("UnstableDesignAchievementIcon.png");

        public override Func<string> GetHowToUnlock { get; } = () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT",
            new object[]
            {
                Language.GetString("AETHERIUM_UNSTABLE_DESIGN_ACHIEVEMENT_NAME"),
                Language.GetString("AETHERIUM_UNSTABLE_DESIGN_ACHIEVEMENT_DESC"),
            });

        public override Func<string> GetUnlocked { get; } = () => Language.GetStringFormatted("UNLOCKED_FORMAT",
            new object[]
            {
                Language.GetString("AETHERIUM_UNSTABLE_DESIGN_ACHIEVEMENT_NAME"),
                Language.GetString("AETHERIUM_UNSTABLE_DESIGN_ACHIEVEMENT_DESC"),
            });

        public static void RegisterLanguage()
        {
            LanguageAPI.Add("AETHERIUM_UNSTABLE_DESIGN_ACHIEVEMENT_NAME", "First Step into Madness");
            LanguageAPI.Add("AETHERIUM_UNSTABLE_DESIGN_ACHIEVEMENT_DESC", "Kill or die to a Perfected Lunar Chimera.");
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

        public class UnstableDesignServerAchievementTracker : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                GlobalEventManager.onCharacterDeathGlobal += CheckIfDiedToOrKilledPefectedLunarChimera;
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
                GlobalEventManager.onCharacterDeathGlobal -= CheckIfDiedToOrKilledPefectedLunarChimera;
            }

            private void CheckIfDiedToOrKilledPefectedLunarChimera(DamageReport report)
            {
                if (report is null || !report.attackerBody || !report.victimBody) { return; }

                if(report.attackerBodyIndex == BodyCatalog.FindBodyIndex("LunarGolemBody"))
                {
                    if(report.attackerBody.inventory && report.attackerBody.inventory.currentEquipmentIndex == EquipmentCatalog.FindEquipmentIndex("AffixLunar") || report.attackerBody.HasBuff(RoR2Content.Buffs.AffixLunar))
                    {
                        if(report.victimBody == base.GetCurrentBody())
                        {
                            base.Grant();
                        }
                    }
                }
                else if(report.victimBodyIndex == BodyCatalog.FindBodyIndex("LunarGolemBody"))
                {
                    if (report.victimBody.inventory && report.victimBody.inventory.currentEquipmentIndex == EquipmentCatalog.FindEquipmentIndex("AffixLunar") || report.victimBody.HasBuff(RoR2Content.Buffs.AffixLunar))
                    {
                        if(report.attackerBody == base.GetCurrentBody())
                        {
                            base.Grant();
                        }
                    }
                }
            }
        }
    }
}
