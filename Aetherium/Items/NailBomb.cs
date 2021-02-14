using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aetherium.Items
{
    public class NailBomb : ItemBase<NailBomb>
    {
        public override string ItemName => "Nail Bomb";

        public override string ItemLangTokenName => "NAIL_BOMB";

        public override string ItemPickupDesc => $"Attacks that deal <style=cIsDamage>high damage</style> release a shrapnel grenade that explodes after a delay.";
        public override string ItemFullDescription => $"Attacks that deal 300% damage or more release a shrapnel grenade that explodes for 10x30% damage. Cooldown of 8s (-15% per stack).";

        public override string ItemLore => throw new NotImplementedException();

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public override ItemTier Tier => ItemTier.Tier1;

        public override string ItemModelPath => throw new NotImplementedException();

        public override string ItemIconPath => throw new NotImplementedException();

        public override void Init(ConfigFile config)
        {
            throw new NotImplementedException();
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            throw new NotImplementedException();
        }

        public override void Hooks()
        {
            throw new NotImplementedException();
        }
    }
}
