using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Hologram;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;
using Aetherium.Utils;
using RoR2.Audio;

namespace Aetherium.Equipment
{
    public class BellTotem : EquipmentBase<BellTotem>
    {

        public static ConfigOption<float> TimeBetweenUses;
        public static ConfigOption<float> RadiusOfBellRinging;
        public static ConfigOption<float> ForceOfBellRinging;
        public static ConfigOption<float> ForcedCooldownBetweenEquipmentUses;

        public override string EquipmentName => "Bell Totem";

        public override string EquipmentLangTokenName => "BELL_TOTEM";

        public override string EquipmentPickupDesc => "On use, spawn an interactable bell totem at the chosen position that stuns and sends enemies flying back. Additionally, any On Shrine Use effects will be activated.";

        public override string EquipmentFullDescription => $"On use, spawn an <style=cIsUtility>interactable bell totem</style> at the chosen position. When activated, trigger a <style=cIsDamage>shockwave</style> with a radius of <style=cIsUtility>{RadiusOfBellRinging}m</style> that stuns and sends enemies flying away from it. " +
            $"Additionally, any <style=cIsUtility>On Shrine Use</style> effects will be activated. " +
            $"<style=cIsUtility>{ForcedCooldownBetweenEquipmentUses}</style> second(s) between equipment uses, <style=cIsUtility>{TimeBetweenUses}</style> second(s) between totem activations, and only one totem can be spawned per activator.";

        public override string EquipmentLore => $"Accompanying every significant event in the universe has been a sound. Always there, whether or not it is heard.\n\n" +
            $"Sharp, bombastic, and the very presence of it ripples through matter with every second forward.\n\n" +
            $"Man first learned to harness this sound, and used it to remind themselves of the passing of time.\n\n" +
            $"They erected monoliths to give praise to beings they could not comprehend, and arranged this sound into a glorious melody so they may also revere that which moves them away from the past and present.\n\n" +
            $"For this purpose, so too shall we show our devotion. For this purpose, we call forth our instrument so that we may remind those that have forgotten that time is not to be taken for granted.\n\n" +
            $"Let us remind the heretics that their time has come to an end.\n\n" +
            $"-The Clockmaker.";

        public override GameObject EquipmentModel => MainAssets.LoadAsset<GameObject>("BellTotemPickup.prefab");

        public override Sprite EquipmentIcon => MainAssets.LoadAsset<Sprite>("BellTotemIcon.png");

        public static GameObject InteractableBodyModelPrefab;

        public static NetworkSoundEventDef BellRingingSound;

        public static GameObject BellSoundwaveEffect;
        public static GameObject NoBellSpawnEffect;

        public static GameObject ItemBodyModelPrefab;

        public override float Cooldown => 20;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEffect();
            CreateInteractable();
            CreateSound();
            CreateEquipment();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            TimeBetweenUses = config.ActiveBind<float>("Equipment: " + EquipmentName, "Time Between Consecutive Uses", 1, "How long in seconds should we have to wait to activate the Bell Totem each time?");
            RadiusOfBellRinging = config.ActiveBind<float>("Equipment: " + EquipmentName, "Radius of Bell Ring Shockwave", 25, "What should the radius of the bell ring shockwave be (in meter(s))?");
            ForceOfBellRinging = config.ActiveBind<float>("Equipment: " + EquipmentName, "Force of Bell Ring Shockwave", 6000, "What should the force be for the bell ring shockwave?");
            ForcedCooldownBetweenEquipmentUses = config.ActiveBind<float>("Equipment: " + EquipmentName, "Forced Cooldown Time Between Equipment Uses", 2, "How long in seconds should we have to wait before we can use another equipment stock of Bell Totem?");
        }

