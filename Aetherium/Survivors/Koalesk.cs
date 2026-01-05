using System;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Aetherium.States.Survivor.Koalesk.Primary;
using Aetherium.Survivors.Components;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Survivors
{
    public class Koalesk : SurvivorBase<Koalesk>
    {
        public override string SurvivorName => "Koalesk";
        public override string SurvivorBodyName => "KoaleskBody";
        public override string SurvivorLangToken => "KOALESK";
        public override string SurvivorSubtitle => "The Dissonant Fusion";
        public override string SurvivorDescription => "Koalesk struggles to maintain form between the Sanguine Plane and the Void.";

        public override string SurvivorEndingSuccessText => "...And so it left, merging the planes into one being.";

        public override string SurvivorEndingFailureText => "...and so it remained, torn asunder by the forces vying for control.";

        public override GameObject SurvivorBodyModelPrefab => MainAssets.LoadAsset<GameObject>("mdlKoalesk");
        public override GameObject SurvivorDisplayModelPrefab => MainAssets.LoadAsset<GameObject>("KoaleskDisplay");
        public override Texture SurvivorPortraitIcon => MainAssets.LoadAsset<Texture>("texKoaleskIcon");

        public override float SurvivorBaseMaxHealth => 110f;
        public override float SurvivorBaseArmor => 20f;
        public override float SurvivorBaseMoveSpeed => 7f;
        public override int SurvivorBaseJumpCount => 1;

        public static SkillDef KoaleskPrimaryRouter;
        public static BuffDef BloodliquorBuff;
        public static BuffDef DarkblightBuff;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateBuffs();
            CreateBodyAndDisplay();
            CreateCharacterMaster();
            CreateEntityStateMachine();
            CreateSkills();
            CreateSkins();
            CreateSurvivor();
            Hooks();
        }

        public void CreateBuffs()
        {
            BloodliquorBuff = ScriptableObject.CreateInstance<BuffDef>();
            BloodliquorBuff.name = "Aetherium: Bloodliquor Stack";
            BloodliquorBuff.buffColor = new Color(0.8f, 0.2f, 0.2f);
            BloodliquorBuff.canStack = true;
            BloodliquorBuff.isDebuff = false;
            BloodliquorBuff.iconSprite = MainAssets.LoadAsset<Sprite>("BloodliquorStackIcon.png");
            ContentAddition.AddBuffDef(BloodliquorBuff);

            DarkblightBuff = ScriptableObject.CreateInstance<BuffDef>();
            DarkblightBuff.name = "Aetherium: Darkblight Stack";
            DarkblightBuff.buffColor = new Color(0.4f, 0.2f, 0.8f);
            DarkblightBuff.canStack = true;
            DarkblightBuff.isDebuff = false;
            DarkblightBuff.iconSprite = MainAssets.LoadAsset<Sprite>("DarkblightStackIcon.png");
            ContentAddition.AddBuffDef(DarkblightBuff);
        }

        public override void CreateSkills()
        {

            SurvivorBodyPrefab.AddComponent<KoaleskPassive>();
            SkillLocator skillLocator = SurvivorBodyPrefab.GetComponent<SkillLocator>();

            skillLocator.primary = null;
            skillLocator.secondary = null;
            skillLocator.utility = null;
            skillLocator.special = null;

            GenericSkill primary = CreateGenericSkill(SurvivorBodyPrefab, "Primary");
            skillLocator.primary = primary;

            KoaleskPrimaryRouter = ScriptableObject.CreateInstance<SkillDef>();
            KoaleskPrimaryRouter.skillName = "KoaleskPrimary";
            KoaleskPrimaryRouter.skillNameToken = "AETHERIUM_PRIMARY_SKILL_NAME";
            KoaleskPrimaryRouter.skillDescriptionToken = "AETHERIUM_PRIMARY_SKILL_DESC";
            KoaleskPrimaryRouter.icon = MainAssets.LoadAsset<Sprite>("KoaleskAbility_RoseThorn.png");

            KoaleskPrimaryRouter.activationState = new EntityStates.SerializableEntityStateType(typeof(PrimaryRouter));
            KoaleskPrimaryRouter.activationStateMachineName = "Weapon";
            KoaleskPrimaryRouter.baseMaxStock = 1;
            KoaleskPrimaryRouter.baseRechargeInterval = 0;
            KoaleskPrimaryRouter.beginSkillCooldownOnSkillEnd = false;
            KoaleskPrimaryRouter.canceledFromSprinting = false;
            KoaleskPrimaryRouter.fullRestockOnAssign = true;
            KoaleskPrimaryRouter.interruptPriority = EntityStates.InterruptPriority.Any;
            KoaleskPrimaryRouter.isCombatSkill = true;
            KoaleskPrimaryRouter.mustKeyPress = false;
            KoaleskPrimaryRouter.rechargeStock = 1;
            KoaleskPrimaryRouter.requiredStock = 0;
            KoaleskPrimaryRouter.stockToConsume = 0;

            ContentAddition.AddSkillDef(KoaleskPrimaryRouter);
            primary.skillFamily.variants = new SkillFamily.Variant[] { new SkillFamily.Variant { skillDef = KoaleskPrimaryRouter } };
        }

        public override void Hooks() { }
        public override void CreateItemDisplays() { }
        public override Type SurvivorMainState => typeof(EntityStates.GenericCharacterMain);

        public static void AddBloodliquorStacks(CharacterBody body, int count)
        {
            if (body && BloodliquorBuff)
            {
                for (int i = 0; i < count; i++) body.AddBuff(BloodliquorBuff);
            }
        }

        public static void AddDarkblightStacks(CharacterBody body, int count)
        {
            if (body && DarkblightBuff)
            {
                for (int i = 0; i < count; i++) body.AddBuff(DarkblightBuff);
            }
        }
    }
}