//
// OpenSoM Runtime Project Shader
// Extremely stripped back Universal Render Pipeline/Simple Lit
//
Shader "OpenSoM/Object (Texture, Lit, Simple)"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" {}
        [MainColor]   _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _EmissionColor("Emissive Color", Color) = (0, 0, 0, 0)
        _ScrollParams("Scroll Params", Vector) = (0, 0, 0, 0)
        _SrcBlend("__src", Float) = 1.0
        _DstBlend("__dst", Float) = 0.0
        _SrcBlendAlpha("__srcA", Float) = 1.0
        _DstBlendAlpha("__dstA", Float) = 0.0
        _FogMultiplier("__fogMult", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Opaque"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            // -------------------------------------
            // Render State Commands
            // Use same blending / depth states as Standard shader
            Blend[_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma target 4.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON
            #pragma shader_feature_local_fragment _ _SPECGLOSSMAP _SPECULAR_COLOR
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fragment _ LIGHTMAP_BICUBIC_SAMPLING
            #pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            //--------------------------------------
            // Defines
            #define BUMP_SCALE_NOT_SUPPORTED 1

            // -------------------------------------
            // Simple Lit Input
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseMap_TexelSize;
            half4 _BaseColor;
            half4 _EmissionColor;
            half4 _ScrollParams;
            half _FogMultiplier;
            UNITY_TEXTURE_STREAMING_DEBUG_VARS;
            CBUFFER_END

                #ifdef UNITY_DOTS_INSTANCING_ENABLED
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
                UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
                UNITY_DOTS_INSTANCED_PROP(float4, _ScrollParams)
                UNITY_DOTS_INSTANCED_PROP(float, _FogMultiplier);
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

                static float4 unity_DOTS_Sampled_BaseColor;
                static float4 unity_DOTS_Sampled_EmissionColor;
                static float4 unity_DOTS_Sampled_ScrollParams;
                static float  unity_DOTS_Sampled_FogMultiplier;

                void SetupDOTSSimpleLitMaterialPropertyCaches()
                {
                    unity_DOTS_Sampled_BaseColor = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor);
                    unity_DOTS_Sampled_EmissionColor = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _EmissionColor);
                    unity_DOTS_Sampled_ScrollParams = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _ScrollParams);
                    unity_DOTS_Sampled_FogMultiplier = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _FogMultiplier);
                }

                #undef UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES
                #define UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES() SetupDOTSSimpleLitMaterialPropertyCaches()
                #define _BaseColor          unity_DOTS_Sampled_BaseColor
                #define _EmissionColor      unity_DOTS_Sampled_EmissionColor
                #define _ScrollParams       unity_DOTS_Sampled_ScrollParams
                #define _FogMultiplier      unity_DOTS_Sampled_FogMultiplier
            #endif

            // -------------------------------------
            // Simple Lit Forward Pass
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // P2V -> Program To Vertex - needs huge optimization.
            // FOR TILES:
            //  We only need Position, Normal and UV... Maybe tangent depending on how unity lighting calculations are performed.
            struct P2V
            {
                float4 positionOS           : POSITION;
                uint   normalOS             : NORMAL;
                uint   tangentOS            : TANGENT;
                float2 texcoord             : TEXCOORD0;
                uint4  colour               : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // V2F -> Vertex To Fragment
            struct V2F
            {
                float2 uv                      : TEXCOORD0;
                float3 positionWS              : TEXCOORD1;    // xyz: posWS
                half3  normalWS                : TEXCOORD2;
                half4  colour                  : TEXCOORD3;

                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);

                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    float4 shadowCoord             : TEXCOORD6;
                #endif

                #ifdef USE_APV_PROBE_OCCLUSION
                    float4 probeOcclusion : TEXCOORD9;
                #endif

                float4 positionCS                  : SV_POSITION;

                // GPU Instancing
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Helper to decode 10-10-10-02 encoded normals
            float3 Decode1010102(uint packed)
            {
                int3 i;
                i.x = (int)(packed << 22) >> 22;
                i.y = (int)(packed << 12) >> 22;
                i.z = (int)(packed << 2) >> 22;

                float3 n = (float3)i * (1.0 / 511.0);

                return normalize(n);
            }

            //
            // URP Simple Lit Vertex Function
            //
            V2F LitPassVertexSimple(P2V input)
            {
                V2F output = (V2F)0;

                // GPU Instancing
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                // Transform the object space vertex into world space and clip space
                float3 vertexWorldSpace = TransformObjectToWorld(input.positionOS.xyz);
                float4 vertexClipSpace  = TransformWorldToHClip(vertexWorldSpace);

                output.positionWS.xyz = vertexWorldSpace;
                output.positionCS = vertexClipSpace;

                // Transform the object space normal into world space
                output.normalWS = TransformObjectToWorldNormal(Decode1010102(input.normalOS));

                // UV Transformation by texture, additionally applying UV scroll
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.uv = output.uv + (_ScrollParams.xy * _ScrollParams.zw) * _Time.x;

                // Must normalise our colour...
                output.colour = input.colour / 255.0;

                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
                        output.shadowCoord = ComputeScreenPos(vertexClipSpace);
                    #else
                        output.shadowCoord = TransformWorldToShadowCoord(vertexWorldSpace);
                    #endif
                #endif

                return output;
            }

            //
            // URP Simple Lit Fragment Function
            //
            half OpenSoMAttenuation(float3 lightPosWS, float3 positionWS, float range, half a0 = 1.0h, half a1 = 0.4h, half a2 = 0.0h)
            {
                float d = distance(lightPosWS, positionWS);

                if (d > range)
                    return 0.0h;

                half denominator = a0 + (a1 * d) + (a2 * d * d);
                half atten = (denominator > 0.0001h) ? (1.0h / denominator) : 0.0h;

                return saturate(atten) * saturate(1.0h - (d / range));
            }

            half3 OpenSoMLambert(half3 lightDirection, half3 surfaceNormal)
            {
                return saturate(dot(surfaceNormal, lightDirection));
            }

            half3 OpenSoMBlinnPhong(Light light, half attenuation)
            {
                return light.color * (attenuation * light.shadowAttenuation);
            }

            half3 OpenSoMBlinnPhongFragment(InputData inputData, half3 albedo)
            {
                // RealtimeLights>CalculateShadowMask
                half4 shadowMask = half4(1.0, 1.0, 1.0, 1.0);

                half3 additionalLightsColour = 0;

                // UNCLEANED BELOW
                #if defined(_ADDITIONAL_LIGHTS)
                #if USE_CLUSTER_LIGHT_LOOP
                [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                {
                    CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
                        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);

                    // This can be massively optimised by not using default GetAdditionalLight, which is doubling some of this work
                    // internally.
                    half distAtten = light.distanceAttenuation;
                #if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
                    if (_AdditionalLightsBuffer[lightIndex].position.w != 0.0f)
                #else
                    if (_AdditionalLightsPosition[lightIndex].w != 0.0f)
                #endif
                    {
                #if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
                        float3 lightPosWS = _AdditionalLightsBuffer[lightIndex].position.xyz;
                        float attenX = _AdditionalLightsBuffer[lightIndex].attenuation.x;
                #else
                        float3 lightPosWS = _AdditionalLightsPosition[lightIndex].xyz;
                        float attenX = _AdditionalLightsAttenuation[lightIndex].x;
                #endif

                        float range = (attenX > 0.0001f) ? rsqrt(attenX) : 10.0f;

                        distAtten = OpenSoMAttenuation(lightPosWS, inputData.positionWS, range, 1.0h, 0.4h, 0.0h);
                    }

                    half3 calcLight = OpenSoMBlinnPhong(light, distAtten);
                    calcLight *= OpenSoMLambert(light.direction, inputData.normalWS);
                    calcLight *= albedo;

                    additionalLightsColour += calcLight;
                }
                #else
                int pixelLightCount = int(min(_AdditionalLightsCount.x, unity_LightData.y));
                #endif

                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);

                // This can be massively optimised by not using default GetAdditionalLight, which is doubling some of this work
                // internally.
                half distAtten = light.distanceAttenuation;
                #if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
                if (_AdditionalLightsBuffer[lightIndex].position.w != 0.0f)
                #else
                if (_AdditionalLightsPosition[lightIndex].w != 0.0f)
                #endif
                {
                    #if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
                    float3 lightPosWS = _AdditionalLightsBuffer[lightIndex].position.xyz;
                    float attenX = _AdditionalLightsBuffer[lightIndex].attenuation.x;
                    #else
                    float3 lightPosWS = _AdditionalLightsPosition[lightIndex].xyz;
                    float attenX = _AdditionalLightsAttenuation[lightIndex].x;
                    #endif

                    float range = (attenX > 0.0001f) ? rsqrt(attenX) : 10.0f;

                    distAtten = OpenSoMAttenuation(lightPosWS, inputData.positionWS, range, 1.0h, 0.4h, 0.0h);
                }

                half3 calcLight = OpenSoMBlinnPhong(light, distAtten);
                calcLight *= OpenSoMLambert(light.direction, inputData.normalWS);
                calcLight *= albedo;

                additionalLightsColour += calcLight;
                LIGHT_LOOP_END
#endif

                    // RealtimeLights>GetMainLight
                    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);

                half3 calcMainLight = OpenSoMBlinnPhong(mainLight, mainLight.distanceAttenuation);
                calcMainLight *= OpenSoMLambert(mainLight.direction, inputData.normalWS);
                calcMainLight *= albedo;

                // Final Lighting Calculation
                half3 lightBlendCalc = half3(0.0, 0.0, 0.0);
                lightBlendCalc += (inputData.bakedGI * albedo);
                lightBlendCalc += calcMainLight;
                lightBlendCalc += additionalLightsColour;
                lightBlendCalc += _EmissionColor.rgb;

                return lightBlendCalc;
            }

            half4 LitPassFragmentSimple(V2F input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // Sample main texture - what is SampleAlbedoAlpha and what fuckery is it secretly doing?
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;

                // Custom colour key logic... keys out any pixel with no channel above than 4 linear rgb steps
                clip(max(albedo.r, max(albedo.g, albedo.b)) - 0.0003);

                // Apply base colour...
                albedo *= _BaseColor.rgb * input.colour;

                // Copied from 'InitializeInputData' - Cleaner but still pathetic
                InputData inputData       = (InputData)0;
                inputData.positionWS      = input.positionWS;
                inputData.normalWS        = input.normalWS;        
                inputData.viewDirectionWS = SafeNormalize(GetWorldSpaceNormalizeViewDir(inputData.positionWS));

                // Sets the shadow map sampling coordinates
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    inputData.shadowCoord = input.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
                #else
                    inputData.shadowCoord = float4(0, 0, 0, 0);
                #endif

                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                // I believe this single function is responsible for all ambient lighting? Including shadow masking... Weird.
                inputData.bakedGI    = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);

                // Apply Lighting (Extracted & cleaned up from Lighting>UniversalFragmentBlinnPhong)
                half3 colour = OpenSoMBlinnPhongFragment(inputData, albedo);

                // Apply Fog
                half viewZ = -(dot(UNITY_MATRIX_V[2].xyz, inputData.positionWS) + UNITY_MATRIX_V[2].w);
                half fogFactor = saturate(mad(viewZ, unity_FogParams.z, unity_FogParams.w - _ProjectionParams.y * unity_FogParams.z));
                colour.rgb = lerp(lerp(colour, half3(unity_FogColor.rgb), _FogMultiplier), colour, fogFactor);

                return half4(colour.rgb, 1.0);
            }

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Front

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #ifndef HAVE_VFX_MODIFICATION
                #pragma multi_compile _ DOTS_INSTANCING_ON
                #if UNITY_PLATFORM_ANDROID || (UNITY_PLATFORM_WEBGL && !SHADER_API_WEBGPU) || UNITY_PLATFORM_UWP
                    #pragma target 3.5 DOTS_INSTANCING_ON
                #else
                    #pragma target 4.5 DOTS_INSTANCING_ON
                #endif
            #endif // HAVE_VFX_MODIFICATION

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            // -------------------------------------
            // Simple Lit Input
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseMap_TexelSize;
                half4 _BaseColor;
                half4 _EmissionColor;
                half4 _ScrollParams;
                half _FogMultiplier;
                UNITY_TEXTURE_STREAMING_DEBUG_VARS;
            CBUFFER_END

            #ifdef UNITY_DOTS_INSTANCING_ENABLED
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
                UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
                UNITY_DOTS_INSTANCED_PROP(float4, _ScrollParams)
                UNITY_DOTS_INSTANCED_PROP(float, _FogMultiplier);
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

                static float4 unity_DOTS_Sampled_BaseColor;
                static float4 unity_DOTS_Sampled_EmissionColor;
                static float4 unity_DOTS_Sampled_ScrollParams;
                static float  unity_DOTS_Sampled_FogMultiplier;

                void SetupDOTSSimpleLitMaterialPropertyCaches()
                {
                    unity_DOTS_Sampled_BaseColor = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor);
                    unity_DOTS_Sampled_EmissionColor = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _EmissionColor);
                    unity_DOTS_Sampled_ScrollParams = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _ScrollParams);
                    unity_DOTS_Sampled_FogMultiplier = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _FogMultiplier);
                }

                #undef UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES
                #define UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES() SetupDOTSSimpleLitMaterialPropertyCaches()
                #define _BaseColor          unity_DOTS_Sampled_BaseColor
                #define _EmissionColor      unity_DOTS_Sampled_EmissionColor
                #define _ScrollParams       unity_DOTS_Sampled_ScrollParams
                #define _FogMultiplier      unity_DOTS_Sampled_FogMultiplier
            #endif

            //
            // From Shadow Caster Pass
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            // Shadow Casting Light geometric parameters. These variables are used when applying the shadow Normal Bias and are set by UnityEngine.Rendering.Universal.ShadowUtils.SetupShadowCasterConstantBuffer in com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs
            // For Directional lights, _LightDirection is used when applying shadow Normal Bias.
            // For Spot lights and Point lights, _LightPosition is used to compute the actual light direction because it is different at each shadow caster geometry vertex.
            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 texcoord     : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS   = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                positionCS = ApplyShadowClamping(positionCS);
                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = GetShadowPositionHClip(input);
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // Custom colour key logic... keys out any pixel with no channel above than 4 linear rgb steps
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
                clip(max(albedo.r, max(albedo.g, albedo.b)) - 0.0003);

                return 0;
            }

            ENDHLSL
        }
    }

    Fallback  "Hidden/Universal Render Pipeline/FallbackError"
}
