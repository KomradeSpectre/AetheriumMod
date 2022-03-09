using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using Aetherium.Equipment;
using static Aetherium.AetheriumPlugin;
using UnityEngine.Networking;
using RoR2;
using UnityEngine;

namespace Aetherium.MyEntityStates.BellTotem
{
    public class BellTotemRingingState : EntityState
    {
        public float Radius = 25;
        public float Duration = 2;
        public Equipment.BellTotem.BellTotemManager BellTotemManager;
        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Base", "Ringing", "Ringing.SpeedMultiplier", Duration);

            if (NetworkServer.active)
            {
                CreateEffect();
                CreateBlastAttack();
            }
        }

        public void CreateEffect()
        {
            EffectManager.SimpleEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/ShrineUseEffect"), gameObject.transform.position, gameObject.transform.rotation, true);
        }

        public void CreateBlastAttack()
        {
            BellTotemManager = gameObject.GetComponent<Equipment.BellTotem.BellTotemManager>();
            if (BellTotemManager && BellTotemManager.LastActivator)
            {
                var bellRinging = gameObject.GetComponent<BellRingingEffect>();
                if (bellRinging)
                {
                    bellRinging.Attacker = BellTotemManager.LastActivator;
                    bellRinging.Radius = Radius;
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge >= Duration)
            {
                BellTotemManager.Stopwatch = 0;
                outer.SetNextStateToMain();
            }
        }
    }
}
