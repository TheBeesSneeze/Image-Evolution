Shader "Custom/StateOverlay"
{
    Properties
    {
        _PrevState ("Previous State", 2D) = "black" {}
        _InputTex  ("Input Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _PrevState;
            sampler2D _InputTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 prev = tex2D(_PrevState, i.uv);
                fixed4 input = tex2D(_InputTex, i.uv);

                // Standard alpha overlay
                fixed4 result;
                result.rgb = input.rgb * input.a + prev.rgb * (1 - input.a);
                result.a = saturate(prev.a + input.a);

                return result;
            }
            ENDHLSL
        }
    }
}