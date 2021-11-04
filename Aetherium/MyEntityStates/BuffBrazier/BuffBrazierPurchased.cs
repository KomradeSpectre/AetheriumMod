using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using RoR2;
using UnityEngine;

namespace Aetherium.MyEntityStates.BuffBrazier
{
    public class BuffBrazierPurchased : EntityState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            AkSoundEngine.PostEvent(3116942153, gameObject.transform.Find("Fire").gameObject);

            var fireParticleSystem = gameObject.transform.Find("Fire").gameObject.GetComponent<ParticleSystem>();
            fireParticleSystem.Stop();

            gameObject.transform.Find("Fire Icon/Fire Light").gameObject.SetActive(false);
            gameObject.transform.Find("Fire Icon").gameObject.SetActive(false);

            EffectManager.SimpleEffect(Resources.Load<GameObject>("prefabs/effects/ShrineUseEffect"), gameObject.transform.position, gameObject.transform.rotation, false);
        }
    }
}
