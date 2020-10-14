using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using static TILER2.MiscUtil;
using System;
using KomradeSpectre.Aetherium;
using Aetherium.Utils;
using UnityEngine.Networking;
using TMPro;
using RoR2.Navigation;
using Generics.Dynamics;
using EntityStates.Engi.EngiWeapon;
using RewiredConsts;

namespace Aetherium.Equipment
{
    class FrenzyPotion : Equipment<FrenzyPotion>
    {
        public override string displayName => "Frenzy Potion";

        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "Nearby characters become hostile to everything around them for a period of time.";

        protected override string NewLangDesc(string langid = null) => $"";

        protected override string NewLangLore(string langID = null) => "";

        public static BuffIndex FrenziedDebuff { get; private set; }

        public FrenzyPotion()
        {
            onAttrib += (tokenIdent, namePrefix) =>
            {
                var frenziedBuff = new R2API.CustomBuff(
                    new RoR2.BuffDef
                    {
                        buffColor = Color.red,
                        canStack = true,
                        isDebuff = true,
                        name = namePrefix + ": Frenzied Debuff",
                        iconPath = "@Aetherium:Assets/Textures/Icons/FeatheredPlumeBuffIcon.png"
                    });
                FrenziedDebuff = R2API.BuffAPI.Add(frenziedBuff);
            };
        }

        protected override void LoadBehavior()
        {
            On.RoR2.CharacterBody.FixedUpdate += CheckFrenzyBuff;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.CharacterBody.FixedUpdate -= CheckFrenzyBuff;
        }

        protected override bool OnEquipUseInner(RoR2.EquipmentSlot slot)
        {
            var body = slot.characterBody;
            if (!body) return false;

            List<Collider> colliders = new List<Collider>();
            new RoR2.SphereSearch
            {
                origin = body.corePosition,
                radius = 10,
                mask = RoR2.LayerIndex.entityPrecise.mask
            }.RefreshCandidates().OrderCandidatesByDistance().GetColliders(colliders);
            foreach (Collider collider in colliders)
            {
                var master = collider.GetComponentInParent<RoR2.CharacterMaster>();
                var targetbody = collider.GetComponentInParent<RoR2.CharacterBody>();
                if (master && targetbody)
                {
                    if (targetbody.teamComponent && !targetbody.GetComponent<FrenzyPotionTracker>())
                    {
                        if(!targetbody.isPlayerControlled)
                        {
                            var FrenzyTracker = targetbody.gameObject.AddComponent<FrenzyPotionTracker>();
                            FrenzyTracker.originalTeam = master.teamIndex;
                            FrenzyTracker.frenziedBody = targetbody;
                            targetbody.AddTimedBuffAuthority(FrenziedDebuff, 20);
                        }
                    }
                }
            }
            return true;

        }

        private void CheckFrenzyBuff(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            var frenzyPotionTracker = self.GetComponent<FrenzyPotionTracker>();
            if (frenzyPotionTracker)
            {
                self.teamComponent.teamIndex = TeamIndex.None;
                self.master.teamIndex = TeamIndex.None;
            }
        }

        public class FrenzyPotionTracker : NetworkBehaviour
        {
            [SyncVar]
            public TeamIndex originalTeam;
            public TeamIndex frenzyTeam;
            public RoR2.CharacterBody frenziedBody;

            public void FixedUpdate()
            {
                if (!frenziedBody.HasBuff(FrenziedDebuff))
                {
                    UnityEngine.Object.Destroy(this);
                }
            }
        }
    }
}
