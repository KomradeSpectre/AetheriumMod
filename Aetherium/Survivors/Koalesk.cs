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
using Aetherium.States.Survivor.Koalesk;
using Aetherium.States.Survivor.Koalesk.Primary;
using Aetherium.States.Survivor.Koalesk.Secondary;
using static Aetherium.AetheriumPlugin;
using static R2API.DamageAPI;
using Aetherium.Utils.Components;
using System.Linq;
using Aetherium.Utils;
using Aetherium.States.Survivor.Koalesk.Primary.RoseThorn;
using Aetherium.States.Survivor.Koalesk.Utility.ShadowDance;

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

        public override Type SurvivorMainState => typeof(KoaleskMainState);

        public static SkillDef KoaleskRoseThorn;
        public static SkillDef KoaleskDarkThorn;

        public static SkillDef KoaleskBloodyStake;

        public static SkillDef KoaleskShadowDance;

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

            KoaleskDarkThornProjectile = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/GravSphere"), "KoaleskDarkThornProjectile");

            foreach(Transform childTransform in KoaleskDarkThornProjectile.transform)
            {
                UnityEngine.Object.Destroy(childTransform.gameObject);
            }

            var model = MainAssets.LoadAsset<GameObject>("KoaleskDarkThornProjectile.prefab");
            model.AddComponent<NetworkIdentity>();
            var ghostController = model.AddComponent<ProjectileGhostController>();
            ghostController.inheritScaleFromProjectile = true;


            var akEvents = KoaleskDarkThornProjectile.GetComponents<AkEvent>();
            foreach (AkEvent akEvent in akEvents)
            {
                UnityEngine.Object.Destroy(akEvent);
            }

            var controller = KoaleskDarkThornProjectile.GetComponent<RoR2.Projectile.ProjectileController>();
            controller.procCoefficient = 0.5f;
            controller.ghostPrefab = model;

            var objectScaleCurve = KoaleskDarkThornProjectile.AddComponent<ObjectScaleCurve>();
            objectScaleCurve.useOverallCurveOnly = true;
            objectScaleCurve.overallCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.2f, 2.5f));

            var projectileDotZone = KoaleskDarkThornProjectile.AddComponent<ProjectileDotZone>();

            var radialForce = KoaleskDarkThornProjectile.GetComponent<RadialForce>();
            radialForce.radius = 2.5f;
            radialForce.damping = 0.85f;
            radialForce.forceMagnitude = -2500f;
            radialForce.forceCoefficientAtEdge = 0.25f;

            var boomerangController = KoaleskDarkThornProjectile.AddComponent<BoomerangProjectile>();
            boomerangController.canHitWorld = false;
            boomerangController.canHitCharacters = true;
            boomerangController.travelSpeed = 60;
            boomerangController.charge = 0.5f;
            boomerangController.transitionDuration = 0.5f;

            var sphereCollider = KoaleskDarkThornProjectile.GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;

            var darkThornProjectileScalar = KoaleskDarkThornProjectile.AddComponent<KoaleskDarkThornProjectileController>();
            darkThornProjectileScalar.Boomerang = boomerangController;
            darkThornProjectileScalar.RadialForce = radialForce;
            darkThornProjectileScalar.Collider = sphereCollider;

            var projectileDamage = KoaleskDarkThornProjectile.GetComponent<ProjectileDamage>();
            projectileDamage.damageType = DamageType.Generic;

            var damageHolderComponent = KoaleskDarkThornProjectile.AddComponent<ModdedDamageTypeHolderComponent>();
            damageHolderComponent.Add(KoaleskDarkThornDamage);

            var projectileSingleTargetImpact = KoaleskDarkThornProjectile.AddComponent<ProjectileSingleTargetImpact>();
            projectileSingleTargetImpact.impactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniImpactExecute");

            /*var projectileOverlapAttack = KoaleskDarkThornProjectile.GetComponent<ProjectileOverlapAttack>();
            projectileOverlapAttack.impactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniImpactExecute");*/

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
            Utils.SurvivorHelpers.SetupAttackHitbox(SurvivorBodyModelPrefab, SurvivorChildLocator.FindChild("RoseThornHitbox"), "RoseThornHitbox");
            Utils.SurvivorHelpers.SetupAttackHitbox(SurvivorBodyModelPrefab, SurvivorChildLocator.FindChild("DarkThornHitbox"), "DarkThornHitbox");
            Utils.SurvivorHelpers.SetupAttackHitbox(SurvivorBodyModelPrefab, SurvivorChildLocator.FindChild("DoubleSlashHitbox"), "DoubleSlashHitbox");
        }

        public override void CreateSkills()
        {
            SurvivorBodyPrefab.AddComponent<KoaleskBuffManager>();
            var skillLocator = Utils.SurvivorHelpers.CreateBasicSkillFamilies(SurvivorBodyPrefab, SurvivorLangToken);
            if (skillLocator)
            {
                #region Koalesk Primaries

                #region Rose Thorn
                Language.Language.Add("AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_ROSE_THORN_NAME", "Rose Thorn");
                Language.Language.Add("AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_ROSE_THORN_DESC", 
                    "Swing the [PLACEHOLDER SWORD NAME] forward, dealing [X] damage.\n" +
                    "If <color=#C65050>Bloodliquor</color> stacks are present, they will be consumed to enhance the move to a double slash.\n" +
                    "This move will generate <color=#9191E8>Darkblight</color> stacks.");

                KoaleskRoseThorn = ScriptableObject.CreateInstance<SteppedSkillDef>();
                KoaleskRoseThorn.skillName = "Rose Thorn";
                KoaleskRoseThorn.skillNameToken = "AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_ROSE_THORN_NAME";
                KoaleskRoseThorn.skillDescriptionToken = "AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_ROSE_THORN_DESC";
                KoaleskRoseThorn.icon = MainAssets.LoadAsset<Sprite>("KoaleskAbility_RoseThorn.png");

                KoaleskRoseThorn.activationState = new EntityStates.SerializableEntityStateType(typeof(RoseThornState));
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
                (KoaleskRoseThorn as SteppedSkillDef).stepCount = 3;

                R2API.ContentAddition.AddSkillDef(KoaleskRoseThorn);
                Utils.SurvivorHelpers.AddSkillToFamily(skillLocator.primary.skillFamily, KoaleskRoseThorn);
                #endregion
                #region Dark Thorn
                Language.Language.Add("AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_DARK_THORN_NAME", "Dark Thorn");
                Language.Language.Add("AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_DARK_THORN_DESC",
                    "Swipe with the [PLACEHOLDER CLAW NAME], dealing [X] damage and pulling enemies towards you.\n" +
                    "If <color=#9191E8>Bloodliquor</color> stacks are present, they will be consumed to enhance range of the swipe.\n" +
                    "This move will generate <color=#C65050>Bloodliquor</color> stacks.");

                KoaleskDarkThorn = ScriptableObject.CreateInstance<SkillDef>();
                KoaleskDarkThorn.skillName = "Dark Thorn";
                KoaleskDarkThorn.skillNameToken = "AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_DARK_THORN_NAME";
                KoaleskDarkThorn.skillDescriptionToken = "AETHERIUM_PRIMARY_SKILL_" + SurvivorLangToken + "_DARK_THORN_DESC";
                KoaleskDarkThorn.icon = MainAssets.LoadAsset<Sprite>("KoaleskAbility_DarkThorn.png");

                KoaleskDarkThorn.activationState = new EntityStates.SerializableEntityStateType(typeof(DarkThornState));
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
                Language.Language.Add("AETHERIUM_SECONDARY_SKILL_" + SurvivorLangToken + "_BLOODY_STAKE_NAME", "Bloody Stake");
                Language.Language.Add("AETHERIUM_SECONDARY_SKILL_" + SurvivorLangToken + "_BLOODY_STAKE_DESC", "Launch an ethereal sword forwards. Having Bloodliquor stacks will generate more swords.");

                KoaleskBloodyStake = ScriptableObject.CreateInstance<SkillDef>();
                KoaleskBloodyStake.skillName = "Bloody Stake";
                KoaleskBloodyStake.skillNameToken = "AETHERIUM_SECONDARY_SKILL_" + SurvivorLangToken + "_BLOODY_STAKE_NAME";
                KoaleskBloodyStake.skillDescriptionToken = "AETHERIUM_SECONDARY_SKILL_" + SurvivorLangToken + "_BLOODY_STAKE_DESC";
                KoaleskBloodyStake.icon = null;

                KoaleskBloodyStake.activationState = new EntityStates.SerializableEntityStateType(typeof(ChargeBloodyStake));
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
                Language.Language.Add("AETHERIUM_UTILITY_SKILL_" + SurvivorLangToken + "_SHADOW_DANCE_NAME", "Shadow Dance");
                Language.Language.Add("AETHERIUM_UTILITY_SKILL_" + SurvivorLangToken + "_SHADOW_DANCE_DESC", "Fire a claw towards an enemy or terrain, and pivot around it freely.");

                KoaleskShadowDance = ScriptableObject.CreateInstance<SkillDef>();
                KoaleskShadowDance.skillName = "Shadow Dance";
                KoaleskShadowDance.skillNameToken = "AETHERIUM_UTILITY_SKILL_" + SurvivorLangToken + "_SHADOW_DANCE_NAME";
                KoaleskShadowDance.skillDescriptionToken = "AETHERIUM_UTILITY_SKILL_" + SurvivorLangToken + "_SHADOW_DANCE_DESC";
                KoaleskShadowDance.icon = MainAssets.LoadAsset<Sprite>("KoaleskAbility_ShadowDance.png");

                KoaleskShadowDance.activationState = new EntityStates.SerializableEntityStateType(typeof(FireShadowDance));
                KoaleskShadowDance.activationStateMachineName = "Weapon";

                KoaleskShadowDance.baseMaxStock = 1;
                KoaleskShadowDance.baseRechargeInterval = 0;
                KoaleskShadowDance.beginSkillCooldownOnSkillEnd = false;
                KoaleskShadowDance.canceledFromSprinting = true;
                KoaleskShadowDance.forceSprintDuringState = false;
                KoaleskShadowDance.fullRestockOnAssign = true;
                KoaleskShadowDance.interruptPriority = EntityStates.InterruptPriority.Skill;
                KoaleskShadowDance.resetCooldownTimerOnUse = false;
                KoaleskShadowDance.isCombatSkill = true;
                KoaleskShadowDance.mustKeyPress = true;
                KoaleskShadowDance.cancelSprintingOnActivation = false;
                KoaleskShadowDance.rechargeStock = 1;
                KoaleskShadowDance.requiredStock = 0;
                KoaleskShadowDance.stockToConsume = 0;

                R2API.ContentAddition.AddSkillDef(KoaleskShadowDance);
                Utils.SurvivorHelpers.AddSkillToFamily(skillLocator.utility.skillFamily, KoaleskShadowDance);
                #endregion

                #region Koalesk Specials
                Language.Language.Add("AETHERIUM_SPECIAL_SKILL_" + SurvivorLangToken + "_BIDENT_SLASH_NAME", "Bident Slash");
                Language.Language.Add("AETHERIUM_SPECIAL_SKILL_" + SurvivorLangToken + "_BIDENT_SLASH_DESC", "Swing the bident to the right.");

                KoaleskRoseThorn = ScriptableObject.CreateInstance<SkillDef>();
                KoaleskRoseThorn.skillName = "Bident Slash";
                KoaleskRoseThorn.skillNameToken = "AETHERIUM_SPECIAL_SKILL_" + SurvivorLangToken + "_BIDENT_SLASH_NAME";
                KoaleskRoseThorn.skillDescriptionToken = "AETHERIUM_SPECIAL_SKILL_" + SurvivorLangToken + "_BIDENT_SLASH_DESC";
                KoaleskRoseThorn.icon = null;

                KoaleskRoseThorn.activationState = new EntityStates.SerializableEntityStateType(typeof(RoseThornState));
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
            On.RoR2.CharacterBody.SetBuffCount += HealOnBloodLiquorStackDecay;
            //On.RoR2.HealthComponent.TakeDamage += PullEnemiesTowardsKoalesk;
            //On.RoR2.CharacterBody.RemoveBuff_BuffDef += HealOnBloodLiquorStackDecay;
        }

        private void HealOnBloodLiquorStackDecay(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            var oldCount = 0;
            bool isBloodLiquor = false;
            if (self)
            {                
                if(buffType == BloodliquorBuff.buffIndex)
                {
                    oldCount = self.GetBuffCount(BloodliquorBuff);
                    isBloodLiquor = true;
                }
            }

            orig(self, buffType, newCount);

            if(self && isBloodLiquor && oldCount > newCount && oldCount > 0)
            {
                var difference = oldCount - newCount;
                self.healthComponent.Heal((self.maxHealth * 0.02f) * difference, default(ProcChainMask));
            }
        }

        private void PullEnemiesTowardsKoalesk(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (self && damageInfo.HasModdedDamageType(KoaleskDarkThornDamage))
            {
                var body = self.body;
                if (body)
                {
                    var attacker = damageInfo.attacker;
                    if (attacker)
                    {
                        var attackerBody = attacker.GetComponent<CharacterBody>();
                        if (attackerBody)
                        {
                            Utils.MiscHelpers.PullEnemiesTowardsBody(attackerBody, body, 50f);
                            AddBloodliquorStacks(attackerBody, 1);
                        }
                    }
                }
            }

            orig(self, damageInfo);
        }

        public static void AddBloodliquorStacks(CharacterBody body, int amountOfStacks, float taperBaseDuration = 4, float taperStart = 2)
        {
            if (!body || !BloodliquorBuff || amountOfStacks <= 0) { return; }

            if (body)
            {
                /*if (body.timedBuffs.Any(x => x.buffIndex == BloodliquorBuff.buffIndex))
                {
                    ItemHelpers.RefreshTimedBuffs(body, BloodliquorBuff, taperBaseDuration, taperStart);
                }
                body.AddTimedBuff(BloodliquorBuff, taperBaseDuration);*/

                body.AddBuff(BloodliquorBuff);
            }
        }

        public static void AddDarkblightStacks(CharacterBody body, int amountOfStacks, float taperBaseDuration = 4, float taperStart = 2)
        {
            if (!body || !DarkblightBuff || amountOfStacks <= 0) { return; }

            if (body)
            {
                /*
                if (body.timedBuffs.Any(x => x.buffIndex == DarkblightBuff.buffIndex))
                {
                    ItemHelpers.RefreshTimedBuffs(body, DarkblightBuff, taperBaseDuration, taperStart);
                }
                body.AddTimedBuff(DarkblightBuff, taperBaseDuration);*/
                body.AddBuff(DarkblightBuff);
            }
        }

        public class KoaleskDarkThornProjectileController : MonoBehaviour
        {
            public RadialForce RadialForce;
            public BoomerangProjectile Boomerang;
            public SphereCollider Collider;

            public void FixedUpdate()
            {
                if(RadialForce && Boomerang && Collider)
                {
                    if(Boomerang.NetworkboomerangState != BoomerangProjectile.BoomerangState.FlyBack)
                    {
                        RadialForce.radius = 0;
                        Collider.radius = 0;
                    }
                    else
                    {
                        RadialForce.radius = gameObject.transform.localScale.x;
                        Collider.radius = gameObject.transform.localScale.x;
                    }
                }
            }
        }

        public class KoaleskBuffManager : MonoBehaviour
        {
            public CharacterBody KoaleskBody;

            public float Timer;
            public float OutOfCombatIntervalBeforeConsumption;

            public void Start()
            {
                KoaleskBody = gameObject.GetComponent<CharacterBody>();
                OutOfCombatIntervalBeforeConsumption = 5;
            }

            public void FixedUpdate()
            {
                if(KoaleskBody && BloodliquorBuff && DarkblightBuff)
                {
                    if (KoaleskBody.outOfCombat)
                    {
                        Timer += Time.fixedDeltaTime;
                    }

                    if(Timer >= OutOfCombatIntervalBeforeConsumption)
                    {
                        var bloodLiquorCount = KoaleskBody.GetBuffCount(BloodliquorBuff);
                        var darkBlightCount = KoaleskBody.GetBuffCount(DarkblightBuff);
                        if(bloodLiquorCount > 0)
                        {
                            KoaleskBody.SetBuffCount(BloodliquorBuff.buffIndex, bloodLiquorCount - 1);
                        }

                        if (darkBlightCount > 0)
                        {
                            KoaleskBody.SetBuffCount(DarkblightBuff.buffIndex, darkBlightCount - 1);
                        }
                        Timer = 0;
                    }
                }
            }
        }
    }
}
