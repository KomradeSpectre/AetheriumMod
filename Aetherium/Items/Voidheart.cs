using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using System;
using KomradeSpectre.Aetherium;
using UnityEngine.Networking;
using RoR2.CharacterAI;
using RoR2.Skills;
using UnityEngine.Rendering;
using EntityStates.NullifierMonster;

namespace Aetherium.Items
{
    class Voidheart : Item<Voidheart>
    {
        public override string displayName => "Heart of the Void";

        public override ItemTier itemTier => RoR2.ItemTier.Lunar;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Cleansable });
        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "On death, cause a void implosion that <style=cIsHealing>revives you if an enemy is killed by it</style> BUT at low health <style=cIsDamage>all healing is converted to damage</style>.";

        protected override string NewLangDesc(string langid = null) => "On death, cause a void implosion with a radius of <style=cIsDamage>15m</style> <style=cStack>(+7.5m per stack)</style> that <style=cIsHealing>revives you if an enemy is killed by it</style> BUT at <style=cIsDamage>30% health</style> <style=cStack>(+5% per stack, max 99%)</style> or lower, <style=cIsDamage>all kinds of healing are converted to damage</style>.";

        protected override string NewLangLore(string langID = null) => "\n[INCIDENT NUMBER 511051]" +
            "\n[TRANSCRIPT TO FOLLOW]" +
            "\nElena: Hey uh...Robert...\n" +
            "\nRobert: Yeah?\n" +
            "\nElena: Remember that heart you told me to pick up from one of those dead crab things?\n" +
            "\nRobert: Yeah? What about it?\n" +
            "\nElena: Did you happen to see...where it went when I touched it a second ago?\n" +
            "\nRobert: Can't say I saw much other than a small flash of light when you did.\n" +
            "\nElena: Hand me the bioscanner really quick.\n" +
            "\n[Humming can be heard for a moment, followed by a loud BEEP.]\n" +
            "\nRobert: ...\n" +
            "\nElena: Robert, I think I know where the heart went. Please call for a medevac. I'm starting to feel unwell.\n" +
            "\nRobert: You'll be fine, we just have a bit more recon to do and we'll be back up on station in no time.\n" +
            "\n[A sound reminiscent of someone patting another on the back can be heard.]\n" +
            "\n[It is followed by a sound reminiscent to an alarm that goes on for a few seconds before it ends in a sharp pop a moment later.]\n" +
            "\n[TRANSCRIPT ENDS]" +
            "\n[ONE SURVIVOR FOUND IN PERFECTLY SPHERICAL CRATER.]";

        public static GameObject ItemBodyModelPrefab;

        public static BuffIndex VoidInstabilityDebuff { get; private set; }
        //public static Dictionary<String, float> ruleLookup = new Dictionary<String, float>();

        public Voidheart()
        {
            modelPathName = "@Aetherium:Assets/Models/Prefabs/Voidheart.prefab";
            iconPathName = "@Aetherium:Assets/Textures/Icons/VoidheartIcon.png";
            onAttrib += (tokenIdent, namePrefix) =>
            {
                var voidInstability = new R2API.CustomBuff(
                    new RoR2.BuffDef
                    {
                        buffColor = Color.magenta,
                        canStack = false,
                        isDebuff = true,
                        name = namePrefix + "VoidInstabilityDebuff",
                        iconPath = "@Aetherium:Assets/Textures/Icons/VoidInstabilityDebuffIcon.png"
                    });
                VoidInstabilityDebuff = R2API.BuffAPI.Add(voidInstability);
            };
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = AetheriumPlugin.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.15f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)

                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.1f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            //ruleLookup.Add("mdlHuntress", 0.1f);
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.5f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.9f, 0.9f, 0.9f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.14f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.11f, 0.11f, 0.11f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.08f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.07f, 0.07f, 0.07f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.09f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.05f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.4f, 0.4f, 0.4f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.1f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.1f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.1f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.11f, 0.11f, 0.11f)
                }
            });
            return rules;
        }

        protected override void LoadBehavior()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = regDef.pickupModelPrefab;  
                regItem.ItemDisplayRules = GenerateItemDisplayRules();

            }

            On.RoR2.CharacterMaster.OnBodyDeath += VoidheartDeathInteraction;
            On.RoR2.HealthComponent.Heal += Voidheart30PercentTimebomb;
            On.RoR2.CharacterBody.FixedUpdate += VoidheartOverlayManager;
            On.RoR2.CharacterBody.Awake += VoidheartPreventionInteraction;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.CharacterMaster.OnBodyDeath -= VoidheartDeathInteraction;
            On.RoR2.HealthComponent.Heal -= Voidheart30PercentTimebomb;
            On.RoR2.CharacterBody.FixedUpdate -= VoidheartOverlayManager;
            On.RoR2.CharacterBody.Awake -= VoidheartPreventionInteraction;
        }

        private void VoidheartDeathInteraction(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, RoR2.CharacterMaster self, RoR2.CharacterBody body)
        {
            var InventoryCount = GetCount(body);
            if(InventoryCount > 0 && !body.healthComponent.killingDamageType.HasFlag(DamageType.VoidDeath) && !body.HasBuff(VoidInstabilityDebuff))
            {
                
                GameObject explosion = new GameObject();
                explosion.transform.position = body.transform.position;
                var componentVoidheartDeath = explosion.AddComponent<VoidheartDeath>();
                componentVoidheartDeath.toReviveMaster = self;
                componentVoidheartDeath.toReviveBody = body;
                componentVoidheartDeath.voidExplosionRadius = 15 + 15 * 0.5f * (InventoryCount-1);
                componentVoidheartDeath.voidInstabilityDebuff = VoidInstabilityDebuff;
                componentVoidheartDeath.Init();
                var tempDestroyOnDeath = self.destroyOnBodyDeath;
                self.destroyOnBodyDeath = false;
                orig(self, body);
                self.destroyOnBodyDeath = tempDestroyOnDeath;
                self.preventGameOver = true;
                return;
            }
            orig(self, body);
        }

        private float Voidheart30PercentTimebomb(On.RoR2.HealthComponent.orig_Heal orig, RoR2.HealthComponent self, float amount, RoR2.ProcChainMask procChainMask, bool nonRegen)
        {
            var InventoryCount = GetCount(self.body);
            if (self.body && InventoryCount > 0)
            {
                if (self.combinedHealth <= self.fullCombinedHealth * Mathf.Clamp((0.3f + (0.05f * InventoryCount - 1)), 0.3f, 0.99f) &&
                    //This check is for the timer to determine time since spawn, at <= 10f it'll only activate after the tenth second
                    self.GetComponent<VoidHeartPrevention>().internalTimer >= 10f)
                {
                    RoR2.DamageInfo damageInfo = new RoR2.DamageInfo();
                    damageInfo.crit = false;
                    damageInfo.damage = amount;
                    damageInfo.force = Vector3.zero;
                    damageInfo.position = self.transform.position;
                    damageInfo.procChainMask = procChainMask;
                    damageInfo.procCoefficient = 0f;
                    damageInfo.damageColorIndex = DamageColorIndex.Default;
                    damageInfo.damageType = DamageType.Generic;
                    self.TakeDamage(damageInfo);
                    return orig(self, 0, procChainMask, nonRegen);
                }
            }
            return orig(self, amount, procChainMask, nonRegen);
        }

        private void VoidheartOverlayManager(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (self.modelLocator && self.modelLocator.modelTransform && self.HasBuff(VoidInstabilityDebuff) && !self.GetComponent<VoidheartCooldown>())
            {
                var Meshes = Voidheart.ItemBodyModelPrefab.GetComponentsInChildren<MeshRenderer>();
                RoR2.TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                overlay.duration = 30;
                overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlay.animateShaderAlpha = true;
                overlay.destroyComponentOnEnd = true;
                overlay.originalMaterial = Meshes[0].material;
                overlay.AddToCharacerModel(self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());
                var VoidheartCooldownTracker = self.gameObject.AddComponent<Voidheart.VoidheartCooldown>();
                VoidheartCooldownTracker.Overlay = overlay;
                VoidheartCooldownTracker.Body = self;
            }
            orig(self);
        }

        public class VoidheartCooldown : MonoBehaviour
        {
            public RoR2.TemporaryOverlay Overlay;
            public RoR2.CharacterBody Body;
            
            public void FixedUpdate()
            {
                if (!Body.HasBuff(VoidInstabilityDebuff))
                {
                    UnityEngine.Object.Destroy(Overlay);
                    UnityEngine.Object.Destroy(this);
                }
            }
        }
        //If I want to update the size of the metaballs in the shader
        /*private void UpdateVoidheartVisual(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            orig(self);
            if (GetCount(self) > 0) 
            {
                var scale = ruleLookup[self.modelLocator.modelTransform.name];
                ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos[0].defaultMaterial.SetFloat("_BlobScale", 3.16f / scale);
                ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos[0].defaultMaterial.SetFloat("_BlobDepth", 2.9f * scale);
                //ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos[0].defaultMaterial.SetFloat("_BlobScale", 3.16f + (1-scale));
                ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos[0].defaultMaterial.SetFloat("_BlobMoveSpeed", 6 * scale);
            }
        }*/

        public class VoidHeartPrevention : MonoBehaviour
        {
            public float internalTimer = 0f;

            private void Update()
            {
                internalTimer += Time.deltaTime;
            }

            public void ResetTimer()
            {
                internalTimer = 0f;
            }
        }

        private void VoidheartPreventionInteraction(On.RoR2.CharacterBody.orig_Awake orig, CharacterBody self)
        {
            //First just run the normal awake stuff
            orig(self);
            //If I somehow lack the Prevention, give me one
            if (!self.gameObject.GetComponent<VoidHeartPrevention>())
            {
                self.gameObject.AddComponent<VoidHeartPrevention>();
            }
            //And reset the timer
            self.gameObject.GetComponent<VoidHeartPrevention>().ResetTimer();
        }
    }
}
