using Aetherium.OrbVisuals;
using EntityStates;
using KomradeSpectre.Aetherium;
using R2API;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TILER2;
using UnityEngine;
using UnityEngine.Networking;

namespace Aetherium.Equipment
{
    public class JarOfReshaping : Equipment<JarOfReshaping>
    {
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("What radius should the jar devour bullets? (Default: 20m)", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public static float baseRadiusGranted { get; private set; } = 20f;

        public override string displayName => "Jar of Reshaping";

        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangDesc(string langID = null) => "";

        protected override string NewLangLore(string langID = null) => "";

        protected override string NewLangPickup(string langID = null) => "";

        public static GameObject ItemBodyModelPrefab;

        public static GameObject JarProjectile;

        public static GameObject JarOrb;

        public JarOfReshaping()
        {
            modelPathName = "@Aetherium:Assets/Models/Prefabs/JarOfReshaping.prefab";
            iconPathName = "@Aetherium:Assets/Textures/Icons/JarOfReshapingIcon.png";
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = AetheriumPlugin.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.4f, 0.4f, 0.4f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(1f, 1f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)

                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.75f, 0.5f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(4.75f, 4.75f, -2),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.75f, 0.5f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.75f, 0.5f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.75f, 0.5f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(2f, 1.25f, -0.75f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.25f, 0.25f, 0.25f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.75f, 0.5f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-4.75f, 4.75f, -2),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.75f, 0.5f, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            return rules;
        }

        protected override void LoadBehavior()
        {
            if(ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = regDef.pickupModelPrefab;
                regEqp.ItemDisplayRules = GenerateItemDisplayRules();
            }
            //JarOrbProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>())

            JarOrb = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/JarOfReshapingOrb.prefab");
            /*Debug.Log("ORB FOUND!");
            var effectComponent = JarOrb.AddComponent<EffectComponent>();
            Debug.Log("EFFECT FOUND!");

            var vfxAttributes = JarOrb.AddComponent<VFXAttributes>();
            vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
            vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Medium;
            Debug.Log("VFX FOUND!");

            var orbEffect = JarOrb.AddComponent<OrbEffect>();
            orbEffect.startVelocity1 = new Vector3(-10, 10, -10);
            orbEffect.startVelocity2 = new Vector3(10, 13, 10);
            orbEffect.endVelocity1 = new Vector3(-10, 0, -10);
            orbEffect.endVelocity2 = new Vector3(10, 5, 10);
            orbEffect.movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            Debug.Log("ORBEFFECT FOUND!");

            JarOrb.AddComponent<NetworkIdentity>();

            if (JarOrb) PrefabAPI.RegisterNetworkPrefab(JarOrb);
            EffectAPI.AddEffect(JarOrb);
            Debug.Log("REGISTERED!");

            OrbAPI.AddOrb(typeof(JarOfReshapingOrb));
            Debug.Log("ADDED ORB! ");*/

            JarProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/PaladinRocket"), "JarOfReshapingProjectile", true);

            var model = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/JarOfReshapingProjectile.prefab");
            model.AddComponent<NetworkIdentity>();
            model.AddComponent<RoR2.Projectile.ProjectileGhostController>();

            var controller = JarProjectile.GetComponent<RoR2.Projectile.ProjectileController>();
            controller.procCoefficient = 0.8f;
            controller.ghostPrefab = model;

            JarProjectile.GetComponent<RoR2.TeamFilter>().teamIndex = TeamIndex.Neutral;

            var damage = JarProjectile.GetComponent<RoR2.Projectile.ProjectileDamage>();
            damage.damageType = DamageType.Generic;
            damage.damage = 0;

            var impactExplosion = JarProjectile.GetComponent<RoR2.Projectile.ProjectileImpactExplosion>();
            impactExplosion.destroyOnEnemy = true;
            impactExplosion.destroyOnWorld = true;
            impactExplosion.impactEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/MeteorStrikeImpact");
            impactExplosion.blastRadius = 3;
            impactExplosion.blastProcCoefficient = 0.2f;

            // register it for networking
            if (JarProjectile) PrefabAPI.RegisterNetworkPrefab(JarProjectile);

            // add it to the projectile catalog or it won't work in multiplayer
            RoR2.ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(JarProjectile);
            };


        }

        protected override void UnloadBehavior()
        {
        }

        protected override bool OnEquipUseInner(EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.teamComponent) return false;
            var body = slot.characterBody;
            var bulletTracker = body.GetComponent<JarBulletTracker>();
            if (!bulletTracker) 
            {
                bulletTracker = body.gameObject.AddComponent<JarBulletTracker>();
                bulletTracker.body = body;
            }
            /*var equipmentDisplayTransform = slot.FindActiveEquipmentDisplay();
            if (equipmentDisplayTransform) 
            { 
                var coreChildLocator = equipmentDisplayTransform.GetComponentInChildren<ChildLocator>();
                if (coreChildLocator)
                {
                    Transform targetTransform = coreChildLocator.FindChild("SuctionTarget");
                    if (targetTransform)
                    {
                        bulletTracker.TargetTransform = targetTransform;
                    }
                }
            }*/

