using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Achievements
{
    public class BlasterSwordAchievement : ModdedUnlockableAndAchievement<CustomSpriteProvider>
    {
        public override string AchievementIdentifier => "AETHERIUM_ACHIEVEMENT_BLASTER_SWORD_ID";

        public override string UnlockableIdentifier => "AETHERIUM_UNLOCKABLE_BLASTER_SWORD_REWARD_ID";

        public override string PrerequisiteUnlockableIdentifier => "AETHERIUM_PREREQUISITE_UNLOCKABLE_BLASTER_SWORD_REWARD_ID";

        public override string AchievementNameToken => "AETHERIUM_BLASTER_SWORD_ACHIEVEMENT_NAME";

        public override string AchievementDescToken => "AETHERIUM_BLASTER_SWORD_ACHIEVEMENT_DESC";

        public override string UnlockableNameToken => "AETHERIUM_BLASTER_SWORD_UNLOCKABLE_NAME";

        protected override CustomSpriteProvider SpriteProvider => throw new NotImplementedException();

        public static RoR2.GameObjectUnlockableFilter SwordInTheStone;

        public override void OnInstall()
        {
            if (AetheriumPlugin.ItemStatusDictionary[Items.BlasterSword.instance])
            {
                base.OnInstall();

                On.RoR2.SceneDirector.Start += PlaceSwordInTheStone;
            }
        }

        private void PlaceSwordInTheStone(On.RoR2.SceneDirector.orig_Start orig, RoR2.SceneDirector self)
        {
            if (self)
            {
                if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "dampcavessimple")
                {

                }
            }
        }
    }
}
