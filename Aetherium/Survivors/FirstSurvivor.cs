using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Survivors
{
    internal class FirstSurvivor : SurvivorBase
    {
        public override string SurvivorName => "Koalesk";

        public override string SurvivorBodyName => "KoaleskBody";

        public override string SurvivorSubtitle => "";

        public override string SurvivorLangToken => "KOALESK";

        public override string SurvivorDescription => "";

        public override string SurvivorEndingSuccessText => "...and so it left, somehow surviving everything.";

        public override string SurvivorEndingFailureText => "...and so it was left behind, somehow still alive.";

        public override float SurvivorBaseMaxHealth => 100;

        public override float SurvivorBaseArmor => 10;

        public override float SurvivorBaseMoveSpeed => 10;

        public override int SurvivorBaseJumpCount => 1;

        public override GameObject SurvivorBodyModelPrefab => MainAssets.LoadAsset<GameObject>("mdlKoalesk");

        public override GameObject SurvivorDisplayModelPrefab => MainAssets.LoadAsset<GameObject>("KoaleskDisplay");

        public override Texture SurvivorPortraitIcon => MainAssets.LoadAsset<Texture>("texCapsuleManIcon");

        public override Type SurvivorMainState => typeof(EntityStates.GenericCharacterMain);

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateBodyAndDisplay();
            CreateCharacterMaster();
            CreateEntityStateMachine();
            CreateSkills();
            CreateSkins();
            CreateSurvivor();
            Hooks();
        }

        public override void CreateItemDisplays()
        {

        }

        public override void CreateSkills()
        {
            var skillLocator = Utils.SurvivorHelpers.CreateBasicSkillFamilies(SurvivorBodyPrefab, SurvivorLangToken);
            if (skillLocator)
            {

            }
        }

        public override void Hooks()
        {
        }
    }
}
