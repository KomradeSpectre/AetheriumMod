using Aetherium.OrbVisuals;
using Aetherium.Utils;
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
    public class JarOfReshaping : Equipment_V2<JarOfReshaping>
    {
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("What radius should the jar devour bullets? (Default: 20m)", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public static float baseRadiusGranted { get; private set; } = 20f;

        public override string displayName => "Jar of Reshaping";

        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetDescString(string langID = null) => "";

        protected override string GetLoreString(string langID = null) => "";

        protected override string GetPickupString(string langID = null) => "";


        public static GameObject ItemBodyModelPrefab;

        public static GameObject JarProjectile;

        public static GameObject JarOrb;

        public JarOfReshaping()
        {
            modelResourcePath = "@Aetherium:Assets/Models/Prefabs/JarOfReshaping.prefab";
            iconResourcePath = "@Aetherium:Assets/Textures/Icons/JarOfReshapingIcon.png";
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

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

        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(modelResourcePath);
                displayRules = GenerateItemDisplayRules();
            }
            base.SetupAttributes();
        }
        public override void Install()
        {
            base.Install();
            //JarOrbProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>())

            JarOrb = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/JarOfReshapingOrb.prefab");

            var effectComponent = JarOrb.AddComponent<RoR2.EffectComponent>();

            var vfxAttributes = JarOrb.AddComponent<RoR2.VFXAttributes>();
            vfxAttributes.vfxIntensity = RoR2.VFXAttributes.VFXIntensity.Low;
            vfxAttributes.vfxPriority = RoR2.VFXAttributes.VFXPriority.Medium;

            var orbEffect = JarOrb.AddComponent<OrbEffect>();

            orbEffect.startVelocity1 = new Vector3(-10, 10, -10);
            orbEffect.startVelocity2 = new Vector3(10, 13, 10);
            orbEffect.endVelocity1 = new Vector3(-10, 0, -10);
            orbEffect.endVelocity2 = new Vector3(10, 5, 10);
            orbEffect.movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            JarOrb.AddComponent<NetworkIdentity>();

            if (JarOrb) PrefabAPI.RegisterNetworkPrefab(JarOrb);
            EffectAPI.AddEffect(JarOrb);

            OrbAPI.AddOrb(typeof(JarOfReshapingOrb));

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
            impactExplosion.impactEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/LunarWispTrackingBombExplosion");
            impactExplosion.blastRadius = 2;
            impactExplosion.blastProcCoefficient = 0.2f;

            // register it for networking
            if (JarProjectile) PrefabAPI.RegisterNetworkPrefab(JarProjectile);

            // add it to the projectile catalog or it won't work in multiplayer
            RoR2.ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(JarProjectile);
            };

            On.RoR2.EquipmentSlot.FixedUpdate += EquipmentUpdate;
        }

        private void EquipmentUpdate(On.RoR2.EquipmentSlot.orig_FixedUpdate orig, RoR2.EquipmentSlot self)
        {
            if (self.equipmentIndex == equipmentDef.equipmentIndex)
            {
                var selfDisplay = self.FindActiveEquipmentDisplay();
                var body = self.characterBody;
                if (selfDisplay && body)
                {
                    var input = body.inputBank;
                    if (input)
                    {
                        selfDisplay.rotation = Util.QuaternionSafeLookRotation(input.aimDirection);
                    }
                }
            }
            orig(self);
        }

        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.EquipmentSlot.FixedUpdate -= EquipmentUpdate;
        }

        protected override bool PerformEquipmentAction(RoR2.EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.teamComponent) return false;
            var body = slot.characterBody;
            var bulletTracker = body.GetComponent<JarBulletTracker>();
            if (!bulletTracker) 
            {
                bulletTracker = body.gameObject.AddComponent<JarBulletTracker>();
                bulletTracker.body = body;
            }

            var equipmentDisplayTransform = slot.FindActiveEquipmentDisplay();
            if (equipmentDisplayTransform)
            {
                bulletTracker.TargetTransform = equipmentDisplayTransform;
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
            public RoR2.CharacterBody body;
            public Transform TargetTransform;
            public float SuckTime;
            public float ChargeTime;
            public float RefireTime;

            public void FixedUpdate()
            {
                var input = body.inputBank;
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
                                var ownerBody = controllerOwner.GetComponent<RoR2.CharacterBody>();
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
                                        //orb.Target = body.gameObject;
                                        orb.origin = controller.transform.position;
                                        orb.Index = TargetTransform ? -1 : 0;
                                        OrbManager.instance.AddOrb(orb);
                                        EntityState.Destroy(controller.gameObject);
                                    }
                                }
                            }
                        }
                        RoR2.Util.PlayScaledSound(EntityStates.ClayBoss.PrepTarBall.prepTarBallSoundString, body.gameObject, 1);
                    }
                }
                if (ChargeTime > 0)
                {
                    ChargeTime -= Time.fixedDeltaTime;
                    if (ChargeTime <= 0)
                    { 
                        var bullet = jarBullets.Last();
                        FireProjectileInfo projectileInfo = new FireProjectileInfo();
                        projectileInfo.projectilePrefab = JarProjectile;
                        projectileInfo.damage = bullet.Damage;
                        projectileInfo.damageColorIndex = bullet.DamageColorIndex;
                        projectileInfo.damageTypeOverride = bullet.DamageType;
                        projectileInfo.owner = body.gameObject;
                        projectileInfo.procChainMask = default(RoR2.ProcChainMask);
                        projectileInfo.position = TargetTransform ? TargetTransform.position : body.corePosition;
                        projectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(input ? input.aimDirection : body.transform.forward);
                        projectileInfo.speedOverride = 120;
                        ProjectileManager.instance.FireProjectile(projectileInfo);
                        RoR2.Util.PlaySound(EntityStates.ClayBoss.FireTarball.attackSoundString, body.gameObject);
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
