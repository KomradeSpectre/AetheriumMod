using EntityStates.NullifierMonster;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using Aetherium.Utils.Components;
using System.Collections.Generic;

namespace Aetherium.Items.TierLunar
{
    /// <summary>
    /// The Heart of the Void implosion. The server instance deals the damage and handles the revive,
    /// every client (host included) spawns its own instance for the visuals and the death camera.
    /// </summary>
    public class VoidheartDeath : MonoBehaviour
    {
        public const float ReviveDelay = 2f;
        public const float LingerDuration = 4f;

        public Vector3 portalPosition;
        public GameObject deathPreExplosionInstance;
        public GameObject intersectionFieldInstance;

        public ObjectScaleCurve scaleCurve;

        public ShakeEmitter shakeEmitter;

        public Material fieldMaterial;
        public Material fieldIntersectionMaterial;
        public Material fieldIntersectionStayMaterial;

        public float voidExplosionRadius;
        public float voidHeartCooldownDuration;
        public float voidHeartImplosionDamageMultiplier;
        public float intendedDuration;
        public float fixedAge;
        public float shrinkDuration;

        public bool isServerInstance;
        public bool hasFiredVoidPortal;
        public bool voidPortalKilledSomething;
        public bool hasRevived;

        public CharacterMaster toReviveMaster;
        public CharacterBody toReviveBody;
        public float attackerDamage;

        public BuffDef voidInstabilityDebuff;

        public float ExplosionTime => intendedDuration + shrinkDuration;

        public void Init()
        {
            portalPosition = base.transform.position;

            if(toReviveBody)
            {
                attackerDamage = toReviveBody.damage;
            }

            SetupVisuals();
            SetupCamera();
        }

        private void SetupVisuals()
        {
            var voidPortalBomb = AetheriumPlugin.MainAssets.LoadAsset<GameObject>("VoidheartExplosionSphere");
            if(!voidPortalBomb) return;

            this.deathPreExplosionInstance = UnityEngine.Object.Instantiate(voidPortalBomb, portalPosition, base.transform.rotation);

            intersectionFieldInstance = this.deathPreExplosionInstance.transform.Find("mdlVoidheartExplosionSphere/VoidheartExplosionIntersectStay").gameObject;

            var animationCurve = Aetherium.Utils.Easings.EasingAnimationCurve.EaseToAnimationCurve(Utils.Easings.EasingFunctions.Ease.EaseOutBack);

            scaleCurve = deathPreExplosionInstance.AddComponent<ObjectScaleCurve>();
            scaleCurve.curveX = animationCurve;
            scaleCurve.curveY = animationCurve;
            scaleCurve.curveZ = animationCurve;
            scaleCurve.timeMax = intendedDuration * 0.7f;
            scaleCurve.baseScale = new Vector3(1, 1, 1) * voidExplosionRadius;

            var scaleParticleSystemDuration = deathPreExplosionInstance.AddComponent<ScaleParticleSystemDuration>();
            scaleParticleSystemDuration.particleSystems = deathPreExplosionInstance.GetComponentsInChildren<ParticleSystem>().Where(x => x.gameObject.name != "SuctionParticles").ToArray();
            scaleParticleSystemDuration.initialDuration = 5f;
            scaleParticleSystemDuration.newDuration = intendedDuration;

            var scaleForcefield = deathPreExplosionInstance.GetComponentInChildren<ParticleSystemForceField>();
            if(scaleForcefield)
            {
                scaleForcefield.endRange = deathPreExplosionInstance.transform.localScale.x * 1.25f;
            }

            var lightObject = deathPreExplosionInstance.transform.Find("mdlVoidheartExplosionSphere/Light").gameObject;
            lightObject.AddComponent<LightScaleFromParent>();

            var lightIntensityCurve = lightObject.AddComponent<LightIntensityCurve>();
            lightIntensityCurve.curve = animationCurve;
            lightIntensityCurve.light = lightObject.GetComponent<Light>();
            lightIntensityCurve.maxIntensity = 1000;
            lightIntensityCurve.timeMax = intendedDuration * 0.7f;

            shakeEmitter = intersectionFieldInstance.AddComponent<ShakeEmitter>();
            shakeEmitter.shakeOnStart = false;
            shakeEmitter.shakeOnEnable = false;
            shakeEmitter.radius = voidExplosionRadius * 4f;
            shakeEmitter.duration = 0.4f;
            shakeEmitter.wave = new Wave()
            {
                frequency = 6f,
                amplitude = 20
            };

            fieldMaterial = deathPreExplosionInstance.transform.Find("mdlVoidheartExplosionSphere").gameObject.GetComponent<Renderer>().material;
            fieldIntersectionMaterial = deathPreExplosionInstance.transform.Find("mdlVoidheartExplosionSphere/VoidheartExplosionIntersect").gameObject.GetComponent<Renderer>().material;
            fieldIntersectionStayMaterial = intersectionFieldInstance.GetComponent<Renderer>().material;
        }

