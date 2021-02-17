using Aetherium.CoreModules;
using Aetherium.Items;
using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Hologram;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Aetherium.Interactables
{
    public class BuffBrazier
    {
        public static int BaseCostForBuffBrazier;

        public string InteractableName = "Buff Brazier";

        public string InteractableContext = "Donate to allow the flames to empower you.";

        public string InteractableLangToken = "BUFF_BRAZIER";

        public string InteractableModelPath = "@Aetherium:Assets/Models/Prefabs/Interactables/BuffBrazier/BuffBrazier.prefab";

        public static GameObject InteractableBodyModelPrefab;

        public static RoR2.InteractableSpawnCard InteractableSpawnCard;

        public void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateInteractable();
        }

        private void CreateConfig(ConfigFile config)
        {
            BaseCostForBuffBrazier = config.Bind<int>("Interactables: " + InteractableName, "Base Cost for Buff Brazier", 120, "What should be the base cost of the Buff Brazier?").Value;
        }

        private void CreateLang()
        {
            LanguageAPI.Add("INTERACTABLE_" + InteractableLangToken + "_NAME", InteractableName);
            LanguageAPI.Add("INTERACTABLE_" + InteractableLangToken + "_CONTEXT", InteractableContext);
        }

        private void CreateInteractable()
        {
            InteractableBodyModelPrefab = Resources.Load<GameObject>(InteractableModelPath);

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

            AetheriumPlugin.ModLogger.LogInfo("Starting to add Component");
            var swordBuffBrazierComponent = InteractableBodyModelPrefab.AddComponent<BuffBrazierManager>();
            AetheriumPlugin.ModLogger.LogInfo("Component added");
            swordBuffBrazierComponent.PurchaseInteraction = purchaseInteraction;
            AetheriumPlugin.ModLogger.LogInfo("Purchase Interaction added");
            swordBuffBrazierComponent.OriginalCost = BaseCostForBuffBrazier;

            var entityLocator = InteractableBodyModelPrefab.GetComponentInChildren<MeshCollider>().gameObject.AddComponent<RoR2.EntityLocator>();
            entityLocator.entity = InteractableBodyModelPrefab;

            var highlightController = InteractableBodyModelPrefab.AddComponent<RoR2.Highlight>();
            highlightController.targetRenderer = InteractableBodyModelPrefab.GetComponentsInChildren<MeshRenderer>()[0];
            highlightController.strength = 1;
            highlightController.highlightColor = RoR2.Highlight.HighlightColor.interactive;

            var hologramController = InteractableBodyModelPrefab.AddComponent<HologramProjector>();
            hologramController.hologramPivot = InteractableBodyModelPrefab.transform.GetChild(2);
            hologramController.displayDistance = 10;

            InteractableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            InteractableSpawnCard.prefab = InteractableBodyModelPrefab;
            InteractableSpawnCard.sendOverNetwork = true;
            InteractableSpawnCard.hullSize = HullClassification.Human;
            InteractableSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            InteractableSpawnCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            InteractableSpawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.NoShrineSpawn;
            InteractableSpawnCard.directorCreditCost = 10;
            InteractableSpawnCard.occupyPosition = true;
            InteractableSpawnCard.orientToFloor = false;
            InteractableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;

            RoR2.DirectorCard directorCard = new RoR2.DirectorCard
            {
                spawnCard = InteractableSpawnCard,
                selectionWeight = 1000
            };
            DirectorAPI.Helpers.AddNewInteractable(directorCard, DirectorAPI.InteractableCategory.Shrines);
        }

        public class BuffBrazierManager : MonoBehaviour
        {
            public Interactor LastInteractor;
            public PurchaseInteraction PurchaseInteraction;

            public float OriginalCost;
            public int ShrineHasBeenUsedThisManyTimes;

            public float CooldownBetweenRestock = 60f;
            public float Timer;
            public bool InUse = false;

            public ParticleSystem ParticleSystem;

            public List<BrazierBuffCuratedType> CuratedBuffList = new List<BrazierBuffCuratedType>();
            public BrazierBuffCuratedType ChosenBrazierBuff;

            public GameObject BrazierAOEIndicator;
            public float AOEEasingInTimer;
            public float AOEEasingOutTimer;

            public void CreateCuratedBuffList()
            {
                //War Buff
                CuratedBuffList.Add(new BrazierBuffCuratedType(BuffIndex.WarCryBuff, new Color(255, 10, 10, 255), new Color(192, 10, 10, 255), 1));

                //Invisibility Buff
                CuratedBuffList.Add(new BrazierBuffCuratedType(BuffIndex.Cloak, new Color(173, 251, 255, 255), new Color(57, 148, 153, 255), 1.5f));

                //Cripple Debuff
                CuratedBuffList.Add(new BrazierBuffCuratedType(BuffIndex.Cripple, new Color(99, 188, 255, 255), new Color(61, 118, 161, 255), 1.25f));

                //Jade Elephant Buff
                CuratedBuffList.Add(new BrazierBuffCuratedType(BuffIndex.ElephantArmorBoost, new Color(10, 219, 113, 255), new Color(15, 120, 60, 255), 2));

                //Super Leech Buff
                CuratedBuffList.Add(new BrazierBuffCuratedType(BuffIndex.LifeSteal, new Color(255, 89, 144, 255), new Color(145, 49, 81, 255), 2));

                //No Cooldown Buff
                CuratedBuffList.Add(new BrazierBuffCuratedType(BuffIndex.NoCooldowns, new Color(142, 10, 161, 255), new Color(70, 16, 79, 255), 4));

                //Slowdown Debuff
                CuratedBuffList.Add(new BrazierBuffCuratedType(BuffIndex.Slow80, new Color(115, 111, 93, 255), new Color(69, 66, 55, 255), 1));
            }

            public BrazierBuffCuratedType ChooseTheBrazierBuff()
            {
                return CuratedBuffList[Run.instance.stageRng.RangeInt(0, CuratedBuffList.Count - 1)];
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
                        material.SetColor("_EmissionColor", normalizedColorEnd);
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
                PurchaseInteraction.onPurchase.AddListener(delegate (Interactor interactor)
                {
                    this.ShrinePurchaseAttempt(interactor);
                });

                CreateCuratedBuffList();
                ChosenBrazierBuff = ChooseTheBrazierBuff();
                FindParticleSystemAndLightAndSetColor();

                PurchaseInteraction.cost = (int)(OriginalCost * ChosenBrazierBuff.CostModifier);
                PurchaseInteraction.Networkcost = (int)(OriginalCost * ChosenBrazierBuff.CostModifier);
            }

            public void FixedUpdate()
            {
                if (InUse)
                {
                    if (!BrazierAOEIndicator)
                    {
                        BrazierAOEIndicator = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/Interactables/BuffBrazier/BuffBrazierActiveField.prefab"));
                        BrazierAOEIndicator.transform.position = this.transform.position + new Vector3(0, 2f, 0);

                        var meshRenderer = BrazierAOEIndicator.GetComponent<MeshRenderer>();
                        var material = new Material(meshRenderer.material);
                        material.shader = AetheriumPlugin.IntersectionShader;
                        material.SetColor("_TintColor", ChosenBrazierBuff.EndColor / 255);
                        meshRenderer.material = material;
                        var materialController = BrazierAOEIndicator.AddComponent<MaterialControllerComponents.HGControllerFinder>();
                        materialController.Material = meshRenderer.material;
                    }

                    AOEEasingInTimer += Time.fixedDeltaTime;

                    if (AOEEasingInTimer <= 1)
                    {
                        var easingValue = EasingFunction.EaseInQuad(0, CooldownBetweenRestock, AOEEasingInTimer);
                        BrazierAOEIndicator.transform.localScale = new Vector3(easingValue, easingValue, easingValue);
                    }
                    else
                    {
                        if (ParticleSystem.isPlaying)
                        {
                            ParticleSystem.Stop();
                        }

                        Timer -= Time.fixedDeltaTime;

                        if (Timer > 0)
                        {
                            RoR2.HurtBox[] hurtBoxes = new RoR2.SphereSearch
                            {
                                radius = CooldownBetweenRestock,
                                mask = RoR2.LayerIndex.entityPrecise.mask,
                                origin = BrazierAOEIndicator.transform.position
                            }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                            foreach (HurtBox hurtbox in hurtBoxes)
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
                            AOEEasingOutTimer += Time.fixedDeltaTime;
                            if(AOEEasingOutTimer <= 1)
                            {
                                var easingValue = EasingFunction.EaseOutQuad(CooldownBetweenRestock, 0, AOEEasingOutTimer);
                                BrazierAOEIndicator.transform.localScale = new Vector3(easingValue, easingValue, easingValue);
                            }
                            else
                            {
                                if (ParticleSystem.isStopped)
                                {
                                    ParticleSystem.Play();
                                }

                                InUse = false;
                                Destroy(BrazierAOEIndicator);
                                PurchaseInteraction.SetAvailable(true);
                            }
                        }
                    }
                    
                }
            }

            public void ShrinePurchaseAttempt(Interactor interactor)
            {
                ShrineHasBeenUsedThisManyTimes++;
                LastInteractor = interactor;
                EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
                {
                    origin = this.transform.position,
                    rotation = Quaternion.identity,
                    scale = 1

                }, true);

                PurchaseInteraction.cost = (int)(OriginalCost * ChosenBrazierBuff.CostModifier * ShrineHasBeenUsedThisManyTimes);
                PurchaseInteraction.Networkcost = (int)(OriginalCost * ChosenBrazierBuff.CostModifier * ShrineHasBeenUsedThisManyTimes);
                InUse = true;
                Timer = CooldownBetweenRestock;
                AOEEasingInTimer = 0;
                AOEEasingOutTimer = 0;
                PurchaseInteraction.SetAvailable(false);
            }
        }

        public class BrazierBuffCuratedType
        {
            public BuffIndex Index;
            public Color StartColor;
            public Color EndColor;
            public float CostModifier;

            public BrazierBuffCuratedType(BuffIndex buffIndex, Color startColor, Color endColor, float costModifier)
            {
                Index = buffIndex;
                StartColor = startColor;
                EndColor = endColor;
                CostModifier = costModifier;
            }
        }
    }
}
