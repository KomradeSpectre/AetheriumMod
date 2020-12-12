using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aetherium.Equipment
{
    public abstract class EquipmentBase<T> : EquipmentBase where T : EquipmentBase<T>
    {
        public static T RunningInstance { get; private set; }

        public EquipmentBase()
        {
            if (RunningInstance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting EquipmentBoilerplate/Equipment was instantiated twice");
            RunningInstance = this as T;
        }

    }

    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }

        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }

        public virtual bool AppearsInSinglePlayer { get; } = true;

        public virtual bool AppearsInMultiPlayer { get; } = true;

        public virtual bool CanDrop { get; } = true;

        public virtual float Cooldown { get; } = 60f;

        public virtual bool EnigmaCompatible { get; } = true;

        public virtual bool IsBoss { get; } = false;

        public virtual bool IsLunar { get; } = false;

        public EquipmentIndex IndexOfEquipment;

        public abstract void Init(ConfigFile config);

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
        protected void CreateLang()
        {
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);
        }

        protected void CreateEquipment()
        {
            EquipmentDef equipmentDef = new RoR2.EquipmentDef()
            {
                name = "EQUIPMENT_" + EquipmentLangTokenName,
                nameToken = "EQUIPMENT_" + EquipmentLangTokenName + "_NAME",
                pickupToken = "EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP",
                descriptionToken = "EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION",
                loreToken = "EQUIPMENT_" + EquipmentLangTokenName + "_LORE",
                pickupModelPath = EquipmentModelPath,
                pickupIconPath = EquipmentIconPath,
                appearsInSinglePlayer = AppearsInSinglePlayer,
                appearsInMultiPlayer = AppearsInMultiPlayer,
                canDrop = CanDrop,
                cooldown = Cooldown,
                enigmaCompatible = EnigmaCompatible,
                isBoss = IsBoss,
                isLunar = IsLunar
            };
            var itemDisplayRules = CreateItemDisplayRules();
            IndexOfEquipment = ItemAPI.Add(new CustomEquipment(equipmentDef, itemDisplayRules));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
        }

        private bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentIndex equipmentIndex)
        {
            if(equipmentIndex == IndexOfEquipment)
            {
                return ActivateEquipment(self);
            }
            else
            {
                return orig(self, equipmentIndex);
            }
        }

        protected abstract bool ActivateEquipment(EquipmentSlot slot);

        public abstract void Hooks();

    }
}
