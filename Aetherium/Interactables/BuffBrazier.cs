using Aetherium.CoreModules;
using Aetherium.Items;
using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Hologram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Aetherium.Interactables
{
    public class BuffBrazier : InteractableBase<BuffBrazier>
    {
        public static int BaseCostForBuffBrazier;
        public static float DurationOfEffect;
        public static float AreaOfEffectRadius;

        public override string InteractableName => "Buff Brazier";

        public override string InteractableContext => "Accept the power of the sacred flames.";

        public override string InteractableLangToken => "BUFF_BRAZIER";

        public override string InteractableModelPath => "@Aetherium:Assets/Models/Prefabs/Interactables/BuffBrazier/BuffBrazier.prefab";

        public static GameObject InteractableBodyModelPrefab;

        public static RoR2.InteractableSpawnCard InteractableSpawnCard;

        public ItemIndex FlameIndex;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateInteractablePrefab();
            CreateInteractableSpawnCard();
            CreateDirectorCard();
            CreateFlameItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            BaseCostForBuffBrazier = config.Bind<int>("Interactable: " + InteractableName, "Base Cost for Buff Brazier", 120, "What should be the base cost of the Buff Brazier?").Value;
            DurationOfEffect = config.Bind<float>("Interactable: " + InteractableName, "Duration of Flame Buff Effect", 60, "How long should the flame last once it is deployed?").Value;
            AreaOfEffectRadius = config.Bind<float>("Interactable: " + InteractableName, "Radius of Flame Buff Effect", 60, "How large (in meters) should the radius of the deployed flame buff be?").Value;
        }

        private void CreateInteractablePrefab()
        {
            InteractableBodyModelPrefab = Resources.Load<GameObject>(InteractableModelPath);

            InteractableBodyModelPrefab.AddComponent<NetworkIdentity>();

            var purchaseInteraction = InteractableBodyModelPrefab.AddComponent<RoR2.PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "INTERACTABLE_" + InteractableLangToken + "_NAME";
            purchaseInteraction.contextToken = "INTERACTABLE_" + InteractableLangToken + "_CONTEXT";
            purchaseInteraction.costType = CostTypeIndex.Money;
            purchaseInteraction.cost = BaseCostForBuffBrazier;
            purchaseInteraction.automaticallyScaleCostWithDifficulty = true;
            purchaseInteraction.available = true;
            purchaseInteraction.lockGameObject = null;
            purchaseInteraction.isShrine = true;
            purchaseInteraction.isGoldShrine = false;

            var swordBuffBrazierComponent = InteractableBodyModelPrefab.AddComponent<BuffBrazierManager>();
            swordBuffBrazierComponent.PurchaseInteraction = purchaseInteraction;
            swordBuffBrazierComponent.OriginalCost = BaseCostForBuffBrazier;
            swordBuffBrazierComponent.DurationOfField = DurationOfEffect;
            swordBuffBrazierComponent.AreaOfEffectRadius = AreaOfEffectRadius;

            var entityLocator = InteractableBodyModelPrefab.GetComponentInChildren<MeshCollider>().gameObject.AddComponent<RoR2.EntityLocator>();
            entityLocator.entity = InteractableBodyModelPrefab;

            var highlightController = InteractableBodyModelPrefab.AddComponent<RoR2.Highlight>();
            highlightController.targetRenderer = InteractableBodyModelPrefab.GetComponentsInChildren<MeshRenderer>()[0];
            highlightController.strength = 1;
            highlightController.highlightColor = RoR2.Highlight.HighlightColor.interactive;

            var hologramController = InteractableBodyModelPrefab.AddComponent<HologramProjector>();
            hologramController.hologramPivot = InteractableBodyModelPrefab.transform.GetChild(2);
            hologramController.displayDistance = 10;

            PrefabAPI.RegisterNetworkPrefab(InteractableBodyModelPrefab);
        }

        public void CreateInteractableSpawnCard()
        {
            InteractableSpawnCard = ScriptableObject.CreateInstance<RoR2.InteractableSpawnCard>();
            InteractableSpawnCard.prefab = InteractableBodyModelPrefab;
            InteractableSpawnCard.sendOverNetwork = true;
            InteractableSpawnCard.hullSize = HullClassification.Human;
            InteractableSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            InteractableSpawnCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            InteractableSpawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.NoShrineSpawn;
            InteractableSpawnCard.directorCreditCost = 10;
            InteractableSpawnCard.occupyPosition = false;
            InteractableSpawnCard.orientToFloor = false;
            InteractableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;
        }

        public void CreateDirectorCard()
        {
            RoR2.DirectorCard directorCard = new RoR2.DirectorCard
            {
                spawnCard = InteractableSpawnCard,
                selectionWeight = 1000
            };
            DirectorAPI.Helpers.AddNewInteractable(directorCard, DirectorAPI.InteractableCategory.Shrines);            
        }

        public void CreateFlameItem()
        {
            LanguageAPI.Add("INTERACTABLE_ITEM_SACRED_FLAME_NAME", "Sacred Flame");
            LanguageAPI.Add("INTERACTABLE_ITEM_SACRED_FLAME_PICKUP", "The sacred flame accepts your plea for help, and will assist your attempt to escape this area.");
            LanguageAPI.Add("INTERACTABLE_ITEM_SACRED_FLAME_DESC", $"Upon activating the teleporter, the flame will spread out into a radius of {AreaOfEffectRadius} around the teleporter and grant you and your allies aid within the area for {DurationOfEffect} seconds.");

            var flameItemDef = new RoR2.ItemDef
            {
                name = "INTERACTABLE_ITEM_SACRED_FLAME",
                nameToken = "INTERACTABLE_ITEM_SACRED_FLAME_NAME",
                pickupToken = "INTERACTABLE_ITEM_SACRED_FLAME_PICKUP",
                descriptionToken = "INTERACTABLE_ITEM_SACRED_FLAME_DESC",
                canRemove = false,
                tier = ItemTier.NoTier,
                tags = new ItemTag[] { ItemTag.WorldUnique | ItemTag.AIBlacklist },
                pickupIconPath = "@Aetherium:Assets/Textures/Icons/Item/Sacred Flame.png"
            };
            FlameIndex = ItemAPI.Add(new CustomItem(flameItemDef, new RoR2.ItemDisplayRule[] { }));
            
        }

        public void Hooks()
        {
            On.RoR2.TeleporterInteraction.OnInteractionBegin += ExpendFlames;
            //On.RoR2.SceneDirector.PopulateScene += RefundDuplicates;
            On.RoR2.Run.EndStage += RemoveErrantSacredFlames;
        }

        private void RemoveErrantSacredFlames(On.RoR2.Run.orig_EndStage orig, RoR2.Run self)
        {
            var players = RoR2.PlayerCharacterMasterController.instances;
            foreach (RoR2.PlayerCharacterMasterController playerController in players)
            {
                var master = playerController.master;

                if (master)
                {
                    var inventoryCount = master.inventory.GetItemCount(FlameIndex);
                    if (inventoryCount > 0)
                    {
                        var flameCaches = master.GetComponents<BuffBrazierSacredFlameCache>();
                        if (flameCaches.Length > 0)
                        {
                            foreach (BuffBrazierSacredFlameCache sacredFlameCache in flameCaches)
                            {
                                UnityEngine.Object.Destroy(sacredFlameCache);
                            }
                        }
                        master.inventory.RemoveItem(FlameIndex, inventoryCount);
                    }
                }
            }
            orig(self);
        }

        private void ExpendFlames(On.RoR2.TeleporterInteraction.orig_OnInteractionBegin orig, RoR2.TeleporterInteraction self, RoR2.Interactor activator)
        {
            var players = RoR2.PlayerCharacterMasterController.instances;
            foreach(RoR2.PlayerCharacterMasterController playerController in players)
            {
                var master = playerController.master;

                if (master)
                {
                    var inventoryCount = master.inventory.GetItemCount(FlameIndex);
                    if (inventoryCount > 0)
                    {
                        var flameCaches = master.GetComponents<BuffBrazierSacredFlameCache>();
                        if(flameCaches.Length > 0)
                        {
                            foreach(BuffBrazierSacredFlameCache sacredFlameCache in flameCaches)
                            {
                                sacredFlameCache.BuffBrazierManager.PositionOfAOE = self.gameObject.transform.position;
                                sacredFlameCache.BuffBrazierManager.ParticleSystem.transform.parent = self.gameObject.transform;
                                sacredFlameCache.BuffBrazierManager.AreaOfEffectRadius = Mathf.Clamp(sacredFlameCache.BuffBrazierManager.AreaOfEffectRadius + RoR2.Run.instance.stageRng.RangeFloat(-1, 1), 0, float.MaxValue);
                                sacredFlameCache.BuffBrazierManager.Timer = DurationOfEffect;
                                sacredFlameCache.BuffBrazierManager.AOEEasingInTimer = 0;
                                sacredFlameCache.BuffBrazierManager.AOEEasingOutTimer = 0;
                                sacredFlameCache.BuffBrazierManager.InUse = true;
                                UnityEngine.Object.Destroy(sacredFlameCache);
                            }
                        }
                        master.inventory.RemoveItem(FlameIndex, inventoryCount);
                    }
                }
            }
            orig(self, activator);
        }



        public class BuffBrazierManager : MonoBehaviour
        {
            public RoR2.Interactor LastInteractor;
            public CharacterMaster LastInteractorMaster;
            public RoR2.PurchaseInteraction PurchaseInteraction;

            public float OriginalCost;

            public float DurationOfField = 60f;
            public float AreaOfEffectRadius = 60f;

            public float Timer;
            public bool InUse = false;

            public ParticleSystem ParticleSystem;

            public List<BrazierBuffCuratedType> CuratedBuffList = new List<BrazierBuffCuratedType>();
            public BrazierBuffCuratedType ChosenBrazierBuff;

            public GameObject BrazierAOEIndicator;
            public Vector3 PositionOfAOE;
            public float AOEEasingInTimer;
            public float AOEEasingOutTimer;

            public void CreateCuratedBuffList()
            {
                //War Buff
                CuratedBuffList.Add(new BrazierBuffCuratedType(BuffIndex.WarCryBuff, new Color(255, 10, 10, 255), new Color(192, 10, 10, 255), 1, false));

                //Invisibility Buff
                CuratedBuffList.Add(new BrazierBuffCuratedType(BuffIndex.Cloak, new Color(173, 251, 255, 255), new Color(57, 148, 153, 255), 1.5f, false));

                //Cripple Debuff
                CuratedBuffList.Add(new BrazierBuffCuratedType(BuffIndex.Cripple, new Color(99, 188, 255, 255), new Color(61, 118, 161, 255), 1.25f, true));

                //Jade Elephant Buff
                CuratedBuffList.Add(new BrazierBuffCuratedType(BuffIndex.ElephantArmorBoost, new Color(10, 219, 113, 255), new Color(15, 120, 60, 255), 2, false));

                //Super Leech Buff
                CuratedBuffList.Add(new BrazierBuffCuratedType(BuffIndex.LifeSteal, new Color(255, 89, 144, 255), new Color(145, 49, 81, 255), 2, false));

                //No Cooldown Buff
                CuratedBuffList.Add(new BrazierBuffCuratedType(BuffIndex.NoCooldowns, new Color(142, 30, 161, 255), new Color(70, 30, 79, 255), 4, false));

                //Slowdown Debuff
                CuratedBuffList.Add(new BrazierBuffCuratedType(BuffIndex.Slow80, new Color(115, 111, 93, 255), new Color(69, 66, 55, 255), 1, true));
            }

            public BrazierBuffCuratedType ChooseTheBrazierBuff()
            {
                return CuratedBuffList[RoR2.Run.instance.stageRng.RangeInt(0, CuratedBuffList.Count)];
            }

            public void FindParticleSystemAndLightAndSetColor()
            {
                var brazierObject = this.gameObject;
                if (brazierObject)
                {
                    var normalizedColorStart = ChosenBrazierBuff.StartColor / 255;
                    var normalizedColorEnd = ChosenBrazierBuff.EndColor / 255;

                    ParticleSystem = brazierObject.GetComponentInChildren<ParticleSystem>();
                    if (ParticleSystem)
                    {
                        var color = ParticleSystem.colorOverLifetime;
                        color.color = new ParticleSystem.MinMaxGradient(normalizedColorStart, normalizedColorEnd);
                    }

                    var particleSystemRenderer = brazierObject.GetComponentInChildren<ParticleSystemRenderer>();
                    if (particleSystemRenderer)
                    {
                        var material = new Material(particleSystemRenderer.material);
                        //material.SetColor("_Tint", normalizedColorEnd);
                        particleSystemRenderer.material = material;
                    }

                    var light = brazierObject.GetComponentInChildren<Light>();
                    if (light)
                    {
                        light.color = normalizedColorEnd;
                        light.intensity = 5;
                    }
                }
            }
            public void Start()
            {
                PurchaseInteraction.SetAvailableTrue();
                PurchaseInteraction.onPurchase.AddListener(delegate (RoR2.Interactor interactor)
                {
                    this.ShrinePurchaseAttempt(interactor);
                });

                CreateCuratedBuffList();
                ChosenBrazierBuff = ChooseTheBrazierBuff();
                FindParticleSystemAndLightAndSetColor();

                PurchaseInteraction.cost = (int)(OriginalCost * ChosenBrazierBuff.CostModifier);
                PurchaseInteraction.Networkcost = (int)(OriginalCost * ChosenBrazierBuff.CostModifier);
            }

            public void ShrinePurchaseAttempt(RoR2.Interactor interactor)
            {
                LastInteractor = interactor;
                RoR2.EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new RoR2.EffectData
                {
                    origin = this.transform.position,
                    rotation = Quaternion.identity,
                    scale = 1

                }, true);

                if (ParticleSystem.isPlaying)
                {
                    ParticleSystem.Stop();
                }

                var body = interactor.GetComponent<RoR2.CharacterBody>();
                if (body)
                {
                    var master = body.master;
                    if (master)
                    {
                        LastInteractorMaster = master;
                        master.inventory.GiveItem(BuffBrazier.instance.FlameIndex);
                        var sacredFlameCache = master.gameObject.AddComponent<BuffBrazierSacredFlameCache>();
                        sacredFlameCache.BuffBrazierManager = this;
                    }

                }
                PurchaseInteraction.SetAvailable(false);
            }

            public void FixedUpdate()
            {
                if (InUse)
                {
                    if (!BrazierAOEIndicator)
                    {
                        BrazierAOEIndicator = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/Interactables/BuffBrazier/BuffBrazierActiveField.prefab"));

                        var meshRenderer = BrazierAOEIndicator.GetComponent<MeshRenderer>();
                        var material = new Material(meshRenderer.material);
                        material.shader = AetheriumPlugin.IntersectionShader;
                        material.SetColor("_TintColor", ChosenBrazierBuff.EndColor / 255);
                        meshRenderer.material = material;
                        var materialController = BrazierAOEIndicator.AddComponent<MaterialControllerComponents.HGControllerFinder>();
                        materialController.MeshRenderer = meshRenderer;

                        NetworkServer.Spawn(BrazierAOEIndicator);
                        BrazierAOEIndicator.transform.position = PositionOfAOE;
                    }

                    AOEEasingInTimer += Time.fixedDeltaTime;

                    if (AOEEasingInTimer <= 1)
                    {
                        var easingValue = EasingFunction.EaseInQuad(0, AreaOfEffectRadius, AOEEasingInTimer);
                        BrazierAOEIndicator.transform.localScale = new Vector3(easingValue, easingValue, easingValue);
                    }
                    else
                    {
                        if (ParticleSystem.isStopped)
                        {
                            ParticleSystem.Play();
                        }
                        Timer -= Time.fixedDeltaTime;

                        if (Timer > 0)
                        {
                            List<HurtBox> HurtBoxes = new List<HurtBox>();
                            if (ChosenBrazierBuff.IsDebuff)
                            {
                                RoR2.TeamMask EnemyTeams = RoR2.TeamMask.GetEnemyTeams(LastInteractorMaster.teamIndex);
                                HurtBoxes = new RoR2.SphereSearch
                                {
                                    radius = DurationOfField,
                                    mask = RoR2.LayerIndex.entityPrecise.mask,
                                    origin = BrazierAOEIndicator.transform.position
                                }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(EnemyTeams).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().ToList();
                            }
                            else
                            {
                                RoR2.TeamMask AlliedTeams = new TeamMask();
                                AlliedTeams.AddTeam(LastInteractorMaster.teamIndex);

                                HurtBoxes = new RoR2.SphereSearch
                                {
                                    radius = DurationOfField,
                                    mask = RoR2.LayerIndex.entityPrecise.mask,
                                    origin = BrazierAOEIndicator.transform.position
                                }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(AlliedTeams).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().ToList();
                            }

                            foreach (RoR2.HurtBox hurtbox in HurtBoxes)
                            {
                                var healthComponent = hurtbox.healthComponent;
                                if (healthComponent)
                                {
                                    var body = healthComponent.body;
                                    if (body)
                                    {
                                        body.AddTimedBuff(ChosenBrazierBuff.Index, 1);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (ParticleSystem.isPlaying)
                            {
                                ParticleSystem.Stop();
                            }
                            AOEEasingOutTimer += Time.fixedDeltaTime;
                            if(AOEEasingOutTimer <= 1)
                            {
                                var easingValue = EasingFunction.EaseOutQuad(DurationOfField, 0, AOEEasingOutTimer);
                                BrazierAOEIndicator.transform.localScale = new Vector3(easingValue, easingValue, easingValue);
                            }
                            else
                            { 
                                Destroy(BrazierAOEIndicator);
                                Destroy(ParticleSystem);
                                Destroy(this);
                            }
                        }
                    }
                    
                }
            }
        }

        public class BuffBrazierSacredFlameCache : MonoBehaviour
        {
            public BuffBrazierManager BuffBrazierManager;
        }

        public class BrazierBuffCuratedType
        {
            public BuffIndex Index;
            public Color StartColor;
            public Color EndColor;
            public float CostModifier;
            public bool IsDebuff;

            public BrazierBuffCuratedType(BuffIndex buffIndex, Color startColor, Color endColor, float costModifier, bool isDebuff)
            {
                Index = buffIndex;
                StartColor = startColor;
                EndColor = endColor;
                CostModifier = costModifier;
                IsDebuff = isDebuff;
            }
        }
    }
}
