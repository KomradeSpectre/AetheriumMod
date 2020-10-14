using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using static TILER2.MiscUtil;
using System;
using KomradeSpectre.Aetherium;
using UnityEngine.Networking;
using RoR2.Projectile;
using RewiredConsts;
using System.Linq;
using EntityStates;
using Mono.Cecil;
using MonoMod.RuntimeDetour;

namespace Aetherium.Items
{
    class BlasterSword : Item<BlasterSword>
    {
        public override string displayName => "Blaster Sword";

        public override ItemTier itemTier => RoR2.ItemTier.Tier3;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });
        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "";

        protected override string NewLangDesc(string langid = null) => $"";

        protected override string NewLangLore(string langID = null) => "";

        public static GameObject ItemBodyModelPrefab;

        public static GameObject SwordProjectile;

        public static bool RecursionPrevention;

        public static HashSet<String> BlacklistedProjectiles = new HashSet<string>()
        {
            "LightningStake",
            "StickyBomb"
        };

        public static BuffIndex BlasterSwordActiveBuff;


        public BlasterSword()
        {
            modelPathName = "@Aetherium:Assets/Models/Prefabs/BlasterSword.prefab";
            iconPathName = "@Aetherium:Assets/Textures/Icons/BlasterSwordIcon.png";

            onAttrib += (tokenIdent, namePrefix) =>
            {
                var blasterSwordActiveBuff = new R2API.CustomBuff(
                    new RoR2.BuffDef
                    {
                        buffColor = Color.white,
                        canStack = false,
                        isDebuff = false,
                        name = namePrefix + ": Blaster Sword Active",
                        iconPath = "@Aetherium:Assets/Textures/Icons/BlasterSwordBuffIcon.png"
                    });
                BlasterSwordActiveBuff = R2API.BuffAPI.Add(blasterSwordActiveBuff);
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
                    childName = "MuzzleLeft",
                    localPos = new Vector3(0, 0, 0.4f),
                    localAngles = new Vector3(-90, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)

                },

                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MuzzleRight",
                    localPos = new Vector3(0, 0, 0.4f),
                    localAngles = new Vector3(-90, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)

                }

            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Arrow",
                    localPos = new Vector3(0.3f, 0f, 0f),
                    localAngles = new Vector3(90f, 270f, 0f),
                    localScale = new Vector3(0.08f, 0.045f, 0.1f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, -0.3f, 1.6f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(4f, 4f, 4f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CannonHeadL",
                    localPos = new Vector3(0f, 1.2f, 0f),
                    localAngles = new Vector3(-180f, 45f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                },

                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CannonHeadR",
                    localPos = new Vector3(0f, 1.2f, 0f),
                    localAngles = new Vector3(-180f, -45f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmL",
                    localPos = new Vector3(0.1f, 0.2f, 0),
                    localAngles = new Vector3(0f, 90f, 190f),
                    localScale = new Vector3(0.07f, 0.025f, 0.07f)
                },

                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmR",
                    localPos = new Vector3(-0.1f, 0.2f, 0),
                    localAngles = new Vector3(0f, -90f, -190f),
                    localScale = new Vector3(0.07f, 0.025f, 0.07f)
                }

            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HandR",
                    localPos = new Vector3(0.67f, 0.28f, 0.01f),
                    localAngles = new Vector3(0f, 0f, 100f),
                    localScale = new Vector3(0.11f, 0.11f, 0.11f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(-0.3f, 1.6f, 0f),
                    localAngles = new Vector3(0f, 0f, 15f),
                    localScale = new Vector3(0.3f, 0.3f, 0.3f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MechHandL",
                    localPos = new Vector3(0.6f, 0.25f, 0.02f),
                    localAngles = new Vector3(20f, -4f, 90f),
                    localScale = new Vector3(0.15f, 0.1f, 0.15f)
                },

                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MechHandR",
                    localPos = new Vector3(-0.6f, 0.25f, 0.02f),
                    localAngles = new Vector3(20f, 4f, -90f),
                    localScale = new Vector3(0.15f, 0.1f, 0.15f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MouthMuzzle",
                    localPos = new Vector3(-9.2f, 2f, 3f),
                    localAngles = new Vector3(90f, 90f, 0f),
                    localScale = new Vector3(1.5f, 1.5f, 1.5f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MuzzleGun",
                    localPos = new Vector3(0f, 0f, 0.65f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = new Vector3(0.12f, 0.12f, 0.12f)
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

            SwordProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/MageIceBolt"), "SwordProjectile", true);

            var model = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/BlasterSwordProjectile.prefab");
            model.AddComponent<NetworkIdentity>();
            model.AddComponent<ProjectileGhostController>();

            var controller = SwordProjectile.GetComponent<ProjectileController>();
            controller.procCoefficient = 0.5f;
            controller.ghostPrefab = model;

            SwordProjectile.GetComponent<RoR2.TeamFilter>().teamIndex = TeamIndex.Player;

            var damage = SwordProjectile.GetComponent<ProjectileDamage>();
            damage.damageType = DamageType.BonusToLowHealth;
            damage.damage = 0;

            var impactExplosion = SwordProjectile.GetComponent<ProjectileImpactExplosion>();
            impactExplosion.impactEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/VagrantCannonExplosion");

            // register it for networking
            if (SwordProjectile) PrefabAPI.RegisterNetworkPrefab(SwordProjectile);

            // add it to the projectile catalog or it won't work in multiplayer
            RoR2.ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(SwordProjectile);
            };

            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += FireTheSword;
            On.RoR2.BulletAttack.Fire += FireTheSwordOnTheJankEvent;
            On.RoR2.OverlapAttack.Fire += FireSwordOnMelee;
            On.RoR2.Orbs.GenericDamageOrb.Begin += FireSwordOnOrbs;
            On.RoR2.CharacterBody.FixedUpdate += ApplyBuffAsIndicatorForReady;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo -= FireTheSword;
            On.RoR2.BulletAttack.Fire -= FireTheSwordOnTheJankEvent;
            On.RoR2.OverlapAttack.Fire -= FireSwordOnMelee;
            On.RoR2.Orbs.GenericDamageOrb.Begin -= FireSwordOnOrbs;
            On.RoR2.CharacterBody.FixedUpdate -= ApplyBuffAsIndicatorForReady;
        }

        private void ApplyBuffAsIndicatorForReady(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            var InventoryCount = GetCount(self);
            if (InventoryCount > 0)
            {
                if (self.healthComponent.combinedHealthFraction >= 1 && !self.HasBuff(BlasterSwordActiveBuff))
                {
                    self.AddBuff(BlasterSwordActiveBuff);
                }
                if (self.healthComponent.combinedHealthFraction < 1 && self.HasBuff(BlasterSwordActiveBuff))
                {
                    self.RemoveBuff(BlasterSwordActiveBuff);
                }
            }
            orig(self);
        }

        private void FireSwordOnOrbs(On.RoR2.Orbs.GenericDamageOrb.orig_Begin orig, RoR2.Orbs.GenericDamageOrb self)
        {
            var owner = self.attacker;
            if (owner)
            {
                var ownerBody = owner.GetComponent<RoR2.CharacterBody>();
                if (ownerBody)
                {
                    var InventoryCount = GetCount(ownerBody);
                    if (InventoryCount > 0)
                    {
                        if (ownerBody.healthComponent.combinedHealthFraction >= 1)
                        {
                            var newProjectileInfo = new FireProjectileInfo();
                            newProjectileInfo.owner = owner;
                            newProjectileInfo.projectilePrefab = SwordProjectile;
                            newProjectileInfo.speedOverride = 100.0f;
                            newProjectileInfo.damage = ownerBody.damage * 2;
                            newProjectileInfo.damageTypeOverride = null;
                            newProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                            newProjectileInfo.procChainMask = default(RoR2.ProcChainMask);
                            newProjectileInfo.position = self.origin;
                            newProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(self.target.transform.position - self.origin);

                            try
                            {
                                RecursionPrevention = true;
                                ProjectileManager.instance.FireProjectile(newProjectileInfo);
                            }
                            finally
                            {
                                RecursionPrevention = false;
                            }
                        }
                    }
                }
            }
            orig(self);
        }

        private bool FireSwordOnMelee(On.RoR2.OverlapAttack.orig_Fire orig, RoR2.OverlapAttack self, List<RoR2.HealthComponent> hitResults)
        {
            var owner = self.inflictor;
            if (owner)
            {
                var body = owner.GetComponent<RoR2.CharacterBody>();
                if (body)
                {
                    var InventoryCount = GetCount(body);
                    if (InventoryCount > 0)
                    {
                        if (body.healthComponent.combinedHealthFraction >= 1)
                        {
                            Vector3 HitPositionSums = Vector3.zero;
                            if (self.overlapList.Count > 0)
                            {

                                for (int i = 0; i < self.overlapList.Count; i++)
                                {
                                    HitPositionSums += self.overlapList[i].hitPosition;
                                }

                                HitPositionSums /= self.overlapList.Count;
                            }
                            else
                            {
                                HitPositionSums += body.corePosition;
                            }
                            var inputBank = body.inputBank;

                            var cooldownHandler = owner.GetComponent<SwordCooldownHandlerIDunno>();
                            if (!cooldownHandler) { cooldownHandler = owner.AddComponent<SwordCooldownHandlerIDunno>(); }

                            if (!cooldownHandler.MeleeTracker.ContainsKey(self))
                            {
                                cooldownHandler.MeleeTracker.Add(self, 0);
                                var newProjectileInfo = new FireProjectileInfo();
                                newProjectileInfo.owner = self.inflictor;
                                newProjectileInfo.projectilePrefab = SwordProjectile;
                                newProjectileInfo.speedOverride = 100.0f;
                                newProjectileInfo.damage = body.damage * 2;
                                newProjectileInfo.damageTypeOverride = null;
                                newProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                                newProjectileInfo.procChainMask = default(RoR2.ProcChainMask);
                                newProjectileInfo.position = HitPositionSums;
                                newProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(inputBank ? inputBank.aimDirection : body.transform.forward);

                                try
                                {
                                    RecursionPrevention = true;
                                    ProjectileManager.instance.FireProjectile(newProjectileInfo);
                                }
                                finally
                                {
                                    RecursionPrevention = false;
                                }
                            }
                        }
                    }
                }
            }
            return orig(self, hitResults);
        }

        private void FireTheSwordOnTheJankEvent(On.RoR2.BulletAttack.orig_Fire orig, RoR2.BulletAttack self)
        {
            var projectileOwner = self.owner;
            if (projectileOwner)
            {
                var projectileBody = projectileOwner.GetComponent<CharacterBody>();
                if (projectileBody)
                {
                    var InventoryCount = GetCount(projectileBody);
                    if (InventoryCount > 0)
                    {
                        if (projectileBody.healthComponent.combinedHealthFraction >= 1)
                        {
                            var newProjectileInfo = new FireProjectileInfo();
                            newProjectileInfo.owner = projectileOwner;
                            newProjectileInfo.projectilePrefab = SwordProjectile;
                            newProjectileInfo.speedOverride = 100.0f;
                            newProjectileInfo.damage = projectileBody.damage * 2;
                            newProjectileInfo.damageTypeOverride = null;
                            newProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                            newProjectileInfo.procChainMask = default(RoR2.ProcChainMask);

                            Vector3 MuzzleTransform = self.origin;
                            var weapon = self.weapon;
                            if (weapon)
                            {
                                var weaponModelLocator = weapon.GetComponent<ModelLocator>();
                                if (weaponModelLocator && weaponModelLocator.transform)
                                {
                                    ChildLocator childLocator = weaponModelLocator.modelTransform.GetComponent<ChildLocator>();
                                    if (childLocator)
                                    {
                                        if (self.muzzleName != null)
                                        {
                                            MuzzleTransform = childLocator.FindChild(self.muzzleName).position;
                                        }
                                    }
                                }

                            }
                            newProjectileInfo.position = MuzzleTransform;
                            newProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(self.aimVector);

                            try
                            {
                                RecursionPrevention = true;
                                ProjectileManager.instance.FireProjectile(newProjectileInfo);
                            }
                            finally
                            {
                                RecursionPrevention = false;
                            }
                        }
                    }
                }
            }
            orig(self);
        }

        private void FireTheSword(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, RoR2.Projectile.ProjectileManager self, FireProjectileInfo fireProjectileInfo)
        {
            if (!RecursionPrevention && !BlacklistedProjectiles.Contains(fireProjectileInfo.projectilePrefab.name))
            {
                var projectileOwner = fireProjectileInfo.owner;
                if (projectileOwner)
                {
                    var body = projectileOwner.GetComponent<RoR2.CharacterBody>();
                    if (body)
                    {
                        var InventoryCount = GetCount(body);
                        if (InventoryCount > 0)
                        {
                            if (body.healthComponent.combinedHealthFraction >= 1)
                            {
                                var newProjectileInfo = fireProjectileInfo;
                                newProjectileInfo.owner = projectileOwner;
                                newProjectileInfo.projectilePrefab = SwordProjectile;
                                newProjectileInfo.speedOverride = 100.0f;
                                newProjectileInfo.damage = body.damage * 2;
                                newProjectileInfo.damageTypeOverride = null;
                                newProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                                newProjectileInfo.procChainMask = default(RoR2.ProcChainMask);

                                try
                                {
                                    RecursionPrevention = true;
                                    ProjectileManager.instance.FireProjectile(newProjectileInfo);
                                }
                                finally
                                {
                                    RecursionPrevention = false;
                                }
                            }
                        }
                    }
                }
            }
            orig(self, fireProjectileInfo);
        }

        public class SwordCooldownHandlerIDunno : MonoBehaviour
        {
            public Dictionary<RoR2.OverlapAttack, float> MeleeTracker = new Dictionary<RoR2.OverlapAttack, float>();
            public void FixedUpdate()
            {
                foreach (RoR2.OverlapAttack attack in MeleeTracker.Keys.ToList())
                {
                    var time = MeleeTracker[attack];
                    time += Time.fixedDeltaTime;

                    if (time > 5)
                    {
                        MeleeTracker.Remove(attack);
                    }
                    else
                    {
                        MeleeTracker[attack] = time;
                    }
                }
            }
        }
    }
}
