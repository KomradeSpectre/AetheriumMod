using Aetherium.CoreModules;
using Aetherium.Items;
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

            public List<BuffDef> BuffsOnlyList = new List<BuffDef>();
            public BuffIndex ChosenBrazierBuff;

            public void Start()
            {
                PurchaseInteraction.SetAvailableTrue();
                PurchaseInteraction.onPurchase.AddListener(delegate (Interactor interactor)
                {
                    this.ShrinePurchaseAttempt(interactor);
                });

                foreach (BuffDef buff in BuffCatalog.buffDefs)
                {
                    if (buff.isDebuff) { continue; }
                    BuffsOnlyList.Add(buff);
                }

                ChosenBrazierBuff = ChooseTheBrazierBuff();
            }

            public void FixedUpdate()
            {
                if (InUse)
                {

                    Timer -= Time.fixedDeltaTime;

                    if (Timer > 0)
                    {
                        RoR2.HurtBox[] hurtBoxes = new RoR2.SphereSearch
                        {
                            radius = Timer,
                            mask = RoR2.LayerIndex.entityPrecise.mask,
                            origin = this.transform.position
                        }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                        foreach(HurtBox hurtbox in hurtBoxes)
                        {
                            var healthComponent = hurtbox.healthComponent;
                            if (healthComponent)
                            {
                                var body = healthComponent.body;
                                if (body)
                                {
                                    body.AddTimedBuff(ChosenBrazierBuff, 1);
                                }
                            }
                        }
                    }
                    else
                    {
                        InUse = false;
                        PurchaseInteraction.SetAvailable(true);
                    }
                }
            }

            public BuffIndex ChooseTheBrazierBuff()
            {
                return BuffsOnlyList[Run.instance.stageRng.RangeInt(0, BuffsOnlyList.Count - 1)].buffIndex;
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

                PurchaseInteraction.cost = (int)(OriginalCost * ShrineHasBeenUsedThisManyTimes);
                PurchaseInteraction.Networkcost = (int)(OriginalCost * ShrineHasBeenUsedThisManyTimes);
                InUse = true;
                Timer = CooldownBetweenRestock;
                PurchaseInteraction.SetAvailable(false);
            }
        }
    }
}
