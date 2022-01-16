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
    public class ShieldingCoreAchievement : AchievementBase
    {
        public override string AchievementName => "Shield Maiden";
        public override string AchievementLangToken => "SHIELDING_CORE";
        public override string AchievementDescription => "Have over half of your health as shields.";
        public override string AchievementPrerequisiteID => "";
        public override Sprite AchievementIcon => null;
        public override bool ServerTracked => true;

        public override void Init()
        {
            RegisterLang();
            CreateAchievement(ServerTracked, new ShieldingCoreServerAchievementTracker());
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
