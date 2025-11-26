using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Aetherium.Utils
{
    public static class ExtensionMethods
    {
        public static void FilterElites(this BullseyeSearch search)
        {
            if (search.candidatesEnumerable.Any())
            {
                search.candidatesEnumerable = (List<BullseyeSearch.CandidateInfo>)search.candidatesEnumerable.Where(x => x.hurtBox && x.hurtBox.IsHurtboxAnElite());
            }
        }

        public static void FilterOutItemWielders(this BullseyeSearch search, ItemDef item)
        {
            if (search.candidatesEnumerable.Any())
            {
                search.candidatesEnumerable = (List<BullseyeSearch.CandidateInfo>)search.candidatesEnumerable.Where(x => x.hurtBox && !x.hurtBox.DoesHurtboxHaveItem(item));
            }
        }

        public static void FilterOutItemWielders(this BullseyeSearch search, List<ItemDef> items)
        {
            List<BullseyeSearch.CandidateInfo> temporaryList = search.candidatesEnumerable.ToList();

            if (temporaryList.Any())
            {
                foreach(ItemDef item in items)
                {
                    temporaryList.RemoveAll(x => x.hurtBox && x.hurtBox.DoesHurtboxHaveItem(item));
                }

                search.candidatesEnumerable = temporaryList;
            }        
        }

        public static bool DoesHurtboxHaveItem(this HurtBox hurtbox, ItemDef item)
        {
            if (!hurtbox.healthComponent || !hurtbox.healthComponent.body || !item)
            {
                AetheriumPlugin.ModLogger.LogError("Can't check if the hurtbox has the item, some information is missing!");
                return false;
            }

            var body = hurtbox.healthComponent.body;
            if(body.inventory.GetItemCount(item) > 0)
            {
                return true;
            }

            return false;
        }

        public static bool IsHurtboxAnElite(this HurtBox hurtbox)
        {
            if (!hurtbox.healthComponent || !hurtbox.healthComponent.body)
            {
                AetheriumPlugin.ModLogger.LogError("Can't check if the hurtbox is an elite, some information is missing!");
                return false;
            }

            return hurtbox.healthComponent.body.isElite;
        }

        public static void ApplyForceImpulseFixed(this CharacterMotor characterMotor, in PhysForceInfo physForceInfo)
        {
            if (NetworkServer.active && !characterMotor.hasEffectiveAuthority)
            {
                characterMotor.CallRpcApplyForceImpulse(physForceInfo);
            }
            else
            {
                Vector3 force = physForceInfo.force;
                if (!physForceInfo.massIsOne)
                {
                    force *= 1f / characterMotor.mass;
                }

                if (characterMotor.mass == 0.0)
                {
                    return;
                }                    

                if (force.y < 6.0 && characterMotor.isGrounded && !physForceInfo.ignoreGroundStick)
                {
                    force.y = 0.0f;
                }
                if (force.y > 0.0)
                {
                    characterMotor.Motor.ForceUnground();
                }
                characterMotor.velocity += force;
                if (!physForceInfo.disableAirControlUntilCollision)
                {
                    return;
                }
                characterMotor.disableAirControlUntilCollision = true;
            }
        }
    }
}
