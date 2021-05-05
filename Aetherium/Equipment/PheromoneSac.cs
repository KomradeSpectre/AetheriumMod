using Aetherium.Effect;
using Aetherium.Utils;
using BepInEx.Configuration;
using EntityStates;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Orbs;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Equipment
{
    public class PheromoneSac : EquipmentBase<PheromoneSac>
    {

        public override string EquipmentName => "Pheromone Sac";

        public override string EquipmentLangTokenName => "PHEROMONE_SAC";

        public override string EquipmentPickupDesc => "On activation, release a cloud of pheromones that frenzy enemies caught inside of it.";

        public override string EquipmentFullDescription => "";

        public override string EquipmentLore => $"[INCIDENT NUMBER 421076]\n" +
            $"[VISUAL RECORDING RECOVERED FROM BLACK BOX ON DERELICT 'UES SAFETY FIRST' ENGINEERING VESSEL. TRANSCRIPT TO FOLLOW]\n" +
            $"\nTerry: Hey Phil, did you see what the boys over in Expeditions brought in?\n" +
            $"Terry: Looks like a pretty ordinary jar right?\n" +
            $"Phil: Yeah, just looks like something you'd find at a housewive's art deco yard sale.\n" +
            $"Terry: Yeah, I thought the same thing, but watch and learn.\n" +
            $"Terry: First, you just hit the bottom of the jar with your palm, and ---\n" +
            $"Phil: Woah! It started glowing and what on Terra is that noise? Is that an alien vacuum cleaner?\n" +
            $"Terry: Far better. Now watch.\n" +
            $"[Terry is seen throwing random objects into the jar and Phil joins him with bewilderment on his face.]\n" +
            $"Phil: That's amazing! Can it do anything else, or is it just a weird vacuum pot?\n" +
            $"Terry: Yeah, if I just squeeze the handle here, it'll empty it all out.\n" +
            $"[Terry is seen squeezing the handles of the jar, but his expression turns to horror a moment later.]\n" +
            $"Terry: Oh hell, oh god. Phil, I just realized that I may have thrown in a few mining grenades earlier when we were having fun with it earlier.\n" +
            $"Terry: Quickly! Get one of the suits on before it---\n" +
            $"[The jar activates, shooting its contents around the room. One of the projectiles hits the hull and explodes, ripping a hole through it moments before the feed is lost.]\n" +
            $"\n[END OF FILE] ";

        public override GameObject EquipmentModel => MainAssets.LoadAsset<GameObject>("PheromoneSac.prefab");

        public override Sprite EquipmentIcon => MainAssets.LoadAsset<Sprite>("PheromoneSacIcon.png");


        public static GameObject ItemBodyModelPrefab;

        public static BuffDef BrainwashDebuff;
        public static BuffDef StrengthenedMindBuff;

        public static Material BrainwashOverlayMaterial => MainAssets.LoadAsset<Material>("PheromoneSacBrainwashOverlay.mat");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuff();
            CreateEquipment();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
        }

        private void CreateBuff()
        {
            BrainwashDebuff = ScriptableObject.CreateInstance<BuffDef>();
            BrainwashDebuff.name = "Aetherium: Brainwashed Debuff";
            BrainwashDebuff.buffColor = Color.white;
            BrainwashDebuff.canStack = false;
            BrainwashDebuff.isDebuff = true;

            BuffAPI.Add(new CustomBuff(BrainwashDebuff));

            StrengthenedMindBuff = ScriptableObject.CreateInstance<BuffDef>();
            StrengthenedMindBuff.name = "Aetherium: Strengthened Mind Buff";
            StrengthenedMindBuff.buffColor = Color.white;
            StrengthenedMindBuff.canStack = false;
            StrengthenedMindBuff.isDebuff = false;

            BuffAPI.Add(new CustomBuff(StrengthenedMindBuff));
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = EquipmentModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.5f, 0.5f, 0.5f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(8f, -4, 8f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-2f, 0, -2f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-8f, 0, 8f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
        }

        protected override bool ActivateEquipment(RoR2.EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.teamComponent || !slot.characterBody.master) return false;

            var ownerMaster = slot.characterBody.master;
            RoR2.TeamMask enemyTeams = RoR2.TeamMask.GetEnemyTeams(slot.teamComponent.teamIndex);
            RoR2.HurtBox[] hurtBoxes = new RoR2.SphereSearch
            {
                radius = 20,
                mask = RoR2.LayerIndex.entityPrecise.mask,
                origin = slot.characterBody.corePosition
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

            foreach(HurtBox hurtbox in hurtBoxes)
            {
                var body = hurtbox.healthComponent.body;
                if (body)
                {
                    if (body.HasBuff(BrainwashDebuff))
                    {
                        body.AddBuff(StrengthenedMindBuff);
                    }
                    if (body.isPlayerControlled || !body.teamComponent || body.HasBuff(StrengthenedMindBuff) || body.isBoss) { continue; }

                    var master = body.master;
                    if (master)
                    {
                        var baseAI = master.GetComponent<BaseAI>();
                        if (baseAI)
                        {
                            body.AddTimedBuff(BrainwashDebuff, 30);

                            var brainwashComponent = master.GetComponent<BrainwashHandler>();
                            if (!brainwashComponent) { brainwashComponent = master.gameObject.AddComponent<BrainwashHandler>(); }

                            brainwashComponent.Master = master;
                            brainwashComponent.Body = body;
                            brainwashComponent.AI = baseAI;

                            brainwashComponent.OriginalTeam = master.teamIndex;

                            body.teamComponent.teamIndex = ownerMaster.teamIndex;

                            UnityEngine.Object.Destroy(body.teamComponent.indicator);
                            body.teamComponent.SetupIndicator();

                            master.teamIndex = ownerMaster.teamIndex;

                            baseAI.currentEnemy.Reset();
                            baseAI.ForceAcquireNearestEnemyIfNoCurrentEnemy();
                        }
                    }
                }
            }
            return true;
        }

        public class BrainwashHandler : MonoBehaviour
        {
            public TeamIndex OriginalTeam;

            public BaseAI AI;

            public CharacterMaster Master;
            public CharacterBody Body;

            public TemporaryOverlay BrainwashOverlay;

            public void Start()
            {
                if (Body)
                {
                    EffectManager.SimpleEffect(Resources.Load<GameObject>("prefabs/effects/LevelUpEffect"), Body.transform.position, Body.transform.rotation, true);

                    if(Body.modelLocator && Body.modelLocator.modelTransform)
                    {
                        BrainwashOverlay = Body.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                        BrainwashOverlay.duration = float.PositiveInfinity;
                        BrainwashOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                        BrainwashOverlay.animateShaderAlpha = true;
                        BrainwashOverlay.destroyComponentOnEnd = true;
                        BrainwashOverlay.originalMaterial = BrainwashOverlayMaterial;
                        BrainwashOverlay.AddToCharacerModel(Body.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());
                    }
                }
            }

            public void FixedUpdate()
            {
                if(Master && Body && AI)
                {
                    if (Body.HasBuff(BrainwashDebuff))
                    {
                        if (AI.currentEnemy != null && AI.currentEnemy.characterBody && AI.currentEnemy.characterBody.teamComponent && AI.currentEnemy.characterBody.teamComponent.teamIndex == Body.teamComponent.teamIndex)
                        {
                            AI.currentEnemy.Reset();
                            AI.ForceAcquireNearestEnemyIfNoCurrentEnemy();
                        }
                    }
                    else
                    {
                        Master.teamIndex = OriginalTeam;
                        Body.teamComponent.teamIndex = OriginalTeam;
                        UnityEngine.Object.Destroy(Body.teamComponent.indicator);
                        Body.teamComponent.SetupIndicator();

                        AI.currentEnemy.Reset();
                        AI.ForceAcquireNearestEnemyIfNoCurrentEnemy();

                        Body.AddBuff(StrengthenedMindBuff);

                        if (BrainwashOverlay)
                        {
                            UnityEngine.Object.Destroy(BrainwashOverlay);
                        }

                        UnityEngine.Object.Destroy(this);
                    }
                }

            }
        }
    }
}