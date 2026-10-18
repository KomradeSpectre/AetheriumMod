using Aetherium.Utils;
using Aetherium.Utils.Components;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using System;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.MathHelpers;

namespace Aetherium.EliteEquipment
{
    public class AffixHypercharged : EliteEquipmentBase<AffixHypercharged>
    {
        public static ConfigOption<float> DurationOfLightningStormBuff;
        public static ConfigOption<float> LightningStrikeCooldown;
        public static ConfigOption<int> AmountOfLightningStrikesPerBarrage;
        public static ConfigOption<float> LightningOnHitDamageCoefficient;
        public static ConfigOption<float> BonusShieldPercentage;

        public override string EliteEquipmentName => "Their Reminder";

        public override string EliteAffixToken => "AFFIX_HYPERCHARGED";

        public override string EliteEquipmentPickupDesc => "Become an aspect of the storm.";

        public override string EliteEquipmentFullDescription => $"Your attacks <style=cIsDamage>call down a lightning strike</style> for <style=cIsDamage>{FloatToPercentageString(LightningOnHitDamageCoefficient)}</style> TOTAL damage. " +
            $"Gain <style=cIsHealing>{FloatToPercentageString(BonusShieldPercentage)}</style> of your maximum health as <style=cIsHealing>shield</style>. " +
            $"On use, <style=cIsDamage>call forth a lightning storm</style> around you for <style=cIsUtility>{DurationOfLightningStormBuff}</style> seconds, dropping <style=cIsDamage>{AmountOfLightningStrikesPerBarrage}</style> strikes every <style=cIsUtility>{LightningStrikeCooldown}</style> second(s).";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Hypercharged";

        public override GameObject EliteEquipmentModel => DisplayModel;

        public override Sprite EliteEquipmentIcon => AresAssets.LoadAsset<Sprite>("TheirReminderIcon.png");

        public override Sprite EliteBuffIcon => AresAssets.LoadAsset<Sprite>("TheirReminderBuffIcon.png");

        public override EliteTier EliteTierDef => EliteTier.T2;

        // Kept darker than Overloading's blue so the two read as different elites.
        public override Color EliteColor => new Color32(36, 70, 160, 255);

        // The Ares LightningRamp1 is a 1px strip that's white at both ends, which rendered the whole elite white.
        public override Texture2D EliteRamp => CreateEliteRamp(EliteColor, "texRampAetheriumHypercharged");

        public override float Cooldown => 30f;

        public override EquipmentDef CrownSourceEquipment => RoR2Content.Equipment.AffixBlue;

        public override bool IsUnfinished => true;

        public static BuffDef LightningStormBuff;

        public static GameObject HyperchargedProjectile;

        public static GameObject DisplayModel;

        private static readonly Xoroshiro128Plus StormRandom = new Xoroshiro128Plus((ulong)DateTime.Now.Ticks);

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuff();
            CreateProjectile();
            CreateDisplayModel();
            CreateEquipment();
            CreateElite();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            DurationOfLightningStormBuff = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Duration of Lightning Storm Buff", 5f, "Duration of the Lightning Storm Buff upon activation of the affix item.");
            LightningStrikeCooldown = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Duration Between Strikes on Lightning Storm Buff", 1f, "Duration between the strikes while Lightning Storm Buff is active.");
            AmountOfLightningStrikesPerBarrage = config.ActiveBind<int>("Elite Equipment: " + EliteEquipmentName, "Amount of Lightning Strikes per Barrage", 16, "How many lightning strikes should be in each strike period of the Lightning Storm Buff?");
            LightningOnHitDamageCoefficient = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Lightning Strike on Hit Damage Coefficient", 0.5f, "What percentage of the hit's damage should the on-hit lightning strike deal? (0.5 = 50%)");
            BonusShieldPercentage = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Bonus Shield Percentage", 0.5f, "What percentage of maximum health should be granted as bonus shield? (0.5 = 50%)");
        }

        private void CreateBuff()
        {
            LightningStormBuff = ScriptableObject.CreateInstance<BuffDef>();
            LightningStormBuff.name = "Aetherium: Hypercharged Lightning Storm";
            LightningStormBuff.buffColor = EliteColor;
            LightningStormBuff.canStack = false;
            LightningStormBuff.iconSprite = AresAssets.LoadAsset<Sprite>("LightningStormBuffIcon.png");

            ContentAddition.AddBuffDef(LightningStormBuff);
        }

        private void CreateProjectile()
        {
            HyperchargedProjectile = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/LightningStake"), "HyperchargedProjectile", true);

            var controller = HyperchargedProjectile.GetComponent<ProjectileController>();
            controller.startSound = "Play_titanboss_shift_shoot";

            var impactExplosion = HyperchargedProjectile.GetComponent<ProjectileImpactExplosion>();
            impactExplosion.blastProcCoefficient = 0;
            impactExplosion.lifetime = 0.5f;
            impactExplosion.impactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/LightningStrikeImpact");
            impactExplosion.blastRadius = 7f;
            impactExplosion.bonusBlastForce = new Vector3(0, 750, 0);

            PrefabAPI.RegisterNetworkPrefab(HyperchargedProjectile);
            ContentAddition.AddProjectile(HyperchargedProjectile);
        }

