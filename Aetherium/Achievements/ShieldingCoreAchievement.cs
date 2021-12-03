using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using static Aetherium.AetheriumPlugin;
using RoR2.Achievements;

namespace Aetherium.Achievements
{
    public class ShieldingCoreAchievement : ModdedUnlockable
    {
        public override string AchievementIdentifier => "AETHERIUM_ACHIEVEMENT_SHIELDING_CORE_ID";

        public override string UnlockableIdentifier => "AETHERIUM_UNLOCKABLE_SHIELDING_CORE_REWARD_ID";

        public override string PrerequisiteUnlockableIdentifier => "";

        public override string AchievementNameToken => "AETHERIUM_SHIELDING_CORE_ACHIEVEMENT_NAME";

        public override string AchievementDescToken => "AETHERIUM_SHIELDING_CORE_ACHIEVEMENT_DESC";

        public override string UnlockableNameToken => "AETHERIUM_SHIELDING_CORE_UNLOCKABLE_NAME";

        public override Sprite Sprite => MainAssets.LoadAsset<Sprite>("ShieldingCoreAchievementIcon.png");

        public override Func<string> GetHowToUnlock { get; } = () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT",
            new object[]
            {
                Language.GetString("AETHERIUM_SHIELDING_CORE_ACHIEVEMENT_NAME"),
                Language.GetString("AETHERIUM_SHIELDING_CORE_ACHIEVEMENT_DESC"),
            });

        public override Func<string> GetUnlocked { get; } = () => Language.GetStringFormatted("UNLOCKED_FORMAT",
            new object[]
            {
                Language.GetString("AETHERIUM_SHIELDING_CORE_ACHIEVEMENT_NAME"),
                Language.GetString("AETHERIUM_SHIELDING_CORE_ACHIEVEMENT_DESC"),
            });

        public static void RegisterLanguage()
        {
            LanguageAPI.Add("AETHERIUM_SHIELDING_CORE_ACHIEVEMENT_NAME", "Shield Maiden");
            LanguageAPI.Add("AETHERIUM_SHIELDING_CORE_ACHIEVEMENT_DESC", "Have over 50% of your health bar as shields without the use of a certain lunar item.");
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

        public class ShieldingCoreServerAchievementTracker : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                On.RoR2.CharacterBody.FixedUpdate += CheckShieldValues;
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
                On.RoR2.CharacterBody.FixedUpdate -= CheckShieldValues;
            }

            private void CheckShieldValues(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
            {
                orig(self);

                if (self && self.healthComponent && self.healthComponent.fullShield > 0 && base.IsCurrentBody(self))
                {
                    var shieldAsHealthPercentage = self.healthComponent.fullShield / (self.healthComponent.fullShield + self.healthComponent.fullHealth);
                    if(self.inventory && self.inventory.GetItemCount(ItemCatalog.FindItemIndex("ShieldOnly")) <= 0 && shieldAsHealthPercentage > 0.50f)
                    {
                        base.Grant();
                    }
                }
            }

        }
    }
}
