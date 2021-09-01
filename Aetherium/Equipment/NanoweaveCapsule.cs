using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using static Aetherium.AetheriumPlugin;
using static Aetherium.CoreModules.StatHooks;

namespace Aetherium.Equipment
{
    public class NanoweaveCapsule : EquipmentBase<NanoweaveCapsule>
    {
        public ConfigOption<float> DurationOfNanoweaveActivation;
        public ConfigOption<float> PercentageDamageTakenToGetArmor;
        public ConfigOption<int> BaseArmorBonusAmount;

        public override string EquipmentName => "Nanoweave Capsule";

        public override string EquipmentLangTokenName => "NANOWEAVE_CAPSULE";

        public override string EquipmentPickupDesc => "On use, for a short duration gain armor and begin channeling damage taken into a blast.";

        public override string EquipmentFullDescription => "";

        public override string EquipmentLore => "";

        public override bool IsLunar => true;

        public override GameObject EquipmentModel => new GameObject();

        public override Sprite EquipmentIcon => MainAssets.LoadAsset<Sprite>("FeatheredPlume.png");

        public BuffDef NanoweaveActivationBuffDef;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuff();
            CreateEquipment();
            Hooks();
        }

        public void CreateConfig(ConfigFile config)
        {
            DurationOfNanoweaveActivation = config.ActiveBind<float>("Equipment: " + EquipmentName, "Duration of Nanoweave Activation", 10f, "How long should the Nanoweave Activation last before detonating?");
            PercentageDamageTakenToGetArmor = config.ActiveBind<float>("Equipment: " + EquipmentName, "Percent Damage Taken to Get Armor Bonus", 0.05f, "What percentage of our health needs to be lost to gain a stack of armor boost during the effect?");
            BaseArmorBonusAmount = config.ActiveBind<int>("Equipment: " + EquipmentName, "Base Armor Bonus Granted During Effect", 20, "How much armor should be granted per stack of armor bonus during the effect?");
        }

        private void CreateBuff()
        {
            NanoweaveActivationBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            NanoweaveActivationBuffDef.name = "Aetherium: Nanoweave Activation";
            NanoweaveActivationBuffDef.buffColor = Color.white;
            NanoweaveActivationBuffDef.canStack = false;
            NanoweaveActivationBuffDef.isDebuff = false;
            NanoweaveActivationBuffDef.iconSprite = MainAssets.LoadAsset<Sprite>("WitchesRingBuffIcon.png");

            BuffAPI.Add(new CustomBuff(NanoweaveActivationBuffDef));
        }


        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += ManageNanoweaveActivation;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += ManageNanoweaveComponent;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += FireNanoweaveBlast;
            GetStatCoefficients += ManageArmorDuringNanoweaveActivation;
        }

        private void ManageNanoweaveComponent(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            if(self && buffDef == NanoweaveActivationBuffDef)
            {
                var nanoweaveDamageComponent = self.GetComponent<NanoweaveDamageComponent>();
                if (!nanoweaveDamageComponent) { nanoweaveDamageComponent = self.gameObject.AddComponent<NanoweaveDamageComponent>(); }
            }

            orig(self, buffDef);
        }

        private void FireNanoweaveBlast(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            if(self && buffDef == NanoweaveActivationBuffDef)
            {
                var nanoweaveComponent = self.GetComponent<NanoweaveDamageComponent>();
                if (nanoweaveComponent && !nanoweaveComponent.Fired)
                {
                    nanoweaveComponent.Fired = true;
                }
            }

            orig(self, buffDef);
        }

        private void ManageNanoweaveActivation(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var body = self.body;
            if (body && body.HasBuff(NanoweaveActivationBuffDef))
            {
                var nanoweaveDamageComponent = body.GetComponent<NanoweaveDamageComponent>();

                if(nanoweaveDamageComponent && !damageInfo.rejected || damageInfo.damage > 0)
                {
                    if(damageInfo.damage >= self.combinedHealthFraction * PercentageDamageTakenToGetArmor)
                    {
                        nanoweaveDamageComponent.ArmorBonusStacks++;
                    }
                    nanoweaveDamageComponent.DamageTaken += damageInfo.damage;
                    nanoweaveDamageComponent.HitsTaken++;
                }
            }

            orig(self, damageInfo);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.master) { return false; }

            if (!slot.characterBody.HasBuff(NanoweaveActivationBuffDef))
            {
                slot.characterBody.AddTimedBuff(NanoweaveActivationBuffDef, DurationOfNanoweaveActivation);
                return true;
            }

            return false;
        }

        private void ManageArmorDuringNanoweaveActivation(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(NanoweaveActivationBuffDef))
            {
                var nanoweaveComponent = sender.GetComponent<NanoweaveDamageComponent>();
                if (nanoweaveComponent)
                {
                    args.armorAdd += Mathf.Clamp(nanoweaveComponent.DamageTaken, 0, 500);
                }
            }
        }

        public class NanoweaveDamageComponent : MonoBehaviour
        {
            public float ArmorBonusStacks;
            public float DamageTaken;
            public int HitsTaken;
            public bool Fired;

            public float AbsorbAndExplodeTime = 1;

            public Light ExplosionLight;

            public void Start()
            {
                ExplosionLight = gameObject.AddComponent<Light>();
                ExplosionLight.color = new Color(0.6f, 0.196f, 0.8f);
                ExplosionLight.range = 1;
                ExplosionLight.intensity = 1;
                ExplosionLight.transform.parent = this.gameObject.transform;
            }

            public void FixedUpdate()
            {
                if (Fired)
                {
                    ExplosionLight.range = EasingFunction.EaseInQuad(0, HitsTaken *2, AbsorbAndExplodeTime);
                    ExplosionLight.intensity = EasingFunction.EaseInQuad(0, HitsTaken * 2, AbsorbAndExplodeTime);

                    AbsorbAndExplodeTime -= Time.fixedDeltaTime;
                    if(AbsorbAndExplodeTime <= 0)
                    {
                        if (EntityStates.NullifierMonster.DeathState.deathExplosionEffect)
                        {
                            EffectManager.SpawnEffect(EntityStates.NullifierMonster.DeathState.deathExplosionEffect, new EffectData
                            {
                                origin = gameObject.transform.position,
                                scale = DamageTaken / 10,
                                rotation = gameObject.transform.rotation
                            }, true);
                        }

                        new BlastAttack()
                        {
                            attacker = gameObject,
                            radius = DamageTaken / 10,
                            baseDamage = DamageTaken,
                            damageType = DamageType.BypassArmor | DamageType.AOE,
                            position = gameObject.transform.position,
                            inflictor = gameObject,
                            baseForce = HitsTaken

                        }.Fire();

                        UnityEngine.Object.Destroy(ExplosionLight);
                        UnityEngine.Object.Destroy(this);
                    }
                }
                else
                {
                    ExplosionLight.range = HitsTaken * 2;
                    ExplosionLight.intensity = HitsTaken * 2;
                }
            }
        }
    }
}
