using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Utils.MiscHelpers;

namespace Aetherium.Items.VoidItems
{
    public class PhasewindFeather : ItemBase<PhasewindFeather>
    {
        public static ConfigOption<float> BaseDurationOfBuffInSeconds;
        public static ConfigOption<float> AdditionalDurationOfBuffInSeconds;
        public static ConfigOption<int> BuffStacksPerPhasewindFeather;
        public static ConfigOption<float> BaseJumpHeightBonusMult;
        public static ConfigOption<float> AdditionalStacksJumpHeightBonusMult;
        public static ConfigOption<float> FallDamageReductionMult;
        public static ConfigOption<float> AdditionalFallDamageReductionMult;
        public static ConfigOption<float> PushbackForceMult;

        public override string ItemName => "Phasewind Feather";

        public override string ItemLangTokenName => "PHASEWIND_FEATHER";

        public override string ItemPickupDesc => "On taking damage, gain increased jump height and fall damage reduction for a duration. Upon landing from a jump, slightly push enemies back.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.VoidTier1;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("FeatheredPlume.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("FeatheredPlumeIcon.png");

        public BuffDef PhasewindWingBuff;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuff();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            BaseDurationOfBuffInSeconds = config.ActiveBind<float>("Item: " + ItemName, "Base Duration of Buff with One Phasewind Feather", 5f, "How many seconds should Phasewind Feather's buff last with a single stack of the item?");
            AdditionalDurationOfBuffInSeconds = config.ActiveBind<float>("Item: " + ItemName, "Additional Duration of Buff per Phasewind Feather Stack", 0.5f, "How many additional seconds of buff should each Phasewind Feather after the first give?");
            BuffStacksPerPhasewindFeather = config.ActiveBind<int>("Item: " + ItemName, "Stacks of Buff per Phasewind Feather", 3, "How many buff stacks should each feather give?");
        }

        private void CreateBuff()
        {
            PhasewindWingBuff = ScriptableObject.CreateInstance<BuffDef>();
            PhasewindWingBuff.name = "Aetherium: Phasewind Wing";
            PhasewindWingBuff.iconSprite = MainAssets.LoadAsset<Sprite>("FeatheredPlumeIcon.png");

        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += GrantVerticalMobilityBoosts;
        }

        private void GrantVerticalMobilityBoosts(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            var body = self.body;
            if (body)
            {
                var buffCount = body.GetBuffCount(SpeedBuff);
                var InventoryCount = GetCount(body);

                if (InventoryCount > 0)
                {
                    var stackTime = BaseDurationOfBuffInSeconds + (AdditionalDurationOfBuffInSeconds * (InventoryCount - 1));
                    if (buffCount < BuffStacksPerPhasewindFeather * InventoryCount)
                    {
                        RefreshTimedBuffs(body, SpeedBuff, stackTime);
                        body.AddTimedBuffAuthority(SpeedBuff.buffIndex, stackTime);
                    }
                    if (buffCount >= BuffStacksPerPhasewindFeather * InventoryCount)
                    {
                        RefreshTimedBuffs(body, SpeedBuff, stackTime);
                    }
                }
            }
            orig(self, damageInfo);
        }
    }
}