        private void CreateDisplayModel()
        {
            // The Ares prefab was saved with the scene camera and light still attached, strip them.
            DisplayModel = PrefabAPI.InstantiateClone(AresAssets.LoadAsset<GameObject>("TheirReminder.prefab"), "AetheriumTheirReminderDisplay", false);
            foreach(var camera in DisplayModel.GetComponentsInChildren<Camera>(true))
            {
                UnityEngine.Object.DestroyImmediate(camera.gameObject);
            }
            foreach(var light in DisplayModel.GetComponentsInChildren<Light>(true))
            {
                UnityEngine.Object.DestroyImmediate(light.gameObject);
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            // Crowns come from CrownSourceEquipment's rules, the Their Reminder orb floats above through EliteFloatingDisplay.
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += CalculateStatsForAffix;
            On.RoR2.CharacterBody.FixedUpdate += ManageLightningStrikes;
            On.RoR2.GlobalEventManager.OnHitAll += SpawnLightning;
            On.RoR2.EquipmentSlot.Update += TeachAIToUseAffixItem;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += AddFloatingDisplay;
        }

        private void AddFloatingDisplay(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if(buffDef == EliteBuffDef)
            {
                EliteFloatingDisplay.Attach(self, EliteBuffDef, DisplayModel, 0.22f, 1.5f);
            }
        }

        private void TeachAIToUseAffixItem(On.RoR2.EquipmentSlot.orig_Update orig, EquipmentSlot self)
        {
            orig(self);
            if(!NetworkServer.active || self.equipmentIndex != EliteEquipmentDef.equipmentIndex || self.stock <= 0) { return; }

            var body = self.characterBody;
            if(!body || !body.master) { return; }

            var ai = body.master.GetComponent<BaseAI>();
            if(ai && ai.currentEnemy != null && ai.currentEnemy.gameObject)
            {
                self.ExecuteIfReady();
            }
        }

        private void CalculateStatsForAffix(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(sender.HasBuff(EliteBuffDef) && sender.healthComponent)
            {
                args.baseShieldAdd += sender.healthComponent.fullHealth * BonusShieldPercentage;
            }
        }

        private void SpawnLightning(On.RoR2.GlobalEventManager.orig_OnHitAll orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            orig(self, damageInfo, hitObject);

            if(!NetworkServer.active || !damageInfo.attacker || damageInfo.procCoefficient <= 0) { return; }

            var body = damageInfo.attacker.GetComponent<CharacterBody>();
            if(body && body.HasBuff(EliteBuffDef))
            {
                // A stationary stake detonates where the hit landed after a short telegraph.
                ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                {
                    projectilePrefab = HyperchargedProjectile,
                    position = damageInfo.position,
                    rotation = Util.QuaternionSafeLookRotation(Vector3.down),
                    owner = damageInfo.attacker,
                    damage = Util.OnHitProcDamage(damageInfo.damage, body.damage, LightningOnHitDamageCoefficient),
                    crit = damageInfo.crit,
                    damageColorIndex = DamageColorIndex.Item,
                    speedOverride = 0f
                });
            }
        }

        private void ManageLightningStrikes(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);

            if(!NetworkServer.active || !self.HasBuff(LightningStormBuff)) { return; }

            var lightningTracker = self.GetComponent<LightningTracker>();
            if(!lightningTracker) { lightningTracker = self.gameObject.AddComponent<LightningTracker>(); }

            lightningTracker.LightningCooldown -= Time.fixedDeltaTime;
            if(lightningTracker.LightningCooldown > 0) { return; }

            var strikes = Math.Max(1, (int)AmountOfLightningStrikesPerBarrage);
            var theta = Mathf.PI * 2 / strikes;
            for(int i = 1; i <= strikes; i++)
            {
                var radius = 20 + StormRandom.RangeFloat(-15, 15);
                var position = self.corePosition + new Vector3(radius * Mathf.Cos(theta * i), 15f, radius * Mathf.Sin(theta * i));

                ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                {
                    projectilePrefab = HyperchargedProjectile,
                    position = position,
                    rotation = Util.QuaternionSafeLookRotation(Vector3.down),
                    owner = self.gameObject,
                    damage = self.damage,
                    crit = self.RollCrit(),
                    damageColorIndex = DamageColorIndex.Default,
                    speedOverride = 150f
                });
            }
            lightningTracker.LightningCooldown = LightningStrikeCooldown;
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if(!slot.characterBody) { return false; }

            slot.characterBody.AddTimedBuff(LightningStormBuff, DurationOfLightningStormBuff);
            return true;
        }

        public class LightningTracker : MonoBehaviour
        {
            public float LightningCooldown;
        }
    }
}
