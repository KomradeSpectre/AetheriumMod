using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.MathHelpers;

namespace Aetherium.Items.Tier2
{
    internal class EmpathicResonator : ItemBase<EmpathicResonator>
    {
        public ConfigOption<float> BaseHealingShareChancePercentage;
        public ConfigOption<float> AdditionalHealingShareChancePercentage;
        public ConfigOption<float> MaxHealingShareChancePercentage;
        public ConfigOption<float> BaseHealingPortionPercentage;
        public ConfigOption<float> AdditionalHealingPortionPercentage;
        public ConfigOption<float> BaseRadius;
        public ConfigOption<float> AdditionalRadiusPerStack;

        public override string ItemName => "Empathic Resonator";
        public override string ItemLangTokenName => "Empathic Resonator";

        public override string ItemPickupDesc => "On heal, have a chance to share a portion of your healing with allies nearby.";

        public override string ItemFullDescription => $"<style=cIsHealing>On heal</style>, have a {FloatToPercentageString(BaseHealingShareChancePercentage)} chance " +
            $"(+{FloatToPercentageString(AdditionalHealingShareChancePercentage)} per stack, hyperbolically) to clone {FloatToPercentageString(BaseHealingPortionPercentage)}" +
            $" of your healing (+{FloatToPercentageString(AdditionalHealingPortionPercentage)} per stack) on allies in a {BaseRadius}(m) radius (+{AdditionalRadiusPerStack}m range per stack).";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => new GameObject();

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("FeatheredPlumeIcon.png");

        public override void Init(ConfigFile config)
        {

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            
        }
    }
}
