using Aetherium.Items;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Aetherium.Utils.MathHelpers;
using System.Text;
using UnityEngine;

namespace Aetherium.Compatability
{
    internal static class ModCompatability
    {
        internal static class BetterAPICompat
        {
            public static void RegisterBuffInfo(BuffDef buffDef, string nameToken, string descriptionToken)
            {
                BetterUI.Buffs.RegisterBuffInfo(buffDef, nameToken, descriptionToken);
            }
        }

        internal static class ItemStatsModCompat
        {
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateAccursedPotionStatDef()
            {
                ItemStats.ItemStatDef SipCooldownStatDef = new ItemStats.ItemStatDef
                {

                    Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => AccursedPotion.BaseSipCooldownDuration * (float)Math.Pow(AccursedPotion.AdditionalStackSipCooldownReductionPercentage, itemCount - 1),
                        (value, ctx) => $"Sip Cooldown Duration: {value} second(s)"
                    ),

                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => AccursedPotion.BaseRadiusGranted + (AccursedPotion.AdditionalRadiusGranted * (itemCount - 1)),
                        (value, ctx) => $"Effect Sharing Radius: {value} meter(s)"
                    )
                }
                };

                ItemStats.ItemStatsMod.AddCustomItemStatDef(AccursedPotion.instance.ItemDef.itemIndex, SipCooldownStatDef);
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateAlienMagnetStatDef()
            {
                ItemStats.ItemStatDef AlienMagnetStatDefs = new ItemStats.ItemStatDef
                {
                    Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => Mathf.Clamp(AlienMagnet.StartingForceMultiplier + (AlienMagnet.AdditionalForceMultiplier * (itemCount - 1)), AlienMagnet.MinimumForceMultiplier, AlienMagnet.MaximumForceMultiplier),
                        (value, ctx) => $"Pull Force Multiplier: {value}"
                    )
                }
                };
                ItemStats.ItemStatsMod.AddCustomItemStatDef(AlienMagnet.instance.ItemDef.itemIndex, AlienMagnetStatDefs);
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateBlasterSwordStatDef()
            {
                ItemStats.ItemStatDef BlasterSwordStatDefs = new ItemStats.ItemStatDef
                {
                    Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => BlasterSword.BaseSwordDamageMultiplier + (BlasterSword.AdditionalSwordDamageMultiplier * (itemCount - 1)),
                        (value, ctx) => $"Current Sword Beam Damage Multiplier: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? ctx.Master.GetBody().damage * (BlasterSword.BaseSwordDamageMultiplier + (BlasterSword.AdditionalSwordDamageMultiplier * (itemCount - 1))) : 0,
                        (value, ctx) => $"Current Sword Beam Damage: {ItemStats.ValueFormatters.Extensions.FormatInt(value, " Damage")}"
                    )
                }
                };
                ItemStats.ItemStatsMod.AddCustomItemStatDef(BlasterSword.instance.ItemDef.itemIndex, BlasterSwordStatDefs);
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateBloodSoakedShieldStatDef()
            {
                ItemStats.ItemStatDef BloodSoakedShieldStatDef = new ItemStats.ItemStatDef
                {
                    Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => InverseHyperbolicScaling(BloodSoakedShield.ShieldPercentageRestoredPerKill, BloodSoakedShield.AdditionalShieldPercentageRestoredPerKillDiminishing, BloodSoakedShield.MaximumPercentageShieldRestoredPerKill, (int)itemCount),
                        (value, ctx) => $"Shield Restored Per Kill: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? ctx.Master.GetBody().healthComponent.fullHealth * BloodSoakedShield.BaseGrantShieldMultiplier: 0,
                        (value, ctx) => $"Base Shield Granted By Item: {ItemStats.ValueFormatters.Extensions.FormatInt(value, " Shield")}"
                    )
                }
                };
                ItemStats.ItemStatsMod.AddCustomItemStatDef(BloodSoakedShield.instance.ItemDef.itemIndex, BloodSoakedShieldStatDef);
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateEngineersToolbeltStatDef()
            {
                ItemStats.ItemStatDef EngineersToolbeltStatDefs = new ItemStats.ItemStatDef
                {
                    Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => InverseHyperbolicScaling(EngineersToolbelt.BaseDuplicationPercentChance, EngineersToolbelt.AdditionalDuplicationPercentChance, 1, (int)itemCount),
                        (value, ctx) => $"Chance To Duplicate Drone/Turret On Purchase: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => InverseHyperbolicScaling(EngineersToolbelt.BaseRevivalPercentChance, EngineersToolbelt.AdditionalRevivalPercentChance, 1, (int)itemCount),
                        (value, ctx) => $"Chance To Revive Drone/Turret On Death: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    )
                }
                };
                ItemStats.ItemStatsMod.AddCustomItemStatDef(EngineersToolbelt.instance.ItemDef.itemIndex, EngineersToolbeltStatDefs);
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateFeatheredPlumeStatDef()
            {
                ItemStats.ItemStatDef FeatheredPlumeStatDefs = new ItemStats.ItemStatDef
                {
                    Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => FeatheredPlume.MoveSpeedPercentageBonusPerBuffStack * (FeatheredPlume.BuffStacksPerFeatheredPlume * itemCount),
                        (value, ctx) => $"Current Max Speed Bonus: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => FeatheredPlume.BaseDurationOfBuffInSeconds + (FeatheredPlume.AdditionalDurationOfBuffInSeconds * (itemCount - 1)),
                        (value, ctx) => $"Current Max Buff Duration: {value} second(s)"
                    )
                }
                };
                ItemStats.ItemStatsMod.AddCustomItemStatDef(FeatheredPlume.instance.ItemDef.itemIndex, FeatheredPlumeStatDefs);
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateInspiringDroneStatDef()
            {
                ItemStats.ItemStatDef InspiringDroneStatDefs = new ItemStats.ItemStatDef();

                if (InspiringDrone.SetAllStatValuesAtOnce)
                {
                    InspiringDroneStatDefs.Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => InspiringDrone.AllStatValueGrantedPercentage * itemCount,
                        (value, ctx) => $"Stats Currently Inherited by Bots: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    )
                };
                }
                else
                {
                    InspiringDroneStatDefs.Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? ctx.Master.GetBody().damage * (InspiringDrone.DamageGrantedPercentage * itemCount) : 0,
                        (value, ctx) => $"Damage Currently Inherited: {value}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? ctx.Master.GetBody().attackSpeed * (InspiringDrone.AttackSpeedGrantedPercentage * itemCount) : 0,
                        (value, ctx) => $"Attack Speed Currently Inherited: {value}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? ctx.Master.GetBody().crit * (InspiringDrone.CritChanceGrantedPercentage * itemCount) : 0,
                        (value, ctx) => $"Crit Chance Currently Inherited: {value}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? ctx.Master.GetBody().maxHealth * (InspiringDrone.HealthGrantedPercentage * itemCount) : 0,
                        (value, ctx) => $"Health Currently Inherited: {value}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? ctx.Master.GetBody().regen * (InspiringDrone.RegenGrantedPercentage * itemCount) : 0,
                        (value, ctx) => $"Regen Currently Inherited: {value}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? ctx.Master.GetBody().armor * (InspiringDrone.ArmorGrantedPercentage * itemCount) : 0,
                        (value, ctx) => $"Armor Currently Inherited: {value}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? ctx.Master.GetBody().moveSpeed * (InspiringDrone.MovementSpeedGrantedPercentage * itemCount) : 0,
                        (value, ctx) => $"Movement Speed Currently Inherited: {value}"
                    ),
                };
                }

                ItemStats.ItemStatsMod.AddCustomItemStatDef(InspiringDrone.instance.ItemDef.itemIndex, InspiringDroneStatDefs);
            }

            /*[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateNailBombStatDef()
            {
                ItemStats.ItemStatDef NailBombStatDefs = new ItemStats.ItemStatDef();

                if (NailBomb.UseAlternateImplementation)
                {
                    NailBombStatDefs.Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? (ctx.Master.GetBody().damage * (1 + NailBomb.PercentDamageBonusOfAdditionalStacks * itemCount)) * NailBomb.PercentDamagePerNailInNailBomb: 0,
                        (value, ctx) => $"Current Damage per Nail: {value}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => NailBomb.NailBombDropDelay / (1 + NailBomb.DurationPercentageReducedByWithAdditionalStacks * (itemCount - 1)),
                        (value, ctx) => $"Current Nail Bomb Cooldown Duration: {value} second(s)"
                    )
                };
                }
                else
                {
                    NailBombStatDefs.Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? (ctx.Master.GetBody().damage * (1 + NailBomb.PercentDamageBonusOfAdditionalStacks * itemCount)) * NailBomb.PercentDamagePerNailInNailBomb: 0,
                        (value, ctx) => $"Current Damage per Nail: {value}"
                    )
                };
                }

                ItemStats.ItemStatsMod.AddCustomItemStatDef(NailBomb.instance.ItemDef.itemIndex, NailBombStatDefs);
            }*/

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateSharkTeethStatDef()
            {
                ItemStats.ItemStatDef SharkTeethStatDefs = new ItemStats.ItemStatDef
                {
                    Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => SharkTeeth.BaseDamageSpreadPercentage + (SharkTeeth.MaxDamageSpreadPercentage - SharkTeeth.MaxDamageSpreadPercentage / (1 + SharkTeeth.AdditionalDamageSpreadPercentage * (itemCount - 1))),
                        (value, ctx) => $"Current Damage Spread Percentage: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    )
                }
                };
                ItemStats.ItemStatsMod.AddCustomItemStatDef(SharkTeeth.instance.ItemDef.itemIndex, SharkTeethStatDefs);
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateShieldingCoreStatDef()
            {
                ItemStats.ItemStatDef ShieldingCoreStatDefs = new ItemStats.ItemStatDef
                {
                    Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => ShieldingCore.BaseShieldingCoreArmorGrant + (ShieldingCore.AdditionalShieldingCoreArmorGrant * (itemCount - 1)),
                        (value, ctx) => $"Current Armor Bonus Provided: {ItemStats.ValueFormatters.Extensions.FormatInt(value, " Armor")}"
                    )
                }
                };
                ItemStats.ItemStatsMod.AddCustomItemStatDef(ShieldingCore.instance.ItemDef.itemIndex, ShieldingCoreStatDefs);
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateUnstableDesignStatDef()
            {
                ItemStats.ItemStatDef UnstableDesignStatDefs = new ItemStats.ItemStatDef
                {
                    Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => 0.10f * (UnstableDesign.LunarChimeraBaseDamageBoost + (UnstableDesign.LunarChimeraAdditionalDamageBoost * (itemCount - 1))),
                        (value, ctx) => $"Current/Next Unstable Design Damage Boost Percentage: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => 0.10f * (UnstableDesign.LunarChimeraBaseHPBoost * itemCount),
                        (value, ctx) => $"Current/Next Unstable Design HP Boost Percentage: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => 0.14f * (UnstableDesign.LunarChimeraBaseMovementSpeedBoost * itemCount),
                        (value, ctx) => $"Current/Next Unstable Design Movement Speed Boost Percentage: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    ),
                }
                };
                ItemStats.ItemStatsMod.AddCustomItemStatDef(UnstableDesign.instance.ItemDef.itemIndex, UnstableDesignStatDefs);
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateVoidheartStatDef()
            {
                ItemStats.ItemStatDef VoidheartStatDefs = new ItemStats.ItemStatDef
                {
                    Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? ctx.Master.GetBody().damage * Voidheart.VoidImplosionDamageMultiplier : 0,
                        (value, ctx) => $"Current Void Implosion Damage: {ItemStats.ValueFormatters.Extensions.FormatInt(value, " Damage")}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => Voidheart.VoidImplosionBaseRadius + (Voidheart.VoidImplosionAdditionalRadius * (itemCount - 1)),
                        (value, ctx) => $"Current Void Implosion Radius: <style=cIsHealing>{value} meter(s)</style>"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => Mathf.Clamp((Voidheart.VoidHeartBaseTickingTimeBombHealthThreshold + (Voidheart.VoidHeartAdditionalTickingTimeBombHealthThreshold * itemCount - 1)), Voidheart.VoidHeartBaseTickingTimeBombHealthThreshold, Voidheart.VoidHeartMaxTickingTimeBombHealthThreshold),
                        (value, ctx) => $"Point of No Return (Ticking Timebomb) Healthbar Percentage: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    )
                }
                };
                ItemStats.ItemStatsMod.AddCustomItemStatDef(Voidheart.instance.ItemDef.itemIndex, VoidheartStatDefs);
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateWeightedAnkletStatDef()
            {
                ItemStats.ItemStatDef WeightedAnkletStatDefs = new ItemStats.ItemStatDef
                {
                    Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => Mathf.Min(itemCount * WeightedAnklet.BaseMovementSpeedReductionPercentage, WeightedAnklet.MovementSpeedReductionPercentageCap),
                        (value, ctx) => $"Weighted Anklet Movement Speed Reduction: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => Mathf.Min(itemCount * WeightedAnklet.BaseAttackSpeedReductionPercentage, WeightedAnklet.AttackSpeedReductionPercentageCap),
                        (value, ctx) => $"Weighted Anklet Attack Speed Reduction: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => WeightedAnklet.BaseKnockbackReductionPercentage * itemCount,
                        (value, ctx) => $"Weighted Anklet Knockback Reduction: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    ),
                }
                };
                ItemStats.ItemStatsMod.AddCustomItemStatDef(WeightedAnklet.instance.ItemDef.itemIndex, WeightedAnkletStatDefs);

                ItemStats.ItemStatDef LimiterReleaseStatDefs = new ItemStats.ItemStatDef
                {
                    Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => itemCount * WeightedAnklet.AttackSpeedGainedPerLimiterRelease,
                        (value, ctx) => $"Limiter Release Attack Speed Bonus: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => itemCount * WeightedAnklet.MovementSpeedGainedPerLimiterRelease,
                        (value, ctx) => $"Limiter Release Movement Speed Bonus: {ItemStats.ValueFormatters.Extensions.FormatPercentage(value)}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? ctx.Master.GetBody().damage * (itemCount * WeightedAnklet.DamagePercentageGainedPerLimiterRelease) : 0,
                        (value, ctx) => $"Limiter Release Damage Bonus: {ItemStats.ValueFormatters.Extensions.FormatInt(value, " Damage")}"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => WeightedAnklet.BaseCooldownOfLimiterReleaseDodge + (WeightedAnklet.AdditionalCooldownOfLimiterReleaseDodge * (itemCount - 1)),
                        (value, ctx) => $"Limiter Release Dodge Cooldown Duration: <style=cIsUtility>{value}</style> second(s)"
                    )
                }
                };
                ItemStats.ItemStatsMod.AddCustomItemStatDef(WeightedAnklet.LimiterReleaseItemDef.itemIndex, LimiterReleaseStatDefs);
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateWitchesRingStatDef()
            {
                ItemStats.ItemStatDef WitchesRingStatDefs = new ItemStats.ItemStatDef
                {
                    Stats = new List<ItemStats.Stat.ItemStat>()
                {
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => (ctx.Master != null && ctx.Master.GetBody()) ? ctx.Master.GetBody().damage * WitchesRing.WitchesRingTriggerThreshold : 0,
                        (value, ctx) => $"Single Hit Damage Required To Activate: <style=cIsHealing>{value} Damage</style>"
                    ),
                    new ItemStats.Stat.ItemStat
                    (
                        (itemCount, ctx) => WitchesRing.BaseCooldownDuration / (1 + WitchesRing.AdditionalCooldownReduction * (itemCount - 1)),
                        (value, ctx) => $"Current Cooldown Duration: <style=cIsHealing>{value} seconds(s)</style>"
                    )
                }
                };
                ItemStats.ItemStatsMod.AddCustomItemStatDef(WitchesRing.instance.ItemDef.itemIndex, WitchesRingStatDefs);
            }
        }
    }
}
