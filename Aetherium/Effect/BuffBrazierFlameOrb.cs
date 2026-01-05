using Aetherium.Equipment;
using Aetherium.Interactables;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace Aetherium.Effect
{
    public class BuffBrazierFlameOrb : Orb
    {
        public GameObject Target;
        public GameObject Activator;
        public int ChosenBuffIndex;
        public float OverrideDuration = 1f;

        public override void Begin()
        {
            if(Target)
            {
                duration = OverrideDuration;
                EffectData effectData = new EffectData
                {
                    scale = 1,
                    origin = this.origin,
                    genericFloat = base.duration,
                    genericUInt = (uint)ChosenBuffIndex,                    
                    rootObject = Target,             
                };
                EffectManager.SpawnEffect(BuffBrazier.BrazierBuffFlameOrb, effectData, true);
            }
        }

        public override void OnArrival()
        {
            if(!Target) return;

            GameObject owner = null;
            Vector3 position = Target.transform.position;
            Quaternion rotation = Target.transform.rotation;

            var body = Target.GetComponent<CharacterBody>();
            if(body && body.master)
            {
                owner = body.gameObject;
                position = body.corePosition;     
            }
            else if(Target.GetComponent<TeleporterInteraction>())
            {
                owner = Target;
            }

            if(owner)
            {
                var flameOrb = Object.Instantiate(BuffBrazier.BrazierBuffOrbitOrb, position, rotation);
                var visualController = flameOrb.GetComponent<BuffBrazierOrbitVisualAndNetworkController>();

                visualController.Owner = owner;
                visualController.ChosenBuffIndex = ChosenBuffIndex;

                if(NetworkServer.active)
                {
                    visualController.OnOwnerChanged(visualController.Owner);
                    visualController.OnBuffIndexChanged(visualController.ChosenBuffIndex);

                    NetworkServer.Spawn(flameOrb);
                }
            }
        }

    }
}