using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Utils.Components
{
    public class FlickerHGStandardEmission : MonoBehaviour
    {
        public Renderer[] renderers;
        public float StartIntensity;
        public float Interval;
        public float Stopwatch;

        public void Update()
        {
            Stopwatch += Time.deltaTime;
            if(Stopwatch >= Interval)
            {
                foreach(Renderer renderer in renderers)
                {
                    foreach(Material material in renderer.materials)
                    {
                        if (material.HasProperty("_EmPower"))
                        {
                            var emissionPower = material.GetFloat("_EmPower");
                            if(emissionPower == StartIntensity)
                            {
                                material.SetFloat("_EmPower", 0);
                            }
                            else if(emissionPower == 0)
                            {
                                material.SetFloat("_EmPower", StartIntensity);
                            }
                            else
                            {
                                material.SetFloat("_EmPower", 0);
                            }
                        }
                    }
                }
                Stopwatch = 0;
            }
        }
    }
}
