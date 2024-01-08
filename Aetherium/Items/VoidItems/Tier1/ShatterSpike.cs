using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;
using Aetherium.Utils;
using Aetherium.Utils.Components;
using static Aetherium.Utils.ItemHelpers;
using static Aetherium.Utils.MiscHelpers;
using static Aetherium.Utils.MathHelpers;
using UnityEngine.Networking;
using RoR2.Projectile;
using static R2API.DamageAPI;

namespace Aetherium.Items.VoidItems.Tier1
{
    internal class ShatterSpike : ItemBase<ShatterSpike>
    {
        public ConfigOption<float> PercentDamageThresholdRequiredToActivate;
        public ConfigOption<int> AmountOfSpikesPerDetonation;
        public ConfigOption<float> PercentDamagePerSpike;
        public ConfigOption<float> PercentDamageBonusPerAdditionalStacks;
        public ConfigOption<float> PlayerCooldownDuration;
        public ConfigOption<float> ArmorReductionMultPerStack;
        public ConfigOption<float> ArmorReductionDuration;

        public override string ItemName => "Shatter Spike";

        public override string ItemLangTokenName => "SHATTER_SPIKE";

        public override string ItemPickupDesc => "Strong hits on a target have a chance to burst a damaging crystal shrapnel from them.";

        public override string ItemFullDescription => $"Attacks that deal {FloatToPercentageString(PercentDamageThresholdRequiredToActivate)} damage or more release a weakening shrapnel nova from the target, dealing {AmountOfSpikesPerDetonation}x{FloatToPercentageString(PercentDamagePerSpike)} of your damage <style=cStack>(+{FloatToPercentageString(PercentDamageBonusPerAdditionalStacks)} more per stack)</style>. Enemies hit that launched spikes are granted {PlayerCooldownDuration} second(s) of immunity to the effect. Additionally, any enemy hit by the spikes have their armor reduced for {ArmorReductionDuration} second(s).";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.VoidTier1;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("NailBomb.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("NailBombIconTier1.png");

        public override string CorruptsItem => "ITEM_NAIL_BOMB_NAME";

        public GameObject ShatterSpikeProjectile;

        public BuffDef ShatterSpikeCooldownDebuff;
        public BuffDef ShatterSpikeArmorReductionDebuff;

        public ModdedDamageType ShatterSpikeArmorDamageType;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateModdedDamage();
            CreateBuff();
            CreateProjectile();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            PercentDamageThresholdRequiredToActivate = config.ActiveBind<float>("Item: " + ItemName, "Percent Damage Threshold Required to Activate Effect", 1.2f, "What percentage of damage should we deal in a single hit to activate the effect of this item?");
            AmountOfSpikesPerDetonation = config.ActiveBind<int>("Item: " + ItemName, "Amount of Spikes per Explosion", 10, "How many spikes should get released upon explosion of the projectile?");
            PercentDamagePerSpike = config.ActiveBind<float>("Item: " + ItemName, "Percent Damage per Spike", 0.3f, "What percentage of damage should each spike in the shatter spike deal?");
            PercentDamageBonusPerAdditionalStacks = config.ActiveBind<float>("Item: " + ItemName, "Percent Damage Bonus of Additional Stacks", 0.5f, "What additional percentage of the body's damage should be given per additional stacks of Shatter Spike?");
            PlayerCooldownDuration = config.ActiveBind<float>("Item: " + ItemName, "Cooldown for Shatter Spike Absurdity Limiter", 2, "What should be the duration we should have to cooldown between each activation of Shatter Spike's effect? (Set to 0 at your own risk.)");
        }

        public void CreateModdedDamage()
        {
            ShatterSpikeArmorDamageType = ReserveDamageType();
        }

        private void CreateBuff()
        {
            ShatterSpikeCooldownDebuff = ScriptableObject.CreateInstance<BuffDef>();
            ShatterSpikeCooldownDebuff.name = "Aetherium: Shatter Spike Cooldown Debuff";
            ShatterSpikeCooldownDebuff.buffColor = new Color(255, 255, 255);
            ShatterSpikeCooldownDebuff.canStack = false;
            ShatterSpikeCooldownDebuff.isDebuff = false;
            ShatterSpikeCooldownDebuff.iconSprite = MainAssets.LoadAsset<Sprite>("NailBombNailCooldownIcon.png");

            ContentAddition.AddBuffDef(ShatterSpikeCooldownDebuff);

            ShatterSpikeArmorReductionDebuff = ScriptableObject.CreateInstance<BuffDef>();
            ShatterSpikeArmorReductionDebuff.name = "Aetherium: Shatter Spike Armor Debuff";
            ShatterSpikeArmorReductionDebuff.buffColor = new Color(255, 255, 255);
            ShatterSpikeArmorReductionDebuff.canStack = false;
            ShatterSpikeArmorReductionDebuff.isDebuff = true;
            ShatterSpikeArmorReductionDebuff.iconSprite = MainAssets.LoadAsset<Sprite>("NailBombNailCooldownIcon.png");

            ContentAddition.AddBuffDef(ShatterSpikeArmorReductionDebuff);
        }

