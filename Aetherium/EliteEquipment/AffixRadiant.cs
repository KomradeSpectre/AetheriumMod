using Aetherium.Utils;
using Aetherium.Utils.Components;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.EliteEquipment
{
    public class AffixRadiant : EliteEquipmentBase<AffixRadiant>
    {
        public static ConfigOption<float> BurnRadius;
        public static ConfigOption<float> BurnPulseInterval;
        public static ConfigOption<float> BurnDuration;
        public static ConfigOption<float> BurnDamageMultiplier;
        public static ConfigOption<float> SolarFlareRadiusMultiplier;
        public static ConfigOption<int> SolarFlareBurnStacks;

        public override string EliteEquipmentName => "Apollo's Brilliance";

        public override string EliteAffixToken => "AFFIX_RADIANT";

        public override string EliteEquipmentPickupDesc => "Become an aspect of the sun.";

        public override string EliteEquipmentFullDescription => $"Radiate light that <style=cIsDamage>burns</style> enemies within <style=cIsDamage>{BurnRadius}m</style> that are in your <style=cIsUtility>line of sight</style>. " +
            $"On use, release a <style=cIsDamage>solar flare</style> that burns every visible enemy within <style=cIsDamage>{BurnRadius * SolarFlareRadiusMultiplier}m</style> <style=cIsDamage>{SolarFlareBurnStacks}</style> times.";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Radiant";

        public override GameObject EliteEquipmentModel => PickupModel;

        public override Sprite EliteEquipmentIcon => AresAssets.LoadAsset<Sprite>("ApollosBrillianceIcon.png");

        public override Sprite EliteBuffIcon => AresAssets.LoadAsset<Sprite>("ApollosBrillianceBuffIcon.png");

        public override EliteTier EliteTierDef => EliteTier.T2;

        public override Color EliteColor => new Color32(255, 214, 92, 255);

        public override Texture2D EliteRamp => AresAssets.LoadAsset<Texture2D>("Ramp Gold.png");

        public override float Cooldown => 30f;

        public override EquipmentDef CrownSourceEquipment => RoR2Content.Equipment.AffixHaunted;

        public override bool IsUnfinished => true;

        public static GameObject PickupModel;

        public static GameObject DisplayModel;

        public static GameObject RadiusIndicator;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateModels();
            CreateEquipment();
            CreateElite();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            BurnRadius = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Burn Radius", 13f, "How far (in meters, plus the body's own radius) does the radiant aura reach?");
            BurnPulseInterval = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Burn Pulse Interval", 1f, "How often (in seconds) does the radiant aura burn exposed enemies?");
            BurnDuration = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Burn Duration", 2f, "How long (in seconds) does each burn from the aura last?");
            BurnDamageMultiplier = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Burn Damage Multiplier", 0.3f, "Damage multiplier of each burn from the aura, relative to a standard burn.");
            SolarFlareRadiusMultiplier = config.ActiveBind<float>("Elite Equipment: " + EliteEquipmentName, "Solar Flare Radius Multiplier", 2f, "How many times larger than the aura is the solar flare's radius?");
            SolarFlareBurnStacks = config.ActiveBind<int>("Elite Equipment: " + EliteEquipmentName, "Solar Flare Burn Stacks", 3, "How many burns does the solar flare apply to each exposed enemy?");
        }

        private void CreateModels()
        {
            var orb = AresAssets.LoadAsset<GameObject>("ApollosBrilliance.prefab");

            PickupModel = PrefabAPI.InstantiateClone(orb, "AetheriumApollosBrilliancePickup", false);
            DisplayModel = PrefabAPI.InstantiateClone(orb, "AetheriumApollosBrillianceDisplay", false);

            // The Buff Brazier's field sphere has a scale equal to its radius in meters, so it doubles as an exact burn radius indicator.
            RadiusIndicator = PrefabAPI.InstantiateClone(MainAssets.LoadAsset<GameObject>("BuffBrazierActiveField.prefab"), "AetheriumRadiantRadiusIndicator", false);
            foreach(var behaviour in RadiusIndicator.GetComponents<MonoBehaviour>())
            {
                UnityEngine.Object.DestroyImmediate(behaviour);
            }
            var networkIdentity = RadiusIndicator.GetComponent<NetworkIdentity>();
            if(networkIdentity) { UnityEngine.Object.DestroyImmediate(networkIdentity); }

            var renderer = RadiusIndicator.GetComponent<Renderer>();
            if(renderer)
            {
                // Own copy so the Buff Brazier's field keeps its colors.
                var material = new Material(renderer.sharedMaterial);
                material.SetColor("_TintColor", EliteColor);
                renderer.sharedMaterial = material;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            // Crowns come from CrownSourceEquipment's rules, the Apollo's Brilliance orb floats above through EliteFloatingDisplay.
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnBuffFirstStackGained += AddRadiance;
        }

        private void AddRadiance(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if(buffDef != EliteBuffDef) { return; }

            EliteFloatingDisplay.Attach(self, EliteBuffDef, DisplayModel, 0.22f, 1.5f);

            if(!self.GetComponent<RadiantRadiusIndicator>())
            {
                var indicator = self.gameObject.AddComponent<RadiantRadiusIndicator>();
                indicator.Body = self;
            }

            if(NetworkServer.active && !self.GetComponent<RadiantAuraController>())
            {
                var aura = self.gameObject.AddComponent<RadiantAuraController>();
                aura.Body = self;
            }
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            var body = slot.characterBody;
            if(!body) { return false; }

            BurnExposedEnemies(body, (BurnRadius + body.radius) * SolarFlareRadiusMultiplier, SolarFlareBurnStacks);
            return true;
        }

        /// <summary>
        /// Burns every enemy within the radius that has an unobstructed line to the body. Server only.
        /// </summary>
        public static void BurnExposedEnemies(CharacterBody body, float radius, int stacks)
        {
            if(!NetworkServer.active || !body || !body.teamComponent) { return; }

            var origin = body.corePosition;
            var hurtBoxes = new SphereSearch
            {
                origin = origin,
                radius = radius,
                mask = LayerIndex.entityPrecise.mask,
                queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(body.teamComponent.teamIndex)).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

            foreach(var hurtBox in hurtBoxes)
            {
                if(!hurtBox.healthComponent || !hurtBox.healthComponent.alive) { continue; }
                if(Physics.Linecast(origin, hurtBox.transform.position, LayerIndex.world.mask, QueryTriggerInteraction.Ignore)) { continue; }

                for(int i = 0; i < stacks; i++)
                {
                    var dotInfo = new InflictDotInfo
                    {
                        attackerObject = body.gameObject,
                        victimObject = hurtBox.healthComponent.gameObject,
                        dotIndex = DotController.DotIndex.Burn,
                        duration = BurnDuration,
                        damageMultiplier = BurnDamageMultiplier
                    };
                    StrengthenBurnUtils.CheckDotForUpgrade(body.inventory, ref dotInfo);
                    DotController.InflictDot(ref dotInfo);
                }
            }
        }

        /// <summary>
        /// Shows the burn radius around the body. Purely visual, so it runs on every client.
        /// </summary>
        public class RadiantRadiusIndicator : MonoBehaviour
        {
            public CharacterBody Body;
            private GameObject _indicator;

            public void LateUpdate()
            {
                if(!Body || !Body.HasBuff(instance.EliteBuffDef) || (Body.healthComponent && !Body.healthComponent.alive))
                {
                    Destroy(this);
                    return;
                }

                if(!_indicator)
                {
                    _indicator = Instantiate(RadiusIndicator);
                }

                var radius = BurnRadius + Body.radius;
                _indicator.transform.position = Body.corePosition;
                _indicator.transform.localScale = new Vector3(radius, radius, radius);
            }

            public void OnDestroy()
            {
                if(_indicator) { Destroy(_indicator); }
            }
        }

        public class RadiantAuraController : MonoBehaviour
        {
            public CharacterBody Body;
            private float _stopwatch;

            public void FixedUpdate()
            {
                if(!Body || !Body.HasBuff(instance.EliteBuffDef))
                {
                    Destroy(this);
                    return;
                }

                _stopwatch += Time.fixedDeltaTime;
                if(_stopwatch < BurnPulseInterval) { return; }
                _stopwatch = 0;

                BurnExposedEnemies(Body, BurnRadius + Body.radius, 1);
            }
        }
    }
}
