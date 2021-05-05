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
        }

        private void CreateConfig(ConfigFile config)
        {
            
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            throw new NotImplementedException();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += DiscountNearbyGoldInteractables;
        }

        private void DiscountNearbyGoldInteractables(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, RoR2.GlobalEventManager self, RoR2.DamageReport damageReport)
        {
            if (damageReport.attackerBody && damageReport.victimBody)
            {
                var inventoryCount = GetCount(damageReport.attackerBody);
                if(inventoryCount > 0)
                {
                    List<Collider> colliders = new List<Collider>();
                    new SphereSearch()
                    {
                        origin = damageReport.victimBody.transform.position,
                        mask = RoR2.LayerIndex.entityPrecise.mask,
                        radius = 30,
                    }.OrderCandidatesByDistance().FilterCandidatesByDistinctColliderEntities().GetColliders(colliders);

                    foreach(Collider collider in colliders)
                    {
                        var interactionComponent = collider.gameObject.GetComponent<PurchaseInteraction>();
                        if (interactionComponent && interactionComponent.costType == CostTypeIndex.Money)
                        {
                            var costCacheComponent = collider.gameObject.GetComponent<MoneyInteractableCostCache>();
                            if (!costCacheComponent) 
                            { 
                                costCacheComponent = collider.gameObject.AddComponent<MoneyInteractableCostCache>();
                                costCacheComponent.OriginalCost = interactionComponent.cost;
                            }

                            var calculatedCostReductionPercentage = Mathf.Clamp((BasePricePercentageReduction + (AdditionalPricePercentageReduction * (inventoryCount - 1))), BaseMinimumPercentPrice - (AdditionalPricePercentageReduction * (inventoryCount - 1)), float.MaxValue);

                            interactionComponent.cost = (int)(costCacheComponent.OriginalCost * calculatedCostReductionPercentage);
                            interactionComponent.Networkcost *= (int)(costCacheComponent.OriginalCost * calculatedCostReductionPercentage);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            orig(self, damageReport);
        }

        public class MoneyInteractableCostCache : MonoBehaviour
        {
            public int OriginalCost;
        }
    }
}
