using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;

public class BellTotemDisappearEffect : NetworkBehaviour
{		
	public void PlayImpactEffect()
	{
        if (NetworkServer.active)
        {
            var effectData = new EffectData()
            {
                origin = gameObject.transform.Find("Base").position,
                rotation = gameObject.transform.rotation,
            };
            EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/impactEffects/SpawnLemurian"), effectData, true);
        }
	}
}
