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
            if (Target)
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
                //effectData.SetChildLocatorTransformReference(Target, Index);
                EffectManager.SpawnEffect(BuffBrazier.BrazierBuffFlameOrb, effectData, true);
            }
        }

        public override void OnArrival()
        {
            if (Target)
            {
                var body = Target.GetComponent<CharacterBody>();
                if (body && body.master)
                {
                    var flameOrb = UnityEngine.Object.Instantiate(BuffBrazier.BrazierBuffOrbitOrb, body.corePosition, body.transform.rotation);
                    var visualController = flameOrb.GetComponent<BuffBrazierOrbitVisualAndNetworkController>();
                    visualController.Owner = body.gameObject;
                    visualController.ChosenBuffIndex = ChosenBuffIndex;

                    if (NetworkServer.active)
                    {
                        NetworkServer.Spawn(flameOrb);
                    }


                }
                else
                {
                    var teleporter = Target.GetComponent<TeleporterInteraction>();
                    if (teleporter)
                    {
                        var flameOrb = UnityEngine.Object.Instantiate(BuffBrazier.BrazierBuffOrbitOrb, Target.transform.position, Target.transform.rotation);
                        var visualController = flameOrb.GetComponent<BuffBrazierOrbitVisualAndNetworkController>();
                        visualController.Owner = Target;
                        visualController.ChosenBuffIndex = ChosenBuffIndex;

                        if (NetworkServer.active)
                        {
                            NetworkServer.Spawn(flameOrb);
                        }
                    }
                }
            }
        }

    }
}