using Aetherium.Utils;
using Aetherium.Utils.Components;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.MathHelpers;

namespace Aetherium.EliteEquipment
{
    public class AffixSplintering : EliteEquipmentBase<AffixSplintering>
    {
        public static ConfigOption<int> CrystalsPerRetaliation;
        public static ConfigOption<float> CrystalDamageCoefficient;
        public static ConfigOption<float> RetaliationCooldown;
        public static ConfigOption<float> CrystalSpeed;
        public static ConfigOption<float> CrystalSpread;
        public static ConfigOption<int> CrystalBurstCount;

        public override string EliteEquipmentName => "His Vengeance";

        public override string EliteAffixToken => "AFFIX_SPLINTERING";

        public override string EliteEquipmentPickupDesc => "Become an aspect of retribution.";

        public override string EliteEquipmentFullDescription => $"When hit, <style=cIsDamage>shoot {CrystalsPerRetaliation} crystals</style> back towards your attacker for <style=cIsDamage>{FloatToPercentageString(CrystalDamageCoefficient)}</style> base damage each. Can only happen once every <style=cIsUtility>{RetaliationCooldown}</style> second(s). " +
            $"On use, <style=cIsDamage>burst {CrystalBurstCount} crystals</style> out in every direction.";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Splintering";

        public override GameObject EliteEquipmentModel => PickupModel;

        public override Sprite EliteEquipmentIcon => AresAssets.LoadAsset<Sprite>("HisVengeanceIcon.png");

        public override Sprite EliteBuffIcon => AresAssets.LoadAsset<Sprite>("HisVengeanceBuffIcon.png");

        public override EliteTier EliteTierDef => EliteTier.T1;

        public override Color EliteColor => new Color32(196, 92, 255, 255);

        // The Ares PearlescentPurpleRamp is a 1px strip that's white at both ends, which rendered the whole elite white.
        public override Texture2D EliteRamp => CreateEliteRamp(EliteColor, "texRampAetheriumSplintering");

        public override float Cooldown => 20f;

        public override EquipmentDef CrownSourceEquipment => RoR2Content.Equipment.AffixWhite;

        public override bool IsUnfinished => true;

        public static GameObject PickupModel;

        public static GameObject CrystalProjectile;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateModels();
            CreateProjectile();
            CreateEquipment();
            CreateElite();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            CrystalsPerRetaliation = config.ActiveBind<int>("Elite Equipment: " + EliteEquipmentName, "Crystals per Retaliation", 3, "How many crystals are fired back at an attacker?");
            CrystalDamageCoefficient = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Crystal Damage Coefficient", 0.6f, "What percentage of the elite's base damage does each crystal deal? (0.6 = 60%)");
            RetaliationCooldown = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Retaliation Cooldown", 1f, "How long (in seconds) before the elite can retaliate again?");
            CrystalSpeed = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Crystal Speed", 60f, "How fast (in meters per second) do the crystals fly?");
            CrystalSpread = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Crystal Spread", 8f, "Maximum spread (in degrees) of retaliation crystals.");
            CrystalBurstCount = config.ActiveBind<int>("Elite Equipment: " + EliteEquipmentName, "Crystal Burst Count", 12, "How many crystals are fired in a ring on use?");
        }

        private void CreateModels()
        {
            // The cluster is about 4.5m across, shrink it down to pickup size.
            PickupModel = PrefabAPI.InstantiateClone(AresAssets.LoadAsset<GameObject>("HisVengeanceCluster.prefab"), "AetheriumHisVengeancePickup", false);
            PickupModel.transform.localScale *= 0.4f;
        }

        private void CreateProjectile()
        {
            CrystalProjectile = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/FMJ"), "AetheriumSplinteringCrystal", true);

            var ghost = PrefabAPI.InstantiateClone(AresAssets.LoadAsset<GameObject>("HisVengeanceProjectile.prefab"), "AetheriumSplinteringCrystalGhost", false);
            ghost.AddComponent<ProjectileGhostController>();

            var controller = CrystalProjectile.GetComponent<ProjectileController>();
            controller.procCoefficient = 0.5f;
            controller.SetGhostPrefab(ghost);

            var damage = CrystalProjectile.GetComponent<ProjectileDamage>();
            damage.damageType = DamageType.Generic;
            damage.damage = 0;

            var intervalController = CrystalProjectile.GetComponent<ProjectileIntervalOverlapAttack>();
            if(intervalController) { UnityEngine.Object.Destroy(intervalController); }

            var projectileSimple = CrystalProjectile.GetComponent<ProjectileSimple>();
            projectileSimple.lifetime = 1.5f;

            PrefabAPI.RegisterNetworkPrefab(CrystalProjectile);
            ContentAddition.AddProjectile(CrystalProjectile);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            // Crowns come from CrownSourceEquipment's rules, the His Vengeance cluster floats above through EliteFloatingDisplay.
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += RetaliateWithCrystals;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += AddFloatingDisplay;
        }

        private void AddFloatingDisplay(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if(buffDef == EliteBuffDef)
            {
                // The cluster is about 4.5m across, so a small multiplier still reads as a crystal cluster.
                EliteFloatingDisplay.Attach(self, EliteBuffDef, AresAssets.LoadAsset<GameObject>("HisVengeanceCluster.prefab"), 0.12f, 1.5f);
            }
        }

        private void RetaliateWithCrystals(DamageReport damageReport)
        {
            var victimBody = damageReport.victimBody;
            var attackerBody = damageReport.attackerBody;
            if(!victimBody || !attackerBody || victimBody == attackerBody) { return; }
            if(damageReport.damageInfo.procCoefficient <= 0 || !victimBody.HasBuff(EliteBuffDef)) { return; }
            if(!damageReport.victim || !damageReport.victim.alive) { return; }

            var tracker = victimBody.GetComponent<RetaliationTracker>();
            if(!tracker) { tracker = victimBody.gameObject.AddComponent<RetaliationTracker>(); }
            if(Time.fixedTime < tracker.NextRetaliationTime) { return; }
            tracker.NextRetaliationTime = Time.fixedTime + RetaliationCooldown;

            var direction = (attackerBody.corePosition - victimBody.corePosition).normalized;
            if(direction == Vector3.zero) { direction = victimBody.transform.forward; }

            for(int i = 0; i < CrystalsPerRetaliation; i++)
            {
                FireCrystal(victimBody, Util.ApplySpread(direction, 0f, CrystalSpread, 1f, 1f));
            }
        }

        private static void FireCrystal(CharacterBody body, Vector3 direction)
        {
            ProjectileManager.instance.FireProjectile(new FireProjectileInfo
            {
                projectilePrefab = CrystalProjectile,
                position = body.corePosition + direction * body.radius,
                rotation = Util.QuaternionSafeLookRotation(direction),
                owner = body.gameObject,
                damage = body.damage * CrystalDamageCoefficient,
                crit = body.RollCrit(),
                damageColorIndex = DamageColorIndex.Default,
                speedOverride = CrystalSpeed
            });
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            var body = slot.characterBody;
            if(!body || !NetworkServer.active) { return false; }

            var count = Mathf.Max(1, CrystalBurstCount);
            for(int i = 0; i < count; i++)
            {
                FireCrystal(body, Quaternion.Euler(0f, 360f / count * i, 0f) * Vector3.forward);
            }
            return true;
        }

        public class RetaliationTracker : MonoBehaviour
        {
            public float NextRetaliationTime;
        }
    }
}
