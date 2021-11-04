using Aetherium.Interactables;
using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Aetherium.MyEntityStates.BuffBrazier
{
    public class BuffBrazierMainState : EntityState
    {
        public BuffBrazierManager BuffBrazierManager;
        public float HurtInterval = 0.3f;
        public float Stopwatch;
        public override void OnEnter()
        {
            base.OnEnter();
            AkSoundEngine.PostEvent(760500611, gameObject.transform.Find("Fire").gameObject);

            var buffBrazierManager = gameObject.GetComponent<BuffBrazierManager>();
            if (buffBrazierManager)
            {
                BuffBrazierManager = buffBrazierManager;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active)
            {
                Stopwatch += Time.fixedDeltaTime;
                if (Stopwatch >= HurtInterval)
                {
                    RoR2.HurtBox[] hurtBoxes = new RoR2.SphereSearch
                    {
                        radius = 1.5f,
                        mask = RoR2.LayerIndex.entityPrecise.mask,
                        origin = gameObject.transform.Find("Fire").position
                    }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                    foreach (HurtBox hurtbox in hurtBoxes)
                    {
                        var healthComponent = hurtbox.healthComponent;
                        if (healthComponent)
                        {
                            DamageInfo damageInfo = new DamageInfo()
                            {
                                damage = healthComponent.fullCombinedHealth * 0.03f,
                                damageType = DamageType.IgniteOnHit,
                                damageColorIndex = DamageColorIndex.Default,
                                inflictor = gameObject,
                                position = healthComponent.gameObject.transform.position,
                            };
                            healthComponent.TakeDamage(damageInfo);

                            if (BuffBrazierManager.ChosenBuffBrazierBuff.BuffDef)
                            {
                                healthComponent.body.AddTimedBuff(BuffBrazierManager.ChosenBuffBrazierBuff.BuffDef, HurtInterval + 0.1f);
                            }
                        }
                    }
                    Stopwatch = 0;
                }
            }

        }
    }
}
