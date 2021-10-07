using System.Collections;
using System.Collections.Generic;
using Aetherium.Equipment;
using UnityEngine;
using RoR2;
using RoR2.Audio;
using UnityEngine.Networking;

public class BellRingingEffect : NetworkBehaviour
{
    public CharacterBody Attacker;
    public float Radius;
	public ParticleSystem BellParticleSystem;
	
    public void Start()
    {
        BellParticleSystem = GetComponentInChildren<ParticleSystem>();
    }
	
	public void RingBell()
	{
		BellParticleSystem.Play();
        if (NetworkServer.active)
        {
            var bellEffectArea = gameObject.transform.Find("EffectArea").gameObject;
            EntitySoundManager.EmitSoundServer(BellTotem.BellRingingSound.akId, gameObject);
            var effectData = new EffectData()
            {
                origin = bellEffectArea.transform.position,
                rotation = bellEffectArea.transform.rotation,
                scale = BellTotem.RadiusOfBellRinging
            };
            EffectManager.SpawnEffect(BellTotem.BellSoundwaveEffect, effectData, true);

            new RoR2.BlastAttack()
            {
                attacker = Attacker.gameObject,
                position = bellEffectArea.transform.position,
                damageType = RoR2.DamageType.Stun1s,
                baseForce = BellTotem.ForceOfBellRinging,
                radius = BellTotem.RadiusOfBellRinging,
                inflictor = bellEffectArea,
                teamIndex = Attacker.teamComponent.teamIndex
            }.Fire();
        }
	}
}
