using EntityStates.NullifierMonster;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Aetherium.Items
{
    internal class VoidheartDeath : NetworkBehaviour
    {
        public Transform muzzleTransform;
        public Vector3? portalPosition;
        public GameObject deathPreExplosionInstance;
        public float voidExplosionRadius;
        public float voidHeartCooldownDuration;
        public float voidHeartImplosionDamageMultiplier;
        public float fixedAge;
        public bool hasFiredVoidPortal;
        public bool voidPortalKilledSomething;
        public CharacterMaster toReviveMaster;
        public CharacterBody toReviveBody;
        public BuffIndex voidInstabilityDebuff;

        public void Init()
        {
            this.muzzleTransform = base.transform;
            if (this.muzzleTransform)
            {
                this.portalPosition = new Vector3?(this.muzzleTransform.position);
                if (DeathState.deathPreExplosionVFX)
                {
                    this.deathPreExplosionInstance = UnityEngine.Object.Instantiate<GameObject>(DeathState.deathPreExplosionVFX, this.muzzleTransform.position, this.muzzleTransform.rotation);
                    this.deathPreExplosionInstance.transform.parent = this.muzzleTransform;
                    this.deathPreExplosionInstance.transform.localScale = Vector3.one * voidExplosionRadius;
                    ScaleParticleSystemDuration component = this.deathPreExplosionInstance.GetComponent<ScaleParticleSystemDuration>();
                    if (component)
                    {
                        component.newDuration = DeathState.duration;
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            if (this.muzzleTransform)
            {
                this.portalPosition = new Vector3?(this.muzzleTransform.position);
            }
            this.fixedAge += Time.fixedDeltaTime;
            if (this.fixedAge >= DeathState.duration)
            {
                if (!this.hasFiredVoidPortal)
                {
                    this.hasFiredVoidPortal = true;
                    this.FireVoidPortal();
                    if (NetworkServer.active)
                    {
                        if (voidPortalKilledSomething)
                        {
                            toReviveMaster.Invoke("RespawnExtraLife", 2f);
                            toReviveMaster.Invoke("PlayExtraLifeSFX", 1f);
                            toReviveMaster.preventRespawnUntilNextStageServer = false;
                        }
                        else
                        {
                            toReviveMaster.preventGameOver = false;
                            if (toReviveMaster.destroyOnBodyDeath)
                            {
                                UnityEngine.Object.Destroy(toReviveMaster, 1);
                            }
                        }
                    }

                    //Revive here if needed
                    return;
                }
                if (NetworkServer.active && this.fixedAge >= DeathState.duration + 4f)
                {
                    if (voidPortalKilledSomething)
                    {
                        var bodyRevived = toReviveMaster.GetBody();
                        bodyRevived.AddTimedBuffAuthority(voidInstabilityDebuff, voidHeartCooldownDuration);
                    }
                    UnityEngine.Object.Destroy(base.gameObject);
                }
            }
        }

        private void FireVoidPortal()
        {
            if (NetworkServer.active && portalPosition != null)
            {
                Collider[] array = Physics.OverlapSphere(portalPosition.Value, voidExplosionRadius, LayerIndex.entityPrecise.mask);
                CharacterBody[] array2 = new CharacterBody[array.Length];
                int count = 0;
                for (int i = 0; i < array.Length; i++)
                {
                    CharacterBody characterBody = Util.HurtBoxColliderToBody(array[i]);
                    if (characterBody && Array.IndexOf<CharacterBody>(array2, characterBody, 0, count) == -1)
                    {
                        array2[count++] = characterBody;
                    }
                }
                foreach (CharacterBody characterBody2 in array2)
                {
                    if (characterBody2 && characterBody2.healthComponent)
                    {
                        DamageInfo damageInfo = new DamageInfo();
                        if (toReviveBody) { damageInfo.attacker = toReviveBody.gameObject; }
                        damageInfo.crit = false;
                        damageInfo.damage = toReviveBody.damage * voidHeartImplosionDamageMultiplier;
                        damageInfo.force = Vector3.zero;
                        damageInfo.inflictor = base.gameObject;
                        damageInfo.position = characterBody2.corePosition;
                        damageInfo.procCoefficient = 1f;
                        damageInfo.damageColorIndex = DamageColorIndex.Default;
                        damageInfo.damageType = DamageType.VoidDeath | DamageType.BypassOneShotProtection | DamageType.BypassArmor;
                        //var healthBefore = characterBody.healthComponent.health; //debug
                        characterBody2.healthComponent.TakeDamage(damageInfo);
                        if (!characterBody2.healthComponent.alive && characterBody2.healthComponent.killingDamageType.HasFlag(DamageType.VoidDeath)) { voidPortalKilledSomething = true; }

                        //characterBody2.healthComponent.Suicide(base.gameObject, base.gameObject, DamageType.VoidDeath);
                    }
                }
                if (DeathState.deathExplosionEffect)
                {
                    EffectManager.SpawnEffect(DeathState.deathExplosionEffect, new EffectData
                    {
                        origin = base.transform.position,
                        scale = voidExplosionRadius
                    }, true);
                }
            }
        }
    }
}