        public void CreateProjectile()
        {
            ShatterSpikeProjectile = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/MageIceBombProjectile"), "ShatterSpikeProjectile");

            var model = MainAssets.LoadAsset<GameObject>("ShatterSpikeCrystalProjectile.prefab");
            model.AddComponent<NetworkIdentity>();
            model.AddComponent<ProjectileGhostController>();

            var akEvents = ShatterSpikeProjectile.GetComponents<AkEvent>();
            foreach(AkEvent akEvent in akEvents)
            {
                UnityEngine.Object.Destroy(akEvent);
            }



            var controller = ShatterSpikeProjectile.GetComponent<RoR2.Projectile.ProjectileController>();
            controller.procCoefficient = 0.5f;
            controller.ghostPrefab = model;

            var scaleCurve = model.AddComponent<ObjectScaleCurve>();
            scaleCurve.useOverallCurveOnly = true;
            scaleCurve.overallCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.05f, 1));

            var projectileSimple = ShatterSpikeProjectile.GetComponent<ProjectileSimple>();
            projectileSimple.lifetime = 5;
            projectileSimple.updateAfterFiring = false;

            var rigidBody = ShatterSpikeProjectile.GetComponent<Rigidbody>();
            rigidBody.useGravity = true;
            rigidBody.freezeRotation = false;
            rigidBody.constraints = RigidbodyConstraints.None;

            var torqueOnStart = ShatterSpikeProjectile.AddComponent<ApplyTorqueOnStart>();
            torqueOnStart.localTorque = new Vector3(5000, 5000, 5000);
            torqueOnStart.randomize = true;

            var sphereCollider = ShatterSpikeProjectile.GetComponent<SphereCollider>();

            var projectileDamage = ShatterSpikeProjectile.GetComponent<ProjectileDamage>();
            projectileDamage.damageType = DamageType.Generic;

            var projectileModdedDamage = ShatterSpikeProjectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            projectileModdedDamage.Add(ShatterSpikeArmorDamageType);

            UnityEngine.Object.Destroy(ShatterSpikeProjectile.GetComponent<ProjectileSingleTargetImpact>());

            var projectileSingleTargetImpact = ShatterSpikeProjectile.AddComponent<ProjectileWorldTargetImpact>();
            projectileSingleTargetImpact.impactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXFMJ");
            projectileSingleTargetImpact.projectileCollider = sphereCollider;

            var projectileOverlapAttack = ShatterSpikeProjectile.GetComponent<ProjectileOverlapAttack>();
            projectileOverlapAttack.impactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXFMJ");

            /*var destroyOnWorld = ShatterSpikeProjectile.AddComponent<ProjectileDestroyOnWorld>();
            destroyOnWorld.impactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXFMJ");*/

            PrefabAPI.RegisterNetworkPrefab(ShatterSpikeProjectile);
            R2API.ContentAddition.AddProjectile(ShatterSpikeProjectile);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += FireShatterSpike;
            On.RoR2.HealthComponent.TakeDamage += AddArmorReductionStacks;
            R2API.RecalculateStatsAPI.GetStatCoefficients += ArmorReductionDebuff;
        }

        private void AddArmorReductionStacks(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.rejected)
            {
                orig(self, damageInfo);
                return;
            }

            if (self && damageInfo.HasModdedDamageType(ShatterSpikeArmorDamageType))
            {
                var body = self.body;
                if (body)
                {
                    body.AddTimedBuff(ShatterSpikeArmorReductionDebuff, 5);
                }
            }

            orig(self, damageInfo);
        }

        private void ArmorReductionDebuff(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(ShatterSpikeArmorReductionDebuff)) 
            {
                args.armorAdd += -50;
            }
        }

        private void FireShatterSpike(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.rejected || damageInfo.procCoefficient <= 0)
            {
                orig(self, damageInfo, victim);
                return;
            }

            if (victim)
            {
                var victimBody = victim.GetComponent<CharacterBody>();
                if (victimBody)
                {
                    var attacker = damageInfo.attacker;
                    if (attacker)
                    {
                        var body = attacker.GetComponent<CharacterBody>();
                        if (body)
                        {
                            if (body.HasBuff(ShatterSpikeCooldownDebuff))
                            {
                                orig(self, damageInfo, victim);
                                return;
                            }

                            var InventoryCount = GetCount(body);
                            if (InventoryCount > 0)
                            {
                                if (damageInfo.damage / body.damage >= PercentDamageThresholdRequiredToActivate)
                                {
                                    for (int i = 0; i < AmountOfSpikesPerDetonation; i++)
                                    {
                                        //var positionChosen = Aetherium.Utils.MathHelpers.GetRandomPointOnSphere(0.1f, Run.instance.stageRng, victimBody.corePosition);
                                        var positionChosen = Aetherium.Utils.MathHelpers.GetRandomPointOnTube(victimBody.corePosition, 0.1f, 0.2f, Quaternion.identity, Run.instance.stageRng);

                                        FireProjectileInfo newProjectileLaunch = new FireProjectileInfo()
                                        {
                                            projectilePrefab = ShatterSpikeProjectile,
                                            owner = body.gameObject,
                                            damage = (body.damage * PercentDamagePerSpike) + (body.damage * (PercentDamageBonusPerAdditionalStacks * (InventoryCount - 1))),
                                            position = positionChosen,
                                            rotation = Util.QuaternionSafeLookRotation(positionChosen - victimBody.corePosition),
                                            speedOverride = 25f,
                                            damageTypeOverride = null,
                                            damageColorIndex = DamageColorIndex.Default,
                                            procChainMask = default,
                                        };

                                        ProjectileManager.instance.FireProjectile(newProjectileLaunch);
                                    }

                                    body.AddTimedBuff(ShatterSpikeCooldownDebuff, PlayerCooldownDuration);
                                }
                            }
                        }
                    }
                }

            }
            orig(self, damageInfo, victim);
        }
    }
}
