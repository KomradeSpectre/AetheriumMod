using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using RoR2;
using static Aetherium.Survivors.Koalesk;
using static Aetherium.AetheriumPlugin;
using UnityEngine;
using System.Linq;

namespace Aetherium.MyEntityStates.Survivors.Koalesk
{
    internal class KoaleskMainState : GenericCharacterMain
    {

        public bool HasGrantedBuff = false;

        public override void OnEnter()
        {
            base.OnEnter();
            if (characterBody)
            {
                if (!characterBody.GetComponent<DarkThornTimingAdjuster>())
                {
                    characterBody.gameObject.AddComponent<DarkThornTimingAdjuster>();
                }

                if (!characterBody.GetComponent<RoseThornTimingAdjuster>())
                {
                    characterBody.gameObject.AddComponent<RoseThornTimingAdjuster>();
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (inputBank.skill4.down)
            {
                characterBody.SetBuffCount(Aetherium.Survivors.Koalesk.BloodliquorBuff.buffIndex, 1000);
            }

            if (inputBank && characterBody && characterBody.skillLocator)
            {
                var skillLocator = characterBody.skillLocator;
                var movementInputDot = Vector3.Dot(inputBank.moveVector.normalized, aimDirection.normalized);

                // Define a small threshold value, like 0.1
                float threshold = 0.1f;

                if (movementInputDot >= -threshold)
                {
                    if (skillLocator.primary)
                    {
                        skillLocator.primary.UnsetSkillOverride(characterBody, KoaleskDarkThorn, GenericSkill.SkillOverridePriority.Replacement);
                    }
                }
                else
                {
                    if (skillLocator.primary)
                    {
                        skillLocator.primary.SetSkillOverride(characterBody, KoaleskDarkThorn, GenericSkill.SkillOverridePriority.Replacement);
                    }
                }
            }
        }

        public class RoseThornTimingAdjuster : MonoBehaviour
        {
            public float baseDuration = 1f;
            public float attackStartTime = 0.35f;
            public float attackEndTime = 0.36f;
            public float baseEarlyExitTime = 1f;
        }

        public class DarkThornTimingAdjuster : MonoBehaviour
        {
            public float baseDuration = 1f;
            public float attackStartTime = 17f / 60;
            public float attackEndTime = 18f / 60;
            public float baseEarlyExitTime = 0.75f;
        }
    }
}
