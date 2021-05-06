using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Aetherium.Utils;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Items
{
    public class BloodDiamond : ItemBase<BloodDiamond>
    {
        public ConfigOption<float> BaseMinimumPercentPrice;
        public ConfigOption<float> AdditionalMinimumPriceReduction;
        public ConfigOption<float> BasePricePercentageReduction;
        public ConfigOption<float> AdditionalPricePercentageReduction;

        public override string ItemName => "Sanguine Coupon";

        public override string ItemLangTokenName => "SANGUINE_COUPON";

        public override string ItemPickupDesc => "";

        public override string ItemFullDescription => "";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => new GameObject();

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("FeatheredPlume");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            BaseMinimumPercentPrice = config.ActiveBind<float>("Item: " + ItemName, "Base Minimum Percentage for Interactable Prices", 0.2f, "What percentage should we be able to reduce prices by at max for the first stack?");
            AdditionalMinimumPriceReduction = config.ActiveBind<float>("Item: " + ItemName, "Minimum Percentage Subtracted per Additional Stacks", 0.05f, "How much further should we reduce the minimum price possible per additional stack of this item?");
            BasePricePercentageReduction = config.ActiveBind<float>("Item: " + ItemName, "Base Price Percentage Reduction per Kill", 0.025f, "How much cost in percentage should we remove per kill?");
            AdditionalPricePercentageReduction = config.ActiveBind<float>("Item: " + ItemName, "Additional Price Percentage Reduction per Kill", 0.025f, "How much cost in percentage should we reduce per additional stacks of this item per kill?");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += DiscountNearbyGoldInteractables;
        }

        private void DiscountNearbyGoldInteractables(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, RoR2.GlobalEventManager self, RoR2.DamageReport damageReport)
        {
            orig(self, damageReport);

            if (damageReport.attackerBody && damageReport.victimBody)
            {
                ModLogger.LogError($"AttackerBody is: {damageReport.attackerBody}\nVictimBody is: {damageReport.victimBody}");

                var inventoryCount = GetCount(damageReport.attackerBody);
                if(inventoryCount > 0)
                {
                    ModLogger.LogError($"Starting Spheresearch.");
                    List<Collider> colliders = new List<Collider>();
                    new SphereSearch()
                    {
                        origin = damageReport.victimBody.transform.position,
                        mask = RoR2.LayerIndex.CommonMasks.interactable,
                        radius = 30,
                        queryTriggerInteraction = QueryTriggerInteraction.Collide,

                    }.ClearCandidates().RefreshCandidates().FilterCandidatesByColliderEntities().OrderCandidatesByDistance().FilterCandidatesByDistinctColliderEntities().GetColliders(colliders);

                    ModLogger.LogError($"Found {colliders.Count} colliders");

                    foreach (Collider collider in colliders)
                    {
                        ModLogger.LogError($"Collider is: {collider}");

                        var interactionEntity = collider.GetComponent<EntityLocator>();
                        if (interactionEntity)
                        {
                            ModLogger.LogError($"Found Interaction Entity: {interactionEntity}");
                            var interactionComponent = interactionEntity.entity.GetComponent<PurchaseInteraction>();

                            if (interactionComponent && interactionComponent.costType == CostTypeIndex.Money && interactionComponent.available)
                            {
                                ModLogger.LogError($"Found Purchase Interaction: {interactionComponent} and passed conditions");
                                var costCacheComponent = collider.gameObject.GetComponent<MoneyInteractableCostCache>();
                                if (!costCacheComponent)
                                {
                                    costCacheComponent = collider.gameObject.AddComponent<MoneyInteractableCostCache>();
                                    costCacheComponent.OriginalCost = interactionComponent.cost;
                                }

                                costCacheComponent.KillsOnThisInteractable++;

                                var minRate = 1 - (BaseMinimumPercentPrice + (1 - BaseMinimumPercentPrice) * (1 - 1 / (1 + AdditionalMinimumPriceReduction * (inventoryCount - 1))));
                                var rate = Mathf.Clamp(1 - (BasePricePercentageReduction + AdditionalPricePercentageReduction * costCacheComponent.KillsOnThisInteractable), minRate, 1);
                                var newPrice = (int)(costCacheComponent.OriginalCost * rate);

                                interactionComponent.cost = newPrice;
                                interactionComponent.Networkcost = newPrice;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
            }
        }

        public class MoneyInteractableCostCache : MonoBehaviour
        {
            public int OriginalCost;
            public int KillsOnThisInteractable;
        }
    }
}
