using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Utils.Components
{
    class RainbowComponent : MonoBehaviour
    {
        public bool changeTexture;
        public bool changeEmission;

        public float hueRate = 1f;
        public float saturation = 1f;
        public float value = 1f;

        float stopwatch;

        Renderer meshRenderer;

        public void Awake()
        {
            meshRenderer = gameObject.GetComponent<Renderer>();
        }

        public void Update()
        {
            var rainbow = Color.HSVToRGB(((hueRate * stopwatch) % 1f + 1f) % 1f, saturation, value);

            if (changeTexture)
                meshRenderer.material.SetColor("_Color", rainbow);
            if (changeEmission)
                meshRenderer.material.SetColor("_EmColor", rainbow);

            stopwatch += Time.deltaTime;
        }

    }
}
