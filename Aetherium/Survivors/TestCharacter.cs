using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Survivors
{
    internal class TestCharacter : SurvivorBase
    {
        public override string SurvivorName => "";

        public override string SurvivorBodyName => "";

        public override string SurvivorSubtitle => "";

        public override string SurvivorLangToken => "";

        public override string SurvivorDescription => "";

        public override string SurvivorEndingSuccessText => "";

        public override string SurvivorEndingFailureText => "";

        public override float SurvivorBaseMaxHealth => 100;

        public override float SurvivorBaseArmor => 10;

        public override float SurvivorBaseMoveSpeed => 10;

        public override int SurvivorBaseJumpCount => 1;

        public override GameObject SurvivorBodyModelPrefab => MainAssets.LoadAsset<GameObject>("");

        public override GameObject SurvivorDisplayModelPrefab => MainAssets.LoadAsset<GameObject>("");

        public override Texture SurvivorPortraitIcon => MainAssets.LoadAsset<Texture>("");

        public override Type SurvivorMainState => throw new NotImplementedException();

        public override void Init()
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

        }

        public override void Hooks()
        {
        }
    }
}
