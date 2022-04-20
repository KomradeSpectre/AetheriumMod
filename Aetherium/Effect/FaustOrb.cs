using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Text;
using static Aetherium.Equipment.Faust;

namespace Aetherium.Effect
{
    public class FaustOrb : GenericDamageOrb
    {
        public override void Begin()
        {
            duration = distanceToTarget / speed;
            if (Equipment.Faust.OrbFaust)
            {
                EffectData effectData = new EffectData
                {
                    scale = scale,
                    origin = origin,
                    genericFloat = duration
                };
                effectData.SetHurtBoxReference(target);
                EffectManager.SpawnEffect(Equipment.Faust.OrbFaust, effectData, true);
            }
        }

        public override void OnArrival()
        {
            base.OnArrival();
            if (this.target)
            {
                HealthComponent healthComponent = this.target.healthComponent;
                if (healthComponent && healthComponent.body)
                {
                    var faust = healthComponent.body.gameObject.AddComponent<FaustComponent>();
                    faust.attacker = attacker;

                    SetStateOnHurt setStateOnHurt = healthComponent.GetComponent<SetStateOnHurt>();
                    if (setStateOnHurt)
                    {
                        setStateOnHurt.SetStun(-1f);
                    }
                }
            }
        }
    }
}