        private void CreateEffect()
        {
            BellSoundwaveEffect = MainAssets.LoadAsset<GameObject>("Soundwave.prefab");

            var effectComponent = BellSoundwaveEffect.AddComponent<RoR2.EffectComponent>();
            effectComponent.parentToReferencedTransform = true;
            effectComponent.positionAtReferencedTransform = true;
            effectComponent.applyScale = true;

            var destroyOnParticleEnd = BellSoundwaveEffect.AddComponent<DestroyOnParticleEnd>();
            destroyOnParticleEnd.ps = BellSoundwaveEffect.transform.Find("Wave").gameObject.GetComponent<ParticleSystem>();

            var vfxAttributes = BellSoundwaveEffect.AddComponent<RoR2.VFXAttributes>();
            vfxAttributes.vfxIntensity = RoR2.VFXAttributes.VFXIntensity.Low;
            vfxAttributes.vfxPriority = RoR2.VFXAttributes.VFXPriority.Always;

            BellSoundwaveEffect.AddComponent<NetworkIdentity>();

            if (BellSoundwaveEffect) PrefabAPI.RegisterNetworkPrefab(BellSoundwaveEffect);
            EffectAPI.AddEffect(BellSoundwaveEffect);

            NoBellSpawnEffect = MainAssets.LoadAsset<GameObject>("NoSpawnAllowedEffect.prefab");

            NoBellSpawnEffect.AddComponent<NetworkIdentity>();

            var secondaryEffectComponent = NoBellSpawnEffect.AddComponent<EffectComponent>();
            secondaryEffectComponent.parentToReferencedTransform = false;
            secondaryEffectComponent.positionAtReferencedTransform = true;
            secondaryEffectComponent.applyScale = true;

            var secondaryVfxAttributes = NoBellSpawnEffect.AddComponent<RoR2.VFXAttributes>();
            secondaryVfxAttributes.vfxIntensity = RoR2.VFXAttributes.VFXIntensity.Low;
            secondaryVfxAttributes.vfxPriority = RoR2.VFXAttributes.VFXPriority.Always;

            var secondaryDestroyOnParticleEnd = NoBellSpawnEffect.AddComponent<DestroyOnParticleEnd>();
            secondaryDestroyOnParticleEnd.ps = NoBellSpawnEffect.GetComponent<ParticleSystem>();

            if (NoBellSpawnEffect) PrefabAPI.RegisterNetworkPrefab(NoBellSpawnEffect);
            EffectAPI.AddEffect(NoBellSpawnEffect);

        }

        public void CreateInteractable()
        {
            LanguageAPI.Add("INTERACTABLE_BELL_TOTEM_NAME", "Bell Totem");
            LanguageAPI.Add("INTERACTABLE_BELL_TOTEM_CONTEXT", "Ring the Bell?");

            InteractableBodyModelPrefab = MainAssets.LoadAsset<GameObject>("BellTotem.prefab");
            InteractableBodyModelPrefab.AddComponent<NetworkIdentity>();

            var purchaseInteraction = InteractableBodyModelPrefab.AddComponent<RoR2.PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "INTERACTABLE_BELL_TOTEM_NAME";
            purchaseInteraction.contextToken = "INTERACTABLE_BELL_TOTEM_CONTEXT";
            purchaseInteraction.costType = CostTypeIndex.None;
            purchaseInteraction.available = true;
            purchaseInteraction.lockGameObject = null;
            purchaseInteraction.setUnavailableOnTeleporterActivated = false;
            purchaseInteraction.isShrine = true;
            purchaseInteraction.isGoldShrine = false;

            var entityStateMachine = InteractableBodyModelPrefab.AddComponent<EntityStateMachine>();
            entityStateMachine.customName = "Body";
            entityStateMachine.initialStateType.stateType = typeof(MyEntityStates.BellTotem.BellTotemAppearState);
            entityStateMachine.mainStateType.stateType = typeof(MyEntityStates.BellTotem.BellTotemMainState);

            var networkStateMachine = InteractableBodyModelPrefab.AddComponent<NetworkStateMachine>();
            networkStateMachine.stateMachines = new EntityStateMachine[]{ entityStateMachine };

            var genericNameDisplay = InteractableBodyModelPrefab.AddComponent<GenericDisplayNameProvider>();
            genericNameDisplay.displayToken = "INTERACTABLE_BELL_TOTEM_NAME";

            var bellTotemManager = InteractableBodyModelPrefab.AddComponent<BellTotemManager>();
            bellTotemManager.PurchaseInteraction = purchaseInteraction;

            var entityLocator = InteractableBodyModelPrefab.GetComponentInChildren<BoxCollider>().gameObject.AddComponent<RoR2.EntityLocator>();
            entityLocator.entity = InteractableBodyModelPrefab;

            var modelLocator = InteractableBodyModelPrefab.AddComponent<ModelLocator>();
            modelLocator.modelTransform = InteractableBodyModelPrefab.transform;
            modelLocator.modelBaseTransform = InteractableBodyModelPrefab.transform.Find("Base");
            modelLocator.dontDetatchFromParent = true;
            modelLocator.autoUpdateModelTransform = true;

            var highlightController = InteractableBodyModelPrefab.GetComponent<RoR2.Highlight>();
            highlightController.targetRenderer = InteractableBodyModelPrefab.GetComponentsInChildren<MeshRenderer>().Where(x => x.gameObject.name.Contains("BellTotemBody")).First();
            highlightController.strength = 1;
            highlightController.highlightColor = RoR2.Highlight.HighlightColor.interactive;

            var childLocator = InteractableBodyModelPrefab.AddComponent<ChildLocator>();
            childLocator.transformPairs = new ChildLocator.NameTransformPair[]
            {
                new ChildLocator.NameTransformPair()
                {
                    name = "FireworkOrigin",
                    transform = InteractableBodyModelPrefab.transform.Find("HologramPivot")
                }
            };

            LoadoutAPI.StateTypeOf<MyEntityStates.BellTotem.BellTotemAppearState>();
            LoadoutAPI.StateTypeOf<MyEntityStates.BellTotem.BellTotemDisappearState>();
            LoadoutAPI.StateTypeOf<MyEntityStates.BellTotem.BellTotemMainState>();
            LoadoutAPI.StateTypeOf<MyEntityStates.BellTotem.BellTotemRingingState>();
            PrefabAPI.RegisterNetworkPrefab(InteractableBodyModelPrefab);
        }

