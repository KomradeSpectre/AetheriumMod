using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Text;
using static Aetherium.Equipment.EliteEquipment.AffixHungering;

namespace Aetherium.Effect
{
    internal class HungeringConsumptionOrb : GenericDamageOrb
    {
        public HungeringEliteController HungeringEliteController;
        public override void Begin()
        {
            base.Begin();
            EffectData effectData = new EffectData
            {
                scale = 1,
                origin = attacker.transform.position,
                genericFloat = base.duration,
                rootObject = target.gameObject
            };
            //effectData.SetChildLocatorTransformReference(Target, Index);
            EffectManager.SpawnEffect(HungeringOrbEffectPrefab, effectData, false);

            HungeringEliteController = attacker.GetComponent<HungeringEliteController>();
        }

        public override void OnArrival()
        {
            base.OnArrival();
            if (attacker && HungeringEliteController && target)
            {
                var healthComponent = target.healthComponent;
                if (healthComponent)
                {
                    var body = healthComponent.body;
                    if (body)
                    {
                        var master = body.master;
                        if (master)
                        {
                            master.TrueKill();
                            HungeringEliteController.Consumed++;
                        }
                    }
                }
            }
        }
    }
}
