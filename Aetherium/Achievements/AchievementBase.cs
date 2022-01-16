using Aetherium.Equipment;
using Aetherium.Items;
using R2API;
using RoR2;
using RoR2.Achievements;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Achievements
{
    public abstract class AchievementBase : ModdedUnlockable, IModdedUnlockableDataProvider
    {
        public abstract string AchievementName { get; }
        public abstract string AchievementLangToken { get; }
        public abstract string AchievementDescription { get; }
        public abstract string AchievementPrerequisiteID { get; }
        public abstract Sprite AchievementIcon { get; }

        public UnlockableDef UnlockableDef;

        public virtual bool ServerTracked { get; set; } = false;

        public override string AchievementIdentifier => $"AETHERIUM_{AchievementLangToken}_ACHIEVEMENT_ID";
        public override string UnlockableIdentifier => $"AETHERIUM_{AchievementLangToken}_UNLOCKABLE_ID";
        public override string PrerequisiteUnlockableIdentifier => AchievementPrerequisiteID;
        public override string AchievementNameToken => $"AETHERIUM_{AchievementLangToken}_ACHIEVEMENT_NAME";
        public override string AchievementDescToken => $"AETHERIUM_{AchievementLangToken}_ACHIEVEMENT_DESC";
        public override string UnlockableNameToken => $"AETHERIUM_{AchievementLangToken}_UNLOCKABLE_NAME";

        public abstract void Init();

        public override Func<string> GetHowToUnlock => () =>
        
            $"{AchievementName}\n" +
            $"<style=cStack>{AchievementDescription}</style>";

        public override Func<string> GetUnlocked => () =>

            $"{AchievementName}\n" +
            $"<style=cStack>{AchievementDescription}</style>";

        public override Sprite Sprite => AchievementIcon;

        public void RegisterLang()
        {
            LanguageAPI.Add($"AETHERIUM_{AchievementLangToken}_ACHIEVEMENT_NAME", AchievementName);
            LanguageAPI.Add($"AETHERIUM_{AchievementLangToken}_ACHIEVEMENT_DESC", AchievementDescription);
            LanguageAPI.Add($"AETHERIUM_{AchievementLangToken}_UNLOCKABLE_NAME", AchievementName);
        }

        public override void OnInstall()
        {
            base.OnInstall();
            base.SetServerTracked(ServerTracked);
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
        }

        public void CreateAchievement(bool serverTracked, BaseServerAchievement serverTrackedAchievement = null)
        {
            UnlockableDef = serverTracked ? UnlockableAPI.AddUnlockable(this.GetType(), serverTrackedAchievement.GetType()) : UnlockableAPI.AddUnlockable(this.GetType());
        }
    }
}
