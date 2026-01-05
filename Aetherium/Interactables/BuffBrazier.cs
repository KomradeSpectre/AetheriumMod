using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Hologram;
using RoR2.Orbs;
using RoR2.Projectile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Aetherium.Utils;
using Aetherium.States.Interactable.BuffBrazier;
using static Aetherium.AetheriumPlugin;
using static Aetherium.Interactables.BuffBrazier;
using static Aetherium.Interactables.BuffBrazierManager;

namespace Aetherium.Interactables
{
    public class BuffBrazier : InteractableBase<BuffBrazier>
    {
        public ConfigOption<bool> EnableBuffCatalogSelection;

        public override string InteractableName => "Buff Brazier";

        public override string InteractableContext => "Purchase the strength of the sacred flames?";

        public override string InteractableLangToken => "BUFF_BRAZIER";

        public override string InteractableInspectDesc => "Allows survivors to gain a temporary buff shown by the flame color and icon above once purchased, the buff only works during the Teleporter Event inside the Teleporter's radius.";

        public override GameObject InteractableModel => MainAssets.LoadAsset<GameObject>("BuffBrazier.prefab");

        [SyncVar]
        public static GameObject InteractableBodyModelPrefab;

        [SyncVar]
        public static InteractableSpawnCard InteractableSpawnCard;

        [SyncVar]
        public static GameObject BrazierBuffFlameOrb;

        [SyncVar]
        public static GameObject BrazierBuffOrbitOrb;

        [SyncVar]
        public static GameObject BrazierFieldEffectPrefab;

        [SyncVar]
        public static List<BrazierBuffCuratedType> CuratedBuffList = new List<BrazierBuffCuratedType>();

        [SyncVar]
        public static float interval;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEffect();
            CreateNetworkObjects();
            CreateInteractable();
            CreateInteractableSpawnCard();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            EnableBuffCatalogSelection = config.ActiveBind<bool>("Interactable: " + InteractableName, "Enable All BuffCatalog Entries for Flame Selection?", false, "If set to true, the Buff Brazier will select buffs from the entire buff catalog instead of the curated list.");
        }

        private void CreateEffect()
        {
            BrazierBuffFlameOrb = MainAssets.LoadAsset<GameObject>("BuffBrazierOrbEffect.prefab");

            var effectComponent = BrazierBuffFlameOrb.AddComponent<EffectComponent>();

            var vfxAttributes = BrazierBuffFlameOrb.AddComponent<VFXAttributes>();
            vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
            vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;

            BrazierBuffFlameOrb.AddComponent<NetworkIdentity>();

            var orbEffect = BrazierBuffFlameOrb.AddComponent<OrbEffect>();
            orbEffect.startVelocity1 = new Vector3(-10, 10, -10);
            orbEffect.startVelocity2 = new Vector3(10, 13, 10);
            orbEffect.endVelocity1 = new Vector3(-10, 0, -10);
            orbEffect.endVelocity2 = new Vector3(10, 5, 10);
            orbEffect.movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            var detachParticleOnDestroy = BrazierBuffFlameOrb.AddComponent<DetachParticleOnDestroyAndEndEmission>();
            detachParticleOnDestroy.particleSystem = BrazierBuffFlameOrb.transform.Find("Fire Icon/Fire Icon Particle System/Fire Icon Trail").gameObject.GetComponent<ParticleSystem>();

            BrazierBuffFlameOrb.transform.Find("Fire Icon").gameObject.AddComponent<Billboard>();

            var visualController = BrazierBuffFlameOrb.AddComponent<BuffBrazierOrbVisualController>();
            visualController.IsVisualOrb = true;

            if(BrazierBuffFlameOrb) PrefabAPI.RegisterNetworkPrefab(BrazierBuffFlameOrb);
            ContentAddition.AddEffect(BrazierBuffFlameOrb);

            OrbAPI.AddOrb(typeof(Effect.BuffBrazierFlameOrb));
        }

