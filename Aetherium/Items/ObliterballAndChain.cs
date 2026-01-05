using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Items
{
    public class ObliterballAndChain : ItemBase<ObliterballAndChain>
    {
        public override string ItemName => "Obliterball and Chain";
        public override string ItemLangTokenName => "OBLITERBALL_AND_CHAIN";
        public override string ItemPickupDesc => "Summon a deadly physics ball tethered to you. Momentum deals damage.";
        public override string ItemFullDescription => "A heavy ball is tethered to you. It swings freely and deals damage based on impact velocity.";
        public override string ItemLore => "It's heavy. Don't trip.";

        public override ItemTier Tier => ItemTier.Lunar;
        public override GameObject ItemModel => new GameObject(); // Placeholder
        public override Sprite ItemIcon => null; // Placeholder

        public static GameObject ObliterballObject;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateNetworkedObject();
            CreateItem();
            Hooks();
        }

        private void CreateNetworkedObject()
        {
            ObliterballObject = MainAssets.LoadAsset<GameObject>("ObliterballObject.prefab");
            ObliterballObject.AddComponent<NetworkIdentity>();

            // IMPORTANT: Ensure the prefab has a SphereCollider and Rigidbody in the editor,
            // or add them here if missing.

            if (!ObliterballObject.GetComponent<Rigidbody>())
            {
                var rigidbody = ObliterballObject.AddComponent<Rigidbody>();
                rigidbody.mass = 25;
            }
            if (!ObliterballObject.GetComponent<SphereCollider>())
            {
                var collider = ObliterballObject.AddComponent<SphereCollider>();
                collider.radius = 1;
            }
            ObliterballObject.AddComponent<ObliterballManagerComponent>();
            ObliterballObject.RegisterNetworkPrefab();
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules() => new ItemDisplayRuleDict();

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += GiveBallController;
        }

        private void GiveBallController(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);

            // Only the server spawns networked objects
            if (!NetworkServer.active) return;

            var inventoryCount = GetCount(self);
            var cache = self.GetComponent<ObliterballAndChainCache>();

            // Spawn the ball if we have the item and don't have a ball yet
            if (inventoryCount > 0 && !cache)
            {
                cache = self.gameObject.AddComponent<ObliterballAndChainCache>();

                // Spawn 4m above player to prevent clipping on start
                var spawnPos = self.corePosition + Vector3.up * 4;
                var obliterball = Object.Instantiate(ObliterballObject, spawnPos, self.transform.rotation);

                // Assign the owner info to the SyncVar
                var manager = obliterball.GetComponent<ObliterballManagerComponent>();
                manager.OwnerObject = self.gameObject;

                NetworkServer.Spawn(obliterball);

                // CRITICAL: Give the owner authority so physics feels smooth locally
                if (self.master && self.master.networkIdentity && self.master.networkIdentity.connectionToClient != null)
                {
                    obliterball.GetComponent<NetworkIdentity>().AssignClientAuthority(self.master.networkIdentity.connectionToClient);
                }

                cache.Obliterball = obliterball;
            }
            // Logic to remove the ball if item count drops to 0 could go here
        }

        public class ObliterballAndChainCache : MonoBehaviour
        {
            public GameObject Obliterball;

            public void OnDestroy()
            {
                if (Obliterball && NetworkServer.active)
                {
                    NetworkServer.Destroy(Obliterball);
                }
            }
        }

        public class ObliterballManagerComponent : NetworkBehaviour
        {
            [SyncVar]
            public GameObject OwnerObject;

            public CharacterBody OwnerBody;
            public Rigidbody BallRigidBody;
            public ConfigurableJoint Joint;
            public LineRenderer LineRenderer;

            // Settings
            public float ChainLength = 5.0f;
            public float DamageCoefficient = 1f;
            public float MinVelocityDamageThreshold = 15f; // Threshold is slightly higher for self-damage to avoid "tripping" damage
            public float KnockbackForce = 2000f;

            // Safety
            private float _selfHitCooldown = 0f; // Prevents insta-death from multi-frame collisions

            public override void OnStartClient()
            {
                base.OnStartClient();
                if (OwnerObject) InitializeJoint();
            }

            public override void OnStartServer()
            {
                base.OnStartServer();
                if (OwnerObject) InitializeJoint();
            }

            private void InitializeJoint()
            {
                if (!OwnerObject) return;
                OwnerBody = OwnerObject.GetComponent<CharacterBody>();
                BallRigidBody = GetComponent<Rigidbody>();
                LineRenderer = GetComponent<LineRenderer>();

                if (!OwnerBody || !BallRigidBody) return;

                // PHYSICS TRICK:
                // We want the ball to pass THROUGH the player (so movement isn't blocked),
                // but still trigger collision events so we can detect the hit.
                // 1. Ensure Ball Collider is NOT a trigger.
                // 2. We use Physics.IgnoreCollision to prevent physical blocking.
                // 3. BUT Physics.IgnoreCollision disables OnCollisionEnter... 
                // 
                // SOLUTION: We keep collision ON (so you can trip), but rely on the Joint 
                // to keep it somewhat manageable. If it's too janky, we switch to Trigger logic.
                // For a true Lunar, physical collision is hilarious. Let's keep it.

                Joint = gameObject.GetComponent<ConfigurableJoint>();
                if (!Joint) Joint = gameObject.AddComponent<ConfigurableJoint>();

                Joint.connectedBody = OwnerBody.rigidbody;
                Joint.autoConfigureConnectedAnchor = false;
                Joint.anchor = Vector3.zero;
                Joint.connectedAnchor = Vector3.zero;

                // Lock rotation
                Joint.angularXMotion = ConfigurableJointMotion.Free;
                Joint.angularYMotion = ConfigurableJointMotion.Free;
                Joint.angularZMotion = ConfigurableJointMotion.Free;

                // Limit the distance
                Joint.xMotion = ConfigurableJointMotion.Limited;
                Joint.yMotion = ConfigurableJointMotion.Limited;
                Joint.zMotion = ConfigurableJointMotion.Limited;

                var limit = new SoftJointLimit();
                limit.limit = ChainLength;
                limit.bounciness = 0.5f;
                Joint.linearLimit = limit;

                var spring = new SoftJointLimitSpring();
                spring.spring = 10f;
                spring.damper = 1f;
                Joint.linearLimitSpring = spring;
            }

            public void FixedUpdate()
            {
                if (_selfHitCooldown > 0) _selfHitCooldown -= Time.fixedDeltaTime;
            }

            public void Update()
            {
                if (OwnerBody && LineRenderer)
                {
                    LineRenderer.SetPosition(0, OwnerBody.corePosition);
                    LineRenderer.SetPosition(1, transform.position);
                }
            }

            public void OnCollisionEnter(Collision collision)
            {
                if (!hasAuthority && !NetworkServer.active) return;

                // Check relative velocity magnitude (How hard did we hit?)
                if (collision.relativeVelocity.magnitude >= MinVelocityDamageThreshold)
                {
                    HurtBox hurtbox = collision.collider.GetComponent<HurtBox>();
                    if (hurtbox)
                    {
                        // LUNAR LOGIC: Friendly Fire Check
                        bool isOwner = (hurtbox.healthComponent.body == OwnerBody);

                        // If it's the owner, check cooldown
                        if (isOwner)
                        {
                            if (_selfHitCooldown > 0) return;
                            _selfHitCooldown = 0.5f; // 0.5s immunity after smacking yourself
                        }

                        // Calculate Damage
                        // If it's the owner, maybe deal EXTRA damage? Or reduced? 
                        // Currently set to 1:1 same as enemies.
                        float damageAmount = collision.relativeVelocity.magnitude * BallRigidBody.mass * DamageCoefficient;

                        DamageInfo damageInfo = new DamageInfo()
                        {
                            damage = damageAmount,
                            inflictor = gameObject,
                            attacker = OwnerObject,
                            position = collision.GetContact(0).point,
                            crit = isOwner ? false : OwnerBody.RollCrit(), // Don't crit yourself, that's just mean
                            damageType = isOwner ? DamageType.NonLethal : DamageType.Generic // Optional: Make self-damage non-lethal? Remove if you want death.
                        };

                        hurtbox.healthComponent.TakeDamage(damageInfo);

                        // Physics Knockback
                        // If we hit the player, this effectively "trips" them or sends them flying.
                        Rigidbody targetRB = collision.collider.attachedRigidbody;
                        if (targetRB && !targetRB.isKinematic)
                        {
                            Vector3 direction = collision.GetContact(0).normal * -1;
                            targetRB.AddForce(direction * KnockbackForce, ForceMode.Impulse);
                        }

                        // Sound Effect
                        if (isOwner)
                        {
                            // Funny dull thud for hitting yourself
                            Util.PlaySound("Play_clayBruiser_attack2_shoot", gameObject);
                        }
                        else
                        {
                            Util.PlaySound("Play_bell_impact", gameObject);
                        }
                    }
                }
            }
        }
    }
}