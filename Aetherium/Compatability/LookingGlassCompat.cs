using Aetherium.Equipment;
using Aetherium.Items;
using Aetherium.Items.Tier1;
using Aetherium.Items.Tier2;
using Aetherium.Items.Tier3;
using Aetherium.Items.TierLunar;
using LookingGlass.ItemStatsNameSpace;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Aetherium.Utils.MathHelpers;
using StatType = LookingGlass.ItemStatsNameSpace.ItemStatsDef.ValueType;
using Units = LookingGlass.ItemStatsNameSpace.ItemStatsDef.MeasurementUnits;

namespace Aetherium.Compatability
{
    /// <summary>
    /// Adds the extra stat lines LookingGlass shows under item descriptions (e.g. "Sword Damage: 250%").
    /// Every method touching LookingGlass types is NoInlining so nothing loads unless LookingGlass is installed.
    /// Values mirror the formulas in each item's hooks, not just its description.
    /// </summary>
    internal static class LookingGlassCompat
    {
        public const string GUID = "droppod.lookingglass";
        public static bool IsLookingGlassInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GUID);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Init()
        {
            ItemCatalog.availability.CallWhenAvailable(RegisterItemStats);
            EquipmentCatalog.availability.CallWhenAvailable(RegisterEquipmentStats);
        }

        private struct Line
        {
            public string description;
            public StatType valueType;
            public Units units;

