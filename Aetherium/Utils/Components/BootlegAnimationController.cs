using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aetherium.Utils.Components
{
    internal class BootlegAnimationController : MonoBehaviour
    {
        public float Duration;
        public Sprite[] Sprites;
        private SpriteRenderer SpriteRenderer;
        private float Timer;
        private int CurrentIndex = 0;

        public void Start()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            if (!SpriteRenderer || Sprites.Length <= 0)
            {
                Destroy(this);
            }

            Timer = Duration;
        }

        public void Update()
        {
            if(Sprites.Length > 0)
            {
                Timer -= Time.deltaTime;

                if(SpriteRenderer.sprite != Sprites[CurrentIndex])
                {
                    SpriteRenderer.sprite = Sprites[CurrentIndex];
                }

                if(Timer <= 0)
                {
                    CurrentIndex++;
                    if(CurrentIndex >= Sprites.Length)
                    {
                        CurrentIndex = 0;
                    }
                    Timer = Duration;
                }

            }
        }

    }
}
