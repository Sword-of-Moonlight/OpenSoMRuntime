Shader "OpenSoM/Sky (Texture, Colour, UVScroll)"
{
    Properties
    {
        [MainTexture] _MainTex("Texture", 2D) = "white" {}
        _ColourA("Colour (Diffuse)", Color) = (1,1,1,1)
        _ColourB("Colour (Emissive)", Color) = (0,0,0,0)

        _ScrollParams("Scroll Params", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Background"
            "Queue"          = "Background"
            "RenderPipeline" = "UniversalPipeline"
        }
        ZWrite Off
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex StaticVertex
            #pragma fragment UnlitStaticTextureZFailFragment

            #include "UnityCG.cginc"

            /**
             * Program -> Vertex
            **/
            struct p2v
            {
                // Pose #1 (Stream #1)
                float3 positionA : POSITION;
                float3 normalA   : NORMAL;

                // Pose #2 (Stream #2)
                float3 positionB : TEXCOORD2;
                float3 normalB   : TEXCOORD3;

                // Generic (Stream #3)
                float2 texcoord    : TEXCOORD0;
                float4 colour      : TEXCOORD1;
                uint4  boneIndices : TEXCOORD4;
                float4 boneWeights : TEXCOORD5;
            };

            /**
             * Vertex -> Fragment
            **/
            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            /**
             * Samplers
            **/
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _ColourA;
            fixed4 _ColourB;
            fixed4 _ScrollParams;

            //
            // Static Vertex
            //
            v2f StaticVertex(p2v v)
            {
                v2f o;
                o.vertex   = UnityObjectToClipPos(v.positionA);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                // Apply UV Scroll...
                o.texcoord = o.texcoord + (_ScrollParams.xy * _ScrollParams.zw) * _Time.x;

                return o;
            }

            //
            // Unlit Textured Fragment
            //
            fixed4 UnlitStaticTextureZFailFragment(v2f i) : SV_Target
            {
                return (tex2D(_MainTex, i.texcoord) * _ColourA) + _ColourB;
            }
            ENDCG
        }
    }
}
