using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;
using static R2API.DamageAPI;

namespace Aetherium.Survivors
{
    internal class Koalesk : SurvivorBase<Koalesk>
    {
        public override string SurvivorName => "Koalesk";

        public override string SurvivorBodyName => "KoaleskBody";

        public override string SurvivorSubtitle => "";

        public override string SurvivorLangToken => "KOALESK";

        public override string SurvivorDescription => "";

        public override string SurvivorEndingSuccessText => "...and so it left, no longer split .";

        public override string SurvivorEndingFailureText => "..and so they remained, enshrouded in petal and gloom, their dual forces stilled by fate's unyielding hand.";

        public override float SurvivorBaseMaxHealth => 100;

        public override float SurvivorBaseArmor => 10;

        public override float SurvivorBaseMoveSpeed => 10;

        public override int SurvivorBaseJumpCount => 1;

        public override GameObject SurvivorBodyModelPrefab => MainAssets.LoadAsset<GameObject>("mdlKoalesk");

        public override GameObject SurvivorDisplayModelPrefab => MainAssets.LoadAsset<GameObject>("KoaleskDisplay");

        public override Texture SurvivorPortraitIcon => MainAssets.LoadAsset<Texture>("texCapsuleManIcon");

        public override Type SurvivorMainState => typeof(MyEntityStates.Survivors.Koalesk.KoaleskMainState);

        public static SkillDef KoaleskRoseThorn;
        public static SkillDef KoaleskDarkThorn;

        public static SkillDef KoaleskBloodyStake;

        public static BuffDef BloodliquorBuff;
        public static BuffDef DarkblightBuff;

        public static GameObject KoaleskDarkThornProjectile;
        public static ModdedDamageType KoaleskDarkThornDamage;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateBuffs();
            CreateProjectile();
            CreateBodyAndDisplay();
            CreateCharacterMaster();
            CreateEntityStateMachine();
            CreateAttackHitboxes();
            CreateSkills();
            CreateSkins();
            CreateSurvivor();
            Hooks();
        }

        private void CreateProjectile()
        {
            KoaleskDarkThornDamage = ReserveDamageType();

            KoaleskDarkThornProjectile = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/Fireball"), "KoaleskDarkThornProjectile", true);

            var model = GameObject.CreatePrimitive(PrimitiveType.Cube);
            model.AddComponent<NetworkIdentity>();
            model.AddComponent<ProjectileGhostController>();

            var controller = KoaleskDarkThornProjectile.GetComponent<ProjectileController>();
            controller.procCoefficient = 1f;
            controller.ghostPrefab = model;

            var projectileDamage = KoaleskDarkThornProjectile.GetComponent<ProjectileDamage>();
            projectileDamage.damageType = DamageType.Generic;

            var damageHolderComponent = KoaleskDarkThornProjectile.AddComponent<ModdedDamageTypeHolderComponent>();
            damageHolderComponent.Add(KoaleskDarkThornDamage);

            // register it for networking
            if (KoaleskDarkThornProjectile) PrefabAPI.RegisterNetworkPrefab(KoaleskDarkThornProjectile);

            // add it to the projectile catalog or it won't work in multiplayer
            ContentAddition.AddProjectile(KoaleskDarkThornProjectile);


        }

        public void CreateBuffs()
        {
            BloodliquorBuff = ScriptableObject.CreateInstance<BuffDef>();
            BloodliquorBuff.name = "Aetherium: Bloodliquor Stack";
            BloodliquorBuff.buffColor = Color.white;
            BloodliquorBuff.canStack = true;
            BloodliquorBuff.isDebuff = false;
            BloodliquorBuff.iconSprite = MainAssets.LoadAsset<Sprite>("BloodliquorStackIcon.png");
            ContentAddition.AddBuffDef(BloodliquorBuff);


            DarkblightBuff = ScriptableObject.CreateInstance<BuffDef>();
            DarkblightBuff.name = "Aetherium: Darkblight Stack";
            DarkblightBuff.buffColor = Color.white;
            DarkblightBuff.canStack = true;
            DarkblightBuff.isDebuff = false;
            DarkblightBuff.iconSprite = MainAssets.LoadAsset<Sprite>("DarkblightStackIcon.png");
            ContentAddition.AddBuffDef(DarkblightBuff);

        }

        public override void CreateItemDisplays()
        {

        }

        public void CreateAttackHitboxes()
        {
            //ModLogger.LogError($"Hitbox found {SurvivorBodyModelPrefab.transform.Find("Slash1Hitbox")}");
            Utils.SurvivorHelpers.SetupAttackHitbox(SurvivorBodyModelPrefab, SurvivorBodyModelPrefab.transform.Find("Slash1Hitbox"), "RoseThornHitbox");
            Utils.SurvivorHelpers.SetupAttackHitbox(SurvivorBodyModelPrefab, SurvivorBodyModelPrefab.transform.Find("Swipe1Hitbox"), "DarkThornHitbox");
        }

