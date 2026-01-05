using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Audio;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Artifacts
{
    public class ArtifactOfProgression : ArtifactBase<ArtifactOfProgression>
    {
        public static ConfigOption<float> ProgressionInterval;
        public static ConfigOption<bool> EnableSounds;
        public static ConfigOption<bool> DoubleGoldAndExpOfProgressions;
        public static ConfigOption<string> BlacklistedEvolutionMastersString;

        public override string ArtifactName => "Artifact of Progression";
        public override string ArtifactLangTokenName => "ARTIFACT_OF_PROGRESSION";
        public override string ArtifactDescription => "Enemies evolve into stronger forms over time.";
        public override Sprite ArtifactEnabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfProgressionEnabledIcon.png");
        public override Sprite ArtifactDisabledIcon => MainAssets.LoadAsset<Sprite>("ArtifactOfProgressionDisabledIcon.png");

        private Dictionary<MasterCatalog.MasterIndex, List<GameObject>> ProgressionCache = new Dictionary<MasterCatalog.MasterIndex, List<GameObject>>();

        private HashSet<string> BlacklistedMasterNames = new HashSet<string>();

        public static BuffDef[] ProgressionBuffs;
        public static NetworkSoundEventDef ProgressionSquelchEvent;

        private List<ProgressionDef> pendingProgressions = new List<ProgressionDef>();
        private struct ProgressionDef { public string From; public string To; }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateAssets();
            CreateArtifact();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            ProgressionInterval = config.ActiveBind<float>("Artifact: " + ArtifactName, "Evolution Interval", 60f, "Seconds until evolution.");
            EnableSounds = config.ActiveBind<bool>("Artifact: " + ArtifactName, "Enable Sounds", true, "Play squelch sound?");
            DoubleGoldAndExpOfProgressions = config.ActiveBind<bool>("Artifact: " + ArtifactName, "Double Rewards", true, "Double gold/xp for evolved forms?");
            BlacklistedEvolutionMastersString = config.ActiveBind<string>("Artifact: " + ArtifactName, "Blacklist", "", "Comma-separated list of master names to ignore.");

            AddProgression("ScorchWurmMaster", "MagmaWormMaster");
            AddProgression("MagmaWormMaster", "ElectricWormMaster");

            //Yes, I know in the lore this is the opposite of their actual life cycle.
            AddProgression("ChildMaster", "ParentMaster");
            AddProgression("ParentMaster", "GrandparentMaster");

            AddProgression("LemurianMaster", "LemurianBruiserMaster");

            AddProgression("BeetleMaster", "BeetleGuardMaster");
            AddProgression("BeetleGuardMaster", "BeetleQueenMaster");

            AddProgression("ImpMaster", "ImpBossMaster");

            AddProgression("GolemMaster", "TitanMaster");

            AddProgression("ClayManMaster", "ClayBruiserMaster");    
            AddProgression("ClayBruiserMaster", "ClayBossMaster");    

            AddProgression("JellyfishMaster", "VagrantMaster");

            AddProgression("WispMaster", "GreaterWispMaster");
            AddProgression("GreaterWispMaster", "ArchWispMaster");

            AddProgression("RoboBallMiniMaster", "RoboBallBossMaster");     

            AddProgression("LunarExploderMaster", "LunarGolemMaster");
            AddProgression("LunarGolemMaster", "LunarWispMaster");

            AddProgression("MinorConstructMaster", "MajorConstructMaster");    

            AddProgression("HalcyoniteMaster", "FalseSonBossMaster");

            AddProgression("VoidBarnacleMaster", "NullifierMaster");
            AddProgression("NullifierMaster", "VoidJailerMaster");
            AddProgression("VoidJailerMaster", "VoidMegaCrabMaster");
        }

        private void AddProgression(string from, string to)
        {
            pendingProgressions.Add(new ProgressionDef { From = from, To = to });
        }

        private void CreateAssets()
        {
            ProgressionSquelchEvent = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            ProgressionSquelchEvent.eventName = "Aetherium_Progression_Squelch";
            ContentAddition.AddNetworkSoundEventDef(ProgressionSquelchEvent);

            ProgressionBuffs = new BuffDef[4];
            string[] names = { "Start", "Quarter", "Half", "Three Quarters" };
            string[] icons = { "ProgressionStartBuffIcon.png", "ProgressionQuarterBuffIcon.png", "ProgressionHalfBuffIcon.png", "ProgressionThreeQuartersBuffIcon.png" };

            for (int i = 0; i < 4; i++)
            {
                var buff = ScriptableObject.CreateInstance<BuffDef>();
                buff.name = $"Aetherium: Progression {names[i]}";
                buff.buffColor = Color.white;
                buff.canStack = false;
                buff.iconSprite = MainAssets.LoadAsset<Sprite>(icons[i]);
                ContentAddition.AddBuffDef(buff);
                ProgressionBuffs[i] = buff;
            }
        }

        public override void Hooks()
        {
            RoR2Application.onLoad += BuildCaches;
            On.RoR2.CharacterAI.BaseAI.OnBodyStart += AttachProgression;
        }

        private void BuildCaches()
        {

            BlacklistedMasterNames.Clear();
            if(!string.IsNullOrWhiteSpace(BlacklistedEvolutionMastersString.ToString()))
            {
                foreach (var s in BlacklistedEvolutionMastersString.ToString().Split(','))
                    BlacklistedMasterNames.Add(s.Trim().ToLowerInvariant());
            }

            ProgressionCache.Clear();
            foreach (var def in pendingProgressions)
            {
                var fromIndex = MasterCatalog.FindMasterIndex(def.From);
                var toPrefab = MasterCatalog.FindMasterPrefab(def.To);

                if(fromIndex != MasterCatalog.MasterIndex.none && toPrefab)
                {
                    if(!ProgressionCache.ContainsKey(fromIndex))
                        ProgressionCache[fromIndex] = new List<GameObject>();

                    ProgressionCache[fromIndex].Add(toPrefab);
                }
            }
            pendingProgressions.Clear();
        }

        private void AttachProgression(On.RoR2.CharacterAI.BaseAI.orig_OnBodyStart orig, RoR2.CharacterAI.BaseAI self, CharacterBody body)
        {
            orig(self, body);

            if(NetworkServer.active && ArtifactEnabled && self.master && body)
            {
                if(self.master.teamIndex == TeamIndex.Player || body.isBoss) return;

                if(BlacklistedMasterNames.Contains(self.master.name.ToLowerInvariant().Replace("(clone)", ""))) return;

                if(ProgressionCache.TryGetValue(self.master.masterIndex, out var options))
                {
                    GameObject evolutionTarget = options[Run.instance.stageRng.RangeInt(0, options.Count)];

                    var component = body.gameObject.AddComponent<ProgressionBehavior>();
                    component.Initialize(evolutionTarget, ProgressionInterval);
                }
            }
        }

        public class ProgressionBehavior : MonoBehaviour
        {
            private CharacterBody body;
            private CharacterMaster master;
            private GameObject targetPrefab;

            private float duration;
            private float timer;

            private int currentStage = 0;
            private float nextThreshold;

            public void Initialize(GameObject target, float totalDuration)
            {
                body = GetComponent<CharacterBody>();
                master = body.master;
                targetPrefab = target;
                duration = totalDuration;

                nextThreshold = duration * 0.25f;
                ApplyStageVisuals(0);
            }

            public void FixedUpdate()
            {
                timer += Time.fixedDeltaTime;

                if(timer >= nextThreshold)
                {
                    AdvanceStage();
                }
            }

            private void AdvanceStage()
            {
                currentStage++;

                if(currentStage >= 4)
                {
                    Evolve();
                    return;
                }

                nextThreshold += (duration * 0.25f);

                ApplyStageVisuals(currentStage);
            }

            private void ApplyStageVisuals(int stageIndex)
            {
                if(stageIndex > 0) body.RemoveBuff(ArtifactOfProgression.ProgressionBuffs[stageIndex - 1]);

                body.AddBuff(ArtifactOfProgression.ProgressionBuffs[stageIndex]);

                Color[] colors = { Color.red, Color.yellow, Color.green, Color.white };
                float[] scales = { 1f, 2f, 3f, 4f };

                EffectData effectData = new EffectData
                {
                    color = colors[stageIndex],
                    origin = body.corePosition,
                    rotation = body.transform.rotation,
                    scale = scales[stageIndex]
                };
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/LevelUpEffectEnemy"), effectData, true);
                EntitySoundManager.EmitSoundServer(ArtifactOfProgression.ProgressionSquelchEvent.akId, body.gameObject);
            }

            private void Evolve()
            {
                enabled = false;

                MasterSummon summon = new MasterSummon
                {
                    masterPrefab = targetPrefab,
                    position = body.corePosition,
                    rotation = body.transform.rotation,
                    summonerBodyObject = body.gameObject,
                    ignoreTeamMemberLimit = true,
                    inventoryToCopy = body.inventory,
                    useAmbientLevel = true
                };

                CharacterMaster newMaster = summon.Perform();

                if(newMaster)
                {
                    CharacterBody newBody = newMaster.GetBody();
                    if(newBody)
                    {
                        newBody.AddTimedBuff(RoR2Content.Buffs.Immune, 2f);

                        if(ArtifactOfProgression.DoubleGoldAndExpOfProgressions)
                        {
                            var rewards = newBody.GetComponent<DeathRewards>();
                            var oldRewards = body.GetComponent<DeathRewards>();
                            if(rewards && oldRewards)
                            {
                                rewards.goldReward = oldRewards.goldReward * 2;
                                rewards.expReward = oldRewards.expReward * 2;
                            }
                        }
                    }
                }

                master.TrueKill();
            }
        }
    }
}