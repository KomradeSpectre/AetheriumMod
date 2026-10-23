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
        public static ConfigOption<int> SimulacrumWavesPerFlame;

        public override string InteractableName => "Buff Brazier";

        public override string InteractableContext => "Purchase the strength of the sacred flames?";

        public override string InteractableLangToken => "BUFF_BRAZIER";

        public override string InteractableInspectDesc => "Allows survivors to gain a temporary buff shown by the flame color and icon above once purchased, the buff only works during the Teleporter Event inside the Teleporter's radius. " +
            $"In the Simulacrum, purchased flames are offered to the Void Crab's ward when the next wave starts and last {SimulacrumWavesPerFlame} wave(s) inside its radius.";

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

        /// <summary>
        /// Buffs that some bodies must not get from a brazier, keyed by the activation state of one of the body's skills.
        /// Brainstalks on the Junk Drone makes it drop temporary items nonstop.
        /// </summary>
        private static readonly Dictionary<BuffDef, HashSet<Type>> BuffSkillStateBlacklist = new Dictionary<BuffDef, HashSet<Type>>();
        private static readonly Dictionary<(BodyIndex, BuffDef), bool> BlacklistCache = new Dictionary<(BodyIndex, BuffDef), bool>();

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
            SimulacrumWavesPerFlame = config.ActiveBind<int>("Interactable: " + InteractableName, "Simulacrum: Waves per Flame", 2, "In the Simulacrum, flames are offered to the Void Crab's ward at the start of the next wave. How many waves should each flame's buff last inside the ward's radius?");
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

            DirectorAPI.Helpers.AddNewInteractable(directorCardHolder, _ => AcceptsExtraInteractables(SceneInfo.instance));
        }

        private static bool AcceptsExtraInteractables(SceneInfo scene)
        {
            return scene && scene.sceneDef && (scene.countsAsStage || scene.sceneDef.allowItemsToSpawnObjects);
        }

        private void Hooks()
        {
            On.RoR2.PurchaseInteraction.GetDisplayName += AppendBuffName;
            On.RoR2.TeleporterInteraction.OnInteractionBegin += SpendFlame;
            On.RoR2.PurchaseInteraction.GetInteractability += StopInteractionIfRedundant;
            On.RoR2.Run.Start += CreateCuratedBuffList;
            InfiniteTowerRun.onWaveInitialized += SpendFlamesOnSafeWard;
            RoR2Application.onLoad += CreateBuffBlacklist;
        }

        private void CreateBuffBlacklist()
        {
            BuffSkillStateBlacklist[RoR2Content.Buffs.NoCooldowns] = new HashSet<Type>
            {
                typeof(EntityStates.Drone.DroneJunk.Surprise)
            };
        }

        /// <summary>
        /// Whether the brazier should withhold this buff from the body, see BuffSkillStateBlacklist.
        /// </summary>
        public static bool IsBuffBlacklistedForBody(CharacterBody body, BuffDef buffDef)
        {
            if(!body || !buffDef || !BuffSkillStateBlacklist.TryGetValue(buffDef, out var blacklistedStates)) { return false; }

            var key = (body.bodyIndex, buffDef);
            if(BlacklistCache.TryGetValue(key, out var cached)) { return cached; }

            bool blacklisted = false;
            if(body.skillLocator)
            {
                foreach(var skill in body.skillLocator.allSkills)
                {
                    var stateType = skill && skill.skillDef ? skill.skillDef.activationState.stateType : null;
                    if(stateType != null && blacklistedStates.Contains(stateType))
                    {
                        blacklisted = true;
                        break;
                    }
                }
            }

            BlacklistCache[key] = blacklisted;
            return blacklisted;
        }

        /// <summary>
        /// The Simulacrum has no teleporter event, so players' flames go to the Void Crab's ward when a wave starts.
        /// </summary>
        private void SpendFlamesOnSafeWard(InfiniteTowerWaveController waveController)
        {
            if(!NetworkServer.active) return;

            var run = Run.instance as InfiniteTowerRun;
            if(!run || !run.safeWardController) return;

            foreach(var playerController in PlayerCharacterMasterController.instances)
            {
                var master = playerController ? playerController.master : null;
                if(!master) continue;

                var flameOrbController = master.GetComponent<BuffBrazierFlameOrbController>();
                if(flameOrbController && flameOrbController.FlameOrbs.Count > 0)
                {
                    flameOrbController.StartCoroutine(flameOrbController.StaggerDeploymentToZone(run.safeWardController.gameObject, 0.3f));
                }
            }
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
                        flameOrbController.StartCoroutine(flameOrbController.StaggerDeploymentToZone(self.gameObject, 0.3f));
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

        /// <summary>
        /// Brazier buffs are networked by BuffCatalog index rather than by position in CuratedBuffList,
        /// because every player builds CuratedBuffList from their own config and the positions can differ between machines.
        /// </summary>
        public static bool TryGetCuratedType(int buffIndex, out BrazierBuffCuratedType curatedType)
        {
            curatedType = default;
            if(buffIndex < 0) { return false; }

            var buffDef = BuffCatalog.GetBuffDef((BuffIndex)buffIndex);
            if(!buffDef) { return false; }

            foreach(var entry in CuratedBuffList)
            {
                if(entry.BuffDef == buffDef)
                {
                    curatedType = entry;
                    return true;
                }
            }

            // The host has it enabled but this player's config doesn't, still show the right buff.
            curatedType = new BrazierBuffCuratedType(buffDef.name, buffDef, buffDef.buffColor, 1, buffDef.isDebuff);
            return true;
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

                    // Seeded from the buff name so every player rolls the same color and cost.
                    var random = new System.Random(StableHash(buff.name));
                    var buffColor = buff.buffColor;

                    if(buff.buffColor == Color.white)
                    {
                        var r = random.Next(40, 192);
                        var g = random.Next(40, 192);
                        var b = random.Next(40, 192);
                        buffColor = new Color32((byte)r, (byte)g, (byte)b, 255);
                    }

                    AddCuratedBuffType(buff.name, buff, buffColor, random.Next(1, 4), buff.isDebuff);
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

        private static int StableHash(string text)
        {
            unchecked
            {
                int hash = (int)2166136261;
                foreach(char character in text)
                {
                    hash = (hash ^ character) * 16777619;
                }
                return hash;
            }
        }

        public abstract class BuffBrazierOrbVisualBase : NetworkBehaviour
        {
            public void ColorOrb(int buffIndex)
            {
                if(!TryGetCuratedType(buffIndex, out var chosenBuff)) { return; }

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
        // Set before NetworkServer.Spawn and never changed afterwards, so clients receive them in the spawn payload.
        // UNet doesn't call SyncVar hooks for the spawn payload, so registration happens in Start instead (once, on every machine).
        [SyncVar]
        public GameObject Owner;

        /// <summary>
        /// BuffCatalog index of the flame's buff.
        /// </summary>
        [SyncVar]
        public int ChosenBuffIndex = -1;

        /// <summary>
        /// Server only. Simulacrum wave index at which this flame burns out, -1 if it doesn't.
        /// </summary>
        public int ExpireAtWave = -1;

        private bool _registered;

        public void Start()
        {
            ColorOrb(ChosenBuffIndex);
            RegisterWithOwner();
        }

        private void RegisterWithOwner()
        {
            if(_registered || !Owner || !TryGetCuratedType(ChosenBuffIndex, out var curatedType)) return;
            _registered = true;

            var body = Owner.GetComponent<CharacterBody>();

            BuffBrazierFlameOrbController flameController = null;

            if(body && body.master)
            {
                flameController = body.master.GetComponent<BuffBrazierFlameOrbController>();
                if(!flameController) flameController = body.master.gameObject.AddComponent<BuffBrazierFlameOrbController>();
            }
            else if(BuffBrazierFlameOrbController.IsZoneAnchor(Owner))
            {
                flameController = Owner.GetComponent<BuffBrazierFlameOrbController>();
                if(!flameController) flameController = Owner.AddComponent<BuffBrazierFlameOrbController>();
            }

            if(flameController)
            {
                flameController.FlameOrbs.Add(new BrazierBuffFlameOrbType(curatedType, gameObject));
            }
        }
    }

    public class BuffBrazierFlameOrbController : NetworkBehaviour
    {
        public CharacterMaster CharacterMaster;
        public CharacterBody CharacterBody;
        public List<BrazierBuffFlameOrbType> FlameOrbs = new List<BrazierBuffFlameOrbType>();

        public float CircleOffset;

        /// <summary>
        /// True when this holds the flames offered to a zone (teleporter or Simulacrum ward) rather than a player's flames.
        /// </summary>
        public bool IsTeleporter;

        /// <summary>
        /// Objects whose zone flames can be offered to: the teleporter, or the Void Crab's ward in the Simulacrum.
        /// </summary>
        public static bool IsZoneAnchor(GameObject gameObject)
        {
            return gameObject && (gameObject.GetComponent<TeleporterInteraction>() || gameObject.GetComponent<InfiniteTowerSafeWardController>());
        }

        public void Start()
        {
            IsTeleporter = IsZoneAnchor(gameObject);
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

            if(IsTeleporter && NetworkServer.active && Run.instance is InfiniteTowerRun infiniteTowerRun)
            {
                foreach(var flameOrb in FlameOrbs)
                {
                    var orbController = flameOrb.FlameOrbObject.GetComponent<BuffBrazierOrbitVisualAndNetworkController>();
                    if(orbController && orbController.ExpireAtWave >= 0 && infiniteTowerRun.waveIndex >= orbController.ExpireAtWave)
                    {
                        NetworkServer.Destroy(flameOrb.FlameOrbObject);
                    }
                }
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

        public IEnumerator StaggerDeploymentToZone(GameObject zoneObject, float delayBetweenConsumption)
        {
            if(!IsZoneAnchor(zoneObject)) yield break;

            while (FlameOrbs.Count > 0 && zoneObject)
            {
                var chosenOrb = FlameOrbs.Last();
                FlameOrbs.RemoveAt(FlameOrbs.Count - 1);
                if(!chosenOrb.FlameOrbObject) continue;

                var orb = new Effect.BuffBrazierFlameOrb()
                {
                    ChosenBuffIndex = (int)chosenOrb.CuratedType.BuffDef.buffIndex,
                    Target = zoneObject,
                    origin = chosenOrb.FlameOrbObject.transform.position,
                };
                OrbManager.instance.AddOrb(orb);

                if(NetworkServer.active) NetworkServer.Destroy(chosenOrb.FlameOrbObject);

                yield return new WaitForSeconds(delayBetweenConsumption);
            }

            // One field per zone, every player's flames feed the same one.
            if(!zoneObject || BuffBrazierFieldController.ExistsFor(zoneObject)) yield break;

            var fieldPrefab = Instantiate(BrazierFieldEffectPrefab, zoneObject.transform.position, Util.QuaternionSafeLookRotation(Vector3.down));
            var fieldController = fieldPrefab.GetComponent<BuffBrazierFieldController>();
            fieldController.Teleporter = zoneObject;

            if(NetworkServer.active) NetworkServer.Spawn(fieldPrefab);
        }
    }

    public class BuffBrazierFieldController : NetworkBehaviour
    {
        private static readonly List<BuffBrazierFieldController> Instances = new List<BuffBrazierFieldController>();

        /// <summary>
        /// The zone this field covers: the teleporter, or the Void Crab's ward in the Simulacrum.
        /// </summary>
        [SyncVar]
        public GameObject Teleporter;

        public GameObject LastTeleporter;

        public BuffBrazierFlameOrbController FlameOrbController;
        public TeleporterInteraction TeleporterInteraction;
        public HoldoutZoneController HoldoutZoneController;
        public VerticalTubeZone SafeWardZone;

        public GameObject Activator;
        private CharacterMaster ActivatorMaster;

        public Renderer Renderer;
        public float Stopwatch;

        public List<Color> Colors = new List<Color>();
        public int CurrentColorIndex = 0;
        public Color CurrentColor;

        private float buffInterval;

        public static bool ExistsFor(GameObject zoneObject)
        {
            return Instances.Any(x => x && x.Teleporter == zoneObject);
        }

        public void OnEnable()
        {
            Instances.Add(this);
        }

        public void OnDisable()
        {
            Instances.Remove(this);
        }

        public void Start()
        {
            Renderer = gameObject.GetComponent<Renderer>();
            FindZoneComponents();
        }

        private void FindZoneComponents()
        {
            if(!Teleporter) return;

            if(!HoldoutZoneController && !SafeWardZone)
            {
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
                }
                else if(Teleporter.GetComponent<InfiniteTowerSafeWardController>())
                {
                    SafeWardZone = Teleporter.GetComponent<VerticalTubeZone>();
                }
            }

            if(!FlameOrbController)
            {
                FlameOrbController = Teleporter.GetComponent<BuffBrazierFlameOrbController>();
            }
        }

        private float CurrentRadius
        {
            get
            {
                if(HoldoutZoneController) return HoldoutZoneController.currentRadius;
                if(SafeWardZone) return SafeWardZone.radius;
                return 0;
            }
        }

        public void FixedUpdate()
        {
            if(NetworkServer.active && !Teleporter)
            {
                // The ward is gone (stage transition), so is this field.
                NetworkServer.Destroy(gameObject);
                return;
            }

            if(Teleporter && LastTeleporter != Teleporter)
            {
                LastTeleporter = Teleporter;
                Renderer = gameObject.GetComponent<Renderer>();
            }

            FindZoneComponents();

            if(Teleporter && SafeWardZone)
            {
                gameObject.transform.position = Teleporter.transform.position;
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
                    CurrentColorIndex = 0;
                    Stopwatch = 0;
                }
            }

            buffInterval -= Time.fixedDeltaTime;
            var radius = CurrentRadius;
            if(buffInterval <= 0f && NetworkServer.active && radius > 0 && (ActivatorMaster || SafeWardZone) && FlameOrbController && FlameOrbController.FlameOrbs.Count > 0)
            {
                buffInterval = 0.2f;
                RoR2.TeamMask enemyTeams = RoR2.TeamMask.GetEnemyTeams(ActivatorMaster ? ActivatorMaster.teamIndex : TeamIndex.Player);
                RoR2.HurtBox[] hurtBoxes = new RoR2.SphereSearch
                {
                    radius = radius,
                    mask = RoR2.LayerIndex.entityPrecise.mask,
                    origin = gameObject.transform.position
                }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                foreach (HurtBox hurtBox in hurtBoxes)
                {
                    var body = hurtBox.healthComponent ? hurtBox.healthComponent.body : null;
                    if(!body || !body.teamComponent) continue;

                    // The ward's zone is a vertical tube, the sphere search only narrows it down.
                    if(SafeWardZone && !SafeWardZone.IsInBounds(body.corePosition)) continue;

                    foreach (BrazierBuffFlameOrbType flameOrbType in FlameOrbController.FlameOrbs.Where(x => x.CuratedType.BuffDef))
                    {
                        if(flameOrbType.CuratedType.IsDebuff == enemyTeams.HasTeam(body.teamComponent.teamIndex) && !BuffBrazier.IsBuffBlacklistedForBody(body, flameOrbType.CuratedType.BuffDef))
                        {
                            body.AddTimedBuff(flameOrbType.CuratedType.BuffDef, 1);
                        }
                    }
                }
            }
        }

        public void Update()
        {
            if(Renderer)
            {
                // A Simulacrum field outlives its flames until the ward goes away, hide it while it has none.
                Renderer.enabled = !FlameOrbController || FlameOrbController.FlameOrbs.Count > 0;
            }

            var radius = CurrentRadius;
            if((HoldoutZoneController || SafeWardZone) && gameObject.transform.localScale.magnitude != radius)
            {
                gameObject.transform.localScale = new Vector3(radius, radius, radius);

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

        /// <summary>
        /// BuffCatalog index of the chosen buff, see BuffBrazier.TryGetCuratedType.
        /// </summary>
        [SyncVar]
        public int ChosenBuffIndex = -1;

        [SyncVar]
        public int BaseCostDetermination;

        public int LastIndex = -1;

        public BrazierBuffCuratedType ChosenBuffBrazierBuff;

        public GameObject BrazierFire;
        public GameObject BrazierFireIcon;
        public GameObject BrazierLight;

        public void Awake()
        {
            // Chosen before NetworkServer.Spawn so clients get the buff in the spawn payload.
            if(NetworkServer.active && Run.instance && CuratedBuffList.Count > 0)
            {
                var chosen = CuratedBuffList[Run.instance.stageRng.RangeInt(0, CuratedBuffList.Count)];
                ChosenBuffIndex = (int)chosen.BuffDef.buffIndex;
                BaseCostDetermination = (int)(PurchaseInteraction.cost * chosen.CostModifier);
            }
        }

        public void Start()
        {
            if(NetworkServer.active)
            {
                PurchaseInteraction.SetAvailableTrue();
            }

            PurchaseInteraction.onPurchase.AddListener(BuffPurchaseAttempt);
            BuffBrazierStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Body");
        }

        public void ConstructFlameChoice()
        {
            if(!TryGetCuratedType(ChosenBuffIndex, out ChosenBuffBrazierBuff)) { return; }

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