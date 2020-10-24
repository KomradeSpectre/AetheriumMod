using Aetherium.OrbVisuals;
using Aetherium.Utils;
using EntityStates;
using KomradeSpectre.Aetherium;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RewiredConsts;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
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

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("How long should the jar be in the projectile absorption state? (Default: 5 seconds)", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public static float projectileAbsorptionTime { get; private set; } = 3f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("How long should the jar's main cooldown be? (Default: 20 seconds)", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public static float jarCooldown { get; private set; } = 20f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("If artifact of chaos is on, should we be able to absorb projectiles from other players?", AutoConfigFlags.PreventNetMismatch, false, true)]
        public static bool iWantToLoseFriendsInChaosMode { get; private set; } = false;

        public override string displayName => "Jar of Reshaping";

        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => "On activation, <style=cIsUtility>suck nearby projectiles into the jar</style>. Upon success, the next activation will <style=cIsDamage>fire out all projectiles stored in the jar</style>.";

        protected override string GetDescString(string langID = null) => $"On activation, <style=cIsUtility>absorb projectiles</style> in a <style=cIsUtility>{baseRadiusGranted}m</style> radius for <style=cIsUtility>{projectileAbsorptionTime}</style> second(s). " +
            $"Upon success, <style=cIsDamage>fire all of the projectiles out of the jar</style> upon next activation. " +
            $"The damage traits of each projectiles fired from the jar depends on the <style=cIsDamage>bullets you absorbed</style>. " +
            $"After all the projectiles have been fired from the jar, it will need to cool down.";

        protected override string GetLoreString(string langID = null) => $"[INCIDENT NUMBER 421076]\n" +
            $"[VISUAL RECORDING RECOVERED FROM BLACK BOX ON DERELICT 'UES SAFETY FIRST' ENGINEERING VESSEL. TRANSCRIPT TO FOLLOW]\n" +
            $"\nTerry: Hey Phil, did you see what the boys over in Expeditions brought in?\n" +
            $"Terry: Looks like a pretty ordinary jar right?\n" +
            $"Phil: Yeah, just looks like something you'd find at a housewive's art deco yard sale.\n" +
            $"Terry: Yeah, I thought the same thing, but watch and learn.\n" +
            $"Terry: First, you just hit the bottom of the jar with your palm, and ---\n" +
            $"Phil: Woah! It started glowing and what on Terra is that noise? Is that an alien vaccum cleaner?\n" +
            $"Terry: Far better. Now watch.\n" +
            $"[Terry is seen throwing random objects into the jar and Phil joins him with bewilderment on his face.]\n" +
            $"Phil: That's amazing! Can it do anything else, or is it just a weird vaccum pot?\n" +
            $"Terry: Yeah, if I just squeeze the handle here, it'll empty it all out.\n" +
            $"[Terry is seen squeezing the handles of the jar, but his expression turns to horror a moment later.]\n" +
            $"Terry: Oh hell, oh god. Phil, I just realized that I may have thrown in a few mining grenades earlier when we were having fun with it earlier.\n" +
            $"Terry: Quickly! Get one of the suits on before it---\n" +
            $"[The jar activates, shooting its contents around the room. One of the projectiles hits the hull and explodes, ripping a hole through it moments before the feed is lost.]\n" +
            $"\n[END OF FILE] ";

        public static GameObject ItemBodyModelPrefab;

        public static GameObject JarProjectile;

        public static GameObject JarOrb;

        public static GameObject JarChargeSphere;

        public JarOfReshaping()
        {
            modelResourcePath = "@Aetherium:Assets/Models/Prefabs/JarOfReshaping.prefab";
            iconResourcePath = "@Aetherium:Assets/Textures/Icons/JarOfReshapingIcon.png";
        }

        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(modelResourcePath);
                displayRules = GenerateItemDisplayRules();
            }
            base.SetupAttributes();

            NetworkingAPI.RegisterMessageType<SyncJarOrb>();
            NetworkingAPI.RegisterMessageType<SyncJarSucking>();
            NetworkingAPI.RegisterMessageType<SyncJarCharging>();

            JarChargeSphere = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/JarOfReshapingAbsorbEffect.prefab");

            var chargeSphereEffectComponent = JarChargeSphere.AddComponent<RoR2.EffectComponent>();
            chargeSphereEffectComponent.parentToReferencedTransform = true;
            chargeSphereEffectComponent.positionAtReferencedTransform = true;

            var chargeSphereTimer = JarChargeSphere.AddComponent<RoR2.DestroyOnTimer>();
            chargeSphereTimer.duration = projectileAbsorptionTime;

            var chargeSphereVfxAttributes = JarChargeSphere.AddComponent<RoR2.VFXAttributes>();
            chargeSphereVfxAttributes.vfxIntensity = RoR2.VFXAttributes.VFXIntensity.Low;
            chargeSphereVfxAttributes.vfxPriority = RoR2.VFXAttributes.VFXPriority.Medium;

            JarChargeSphere.AddComponent<NetworkIdentity>();
            if (JarChargeSphere) PrefabAPI.RegisterNetworkPrefab(JarChargeSphere);
            EffectAPI.AddEffect(JarChargeSphere);
            //JarOrbProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>())

            JarOrb = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/JarOfReshapingOrb.prefab");

            var jarOrbEffectComponent = JarOrb.AddComponent<RoR2.EffectComponent>();

            var vfxAttributes = JarOrb.AddComponent<RoR2.VFXAttributes>();
            vfxAttributes.vfxIntensity = RoR2.VFXAttributes.VFXIntensity.Low;
            vfxAttributes.vfxPriority = RoR2.VFXAttributes.VFXPriority.Medium;

            var orbEffect = JarOrb.AddComponent<OrbEffect>();

            orbEffect.startEffect = Resources.Load<GameObject>("Prefabs/Effects/ShieldBreakEffect");
            orbEffect.endEffect = Resources.Load<GameObject>("Prefabs/Effects/MuzzleFlashes/MuzzleFlashMageIce");
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
            model.AddComponent<ProjectileGhostController>();

            var controller = JarProjectile.GetComponent<RoR2.Projectile.ProjectileController>();
            controller.procCoefficient = 1;
            controller.ghostPrefab = model;

            JarProjectile.GetComponent<RoR2.TeamFilter>().teamIndex = TeamIndex.Neutral;

            var damage = JarProjectile.GetComponent<RoR2.Projectile.ProjectileDamage>();
            damage.damageType = DamageType.Generic;
            damage.damage = 0;

            var impactExplosion = JarProjectile.GetComponent<RoR2.Projectile.ProjectileImpactExplosion>();
            impactExplosion.destroyOnEnemy = true;
            impactExplosion.destroyOnWorld = true;
            impactExplosion.impactEffect = Resources.Load<GameObject>("Prefabs/Effects/BrittleDeath");
            impactExplosion.blastRadius = 4;
            impactExplosion.blastProcCoefficient = 1f;

            // register it for networking
            if (JarProjectile) PrefabAPI.RegisterNetworkPrefab(JarProjectile);

            // add it to the projectile catalog or it won't work in multiplayer
            RoR2.ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(JarProjectile);
            };
        }

        public override void SetupConfig()
        {
            base.SetupConfig();
            cooldown = jarCooldown;
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
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
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
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
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
                    childName = "Base",
                    localPos = new Vector3(8f, -4, 8f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
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
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
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
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
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
                    childName = "Base",
                    localPos = new Vector3(-2f, 0, -2f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
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
                    childName = "Base",
                    localPos = new Vector3(-8f, 0, 8f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            return rules;
        }

        public override void Install()
        {
            base.Install();
            On.RoR2.EquipmentSlot.Update += EquipmentUpdate;
            On.RoR2.CharacterBody.FixedUpdate += AddTrackerToBodies;
        }

        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.EquipmentSlot.Update -= EquipmentUpdate;
            On.RoR2.CharacterBody.FixedUpdate -= AddTrackerToBodies;
        }

        private void AddTrackerToBodies(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            var slot = self.equipmentSlot;
            if (slot)
            {
                if (slot.equipmentIndex == equipmentDef.equipmentIndex)
                {
                    var bulletTracker = self.GetComponent<JarBulletTracker>();
                    if (!bulletTracker)
                    {
                        bulletTracker = self.gameObject.AddComponent<JarBulletTracker>();
                        bulletTracker.body = self;
                    }
                }
            }
            orig(self);
        }

        private void EquipmentUpdate(On.RoR2.EquipmentSlot.orig_Update orig, RoR2.EquipmentSlot self)
        {
            if (self.equipmentIndex == equipmentDef.equipmentIndex)
            {
                var selfDisplay = self.FindActiveEquipmentDisplay();
                var body = self.characterBody;
                if (selfDisplay && body)
                {
                    var bulletTracker = body.GetComponent<JarBulletTracker>();
                    var input = body.inputBank;
                    if (input && bulletTracker)
                    {
                        //Debug.Log($"Update ChargeTime: {bulletTracker.ChargeTime}");
                        if (bulletTracker.ChargeTime > 0)
                        {
                            selfDisplay.rotation = Quaternion.Slerp(selfDisplay.rotation, RoR2.Util.QuaternionSafeLookRotation(input.aimDirection), 0.15f);
                            orig(self);
                            return;
                        }
                    }
                    selfDisplay.rotation = Quaternion.Slerp(selfDisplay.rotation, RoR2.Util.QuaternionSafeLookRotation(Vector3.up), 0.15f);
                }
            }
            orig(self);
        }

        protected override bool PerformEquipmentAction(RoR2.EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.teamComponent) return false;
            var body = slot.characterBody;
            var bodyIdentity = body.gameObject.GetComponent<NetworkIdentity>();
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

            if (bulletTracker.jarBullets.Count > 0 && bulletTracker.ChargeTime <= 0 && bulletTracker.SuckTime <= 0)
            {
                bulletTracker.ChargeTime = 1;
                bulletTracker.RefireTime = 0.2f;
                return true;
            }
            else if (bulletTracker.jarBullets.Count <= 0 && bulletTracker.SuckTime <= 0)
            {
                bulletTracker.IsSuckingProjectiles = false;
                bulletTracker.SuckTime = projectileAbsorptionTime;
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
            public int ClientBullets;
            public bool IsSuckingProjectiles;
            public bool IsCharging;

            public void FixedUpdate()
            {
                var input = body.inputBank;
                if (SuckTime > 0)
                {
                    if (!IsSuckingProjectiles)
                    {
                        RoR2.EffectData sphere = new RoR2.EffectData
                        {
                            origin = TargetTransform ? TargetTransform.position : body.transform.position,
                            rotation = TargetTransform ? TargetTransform.rotation : body.transform.rotation,
                            rootObject = TargetTransform ? TargetTransform.gameObject : body.gameObject
                        };
                        RoR2.EffectManager.SpawnEffect(JarChargeSphere, sphere, false);

                        var bodyIdentity = body.gameObject.GetComponent<NetworkIdentity>();
                        if (bodyIdentity && NetworkServer.active) 
                        {
                            new SyncJarSucking(SyncJarSucking.MessageType.Charging, true, projectileAbsorptionTime, bodyIdentity.netId).Send(NetworkDestination.Clients);
                        }
                        IsSuckingProjectiles = true;
                    }
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
                                        if(FriendlyFireManager.friendlyFireMode != FriendlyFireManager.FriendlyFireMode.Off && iWantToLoseFriendsInChaosMode) 
                                        {
                                            if(ownerBody == body)
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            continue;
                                        }

                                    }
                                    var projectileDamage = controller.gameObject.GetComponent<ProjectileDamage>();
                                    if (projectileDamage)
                                    {
                                        jarBullets.Add(new JarBullet(projectileDamage.damage, projectileDamage.damageColorIndex, projectileDamage.damageType));

                                        var orb = new JarOfReshapingOrb();
                                        orb.Target = TargetTransform ? TargetTransform.gameObject : body.gameObject; //Where it is going to
                                        orb.origin = controller.transform.position; //Where it is coming from
                                        orb.Index = -1;
                                        OrbManager.instance.AddOrb(orb); // Fire


                                        var bodyIdentity = body.gameObject.GetComponent<NetworkIdentity>();

                                        if (bodyIdentity && NetworkServer.active)
                                        {
                                            new SyncJarOrb(SyncJarOrb.MessageType.Fired, bodyIdentity.netId, controller.transform.position).Send(NetworkDestination.Clients);
                                        }
                                        RoR2.Util.PlayScaledSound(EntityStates.Engi.EngiWeapon.ChargeGrenades.chargeStockSoundString, body.gameObject, jarBullets.Count);
                                        EntityState.Destroy(controller.gameObject);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    IsSuckingProjectiles = false;
                }
                if (ChargeTime > 0)
                {
                    if (!IsCharging && NetworkServer.active)
                    {
                        var bodyIdentity = body.gameObject.GetComponent<NetworkIdentity>();
                        if (bodyIdentity)
                        {
                            new SyncJarCharging(jarBullets.Count, ChargeTime, RefireTime, bodyIdentity.netId).Send(NetworkDestination.Clients);
                        }
                        IsCharging = true;
                    }
                    ChargeTime -= Time.fixedDeltaTime;
                    if (ChargeTime <= 0)
                    {
                        if (NetworkServer.active)
                        {
                            var bullet = jarBullets.Last();
                            FireProjectileInfo projectileInfo = new FireProjectileInfo();
                            projectileInfo.projectilePrefab = JarProjectile;
                            projectileInfo.damage = 20 + bullet.Damage * 2;
                            projectileInfo.damageColorIndex = bullet.DamageColorIndex;
                            projectileInfo.damageTypeOverride = bullet.DamageType;
                            projectileInfo.owner = body.gameObject;
                            projectileInfo.procChainMask = default(RoR2.ProcChainMask);
                            projectileInfo.position = TargetTransform ? TargetTransform.position : body.corePosition;
                            projectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(input ? input.aimDirection : body.transform.forward);
                            projectileInfo.speedOverride = 120;
                            ProjectileManager.instance.FireProjectile(projectileInfo);
                            jarBullets.RemoveAt(jarBullets.Count - 1);
                        }
                        RoR2.Util.PlaySound(EntityStates.ClayBoss.FireTarball.attackSoundString, body.gameObject);
                        ClientBullets--;
                        if (jarBullets.Count > 0 || ClientBullets > 0)
                        {
                            ChargeTime += RefireTime;
                        }
                    }
                }
                else
                {
                    IsCharging = false;
                }
            }
        }

        public class SyncJarCharging : INetMessage
        {
            private int BulletAmount;
            private float ChargeTime;
            private float RefireTime;
            private NetworkInstanceId BodyID;

            public SyncJarCharging()
            {
            }

            public SyncJarCharging(int bulletamount, float chargeTime, float refireTime, NetworkInstanceId bodyID)
            {
                BulletAmount = bulletamount;
                ChargeTime = chargeTime;
                RefireTime = refireTime;
                BodyID = bodyID;
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(BulletAmount);
                writer.Write(ChargeTime);
                writer.Write(RefireTime);
                writer.Write(BodyID);
            }

            public void Deserialize(NetworkReader reader)
            {
                BulletAmount = reader.ReadInt32();
                ChargeTime = reader.ReadSingle();
                RefireTime = reader.ReadSingle();
                BodyID = reader.ReadNetworkId();
            }

            public void OnReceived()
            {
                if (NetworkServer.active) return;
                var playerGameObject = RoR2.Util.FindNetworkObject(BodyID);
                if (playerGameObject)
                {
                    var body = playerGameObject.GetComponent<RoR2.CharacterBody>();
                    if (body)
                    {
                        var JarBulletTracker = body.GetComponent<JarBulletTracker>();
                        if (JarBulletTracker)
                        {
                            JarBulletTracker.ChargeTime = ChargeTime;
                            JarBulletTracker.RefireTime = RefireTime;
                            JarBulletTracker.ClientBullets = BulletAmount;
                        }
                    }
                }
            }
        }

        public class SyncJarSucking : INetMessage
        {
            private MessageType TypeOfMessage;
            private bool ChargeState;
            private float Duration;
            private NetworkInstanceId BodyID;

            public SyncJarSucking() 
            { 
            }

            public SyncJarSucking(MessageType messageType, bool chargeState, float duration, NetworkInstanceId bodyId) 
            {
                TypeOfMessage = messageType;
                ChargeState = chargeState;
                Duration = duration;
                BodyID = bodyId;
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write((byte)TypeOfMessage);
                writer.Write(ChargeState);
                writer.Write(Duration);
                writer.Write(BodyID);
            }

            public void Deserialize(NetworkReader reader)
            {
                TypeOfMessage = (MessageType)reader.ReadByte();
                ChargeState = reader.ReadBoolean();
                Duration = reader.ReadSingle();
                BodyID = reader.ReadNetworkId();
            }

            public void OnReceived()
            {
                if (NetworkServer.active) return;
                GameObject target = null;
                var playerGameObject = RoR2.Util.FindNetworkObject(BodyID);
                if (playerGameObject)
                {
                    var playerBody = playerGameObject.GetComponent<RoR2.CharacterBody>();
                    if (playerBody)
                    {
                        var eqp = playerBody.equipmentSlot.FindActiveEquipmentDisplay();
                        target = eqp ? eqp.gameObject : playerGameObject;

                        if (target)
                        {
                            if (ChargeState)
                            {
                                RoR2.EffectData sphere = new RoR2.EffectData
                                {
                                    origin = target.transform.position,
                                    rotation = target.transform.rotation,
                                    rootObject = target
                                };
                                RoR2.EffectManager.SpawnEffect(JarChargeSphere, sphere, false);
                            }
                        }
                    }

                }
                else
                {
                    Debug.LogError("We don't have a jar or a playerbody. Can't do a sphere!");
                    return;
                }
            }

            public enum MessageType
            {
                Charging,
                Firing
            }
        }

        public class SyncJarOrb : INetMessage
        {
            private MessageType TypeOfMessage;
            private NetworkInstanceId PlayerBody;
            private Vector3 StartingPosition;

            public SyncJarOrb()
            {
            }

            public SyncJarOrb(MessageType messageType, NetworkInstanceId playerbody, Vector3 startingPosition)
            {
                TypeOfMessage = messageType;
                PlayerBody = playerbody;
                StartingPosition = startingPosition;
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write((byte)TypeOfMessage);
                writer.Write(PlayerBody);
                writer.Write(StartingPosition);
            }

            public void Deserialize(NetworkReader reader)
            {
                TypeOfMessage = (MessageType)reader.ReadByte();
                PlayerBody = reader.ReadNetworkId();
                StartingPosition = reader.ReadVector3();
            }

            public void OnReceived()
            {
                if (NetworkServer.active) return;
                GameObject target = null;
                var playerGameObject = RoR2.Util.FindNetworkObject(PlayerBody);
                if (playerGameObject)
                {
                    var playerBody = playerGameObject.GetComponent<RoR2.CharacterBody>();
                    if (playerBody) 
                    {
                        var eqp = playerBody.equipmentSlot.FindActiveEquipmentDisplay();
                        target = eqp ? eqp.gameObject : playerGameObject;
                    }
                    
                }
                else
                {
                    Debug.LogError("We don't have a jar or a playerbody. Can't do an orb!");
                    return;
                }

                if (target) 
                {
                    var orb = new JarOfReshapingOrb();
                    orb.Target = target;
                    orb.origin = StartingPosition;
                    orb.Index = -1;
                    OrbManager.instance.AddOrb(orb);
                }
            }
            public enum MessageType : byte
            {
                Fired
            }
        }
    }
}
