using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aetherium.Compatability
{
    internal static class ModCompatability
    {
        internal static class BetterAPICompat
        {
            public static void RegisterBuffInfo(BuffDef buffDef, string nameToken, string descriptionToken)
            {
                BetterUI.Buffs.RegisterBuffInfo(buffDef, nameToken, descriptionToken);
            }
        }

        internal static class ItemStatsModCompat
        {

        }
    }
}
