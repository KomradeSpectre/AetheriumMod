using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using RoR2;
using UnityEngine;

namespace Aetherium.MyEntityStates.BellTotem
{
    public class BellTotemAppearState : EntityState
    {
        public float Duration = 0.4f;

        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Base", "Appear", "Appear.SpeedMultiplier", Duration);
            var effectData = new EffectData()
            {
                origin = gameObject.transform.Find("Base").position,
                rotation = gameObject.transform.rotation,
            };
            EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/impactEffects/SpawnLemurian"), effectData, true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge >= Duration)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}
