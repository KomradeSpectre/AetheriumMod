using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Items.VoidItems
{
    internal class UmbralAspis : ItemBase<UmbralAspis>
    {
        public override string ItemName => "Umbral Aspis";

        public override string ItemLangTokenName => "UMBRAL_ASPIS";

        public override string ItemPickupDesc => "When ";

        public override string ItemFullDescription => throw new NotImplementedException();

        public override string ItemLore => throw new NotImplementedException();

        public override ItemTier Tier => throw new NotImplementedException();

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
