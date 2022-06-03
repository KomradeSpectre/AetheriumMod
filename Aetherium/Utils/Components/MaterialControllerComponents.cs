using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using R2API.Utils;
using RoR2;
using UnityEngine.Networking;

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
            public Material[] Materials;

            public void Start()
            {
                Renderer = gameObject.GetComponent<Renderer>();
                if (Renderer)
                {
                    Materials = Renderer.materials;

                    foreach (Material material in Materials)
                    {
                        if (material)
                        {

                            switch (material.shader.name)
                            {
                                case "Hopoo Games/Deferred/Standard":
                                    var standardController = gameObject.AddComponent<HGStandardController>();
                                    standardController.Material = material;
                                    standardController.Renderer = Renderer;
                                    break;
                                case "Hopoo Games/Deferred/Snow Topped":
                                    var snowToppedController = gameObject.AddComponent<HGSnowToppedController>();
                                    snowToppedController.Material = material;
                                    snowToppedController.Renderer = Renderer;
                                    snowToppedController.name = material.name + "(HGSnowTopped) Controller";
                                    break;
                                case "Hopoo Games/Deferred/Triplanar Terrain Blend":
                                    var triplanarController = gameObject.AddComponent<HGTriplanarTerrainBlend>();
                                    triplanarController.Material = material;
                                    triplanarController.Renderer = Renderer;
                                    break;
                                case "Hopoo Games/FX/Distortion":
                                    var distortionController = gameObject.AddComponent<HGDistortionController>();
                                    distortionController.Material = material;
                                    distortionController.Renderer = Renderer;
                                    break;
                                case "Hopoo Games/FX/Solid Parallax":
                                    var parallaxController = gameObject.AddComponent<HGSolidParallaxController>();
                                    parallaxController.Material = material;
                                    parallaxController.Renderer = Renderer;
                                    break;
                                case "Hopoo Games/FX/Cloud Remap":
                                    var cloudController = gameObject.AddComponent<HGCloudRemapController>();
                                    cloudController.Material = material;
                                    cloudController.Renderer = Renderer;
                                    break;
                                case "Hopoo Games/FX/Cloud Intersection Remap":
                                    var intersectionController = gameObject.AddComponent<HGIntersectionController>();
                                    intersectionController.Material = material;
                                    intersectionController.Renderer = Renderer;
                                    break;
                            }
                        }
                    }
                }
                Destroy(this);
            }
        }

        /// <summary>
        /// The base class of any shader controller below. Serves to reduce code redundancy, set code standards, and implements a dynamic checking functionality to all inheritors.
        /// </summary>
        public class HGBaseController : MonoBehaviour
        {
            public Material Material;
            public Renderer Renderer;
            public virtual string ShaderName { get; set; }
            public GameObject OwnerGameObject;

            public void Start()
            {
                OwnerGameObject = gameObject;
            }

            public void Update()
            {
                if (!Material || !Renderer)
                {
                    Destroy(this);
                }

                if (Renderer && Material && !Material.shader.name.Contains(ShaderName) || Renderer && Renderer.gameObject != OwnerGameObject)
                {
                    var finder = Renderer.gameObject.AddComponent<HGControllerFinder>();
                    finder.Renderer = Renderer;
                    Destroy(this);
                }
            }
        }

        /// <summary>
        /// Attach to anything, and feed in a material that has the hgstandard shader.
        /// You then gain access to manipulate this in any runtime inspector of your choice.
        /// </summary>
        public class HGStandardController : HGBaseController
        {
            public override string ShaderName => "Hopoo Games/Deferred/Standard";

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
        }

        /// <summary>
        /// Developed by Vale-X. Attach to anything, and feed in a material that has the hgsnowtopped shader.
        /// You then gain access to manipulate this in any Runtime Inspector of your choice.
        /// </summary>
        public class HGSnowToppedController : HGBaseController
        {
            public override string ShaderName => "Hopoo Games/Deferred/Snow Topped";

            public Color _Color { get => Material?.GetColor("_Color") ?? default(Color); set => Material?.SetColor("_Color", value); }
            public Texture _MainTex { get => Material?.GetTexture("_MainTex") ?? null; set => Material?.SetTexture("_MainTex", value); }
            public Vector2 _MainTexScale { get => Material?.GetTextureScale("_MainTex") ?? Vector2.zero; set => Material?.SetTextureScale("_MainTex", value); }
            public Vector2 _MainTexOffset { get => Material?.GetTextureOffset("_MainTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_MainTex", value); }

            public float _NormalStrength { get => Material?.GetFloat("_NormalStrength") ?? 0; set => Material?.SetFloat("_NormalStrength", Mathf.Clamp(value, 0, 1)); }

            public Texture _NormalTex { get => Material?.GetTexture("_NormalTex") ?? null; set => Material?.SetTexture("_NormalTex", value); }
            public Vector2 _NormalTexScale { get => Material?.GetTextureScale("_NormalTex") ?? Vector2.zero; set => Material?.SetTextureScale("_NormalTex", value); }
            public Vector2 _NormalTexOffset { get => Material?.GetTextureOffset("_NormalTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_NormalTex", value); }

            public Texture _SnowTex { get => Material?.GetTexture("_SnowTex") ?? null; set => Material?.SetTexture("_SnowTex", value); }
            public Vector2 _SnowTexScale { get => Material?.GetTextureScale("_SnowTex") ?? Vector2.zero; set => Material?.SetTextureScale("_SnowTex", value); }
            public Vector2 _SnowTexOffset { get => Material?.GetTextureOffset("_SnowTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_SnowTex", value); }

            public Texture _SnowNormalTex { get => Material?.GetTexture("_SnowNormalTex") ?? null; set => Material?.SetTexture("_SnowNormalTex", value); }
            public Vector2 _SnowNormalTexScale { get => Material?.GetTextureScale("_SnowNormalTex") ?? Vector2.zero; set => Material?.SetTextureScale("_SnowNormalTex", value); }
            public Vector2 _SnowNormalTexOffset { get => Material?.GetTextureOffset("_SnowNormalTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_SnowNormalTex", value); }

            public float _SnowBias { get => Material?.GetFloat("_SnowBias") ?? 0; set => Material?.SetFloat("_SnowBias", Mathf.Clamp(value, -1, 1)); }

            public float _Depth { get => Material?.GetFloat("_Depth") ?? 0; set => Material?.SetFloat("_Depth", Mathf.Clamp(value, 0, 1)); }

            public bool _IgnoreAlphaWeights { get => Material?.IsKeywordEnabled("IGNORE_BIAS") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "IGNORE_BIAS"); }
            public bool _BlendWeightsBinarily { get => Material?.IsKeywordEnabled("BINARYBLEND") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "BINARYBLEND"); }

            public enum _RampInfoEnum
            {
                TwoTone = 0,
                SmoothedTwoTone = 1,
                Unlitish = 3,
                Subsurface = 4,
                Grass = 5
            }
            public _RampInfoEnum _RampChoice { get => (_RampInfoEnum)(int)(Material?.GetFloat("_RampInfo") ?? 1); set => Material?.SetFloat("_RampInfo", Convert.ToSingle(value)); }

            public bool _IgnoreDiffuseAlphaForSpeculars { get => Material?.IsKeywordEnabled("FORCE_SPEC") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "FORCE_SPEC"); }

            public float _SpecularStrength { get => Material?.GetFloat("_SpecularStrength") ?? 0; set => Material?.SetFloat("_SpecularStrength", Mathf.Clamp(value, 0, 1)); }

            public float _SpecularExponent { get => Material?.GetFloat("_SpecularExponent") ?? 0; set => Material?.SetFloat("_SpecularExponent", Mathf.Clamp(value, 0.1f, 20)); }

            public float _Smoothness { get => Material?.GetFloat("_Smoothness") ?? 0; set => Material?.SetFloat("_Smoothness", Mathf.Clamp(value, 0, 1)); }

            public float _SnowSpecularStrength { get => Material?.GetFloat("_SnowSpecularStrength") ?? 0; set => Material?.SetFloat("_SnowSpecularStrength", Mathf.Clamp(value, 0, 1)); }

            public float _SnowSpecularExponent { get => Material?.GetFloat("_SnowSpecularExponent") ?? 0; set => Material?.SetFloat("_SnowSpecularExponent", Mathf.Clamp(value, 0.1f, 20)); }

            public float _SnowSmoothness { get => Material?.GetFloat("_SnowSmoothness") ?? 0; set => Material?.SetFloat("_SnowSmoothness", Mathf.Clamp(value, 0, 1)); }

            public float _Fade { get => Material?.GetFloat("_Fade") ?? 0; set => Material?.SetFloat("_Fade", Mathf.Clamp(value, 0, 1)); }

            public bool _DitherOn { get => Material?.IsKeywordEnabled("DITHER") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "DITHER"); }

            public bool _TriplanarOn { get => Material?.IsKeywordEnabled("TRIPLANAR") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "TRIPLANAR"); }

            public float _TriplanarTextureFactor { get => Material?.GetFloat("_TriplanarTextureFactor") ?? 0; set => Material?.SetFloat("_TriplanarTextureFactor", Mathf.Clamp(value, 0, 1)); }

            public bool _SnowOn { get => Material?.IsKeywordEnabled("MICROFACET_SNOW") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "MICROFACET_SNOW"); }

            public bool _GradientBiasOn { get => Material?.IsKeywordEnabled("GRADIENTBIAS") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "GRADIENTBIAS"); }

            public Vector4 _GradientBiasVector { get => Material?.GetVector("_GradientBiasVector") ?? Vector4.zero; set => Material?.SetVector("_GradientBiasVector", value); }

            public bool _DirtOn { get => Material?.IsKeywordEnabled("DIRTON") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "DIRTON"); }

            public Texture _DirtTex { get => Material?.GetTexture("_DirtTex") ?? null; set => Material?.SetTexture("_DirtTex", value); }
            public Vector2 _DirtTexScale { get => Material?.GetTextureScale("_DirtTex") ?? Vector2.zero; set => Material?.SetTextureScale("_DirtTex", value); }
            public Vector2 _DirtTexOffset { get => Material?.GetTextureOffset("_DirtTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_DirtTex", value); }

            public Texture _DirtNormalTex { get => Material?.GetTexture("_DirtNormalTex") ?? null; set => Material?.SetTexture("_DirtNormalTex", value); }
            public Vector2 _DirtNormalTexScale { get => Material?.GetTextureScale("_DirtNormalTex") ?? Vector2.zero; set => Material?.SetTextureScale("_DirtNormalTex", value); }
            public Vector2 _DirtNormalTexOffset { get => Material?.GetTextureOffset("_DirtNormalTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_DirtNormalTex", value); }

            public float _DirtBias { get => Material?.GetFloat("_DirtBias") ?? 0; set => Material?.SetFloat("_DirtBias", Mathf.Clamp(value, -2, 2)); }

            public float _DirtSpecularStrength { get => Material?.GetFloat("_DirtSpecularStrength") ?? 0; set => Material?.SetFloat("_DirtSpecularStrength", Mathf.Clamp(value, 0, 1)); }

            public float _DirtSpecularExponent { get => Material?.GetFloat("_DirtSpecularExponent") ?? 0; set => Material?.SetFloat("_DirtSpecularExponent", Mathf.Clamp(value, 0, 20)); }

            public float _DirtSmoothness { get => Material?.GetFloat("_DirtSmoothness") ?? 0; set => Material?.SetFloat("_DirtSmoothness", Mathf.Clamp(value, 0, 1)); }

        }

        /// <summary>
        /// Attach to anything, and feed in a material that has the hgtriplanarterrainblend shader.
        /// You then gain access to manipulate this in any runtime inspector of your choice.
        /// </summary>
        public class HGTriplanarTerrainBlend : HGBaseController
        {
            public override string ShaderName => "Hopoo Games/Deferred/Triplanar Terrain Blend";
            public bool _Use_Vertex_Colors { get => Material?.IsKeywordEnabled("USE_VERTEX_COLORS") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "USE_VERTEX_COLORS"); }
            public bool _Mix_Vertex_Colors { get => Material?.IsKeywordEnabled("MIX_VERTEX_COLORS") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "MIX_VERTEX_COLORS"); }
            public bool _Use_Alpha_As_Mask { get => Material?.IsKeywordEnabled("USE_ALPHA_AS_MASK") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "USE_ALPHA_AS_MASK"); }
            public bool _Vertical_Bias_On { get => Material?.IsKeywordEnabled("USE_VERTICAL_BIAS") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "USE_VERTICAL_BIAS"); }
            public bool _Double_Sample_On { get => Material?.IsKeywordEnabled("DOUBLESAMPLE") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "DOUBLESAMPLE"); }

            public Color _Color { get => Material?.GetColor("_Color") ?? default(Color); set => Material?.SetColor("_Color", value); }
            public Texture _NormalTex { get => Material?.GetTexture("_NormalTex") ?? null; set => Material?.SetTexture("_NormalTex", value); }
            public Vector2 _NormalTexScale { get => Material?.GetTextureScale("_NormalTex") ?? Vector2.zero; set => Material?.SetTextureScale("_NormalTex", value); }
            public Vector2 _NormalTexOffset { get => Material?.GetTextureOffset("_NormalTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_NormalTex", value); }

            public float _NormalStrength { get => Material?.GetFloat("_NormalStrength") ?? 0; set => Material?.SetFloat("_NormalStrength", Mathf.Clamp(value, 0, 1)); }

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

            public enum _CullEnum
            {
                Off = 0,
                Front = 1,
                Back = 2
            }
            public _CullEnum _Cull_Mode { get => (_CullEnum)(int)(Material?.GetFloat("_Cull") ?? 1); set => Material?.SetFloat("_Cull", Convert.ToSingle(value)); }

            public float _TextureFactor { get => Material?.GetFloat("_TextureFactor") ?? 0; set => Material?.SetFloat("_TextureFactor", Mathf.Clamp(value, 0, 1)); }

            public float _Depth { get => Material?.GetFloat("_Depth") ?? 0; set => Material?.SetFloat("_Depth", Mathf.Clamp(value, 0, 1)); }

            public Texture _SplatmapTex { get => Material?.GetTexture("_SplatmapTex") ?? null; set => Material?.SetTexture("_SplatmapTex", value); }
            public Vector2 _SplatmapTexScale { get => Material?.GetTextureScale("_SplatmapTex") ?? Vector2.zero; set => Material?.SetTextureScale("_SplatmapTex", value); }
            public Vector2 _SplatmapTexOffset { get => Material?.GetTextureOffset("_SplatmapTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_SplatmapTex", value); }

            public Texture _RedChannelTopTex { get => Material?.GetTexture("_RedChannelTopTex") ?? null; set => Material?.SetTexture("_RedChannelTopTex", value); }
            public Vector2 _RedChannelTopTexScale { get => Material?.GetTextureScale("_RedChannelTopTex") ?? Vector2.zero; set => Material?.SetTextureScale("_RedChannelTopTex", value); }
            public Vector2 _RedChannelTopTexOffset { get => Material?.GetTextureOffset("_RedChannelTopTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_RedChannelTopTex", value); }
            public Texture _RedChannelSideTex { get => Material?.GetTexture("_RedChannelSideTex") ?? null; set => Material?.SetTexture("_RedChannelSideTex", value); }
            public Vector2 _RedChannelSideTexScale { get => Material?.GetTextureScale("_RedChannelSideTex") ?? Vector2.zero; set => Material?.SetTextureScale("_RedChannelSideTex", value); }
            public Vector2 _RedChannelSideTexOffset { get => Material?.GetTextureOffset("_RedChannelSideTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_RedChannelSideTex", value); }
            public float _RedChannelSmoothness { get => Material?.GetFloat("_RedChannelSmoothness") ?? 0; set => Material?.SetFloat("_RedChannelSmoothness", Mathf.Clamp(value, 0, 1)); }
            public float _RedChannelSpecularStrength { get => Material?.GetFloat("_RedChannelSpecularStrength") ?? 0; set => Material?.SetFloat("_RedChannelSpecularStrength", Mathf.Clamp(value, 0, 1)); }
            public float _RedChannelSpecularExponent { get => Material?.GetFloat("_RedChannelSpecularExponent") ?? 0; set => Material?.SetFloat("_RedChannelSpecularExponent", Mathf.Clamp(value, 0.1f, 20)); }
            public float _RedChannelBias { get => Material?.GetFloat("_RedChannelBias") ?? 0; set => Material?.SetFloat("_RedChannelBias", Mathf.Clamp(value, -2, 5)); }

            public Texture _GreenChannelTex { get => Material?.GetTexture("_GreenChannelTex") ?? null; set => Material?.SetTexture("_GreenChannelTex", value); }
            public Vector2 _GreenChannelTexScale { get => Material?.GetTextureScale("_GreenChannelTex") ?? Vector2.zero; set => Material?.SetTextureScale("_GreenChannelTex", value); }
            public Vector2 _GreenChannelTexOffset { get => Material?.GetTextureOffset("_GreenChannelTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_GreenChannelTex", value); }
            public float _GreenChannelSmoothness { get => Material?.GetFloat("_GreenChannelSmoothness") ?? 0; set => Material?.SetFloat("_GreenChannelSmoothness", Mathf.Clamp(value, 0, 1)); }
            public float _GreenChannelSpecularStrength { get => Material?.GetFloat("_GreenChannelSpecularStrength") ?? 0; set => Material?.SetFloat("_GreenChannelSpecularStrength", Mathf.Clamp(value, 0, 1)); }
            public float _GreenChannelSpecularExponent { get => Material?.GetFloat("_GreenChannelSpecularExponent") ?? 0; set => Material?.SetFloat("_GreenChannelSpecularExponent", Mathf.Clamp(value, 0.1f, 20)); }
            public float _GreenChannelBias { get => Material?.GetFloat("_GreenChannelBias") ?? 0; set => Material?.SetFloat("_GreenChannelBias", Mathf.Clamp(value, -2, 5)); }

            public Texture _BlueChannelTex { get => Material?.GetTexture("_RedChannelTopTex") ?? null; set => Material?.SetTexture("_BlueChannelTex", value); }
            public Vector2 _BlueChannelTexScale { get => Material?.GetTextureScale("_BlueChannelTex") ?? Vector2.zero; set => Material?.SetTextureScale("_BlueChannelTex", value); }
            public Vector2 _BlueChannelTexOffset { get => Material?.GetTextureOffset("_BlueChannelTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_BlueChannelTex", value); }
            public float _BlueChannelSmoothness { get => Material?.GetFloat("_BlueChannelSmoothness") ?? 0; set => Material?.SetFloat("_BlueChannelSmoothness", Mathf.Clamp(value, 0, 1)); }
            public float _BlueChannelSpecularStrength { get => Material?.GetFloat("_BlueChannelSpecularStrength") ?? 0; set => Material?.SetFloat("_BlueChannelSpecularStrength", Mathf.Clamp(value, 0, 1)); }
            public float _BlueChannelSpecularExponent { get => Material?.GetFloat("_BlueChannelSpecularExponent") ?? 0; set => Material?.SetFloat("_BlueChannelSpecularExponent", Mathf.Clamp(value, 0.1f, 20)); }
            public float _BlueChannelBias { get => Material?.GetFloat("_BlueChannelBias") ?? 0; set => Material?.SetFloat("_BlueChannelBias", Mathf.Clamp(value, -2, 5)); }

            public bool _Treat_Green_Channel_As_Snow { get => Material?.IsKeywordEnabled("MICROFACET_SNOW") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "MICROFACET_SNOW"); }
        }

        /// <summary>
        /// Attach to anything, and feed in a material that has the hgdistortion shader.
        /// You then gain access to manipulate this in any runtime inspector of your choice.
        /// </summary>
        public class HGDistortionController : HGBaseController
        {
            public Texture _BumpMap { get => Material?.GetTexture("_BumpMap") ?? null; set => Material?.SetTexture("_BumpMap", value); }
            public Vector2 _BumpMapScale { get => Material?.GetTextureScale("_BumpMap") ?? Vector2.zero; set => Material?.SetTextureScale("_BumpMap", value); }
            public Vector2 _BumpMapOffset { get => Material?.GetTextureOffset("_BumpMap") ?? Vector2.zero; set => Material?.SetTextureOffset("_BumpMap", value); }

            public Texture _MaskTex { get => Material?.GetTexture("_MaskTex") ?? null; set => Material?.SetTexture("_MaskTex", value); }
            public Vector2 _MaskTexScale { get => Material?.GetTextureScale("_MaskTex") ?? Vector2.zero; set => Material?.SetTextureScale("_MaskTex", value); }
            public Vector2 _MaskTexOffset { get => Material?.GetTextureOffset("_MaskTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_MaskTex", value); }

            public float _Magnitude { get => Material?.GetFloat("_Magnitude") ?? 0; set => Material?.SetFloat("_Magnitude", Mathf.Clamp(value, 0, 10)); }

            public float _NearFadeZeroDistance { get => Material?.GetFloat("_NearFadeZeroDistance") ?? 0; set => Material?.SetFloat("_NearFadeZeroDistance", value); }
            public float _NearFadeOneDistance { get => Material?.GetFloat("_NearFadeOneDistance") ?? 0; set => Material?.SetFloat("_NearFadeOneDistance", value); }
            public float _FarFadeOneDistance { get => Material?.GetFloat("_FarFadeOneDistance") ?? 0; set => Material?.SetFloat("_FarFadeOneDistance", value); }
            public float _FarFadeZeroDistance { get => Material?.GetFloat("_FarFadeZeroDistance") ?? 0; set => Material?.SetFloat("_FarFadeZeroDistance", value); }

            public bool _DistanceModulationOn { get => Material?.IsKeywordEnabled("DISTANCEMODULATION") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "DISTANCEMODULATION"); }

            public float _DistanceModulationMagnitude { get => Material?.GetFloat("_DistanceModulationMagnitude") ?? 0; set => Material?.SetFloat("_DistanceModulationMagnitude", Mathf.Clamp(value, 0, 1)); }
            public float _InvFade { get => Material?.GetFloat("_InvFade") ?? 0; set => Material?.SetFloat("_InvFade", Mathf.Clamp(value, 0, 2)); }


        }

        /// <summary>
        /// Attach to anything, and feed in a material that has the hgsolidparallax shader.
        /// You then gain access to manipulate this in any runtime inspector of your choice.
        /// </summary>
        public class HGSolidParallaxController : HGBaseController
        {
            public override string ShaderName => "Hopoo Games/FX/Solid Parallax";
            public string MaterialName { get => Material?.name ?? ""; }
            public Color _Color { get => Material?.GetColor("_Color") ?? default(Color); set => Material?.SetColor("_Color", value); }
            public Texture _MainTex { get => Material?.GetTexture("_MainTex") ?? null; set => Material?.SetTexture("_MainTex", value); }
            public Vector2 _MainTexScale { get => Material?.GetTextureScale("_MainTex") ?? Vector2.zero; set => Material?.SetTextureScale("_MainTex", value); }
            public Vector2 _MainTexOffset { get => Material?.GetTextureOffset("_MainTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_MainTex", value); }
            public Texture _EmissionTex { get => Material?.GetTexture("_EmissionTex") ?? null; set => Material?.SetTexture("_EmissionTex", value); }
            public Vector2 _EmissionTexScale { get => Material?.GetTextureScale("_EmissionTex") ?? Vector2.zero; set => Material?.SetTextureScale("_EmissionTex", value); }
            public Vector2 _EmissionTexOffset { get => Material?.GetTextureOffset("_EmissionTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_EmissionTex", value); }
            public float _EmissionPower { get => Material?.GetFloat("_EmissionPower") ?? 0; set => Material?.SetFloat("_EmissionPower", Mathf.Clamp(value, 0.1f, 20)); }
            public Texture _Normal { get => Material?.GetTexture("_Normal") ?? null; set => Material?.SetTexture("_Normal", value); }
            public Vector2 _NormalScale { get => Material?.GetTextureScale("_Normal") ?? Vector2.zero; set => Material?.SetTextureScale("_Normal", value); }
            public Vector2 _NormalOffset { get => Material?.GetTextureOffset("_Normal") ?? Vector2.zero; set => Material?.SetTextureOffset("_Normal", value); }
            public float _SpecularStrength { get => Material?.GetFloat("_SpecularStrength") ?? 0; set => Material?.SetFloat("_SpecularStrength", Mathf.Clamp(value, 0, 1)); }
            public float _SpecularExponent { get => Material?.GetFloat("_SpecularExponent") ?? 0; set => Material?.SetFloat("_SpecularExponent", Mathf.Clamp(value, 0.1f, 20)); }
            public float _Smoothness { get => Material?.GetFloat("_Smoothness") ?? 0; set => Material?.SetFloat("_Smoothness", Mathf.Clamp(value, 0f, 1)); }
            public Texture _Height1 { get => Material?.GetTexture("_Height1") ?? null; set => Material?.SetTexture("_Height1", value); }
            public Vector2 _Height1Scale { get => Material?.GetTextureScale("_Height1") ?? Vector2.zero; set => Material?.SetTextureScale("_Height1", value); }
            public Vector2 _Height1Offset { get => Material?.GetTextureOffset("_Height1") ?? Vector2.zero; set => Material?.SetTextureOffset("_Height1", value); }
            public Texture _Height2 { get => Material?.GetTexture("_Height2") ?? null; set => Material?.SetTexture("_Height2", value); }
            public Vector2 _Height2Scale { get => Material?.GetTextureScale("_Height2") ?? Vector2.zero; set => Material?.SetTextureScale("_Height2", value); }
            public Vector2 _Height2Offset { get => Material?.GetTextureOffset("_Height2") ?? Vector2.zero; set => Material?.SetTextureOffset("_Height2", value); }
            public float _HeightStrength { get => Material?.GetFloat("_HeightStrength") ?? 0; set => Material?.SetFloat("_HeightStrength", Mathf.Clamp(value, 0f, 20)); }
            public float _HeightBias { get => Material?.GetFloat("_HeightBias") ?? 0; set => Material?.SetFloat("_HeightBias", Mathf.Clamp(value, 0f, 1)); }
            public float _Parallax { get => Material?.GetFloat("_Parallax") ?? 0; set => Material?.SetFloat("_Parallax", value); }
            public Vector4 _ScrollSpeed { get => Material?.GetVector("_ScrollSpeed") ?? Vector4.zero; set => Material?.SetVector("_ScrollSpeed", value); }

            public enum _RampInfoEnum
            {
                TwoTone = 0,
                SmoothedTwoTone = 1,
                Unlitish = 3,
                Subsurface = 4,
                Grass = 5
            }
            public _RampInfoEnum _RampInfo { get => (_RampInfoEnum)(int)(Material?.GetFloat("_RampInfo") ?? 1); set => Material?.SetFloat("_RampInfo", Convert.ToSingle(value)); }

            public enum _CullEnum
            {
                Off = 0,
                Front = 1,
                Back = 2
            }
            public _CullEnum _Cull_Mode { get => (_CullEnum)(int)(Material?.GetFloat("_Cull") ?? 1); set => Material?.SetFloat("_Cull", Convert.ToSingle(value)); }

            public bool _ClipOn { get => Material?.IsKeywordEnabled("ALPHACLIP") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "ALPHACLIP"); }
        }

        /// <summary>
        /// Attach to anything, and feed in a material that has the hgcloudremap shader.
        /// You then gain access to manipulate this in any Runtime Inspector of your choice.
        /// </summary>
        public class HGCloudRemapController : HGBaseController
        {
            public override string ShaderName => "Hopoo Games/FX/Cloud Remap";
            public string MaterialName { get => Material?.name ?? ""; }

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
        }

        /// <summary>
        /// Attach to anything, and feed in a material that has the hgcloudintersectionremap shader.
        /// You then gain access to manipulate this in any Runtime Inspector of your choice.
        /// </summary>
        public class HGIntersectionController : HGBaseController
        {
            public override string ShaderName => "Hopoo Games/FX/Cloud Intersection Remap";
            public string MaterialName { get => Material?.name ?? ""; }

            public UnityEngine.Rendering.BlendMode _Source_Blend_Mode { get => (UnityEngine.Rendering.BlendMode)(int)(Material?.GetFloat("_SrcBlendFloat") ?? 1); set => Material?.SetFloat("_SrcBlendFloat", Convert.ToSingle(value)); }
            public UnityEngine.Rendering.BlendMode _Destination_Blend_Mode { get => (UnityEngine.Rendering.BlendMode)(int)(Material?.GetFloat("_DstBlendFloat") ?? 1); set => Material?.SetFloat("_DstBlendFloat", Convert.ToSingle(value)); }

            public Color _Tint { get => Material?.GetColor("_TintColor") ?? default(Color); set => Material?.SetColor("_TintColor", value); }
            public Texture _MainTex { get => Material?.GetTexture("_MainTex") ?? null; set => Material?.SetTexture("_MainTex", value); }
            public Vector2 _MainTexScale { get => Material?.GetTextureScale("_MainTex") ?? Vector2.zero; set => Material?.SetTextureScale("_MainTex", value); }
            public Vector2 _MainTexOffset { get => Material?.GetTextureOffset("_MainTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_MainTex", value); }
            public Texture _Cloud1Tex { get => Material?.GetTexture("_Cloud1Tex") ?? null; set => Material?.SetTexture("_Cloud1Tex", value); }
            public Vector2 _Cloud1TexScale { get => Material?.GetTextureScale("_Cloud1Tex") ?? Vector2.zero; set => Material?.SetTextureScale("_Cloud1Tex", value); }
            public Vector2 _Cloud1TexOffset { get => Material?.GetTextureOffset("_Cloud1Tex") ?? Vector2.zero; set => Material?.SetTextureOffset("_Cloud1Tex", value); }
            public Texture _Cloud2Tex { get => Material?.GetTexture("_Cloud2Tex") ?? null; set => Material?.SetTexture("_Cloud2Tex", value); }
            public Vector2 _Cloud2TexScale { get => Material?.GetTextureScale("_Cloud2Tex") ?? Vector2.zero; set => Material?.SetTextureScale("_Cloud2Tex", value); }
            public Vector2 _Cloud2TexOffset { get => Material?.GetTextureOffset("_Cloud2Tex") ?? Vector2.zero; set => Material?.SetTextureOffset("_Cloud2Tex", value); }
            public Texture _RemapTex { get => Material?.GetTexture("_RemapTex") ?? null; set => Material?.SetTexture("_RemapTex", value); }
            public Vector2 _RemapTexScale { get => Material?.GetTextureScale("_RemapTex") ?? Vector2.zero; set => Material?.SetTextureScale("_RemapTex", value); }
            public Vector2 _RemapTexOffset { get => Material?.GetTextureOffset("_RemapTex") ?? Vector2.zero; set => Material?.SetTextureOffset("_RemapTex", value); }
            public Vector4 _CutoffScroll { get => Material?.GetVector("_CutoffScroll") ?? Vector4.zero; set => Material?.SetVector("_CutoffScroll", value); }

            public float _SoftFactor { get => Material?.GetFloat("_InvFade") ?? 0; set => Material?.SetFloat("_InvFade", Mathf.Clamp(value, 0, 30)); }

            public float _SoftPower { get => Material?.GetFloat("_SoftPower") ?? 0; set => Material?.SetFloat("_SoftPower", Mathf.Clamp(value, 0.1f, 20)); }

            public float _BrightnessBoost { get => Material?.GetFloat("_Boost") ?? 0; set => Material?.SetFloat("_Boost", Mathf.Clamp(value, 0, 5)); }

            public float _RimPower { get => Material?.GetFloat("_RimPower") ?? 0; set => Material?.SetFloat("_RimPower", Mathf.Clamp(value, 0.1f, 20)); }

            public float _RimStrength { get => Material?.GetFloat("_RimStrength") ?? 0; set => Material?.SetFloat("_RimStrength", Mathf.Clamp(value, 0, 5)); }

            public float _AlphaBoost { get => Material?.GetFloat("_AlphaBoost") ?? 0; set => Material?.SetFloat("_AlphaBoost", Mathf.Clamp(value, 0, 20)); }

            public float _IntersectionStrength { get => Material?.GetFloat("_IntersectionStrength") ?? 0; set => Material?.SetFloat("_IntersectionStrength", Mathf.Clamp(value, 0, 20)); }

            public enum _CullEnum
            {
                Off = 0,
                Front = 1,
                Back = 2
            }
            public _CullEnum _Cull_Mode { get => (_CullEnum)(int)(Material?.GetFloat("_Cull") ?? 1); set => Material?.SetFloat("_Cull", Convert.ToSingle(value)); }

            public float _ExternalAlpha { get => Material?.GetFloat("_ExternalAlpha") ?? 0; set => Material?.SetFloat("_ExternalAlpha", Mathf.Clamp(value, 0, 1)); }

            public bool _FadeFromVertexColorsOn { get => Material?.IsKeywordEnabled("FADE_FROM_VERTEX_COLORS") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "FADE_FROM_VERTEX_COLORS"); }
            public bool _EnableTriplanarProjectionsForClouds { get => Material?.IsKeywordEnabled("TRIPLANAR") ?? false; set => SetShaderKeywordBasedOnBool(value, Material, "TRIPLANAR"); }

        }
    }
}
