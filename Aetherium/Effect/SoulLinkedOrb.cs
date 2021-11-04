using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace Aetherium.Effect
{
    public class SoulLinkedOrb : Orb
    {
        public GameObject Target;
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
                    genericFloat = base.duration,
                    rootObject = Target
                };
                //effectData.SetChildLocatorTransformReference(Target, Index);
                EffectManager.SpawnEffect(StandaloneBuffs.SoulLinked.SoulLinkedOrbEffect, effectData, true);
            }
        }
    }
}
