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
            else if(BuffBrazierFlameOrbController.IsZoneAnchor(Target))
            {
                owner = Target;
            }

            if(owner)
            {
                var flameOrb = Object.Instantiate(BuffBrazier.BrazierBuffOrbitOrb, position, rotation);
                var visualController = flameOrb.GetComponent<BuffBrazierOrbitVisualAndNetworkController>();

                // Registered with the owner in the controller's Start, on the server and every client alike.
                visualController.Owner = owner;
                visualController.ChosenBuffIndex = ChosenBuffIndex;

                // Simulacrum waves never end the way a teleporter event does, so ward flames burn out after a few waves instead.
                if(Run.instance is InfiniteTowerRun infiniteTowerRun && owner.GetComponent<InfiniteTowerSafeWardController>())
                {
                    visualController.ExpireAtWave = infiniteTowerRun.waveIndex + Mathf.Max(1, BuffBrazier.SimulacrumWavesPerFlame);
                }

                if(NetworkServer.active)
                {
                    NetworkServer.Spawn(flameOrb);
                }
            }
        }

    }
}