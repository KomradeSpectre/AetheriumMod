using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Interactables.BuffBrazier;
using static Aetherium.Compatability.ModCompatability.BetterUICompat;
using RoR2;

namespace Aetherium.StandaloneBuffs.Tier1
{
    public class DoubleXPDoubleGold : BuffBase<DoubleXPDoubleGold>
    {
        public override string BuffName => "Double XP and Double Gold";

        public override Color Color => new Color32(255, 215, 0, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("DoubleGoldDoubleXPBuffIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.DeathRewards.OnKilledServer += DoubleXPAndGold;
            RoR2Application.onLoad += OnLoadModCompat;
        }

        private void OnLoadModCompat()
        {
            if (Compatability.ModCompatability.BetterUICompat.IsBetterUIInstalled)
            {
                var buffInfo = CreateBetterUIBuffInformation($"AETHERIUM_DOUBLE_GOLD_DOUBLE_XP_BUFF", BuffName, "All kills done by you grant double gold and double xp to you.");
                RegisterBuffInfo(BuffDef, buffInfo.Item1, buffInfo.Item2);
            }

            if (Aetherium.Interactables.BuffBrazier.instance != null)
            {
                AddCuratedBuffType("Double Gold and Double XP", BuffDef, Color, 1.25f, false);
            }
        }

        private void DoubleXPAndGold(On.RoR2.DeathRewards.orig_OnKilledServer orig, RoR2.DeathRewards self, RoR2.DamageReport damageReport)
        {
            if(damageReport.attackerBody && damageReport.attackerBody.HasBuff(BuffDef))
            {
                self.goldReward *= 2;
                self.expReward *= 2;
            }
            orig(self, damageReport);
        }
    }
}
