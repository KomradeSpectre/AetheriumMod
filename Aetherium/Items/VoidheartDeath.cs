using EntityStates.NullifierMonster;
using RoR2;
using System;
using EntityStates;
using UnityEngine;
using UnityEngine.Networking;

namespace Aetherium.Items
{
    public class VoidheartDeath : NetworkBehaviour
    {
        public Transform muzzleTransform;
        public Vector3? portalPosition;
        public GameObject deathPreExplosionInstance;
        public float voidExplosionRadius;
        public float voidHeartCooldownDuration;
        public float voidHeartImplosionDamageMultiplier;
        public float fixedAge;
        public float ExplosionHappenedAtThisTime;
        public bool hasFiredVoidPortal;
        public bool voidPortalKilledSomething;
        public CharacterMaster toReviveMaster;
        public CharacterBody toReviveBody;
        public BuffDef voidInstabilityDebuff;

        public void Init()
        {
            this.muzzleTransform = base.transform;
            if (this.muzzleTransform)
            {
                this.portalPosition = new Vector3?(this.muzzleTransform.position);
                if (DeathState.voidDeathEffect)
                {
                    this.deathPreExplosionInstance = UnityEngine.Object.Instantiate<GameObject>(DeathState.voidDeathEffect, this.muzzleTransform.position, this.muzzleTransform.rotation);
                    this.deathPreExplosionInstance.transform.parent = this.muzzleTransform;
                    this.deathPreExplosionInstance.transform.localScale = Vector3.one * voidExplosionRadius;
                    ScaleParticleSystemDuration component = this.deathPreExplosionInstance.GetComponent<ScaleParticleSystemDuration>();
                    if (component)
                    {
                        component.newDuration = DeathState.duration;
                    }
                    if (toReviveMaster)
                    {
                        var cameraComponent = toReviveMaster.GetComponent<VoidheartDeathCameraController>();
                        if (!cameraComponent)
                        {
                            cameraComponent = toReviveMaster.gameObject.AddComponent<VoidheartDeathCameraController>();
                            cameraComponent.Master = toReviveMaster;
                            cameraComponent.DeathPoint = portalPosition.Value;
                            cameraComponent.VoidheartDeath = this;
                        }
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
                        var inventory = toReviveMaster.inventory;
                        var bearCount = inventory.GetItemCount(RoR2Content.Items.ExtraLifeConsumed);
                        if(bearCount > 0) 
                        {
                            inventory.RemoveItem(RoR2Content.Items.ExtraLifeConsumed, 1);
                        }

                        var bodyRevived = toReviveMaster.GetBody();
                        bodyRevived.AddTimedBuffAuthority(voidInstabilityDebuff.buffIndex, voidHeartCooldownDuration);
                    }
                    UnityEngine.Object.Destroy(base.gameObject);
                }
            }
        }

        private void FireVoidPortal()
        {
            ExplosionHappenedAtThisTime = Time.time;

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
                if (DeathState.voidDeathEffect)
                {
                    EffectManager.SpawnEffect(DeathState.voidDeathEffect, new EffectData
                    {
                        origin = base.transform.position,
                        scale = voidExplosionRadius
                    }, true);
                }
            }
        }
    }

    public class VoidheartDeathCameraController : NetworkBehaviour
    {
        public CharacterMaster Master;
        public GameObject DeathObject;
        public Vector3 DeathPoint;
        public VoidheartDeath VoidheartDeath;

        [SyncVar]
        public bool DestroyThisNow = false;

        public CameraRigController Camera;
        public ForcedCamera ForcedCameraInstance;

        [SyncVar]
        public double Angle = 0;

        [SyncVar]
        public float Radius = 0;

        [SyncVar]
        public float Slide = 0;

        public void FixedUpdate()
        {
            if (!Master.preventGameOver || Master.currentLifeStopwatch > 0 || Slide >= 1 || DestroyThisNow)
            {
                UnityEngine.Object.Destroy(this);
            }

            var playerCharacterMasterController = Master.playerCharacterMasterController;

            if (playerCharacterMasterController && playerCharacterMasterController.networkUser && playerCharacterMasterController.networkUser.cameraRigController)
            {
                Camera = playerCharacterMasterController.networkUser.cameraRigController;

                if (Camera)
                {
                    if (!DeathObject)
                    {
                        DeathObject = new GameObject();
                        ForcedCameraInstance = DeathObject.AddComponent<ForcedCamera>();
                        ForcedCameraInstance.fovOverride = 100;
                        Camera.SetOverrideCam(ForcedCameraInstance, 0);
                    }

                    Angle += Math.PI * Voidheart.VoidImplosionCameraSpinSpeed;

                    if (VoidheartDeath.voidPortalKilledSomething)
                    {
                        Slide = Mathf.Clamp((Time.time - VoidheartDeath.ExplosionHappenedAtThisTime) / 2f, 0, 1);
                        Radius = EasingFunction.EaseInQuad(15, 1, Slide);
                    }
                    else
                    {
                        Radius = Mathf.Clamp(Radius + 0.1f, 0, 15);
                        if (Radius >= 15 && VoidheartDeath.fixedAge >= DeathState.duration + 4f)
                        {
                            DestroyThisNow = true;
                        }
                    }

                    DeathObject.transform.position = DeathPoint + new Vector3(Radius * (float)Math.Sin(Angle), Radius / 2, Radius * (float)Math.Cos(Angle));
                    DeathObject.transform.rotation = Util.QuaternionSafeLookRotation(DeathPoint - DeathObject.transform.position);
                }
            }
        }

        public void OnDestroy()
        {
            if (DeathObject)
            {
                UnityEngine.Object.Destroy(DeathObject);
            }
        }
    }
}