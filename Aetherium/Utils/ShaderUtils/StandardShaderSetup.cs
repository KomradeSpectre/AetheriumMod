using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using UnityEngine;
using RoR2;
using R2API;
using TILER2;

namespace Aetherium.Utils.ShaderUtils
{
    //Used in the creation of a Hopoo Games Standard Shader material.
    public class StandardShaderSetup
    {
        public bool EnableCutout = false;

        public Color MainColor = new Color(0.5f, 0.5f, 0.5f, 1);
        public Texture2D MainTexture = Texture2D.whiteTexture;

        public float NormalStrength = 1;
        public Texture2D NormalMap = Texture2D.whiteTexture;

        public Color EmissionColor = new Color(0, 0, 0, 1);
        public Texture2D EmissionTexture = Texture2D.whiteTexture;
        public float EmissionPower = 1;

        public float Smoothness = 0;

        public bool ForceSpecularOn = false;

        public enum RampChoiceEnums
        {
            TwoTone,
            SmoothedTwoTone,
            Unlitish,
            Subsurface,
            Grass
        }
        public RampChoiceEnums RampChoice = RampChoiceEnums.TwoTone;

        public enum DecalLayerEnums
        {
            Default,
            Environment,
            Character,
            Misc
        }
        public DecalLayerEnums DecalLayer = DecalLayerEnums.Default;



    }
}
