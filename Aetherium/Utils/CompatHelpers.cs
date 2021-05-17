using System;
using R2API;
using System.Collections.Generic;
using System.Text;

namespace Aetherium.Utils
{
    public static class CompatHelpers
    {
        public static Tuple<string, string> CreateBetterUIBuffInformation(string langTokenName, string name, string description, bool isBuff = true)
        {
            string nameToken = isBuff ? $"BUFF_{langTokenName}_NAME" : $"DEBUFF_{langTokenName}_NAME";
            string descToken = isBuff ? $"BUFF_{langTokenName}_DESC" : $"DEBUFF_{langTokenName}_DESC";

            LanguageAPI.Add(nameToken, name);
            LanguageAPI.Add(descToken, description);

            return Tuple.Create(nameToken, descToken);
        }
    }
}
