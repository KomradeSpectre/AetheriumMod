using Aetherium.Effect;
using Aetherium.Utils;
using BepInEx.Configuration;
using EntityStates;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Aetherium.Equipment
{
    public class PheromoneSac : EquipmentBase<PheromoneSac>
    {
        public static ConfigEntry<float> BaseRadiusGranted;
        public static ConfigEntry<float> ProjectileAbsorptionTime;
        public static ConfigEntry<float> JarCooldown;
        public static ConfigEntry<bool> IWantToLoseFriendsInChaosMode;

        public override string EquipmentName => "Pheromone Sac";

        public override string EquipmentLangTokenName => "PHEROMONE_SAC";

        public override string EquipmentPickupDesc => "On activation, release a cloud of pheromones that frenzy enemies caught inside of it.";

        public override string EquipmentFullDescription => $"On activation, <style=cIsUtility>absorb projectiles</style> in a <style=cIsUtility>{BaseRadiusGranted.Value}m</style> radius for <style=cIsUtility>{ProjectileAbsorptionTime.Value}</style> second(s). " +
            $"Upon success, <style=cIsDamage>fire all of the projectiles out of the jar</style> upon next activation. " +
            $"The damage traits of each projectiles fired from the jar depends on the <style=cIsDamage>bullets you absorbed</style>. " +
            $"After all the projectiles have been fired from the jar, it will need to cool down.";

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

        public override string EquipmentModelPath => "@Aetherium:Assets/Models/Prefabs/Equipment/JarOfReshaping/JarOfReshaping.prefab";

        public override string EquipmentIconPath => "@Aetherium:Assets/Textures/Icons/Equipment/JarOfReshapingIcon.png";

        public override float Cooldown => JarCooldown.Value;

        public static GameObject ItemBodyModelPrefab;

        public static GameObject JarProjectile;

        public static GameObject JarOrb;

        public static GameObject JarChargeSphere;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = Resources.Load<GameObject>(EquipmentModelPath);
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
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
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
        }
    }
}