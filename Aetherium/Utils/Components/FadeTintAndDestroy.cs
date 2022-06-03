using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Utils.Components
{
    internal class FadeTintAndDestroy : MonoBehaviour
    {
        public Material Material;
        public Color Color;

        public float ColorFadeDuration;
        public float Stopwatch;

        public void Start()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer)
            {
                Material = renderer.material;
                if (Material)
                {
                    Color = Material.GetColor("_TintColor");
                }
            }
        }

        public void Update()
        {
            if(Material)
            {
                Stopwatch += Time.deltaTime;

                var color = Color;
                color.a = 0;

                Material.SetColor("_TintColor", Color.Lerp(Color, color, EasingFunction.EaseOutQuad(0, 1, Stopwatch / ColorFadeDuration)));

                if(Stopwatch > ColorFadeDuration)
                {
                    UnityEngine.Object.Destroy(gameObject);
                }
            }
        }
    }
}