        private void SetupCamera()
        {
            if(!toReviveMaster || Voidheart.VoidImplosionCameraSpinSpeed <= 0) return;

            foreach(var cameraRig in CameraRigController.readOnlyInstancesList)
            {
                if(cameraRig && !cameraRig.hasOverride && cameraRig.viewer && cameraRig.viewer.master == toReviveMaster)
                {
                    var deathCamera = base.gameObject.AddComponent<VoidheartDeathCamera>();
                    deathCamera.CameraRig = cameraRig;
                    deathCamera.Master = toReviveMaster;
                    deathCamera.DeathPoint = portalPosition;
                    deathCamera.OrbitRadius = Mathf.Max(15f, voidExplosionRadius * 1.6f);
                    deathCamera.AngularSpeed = Mathf.PI * Voidheart.VoidImplosionCameraSpinSpeed / Time.fixedDeltaTime;
                    deathCamera.MaxDuration = ExplosionTime + ReviveDelay + 1f;
                }
            }
        }

        public void FixedUpdate()
        {
            this.fixedAge += Time.fixedDeltaTime;

            if(!this.hasFiredVoidPortal)
            {
                if(this.fixedAge < intendedDuration) return;

                if(scaleCurve)
                {
                    UnityEngine.Object.Destroy(scaleCurve);
                    scaleCurve = null;
                    if(intersectionFieldInstance) intersectionFieldInstance.transform.parent = null;
                }

                var shrinkTime = this.fixedAge - intendedDuration;
                if(shrinkTime < shrinkDuration)
                {
                    if(deathPreExplosionInstance)
                    {
                        var shrinkSlide = Mathf.Clamp01(shrinkTime / shrinkDuration);
                        deathPreExplosionInstance.transform.localScale = EasingFunction.EaseInExpo(voidExplosionRadius, 0, shrinkSlide) * Vector3.one;
                        fieldMaterial.SetFloat("_Boost", EasingFunction.EaseInExpo(7, 20, shrinkSlide));
                        fieldIntersectionMaterial.SetFloat("_Boost", EasingFunction.EaseInExpo(7, 20, shrinkSlide));
                    }
                    return;
                }

                this.hasFiredVoidPortal = true;

                if(deathPreExplosionInstance) UnityEngine.Object.Destroy(deathPreExplosionInstance);
                if(shakeEmitter) shakeEmitter.StartShake();
                if(intersectionFieldInstance)
                {
                    var fadeTint = intersectionFieldInstance.AddComponent<FadeTintAndDestroy>();
                    fadeTint.ColorFadeDuration = 1;
                }

                if(isServerInstance && NetworkServer.active)
                {
                    this.FireVoidPortal();
                    OnImplosionResolvedServer();
                }
                return;
            }

            if(isServerInstance && NetworkServer.active && voidPortalKilledSomething && !hasRevived && this.fixedAge >= ExplosionTime + ReviveDelay)
            {
                hasRevived = true;
                ReviveServer();
            }

            if(this.fixedAge >= ExplosionTime + LingerDuration)
            {
                UnityEngine.Object.Destroy(base.gameObject);
            }
        }

        private void OnImplosionResolvedServer()
        {
            if(!toReviveMaster) return;

            if(voidPortalKilledSomething)
            {
                toReviveMaster.preventRespawnUntilNextStageServer = false;
                toReviveMaster.Invoke("PlayExtraLifeSFX", ReviveDelay * 0.5f);
            }
            else
            {
                toReviveMaster.preventGameOver = false;
                if(toReviveMaster.destroyOnBodyDeath)
                {
                    UnityEngine.Object.Destroy(toReviveMaster.gameObject, 1f);
                }
            }
        }

