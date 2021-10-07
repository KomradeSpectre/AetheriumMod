using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.Items
{
    public class RicochetTest : ItemBase<RicochetTest>
    {
        public override string ItemName => "Ricochet";

        public override string ItemLangTokenName => "RICOCHET";

        public override string ItemPickupDesc => "";

        public override string ItemFullDescription => "";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => new GameObject();

        public override Sprite ItemIcon => null;

        public static BuffDef RicochetChargeBuff;
        public static BuffDef RicochetCooldown;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateBuffs();
            CreateItem();
            Hooks();
        }

        private void CreateBuffs()
        {
            RicochetChargeBuff = ScriptableObject.CreateInstance<BuffDef>();
            RicochetChargeBuff.name = "Aetherium: Ricochet Charges";
            RicochetChargeBuff.buffColor = new Color(195, 61, 100, 255);
            RicochetChargeBuff.canStack = true;
            RicochetChargeBuff.isDebuff = false;
            RicochetChargeBuff.iconSprite = MainAssets.LoadAsset<Sprite>("ZenithAccelerationBuffIcon.png");

            BuffAPI.Add(new CustomBuff(RicochetChargeBuff));

            RicochetCooldown = ScriptableObject.CreateInstance<BuffDef>();
            RicochetCooldown.name = "Aetherium: Ricochet Cooldown";
            RicochetCooldown.buffColor = new Color(195, 61, 100, 255);
            RicochetCooldown.canStack = true;
            RicochetCooldown.isDebuff = false;
            RicochetCooldown.iconSprite = MainAssets.LoadAsset<Sprite>("ZenithAccelerationBuffIcon.png");

            BuffAPI.Add(new CustomBuff(RicochetCooldown));
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.BulletAttack.ProcessHit += Ricochet;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += ManageRicochetBuffStacks;
        }

        [ConCommand(commandName = "grant_ricochet_stacks", flags = ConVarFlags.ExecuteOnServer, helpText = "Gives Ricochet stacks")]
        private static void GrantRicochetStacks(ConCommandArgs args)
        {
            int? ricochetAmount = args.TryGetArgInt(0);
            if (ricochetAmount.HasValue)
            {
                var body = args.GetSenderBody();
                if (body) 
                {
                    for(int i = 0; i < ricochetAmount; i++)
                    {
                        body.AddBuff(RicochetChargeBuff);
                    }
                }
            }
            else
            {
                Debug.LogError("Can't grant ricochet amount, it's not a valid int!");
            }
        }

        private void ManageRicochetBuffStacks(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            var inventoryCount = GetCount(self);
            if(buffDef == RicochetChargeBuff && inventoryCount > 0)
            {
                self.AddTimedBuff(RicochetCooldown, 10);
            }            
            if(buffDef == RicochetCooldown && inventoryCount > 0)
            {
                self.AddBuff(RicochetChargeBuff);
                for (int i = 0; i < 5; i++)
                {
                    self.AddBuff(RicochetChargeBuff);
                }
            }
            orig(self, buffDef);
        }

        private bool Ricochet(On.RoR2.BulletAttack.orig_ProcessHit orig, RoR2.BulletAttack self, ref RoR2.BulletAttack.BulletHit hitInfo)
        {
            var owner = self.owner;
            if (owner)
            {
                var body = owner.GetComponent<CharacterBody>();
                if (body)
                {
                    var inventoryCount = GetCount(body);
                    if(inventoryCount > 0 && body.GetBuffCount(RicochetChargeBuff) > 0)
                    {
                        self.origin = hitInfo.point;
                        self.aimVector = Vector3.Reflect(hitInfo.direction, hitInfo.surfaceNormal);
                        body.RemoveBuff(RicochetChargeBuff);
                        self.Fire();
                    }
                }
            }
            return orig(self, ref hitInfo);
        }
    }
}
