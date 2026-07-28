Shader "OpenSoM/Sprite (Transparent, Colour Key)"
{
    Properties
    {
        [MainTexture] _MainTex("Texture", 2D) = "white" {}
        _ColorKey("Transparent Color", Color) = (1,0,1,1)
        _ColorKeyThreshold("Color Key Threshold", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "IgnoreProjector" = "True"
            "Queue"           = "Transparent"
        }
        ZWrite Off
        Cull Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex VertexPassthrough
            #pragma fragment SpriteTransparentColourKeyFragment

            #include "UnityCG.cginc"

            /**
             * Program -> Vertex
            **/
            struct p2v
            {
                float4 vertex : POSITION;
                float4 colour : COLOR0;
                float2 uv     : TEXCOORD0;
            };

            /**
                * Vertex -> Fragment
            **/
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 colour : COLOR0;
                float2 uv     : TEXCOORD0;
            };

            /**
                * Samplers
            **/
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _ColorKey;
            float _ColorKeyThreshold;

            //
            // VERTEX PASSTHROUGH SHADER
            //
            v2f VertexPassthrough(p2v v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.colour = v.colour;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            //
            // Transparent Sprite (Colour Key) Fragment
            //
            fixed4 SpriteTransparentColourKeyFragment(v2f i) : SV_Target
            {
                // Get the colour from the texture
                float4 texColour = tex2D(_MainTex, i.uv);

                // Clip the pixel if the colour key value is close enough
                clip(distance(texColour.rgb, _ColorKey.rgb) - _ColorKeyThreshold);

                return texColour * i.colour;
            }
            ENDCG
        }
    }
}
