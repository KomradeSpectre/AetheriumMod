using Aetherium.Effect;
using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Equipment.EliteEquipment
{
    internal class AffixHungering : EliteEquipmentBase<AffixHungering>
    {
        public static ConfigOption<float> CostMultiplierOfElite;

        public override string EliteEquipmentName => "The Hunger";

        public override string EliteAffixToken => "AFFIX_HUNGERING";

        public override string EliteEquipmentPickupDesc => "Become the aspect of ravenous appetite.";

        public override string EliteEquipmentFullDescription => "";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Hungering";

        public override GameObject EliteEquipmentModel => MainAssets.LoadAsset<GameObject>("PickupAffixAbyssal");

        public override Sprite EliteEquipmentIcon => MainAssets.LoadAsset<Sprite>("AffixAbyssalIcon.png");

        public override Sprite EliteBuffIcon => MainAssets.LoadAsset<Sprite>("AffixAbyssalBuffIcon.png");

        public static GameObject HungeringOrbEffectPrefab;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEffectOrb();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            Hooks();
        }

        private void CreateEffectOrb()
        {
            HungeringOrbEffectPrefab = MainAssets.LoadAsset<GameObject>("JarOfReshapingOrb.prefab");

            HungeringOrbEffectPrefab.AddComponent<RoR2.EffectComponent>();

            var vfxAttributes = HungeringOrbEffectPrefab.AddComponent<RoR2.VFXAttributes>();
            vfxAttributes.vfxIntensity = RoR2.VFXAttributes.VFXIntensity.Low;
            vfxAttributes.vfxPriority = RoR2.VFXAttributes.VFXPriority.Medium;

            var orbEffect = HungeringOrbEffectPrefab.AddComponent<OrbEffect>();

            orbEffect.startEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShieldBreakEffect");
            orbEffect.endEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MuzzleFlashes/MuzzleFlashMageIce");
            orbEffect.startVelocity1 = new Vector3(-10, 10, -10);
            orbEffect.startVelocity2 = new Vector3(10, 13, 10);
            orbEffect.endVelocity1 = new Vector3(-10, 0, -10);
            orbEffect.endVelocity2 = new Vector3(10, 5, 10);
            orbEffect.movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            HungeringOrbEffectPrefab.AddComponent<NetworkIdentity>();

            if (HungeringOrbEffectPrefab) PrefabAPI.RegisterNetworkPrefab(HungeringOrbEffectPrefab);
            ContentAddition.AddEffect(HungeringOrbEffectPrefab);

            OrbAPI.AddOrb(typeof(HungeringConsumptionOrb));
        }

        private void CreateConfig(ConfigFile config)
        {
            CostMultiplierOfElite = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Cost Multiplier of Elite", 2.5f, "How many times higher than the base elite cost should the cost of this elite be? (Do not set this to 0, only warning haha.)");

        }

        public void CreateEliteTiers()
        {
            CanAppearInEliteTiers = new CombatDirector.EliteTierDef[]
            {
                new CombatDirector.EliteTierDef()
                {
                    costMultiplier = CombatDirector.baseEliteCostMultiplier * CostMultiplierOfElite,
                    eliteTypes = Array.Empty<EliteDef>(),
                    isAvailable = SetAvailability
                }
            };
        }

        private bool SetAvailability(SpawnCard.EliteRules arg)
        {
            foreach (KeyValuePair<NetworkUserId, CharacterMaster> userMaster in Run.instance.userMasters)
            {
                var master = userMaster.Value;
                if (master && master.teamIndex == TeamIndex.Player)
                {
                    var body = master.GetBody();
                    if (body)
                    {
                        var inventory = body.inventory;
                        if (inventory)
                        {
                            var pointCountWhite = inventory.GetTotalItemCountOfTier(ItemTier.VoidTier1) + inventory.GetTotalItemCountOfTier(ItemTier.Tier1);
                            var pointCountGreen = (inventory.GetTotalItemCountOfTier(ItemTier.VoidTier2) + inventory.GetTotalItemCountOfTier(ItemTier.Tier2)) * 3;
                            var pointCountRed = (inventory.GetTotalItemCountOfTier(ItemTier.VoidTier3) + inventory.GetTotalItemCountOfTier(ItemTier.Tier3)) * 9;
                            var pointCountBoss = (inventory.GetTotalItemCountOfTier(ItemTier.Boss) + inventory.GetTotalItemCountOfTier(ItemTier.Boss)) * 6;
                            var pointCountLunar = inventory.GetTotalItemCountOfTier(ItemTier.Lunar) * -3;

                            var totalPointCount = pointCountWhite + pointCountGreen + pointCountRed + pointCountBoss + pointCountLunar;
                            if (totalPointCount > 50)
                            {
                                return true && arg == SpawnCard.EliteRules.Default;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnEquipmentGained += AddHungeringEliteController;
            On.RoR2.CharacterBody.OnEquipmentLost += RemoveHungeringEliteController;
        }

        private void AddHungeringEliteController(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);
            if (equipmentDef == EliteEquipmentDef && !self.isPlayerControlled)
            {
                var hungeringEliteController = self.GetComponent<HungeringEliteController>();
                if (!hungeringEliteController)
                {
                    hungeringEliteController = self.gameObject.AddComponent<HungeringEliteController>();
                }
            }
        }

        private void RemoveHungeringEliteController(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EliteEquipmentDef)
            {
                var hungeringEliteController = self.GetComponent<HungeringEliteController>();
                if (hungeringEliteController)
                {
                    UnityEngine.Object.Destroy(hungeringEliteController);
                }
            }
            orig(self, equipmentDef);
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }

        public class HungeringEliteController : NetworkBehaviour
        {
            public CharacterBody OwnerBody;
            public float Stopwatch;
            public float CheckNearbyConsumablesInterval = 1f;
            public Vector3 InitialTransformScale;

            public int Consumed = 0;

            public void Start()
            {
                OwnerBody = gameObject.GetComponent<CharacterBody>();

            }

            public void FixedUpdate()
            {
                if (OwnerBody)
                {
                    Stopwatch += Time.fixedDeltaTime;

                    if (Stopwatch > CheckNearbyConsumablesInterval)
                    {
                        TeamMask allyTeam = new TeamMask();
                        allyTeam.AddTeam(OwnerBody.teamComponent.teamIndex);

                        HurtBox potentialConsumptionTarget = new SphereSearch()
                        {
                            mask = LayerIndex.entityPrecise.mask,
                            origin = OwnerBody.corePosition,
                            radius = 20,
                        }.RefreshCandidates().OrderCandidatesByDistance().FilterCandidatesByHurtBoxTeam(allyTeam).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().Where(x => x.healthComponent && x.healthComponent.body && x.healthComponent.body != OwnerBody && x.healthComponent.combinedHealthFraction < 0.3f && !x.GetComponent<ConsumptionManager>()).FirstOrDefault();

                        if (potentialConsumptionTarget)
                        {                    
                            var healthComponent = potentialConsumptionTarget.healthComponent;
                            if (healthComponent)
                            {
                                var body = healthComponent.body;
                                if (body)
                                {
                                    var consumptionManager = body.gameObject.AddComponent<ConsumptionManager>();
                                    consumptionManager.Consumer = OwnerBody;
                                    EffectManager.SimpleEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniImpactExecute"), OwnerBody.corePosition, Quaternion.identity, false);
                                    ModLogger.LogError($"{OwnerBody} added a consumption manager for {body}");
                                }
                            }
                        }

                        Stopwatch = 0;
                    }
                }
            }
        }

        public class ConsumptionManager : NetworkBehaviour
        {
            public List<ConsumerRequestInfo> Consumers = new List<ConsumerRequestInfo>();
            public CharacterBody Consumer;
            public CharacterBody VictimBody;
            public bool HasFiredOrb = false;
            public bool HasResolvedConsumptionRequests = false;

            public void Start()
            {
                if (!VictimBody)
                {
                    VictimBody = gameObject.GetComponent<CharacterBody>();
                    if (!VictimBody) { Destroy(this); }
                }
            }

            public void FixedUpdate()
            {
                if (!HasFiredOrb && HasResolvedConsumptionRequests)
                {
                    if (Consumer)
                    {
                        var healthComponent = Consumer.healthComponent;
                        if (healthComponent)
                        {
                            if (healthComponent.alive)
                            {
                                var consumptionOrb = new HungeringConsumptionOrb();
                                consumptionOrb.attacker = Consumer.gameObject;
                                consumptionOrb.target = VictimBody.mainHurtBox;
                                consumptionOrb.origin = Consumer.corePosition;
                                consumptionOrb.duration = 2f;
                                OrbManager.instance.AddOrb(consumptionOrb);

                                HasFiredOrb = true;
                            }
                            else
                            {
                                Destroy(this);
                            }
                        }
                    }
                }
            }
            public struct ConsumerRequestInfo
            {
                public CharacterBody ConsumerBody;
                public CharacterBody ConsumptionTarget;
                public float ConsumptionRequestTimestamp;

                public ConsumerRequestInfo(CharacterBody consumerBody, CharacterBody consumptionTargetBody, float consumptionRequestTime)
                {
                    ConsumerBody = consumerBody;
                    ConsumptionTarget = consumptionTargetBody;
                    ConsumptionRequestTimestamp = consumptionRequestTime;
                }
            }
        }
    }
}
