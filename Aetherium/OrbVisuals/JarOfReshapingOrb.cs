using Aetherium.Equipment;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.OrbVisuals
{
    public class JarOfReshapingOrb : Orb
    {
        public GameObject Target;
        public int Index;
        public float OverrideDuration = 0.3f;
        public override void Begin()
        {
            if (Target)
            {
                duration = OverrideDuration;
                EffectData effectData = new EffectData
                {
                    scale = 1,
                    origin = this.origin,
                    genericFloat = base.duration
                };

                effectData.SetChildLocatorTransformReference(Target, Index);
                EffectManager.SpawnEffect(JarOfReshaping.JarOrb, effectData, true);
            }
        }
    }
}
