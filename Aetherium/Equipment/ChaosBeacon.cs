using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Equipment
{
    public class ChaosBeacon : EquipmentBase<ChaosBeacon>
    {
        public override string EquipmentName => "Chaos Beacon";

        public override string EquipmentLangTokenName => "CHAOS_BEACON";

        public override string EquipmentPickupDesc => "On use, place down a field that frenzies anything that enters it. Frenzied beings will attack the closest thing to them.";

        public override string EquipmentFullDescription => "";

        public override string EquipmentLore => "";

        public override bool IsLunar => true;

        public override GameObject EquipmentModel => new GameObject();

        public override Sprite EquipmentIcon => MainAssets.LoadAsset<Sprite>("FeatheredPlume.png");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            Hooks();
        }

        public void CreateConfig(ConfigFile config)
        {

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.master) { return false; }

            
        }
    }
}
