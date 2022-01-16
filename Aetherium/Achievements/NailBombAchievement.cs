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
    public class NailBombAchievement : AchievementBase
    {
        public override string AchievementName => "Duck in the Woods";

        public override string AchievementLangToken => "NAIL_BOMB";

        public override string AchievementDescription => "Fail to open a Rusted Lockbox.";

        public override string AchievementPrerequisiteID => "";

        public override Sprite AchievementIcon => MainAssets.LoadAsset<Sprite>("FeatheredPlumeIcon.png");
        public override bool ServerTracked => true;

        public override void Init()
        {
            RegisterLang();
            CreateAchievement(ServerTracked, new NailBombServerAchievementTracker());
        }

        public class NailBombServerAchievementTracker : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                IL.RoR2.Interactor.PerformInteraction += TedKaczynski;
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
                IL.RoR2.Interactor.PerformInteraction -= TedKaczynski;
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
        }
    }
}