        private void CreateNetworkObjects()
        {
            BrazierBuffOrbitOrb = MainAssets.LoadAsset<GameObject>("BuffBrazierOrbitOrb.prefab");

            BrazierBuffOrbitOrb.AddComponent<NetworkIdentity>();

            BrazierBuffOrbitOrb.AddComponent<SetDontDestroyOnLoad>();

            BrazierBuffOrbitOrb.AddComponent<BuffBrazierOrbitVisualAndNetworkController>();

            BrazierBuffOrbitOrb.transform.Find("Fire Icon").gameObject.AddComponent<Billboard>();

            var scaleCurve = BrazierBuffOrbitOrb.AddComponent<ObjectScaleCurve>();
            scaleCurve.overallCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.1f, 0.6f));
            scaleCurve.useOverallCurveOnly = true;

            if(BrazierBuffOrbitOrb) { PrefabAPI.RegisterNetworkPrefab(BrazierBuffOrbitOrb); }

            BrazierFieldEffectPrefab = MainAssets.LoadAsset<GameObject>("BuffBrazierActiveField.prefab");
            BrazierFieldEffectPrefab.AddComponent<BuffBrazierFieldController>();

            PrefabAPI.RegisterNetworkPrefab(BrazierFieldEffectPrefab);
        }

        public void CreateInteractable()
        {
            InteractableBodyModelPrefab = InteractableModel;
            InteractableBodyModelPrefab.AddComponent<NetworkIdentity>();

            var purchaseInteraction = InteractableBodyModelPrefab.AddComponent<RoR2.PurchaseInteraction>();
            purchaseInteraction.displayNameToken = $"INTERACTABLE_{InteractableLangToken}_NAME";
            purchaseInteraction.contextToken = $"INTERACTABLE_{InteractableLangToken}_CONTEXT";
            purchaseInteraction.costType = CostTypeIndex.Money;
            purchaseInteraction.automaticallyScaleCostWithDifficulty = false;
            purchaseInteraction.cost = 25;
            purchaseInteraction.available = true;
            purchaseInteraction.setUnavailableOnTeleporterActivated = true;
            purchaseInteraction.isShrine = true;
            purchaseInteraction.isGoldShrine = false;

            var pingInfoProvider = InteractableBodyModelPrefab.AddComponent<PingInfoProvider>();
            pingInfoProvider.pingIconOverride = MainAssets.LoadAsset<Sprite>("BuffBrazierShrineIcon.png");

            var inspect = ScriptableObject.CreateInstance<InspectDef>();
            var info = inspect.Info = new RoR2.UI.InspectInfo();
            info.Visual = MainAssets.LoadAsset<Sprite>("BuffBrazierShrineIcon.png");
            info.DescriptionToken = $"INTERACTABLE_{InteractableLangToken}_INSPECT";
            info.TitleToken = $"INTERACTABLE_{InteractableLangToken}_TITLE";
            inspect.Info = info;

            var giip = InteractableBodyModelPrefab.gameObject.AddComponent<GenericInspectInfoProvider>();
            giip.InspectInfo = inspect;

            var entityStateMachine = InteractableBodyModelPrefab.AddComponent<EntityStateMachine>();
            entityStateMachine.customName = "Body";
            entityStateMachine.initialStateType.stateType = typeof(BuffBrazierMainState);
            entityStateMachine.mainStateType.stateType = typeof(BuffBrazierMainState);

            var networkStateMachine = InteractableBodyModelPrefab.AddComponent<NetworkStateMachine>();
            networkStateMachine.stateMachines = new EntityStateMachine[] { entityStateMachine };

            var genericNameDisplay = InteractableBodyModelPrefab.AddComponent<GenericDisplayNameProvider>();
            genericNameDisplay.displayToken = $"iNTERACTABLE_{InteractableLangToken}_NAME";

            var bellTotemManager = InteractableBodyModelPrefab.AddComponent<BuffBrazierManager>();
            bellTotemManager.PurchaseInteraction = purchaseInteraction;

            var entityLocator = InteractableBodyModelPrefab.GetComponentInChildren<MeshCollider>().gameObject.AddComponent<RoR2.EntityLocator>();
            entityLocator.entity = InteractableBodyModelPrefab;

            var modelLocator = InteractableBodyModelPrefab.AddComponent<ModelLocator>();
            modelLocator.modelTransform = InteractableBodyModelPrefab.transform.Find("mdlBuffBrazier");
            modelLocator.modelBaseTransform = InteractableBodyModelPrefab.transform.Find("Base");
            modelLocator.dontDetatchFromParent = true;
            modelLocator.autoUpdateModelTransform = true;

            var highlightController = InteractableBodyModelPrefab.GetComponent<RoR2.Highlight>();
            highlightController.targetRenderer = InteractableBodyModelPrefab.GetComponentsInChildren<MeshRenderer>().Where(x => x.gameObject.name.Contains("mdlBuffBrazier")).First();
            highlightController.strength = 1;
            highlightController.highlightColor = RoR2.Highlight.HighlightColor.interactive;

            var hologramController = InteractableBodyModelPrefab.AddComponent<HologramProjector>();
            hologramController.hologramPivot = InteractableBodyModelPrefab.transform.Find("HologramPivot");
            hologramController.displayDistance = 10;
            hologramController.disableHologramRotation = true;

            var childLocator = InteractableBodyModelPrefab.AddComponent<ChildLocator>();
            childLocator.transformPairs = new ChildLocator.NameTransformPair[]
            {
                new ChildLocator.NameTransformPair()
                {
                    name = "FireworkOrigin",
                    transform = InteractableBodyModelPrefab.transform.Find("FireworkEmitter")
                }
            };

            var billboard = InteractableBodyModelPrefab.transform.Find("Fire Icon").gameObject.AddComponent<Billboard>();

            ContentAddition.AddEntityState<BuffBrazierMainState>(out _);
            ContentAddition.AddEntityState<BuffBrazierPurchased>(out _);
            PrefabAPI.RegisterNetworkPrefab(InteractableBodyModelPrefab);
        }

        public void CreateInteractableSpawnCard()
        {
            InteractableSpawnCard = ScriptableObject.CreateInstance<RoR2.InteractableSpawnCard>();
            InteractableSpawnCard.name = "iscBuffBrazier";
            InteractableSpawnCard.prefab = InteractableBodyModelPrefab;
            InteractableSpawnCard.sendOverNetwork = true;
            InteractableSpawnCard.hullSize = HullClassification.Golem;
            InteractableSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            InteractableSpawnCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            InteractableSpawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.NoShrineSpawn | RoR2.Navigation.NodeFlags.NoChestSpawn;
            InteractableSpawnCard.directorCreditCost = 20;
            InteractableSpawnCard.occupyPosition = true;
            InteractableSpawnCard.orientToFloor = false;
            InteractableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;

            RoR2.DirectorCard directorCard = new RoR2.DirectorCard
            {
                selectionWeight = 4,
                spawnCard = InteractableSpawnCard,
            };

            DirectorAPI.DirectorCardHolder directorCardHolder = new DirectorAPI.DirectorCardHolder
            {
                Card = directorCard,
                InteractableCategory = DirectorAPI.InteractableCategory.Shrines,
            };

            DirectorAPI.Helpers.AddNewInteractable(directorCardHolder);
        }

        private void Hooks()
        {
            On.RoR2.PurchaseInteraction.GetDisplayName += AppendBuffName;
            On.RoR2.TeleporterInteraction.OnInteractionBegin += SpendFlame;
            On.RoR2.PurchaseInteraction.GetInteractability += StopInteractionIfRedundant;
            On.RoR2.Run.Start += CreateCuratedBuffList;
        }

        private void CreateCuratedBuffList(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);
            CreateBaseCuratedBuffList();
        }

        private Interactability StopInteractionIfRedundant(On.RoR2.PurchaseInteraction.orig_GetInteractability orig, PurchaseInteraction self, Interactor activator)
        {
            if(self.displayNameToken == $"INTERACTABLE_{InteractableLangToken}_NAME" && activator)
            {
                var body = activator.GetComponent<CharacterBody>();
                var buffBrazierManager = self.gameObject.GetComponent<BuffBrazierManager>();
                if(body && body.master && buffBrazierManager)
                {
                    var flameOrbController = body.master.GetComponent<BuffBrazierFlameOrbController>();
                    if(flameOrbController && flameOrbController.FlameOrbs.Any(x => x.CuratedType.BuffDef == buffBrazierManager.ChosenBuffBrazierBuff.BuffDef))
                    {
                        return Interactability.ConditionsNotMet;
                    }
                }
            }

            return orig(self, activator);
        }

        private string AppendBuffName(On.RoR2.PurchaseInteraction.orig_GetDisplayName orig, PurchaseInteraction self)
        {
            if(self.displayNameToken == $"INTERACTABLE_{InteractableLangToken}_NAME")
            {
                var brazierManagerComponent = self.gameObject.GetComponent<BuffBrazierManager>();
                if(brazierManagerComponent)
                {
                    if(brazierManagerComponent.ChosenBuffBrazierBuff.BuffDef)
                    {
                        return $"Buff Brazier ({brazierManagerComponent.ChosenBuffBrazierBuff.DisplayName})";
                    }
                }
            }
            return orig(self);
        }

        private void SpendFlame(On.RoR2.TeleporterInteraction.orig_OnInteractionBegin orig, TeleporterInteraction self, Interactor activator)
        {
            orig(self, activator);

            if(activator && !self.isCharged)
            {
                var body = activator.GetComponent<CharacterBody>();
                if(body && body.master)
                {
                    var flameOrbController = body.master.GetComponent<BuffBrazierFlameOrbController>();
                    if(flameOrbController)
                    {
                        flameOrbController.StartCoroutine(flameOrbController.StaggerDeploymentToTeleporter(self.gameObject, 0.3f));
                    }
                }
            }
        }

        public struct BrazierBuffCuratedType
        {
            public string DisplayName;
            public BuffDef BuffDef;
            public Color FlameColor;
            public float CostModifier;
            public bool IsDebuff;

            public BrazierBuffCuratedType(string displayName, BuffDef buffDef, Color flameColor, float costModifier, bool isDebuff)
            {
                DisplayName = displayName;
                BuffDef = buffDef;
                FlameColor = flameColor;
                CostModifier = costModifier;
                IsDebuff = isDebuff;
            }
        }

        public struct BrazierBuffFlameOrbType
        {
            public BrazierBuffCuratedType CuratedType;
            public GameObject FlameOrbObject;

            public BrazierBuffFlameOrbType(BrazierBuffCuratedType curatedType, GameObject flameOrbObject)
            {
                CuratedType = curatedType;
                FlameOrbObject = flameOrbObject;
            }
        }

        public void AddCuratedBuffType(string displayName, BuffDef buffDef, Color32 color, float costMultiplier, bool isDebuff)
        {
            CharacterBody body = new CharacterBody();
            if(String.IsNullOrWhiteSpace(displayName))
            {
                ModLogger.LogError($"Provided displayName {displayName} is null, empty, or only contains whitespace characters! Aborting adding to curated brazier buff list!");
                return;
            }

            if(!buffDef || !buffDef.iconSprite)
            {
                ModLogger.LogError($"Provided BuffDef is null for or BuffDef {buffDef} does not contain a sprite! Aborting adding to curated brazier buff list!");
                return;
            }

            if(CuratedBuffList.Any(x => x.BuffDef == buffDef))
            {
                ModLogger.LogError($"BuffDef {buffDef} already exists in curated brazier buff list! Aborting!");
                return;
            }

            var enabled = MainConfig.ActiveBind<bool>($"Interactable: Buff Brazier {(isDebuff ? "Debuffs" : "Buffs")}", $"{displayName}: Enable in Runs?", true, $"Should this {(isDebuff ? "debuff" : "buff")} be able to appear in runs?");

            if(enabled)
            {
                CuratedBuffList.Add(new BrazierBuffCuratedType(displayName, buffDef, color, costMultiplier, isDebuff));
            }
        }

        private void CreateBaseCuratedBuffList()
        {
            if(EnableBuffCatalogSelection)
            {
                foreach (BuffDef buff in BuffCatalog.buffDefs)
                {
                    if(buff.iconSprite == null || String.IsNullOrWhiteSpace(buff.name)) { continue; }

                    var buffColor = buff.buffColor;

                    if(buff.buffColor == Color.white)
                    {
                        var r = UnityEngine.Random.Range(40, 192);
                        var g = UnityEngine.Random.Range(40, 192);
                        var b = UnityEngine.Random.Range(40, 192);
                        buffColor = new Color32((byte)r, (byte)g, (byte)b, 255);
                    }

                    AddCuratedBuffType(buff.name, buff, buffColor, UnityEngine.Random.Range(1, 4), buff.isDebuff);
                }

                return;
            }
            AddCuratedBuffType("Warcry", RoR2Content.Buffs.WarCryBuff, new Color32(255, 0, 0, 255), 1, false);

            AddCuratedBuffType("Cripple", RoR2Content.Buffs.Cripple, new Color32(0, 145, 255, 255), 1.25f, true);

            AddCuratedBuffType("Jade Elephant", RoR2Content.Buffs.ElephantArmorBoost, new Color32(0, 177, 40, 255), 2, false);

            AddCuratedBuffType("Super Leech", RoR2Content.Buffs.LifeSteal, new Color32(255, 0, 68, 255), 2, false);

            AddCuratedBuffType("Brainstalks", RoR2Content.Buffs.NoCooldowns, new Color32(196, 7, 125, 255), 4, false);

            AddCuratedBuffType("80 Percent Slowdown", RoR2Content.Buffs.Slow80, new Color32(179, 154, 61, 255), 1.5f, true);

            AddCuratedBuffType("Mercenary Expose", RoR2Content.Buffs.MercExpose, new Color32(89, 252, 255, 255), 4, true);

            if(StandaloneBuffs.Tier1.DoubleXPDoubleGold.instance != null && StandaloneBuffs.Tier1.DoubleXPDoubleGold.instance.BuffDef)
            {
                AddCuratedBuffType("Double XP and Double Gold", StandaloneBuffs.Tier1.DoubleXPDoubleGold.instance.BuffDef, StandaloneBuffs.Tier1.DoubleXPDoubleGold.instance.Color, 1, false);
            }

            if(StandaloneBuffs.Tier3.SoulLinked.instance != null && StandaloneBuffs.Tier3.SoulLinked.instance.BuffDef)
            {
                AddCuratedBuffType("Soul Linked", StandaloneBuffs.Tier3.SoulLinked.instance.BuffDef, StandaloneBuffs.Tier3.SoulLinked.instance.Color, 1.25f, true);
            }


        }

        public abstract class BuffBrazierOrbVisualBase : NetworkBehaviour
        {
            public void ColorOrb(int chosenBuffIndex)
            {
                if(!(chosenBuffIndex >= 0 && chosenBuffIndex < CuratedBuffList.Count)) { return; }

                var chosenBuff = CuratedBuffList[chosenBuffIndex];
                if(chosenBuff.BuffDef)
                {
                    var light = gameObject.transform.Find("Fire Light").GetComponent<Light>();
                    if(light)
                    {
                        light.color = chosenBuff.FlameColor;
                    }

                    var fireIcon = gameObject.transform.Find("Fire Icon").GetComponent<Renderer>();
                    if(fireIcon)
                    {
                        fireIcon.materials[0].SetTexture("_MainTex", chosenBuff.BuffDef.iconSprite.texture);
                        fireIcon.materials[0].SetColor("_TintColor", chosenBuff.FlameColor);

                        var fireIconParticleSystem = fireIcon.transform.Find("Fire Icon Particle System").GetComponent<Renderer>();
                        if(fireIconParticleSystem)
                        {
                            fireIconParticleSystem.materials[0].SetColor("_TintColor", chosenBuff.FlameColor);

                            var fireIconTrail = fireIconParticleSystem.transform.Find("Fire Icon Trail")?.GetComponent<Renderer>();
                            if(fireIconTrail)
                            {
                                fireIconTrail.materials[0].SetColor("_TintColor", chosenBuff.FlameColor);
                            }
                        }
                    }
                }
            }
        }

        public class BuffBrazierOrbVisualController : BuffBrazierOrbVisualBase
        {
            public bool IsVisualOrb = false;

            public void Start()
            {
                var effectComponent = gameObject.GetComponent<EffectComponent>();
                if(effectComponent)
                {
                    var effectData = effectComponent.effectData;
                    {
                        if(effectData != null)
                        {
                            ColorOrb((int)effectData.genericUInt);
                        }
                    }
                }
            }
        }
    }

    public class BuffBrazierOrbitVisualAndNetworkController : BuffBrazierOrbVisualBase
    {
        [SyncVar(hook = nameof(OnOwnerChanged))]
        public GameObject Owner;

        [SyncVar(hook = nameof(OnBuffIndexChanged))]
        public int ChosenBuffIndex;

        public void OnOwnerChanged(GameObject newOwner)
        {
            Owner = newOwner;
            if(!Owner) return;

            var body = Owner.GetComponent<CharacterBody>();
            var teleporter = Owner.GetComponent<TeleporterInteraction>();

            BuffBrazierFlameOrbController flameController = null;

            if(body && body.master)
            {
                flameController = body.master.GetComponent<BuffBrazierFlameOrbController>();
                if(!flameController) flameController = body.master.gameObject.AddComponent<BuffBrazierFlameOrbController>();
            }
            else if(teleporter)
            {
                flameController = Owner.GetComponent<BuffBrazierFlameOrbController>();
                if(!flameController) flameController = Owner.AddComponent<BuffBrazierFlameOrbController>();
            }

            if(flameController)
            {
                flameController.FlameOrbs.Add(new BrazierBuffFlameOrbType(CuratedBuffList[ChosenBuffIndex], gameObject));
            }
        }

        public void OnBuffIndexChanged(int newIndex)
        {
            ChosenBuffIndex = newIndex;
            ColorOrb(ChosenBuffIndex);
        }

    }

    public class BuffBrazierFlameOrbController : NetworkBehaviour
    {
        public CharacterMaster CharacterMaster;
        public CharacterBody CharacterBody;
        public List<BrazierBuffFlameOrbType> FlameOrbs = new List<BrazierBuffFlameOrbType>();

        public float CircleOffset;
        public bool IsTeleporter;

        public void Start()
        {
            IsTeleporter = gameObject.GetComponent<TeleporterInteraction>();
            if(!IsTeleporter)
            {
                CharacterMaster = gameObject.GetComponent<CharacterMaster>();
                if(CharacterMaster) CharacterBody = CharacterMaster.GetBody();
            }
        }

        public void OnDestroy()
        {
            foreach (BrazierBuffFlameOrbType buffFlameOrbType in FlameOrbs)
            {
                if(buffFlameOrbType.FlameOrbObject)
                {
                    if(NetworkServer.active) NetworkServer.UnSpawn(buffFlameOrbType.FlameOrbObject);
                    Destroy(buffFlameOrbType.FlameOrbObject);
                }
            }
        }

        public void FixedUpdate()
        {
            for (int i = FlameOrbs.Count - 1; i >= 0; i--)
            {
                if(FlameOrbs[i].FlameOrbObject == null) FlameOrbs.RemoveAt(i);
            }

            if(!IsTeleporter)
            {
                if(!CharacterMaster) CharacterMaster = gameObject.GetComponent<CharacterMaster>();
                if(CharacterMaster && !CharacterBody) CharacterBody = CharacterMaster.GetBody();
            }
        }

        public void Update()
        {
            int orbCount = FlameOrbs.Count;
            if(orbCount <= 0) return;

            CircleOffset += Time.deltaTime;         

            Vector3 center;
            float radius;

            if(!IsTeleporter && CharacterBody)
            {
                center = CharacterBody.corePosition;
                radius = 0.5f + CharacterBody.radius;
            }
            else
            {
                center = gameObject.transform.position + new Vector3(0, 1.5f, 0);
                radius = 1.5f;
            }

            float angleStep = (Mathf.PI * 2) / orbCount;

            for (int i = 0; i < orbCount; i++)
            {
                if(FlameOrbs[i].FlameOrbObject)
                {
                    float angle = (angleStep * i) + CircleOffset;

                    float x = Mathf.Cos(angle) * radius;
                    float z = Mathf.Sin(angle) * radius;

                    FlameOrbs[i].FlameOrbObject.transform.localPosition = new Vector3(center.x + x, center.y, center.z + z);
                }
            }
        }

        public IEnumerator StaggerDeploymentToTeleporter(GameObject teleporterObject, float delayBetweenConsumption)
        {
            var holdoutZoneController = teleporterObject.GetComponent<HoldoutZoneController>();
            var teleporterInteraction = teleporterObject.GetComponent<TeleporterInteraction>();

            if(!holdoutZoneController || !teleporterInteraction) yield break;

            while (FlameOrbs.Count > 0)
            {
                var chosenOrb = FlameOrbs.Last();
                var orb = new Effect.BuffBrazierFlameOrb()
                {
                    ChosenBuffIndex = CuratedBuffList.IndexOf(chosenOrb.CuratedType),
                    Target = teleporterObject,
                    origin = chosenOrb.FlameOrbObject.transform.position,
                };
                OrbManager.instance.AddOrb(orb);

                if(NetworkServer.active) NetworkServer.Destroy(chosenOrb.FlameOrbObject);

                FlameOrbs.RemoveAt(FlameOrbs.Count - 1);
                yield return new WaitForSeconds(delayBetweenConsumption);
            }

            var fieldPrefab = Instantiate(BrazierFieldEffectPrefab, teleporterObject.transform.position, Util.QuaternionSafeLookRotation(Vector3.down));
            var fieldController = fieldPrefab.GetComponent<BuffBrazierFieldController>();
            fieldController.Teleporter = teleporterObject;

            if(NetworkServer.active) NetworkServer.Spawn(fieldPrefab);
        }
    }

    public class BuffBrazierFieldController : NetworkBehaviour
    {
        [SyncVar]
        public GameObject Teleporter;

        public GameObject LastTeleporter;

        public BuffBrazierFlameOrbController FlameOrbController;
        public TeleporterInteraction TeleporterInteraction;
        public HoldoutZoneController HoldoutZoneController;

        public GameObject Activator;
        private CharacterMaster ActivatorMaster;

        public Renderer Renderer;
        public float Stopwatch;

        public List<Color> Colors = new List<Color>();
        public int CurrentColorIndex = 0;
        public Color CurrentColor;

        public void Start()
        {
            Renderer = gameObject.GetComponent<Renderer>();

            var teleporterInteraction = Teleporter.GetComponent<TeleporterInteraction>();
            var holdoutZone = Teleporter.GetComponent<HoldoutZoneController>();
            if(teleporterInteraction && holdoutZone)
            {
                HoldoutZoneController = holdoutZone;
                TeleporterInteraction = teleporterInteraction;
                Activator = teleporterInteraction.chargeActivatorServer;
                if(Activator)
                {
                    var body = Activator.GetComponent<CharacterBody>();
                    if(body && body.master)
                    {
                        ActivatorMaster = body.master;
                    }
                }
                FlameOrbController = Teleporter.GetComponent<BuffBrazierFlameOrbController>();
            }
        }

        public void FixedUpdate()
        {
            if(Teleporter && LastTeleporter != Teleporter)
            {
                LastTeleporter = Teleporter;
                Renderer = gameObject.GetComponent<Renderer>();
            }

            if(Teleporter && !HoldoutZoneController)
            {
                HoldoutZoneController = Teleporter.GetComponent<HoldoutZoneController>();
            }

            if(Teleporter && !FlameOrbController)
            {
                FlameOrbController = Teleporter.GetComponent<BuffBrazierFlameOrbController>();
            }

            if(FlameOrbController && FlameOrbController.FlameOrbs.Count > 0)
            {
                if(Colors.Count != FlameOrbController.FlameOrbs.Count)
                {
                    Colors = new List<Color>();
                    foreach (BrazierBuffFlameOrbType brazierBuffFlameOrbType in FlameOrbController.FlameOrbs)
                    {
                        Colors.Add(brazierBuffFlameOrbType.CuratedType.FlameColor);
                    }
                }
            }

            interval -= Time.fixedDeltaTime;
            if(interval <= 0f && NetworkServer.active && HoldoutZoneController && HoldoutZoneController.currentRadius > 0 && ActivatorMaster && FlameOrbController)
            {
                interval = 0.2f;
                RoR2.TeamMask enemyTeams = RoR2.TeamMask.GetEnemyTeams(ActivatorMaster.teamIndex);
                RoR2.HurtBox[] hurtBoxes = new RoR2.SphereSearch
                {
                    radius = HoldoutZoneController.currentRadius,
                    mask = RoR2.LayerIndex.entityPrecise.mask,
                    origin = gameObject.transform.position
                }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                foreach (HurtBox hurtBox in hurtBoxes)
                {
                    if(hurtBox.healthComponent && hurtBox.healthComponent.body && hurtBox.healthComponent.body.teamComponent)
                    {
                        foreach (BrazierBuffFlameOrbType flameOrbType in FlameOrbController.FlameOrbs.Where(x => x.CuratedType.BuffDef))
                        {
                            if(hurtBox.healthComponent.body.teamComponent && (flameOrbType.CuratedType.IsDebuff == enemyTeams.HasTeam(hurtBox.healthComponent.body.teamComponent.teamIndex)))
                            {
                                hurtBox.healthComponent.body.AddTimedBuff(flameOrbType.CuratedType.BuffDef, 1);
                            }
                        }
                    }
                }
            }
        }

        public void Update()
        {
            if(HoldoutZoneController && gameObject.transform.localScale.magnitude != HoldoutZoneController.currentRadius)
            {
                gameObject.transform.localScale = new Vector3(HoldoutZoneController.currentRadius, HoldoutZoneController.currentRadius, HoldoutZoneController.currentRadius);

                if(Renderer)
                {
                    if(Colors.Count > 1)
                    {
                        Stopwatch += Time.deltaTime / 3;
                        CurrentColor = Color.Lerp(Colors[CurrentColorIndex], Colors[(CurrentColorIndex + 1) % Colors.Count], Stopwatch);
                        Renderer.materials[0].SetColor("_TintColor", CurrentColor);

                        if(Stopwatch >= 1)
                        {
                            CurrentColorIndex = (CurrentColorIndex + 1) % Colors.Count;

                            Stopwatch = 0;
                        }
                    }
                    else if(Colors.Count == 1)
                    {
                        Renderer.materials[0].SetColor("_TintColor", Colors[CurrentColorIndex]);
                    }

                }

            }
        }
    }

    public class BuffBrazierManager : NetworkBehaviour
    {
        public CharacterBody Owner;
        public CharacterBody LastActivator;

        public PurchaseInteraction PurchaseInteraction;
        public EntityStateMachine BuffBrazierStateMachine;

        [SyncVar]
        public int ChosenBuffIndex;

        [SyncVar]
        public int BaseCostDetermination;

        public int LastIndex = -1;

        public BrazierBuffCuratedType ChosenBuffBrazierBuff;

        public GameObject BrazierFire;
        public GameObject BrazierFireIcon;
        public GameObject BrazierLight;


        public void Start()
        {
            if(NetworkServer.active && Run.instance)
            {
                ChosenBuffIndex = Run.instance.stageRng.RangeInt(0, CuratedBuffList.Count);
                PurchaseInteraction.SetAvailableTrue();
            }

            PurchaseInteraction.onPurchase.AddListener(BuffPurchaseAttempt);
            BuffBrazierStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Body");

            ConstructFlameChoice();
            BaseCostDetermination = (int)(PurchaseInteraction.cost * ChosenBuffBrazierBuff.CostModifier);
            SetCost();

        }

        public void ConstructFlameChoice()
        {
            ChosenBuffBrazierBuff = CuratedBuffList[ChosenBuffIndex];

            BrazierFire = gameObject.transform.Find("Fire").gameObject;

            if(BrazierFire)
            {
                var renderer = BrazierFire.GetComponent<ParticleSystemRenderer>();
                if(renderer)
                {
                    renderer.materials[0].SetColor("_TintColor", ChosenBuffBrazierBuff.FlameColor);
                }
            }

            BrazierLight = gameObject.transform.Find("Fire Icon/Fire Light").gameObject;

            if(BrazierLight)
            {
                var light = BrazierLight.GetComponent<Light>();
                if(light)
                {
                    light.color = ChosenBuffBrazierBuff.FlameColor;
                }
            }

            BrazierFireIcon = gameObject.transform.Find("Fire Icon").gameObject;

            if(BrazierFireIcon)
            {
                var renderer = BrazierFireIcon.GetComponent<Renderer>();
                if(renderer)
                {
                    renderer.materials[0].SetColor("_TintColor", ChosenBuffBrazierBuff.FlameColor);
                    renderer.materials[0].SetTexture("_MainTex", ChosenBuffBrazierBuff.BuffDef.iconSprite.texture);
                }

                var childRenderer = BrazierFireIcon.transform.Find("Fire Icon Particle System").gameObject.GetComponent<Renderer>();
                if(childRenderer)
                {
                    childRenderer.materials[0].SetColor("_TintColor", ChosenBuffBrazierBuff.FlameColor);
                }
            }
        }

        private void SetCost()
        {
            PurchaseInteraction.cost = BaseCostDetermination;
        }

        public void BuffPurchaseAttempt(Interactor interactor)
        {
            if(!interactor || LastIndex != ChosenBuffIndex) { return; }

            var body = interactor.GetComponent<CharacterBody>();
            if(body && body.master)
            {
                var flameCache = body.master.GetComponent<BuffBrazierFlameOrbController>();
                if(flameCache && flameCache.FlameOrbs.Any(x => x.CuratedType.BuffDef == ChosenBuffBrazierBuff.BuffDef))
                {
                    return;
                }

                LastActivator = body;

                if(BuffBrazierStateMachine.state is BuffBrazierMainState)
                {
                    BuffBrazierStateMachine.SetNextState(new BuffBrazierPurchased());

                    var orb = new Effect.BuffBrazierFlameOrb()
                    {
                        ChosenBuffIndex = ChosenBuffIndex,
                        Target = body.gameObject,
                        origin = gameObject.transform.Find("Fire Icon").position
                    };
                    OrbManager.instance.AddOrb(orb);

                    if(NetworkServer.active)
                    {
                        PurchaseInteraction.SetAvailable(false);
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            if(LastIndex != ChosenBuffIndex)
            {
                LastIndex = ChosenBuffIndex;
                ConstructFlameChoice();
                SetCost();
            }
        }
    }
}