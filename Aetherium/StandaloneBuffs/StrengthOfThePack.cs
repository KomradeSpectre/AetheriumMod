using Aetherium.Utils;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static Aetherium.AetheriumPlugin;

namespace Aetherium.StandaloneBuffs
{
    public class StrengthOfThePack : BuffBase<StrengthOfThePack>
    {
        public static ConfigOption<float> MinimumDistanceToAllyRequired;

        public override string BuffName => "Strength of the Pack";

        public override Color Color => new Color(255, 165, 0, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("BlasterSwordBuffIcon.png");

        public BuffDef WolvenPower;
        public BuffDef WolvenVengeance;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateRequiredTemporaryBuffs();
            CreateBuff();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            MinimumDistanceToAllyRequired = config.ActiveBind<float>("Buff: " + BuffName, "Minimum Ally Distance for Effect Activation", 10, "What distance from an ally with this buff do you have to be within to receive bonuses from it?");
        }

        private void CreateRequiredTemporaryBuffs()
        {
            WolvenVengeance = ScriptableObject.CreateInstance<RoR2.BuffDef>();
            WolvenVengeance.name = "Strength of the Pack : Power";
            WolvenVengeance.buffColor = Color.white;
            WolvenVengeance.canStack = true;
            WolvenVengeance.isDebuff = false;
            WolvenVengeance.iconSprite = MainAssets.LoadAsset<Sprite>("BlasterSwordBuffIcon.png");

            ContentAddition.AddBuffDef(WolvenVengeance);

            WolvenPower = ScriptableObject.CreateInstance<RoR2.BuffDef>();
            WolvenPower.name = "Strength of the Pack : Endurance";
            WolvenPower.buffColor = Color.white;
            WolvenPower.canStack = true;
            WolvenPower.isDebuff = false;
            WolvenPower.iconSprite = MainAssets.LoadAsset<Sprite>("BlasterSwordBuffIcon.png");

            ContentAddition.AddBuffDef(WolvenPower);
        }

        public List<TeamComponent> TeammatesWithStrengthOfThePack(CharacterBody body)
        {
            if(!body || !body.teamComponent) { return null; }

            var matchedTeamMembers = TeamComponent.GetTeamMembers(body.teamComponent.teamIndex).Where(x => x.body && x.body != body && x.body.HasBuff(BuffDef) &&
                Vector3.Distance(body.transform.position, x.body.transform.position) < MinimumDistanceToAllyRequired).ToList();

            return matchedTeamMembers;
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += GrantWolvenPowerStacks;
            On.RoR2.HealthComponent.TakeDamage += GrantWolvenVengeanceStacks;
            R2API.RecalculateStatsAPI.GetStatCoefficients += AddWolvenBonuses;
        }

        private void AddWolvenBonuses(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender)
            {
                if (sender.HasBuff(WolvenPower))
                {
                    args.damageMultAdd += 0.05f * sender.GetBuffCount(WolvenPower);
                }

                if (sender.HasBuff(WolvenVengeance))
                {
                    args.attackSpeedMultAdd += 0.05f * sender.GetBuffCount(WolvenVengeance);
                }
            }
        }

        private void GrantWolvenVengeanceStacks(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);

            if (self && self.body && self.body.HasBuff(BuffDef) && damageInfo.attacker)
            {
                var body = self.body;
                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (body && body.teamComponent && attackerBody)
                {
                    var enemyTeams = TeamComponent.GetTeamMembers(body.teamComponent.teamIndex);
                    if(!enemyTeams.Any(x => x.teamIndex == attackerBody.teamComponent.teamIndex)) { return; }

                    var teamMembers = TeammatesWithStrengthOfThePack(body);

                    if (teamMembers.Count > 0)
                    {
                        if (body.GetBuffCount(WolvenVengeance) > 0) { ItemHelpers.RefreshTimedBuffs(body, WolvenVengeance, 5); }

                        body.AddTimedBuff(WolvenVengeance, 5);
                    }

                    foreach (TeamComponent teamComponent in teamMembers)
                    {
                        if (!teamComponent.body.healthComponent || teamComponent.body.healthComponent && !teamComponent.body.healthComponent.alive) { continue; }

                        if (teamComponent.body.GetBuffCount(WolvenVengeance) > 0) { ItemHelpers.RefreshTimedBuffs(teamComponent.body, WolvenVengeance, 5); }

                        teamComponent.body.AddTimedBuff(WolvenVengeance, 5);
                    }
                }
            }
        }

        private void GrantWolvenPowerStacks(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);

            if (damageReport.attackerBody && damageReport.attackerBody.HasBuff(BuffDef))
            {
                var teamMembers = TeammatesWithStrengthOfThePack(damageReport.attackerBody);

                if(teamMembers.Count > 0)
                {
                    if (damageReport.attackerBody.GetBuffCount(WolvenPower) > 0) { ItemHelpers.RefreshTimedBuffs(damageReport.attackerBody, WolvenPower, 5); }

                    damageReport.attackerBody.AddTimedBuff(WolvenPower, 5);
                }

                foreach(TeamComponent teamComponent in teamMembers)
                {
                    if(!teamComponent.body.healthComponent || teamComponent.body.healthComponent && !teamComponent.body.healthComponent.alive) { continue; }

                    if(teamComponent.body.GetBuffCount(WolvenPower) > 0) { ItemHelpers.RefreshTimedBuffs(teamComponent.body, WolvenPower, 5); }

                    teamComponent.body.AddTimedBuff(WolvenPower, 5);
                }
            }
        }
    }
}
