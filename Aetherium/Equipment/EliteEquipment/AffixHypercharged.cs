using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Equipment.EliteEquipment
{
    public class AffixHypercharged : EliteEquipmentBase<AffixHypercharged>
    {

        public ConfigOption<float> DurationOfLightningStormBuff;
        public ConfigOption<float> LightningStrikeCooldown;
        public ConfigOption<int> AmountOfLightningStrikesPerBarrage;
        public ConfigOption<float> CostMultiplierOfElite;

        public override string EliteEquipmentName => "Their Reminder";

        public override string EliteAffixToken => "AFFIX_HYPERCHARGED";

        public override string EliteEquipmentPickupDesc => "Become an aspect of the storm.";

        public override string EliteEquipmentFullDescription => "";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Hypercharged {0}";

        public override GameObject EliteEquipmentModel => new GameObject();

        public override Sprite EliteEquipmentIcon => null;

        public override Sprite EliteBuffIcon => null;

        public override Material EliteMaterial => MainAssets.LoadAsset<Material>("Distortion.mat");

        public override CombatDirector.EliteTierDef[] CanAppearInEliteTiers { get; set; }

        public BuffDef LightningStormBuff;

        public static GameObject HyperchargedProjectile;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuff();
            CreateProjectile();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            Hooks();
        }

        public void CreateConfig(ConfigFile config)
        {
            DurationOfLightningStormBuff = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Duration of Lightning Storm Buff", 5f, "Duration of the Lightning Storm Buff upon activation of the affix item.");
            LightningStrikeCooldown = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Duration Between Strikes on Lightning Storm Buff", 1f, "Duration between the strikes while Lightning Storm Buff is active.");
            AmountOfLightningStrikesPerBarrage = config.ActiveBind<int>("Elite Equipment: " + EliteEquipmentName, "Amount of Lightning Strikes per Barrage", 16, "How many lightning strikes should be in each strike period of the Lightning Storm Buff?");
            CostMultiplierOfElite = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Cost Multiplier of Elite", 2.5f, "How many times higher than the base elite cost should the cost of this elite be? (Do not set this to 0, only warning haha.)");
        }

        public void CreateBuff()
        {
            LightningStormBuff = ScriptableObject.CreateInstance<BuffDef>();
            LightningStormBuff.name = "Hypercharged Lightning Storm";
            LightningStormBuff.buffColor = new Color32(255, 255, 255, byte.MaxValue);
            LightningStormBuff.canStack = false;

            ContentAddition.AddBuffDef(LightningStormBuff);
        }

        public void CreateProjectile()
        {
            HyperchargedProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/LightningStake"), "HyperchargedProjectile", true);

            var controller = HyperchargedProjectile.GetComponent<ProjectileController>();
            controller.startSound = "Play_titanboss_shift_shoot";

            var impactExplosion = HyperchargedProjectile.GetComponent<ProjectileImpactExplosion>();
            impactExplosion.blastProcCoefficient = 0;
            impactExplosion.lifetime = 0.5f;
            impactExplosion.impactEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/LightningStrikeImpact");
            impactExplosion.blastRadius = 7f;
            impactExplosion.bonusBlastForce = new Vector3(0, 750, 0);

            // register it for networking
            if (HyperchargedProjectile) PrefabAPI.RegisterNetworkPrefab(HyperchargedProjectile);

            // add it to the projectile catalog or it won't work in multiplayer 
            ContentAddition.AddProjectile(HyperchargedProjectile);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        private void CreateEliteTiers()
        {
            CanAppearInEliteTiers = new CombatDirector.EliteTierDef[]
            {
                new CombatDirector.EliteTierDef()
                {
                    costMultiplier = CombatDirector.baseEliteCostMultiplier * CostMultiplierOfElite,
                    eliteTypes = Array.Empty<EliteDef>(),
                    isAvailable = SetAvailability
                }
            };
        }

        private bool SetAvailability(SpawnCard.EliteRules arg)
        {
            return Run.instance.loopClearCount > 0 && arg == SpawnCard.EliteRules.Default;
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += CalculateStatsForAffix;
            On.RoR2.CharacterBody.FixedUpdate += ManageLightningStrikes;
            On.RoR2.GlobalEventManager.OnHitAll += SpawnLightning;
            On.RoR2.EquipmentSlot.FixedUpdate += TeachAIToUseAffixItem;
        }

        private void TeachAIToUseAffixItem(On.RoR2.EquipmentSlot.orig_FixedUpdate orig, EquipmentSlot self)
        {
            orig(self);
            if(self.equipmentIndex == EliteEquipmentDef.equipmentIndex && self.cooldownTimer <= 0)
            {
                var body = self.characterBody;
                if (body)
                {
                    var master = self.characterBody.master;
                    if (master && master.GetComponent<BaseAI>())
                    {
                        self.PerformEquipmentAction(EliteEquipmentDef);
                    }
                }
            }         
        }

        private void CalculateStatsForAffix(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(EliteBuffDef))
            {
                args.baseShieldAdd += sender.healthComponent.fullHealth * 0.5f;
            }
        }

        private void SpawnLightning(On.RoR2.GlobalEventManager.orig_OnHitAll orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            if (damageInfo.attacker && damageInfo.procCoefficient > 0)
            {
                var body = damageInfo.attacker.GetComponent<CharacterBody>();
                if (body)
                {
                    if (body.HasBuff(EliteBuffDef))
                    {

                        float damageCoefficient2 = 0.5f;
                        float damage = Util.OnHitProcDamage(damageInfo.damage, body.damage, damageCoefficient2);
                        float force = 0f;
                        Vector3 position = damageInfo.position;
                        ProjectileManager.instance.FireProjectile(HyperchargedProjectile, position, Quaternion.identity, damageInfo.attacker, damage, force, damageInfo.crit, DamageColorIndex.Item);
                    }
                }
            }
            orig(self, damageInfo, hitObject);
        }

        private void ManageLightningStrikes(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (self.HasBuff(LightningStormBuff))
            {
                var lightningTracker = self.GetComponent<LightningTracker>();
                if (!lightningTracker) { lightningTracker = self.gameObject.AddComponent<LightningTracker>(); }

                if (lightningTracker.LightningCooldown > 0)
                {
                    lightningTracker.LightningCooldown -= Time.fixedDeltaTime;
                }
                if (lightningTracker.LightningCooldown <= 0)
                {
                    for (int i = 1; i <= AmountOfLightningStrikesPerBarrage; i++)
                    {
                        var newProjectileInfo = new FireProjectileInfo
                        {
                            owner = self.gameObject,
                            projectilePrefab = HyperchargedProjectile,
                            speedOverride = 150.0f,
                            damage = self.damage,
                            damageTypeOverride = null,
                            damageColorIndex = DamageColorIndex.Default,
                            procChainMask = default
                        };
                        var theta = (Math.PI * 2) / AmountOfLightningStrikesPerBarrage;
                        var angle = theta * i;
                        var radius = 20 + Run.instance.runRNG.RangeFloat(-15, 15);
                        var positionChosen = new Vector3((float)(radius * Math.Cos(angle) + self.corePosition.x), self.corePosition.y + 1, (float)(radius * Math.Sin(angle) + self.corePosition.z));
                        var raycastedChosen = MiscUtils.RaycastToDirection(positionChosen, 1000f, Vector3.down);
                        if (raycastedChosen != null)
                        {
                            positionChosen = raycastedChosen.Value + new Vector3(0, 0.5f, 0);
                        }
                        newProjectileInfo.position = positionChosen;
                        newProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(positionChosen + Vector3.down);
                        ProjectileManager.instance.FireProjectile(newProjectileInfo);
                    }
                    lightningTracker.LightningCooldown = LightningStrikeCooldown;
                }
            }
            orig(self);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody) { return false; }
            var body = slot.characterBody;

            if (NetworkServer.active)
            {
                body.AddTimedBuff(LightningStormBuff, DurationOfLightningStormBuff);
            }
            return true;
        }

        public class LightningTracker : MonoBehaviour
        {
            public float LightningCooldown;
        }
    }
}
