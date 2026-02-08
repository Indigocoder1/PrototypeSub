Shader "Unlit/HighlightShader"
{
    Properties
    {
        _InnerColor ("Inner Color", Color) = (1,1,1,1)
        _OuterColor ("Outer Color", Color) = (1,1,1,1)
        _Falloff ("Falloff Bias", Float) = 0
        _ColorBias ("Color Bias", Float) = 0
        _Intensity ("Intensity", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 vertexColor : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 vertexColor : COLOR;
            };

            fixed4 _OuterColor;
            fixed4 _InnerColor;
            half _Falloff;
            half _ColorBias;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.vertexColor = v.vertexColor;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float alphaY = saturate(i.uv.y + _Falloff);
                float colorY  = saturate(i.uv.y + _ColorBias);
                fixed4 col = lerp(_InnerColor, _OuterColor, colorY);
                float a = 1 - alphaY;
                return fixed4(col.rgb * _Intensity, a * col.a * i.vertexColor.a);
            }
            ENDCG
        }
    }
}
