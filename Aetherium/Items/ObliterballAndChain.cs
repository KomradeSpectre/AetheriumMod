using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Items
{
    public class ObliterballAndChain : ItemBase<ObliterballAndChain>
    {
        public override string ItemName => "Obliterball and Chain";

        public override string ItemLangTokenName => "OBLITERBALL_AND_CHAIN";

        public override string ItemPickupDesc => "Summon forth a deadly ball that is attached to you by a chain. Any impacts from this ball will damage anything it can proportional to its impact force.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => new GameObject();

        public override Sprite ItemIcon => null;

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
            /*var hitbox = ObliterballObject.transform.Find("Hitbox").gameObject.AddComponent<HitBox>();

            var hitboxLocator = ObliterballObject.AddComponent<HitBoxGroup>();
            hitboxLocator.hitBoxes = new HitBox[]
            {
                hitbox
            };*/

            ObliterballObject.AddComponent<ObliterballManagerComponent>();
            ObliterballObject.RegisterNetworkPrefab();

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += GiveBallController;
        }

        private void GiveBallController(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            var inventoryCount = GetCount(self);
            if(inventoryCount > 0)
            {
                var obliterballAndChainCache = self.GetComponent<ObliterballAndChainCache>();
                if (!obliterballAndChainCache)
                {
                    obliterballAndChainCache = self.gameObject.AddComponent<ObliterballAndChainCache>();

                    var obliterball = UnityEngine.Object.Instantiate(ObliterballObject, Utils.MiscUtils.AboveTargetBody(self, 4).Value, self.transform.rotation);
                    obliterballAndChainCache.Obliterball = obliterball;
                    obliterballAndChainCache.UpdateOwner(self);
                    NetworkServer.Spawn(obliterball);

                }
            }
        }

        public class ObliterballAndChainCache : MonoBehaviour
        {
            public GameObject Obliterball;
            public ObliterballManagerComponent ObliterballManager;

            public void UpdateOwner(CharacterBody characterBody)
            {
                if (!characterBody) { return; }
                var obliterballManagerComponent = Obliterball.GetComponent<ObliterballManagerComponent>();
                if (obliterballManagerComponent)
                {
                    ObliterballManager = obliterballManagerComponent;
                    ObliterballManager.CreateLinks(characterBody);
                }
            }
        }

        public class ObliterballManagerComponent : NetworkBehaviour
        {
            public CharacterBody Owner;
            public Rigidbody BallRigidBody;
            public LineRenderer LineRenderer;
            public float DamageCoefficient = 1;
            public float MinVelocityDamageThreshold = 5;
            public float MaxDistance = 0.7f;
            public float SpringSpeed = 0.5f;
            public float BallForce = 100;

            public void CreateLinks(CharacterBody characterBody)
            {
                Owner = characterBody;
                if (Owner)
                {
                    BallRigidBody = gameObject.GetComponent<Rigidbody>();
                    LineRenderer = gameObject.GetComponent<LineRenderer>();
                }
            }

            public void OnDestroy()
            {
                Destroy(LineRenderer);
                if (NetworkServer.active)
                {
                    NetworkServer.UnSpawn(gameObject);
                }                
                Destroy(gameObject);
            }

            public void FixedUpdate()
            {
                if (Owner)
                {
                    var distance = (Owner.corePosition - gameObject.transform.position);
                    if (distance.magnitude > MaxDistance)
                    {
                        var overshot = distance * ((distance.magnitude - MaxDistance) / distance.magnitude);
                        BallRigidBody.velocity += overshot * SpringSpeed;
                    }
                }
            }

            public void Update()
            {
                if (Owner)
                {
                    LineRenderer.SetPositions(new Vector3[]
                    {
                        Owner.corePosition,
                        gameObject.transform.position
                    });
                }
            }

            public void OnCollisionEnter(Collision collision)
            {
                if (collision.collider && BallRigidBody.velocity.magnitude >= MinVelocityDamageThreshold)
                {
                    var hurtbox = collision.collider.gameObject.GetComponent<HurtBox>();
                    if (hurtbox)
                    {
                        var damageInfo = new DamageInfo()
                        {
                            damage = BallRigidBody.velocity.magnitude * DamageCoefficient,
                            inflictor = gameObject,
                            position = collision.GetContact(0).point,
                        };
                        hurtbox.healthComponent.TakeDamage(damageInfo);

                        var collisionRigidbody = collision.collider.attachedRigidbody;
                        if (collisionRigidbody)
                        {
                            var preKinematicState = collisionRigidbody.isKinematic;
                            var preInterpolationState = collisionRigidbody.interpolation;

                            collisionRigidbody.isKinematic = false;
                            collisionRigidbody.interpolation = RigidbodyInterpolation.None;

                            collisionRigidbody.AddForce(collision.GetContact(0).normal * collisionRigidbody.mass * BallForce, ForceMode.Impulse);

                            collisionRigidbody.isKinematic = preKinematicState;
                            collisionRigidbody.interpolation = preInterpolationState;
                        }
                        
                    }
                }
            }
        }
    }
}
