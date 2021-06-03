using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using AK.Wwise;

using static Aetherium.AetheriumPlugin;
using static Aetherium.CoreModules.ItemHelperModule;
using static Aetherium.Compatability.ModCompatability.BetterAPICompat;
using UnityEngine.Networking;

namespace Aetherium.Equipment
{
    public class SoulExtractionCircle : EquipmentBase<SoulExtractionCircle>
    {
        public override string EquipmentName => "Soul Extraction Circle";

        public override string EquipmentLangTokenName => "SOUL_EXTRACTION_CIRCLE";

        public override string EquipmentPickupDesc => "On use, fire a projectile that marks an elite enemy. If that elite enemy dies to you, this equipment transforms into its Aspect.";

        public override string EquipmentFullDescription => "On use, you can mark an elite enemy. If that elite enemy dies to you, this equipment transforms into its Aspect.";

        public override string EquipmentLore => "";

        public override GameObject EquipmentModel => MainAssets.LoadAsset<GameObject>("SoulExtractionCircle.prefab");

        public override Sprite EquipmentIcon => MainAssets.LoadAsset<Sprite>("FeatheredPlumeIcon.png");

        public static GameObject SoulConversionProjectile;

        public static BuffDef SoulConversionDebuff;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateBuff();

            if (IsBetterUIInstalled)
            {

            }

            CreateProjectile();

            CreateEquipment();
            Hooks();
        }

        private void CreateBuff()
        {
            SoulConversionDebuff = ScriptableObject.CreateInstance<BuffDef>();
            SoulConversionDebuff.name = "Aetherium: Soul Conversion Debuff";
            SoulConversionDebuff.buffColor = Color.white;
            SoulConversionDebuff.canStack = false;
            SoulConversionDebuff.isDebuff = true;
            SoulConversionDebuff.iconSprite = MainAssets.LoadAsset<Sprite>("AccursedPotionSipCooldownDebuffIcon.png");

            BuffAPI.Add(new CustomBuff(SoulConversionDebuff));
        }

        private void CreateProjectile()
        {
            SoulConversionProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/Fireball"), "SoulConversionProjectile", true);

            var projectileController = SoulConversionProjectile.GetComponent<ProjectileController>();
            var ghost = projectileController.ghostPrefab;

            var fireballGhostClone = PrefabAPI.InstantiateClone(ghost, "SoulConversionProjectileGhost", true);
            fireballGhostClone.AddComponent<NetworkIdentity>();

            var fireballGhostParticlesColor = fireballGhostClone.GetComponentInChildren<ParticleSystem>().colorOverLifetime;
            fireballGhostParticlesColor.color = new ParticleSystem.MinMaxGradient(new Color(45, 255, 45, 255), new Color(10, 128, 10, 255));

            var fireballGhostLight = fireballGhostClone.GetComponentInChildren<Light>();
            fireballGhostLight.color = new Color(0.18f, 1, 0.18f);

            projectileController.ghostPrefab = fireballGhostClone;

            var projectileDamage = SoulConversionProjectile.GetComponent<ProjectileDamage>();
            projectileDamage.damageType = DamageType.Generic;

            var projectileInflictDebuff = SoulConversionProjectile.AddComponent<ProjectileInflictTimedBuff>();
            projectileInflictDebuff.buffDef = SoulConversionDebuff;
            projectileInflictDebuff.duration = 60;

            PrefabAPI.RegisterNetworkPrefab(SoulConversionProjectile);
            ProjectileAPI.Add(SoulConversionProjectile);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnBuffFirstStackGained += RemoveBuffFromNonElites;
            On.RoR2.GlobalEventManager.OnCharacterDeath += MorphEquipmentIntoAffix;
        }

        private void RemoveBuffFromNonElites(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            if (self.HasBuff(SoulConversionDebuff) && !self.isElite)
            {
                self.RemoveBuff(SoulConversionDebuff);
            }
        }

        private void MorphEquipmentIntoAffix(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            if(damageReport.attackerMaster && damageReport.victimBody)
            {
                if(damageReport.attackerMaster.inventory.currentEquipmentIndex == EquipmentDef.equipmentIndex && damageReport.victimBody.HasBuff(SoulConversionDebuff))
                {
                    var victimEquipmentDef = EquipmentCatalog.GetEquipmentDef(damageReport.victimBody.inventory.GetEquipmentIndex());

                    if (victimEquipmentDef && EliteEquipmentDefs.Any(x => x == victimEquipmentDef))
                    {
                        damageReport.attackerMaster.inventory.GiveEquipmentString(victimEquipmentDef.name);
                    }
                }
            }

            orig(self, damageReport);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if(!slot.characterBody || !slot.characterBody.inputBank) { return false; }

            var aimray = slot.characterBody.inputBank.GetAimRay();

            FireProjectileInfo SoulConversionInfo = new FireProjectileInfo()
            {
                owner = slot.characterBody.gameObject,
                position = aimray.origin,
                rotation = aimray.direction == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(aimray.direction),
                damage = slot.characterBody.damage,
                projectilePrefab = SoulConversionProjectile,
                speedOverride = 100,
                procChainMask = default(ProcChainMask),                
            };

            ProjectileManager.instance.FireProjectile(SoulConversionInfo);

            return true;
        }
    }
}
