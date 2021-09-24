using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Aetherium.Utils
{
    public class MaterialControllerComponents
    {
        public static void SetShaderKeywordBasedOnBool(bool enabled, Material material, string keyword)
        {
            if (!material)
            {
                AetheriumPlugin.ModLogger.LogError($"Material field was null, cannot run shader keyword method.");
                return;
            }

            if (enabled)
            {
                if (!material.IsKeywordEnabled(keyword))
                {
                    material.EnableKeyword(keyword);
                }
            }
            else
            {
                if (material.IsKeywordEnabled(keyword))
                {
                    material.DisableKeyword(keyword);
                }
            }
        }

        public static void PutMaterialIntoMeshRenderer(Renderer meshRenderer, Material material)
        {
            if (material && meshRenderer)
            {
                meshRenderer.materials[0] = material;
            }
        }

        /// <summary>
        /// Attach this component to a gameObject and pass a renderer in. It'll attempt to find the correct shader controller from the meshrenderer material, attach it if it finds it, and destroy itself.
        /// </summary>
        public class HGControllerFinder : MonoBehaviour
        {
            public Renderer Renderer;
            public Material Material;

            public void Start()
            {
                if (Renderer)
                {
                    Material = Renderer.materials[0];

                    if (Material)
                    {

                        switch (Material.shader.name)
                        {
                            case "Hopoo Games/Deferred/Standard":
                                var standardController = gameObject.AddComponent<HGStandardController>();
                                standardController.Material = Material;
                                standardController.Renderer = Renderer;
                                break;
                            case "Hopoo Games/FX/Cloud Remap":
                                var cloudController = gameObject.AddComponent<HGCloudRemapController>();
                                cloudController.Material = Material;
                                cloudController.Renderer = Renderer;
                                break;
                            case "Hopoo Games/FX/Cloud Intersection Remap":
                                var intersectionController = gameObject.AddComponent<HGIntersectionController>();
                                intersectionController.Material = Material;
                                intersectionController.Renderer = Renderer;
                                break;
                            case "Hopoo Games/Deferred/Snow Topped":
                                var snowToppedController = gameObject.AddComponent<HGSnowToppedController>();
                                snowToppedController.Material = Material;
                                snowToppedController.Renderer = Renderer;
                                snowToppedController.name = Material.name + "(HGSnowTopped) Controller";
                                break;
                        }
                    }
                }
                Destroy(this);
            }
        }

        /// <summary>
        /// Attach to anything, and feed in a material that has the hgstandard shader.
        /// You then gain access to manipulate this in any runtime inspector of your choice.
        /// </summary>
        public class HGStandardController : MonoBehaviour
        {
            public Material Material;
            public Renderer Renderer;
            public string MaterialName { get => Material?.name ?? ""; }

            public bool _EnableCutout { get => Material?.IsKeywordEnabled("CUTOUT") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "CUTOUT"); }
            public Color _Color { get => Material?.GetColor("_Color") ?? default(Color); set => Material?.SetColor("_Color", value); }
            public Texture _MainTex { get => Material?.GetTexture("_MainTex") ?? null; set => Material?.SetTexture("_MainTex", value); }
            public Vector2 _MainTexScale { get => Material?.GetTextureScale("_MainTex") ?? Vector2.zero; set => Material?.SetTextureScale("_MainTex", value); }
            public Vector2 _MainTexOffset { get => Material?.GetTextureOffset("_MainTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_MainTex", value); }

            public float _NormalStrength { get => Material?.GetFloat("_NormalStrength") ?? 0; set => Material?.SetFloat("_NormalStrength", Mathf.Clamp(value, 0, 5)); }

            public Texture _NormalTex { get => Material?.GetTexture("_NormalTex") ?? null; set => Material?.SetTexture("_NormalTex", value); }
            public Vector2 _NormalTexScale { get => Material?.GetTextureScale("_NormalTex") ?? Vector2.zero; set => Material?.SetTextureScale("_NormalTex", value); }
            public Vector2 _NormalTexOffset { get => Material?.GetTextureOffset("_NormalTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_NormalTex", value); }
            public Color _EmColor { get => Material?.GetColor("_EmColor") ?? default(Color); set => Material?.SetColor("_EmColor", value); }
            public Texture _EmTex { get => Material?.GetTexture("_EmTex") ?? null; set => Material?.SetTexture("_EmTex", value); }

            public float _EmPower { get => Material?.GetFloat("_EmPower") ?? 0; set => Material?.SetFloat("_EmPower", Mathf.Clamp(value, 0, 10)); }

            public float _Smoothness { get => Material?.GetFloat("_Smoothness") ?? 0; set => Material?.SetFloat("_Smoothness", Mathf.Clamp(value, 0, 1)); }

            public bool _IgnoreDiffuseAlphaForSpeculars { get => Material?.IsKeywordEnabled("FORCE_SPEC") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "FORCE_SPEC"); }

            public enum _RampInfoEnum
            {
                TwoTone = 0,
                SmoothedTwoTone = 1,
                Unlitish = 3,
                Subsurface = 4,
                Grass = 5
            }
            public _RampInfoEnum _RampChoice { get => (_RampInfoEnum)(int)(Material?.GetFloat("_RampInfo") ?? 1); set => Material?.SetFloat("_RampInfo", Convert.ToSingle(value)); }

            public enum _DecalLayerEnum
            {
                Default = 0,
                Environment = 1,
                Character = 2,
                Misc = 3
            }
            public _DecalLayerEnum _DecalLayer { get => (_DecalLayerEnum)(int)(Material?.GetFloat("_DecalLayer") ?? 1); set => Material?.SetFloat("_DecalLayer", Convert.ToSingle(value)); }

            public float _SpecularStrength { get => Material?.GetFloat("_SpecularStrength") ?? 0; set => Material?.SetFloat("_SpecularStrength", Mathf.Clamp(value, 0, 1)); }

            public float _SpecularExponent { get => Material?.GetFloat("_SpecularExponent") ?? 0; set => Material?.SetFloat("_SpecularExponent", Mathf.Clamp(value, 0.1f, 20)); }

            public enum _CullEnum
            {
                Off = 0,
                Front = 1,
                Back = 2
            }
            public _CullEnum _Cull_Mode { get => (_CullEnum)(int)(Material?.GetFloat("_Cull") ?? 1); set => Material?.SetFloat("_Cull", Convert.ToSingle(value)); }

            public bool _EnableDither { get => Material?.IsKeywordEnabled("DITHER") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "DITHER"); }

            public float _FadeBias { get => Material?.GetFloat("_FadeBias") ?? 0; set => Material?.SetFloat("_FadeBias", Mathf.Clamp(value, 0, 1)); }

            public bool _EnableFresnelEmission { get => Material?.IsKeywordEnabled("FRESNEL_EMISSION") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "FRESNEL_EMISSION"); }

            public Texture _FresnelRamp { get => Material?.GetTexture("_FresnelRamp") ?? null; set => Material?.SetTexture("_FresnelRamp", value); }

            public float _FresnelPower { get => Material?.GetFloat("_FresnelPower") ?? 0; set => Material?.SetFloat("_FresnelPower", Mathf.Clamp(value, 0.1f, 20)); }

            public Texture _FresnelMask { get => Material?.GetTexture("_FresnelMask") ?? null; set => Material?.SetTexture("_FresnelMask", value); }

            public float _FresnelBoost { get => Material?.GetFloat("_FresnelBoost") ?? 0; set => Material?.SetFloat("_FresnelBoost", Mathf.Clamp(value, 0, 20)); }

            public bool _EnablePrinting { get => Material?.IsKeywordEnabled("PRINT_CUTOFF") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "PRINT_CUTOFF"); }

            public float _SliceHeight { get => Material?.GetFloat("_SliceHeight") ?? 0; set => Material?.SetFloat("_SliceHeight", Mathf.Clamp(value, -25, 25)); }

            public float _PrintBandHeight { get => Material?.GetFloat("_SliceBandHeight") ?? 0; set => Material?.SetFloat("_SliceBandHeight", Mathf.Clamp(value, 0, 10)); }

            public float _PrintAlphaDepth { get => Material?.GetFloat("_SliceAlphaDepth") ?? 0; set => Material?.SetFloat("_SliceAlphaDepth", Mathf.Clamp(value, 0, 1)); }

            public Texture _PrintAlphaTexture { get => Material?.GetTexture("_SliceAlphaTex") ?? null; set => Material?.SetTexture("_SliceAlphaTex", value); }
            public Vector2 _PrintAlphaTextureScale { get => Material?.GetTextureScale("_SliceAlphaTex") ?? Vector2.zero; set => Material?.SetTextureScale("_SliceAlphaTex", value); }
            public Vector2 _PrintAlphaTextureOffset { get => Material?.GetTextureOffset("_SliceAlphaTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_SliceAlphaTex", value); }

            public float _PrintColorBoost { get => Material?.GetFloat("_PrintBoost") ?? 0; set => Material?.SetFloat("_PrintBoost", Mathf.Clamp(value, 0, 10)); }

            public float _PrintAlphaBias { get => Material?.GetFloat("_PrintBias") ?? 0; set => Material?.SetFloat("_PrintBias", Mathf.Clamp(value, 0, 4)); }

            public float _PrintEmissionToAlbedoLerp { get => Material?.GetFloat("_PrintEmissionToAlbedoLerp") ?? 0; set => Material?.SetFloat("_PrintEmissionToAlbedoLerp", Mathf.Clamp(value, 0, 1)); }

            public enum _PrintDirectionEnum
            {
                BottomUp = 0,
                TopDown = 1,
                BackToFront = 3
            }
            public _PrintDirectionEnum _PrintDirection { get => (_PrintDirectionEnum)(int)(Material?.GetFloat("_PrintDirection") ?? 1); set => Material?.SetFloat("_PrintDirection", Convert.ToSingle(value)); }

            public Texture _PrintRamp { get => Material?.GetTexture("_PrintRamp") ?? null; set => Material?.SetTexture("_PrintRamp", value); }

            public float _EliteIndex { get => Material?.GetFloat("_EliteIndex") ?? 0; set => Material?.SetFloat("_EliteIndex", value); }

            public float _EliteBrightnessMin { get => Material?.GetFloat("_EliteBrightnessMin") ?? 0; set => Material?.SetFloat("_EliteBrightnessMin", Mathf.Clamp(value, -10, 10)); }

            public float _EliteBrightnessMax { get => Material?.GetFloat("_EliteBrightnessMax") ?? 0; set => Material?.SetFloat("_EliteBrightnessMax", Mathf.Clamp(value, -10, 10)); }

            public bool _EnableSplatmap { get => Material?.IsKeywordEnabled("SPLATMAP") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "SPLATMAP"); }
            public bool _UseVertexColorsInstead { get => Material?.IsKeywordEnabled("USE_VERTEX_COLORS") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "USE_VERTEX_COLORS"); }

            public float _BlendDepth { get => Material?.GetFloat("_Depth") ?? 0; set => Material?.SetFloat("_Depth", Mathf.Clamp(value, 0, 1)); }

            public Texture _SplatmapTex { get => Material?.GetTexture("_SplatmapTex") ?? null; set => Material?.SetTexture("_SplatmapTex", value); }
            public Vector2 _SplatmapTexScale { get => Material?.GetTextureScale("_SplatmapTex") ?? Vector2.zero; set => Material?.SetTextureScale("_SplatmapTex", value); }
            public Vector2 _SplatmapTexOffset { get => Material?.GetTextureOffset("_SplatmapTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_SplatmapTex", value); }

            public float _SplatmapTileScale { get => Material?.GetFloat("_SplatmapTileScale") ?? 0; set => Material?.SetFloat("_SplatmapTileScale", Mathf.Clamp(value, 0, 20)); }

            public Texture _GreenChannelTex { get => Material?.GetTexture("_GreenChannelTex") ?? null; set => Material?.SetTexture("_GreenChannelTex", value); }
            public Texture _GreenChannelNormalTex { get => Material?.GetTexture("_GreenChannelNormalTex") ?? null; set => Material?.SetTexture("_GreenChannelNormalTex", value); }

            public float _GreenChannelSmoothness { get => Material?.GetFloat("_GreenChannelSmoothness") ?? 0; set => Material?.SetFloat("_GreenChannelSmoothness", Mathf.Clamp(value, 0, 1)); }

            public float _GreenChannelBias { get => Material?.GetFloat("_GreenChannelBias") ?? 0; set => Material?.SetFloat("_GreenChannelBias", Mathf.Clamp(value, -2, 5)); }

            public Texture _BlueChannelTex { get => Material?.GetTexture("_BlueChannelTex") ?? null; set => Material?.SetTexture("_BlueChannelTex", value); }
            public Texture _BlueChannelNormalTex { get => Material?.GetTexture("_BlueChannelNormalTex") ?? null; set => Material?.SetTexture("_BlueChannelNormalTex", value); }

            public float _BlueChannelSmoothness { get => Material?.GetFloat("_BlueChannelSmoothness") ?? 0; set => Material?.SetFloat("_BlueChannelSmoothness", Mathf.Clamp(value, 0, 1)); }

            public float _BlueChannelBias { get => Material?.GetFloat("_BlueChannelBias") ?? 0; set => Material?.SetFloat("_BlueChannelBias", Mathf.Clamp(value, -2, 5)); }

            public bool _EnableFlowmap { get => Material?.IsKeywordEnabled("FLOWMAP") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "FLOWMAP"); }
            public Texture _FlowTexture { get => Material?.GetTexture("_FlowTex") ?? null; set => Material?.SetTexture("_FlowTex", value); }
            public Texture _FlowHeightmap { get => Material?.GetTexture("_FlowHeightmap") ?? null; set => Material?.SetTexture("_FlowHeightmap", value); }
            public Vector2 _FlowHeightmapScale { get => Material?.GetTextureScale("_FlowHeightmap") ?? Vector2.zero; set => Material?.SetTextureScale("_FlowHeightmap", value); }
            public Vector2 _FlowHeightmapOffset { get => Material?.GetTextureOffset("_FlowHeightmap") ?? Vector2.zero; set => Material?.SetTextureOffset("_FlowHeightmap", value); }
            public Texture _FlowHeightRamp { get => Material?.GetTexture("_FlowHeightRamp") ?? null; set => Material?.SetTexture("_FlowHeightRamp", value); }
            public Vector2 _FlowHeightRampScale { get => Material?.GetTextureScale("_FlowHeightRamp") ?? Vector2.zero; set => Material?.SetTextureScale("_FlowHeightRamp", value); }
            public Vector2 _FlowHeightRampOffset { get => Material?.GetTextureOffset("_FlowHeightRamp") ?? Vector2.zero; set => Material?.SetTextureOffset("_FlowHeightRamp", value); }

            public float _FlowHeightBias { get => Material?.GetFloat("_FlowHeightBias") ?? 0; set => Material?.SetFloat("_FlowHeightBias", Mathf.Clamp(value, -1, 1)); }

            public float _FlowHeightPower { get => Material?.GetFloat("_FlowHeightPower") ?? 0; set => Material?.SetFloat("_FlowHeightPower", Mathf.Clamp(value, 0.1f, 20)); }

            public float _FlowEmissionStrength { get => Material?.GetFloat("_FlowEmissionStrength") ?? 0; set => Material?.SetFloat("_FlowEmissionStrength", Mathf.Clamp(value, 0.1f, 20)); }

            public float _FlowSpeed { get => Material?.GetFloat("_FlowSpeed") ?? 0; set => Material?.SetFloat("_FlowSpeed", Mathf.Clamp(value, 0, 15)); }

            public float _MaskFlowStrength { get => Material?.GetFloat("_FlowMaskStrength") ?? 0; set => Material?.SetFloat("_FlowMaskStrength", Mathf.Clamp(value, 0, 5)); }

            public float _NormalFlowStrength { get => Material?.GetFloat("_FlowNormalStrength") ?? 0; set => Material?.SetFloat("_FlowNormalStrength", Mathf.Clamp(value, 0, 5)); }

            public float _FlowTextureScaleFactor { get => Material?.GetFloat("_FlowTextureScaleFactor") ?? 0; set => Material?.SetFloat("_FlowTextureScaleFactor", Mathf.Clamp(value, 0, 10)); }

            public bool _EnableLimbRemoval { get => Material?.IsKeywordEnabled("LIMBREMOVAL") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "LIMBREMOVAL"); }

            public float _LimbPrimeMask { get => Material?.GetFloat("_LimbPrimeMask") ?? 0; set => Material?.SetFloat("_LimbPrimeMask", Mathf.Clamp(value, 1, 10000)); }

            public Color _FlashColor { get => Material?.GetColor("_FlashColor") ?? default(Color); set => Material?.SetColor("_FlashColor", value); }

            public float _Fade { get => Material?.GetFloat("_Fade") ?? 0; set => Material?.SetFloat("_Fade", Mathf.Clamp(value, 0, 1)); }

            public void Update()
            {
                if (!Material)
                {
                    Destroy(this);
                }
            }

        }

        /// <summary>
        /// Developed by Vale-X. Attach to anything, and feed in a material that has the hgsnowtopped shader.
        /// You then gain access to manipulate this in any Runtime Inspector of your choice.
        /// </summary>
        public class HGSnowToppedController : MonoBehaviour
        {
            public Material Material;
            public Renderer Renderer;
            public string MaterialName;

            public Color _Color;
            public Texture _MainTex;
            public Vector2 _MainTexScale;
            public Vector2 _MainTexOffset;

            [Range(0f, 5f)]
            public float _NormalStrength;

            public Texture _NormalTex;
            public Vector2 _NormalTexScale;
            public Vector2 _NormalTexOffset;

            public Texture _SnowTex;
            public Vector2 _SnowTexScale;
            public Vector2 _SnowTexOffset;

            public Texture _SnowNormalTex;
            public Vector2 _SnowNormalTexScale;
            public Vector2 _SnowNormalTexOffset;

            [Range(-1f, 1f)]
            public float _SnowBias;

            [Range(0f, 1f)]
            public float _Depth;

            public bool _IgnoreAlphaWeights;
            public bool _BlendWeightsBinarily;

            public enum _RampInfoEnum
            {
                TwoTone = 0,
                SmoothedTwoTone = 1,
                Unlitish = 3,
                Subsurface = 4,
                Grass = 5
            }
            public _RampInfoEnum _RampChoice;

            public bool _IgnoreDiffuseAlphaForSpeculars;

            [Range(0f, 1f)]
            public float _SpecularStrength;

            [Range(0.1f, 20f)]
            public float _SpecularExponent;

            [Range(0f, 1f)]
            public float _Smoothness;

            [Range(0f, 1f)]
            public float _SnowSpecularStrength;

            [Range(0.1f, 20f)]
            public float _SnowSpecularExponent;

            [Range(0f, 1f)]
            public float _SnowSmoothness;

            public bool _DitherOn;

            public bool _TriplanarOn;

            [Range(0f, 1f)]
            public float _TriplanarTextureFactor;

            public bool _SnowOn;

            public bool _GradientBiasOn;

            public Vector4 _GradientBiasVector;

            public bool _DirtOn;

            public Texture _DirtTex;
            public Vector2 _DirtTexScale;
            public Vector2 _DirtTexOffset;

            public Texture _DirtNormalTex;
            public Vector2 _DirtNormalTexScale;
            public Vector2 _DirtNormalTexOffset;

            [Range(-2f, 2f)]
            public float _DirtBias;

            [Range(0f, 1f)]
            public float _DirtSpecularStrength;

            [Range(0f, 20f)]
            public float _DirtSpecularExponent;

            [Range(0f, 1f)]
            public float _DirtSmoothness;

            public void Start()
            {
                GrabMaterialValues();
            }
            public void GrabMaterialValues()
            {
                if (Material)
                {
                    _Color = Material.GetColor("_Color");
                    _MainTex = Material.GetTexture("_MainTex");
                    _MainTexScale = Material.GetTextureScale("_MainTex");
                    _MainTexOffset = Material.GetTextureOffset("_MainTex");
                    _NormalStrength = Material.GetFloat("_NormalStrength");
                    _NormalTex = Material.GetTexture("_NormalTex");
                    _NormalTexScale = Material.GetTextureScale("_NormalTex");
                    _NormalTexOffset = Material.GetTextureOffset("_NormalTex");
                    _SnowTex = Material.GetTexture("_SnowTex");
                    _SnowTexScale = Material.GetTextureScale("_SnowTex");
                    _SnowTexOffset = Material.GetTextureOffset("_SnowTex");
                    _SnowNormalTex = Material.GetTexture("_SnowNormalTex");
                    _SnowNormalTexScale = Material.GetTextureScale("_SnowNormalTex");
                    _SnowNormalTexOffset = Material.GetTextureOffset("_SnowNormalTex");
                    _SnowBias = Material.GetFloat("_SnowBias");
                    _Depth = Material.GetFloat("_Depth");
                    _IgnoreAlphaWeights = Material.IsKeywordEnabled("IGNORE_BIAS");
                    _BlendWeightsBinarily = Material.IsKeywordEnabled("BINARYBLEND");
                    _RampChoice = (_RampInfoEnum)(int)Material.GetFloat("_RampInfo");
                    _IgnoreDiffuseAlphaForSpeculars = Material.IsKeywordEnabled("FORCE_SPEC");
                    _SpecularStrength = Material.GetFloat("_SpecularStrength");
                    _SpecularExponent = Material.GetFloat("_SpecularExponent");
                    _Smoothness = Material.GetFloat("_Smoothness");
                    _SnowSpecularStrength = Material.GetFloat("_SnowSpecularStrength");
                    _SnowSpecularExponent = Material.GetFloat("_SnowSpecularExponent");
                    _SnowSmoothness = Material.GetFloat("_SnowSmoothness");
                    _DitherOn = Material.IsKeywordEnabled("DITHER");
                    _TriplanarOn = Material.IsKeywordEnabled("TRIPLANAR");
                    _TriplanarTextureFactor = Material.GetFloat("_TriplanarTextureFactor");
                    _SnowOn = Material.IsKeywordEnabled("MICROFACET_SNOW");
                    _GradientBiasOn = Material.IsKeywordEnabled("GRADIENTBIAS");
                    _GradientBiasVector = Material.GetVector("_GradientBiasVector");
                    _DirtOn = Material.IsKeywordEnabled("DIRTON");
                    _DirtTex = Material.GetTexture("_DirtTex");
                    _DirtTexScale = Material.GetTextureScale("_DirtTex");
                    _DirtTexOffset = Material.GetTextureOffset("_DirtTex");
                    _DirtNormalTex = Material.GetTexture("_DirtNormalTex");
                    _DirtNormalTexScale = Material.GetTextureScale("_DirtNormalTex");
                    _DirtNormalTexOffset = Material.GetTextureOffset("_DirtNormalTex");
                    _DirtBias = Material.GetFloat("_DirtBias");
                    _DirtSpecularStrength = Material.GetFloat("_DirtSpecularStrength");
                    _DirtSpecularExponent = Material.GetFloat("_DirtSpecularExponent");
                    _DirtSmoothness = Material.GetFloat("_DirtSmoothness");
                    MaterialName = Material.name;
                }
            }

            public void Update()
            {
                if (Material)
                {
                    if (Material.name != MaterialName && Renderer)
                    {
                        GrabMaterialValues();
                        PutMaterialIntoMeshRenderer(Renderer, Material);
                    }

                    Material.SetColor("_Color", _Color);

                    if (_MainTex)
                    {
                        Material.SetTexture("_MainTex", _MainTex);
                        Material.SetTextureScale("_MainTex", _MainTexScale);
                        Material.SetTextureOffset("_MainTex", _MainTexOffset);
                    }
                    else
                    {
                        Material.SetTexture("_MainTex", null);
                    }

                    Material.SetFloat("_NormalStrength", _NormalStrength);

                    if (_NormalTex)
                    {
                        Material.SetTexture("_NormalTex", _NormalTex);
                        Material.SetTextureScale("_NormalTex", _NormalTexScale);
                        Material.SetTextureOffset("_NormalTex", _NormalTexOffset);
                    }
                    else
                    {
                        Material.SetTexture("_NormalTex", null);
                    }

                    if (_SnowTex)
                    {
                        Material.SetTexture("_SnowTex", _SnowTex);
                        Material.SetTextureScale("_SnowTex", _SnowTexScale);
                        Material.SetTextureOffset("_SnowTex", _SnowTexOffset);
                    }
                    else
                    {
                        Material.SetTexture("_SnowTex", null);
                    }

                    if (_SnowNormalTex)
                    {
                        Material.SetTexture("_SnowNormalTex", _SnowNormalTex);
                        Material.SetTextureScale("_SnowNormalTex", _SnowNormalTexScale);
                        Material.SetTextureOffset("_SnowNormalTex", _SnowNormalTexOffset);
                    }
                    else
                    {
                        Material.SetTexture("_SnowNormalTex", null);
                    }

                    Material.SetFloat("_SnowBias", _SnowBias);
                    Material.SetFloat("_Depth", _Depth);

                    SetShaderKeywordBasedOnBool(_IgnoreAlphaWeights, Material, "IGNORE_BIAS");
                    SetShaderKeywordBasedOnBool(_BlendWeightsBinarily, Material, "BINARYBLEND");

                    Material.SetFloat("_RampInfo", Convert.ToSingle(_RampChoice));

                    SetShaderKeywordBasedOnBool(_IgnoreDiffuseAlphaForSpeculars, Material, "FORCE_SPEC");


                    Material.SetFloat("_SpecularStrength", _SpecularStrength);
                    Material.SetFloat("_SpecularExponent", _SpecularExponent);
                    Material.SetFloat("_Smoothness", _Smoothness);

                    Material.SetFloat("_SnowSpecularStrength", _SnowSpecularStrength);
                    Material.SetFloat("_SnowSpecularExponent", _SnowSpecularExponent);
                    Material.SetFloat("_SnowSmoothness", _SnowSmoothness);

                    SetShaderKeywordBasedOnBool(_DitherOn, Material, "DITHER");

                    SetShaderKeywordBasedOnBool(_TriplanarOn, Material, "TRIPLANAR");
                    Material.SetFloat("_TriplanarTextureFactor", _TriplanarTextureFactor);
                    SetShaderKeywordBasedOnBool(_SnowOn, Material, "MICROFACET_SNOW");

                    SetShaderKeywordBasedOnBool(_GradientBiasOn, Material, "GRADIENTBIAS");
                    Material.SetVector("_GradientBiasVector", _GradientBiasVector);
                    SetShaderKeywordBasedOnBool(_DirtOn, Material, "DIRTON");

                    if (_DirtTex)
                    {
                        Material.SetTexture("_DirtTex", _DirtTex);
                        Material.SetTextureScale("_DirtTex", _DirtTexScale);
                        Material.SetTextureOffset("_DirtTex", _DirtTexOffset);
                    }
                    else
                    {
                        Material.SetTexture("_DirtTex", null);
                    }

                    if (_DirtNormalTex)
                    {
                        Material.SetTexture("_DirtNormalTex", _DirtNormalTex);
                        Material.SetTextureScale("_DirtNormalTex", _DirtNormalTexScale);
                        Material.SetTextureOffset("_DirtNormalTex", _DirtNormalTexOffset);
                    }
                    else
                    {
                        Material.SetTexture("_DirtNormalTex", null);
                    }

                    Material.SetFloat("_DirtBias", _DirtBias);
                    Material.SetFloat("_DirtSpecularStrength", _DirtSpecularStrength);
                    Material.SetFloat("_DirtSpecularExponent", _DirtSpecularExponent);
                    Material.SetFloat("_DirtSmoothness", _DirtSmoothness);
                }
            }

        }

        /// <summary>
        /// Attach to anything, and feed in a material that has the hgcloudremap shader.
        /// You then gain access to manipulate this in any Runtime Inspector of your choice.
        /// </summary>
        public class HGCloudRemapController : MonoBehaviour
        {
            public Material Material;
            public Renderer Renderer;
            public string MaterialName { get => Material?.name ?? "";}

            public UnityEngine.Rendering.BlendMode _Source_Blend_Mode { get => (UnityEngine.Rendering.BlendMode)(int)(Material?.GetFloat("_SrcBlend") ?? 1); set => Material?.SetFloat("_SrcBlend", Convert.ToSingle(value)); }
            public UnityEngine.Rendering.BlendMode _Destination_Blend_Mode { get => (UnityEngine.Rendering.BlendMode)(int)(Material?.GetFloat("_DstBlend") ?? 1); set => Material?.SetFloat("_DstBlend", Convert.ToSingle(value)); }

            public Color _Tint { get => Material?.GetColor("_TintColor") ?? default(Color); set => Material?.SetColor("_TintColor", value); }
            public bool _DisableRemapping { get => Material?.IsKeywordEnabled("DISABLEREMAP") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "DISABLEREMAP"); }
            public Texture _MainTex { get => Material?.GetTexture("_MainTex") ?? null; set => Material?.SetTexture("_MainTex", value); }
            public Vector2 _MainTexScale { get => Material?.GetTextureScale("_MainTex") ?? Vector2.zero; set => Material?.SetTextureScale("_MainTex", value); }
            public Vector2 _MainTexOffset { get => Material?.GetTextureOffset("_MainTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_MainTex", value); }
            public Texture _RemapTex { get => Material?.GetTexture("_RemapTex") ?? null; set => Material?.SetTexture("_RemapTex", value); }
            public Vector2 _RemapTexScale { get => Material?.GetTextureScale("_RemapTex") ?? Vector2.zero; set => Material?.SetTextureScale("_RemapTex", value); }
            public Vector2 _RemapTexOffset { get => Material?.GetTextureOffset("_RemapTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_RemapTex", value); }

            public float _SoftFactor { get => Material?.GetFloat("_InvFade") ?? 0; set => Material?.SetFloat("_InvFade", Mathf.Clamp(value, 0, 2)); }

            public float _BrightnessBoost { get => Material?.GetFloat("_Boost") ?? 0; set => Material?.SetFloat("_Boost", Mathf.Clamp(value, 1, 20)); }

            public float _AlphaBoost { get => Material?.GetFloat("_AlphaBoost") ?? 0; set => Material?.SetFloat("_AlphaBoost", Mathf.Clamp(value, 0, 20)); }

            public float _AlphaBias { get => Material?.GetFloat("_AlphaBias") ?? 0; set => Material?.SetFloat("_AlphaBias", Mathf.Clamp(value, 0, 20)); }

            public bool _UseUV1 { get => Material?.IsKeywordEnabled("USE_UV1") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "USE_UV1"); }
            public bool _FadeWhenNearCamera { get => Material?.IsKeywordEnabled("FADECLOSE") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "FADECLOSE"); }

            public float _FadeCloseDistance { get => Material?.GetFloat("_FadeCloseDistance") ?? 0; set => Material?.SetFloat("_FadeCloseDistance", Mathf.Clamp(value, 0, 1)); }

            public enum _CullEnum
            {
                Off = 0,
                Front = 1,
                Back = 2
            }
            public _CullEnum _Cull_Mode { get => (_CullEnum)(int)(Material?.GetFloat("_Cull") ?? 1); set => Material?.SetFloat("_Cull", Convert.ToSingle(value)); }

            public UnityEngine.Rendering.CompareFunction _ZTest_Mode { get => (UnityEngine.Rendering.CompareFunction)(int)(Material?.GetFloat("_ZTest") ?? 1); set => Material?.SetFloat("_ZTest", Convert.ToSingle(value)); }

            public float _DepthOffset { get => Material?.GetFloat("_DepthOffset") ?? 0; set => Material?.SetFloat("_DepthOffset", Mathf.Clamp(value, -10, 10)); }

            public bool _CloudRemapping { get => Material?.IsKeywordEnabled("USE_CLOUDS") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "USE_CLOUDS"); }
            public bool _DistortionClouds { get => Material?.IsKeywordEnabled("CLOUDOFFSET") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "CLOUDOFFSET"); }
            public float _DistortionStrength { get => Material?.GetFloat("_DistortionStrength") ?? 0; set => Material?.SetFloat("_DistortionStrength", Mathf.Clamp(value, -2, 2)); }

            public Texture _Cloud1Tex { get => Material?.GetTexture("_Cloud1Tex") ?? null; set => Material?.SetTexture("_Cloud1Tex", value); }
            public Vector2 _Cloud1TexScale { get => Material?.GetTextureScale("_Cloud1Tex") ?? Vector2.zero; set => Material?.SetTextureScale("_Cloud1Tex", value); }
            public Vector2 _Cloud1TexOffset { get => Material?.GetTextureOffset("_Cloud1Tex") ?? Vector2.zero; set => Material?.SetTextureOffset("_Cloud1Tex", value); }
            public Texture _Cloud2Tex { get => Material?.GetTexture("_Cloud2Tex") ?? null; set => Material?.SetTexture("_Cloud2Tex", value); }
            public Vector2 _Cloud2TexScale { get => Material?.GetTextureScale("_Cloud2Tex") ?? Vector2.zero; set => Material?.SetTextureScale("_Cloud2Tex", value); }
            public Vector2 _Cloud2TexOffset { get => Material?.GetTextureOffset("_Cloud2Tex") ?? Vector2.zero; set => Material?.SetTextureOffset("_Cloud2Tex", value); }

            public Vector4 _CutoffScroll { get => Material?.GetVector("_CutoffScroll") ?? Vector4.zero; set => Material?.SetVector("_CutoffScroll", value); }
            public bool _VertexColors { get => Material?.IsKeywordEnabled("VERTEXCOLOR") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "VERTEXCOLOR"); }
            public bool _LuminanceForVertexAlpha { get => Material?.IsKeywordEnabled("VERTEXALPHA") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "VERTEXALPHA"); }
            public bool _LuminanceForTextureAlpha { get => Material?.IsKeywordEnabled("CALCTEXTUREALPHA") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "CALCTEXTUREALPHA"); }
            public bool _VertexOffset { get => Material?.IsKeywordEnabled("VERTEXOFFSET") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "VERTEXOFFSET"); }
            public bool _FresnelFade { get => Material?.IsKeywordEnabled("FRESNEL") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "FRESNEL"); }
            public bool _SkyboxOnly { get => Material?.IsKeywordEnabled("SKYBOX_ONLY") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "SKYBOX_ONLY"); }

            public float _FresnelPower { get => Material?.GetFloat("_FresnelPower") ?? 0; set => Material?.SetFloat("_FresnelPower", Mathf.Clamp(value, -20, 20)); }
            public float _VertexOffsetAmount { get => Material?.GetFloat("_OffsetAmount") ?? 0; set => Material?.SetFloat("_OffsetAmount", Mathf.Clamp(value, 0, 3)); }

            public float _ExternalAlpha { get => Material?.GetFloat("_ExternalAlpha") ?? 0; set => Material?.SetFloat("_ExternalAlpha", Mathf.Clamp(value, 0, 1)); }
            public float _Fade { get => Material?.GetFloat("_Fade") ?? 0; set => Material?.SetFloat("_Fade", Mathf.Clamp(value, 0, 1)); }

            public void Update()
            {
                if (!Material)
                {
                    Destroy(this);
                }
            }
        }

        /// <summary>
        /// Attach to anything, and feed in a material that has the hgcloudintersectionremap shader.
        /// You then gain access to manipulate this in any Runtime Inspector of your choice.
        /// </summary>
        public class HGIntersectionController : MonoBehaviour
        {
            public Material Material;
            public Renderer Renderer;
            public string MaterialName;

            public UnityEngine.Rendering.BlendMode _Source_Blend_Mode;
            public UnityEngine.Rendering.BlendMode _Destination_Blend_Mode;

            public Color _Tint;
            public Texture _MainTex;
            public Vector2 _MainTexScale;
            public Vector2 _MainTexOffset;
            public Texture _Cloud1Tex;
            public Vector2 _Cloud1TexScale;
            public Vector2 _Cloud1TexOffset;
            public Texture _Cloud2Tex;
            public Vector2 _Cloud2TexScale;
            public Vector2 _Cloud2TexOffset;
            public Texture _RemapTex;
            public Vector2 _RemapTexScale;
            public Vector2 _RemapTexOffset;
            public Vector4 _CutoffScroll;

            [Range(0f, 30f)]
            public float _SoftFactor;

            [Range(0.1f, 20f)]
            public float _SoftPower;

            [Range(0f, 5f)]
            public float _BrightnessBoost;

            [Range(0.1f, 20f)]
            public float _RimPower;

            [Range(0f, 5f)]
            public float _RimStrength;

            [Range(0f, 20f)]
            public float _AlphaBoost;

            [Range(0f, 20f)]
            public float _IntersectionStrength;

            public enum _CullEnum
            {
                Off = 0,
                Front = 1,
                Back = 2
            }
            public _CullEnum _Cull_Mode;

            public bool _FadeFromVertexColorsOn;
            public bool _EnableTriplanarProjectionsForClouds;

            public void Start()
            {
                GrabMaterialValues();
            }

            public void GrabMaterialValues()
            {
                if (Material)
                {
                    _Source_Blend_Mode = (UnityEngine.Rendering.BlendMode)Material.GetFloat("_SrcBlendFloat");
                    _Destination_Blend_Mode = (UnityEngine.Rendering.BlendMode)Material.GetFloat("_DstBlendFloat");
                    _Tint = Material.GetColor("_TintColor");
                    _MainTex = Material.GetTexture("_MainTex");
                    _MainTexScale = Material.GetTextureScale("_MainTex");
                    _MainTexOffset = Material.GetTextureOffset("_MainTex");
                    _Cloud1Tex = Material.GetTexture("_Cloud1Tex");
                    _Cloud1TexScale = Material.GetTextureScale("_Cloud1Tex");
                    _Cloud1TexOffset = Material.GetTextureOffset("_Cloud1Tex");
                    _Cloud2Tex = Material.GetTexture("_Cloud2Tex");
                    _Cloud2TexScale = Material.GetTextureScale("_Cloud2Tex");
                    _Cloud2TexOffset = Material.GetTextureOffset("_Cloud2Tex");
                    _RemapTex = Material.GetTexture("_RemapTex");
                    _RemapTexScale = Material.GetTextureScale("_RemapTex");
                    _RemapTexOffset = Material.GetTextureOffset("_RemapTex");
                    _CutoffScroll = Material.GetVector("_CutoffScroll");
                    _SoftFactor = Material.GetFloat("_InvFade");
                    _SoftPower = Material.GetFloat("_SoftPower");
                    _BrightnessBoost = Material.GetFloat("_Boost");
                    _RimPower = Material.GetFloat("_RimPower");
                    _RimStrength = Material.GetFloat("_RimStrength");
                    _AlphaBoost = Material.GetFloat("_AlphaBoost");
                    _IntersectionStrength = Material.GetFloat("_IntersectionStrength");
                    _Cull_Mode = (_CullEnum)(int)Material.GetFloat("_Cull");
                    _FadeFromVertexColorsOn = Material.IsKeywordEnabled("FADE_FROM_VERTEX_COLORS");
                    _EnableTriplanarProjectionsForClouds = Material.IsKeywordEnabled("TRIPLANAR");
                    MaterialName = Material.name;
                }
            }

            public void Update()
            {

                if (Material)
                {
                    if (Material.name != MaterialName && Renderer)
                    {
                        GrabMaterialValues();
                        PutMaterialIntoMeshRenderer(Renderer, Material);
                    }
                    Material.SetFloat("_SrcBlendFloat", Convert.ToSingle(_Source_Blend_Mode));
                    Material.SetFloat("_DstBlendFloat", Convert.ToSingle(_Destination_Blend_Mode));
                    Material.SetColor("_TintColor", _Tint);
                    if (_MainTex)
                    {
                        Material.SetTexture("_MainTex", _MainTex);
                        Material.SetTextureScale("_MainTex", _MainTexScale);
                        Material.SetTextureOffset("_MainTex", _MainTexOffset);
                    }
                    else
                    {
                        Material.SetTexture("_MainTex", null);
                    }

                    if (_Cloud1Tex)
                    {
                        Material.SetTexture("_Cloud1Tex", _Cloud1Tex);
                        Material.SetTextureScale("_Cloud1Tex", _Cloud1TexScale);
                        Material.SetTextureOffset("_Cloud1Tex", _Cloud1TexOffset);
                    }
                    else
                    {
                        Material.SetTexture("_Cloud1Tex", null);
                    }

                    if (_Cloud2Tex)
                    {
                        Material.SetTexture("_Cloud2Tex", _Cloud2Tex);
                        Material.SetTextureScale("_Cloud2Tex", _Cloud2TexScale);
                        Material.SetTextureOffset("_Cloud2Tex", _Cloud2TexOffset);
                    }
                    else
                    {
                        Material.SetTexture("_Cloud2Tex", null);
                    }

                    if (_RemapTex)
                    {
                        Material.SetTexture("_RemapTex", _RemapTex);
                        Material.SetTextureScale("_RemapTex", _RemapTexScale);
                        Material.SetTextureOffset("_RemapTex", _RemapTexOffset);
                    }
                    else
                    {
                        Material.SetTexture("_RemapTex", null);
                    }

                    Material.SetVector("_CutoffScroll", _CutoffScroll);
                    Material.SetFloat("_InvFade", _SoftFactor);
                    Material.SetFloat("_SoftPower", _SoftPower);
                    Material.SetFloat("_Boost", _BrightnessBoost);
                    Material.SetFloat("_RimPower", _RimPower);
                    Material.SetFloat("_RimStrength", _RimStrength);
                    Material.SetFloat("_AlphaBoost", _AlphaBoost);
                    Material.SetFloat("_IntersectionStrength", _IntersectionStrength);
                    Material.SetFloat("_Cull", Convert.ToSingle(_Cull_Mode));

                    SetShaderKeywordBasedOnBool(_FadeFromVertexColorsOn, Material, "FADE_FROM_VERTEX_COLORS");
                    SetShaderKeywordBasedOnBool(_EnableTriplanarProjectionsForClouds, Material, "TRIPLANAR");
                }
            }
        }
    }
}