        public override void CreateSkills()
        {
            var skillLocator = Utils.SurvivorHelpers.CreateBasicSkillFamilies(SurvivorBodyPrefab, SurvivorLangToken);
            if (skillLocator)
            {
                #region Koalesk Primaries

                #region Rose Thorn
                LanguageAPI.Add("AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_ROSE_THORN_NAME", "Rose Thorn");
                LanguageAPI.Add("AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_ROSE_THORN_DESC", 
                    "Swing the [PLACEHOLDER SWORD NAME] forward, dealing [X] damage.\n" +
                    "If <color=#C65050>Bloodliquor</color> stacks are present, they will be consumed to enhance the move to a double slash.\n" +
                    "This move will generate <color=#9191E8>Darkblight</color> stacks.");

                KoaleskRoseThorn = ScriptableObject.CreateInstance<SkillDef>();
                KoaleskRoseThorn.skillName = "Rose Thorn";
                KoaleskRoseThorn.skillNameToken = "AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_ROSE_THORN_NAME";
                KoaleskRoseThorn.skillDescriptionToken = "AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_ROSE_THORN_DESC";
                KoaleskRoseThorn.icon = MainAssets.LoadAsset<Sprite>("BloodliquorStackIcon.png");

                KoaleskRoseThorn.activationState = new EntityStates.SerializableEntityStateType(typeof(MyEntityStates.Survivors.Koalesk.RoseThornState));
                KoaleskRoseThorn.activationStateMachineName = "Weapon";

                KoaleskRoseThorn.baseMaxStock = 1;
                KoaleskRoseThorn.baseRechargeInterval = 0;
                KoaleskRoseThorn.beginSkillCooldownOnSkillEnd = false;
                KoaleskRoseThorn.canceledFromSprinting = false;
                KoaleskRoseThorn.forceSprintDuringState = false;
                KoaleskRoseThorn.fullRestockOnAssign = true;
                KoaleskRoseThorn.interruptPriority = EntityStates.InterruptPriority.Any;
                KoaleskRoseThorn.resetCooldownTimerOnUse = false;
                KoaleskRoseThorn.isCombatSkill = true;
                KoaleskRoseThorn.mustKeyPress = false;
                KoaleskRoseThorn.cancelSprintingOnActivation = false;
                KoaleskRoseThorn.rechargeStock = 1;
                KoaleskRoseThorn.requiredStock = 0;
                KoaleskRoseThorn.stockToConsume = 0;

                R2API.ContentAddition.AddSkillDef(KoaleskRoseThorn);
                Utils.SurvivorHelpers.AddSkillToFamily(skillLocator.primary.skillFamily, KoaleskRoseThorn);
                #endregion
                #region Dark Thorn
                LanguageAPI.Add("AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_DARK_THORN_NAME", "Dark Thorn");
                LanguageAPI.Add("AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_DARK_THORN_DESC",
                    "Swipe with the [PLACEHOLDER CLAW NAME], dealing [X] damage and pulling enemies towards you.\n" +
                    "If <color=#9191E8>Bloodliquor</color> stacks are present, they will be consumed to enhance range of the swipe.\n" +
                    "This move will generate <color=#C65050>Bloodliquor</color> stacks.");

                KoaleskDarkThorn = ScriptableObject.CreateInstance<SkillDef>();
                KoaleskDarkThorn.skillName = "Dark Thorn";
                KoaleskDarkThorn.skillNameToken = "AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_DARK_THORN_NAME";
                KoaleskDarkThorn.skillDescriptionToken = "AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_DARK_THORN_DESC";
                KoaleskDarkThorn.icon = MainAssets.LoadAsset<Sprite>("DarkblightStackIcon.png");

                KoaleskDarkThorn.activationState = new EntityStates.SerializableEntityStateType(typeof(MyEntityStates.Survivors.Koalesk.DarkThornState));
                KoaleskDarkThorn.activationStateMachineName = "Weapon";

                KoaleskDarkThorn.baseMaxStock = 1;
                KoaleskDarkThorn.baseRechargeInterval = 0;
                KoaleskDarkThorn.beginSkillCooldownOnSkillEnd = false;
                KoaleskDarkThorn.canceledFromSprinting = false;
                KoaleskDarkThorn.forceSprintDuringState = false;
                KoaleskDarkThorn.fullRestockOnAssign = true;
                KoaleskDarkThorn.interruptPriority = EntityStates.InterruptPriority.Any;
                KoaleskDarkThorn.resetCooldownTimerOnUse = false;
                KoaleskDarkThorn.isCombatSkill = true;
                KoaleskDarkThorn.mustKeyPress = false;
                KoaleskDarkThorn.cancelSprintingOnActivation = false;
                KoaleskDarkThorn.rechargeStock = 1;
                KoaleskDarkThorn.requiredStock = 0;
                KoaleskDarkThorn.stockToConsume = 0;

                R2API.ContentAddition.AddSkillDef(KoaleskDarkThorn);
                Utils.SurvivorHelpers.AddSkillToFamily(skillLocator.primary.skillFamily, KoaleskDarkThorn);
                #endregion

                #endregion

                #region Koalesk Secondaries
                LanguageAPI.Add("AETHERIUM_SECONDARY_SKILL_" + SurvivorLangToken + "_BLOODY_STAKE_NAME", "Bloody Stake");
                LanguageAPI.Add("AETHERIUM_SECONDARY_SKILL_" + SurvivorLangToken + "_BLOODY_STAKE_DESC", "Launch an ethereal sword forwards. Having Bloodliquor stacks will generate more swords.");

                KoaleskBloodyStake = ScriptableObject.CreateInstance<SkillDef>();
                KoaleskBloodyStake.skillName = "Bloody Stake";
                KoaleskBloodyStake.skillNameToken = "AETHERIUM_SECONDARY_SKILL_" + SurvivorLangToken + "_BLOODY_STAKE_NAME";
                KoaleskBloodyStake.skillDescriptionToken = "AETHERIUM_SECONDARY_SKILL_" + SurvivorLangToken + "_BLOODY_STAKE_DESC";
                KoaleskBloodyStake.icon = null;

                KoaleskBloodyStake.activationState = new EntityStates.SerializableEntityStateType(typeof(MyEntityStates.Survivors.Koalesk.ChargeBloodyStake));
                KoaleskBloodyStake.activationStateMachineName = "Weapon";

                KoaleskBloodyStake.baseMaxStock = 1;
                KoaleskBloodyStake.baseRechargeInterval = 5;
                KoaleskBloodyStake.beginSkillCooldownOnSkillEnd = true;
                KoaleskBloodyStake.canceledFromSprinting = false;
                KoaleskBloodyStake.forceSprintDuringState = false;
                KoaleskBloodyStake.fullRestockOnAssign = true;
                KoaleskBloodyStake.interruptPriority = EntityStates.InterruptPriority.Skill;
                KoaleskBloodyStake.resetCooldownTimerOnUse = false;
                KoaleskBloodyStake.isCombatSkill = true;
                KoaleskBloodyStake.mustKeyPress = true;
                KoaleskBloodyStake.cancelSprintingOnActivation = false;
                KoaleskBloodyStake.rechargeStock = 1;
                KoaleskBloodyStake.requiredStock = 0;
                KoaleskBloodyStake.stockToConsume = 0;

                R2API.ContentAddition.AddSkillDef(KoaleskBloodyStake);
                Utils.SurvivorHelpers.AddSkillToFamily(skillLocator.secondary.skillFamily, KoaleskBloodyStake);
                #endregion

                #region Koalesk Utilities
                LanguageAPI.Add("AETHERIUM_UTILITY_SKILL_" + SurvivorLangToken + "_BIDENT_SLASH_NAME", "Bident Slash");
                LanguageAPI.Add("AETHERIUM_UTILITY_SKILL_" + SurvivorLangToken + "_BIDENT_SLASH_DESC", "Swing the bident to the right.");

                KoaleskRoseThorn = ScriptableObject.CreateInstance<SkillDef>();
                KoaleskRoseThorn.skillName = "Bident Slash";
                KoaleskRoseThorn.skillNameToken = "AETHERIUM_UTILITY_SKILL_" + SurvivorLangToken + "_BIDENT_SLASH_NAME";
                KoaleskRoseThorn.skillDescriptionToken = "AETHERIUM_UTILITY_SKILL_" + SurvivorLangToken + "_BIDENT_SLASH_DESC";
                KoaleskRoseThorn.icon = null;

                KoaleskRoseThorn.activationState = new EntityStates.SerializableEntityStateType(typeof(MyEntityStates.Survivors.Koalesk.RoseThornState));
                KoaleskRoseThorn.activationStateMachineName = "Weapon";

                KoaleskRoseThorn.baseMaxStock = 1;
                KoaleskRoseThorn.baseRechargeInterval = 0;
                KoaleskRoseThorn.beginSkillCooldownOnSkillEnd = false;
                KoaleskRoseThorn.canceledFromSprinting = false;
                KoaleskRoseThorn.forceSprintDuringState = false;
                KoaleskRoseThorn.fullRestockOnAssign = true;
                KoaleskRoseThorn.interruptPriority = EntityStates.InterruptPriority.Skill;
                KoaleskRoseThorn.resetCooldownTimerOnUse = false;
                KoaleskRoseThorn.isCombatSkill = true;
                KoaleskRoseThorn.mustKeyPress = false;
                KoaleskRoseThorn.cancelSprintingOnActivation = false;
                KoaleskRoseThorn.rechargeStock = 1;
                KoaleskRoseThorn.requiredStock = 0;
                KoaleskRoseThorn.stockToConsume = 0;

                R2API.ContentAddition.AddSkillDef(KoaleskRoseThorn);
                Utils.SurvivorHelpers.AddSkillToFamily(skillLocator.utility.skillFamily, KoaleskRoseThorn);
                #endregion

                #region Koalesk Specials
                LanguageAPI.Add("AETHERIUM_SPECIAL_SKILL_" + SurvivorLangToken + "_BIDENT_SLASH_NAME", "Bident Slash");
                LanguageAPI.Add("AETHERIUM_SPECIAL_SKILL_" + SurvivorLangToken + "_BIDENT_SLASH_DESC", "Swing the bident to the right.");

                KoaleskRoseThorn = ScriptableObject.CreateInstance<SkillDef>();
                KoaleskRoseThorn.skillName = "Bident Slash";
                KoaleskRoseThorn.skillNameToken = "AETHERIUM_SPECIAL_SKILL_" + SurvivorLangToken + "_BIDENT_SLASH_NAME";
                KoaleskRoseThorn.skillDescriptionToken = "AETHERIUM_SPECIAL_SKILL_" + SurvivorLangToken + "_BIDENT_SLASH_DESC";
                KoaleskRoseThorn.icon = null;

                KoaleskRoseThorn.activationState = new EntityStates.SerializableEntityStateType(typeof(MyEntityStates.Survivors.Koalesk.RoseThornState));
                KoaleskRoseThorn.activationStateMachineName = "Weapon";

                KoaleskRoseThorn.baseMaxStock = 1;
                KoaleskRoseThorn.baseRechargeInterval = 0;
                KoaleskRoseThorn.beginSkillCooldownOnSkillEnd = false;
                KoaleskRoseThorn.canceledFromSprinting = false;
                KoaleskRoseThorn.forceSprintDuringState = false;
                KoaleskRoseThorn.fullRestockOnAssign = true;
                KoaleskRoseThorn.interruptPriority = EntityStates.InterruptPriority.Skill;
                KoaleskRoseThorn.resetCooldownTimerOnUse = false;
                KoaleskRoseThorn.isCombatSkill = true;
                KoaleskRoseThorn.mustKeyPress = false;
                KoaleskRoseThorn.cancelSprintingOnActivation = false;
                KoaleskRoseThorn.rechargeStock = 1;
                KoaleskRoseThorn.requiredStock = 0;
                KoaleskRoseThorn.stockToConsume = 0;

                R2API.ContentAddition.AddSkillDef(KoaleskRoseThorn);
                Utils.SurvivorHelpers.AddSkillToFamily(skillLocator.special.skillFamily, KoaleskRoseThorn);
                #endregion
            }
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.RemoveBuff_BuffIndex += HealOnBloodLiquorStackDecay;
            On.RoR2.HealthComponent.TakeDamage += PullEnemiesTowardsKoalesk;
            //On.RoR2.CharacterBody.RemoveBuff_BuffDef += HealOnBloodLiquorStackDecay;
        }

        private void PullEnemiesTowardsKoalesk(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (self && self.body && self.body.rigidbody && damageInfo.HasModdedDamageType(KoaleskDarkThornDamage))
            {
                var victimRigidBody = self.body.rigidbody;
                var attacker = damageInfo.attacker;
                if (attacker)
                {
                    var attackerBody = attacker.GetComponent<CharacterBody>();
                    if (attackerBody)
                    {
                        var direction = (attackerBody.corePosition - self.body.corePosition).normalized;
                        victimRigidBody.AddForce(direction * 100f, ForceMode.Impulse);
                    }
                }

            }

            orig(self, damageInfo);
        }

        private void HealOnBloodLiquorStackDecay(On.RoR2.CharacterBody.orig_RemoveBuff_BuffIndex orig, CharacterBody self, BuffIndex buffIndex)
        {
            if (self)
            {
                if (buffIndex == BloodliquorBuff.buffIndex)
                {
                    self.healthComponent.Heal(self.maxHealth * 0.02f, default(ProcChainMask));
                }
            }

            orig(self, buffIndex);
        }
    }
}
