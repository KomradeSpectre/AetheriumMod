using Aetherium.Utils;
using EntityStates.Merc;
using KomradeSpectre.Aetherium;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TILER2;
using UnityEngine;
using UnityEngine.Networking;
using static TILER2.MiscUtil;

namespace Aetherium.Items
{
    public class BlasterSword : Item_V2<BlasterSword>
    {
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Should the swords impale and stick to targets (true), or pierce and explode on world collision (false)?", AutoConfigFlags.PreventNetMismatch, false, true)]
        public bool useImpaleProjectile { get; private set; } = true;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In percentage, how much of the wielder's damage should we have? (Default: 2 (200%))", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseSwordDamageMultiplier { get; private set; } = 2f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In percentage, how much of the wielder's damage should we add per additional stack? (Default: 0.5 (50%))", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float additionalSwordDamageMultiplier { get; private set; } = 0.5f;

        public override string displayName => "Blaster Sword";

        public override ItemTier itemTier => RoR2.ItemTier.Tier3;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });
        protected override string GetNameString(string langID = null) => displayName;

        protected override string GetPickupString(string langID = null) => $"At <style=cIsHealth>full health</style>, most attacks will <style=cIsDamage>fire out a sword beam that {(useImpaleProjectile ? "impales an enemy, crippling them and exploding shortly after." : "explodes and cripples an enemy on impact.")}</style>";

        protected override string GetDescString(string langid = null) => $"At <style=cIsHealth>full health</style>, most attacks will <style=cIsDamage>fire out a sword beam</style> that has <style=cIsDamage>{Pct(baseSwordDamageMultiplier)} of your damage</style> <style=cStack>(+{Pct(additionalSwordDamageMultiplier)} per stack)</style> " +
            $"when it <style=cIsDamage>{(useImpaleProjectile ? "explodes after having impaled an enemy for a short duration." : "explodes on contact with an enemy.")}</style>";

        protected override string GetLoreString(string langID = null) => "<style=cMono>. . . . . . . . . .</style>\n" +
            "\n<style=cMono>THEY</style> have chosen to <style=cMono>LISTEN</style> to our words.\n" +
            "\n<style=cMono>WE</style> have chosen to <style=cMono>GRANT</style> upon you an exceptional <style=cMono>WEAPON</style> to <style=cMono>UTILIZE</style> your <style=cMono>SOULS TRUE STRENGTH</style>.\n" +
            "\nThe weapon will <style=cMono>ADAPT</style> to fit the needs of the <style=cMono>WIELDER</style>. Once wielded, it is no different than their very soul.\n" +
            "\nShould the <style=cMono>WIELDER</style> survive their journey, they <style=cMono>WILL</style> discard the frail form of what they once were and <style=cMono>ASCEND</style>.\n" +
            "\n<style=cMono>. . . . . . . . . .</style>";

        public static GameObject ItemBodyModelPrefab;

        public static GameObject SwordProjectile;

        public static bool RecursionPrevention;

        public static Xoroshiro128Plus swordRandom = new Xoroshiro128Plus((ulong)System.DateTime.Now.Ticks);

        public static HashSet<String> BlacklistedProjectiles = new HashSet<string>()
        {
            "LightningStake",
            "StickyBomb"
        };

        public static BuffIndex BlasterSwordActiveBuff;


        public BlasterSword()
        {
            modelResourcePath = "@Aetherium:Assets/Models/Prefabs/BlasterSword.prefab";
            iconResourcePath = "@Aetherium:Assets/Textures/Icons/BlasterSwordIcon.png";
        }

        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(modelResourcePath);
                displayRules = GenerateItemDisplayRules();
            }
            base.SetupAttributes();
            var blasterSwordActiveBuff = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = Color.white,
                canStack = false,
                isDebuff = false,
                name = "ATHRMBlaster Sword Active",
                iconPath = "@Aetherium:Assets/Textures/Icons/BlasterSwordBuffIcon.png"
            });
            BlasterSwordActiveBuff = R2API.BuffAPI.Add(blasterSwordActiveBuff);

            //The base of this was provided by Rolo to us.
            SwordProjectile = useImpaleProjectile ? PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/Thermite"), "SwordProjectile", true) : PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/MageIceBolt"), "SwordProjectile", true);

            var model = Resources.Load<GameObject>("@Aetherium:Assets/Models/Prefabs/BlasterSwordProjectile.prefab");
            model.AddComponent<NetworkIdentity>();
            model.AddComponent<RoR2.Projectile.ProjectileGhostController>();

            var controller = SwordProjectile.GetComponent<RoR2.Projectile.ProjectileController>();
            controller.procCoefficient = 0.5f;
            controller.ghostPrefab = model;

            SwordProjectile.GetComponent<RoR2.TeamFilter>().teamIndex = TeamIndex.Player;

            var damage = SwordProjectile.GetComponent<RoR2.Projectile.ProjectileDamage>();
            damage.damageType = DamageType.CrippleOnHit;
            damage.damage = 0;

            var impactExplosion = SwordProjectile.GetComponent<RoR2.Projectile.ProjectileImpactExplosion>();
            impactExplosion.impactEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/VagrantCannonExplosion");
            impactExplosion.blastRadius = 2;
            impactExplosion.blastProcCoefficient = 0.2f;

            if (useImpaleProjectile)
            {
                var stickOnImpact = SwordProjectile.GetComponent<RoR2.Projectile.ProjectileStickOnImpact>();
                stickOnImpact.alignNormals = false;
                impactExplosion.lifetimeAfterImpact = 1.5f;
                impactExplosion.timerAfterImpact = true;
            }

            // register it for networking
            if (SwordProjectile) PrefabAPI.RegisterNetworkPrefab(SwordProjectile);

            // add it to the projectile catalog or it won't work in multiplayer
            RoR2.ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(SwordProjectile);
            };
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<RoR2.ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

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
                    childName = "MuzzleNailgun",
                    localPos = new Vector3(-2.6f, 0.8f, 1.3f),
                    localAngles = new Vector3(60f, 0.8f, -90f),
                    localScale = new Vector3(1f, 1f, 1f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MuzzleNailgun",
                    localPos = new Vector3(-2.6f, 0.8f, -1.3f),
                    localAngles = new Vector3(-60f, 0f, -90f),
                    localScale = new Vector3(1f, 1f, 1f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MuzzleNailgun",
                    localPos = new Vector3(-2.6f, -1.5f, 0f),
                    localAngles = new Vector3(0f, 0f, -90f),
                    localScale = new Vector3(1f, 1f, 1f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MuzzleSpear",
                    localPos = new Vector3(0f, 5.9f, 0f),
                    localAngles = new Vector3(0f, 0f, 180f),
                    localScale = new Vector3(1.5f, 1.5f, 1.5f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MuzzleBuzzsaw",
                    localPos = new Vector3(0f, 1f, 1f),
                    localAngles = new Vector3(0f, 0f, 180f),
                    localScale = new Vector3(2f, 1f, 1.5f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MuzzleBuzzsaw",
                    localPos = new Vector3(0f, 1f, -1f),
                    localAngles = new Vector3(0f, 0f, 180f),
                    localScale = new Vector3(2f, 1f, 1.5f)
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
            rules.Add("mdlBrother", new ItemDisplayRule[]
            {
               new RoR2.ItemDisplayRule
               {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MuzzleLeft",
                    localPos = new Vector3(0, 0, 0f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0f, 0f, 0f)

                },

                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MuzzleRight",
                    localPos = new Vector3(0, 0, 0f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0f, 0f, 0f)

                },

                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "chest",
                    localPos = new Vector3(0, 0.15f, -0.1f),
                    localAngles = new Vector3(90, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)

                }
            });
            return rules;
        }

        public override void Install()
        {
            base.Install();

            On.RoR2.CharacterBody.FixedUpdate += ApplyBuffAsIndicatorForReady;
            IL.EntityStates.Merc.Evis.FixedUpdate += Anime;
            On.RoR2.Orbs.GenericDamageOrb.Begin += FireSwordOnOrbs;
            On.RoR2.OverlapAttack.Fire += FireSwordOnMelee;
            On.RoR2.BulletAttack.Fire += FireTheSwordOnBulletAttack;
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += FireTheSwordOnProjectiles;
        }

        private void Anime(ILContext il)
        {
            var c = new ILCursor(il);

            int damageInfoIndex = 4;
            c.GotoNext(x => x.MatchNewobj<DamageInfo>(), x => x.MatchStloc(out damageInfoIndex));
            c.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt<GlobalEventManager>("OnHitAll"));
            c.Emit(OpCodes.Ldloc, damageInfoIndex);
            c.EmitDelegate<Action<DamageInfo>>((damageInfo) =>
            {
                if (damageInfo.attacker)
                {
                    var body = damageInfo.attacker.GetComponent<CharacterBody>();
                    if (body)
                    {
                        var InventoryCount = GetCount(body);
                        if (InventoryCount > 0)
                        {
                            if (body.healthComponent.combinedHealthFraction >= 1)
                            {
                                var newProjectileInfo = new FireProjectileInfo();
                                newProjectileInfo.owner = body.gameObject;
                                newProjectileInfo.projectilePrefab = SwordProjectile;
                                newProjectileInfo.speedOverride = 100.0f;
                                newProjectileInfo.damage = body.damage * baseSwordDamageMultiplier + (body.damage * additionalSwordDamageMultiplier * (InventoryCount - 1));
                                newProjectileInfo.damageTypeOverride = null;
                                newProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                                newProjectileInfo.procChainMask = default(RoR2.ProcChainMask);
                                var positionChosen = damageInfo.position + new Vector3(swordRandom.RangeFloat(-10, 10), swordRandom.RangeFloat(0, 10), swordRandom.RangeFloat(-10, 10)).normalized * 4;
                                newProjectileInfo.position = positionChosen;
                                newProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(damageInfo.position - positionChosen);

                                try
                                {
                                    RecursionPrevention = true;
                                    RoR2.Projectile.ProjectileManager.instance.FireProjectile(newProjectileInfo);
                                }
                                finally
                                {
                                    RecursionPrevention = false;
                                }
                            }
                        }
                    }
                }
            });
        }

        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.CharacterBody.FixedUpdate -= ApplyBuffAsIndicatorForReady;
            IL.EntityStates.Merc.Evis.FixedUpdate -= Anime;
            On.RoR2.Orbs.GenericDamageOrb.Begin -= FireSwordOnOrbs;
            On.RoR2.OverlapAttack.Fire -= FireSwordOnMelee;
            On.RoR2.BulletAttack.Fire -= FireTheSwordOnBulletAttack;
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo -= FireTheSwordOnProjectiles;
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
            else
            {
                if (self.HasBuff(BlasterSwordActiveBuff))
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
                            newProjectileInfo.damage = ownerBody.damage * baseSwordDamageMultiplier + (ownerBody.damage * additionalSwordDamageMultiplier * (InventoryCount - 1));
                            newProjectileInfo.damageTypeOverride = null;
                            newProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                            newProjectileInfo.procChainMask = default(RoR2.ProcChainMask);
                            newProjectileInfo.position = self.origin;
                            newProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(self.target.transform.position - self.origin);

                            try
                            {
                                RecursionPrevention = true;
                                RoR2.Projectile.ProjectileManager.instance.FireProjectile(newProjectileInfo);
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
                                newProjectileInfo.damage = body.damage * baseSwordDamageMultiplier + (body.damage * additionalSwordDamageMultiplier * (InventoryCount - 1));
                                newProjectileInfo.damageTypeOverride = null;
                                newProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                                newProjectileInfo.procChainMask = default(ProcChainMask);
                                newProjectileInfo.position = HitPositionSums;
                                newProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(inputBank ? inputBank.aimDirection : body.transform.forward);

                                try
                                {
                                    RecursionPrevention = true;
                                    RoR2.Projectile.ProjectileManager.instance.FireProjectile(newProjectileInfo);
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

        private void FireTheSwordOnBulletAttack(On.RoR2.BulletAttack.orig_Fire orig, RoR2.BulletAttack self)
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
                            newProjectileInfo.damage = projectileBody.damage * baseSwordDamageMultiplier + (projectileBody.damage * additionalSwordDamageMultiplier * (InventoryCount - 1));
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
                                RoR2.Projectile.ProjectileManager.instance.FireProjectile(newProjectileInfo);
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

        private void FireTheSwordOnProjectiles(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, RoR2.Projectile.ProjectileManager self, FireProjectileInfo fireProjectileInfo)
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
                                newProjectileInfo.damage = body.damage * baseSwordDamageMultiplier + (body.damage * additionalSwordDamageMultiplier * (InventoryCount - 1));
                                newProjectileInfo.damageTypeOverride = null;
                                newProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                                newProjectileInfo.procChainMask = default(RoR2.ProcChainMask);

                                try
                                {
                                    RecursionPrevention = true;
                                    RoR2.Projectile.ProjectileManager.instance.FireProjectile(newProjectileInfo);
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