            if (bulletTracker.jarBullets.Count > 0 && bulletTracker.ChargeTime <= 0 && bulletTracker.SuckTime <= 0)
            {
                bulletTracker.ChargeTime = 1;
                bulletTracker.RefireTime = 0.2f;
                return true;
            }
            else if (bulletTracker.jarBullets.Count <= 0 && bulletTracker.SuckTime <= 0)
            {
                bulletTracker.SuckTime = 3;
            }
            return false;
        }

        public class JarBullet
        {
            public float Damage;
            public DamageColorIndex DamageColorIndex;
            public DamageType DamageType;

            public JarBullet(float damage, DamageColorIndex damageColorIndex, DamageType damageType)
            {
                Damage = damage;
                DamageColorIndex = damageColorIndex;
                DamageType = damageType;
            }

        }

        public class JarBulletTracker : MonoBehaviour
        {
            public List<JarBullet> jarBullets = new List<JarBullet>();
            public CharacterBody body;
            public Transform TargetTransform;
            public float SuckTime;
            public float ChargeTime;
            public float RefireTime;

            public void FixedUpdate()
            {
                if (SuckTime > 0)
                {
                    SuckTime -= Time.fixedDeltaTime;
                    List<ProjectileController> bullets = new List<ProjectileController>();
                    new RoR2.SphereSearch
                    {
                        radius = baseRadiusGranted,
                        mask = RoR2.LayerIndex.projectile.mask,
                        origin = body.corePosition
                    }.RefreshCandidates().FilterCandidatesByProjectileControllers().GetProjectileControllers(bullets);
                    if (bullets.Count > 0)
                    {
                        foreach (ProjectileController controller in bullets)
                        {
                            var controllerOwner = controller.owner;
                            if (controllerOwner)
                            {
                                var ownerBody = controllerOwner.GetComponent<CharacterBody>();
                                if (ownerBody)
                                {
                                    if (ownerBody.teamComponent.teamIndex == body.teamComponent.teamIndex)
                                    {
                                        continue;
                                    }
                                    var projectileDamage = controller.gameObject.GetComponent<ProjectileDamage>();
                                    if (projectileDamage)
                                    {
                                        jarBullets.Add(new JarBullet(projectileDamage.damage, projectileDamage.damageColorIndex, projectileDamage.damageType));
                                        var orb = new JarOfReshapingOrb();
                                        orb.Target = TargetTransform ? TargetTransform.gameObject : body.gameObject;
                                        orb.origin = controller.transform.position;
                                        orb.Index = TargetTransform ? -1 : 0;

                                        /*var orb = new HealOrb();
                                        orb.target = body.mainHurtBox;
                                        orb.origin = controller.transform.position;
                                        orb.healValue = 0;
                                        orb.overrideDuration = 0.3f;*/
                                        OrbManager.instance.AddOrb(orb);
                                        EntityState.Destroy(controller.gameObject);
                                    }
                                }
                            }
                        }
                        Util.PlayScaledSound(EntityStates.ClayBoss.PrepTarBall.prepTarBallSoundString, body.gameObject, 1);
                    }
                }
                if (ChargeTime > 0)
                {
                    ChargeTime -= Time.fixedDeltaTime;
                    if (ChargeTime <= 0)
                    { 
                        var bullet = jarBullets.Last();
                        var inputBank = body.inputBank;
                        FireProjectileInfo projectileInfo = new FireProjectileInfo();
                        projectileInfo.projectilePrefab = JarProjectile;
                        projectileInfo.damage = bullet.Damage;
                        projectileInfo.damageColorIndex = bullet.DamageColorIndex;
                        projectileInfo.damageTypeOverride = bullet.DamageType;
                        projectileInfo.owner = body.gameObject;
                        projectileInfo.procChainMask = default(ProcChainMask);
                        projectileInfo.position = body.corePosition;
                        projectileInfo.rotation = Util.QuaternionSafeLookRotation(inputBank ? inputBank.aimDirection : body.transform.forward);
                        projectileInfo.speedOverride = 120;
                        ProjectileManager.instance.FireProjectile(projectileInfo);
                        Util.PlaySound(EntityStates.ClayBoss.FireTarball.attackSoundString, body.gameObject);
                        jarBullets.RemoveAt(jarBullets.Count - 1);
                        if(jarBullets.Count > 0)
                        {
                            ChargeTime += RefireTime;
                        }
                    }
                }
            }
        }
    }
}
