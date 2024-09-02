using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public ConfigOption<bool> ShouldWeAllowRegenToProcEffect;

        public override string ItemName => "Empathic Resonator";
        public override string ItemLangTokenName => "EMPATHIC_RESONATOR";

        public override string ItemPickupDesc => "On heal, have a chance to share a portion of your healing with allies nearby.";

        public override string ItemFullDescription => $"<style=cIsHealing>On heal</style>, have a {FloatToPercentageString(BaseHealingShareChancePercentage)} chance " +
            $"(+{FloatToPercentageString(AdditionalHealingShareChancePercentage)} per stack, hyperbolically) to clone {FloatToPercentageString(BaseHealingPortionPercentage)}" +
            $" of your healing (+{FloatToPercentageString(AdditionalHealingPortionPercentage)} per stack) on allies in a {BaseRadius}(m) radius (+{AdditionalRadiusPerStack}m range per stack).";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => GameObject.CreatePrimitive(PrimitiveType.Cube);

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("FeatheredPlumeIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            BaseHealingShareChancePercentage = config.ActiveBind<float>("Item: " + ItemName, "Base Healing Share Chance Percentage", 0.25f, "What should the very base chance percentage that we heal a nearby ally be?");
            AdditionalHealingShareChancePercentage = config.ActiveBind<float>("Item: " + ItemName, "Additional Healing Share Chance Percentage per Stack", 0.15f, "How much more percentage chance should we get per stack hyperbolically?");
            MaxHealingShareChancePercentage = config.ActiveBind<float>("Item: " + ItemName, "Maximum Healing Share Chance Percentage", 0.75f, "What should be the maximum percentage chance we can ever get with this item?");
            BaseHealingPortionPercentage = config.ActiveBind<float>("Item: " + ItemName, "Base Healing Portion Percentage", 0.5f, "What should be the base percentage of the portion of healing we apply to allies?");
            AdditionalHealingPortionPercentage = config.ActiveBind<float>("Item: " + ItemName, "Additional Healing Portion Percemtage", 0.1f, "How much more healing percentage should we add on top of the base one per additional stack?");
            BaseRadius = config.ActiveBind<float>("Item: " + ItemName, "Base Radius to Check for Allies to Heal", 10, "How far in meter should we search for allies to heal?");
            AdditionalRadiusPerStack = config.ActiveBind<float>("Item: " + ItemName, "Additional Radius to Check for Allies to Heal per Stack", 5, "How much further in meters should we search for allies to heal per stack?");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.Heal += PotentiallyShareHealing;
        }

        private float PotentiallyShareHealing(On.RoR2.HealthComponent.orig_Heal orig, RoR2.HealthComponent self, float amount, RoR2.ProcChainMask procChainMask, bool nonRegen)
        {
            if(self)
            {
                var body = self.body;
                if (body && body.teamComponent && body.master)
                {
                    var itemCount = GetCount(body);
                    if(itemCount > 0)
                    {
                        var hasProcced = Util.CheckRoll(InverseHyperbolicScaling(BaseHealingShareChancePercentage, AdditionalHealingShareChancePercentage, MaxHealingShareChancePercentage, itemCount), body.master);
                        if (hasProcced)
                        {
                            var eligableTeamMembers = TeamComponent.GetTeamMembers(body.teamComponent.teamIndex).Where(x => x && x.body != body && x.body && Vector3.Distance(x.body.corePosition, body.corePosition) <= BaseRadius + (AdditionalRadiusPerStack * (itemCount - 1)));

                            foreach(TeamComponent teamMemberComponent in eligableTeamMembers)
                            {
                                if (teamMemberComponent)
                                {
                                    var teamBody = teamMemberComponent.body;
                                    if (teamBody)
                                    {
                                        ModLogger.LogError($"Healing {teamBody} for {amount * BaseHealingPortionPercentage + (AdditionalHealingPortionPercentage * (itemCount - 1))}.");
                                        var healthComponent = teamMemberComponent.body.healthComponent;
                                        if (healthComponent)
                                        {
                                            healthComponent.Heal(amount * BaseHealingPortionPercentage + (AdditionalHealingPortionPercentage * (itemCount - 1)), default(ProcChainMask), true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return orig(self, amount, procChainMask, nonRegen);
        }
    }
}
