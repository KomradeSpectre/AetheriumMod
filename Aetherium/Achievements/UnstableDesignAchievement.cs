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
    public class UnstableDesignAchievement : AchievementBase
    {
        public override string AchievementName => "First Step into Madness";

        public override string AchievementLangToken => "UNSTABLE_DESIGN";

        public override string AchievementDescription => "Kill or die to a Perfected Lunar Chimera";

        public override string AchievementPrerequisiteID => "";

        public override Sprite AchievementIcon => MainAssets.LoadAsset<Sprite>("UnstableDesignIcon.png");

        public override void Init()
        {
            RegisterLang();
            CreateAchievement(ServerTracked, new UnstableDesignServerAchievementTracker());
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