        private void ReviveServer()
        {
            if(!toReviveMaster) return;

            toReviveMaster.RespawnExtraLife();

            // Apply the cooldown the moment the new body exists, so it can't trigger again right after reviving.
            var bodyRevived = toReviveMaster.GetBody();
            if(bodyRevived && voidInstabilityDebuff)
            {
                bodyRevived.AddTimedBuff(voidInstabilityDebuff, voidHeartCooldownDuration);
            }
        }

        private void FireVoidPortal()
        {
            Collider[] array = Physics.OverlapSphere(portalPosition, voidExplosionRadius, LayerIndex.entityPrecise.mask);

            HashSet<CharacterBody> distinctBodies = new HashSet<CharacterBody>();

            foreach (var collider in array)
            {
                CharacterBody body = Util.HurtBoxColliderToBody(collider);
                if(body && body != toReviveBody) { distinctBodies.Add(body); }
            }

            foreach (CharacterBody characterBody2 in distinctBodies)
            {
                var healthComponent = characterBody2.healthComponent;
                // Intentionally ignores teams: the implosion hits allies too, and killing them counts toward the revive.
                if(!healthComponent || !healthComponent.alive) continue;

                DamageInfo damageInfo = new DamageInfo();
                if(toReviveBody) { damageInfo.attacker = toReviveBody.gameObject; }
                damageInfo.crit = false;
                damageInfo.damage = attackerDamage * voidHeartImplosionDamageMultiplier;
                damageInfo.force = Vector3.zero;
                damageInfo.inflictor = base.gameObject;
                damageInfo.position = characterBody2.corePosition;
                damageInfo.procCoefficient = 1f;
                damageInfo.damageColorIndex = DamageColorIndex.Default;
                damageInfo.damageType = DamageType.VoidDeath | DamageType.BypassOneShotProtection | DamageType.BypassArmor;
                healthComponent.TakeDamage(damageInfo);
                if(!healthComponent.alive && healthComponent.killingDamageType.damageType.HasFlag(DamageType.VoidDeath)) { voidPortalKilledSomething = true; }
            }

            if(DeathState.voidDeathEffect)
            {
                EffectManager.SpawnEffect(DeathState.voidDeathEffect, new EffectData
                {
                    origin = portalPosition,
                    scale = voidExplosionRadius
                }, true);
            }
        }
    }

    /// <summary>
    /// Client-side orbit camera around the implosion, applied only to the camera of the player who died.
    /// </summary>
    public class VoidheartDeathCamera : MonoBehaviour, ICameraStateProvider
    {
        public CameraRigController CameraRig;
        public CharacterMaster Master;
        public Vector3 DeathPoint;
        public float OrbitRadius;
        public float AngularSpeed;
        public float MaxDuration;

        private float age;
        private float startAngle;
        private bool released;

        public void Start()
        {
            if(!CameraRig)
            {
                Release();
                return;
            }

            var offset = CameraRig.transform.position - DeathPoint;
            startAngle = Mathf.Atan2(offset.x, offset.z);
            CameraRig.SetOverrideCam(this, 1f);
        }

        public void Update()
        {
            if(released) return;
            age += Time.deltaTime;

            var body = Master ? Master.GetBody() : null;
            bool revived = body && body.healthComponent && body.healthComponent.alive;

            if(!CameraRig || !Master || revived || age >= MaxDuration)
            {
                Release();
            }
        }

        public void OnDestroy()
        {
            Release();
        }

        private void Release()
        {
            if(released) return;
            released = true;

            if(CameraRig && CameraRig.IsOverrideCam(this))
            {
                CameraRig.SetOverrideCam(null, 1f);
            }
        }

        public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
        {
            float angle = startAngle + AngularSpeed * age;
            var direction = new Vector3(Mathf.Sin(angle), 0.5f, Mathf.Cos(angle)).normalized;
            float distance = OrbitRadius * 1.118f; // keeps the same height-to-radius ratio as the old camera (r, r/2)

            // Don't put the camera inside walls.
            if(Physics.SphereCast(DeathPoint, 0.5f, direction, out var hit, distance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                distance = Mathf.Max(hit.distance - 0.5f, 1f);
            }

            var position = DeathPoint + direction * distance;
            cameraState.position = position;
            cameraState.rotation = Util.QuaternionSafeLookRotation(DeathPoint - position);
            cameraState.fov = 90f;
        }

        public bool IsUserLookAllowed(CameraRigController cameraRigController) => false;

        public bool IsUserControlAllowed(CameraRigController cameraRigController) => false;

        public bool IsHudAllowed(CameraRigController cameraRigController) => false;
    }
}
