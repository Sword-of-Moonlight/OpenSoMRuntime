//
// OpenSoM Runtime Project Shader
// Based on Universal Render Pipeline/Simple Lit
//
Shader "OpenSoM/Tile (Texture, Lit, Simple)"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" {}
        [MainColor]   _BaseColor("Base Color", Color) = (1, 1, 1, 1)

        // This is bull shit required by the SRP batcher. I'd love to get rid of it.
        _Surface("__surface", Float) = 0.0

        // I want to optimize these out, but I'll be leaving it until later.
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
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
            half4 _SpecColor;
            half4 _EmissionColor;
            half _Cutoff;
            half _Surface;
            UNITY_TEXTURE_STREAMING_DEBUG_VARS;
            CBUFFER_END

            #ifdef UNITY_DOTS_INSTANCING_ENABLED
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
                UNITY_DOTS_INSTANCED_PROP(float4, _SpecColor)
                UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
                UNITY_DOTS_INSTANCED_PROP(float, _Cutoff)
                UNITY_DOTS_INSTANCED_PROP(float, _Surface)
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

                static float4 unity_DOTS_Sampled_BaseColor;
                static float4 unity_DOTS_Sampled_SpecColor;
                static float4 unity_DOTS_Sampled_EmissionColor;
                static float  unity_DOTS_Sampled_Cutoff;
                static float  unity_DOTS_Sampled_Surface;

                void SetupDOTSSimpleLitMaterialPropertyCaches()
                {
                    unity_DOTS_Sampled_BaseColor = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor);
                    unity_DOTS_Sampled_SpecColor = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _SpecColor);
                    unity_DOTS_Sampled_EmissionColor = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _EmissionColor);
                    unity_DOTS_Sampled_Cutoff = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Cutoff);
                    unity_DOTS_Sampled_Surface = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Surface);
                }

                #undef UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES
                #define UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES() SetupDOTSSimpleLitMaterialPropertyCaches()
                #define _BaseColor          unity_DOTS_Sampled_BaseColor
                #define _SpecColor          unity_DOTS_Sampled_SpecColor
                #define _EmissionColor      unity_DOTS_Sampled_EmissionColor
                #define _Cutoff             unity_DOTS_Sampled_Cutoff
                #define _Surface            unity_DOTS_Sampled_Surface
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


                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    float4 shadowCoord             : TEXCOORD6;
                #endif

                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);

                #ifdef USE_APV_PROBE_OCCLUSION
                    float4 probeOcclusion : TEXCOORD9;
                #endif

                float4 positionCS                  : SV_POSITION;

                // GPU Instancing
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            //
            // URP Simple Lit Vertex Function
            //
            float3 Decode1010102(uint packed)
            {
                int3 i;
                i.x = (int)(packed << 22) >> 22;
                i.y = (int)(packed << 12) >> 22;
                i.z = (int)(packed << 2) >> 22;

                float3 n = (float3)i * (1.0 / 511.0);

                return normalize(n);
            }

            V2F LitPassVertexSimple(P2V input)
            {
                V2F output = (V2F)0;

                // GPU Instancing
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                // Actual fucking transfer (NEEDS CLEAN UP)
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput   = GetVertexNormalInputs(Decode1010102(input.normalOS), float4(Decode1010102(input.tangentOS), 1.0));

                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionWS.xyz = vertexInput.positionWS;
                output.positionCS     = vertexInput.positionCS;

                output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);

                OUTPUT_SH4(vertexInput.positionWS, output.normalWS.xyz, GetWorldSpaceNormalizeViewDir(vertexInput.positionWS), output.vertexSH, output.probeOcclusion);

                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    output.shadowCoord = GetShadowCoord(vertexInput);
                #endif

                return output;
            }

            /*
            half3 OpenSoMLightDiffuseFunc(half3 lightDirection, half3 viewDirection, half3 surfaceNormal)
            {
                float NdotL = saturate(dot(surfaceNormal, lightDirection));
                float NdotV = saturate(dot(surfaceNormal, viewDirection));
                float minnaertPower = 1.0;

                return NdotL * pow(NdotV, minnaertPower);
            }
            */

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
                                float attenX      = _AdditionalLightsBuffer[lightIndex].attenuation.x;
                                #else
                                float3 lightPosWS = _AdditionalLightsPosition[lightIndex].xyz;
                                float attenX      = _AdditionalLightsAttenuation[lightIndex].x;
                                #endif

                                float range       = (attenX > 0.0001f) ? rsqrt(attenX) : 10.0f;

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
                // lightBlendCalc += _EmissionColor.rgb;

                return lightBlendCalc;
            }

            half4 LitPassFragmentSimple(V2F input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // Sample main texture - what is SampleAlbedoAlpha and what fuckery is it secretly doing?
                half3 albedo = SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).rgb;

                // Custom colour key logic... keys out any pixel with no channel above than 4 linear rgb steps
                clip(max(albedo.r, max(albedo.g, albedo.b)) - 0.0003);

                // Apply base colour...
                albedo *= _BaseColor.rgb;

                // Copied from 'InitializeInputData' - Cleaner but still pathetic
                InputData inputData       = (InputData)0;
                inputData.positionWS      = input.positionWS;
                inputData.normalWS        = NormalizeNormalPerPixel(input.normalWS);        
                inputData.viewDirectionWS = SafeNormalize(GetWorldSpaceNormalizeViewDir(inputData.positionWS));

                // Sets the shadow map sampling coordinates
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    inputData.shadowCoord = input.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
                #else
                    inputData.shadowCoord = float4(0, 0, 0, 0);
                #endif

                // GetNormalizedScreenSpaceUV - not sure if we're even using this yet
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                // I believe this single function is responsible for all ambient lighting? Including shadow masking... Weird.
                inputData.bakedGI    = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                // Apply Lighting (Extracted & cleaned up from Lighting>UniversalFragmentBlinnPhong)
                half3 colour = OpenSoMBlinnPhongFragment(inputData, albedo);

                // Apply Fog
                half viewZ     = -(dot(UNITY_MATRIX_V[2].xyz, inputData.positionWS) + UNITY_MATRIX_V[2].w);
                half fogFactor = saturate(mad(viewZ, unity_FogParams.z, unity_FogParams.w - _ProjectionParams.y * unity_FogParams.z));
                colour.rgb     = lerp(half3(unity_FogColor.rgb), colour, fogFactor);

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
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"

            //
            // From Shadow Caster Pass
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

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
                //#if defined(_ALPHATEST_ON)
                    float2 uv       : TEXCOORD0;
                //#endif

                float4 positionCS   : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

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

                //#if defined(_ALPHATEST_ON)
                    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                //#endif

                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);

                //#if defined(_ALPHATEST_ON)
                    //Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
                //#endif

                // Custom colour key logic... keys out any pixel with no channel above than 4 linear rgb steps
                half3 albedo = SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).rgb;
                clip(max(albedo.r, max(albedo.g, albedo.b)) - 0.0003);

                #if defined(LOD_FADE_CROSSFADE)
                    LODFadeCrossFade(input.positionCS);
                #endif

                return 0;
            }

            ENDHLSL
        }

        Pass
        {
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma target 4.5

            // Deferred Rendering Path does not support the OpenGL-based graphics API:
            // Desktop OpenGL, OpenGL ES 3.0, WebGL 2.0.
            #pragma exclude_renderers gles3 glcore

            // -------------------------------------
            // Shader Stages
            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _ _SPECGLOSSMAP _SPECULAR_COLOR
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            //#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fragment _ LIGHTMAP_BICUBIC_SAMPLING
            #pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            //--------------------------------------
            // Defines
            #define BUMP_SCALE_NOT_SUPPORTED 1

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitGBufferPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

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
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            // Universal Pipeline keywords
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitDepthNormalsPass.hlsl"
            ENDHLSL
        }
    }

    Fallback  "Hidden/Universal Render Pipeline/FallbackError"
}
