using HG;
using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Aetherium.Utils.Components
{
    public class ProjectileFixedImpactExplosion : MonoBehaviour, IProjectileImpactBehavior
    {
        public ProjectileController projectileController;

        public ProjectileDamage projectileDamage;

        public bool alive = true;

        [ShowFieldObsolete]
        [Obsolete("This sound will not play over the network. Provide the sound via the prefab referenced by explosionEffect instead.", false)]
        [Tooltip("This sound will not play over the network. Provide the sound via the prefab referenced by explosionEffect instead.")]
        public string explosionSoundString;

        public BlastAttack.FalloffModel falloffModel = BlastAttack.FalloffModel.Linear;

        public float blastRadius;

        [Tooltip("The percentage of the damage, proc coefficient, and force of the initial projectile. Ranges from 0-1")]
        public float blastDamageCoefficient;

        public float blastProcCoefficient = 1f;

        public AttackerFiltering blastAttackerFiltering;

        public Vector3 bonusBlastForce;

        [Tooltip("Does this projectile release children on death?")]
        public bool fireChildren;

        [Tooltip("Is will this be firing a bullet attack instead of a projectile attack?")]
        public bool ChildBulletAttack;

        public GameObject childrenProjectilePrefab;

        public GameObject childTracerPrefab;

        public GameObject childHitEffectPrefab;

        public float childBulletAttackProcCoefficient;

        public BulletAttack.HitCallback childBulletAttackHitCallback = null;

        public int childrenCount;

        [Tooltip("What percentage of our damage does the children get?")]
        public float childrenDamageCoefficient;

        public float MinDeviationAngle;
        public float MaxDeviationAngle;
        public Vector3 Direction;

        public Vector3 minAngleOffset;

        public Vector3 maxAngleOffset;

        [Tooltip("useLocalSpaceForChildren is unused by ProjectileImpactExplosion")]
        public bool useLocalSpaceForChildren;

        public HealthComponent projectileHealthComponent;

        public GameObject explosionEffect;

        public enum TransformSpace
        {
            World,
            Local,
            Normal
        }

        private Vector3 impactNormal = Vector3.up;

        public GameObject impactEffect;

        [ShowFieldObsolete]
        [Tooltip("This sound will not play over the network. Use lifetimeExpiredSound instead.")]
        [Obsolete("This sound will not play over the network. Use lifetimeExpiredSound instead.", false)]
        public string lifetimeExpiredSoundString;

        public NetworkSoundEventDef lifetimeExpiredSound;

        public float offsetForLifetimeExpiredSound;

        public bool destroyOnEnemy = true;

        public bool destroyOnWorld;

        public bool timerAfterImpact;

        public float lifetime;

        public float lifetimeAfterImpact;

        public float lifetimeRandomOffset;

        private float stopwatch;

        private float stopwatchAfterImpact;

        private bool hasImpact;

        private bool hasPlayedLifetimeExpiredSound;

        public TransformSpace transformSpace;

        public Matrix4x4 LookAt(Vector3 dir, Vector3 up, Vector3 right)
        {
            if (Mathf.Abs(Vector3.Dot(dir, up) / (dir.magnitude * up.magnitude)) > 1 - 0.000001)
            {
                up = right;
            }
            return Matrix4x4.LookAt(Vector3.zero, dir, up);
        }

        public void Awake()
        {
            projectileController = GetComponent<ProjectileController>();
            projectileDamage = GetComponent<ProjectileDamage>();
            lifetime += UnityEngine.Random.Range(0f, lifetimeRandomOffset);
        }

        public void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if (!NetworkServer.active && !projectileController.isPrediction)
            {
                return;
            }
            if (timerAfterImpact && hasImpact)
            {
                stopwatchAfterImpact += Time.fixedDeltaTime;
            }
            bool num = stopwatch >= lifetime;
            bool flag = timerAfterImpact && stopwatchAfterImpact > lifetimeAfterImpact;
            bool flag2 = projectileHealthComponent && !projectileHealthComponent.alive;
            if (num || flag || flag2)
            {
                alive = false;
            }
            if (alive && !hasPlayedLifetimeExpiredSound)
            {
                bool flag3 = stopwatch > lifetime - offsetForLifetimeExpiredSound;
                if (timerAfterImpact)
                {
                    flag3 |= stopwatchAfterImpact > lifetimeAfterImpact - offsetForLifetimeExpiredSound;
                }
                if (flag3)
                {
                    hasPlayedLifetimeExpiredSound = true;
                    if (NetworkServer.active && lifetimeExpiredSound)
                    {
                        PointSoundManager.EmitSoundServer(lifetimeExpiredSound.index, base.transform.position);
                    }
                }
            }
            if (!alive)
            {
                explosionEffect = impactEffect ?? explosionEffect;
                Detonate();
            }
        }

        public Vector3 GetRandomDirectionForChild()
        {
            var matrixLocal = Matrix4x4.Rotate(Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.forward)) * Matrix4x4.Rotate(Quaternion.AngleAxis(UnityEngine.Random.Range(MinDeviationAngle, MaxDeviationAngle), Vector3.up));

            Matrix4x4 matrixDirection = new Matrix4x4();

            switch (transformSpace)
            {
                case (TransformSpace.World):
                    matrixDirection = LookAt(Direction, Vector3.up, Vector3.right) * matrixLocal;
                    break;

                case (TransformSpace.Normal):
                    matrixDirection = LookAt(impactNormal, Vector3.up, Vector3.right) * LookAt(Direction, Vector3.up, Vector3.right) * matrixLocal;
                    break;

                case (TransformSpace.Local):
                    matrixDirection = LookAt(base.transform.forward, Vector3.up, Vector3.right) * LookAt(Direction, Vector3.up, Vector3.right) * matrixLocal;
                    break;
            }
            return matrixDirection.MultiplyPoint(Vector3.forward);
        }

        public void Detonate()
        {
            if (NetworkServer.active)
            {
                DetonateServer();
            }
            UnityEngine.Object.Destroy(base.gameObject);
        }

        protected void DetonateServer()
        {
            if (explosionEffect)
            {
                EffectManager.SpawnEffect(explosionEffect, new EffectData
                {
                    origin = base.transform.position,
                    scale = blastRadius
                }, transmit: true);
            }
            if (projectileDamage)
            {
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.position = base.transform.position;
                blastAttack.baseDamage = projectileDamage.damage * blastDamageCoefficient;
                blastAttack.baseForce = projectileDamage.force * blastDamageCoefficient;
                blastAttack.radius = blastRadius;
                blastAttack.attacker = (projectileController.owner ? projectileController.owner.gameObject : null);
                blastAttack.inflictor = base.gameObject;
                blastAttack.teamIndex = projectileController.teamFilter.teamIndex;
                blastAttack.crit = projectileDamage.crit;
                blastAttack.procChainMask = projectileController.procChainMask;
                blastAttack.procCoefficient = projectileController.procCoefficient * blastProcCoefficient;
                blastAttack.bonusForce = bonusBlastForce;
                blastAttack.falloffModel = falloffModel;
                blastAttack.damageColorIndex = projectileDamage.damageColorIndex;
                blastAttack.damageType = projectileDamage.damageType;
                blastAttack.attackerFiltering = blastAttackerFiltering;
                blastAttack.Fire();
            }
            if (explosionSoundString.Length > 0)
            {
                Util.PlaySound(explosionSoundString, base.gameObject);
            }
            if (fireChildren)
            {
                for (int i = 0; i < childrenCount; i++)
                {
                    FireChild();
                }
            }
        }

        protected void FireChild()
        {
            Vector3 randomDirectionForChild = GetRandomDirectionForChild();

            if (ChildBulletAttack && projectileController && projectileDamage)
            {
                BulletAttack childBulletAttack = new BulletAttack()
                {
                    owner = projectileController.owner,
                    weapon = gameObject,
                    origin = base.transform.position,
                    aimVector = randomDirectionForChild,
                    minSpread = 0,
                    maxSpread = 0,
                    isCrit = projectileDamage.crit,
                    procCoefficient = childBulletAttackProcCoefficient,
                    procChainMask = projectileController.procChainMask,
                    damage = projectileDamage.damage,
                    tracerEffectPrefab = childTracerPrefab,
                    hitEffectPrefab = childHitEffectPrefab,
                    smartCollision = true                    
                    
                };

                if(childBulletAttackHitCallback != null)
                {
                    childBulletAttack.hitCallback = childBulletAttackHitCallback;
                }

                childBulletAttack.Fire();
            }
            else
            {
                GameObject obj = UnityEngine.Object.Instantiate(childrenProjectilePrefab, base.transform.position, Util.QuaternionSafeLookRotation(randomDirectionForChild));
                ProjectileController component = obj.GetComponent<ProjectileController>();
                if (component)
                {
                    component.procChainMask = projectileController.procChainMask;
                    component.procCoefficient = projectileController.procCoefficient;
                    component.Networkowner = projectileController.owner;
                }
                obj.GetComponent<TeamFilter>().teamIndex = GetComponent<TeamFilter>().teamIndex;
                ProjectileDamage component2 = obj.GetComponent<ProjectileDamage>();
                if (component2)
                {
                    component2.damage = projectileDamage.damage * childrenDamageCoefficient;
                    component2.crit = projectileDamage.crit;
                    component2.force = projectileDamage.force;
                    component2.damageColorIndex = projectileDamage.damageColorIndex;
                }
                NetworkServer.Spawn(obj);
            }
        }

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            if (!alive)
            {
                return;
            }
            Collider collider = impactInfo.collider;
            impactNormal = impactInfo.estimatedImpactNormal;
            if (!collider)
            {
                return;
            }
            DamageInfo damageInfo = new DamageInfo();
            if (projectileDamage)
            {
                damageInfo.damage = projectileDamage.damage;
                damageInfo.crit = projectileDamage.crit;
                damageInfo.attacker = (projectileController.owner ? projectileController.owner.gameObject : null);
                damageInfo.inflictor = base.gameObject;
                damageInfo.position = impactInfo.estimatedPointOfImpact;
                damageInfo.force = projectileDamage.force * base.transform.forward;
                damageInfo.procChainMask = projectileController.procChainMask;
                damageInfo.procCoefficient = projectileController.procCoefficient;
            }
            else
            {
                Debug.Log("No projectile damage component!");
            }
            HurtBox component = collider.GetComponent<HurtBox>();
            if (component)
            {
                if (destroyOnEnemy)
                {
                    HealthComponent healthComponent = component.healthComponent;
                    if (healthComponent)
                    {
                        if (healthComponent.gameObject == projectileController.owner || (projectileHealthComponent && healthComponent == projectileHealthComponent))
                        {
                            return;
                        }
                        alive = false;
                    }
                }
            }
            else if (destroyOnWorld)
            {
                alive = false;
            }
            hasImpact = true;
            if (NetworkServer.active)
            {
                GlobalEventManager.instance.OnHitAll(damageInfo, collider.gameObject);
            }
        }

        public void SetExplosionRadius(float newRadius)
        {
            blastRadius = newRadius;
        }

        public void SetAlive(bool newAlive)
        {
            alive = newAlive;
        }

        public void OnValidate()
        {
            if (!Application.IsPlaying(this))
            {
                if (!string.IsNullOrEmpty(explosionSoundString))
                {
                    Debug.LogWarningFormat(base.gameObject, "{0} ProjectileImpactExplosion component supplies a value in the explosionSoundString field. This will not play correctly over the network. Please move the sound to the explosion effect.", Util.GetGameObjectHierarchyName(base.gameObject));
                }
                if (!string.IsNullOrEmpty(lifetimeExpiredSoundString))
                {
                    Debug.LogWarningFormat(base.gameObject, "{0} ProjectileImpactExplosion component supplies a value in the lifetimeExpiredSoundString field. This will not play correctly over the network. Please use lifetimeExpiredSound instead.", Util.GetGameObjectHierarchyName(base.gameObject));
                }
            }
        }
    }
}