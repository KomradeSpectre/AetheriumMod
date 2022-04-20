using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Items.VoidItems
{
    internal class RazorfinWing : ItemBase<RazorfinWing>
    {
        public override string ItemName => "Razorfin Plume";

        public override string ItemLangTokenName => "RAZORFIN_PLUME";

        public override string ItemPickupDesc => "On taking damage, grow razor sharp wings that deal damage to enemies nearby based on your current velocity. Corrupts Feathered Plumes.";

        public override string ItemFullDescription => throw new NotImplementedException();

        public override string ItemLore => throw new NotImplementedException();

        public override ItemTier Tier => throw new NotImplementedException();

        public override string CorruptsItem => "ITEM_FEATHERED_PLUME";

        public override GameObject ItemModel => throw new NotImplementedException();

        public override Sprite ItemIcon => throw new NotImplementedException();

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            throw new NotImplementedException();
        }

        public override void Hooks()
        {
            throw new NotImplementedException();
        }

        public override void Init(ConfigFile config)
        {
            throw new NotImplementedException();
        }
    }
}
