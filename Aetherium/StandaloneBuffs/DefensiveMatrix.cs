using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.StandaloneBuffs
{
    internal class DefensiveMatrix : BuffBase<DefensiveMatrix>
    {
        public override string BuffName => "Defensive Matrix";

        public override Color Color => new Color(0, 199, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("");

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateBuff();
        }

        public override void Hooks()
        {
            throw new NotImplementedException();
        }
    }
}