            public Line(string description, StatType valueType, Units units)
            {
                this.description = description;
                this.valueType = valueType;
                this.units = units;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ItemStatsDef Create(Line[] lines)
        {
            var def = new ItemStatsDef();
            foreach(var line in lines)
            {
                def.descriptions.Add(line.description);
                def.valueTypes.Add(line.valueType);
                def.measurementUnits.Add(line.units);
            }
            return def;
        }

        /// <summary>Stats that only depend on the stack count.</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void AddItem(ItemBase item, Func<int, List<float>> values, params Line[] lines)
        {
            var def = Create(lines);
            def.calculateValuesFlat = values;
            Register(item, def);
        }

        /// <summary>Stats with a chance that LookingGlass should adjust for luck.</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void AddChanceItem(ItemBase item, Func<float, int, List<float>> values, params Line[] lines)
        {
            var def = Create(lines);
            def.hasChance = true;
            def.calculateValuesNew = (luck, stackCount, procChance) => values(luck, stackCount);
            Register(item, def);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void AddEquipment(EquipmentBase equipment, Func<List<float>> values, params Line[] lines)
        {
            if(equipment == null || !equipment.EquipmentDef || equipment.EquipmentDef.equipmentIndex == EquipmentIndex.None) return;

            var def = Create(lines);
            def.calculateValuesFlat = count => values();
            ItemDefinitions.allEquipmentDefinitions[(int)equipment.EquipmentDef.equipmentIndex] = def;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Register(ItemBase item, ItemStatsDef def)
        {
            // Disabled items never get an instance or an ItemDef.
            if(item == null || !item.ItemDef || item.ItemDef.itemIndex == ItemIndex.None) return;

            ItemDefinitions.allItemDefinitions[(int)item.ItemDef.itemIndex] = def;
        }

        private static float Stack(float initial, float additional, int count) => initial + additional * (count - 1);

        /// <summary>Same rerolling Util.CheckRoll does for luck (LookingGlass's own helper is internal). Chance is 0..1.</summary>
        private static float ChanceWithLuck(float chance, float luck)
        {
            chance = Math.Min(chance, 1f);
            int rolls = (int)Math.Ceiling(luck);
            if(rolls > 0) return 1f - (float)Math.Pow(1f - chance, rolls + 1);
            if(rolls < 0) return (float)Math.Pow(chance, -rolls + 1);
            return chance;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RegisterItemStats()
        {
            try
            {
                RegisterTier1();
                RegisterTier2();
                RegisterTier3();
                RegisterLunar();
            }
            catch(Exception e)
            {
                AetheriumPlugin.ModLogger.LogError("Failed to register LookingGlass item stats: " + e);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RegisterTier1()
        {
            AddItem(FeatheredPlume.instance, count => new List<float>
                {
                    FeatheredPlume.MoveSpeedPercentageBonusPerBuffStack * FeatheredPlume.BuffStacksPerFeatheredPlume * count,
                    FeatheredPlume.BuffStacksPerFeatheredPlume * count,
                    Stack(FeatheredPlume.BaseDurationOfBuffInSeconds, FeatheredPlume.AdditionalDurationOfBuffInSeconds, count),
                },
                new Line("Max Speed Boost: ", StatType.Utility, Units.Percentage),
                new Line("Max Buff Stacks: ", StatType.Utility, Units.Number),
                new Line("Buff Duration: ", StatType.Utility, Units.Seconds));

            // Each nail deals the per-nail coefficient of the grenade's damage, which grows with stacks.
            if(NailBomb.UseAlternateImplementation)
            {
                AddItem(NailBomb.instance, count => new List<float>
                    {
                        NailBomb.NailBombDropDelay / (1 + NailBomb.DurationPercentageReducedByWithAdditionalStacks * (count - 1)),
                        NailBomb.PercentDamagePerNailInNailBomb * Stack(1f, NailBomb.PercentDamageBonusOfAdditionalStacks, count),
                        NailBomb.AmountOfNailsPerNailBomb,
                    },
                    new Line("Drop Interval: ", StatType.Utility, Units.Seconds),
                    new Line("Damage per Nail: ", StatType.Damage, Units.Percentage),
                    new Line("Nails: ", StatType.Damage, Units.Number));
            }
            else
            {
                AddItem(NailBomb.instance, count => new List<float>
                    {
                        NailBomb.PercentDamageThresholdRequiredToActivate,
                        NailBomb.PercentDamagePerNailInNailBomb * Stack(1f, NailBomb.PercentDamageBonusOfAdditionalStacks, count),
                        NailBomb.AmountOfNailsPerNailBomb,
                    },
                    new Line("Damage Threshold: ", StatType.Damage, Units.Percentage),
                    new Line("Damage per Nail: ", StatType.Damage, Units.Percentage),
                    new Line("Nails: ", StatType.Damage, Units.Number));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RegisterTier2()
        {
            AddItem(BloodthirstyShield.instance, count => new List<float>
                {
                    InverseHyperbolicScaling(BloodthirstyShield.ShieldPercentageRestoredPerKill, BloodthirstyShield.AdditionalShieldPercentageRestoredPerKillDiminishing, BloodthirstyShield.MaximumPercentageShieldRestoredPerKill, count),
                    BloodthirstyShield.BaseGrantShieldMultiplier,
                },
                new Line("Shield Restored per Kill: ", StatType.Healing, Units.Percentage),
                new Line("Shield from Max Health: ", StatType.Healing, Units.Percentage));

            AddChanceItem(EngineersToolbelt.instance, (luck, count) => new List<float>
                {
                    ChanceWithLuck(InverseHyperbolicScaling(EngineersToolbelt.BaseDuplicationPercentChance, EngineersToolbelt.AdditionalDuplicationPercentChance, 1, count), luck),
                    ChanceWithLuck(InverseHyperbolicScaling(EngineersToolbelt.BaseRevivalPercentChance, EngineersToolbelt.AdditionalRevivalPercentChance, 1, count), luck),
                },
                new Line("Duplication Chance: ", StatType.Utility, Units.Percentage),
                new Line("Revival Chance: ", StatType.Utility, Units.Percentage));

            // Same curve as SharkTeeth.MitigateDamage, including its 90% clamp.
            Func<int, float> sharkTeethDelayed = count => Mathf.Clamp(SharkTeeth.BaseDamageSpreadPercentage
                + (SharkTeeth.MaxDamageSpreadPercentage - SharkTeeth.MaxDamageSpreadPercentage / (1f + SharkTeeth.AdditionalDamageSpreadPercentage * (count - 1))), 0f, 0.9f);
            if(SharkTeeth.EnableCleanseOnHit)
            {
                AddItem(SharkTeeth.instance, count => new List<float>
                    {
                        sharkTeethDelayed(count),
                        SharkTeeth.DurationOfDamageSpread,
                        SharkTeeth.CleansePercentage,
                    },
                    new Line("Damage Delayed: ", StatType.Utility, Units.Percentage),
                    new Line("Bleed Duration: ", StatType.Utility, Units.Seconds),
                    new Line("Bleed Cleansed per Hit: ", StatType.Healing, Units.Percentage));
            }
            else
            {
                AddItem(SharkTeeth.instance, count => new List<float>
                    {
                        sharkTeethDelayed(count),
                        SharkTeeth.DurationOfDamageSpread,
                    },
                    new Line("Damage Delayed: ", StatType.Utility, Units.Percentage),
                    new Line("Bleed Duration: ", StatType.Utility, Units.Seconds));
            }

            AddItem(ShieldingCore.instance, count => new List<float>
                {
                    Stack(ShieldingCore.BaseShieldingCoreArmorGrant, ShieldingCore.AdditionalShieldingCoreArmorGrant, count),
                    ShieldingCore.BaseGrantShieldMultiplier,
                },
                new Line("Armor While Shielded: ", StatType.Armor, Units.Number),
                new Line("Shield from Max Health: ", StatType.Healing, Units.Percentage));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RegisterTier3()
        {
            AddItem(BlasterSword.instance, count => new List<float>
                {
                    Stack(BlasterSword.BaseSwordDamageMultiplier, BlasterSword.AdditionalSwordDamageMultiplier, count),
                },
                new Line("Sword Damage: ", StatType.Damage, Units.Percentage));

            if(InspiringDrone.EnableDeathExplosion)
            {
                // The explosion uses hardcoded values in DetonateInspiredDrone, not the death explosion configs.
                AddItem(InspiringDrone.instance, count => new List<float>
                    {
                        InspiringDrone.AllStatValueGrantedPercentage * count,
                        3f * count,
                        12f + 2f * count,
                    },
                    new Line("Stats Shared: ", StatType.Utility, Units.Percentage),
                    new Line("Explosion Damage: ", StatType.Damage, Units.Percentage),
                    new Line("Explosion Radius: ", StatType.Damage, Units.Meters));
            }
            else
            {
                AddItem(InspiringDrone.instance, count => new List<float>
                    {
                        InspiringDrone.AllStatValueGrantedPercentage * count,
                    },
                    new Line("Stats Shared: ", StatType.Utility, Units.Percentage));
            }

            AddItem(WitchesRing.instance, count => new List<float>
                {
                    WitchesRing.WitchesRingTriggerThreshold,
                    WitchesRing.BaseCooldownDuration / (1f + WitchesRing.AdditionalCooldownReduction * (count - 1)),
                },
                new Line("Damage Threshold: ", StatType.Damage, Units.Percentage),
                new Line("Cooldown: ", StatType.Utility, Units.Seconds));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RegisterLunar()
        {
            AddItem(AccursedPotion.instance, count => new List<float>
                {
                    AccursedPotion.BaseSipCooldownDuration * Mathf.Pow(AccursedPotion.AdditionalStackSipCooldownReductionPercentage, count - 1),
                    Stack(AccursedPotion.BaseRadiusGranted, AccursedPotion.AdditionalRadiusGranted, count),
                },
                new Line("Sip Cooldown: ", StatType.Utility, Units.Seconds),
                new Line("Sharing Radius: ", StatType.Utility, Units.Meters));

            if(AlienMagnet.UseOldFunctionality)
            {
                AddItem(AlienMagnet.instance, count => new List<float>
                    {
                        Mathf.Clamp(Stack(AlienMagnet.OldStartingForceMultiplier, AlienMagnet.OldAdditionalForceMultiplier, count), AlienMagnet.OldMinimumForceMultiplier, AlienMagnet.OldMaximumForceMultiplier),
                    },
                    new Line("Pull Force Multiplier: ", StatType.Utility, Units.Number));
            }
            else
            {
                AddItem(AlienMagnet.instance, count => new List<float>
                    {
                        Stack(AlienMagnet.NewStartingForceMultiplier, AlienMagnet.NewAdditionalForceMultiplier, count),
                    },
                    new Line("Pull Force Multiplier: ", StatType.Utility, Units.Number));
            }

            AddItem(AlienMagnetGravityRework.instance, count => new List<float>
                {
                    AlienMagnetGravityRework.HitsRequiredToLevitate,
                    AlienMagnetGravityRework.LevitationDuration * count,
                    Stack(1f, AlienMagnetGravityRework.PullStrengthIncrease, count),
                    1f / (1f + (count - 1) * AlienMagnetGravityRework.SelfPullReduction),
                },
                new Line("Hits to Levitate: ", StatType.Utility, Units.Number),
                new Line("Levitation Duration: ", StatType.Utility, Units.Seconds),
                new Line("Pull Strength: ", StatType.Utility, Units.Percentage),
                new Line("Pull on Self: ", StatType.Utility, Units.Percentage));

            // Boost items give 10% damage/HP/attack speed each and hooves give 14% movement speed.
            // The damage count mirrors the item code: base + additional * count - 1 (not additional * (count - 1)).
            AddItem(UnstableDesign.instance, count => new List<float>
                {
                    (UnstableDesign.LunarChimeraBaseDamageBoost + (UnstableDesign.LunarChimeraAdditionalDamageBoost * count - 1)) * 0.1f,
                    UnstableDesign.LunarChimeraBaseHPBoost * count * 0.1f,
                    UnstableDesign.LunarChimeraBaseAttackSpeedBoost * 0.1f,
                    UnstableDesign.LunarChimeraBaseMovementSpeedBoost * count * 0.14f,
                    UnstableDesign.LunarChimeraResummonCooldownDuration,
                },
                new Line("Chimera Damage Boost: ", StatType.Damage, Units.Percentage),
                new Line("Chimera Health Boost: ", StatType.Health, Units.Percentage),
                new Line("Chimera Attack Speed Boost: ", StatType.Damage, Units.Percentage),
                new Line("Chimera Movement Speed Boost: ", StatType.Utility, Units.Percentage),
                new Line("Summon Cooldown: ", StatType.Utility, Units.Seconds));

            AddItem(Voidheart.instance, count => new List<float>
                {
                    Voidheart.VoidImplosionDamageMultiplier,
                    Stack(Voidheart.VoidImplosionBaseRadius, Voidheart.VoidImplosionAdditionalRadius, count),
                    Mathf.Clamp(Stack(Voidheart.VoidHeartBaseTickingTimeBombHealthThreshold, Voidheart.VoidHeartAdditionalTickingTimeBombHealthThreshold, count), Voidheart.VoidHeartBaseTickingTimeBombHealthThreshold, Voidheart.VoidHeartMaxTickingTimeBombHealthThreshold),
                    Voidheart.VoidHeartCooldownDebuffDuration,
                },
                new Line("Implosion Damage: ", StatType.Void, Units.Percentage),
                new Line("Implosion Radius: ", StatType.Void, Units.Meters),
                new Line("Health Threshold: ", StatType.Death, Units.Percentage),
                new Line("Cooldown: ", StatType.Utility, Units.Seconds));

            AddItem(WeightedAnklet.instance, count => new List<float>
                {
                    Mathf.Min(count * WeightedAnklet.BaseAttackSpeedReductionPercentage, WeightedAnklet.AttackSpeedReductionPercentageCap),
                    Mathf.Min(count * WeightedAnklet.BaseMovementSpeedReductionPercentage, WeightedAnklet.MovementSpeedReductionPercentageCap),
                    1f - Mathf.Clamp01(1f - count * WeightedAnklet.BaseKnockbackReductionPercentage),
                },
                new Line("Attack Speed Penalty: ", StatType.Death, Units.Percentage),
                new Line("Movement Speed Penalty: ", StatType.Death, Units.Percentage),
                new Line("Knockback Reduction: ", StatType.Utility, Units.Percentage));

            // Attack speed is multiplied by base^count at 0 buff stacks, rising per buff stack up to the cap.
            // Zenith Accelerator's configs are instance fields, so read them off the instance.
            var zenith = ZenithAccelerator.instance;
            if(zenith != null)
            {
                AddItem(zenith, count => new List<float>
                    {
                        Mathf.Pow(zenith.ExponentialBaseMultiplier, count),
                        Stack(zenith.BaseMaximumPercentageCap, zenith.AdditionalMaximumPercentageCap, count),
                        zenith.PercentageIncreasePerHit,
                    },
                    new Line("Minimum Attack Speed: ", StatType.Death, Units.Percentage),
                    new Line("Maximum Attack Speed: ", StatType.Damage, Units.Percentage),
                    new Line("Attack Speed per Hit: ", StatType.Damage, Units.Percentage));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RegisterEquipmentStats()
        {
            try
            {
                AddEquipment(BellTotem.instance, () => new List<float>
                    {
                        BellTotem.RadiusOfBellRinging,
                        BellTotem.TimeBetweenUses,
                    },
                    new Line("Shockwave Radius: ", StatType.Utility, Units.Meters),
                    new Line("Time Between Rings: ", StatType.Utility, Units.Seconds));

                AddEquipment(Faust.instance, () => new List<float>
                    {
                        Faust.ResourceMultiplierPerHit,
                        Faust.HealthThresholdPercentage,
                        Faust.MaxActiveBargains,
                    },
                    new Line("Kill Reward per Hit: ", StatType.Gold, Units.Percentage),
                    new Line("Health Lost Before Hat Falls: ", StatType.Health, Units.Percentage),
                    new Line("Max Active Bargains: ", StatType.Utility, Units.Number));

                AddEquipment(JarOfReshaping.instance, () => new List<float>
                    {
                        JarOfReshaping.BaseRadiusGranted,
                        JarOfReshaping.ProjectileAbsorptionTime,
                    },
                    new Line("Absorption Radius: ", StatType.Utility, Units.Meters),
                    new Line("Absorption Duration: ", StatType.Utility, Units.Seconds));
            }
            catch(Exception e)
            {
                AetheriumPlugin.ModLogger.LogError("Failed to register LookingGlass equipment stats: " + e);
            }
        }
    }
}
