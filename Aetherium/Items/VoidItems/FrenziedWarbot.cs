using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Items.VoidItems
{
    internal class FrenziedWarbot : ItemBase<FrenziedWarbot>
    {
        public override string ItemName => "Frenzied Warbot";

        public override string ItemLangTokenName => "FRENZIED_WARBOT";

        public override string ItemPickupDesc => "Gain an undying combat drone that attacks enemies in close range. It gains strength based on the number of bots in your party. Corrupts Inspiring Drone.";

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
