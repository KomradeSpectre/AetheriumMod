using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using UnityEngine;
using UnityEngine.Networking;

namespace Aetherium.MyEntityStates.BellTotem
{
    public class BellTotemDisappearState : EntityState
    {
        public float Duration = 0.45f;

        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Base", "Disappear", "Disappear.SpeedMultiplier", Duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge >= Duration)
            {
                if (NetworkServer.active)
                {
                    NetworkServer.UnSpawn(gameObject);
                }
                Destroy(gameObject);
            }
        }
    }
}