        private void CreateSound()
        {
            BellRingingSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            BellRingingSound.eventName = "Aetherium_Ring_Bell";
            SoundAPI.AddNetworkedSoundEvent(BellRingingSound);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("BellTotemDisplay.prefab");
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HandL",
                    localPos = new Vector3(0.13675F, 0.09152F, -0.00193F),
                    localAngles = new Vector3(0F, 0F, 90F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.02373F, 0.13848F, 0.01647F),
                    localAngles = new Vector3(353.7766F, 126.154F, 357.1213F),
                    localScale = new Vector3(0.035F, 0.035F, 0.035F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(2.17136F, 1.37581F, -1.87013F),
                    localAngles = new Vector3(87.07939F, 1.87543F, -0.00057F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CannonHeadR",
                    localPos = new Vector3(-0.13495F, 0.43162F, -0.29543F),
                    localAngles = new Vector3(83.80211F, 0F, 0F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.07742F, 0.30326F, 0.06456F),
                    localAngles = new Vector3(0.00001F, 48.9178F, 253.1032F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(-0.13256F, 0.29696F, -0.00507F),
                    localAngles = new Vector3(282.5898F, 43.49134F, 57.07248F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "WeaponPlatformEnd",
                    localPos = new Vector3(-0.03234F, -0.15214F, 0.03085F),
                    localAngles = new Vector3(83.19716F, 272.0715F, 272.498F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.09724F, 0.35169F, -0.33834F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(-0.26618F, 3.01169F, 1.3174F),
                    localAngles = new Vector3(10.41794F, 37.62206F, 264.5151F),
                    localScale = new Vector3(0.7F, 0.7F, 0.7F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.12027F, 0.36607F, 0.02726F),
                    localAngles = new Vector3(68.82552F, 239.7216F, 335.1728F),
                    localScale = new Vector3(0.04F, 0.04F, 0.04F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MainWeapon",
                    localPos = new Vector3(0.043F, 0.82586F, 0.00884F),
                    localAngles = new Vector3(356.5746F, 350.7418F, 89.80155F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LeftArm4",
                    localPos = new Vector3(-0.00144F, 0.00328F, 0.00045F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.00232F, 0.00232F, 0.00232F)
                }
            });

            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HandL",
                    localPos = new Vector3(-0.07161F, -0.17384F, -0.0176F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.06796F, 0.06796F, 0.06796F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00433F, 0.10519F, 0.14039F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.02943F, 0.02943F, 0.02943F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(-0.02793F, 0.18544F, 0.01705F),
                    localAngles = new Vector3(342.5478F, 59.65375F, 2.79862F),
                    localScale = new Vector3(0.05F, 0.05F, 0.05F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Model",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Model",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            return rules;
        }


        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnEquipmentGained += GiveBellTotemCache;
            On.RoR2.CharacterBody.OnEquipmentLost += RemoveBellTotemCache;
        }

        private void GiveBellTotemCache(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);
            if(equipmentDef == EquipmentDef)
            {
                var bellTotemCache = self.GetComponent<BellTotemCache>();
                if (!bellTotemCache)
                {
                    self.gameObject.AddComponent<BellTotemCache>();
                }
            }
        }

        private void RemoveBellTotemCache(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EquipmentDef)
            {
                var bellTotemCache = self.GetComponent<BellTotemCache>();
                if (bellTotemCache)
                {
                    UnityEngine.Object.Destroy(bellTotemCache);
                }
            }
            orig(self, equipmentDef);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if(slot.subcooldownTimer < ForcedCooldownBetweenEquipmentUses) { slot.subcooldownTimer = ForcedCooldownBetweenEquipmentUses; }

            var body = slot.characterBody;
            if (body && slot.inputBank)
            {
                var BellTotemCache = body.GetComponent<BellTotemCache>();
                if (BellTotemCache)
                {
                    if (BellTotemCache.BellTotem && BellTotemCache.BellTotemManager && !(BellTotemCache.BellTotemManager.BellTotemStateMachine.state is MyEntityStates.BellTotem.BellTotemDisappearState))
                    {
                        BellTotemCache.BellTotemManager.BellTotemStateMachine.SetNextState(new MyEntityStates.BellTotem.BellTotemDisappearState());
                    }

                    var hitPlace = Physics.Raycast(new Ray(slot.inputBank.aimOrigin, slot.inputBank.aimDirection), out RaycastHit raycastHit, 1000, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
                    if (hitPlace)
                    {
                        if(raycastHit.collider && raycastHit.collider.gameObject.name.Contains("BellTotem")) 
                        {
                            slot.subcooldownTimer = 0.1f;

                            var effectData = new EffectData()
                            {
                                origin = raycastHit.point,
                                scale = 0.4f
                            };
                            EffectManager.SpawnEffect(NoBellSpawnEffect, effectData, true);
                            return false; 
                        }

                        var right = Vector3.Cross(slot.inputBank.aimDirection, Vector3.up);
                        var up = Vector3.ProjectOnPlane(raycastHit.normal, right);
                        var forward = Vector3.Cross(right, up);

                        var objectToSpawn = UnityEngine.Object.Instantiate(InteractableBodyModelPrefab, raycastHit.point, Util.QuaternionSafeLookRotation(forward, up));

                        BellTotemCache.BellTotem = objectToSpawn;
                        BellTotemCache.UpdateOwner(body);

                        NetworkServer.Spawn(objectToSpawn);

                        return true;
                    }
                }
            }
            return false;
        }

        public class BellTotemCache : MonoBehaviour
        {
            public GameObject BellTotem;
            public BellTotemManager BellTotemManager;

            public void UpdateOwner(CharacterBody characterBody)
            {
                if (!characterBody || !BellTotem) { return; }

                var bellTotemManager = BellTotem.GetComponent<BellTotemManager>();
                if (bellTotemManager)
                {
                    BellTotemManager = bellTotemManager;
                    BellTotemManager.Owner = characterBody;
                }
            }
        }

        public class BellTotemManager : NetworkBehaviour
        {

            public CharacterBody Owner;
            public CharacterBody LastActivator;

            public PurchaseInteraction PurchaseInteraction;
            public EntityStateMachine BellTotemStateMachine;

            [SyncVar]
            public float Stopwatch;

            public float CooldownBetweenUses;
            public float RangeOfBlast;

            public void Start()
            {
                PurchaseInteraction.SetAvailableTrue();
                PurchaseInteraction.onPurchase.AddListener(delegate (RoR2.Interactor interactor)
                {
                    BellPurchaseAttempt(interactor);
                });

                BellTotemStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Body");
                CooldownBetweenUses = TimeBetweenUses;
            }

            public void BellPurchaseAttempt(Interactor interactor)
            {
                if (!interactor) { return; }

                var body = interactor.GetComponent<CharacterBody>();
                if (body)
                {
                    LastActivator = body;

                    if (BellTotemStateMachine.state is MyEntityStates.BellTotem.BellTotemMainState)
                    {
                        BellTotemStateMachine.SetNextState(new MyEntityStates.BellTotem.BellTotemRingingState());
                    }
                }
            }

            public void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    if (!(BellTotemStateMachine.state is MyEntityStates.BellTotem.BellTotemMainState))
                    {
                        PurchaseInteraction.SetAvailable(false);
                    }
                    else
                    {
                        Stopwatch += Time.fixedDeltaTime;
                        if (Stopwatch >= CooldownBetweenUses)
                        {
                            PurchaseInteraction.SetAvailable(true);
                        }
                        else
                        {
                            PurchaseInteraction.SetAvailable(false);
                        }
                    }

                    if (!Owner && !(BellTotemStateMachine.state is MyEntityStates.BellTotem.BellTotemDisappearState))
                    {
                        BellTotemStateMachine.SetNextState(new MyEntityStates.BellTotem.BellTotemDisappearState());
                    }
                }
            }
        }
    }
}
