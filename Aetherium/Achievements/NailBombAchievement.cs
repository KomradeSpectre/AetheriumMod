using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Achievements;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;
using UnityEngine.Networking;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Aetherium.Achievements
{
    public class NailBombAchievement : ModdedUnlockable
    {
        public override string AchievementIdentifier => "AETHERIUM_ACHIEVEMENT_NAIL_BOMB_ID";

        public override string UnlockableIdentifier => "AETHERIUM_UNLOCKABLE_NAIL_BOMB_REWARD_ID";

        public override string PrerequisiteUnlockableIdentifier => "";

        public override string AchievementNameToken => "AETHERIUM_NAIL_BOMB_ACHIEVEMENT_NAME";

        public override string AchievementDescToken => "AETHERIUM_NAIL_BOMB_ACHIEVEMENT_DESC";

        public override string UnlockableNameToken => "AETHERIUM_NAIL_BOMB_UNLOCKABLE_NAME";

        public override Sprite Sprite => MainAssets.LoadAsset<Sprite>("ShieldingCoreAchievementIcon.png");

        public override Func<string> GetHowToUnlock { get; } = () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT",
            new object[]
            {
                Language.GetString("AETHERIUM_NAIL_BOMB_ACHIEVEMENT_NAME"),
                Language.GetString("AETHERIUM_NAIL_BOMB_ACHIEVEMENT_DESC"),
            });

        public override Func<string> GetUnlocked { get; } = () => Language.GetStringFormatted("UNLOCKED_FORMAT",
            new object[]
            {
                Language.GetString("AETHERIUM_NAIL_BOMB_ACHIEVEMENT_NAME"),
                Language.GetString("AETHERIUM_NAIL_BOMB_ACHIEVEMENT_DESC"),
            });

        public static void RegisterLanguage()
        {
            LanguageAPI.Add("AETHERIUM_NAIL_BOMB_ACHIEVEMENT_NAME", "Duck in the Woods");
            LanguageAPI.Add("AETHERIUM_NAIL_BOMB_ACHIEVEMENT_DESC", "Fail to open a rusted lockbox the proper way.");
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

        public class NailBombServerAchievementTracker : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                IL.RoR2.Interactor.PerformInteraction += TedKaczynski;
            }

            private void TedKaczynski(ILContext il)
            {
                int flagindex = 0;
                ILCursor c = new ILCursor(il);
                bool ILFound = c.TryGotoNext(
                    x => x.MatchCgtUn(),
                   x => x.MatchOr(),
                   x => x.MatchStloc(out flagindex)
                ) && c.TryGotoNext(
                   x => x.MatchLdloc(flagindex),
                   x => x.MatchBrfalse(out _)
                );
                if (ILFound)
                {
                    c.Index++;
                    c.EmitDelegate<Func<bool, bool>>((bool interactionSuccess) => {
                        if (!interactionSuccess)
                        {

                        }
                        return interactionSuccess;
                    });
                }
                else
                {
                    //Error Out
                }
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
                IL.RoR2.Interactor.PerformInteraction -= TedKaczynski;
            }
        }
    }
}
