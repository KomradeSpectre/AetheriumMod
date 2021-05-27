using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2API;
using RoR2;

namespace Aetherium.CoreModules
{
    public class ItemHelperModule : CoreModule
    {
        public override string Name => "Item Helper Module";

        public static List<EquipmentDef> EliteEquipmentDefs = new List<EquipmentDef>();

        public override void Init()
        {
            RoR2Application.onLoad += PopulateEliteEquipmentDefList;
        }

        private void PopulateEliteEquipmentDefList()
        {
            var eliteDefsThatMatch = EliteCatalog.eliteDefs.Where(x => x.eliteEquipmentDef).ToList();

            foreach (EliteDef eliteDef in eliteDefsThatMatch)
            {
                EliteEquipmentDefs.Add(eliteDef.eliteEquipmentDef);
            }
        }

    }
}
