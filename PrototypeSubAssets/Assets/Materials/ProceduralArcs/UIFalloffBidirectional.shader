Shader "Unlit/UIFalloffBidirectional"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _TargetCenter ("Opacity Center", Float) = 0.5
        _Falloff ("Falloff", Range(0,  0.5)) = 0
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
                float4 vertexColor : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 vertexColor : COLOR;
            };

            fixed4 _Color;
            fixed _TargetCenter;
            float _Intensity;
            half _Falloff;

            float invLerp(float from, float to, float value)
            {
                return (value - from) / (to - from);
            }

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
                float stepMax = smoothstep(1, 0, invLerp(_TargetCenter + (0.5 - _Falloff), 1, i.uv.y));
                float stepMin = smoothstep(0, 1, invLerp(0, _TargetCenter - (0.5 - _Falloff), i.uv.y));
                float smoothStep = min(stepMax, stepMin);

                float4 smoothedColor = _Color * _Intensity * smoothStep;
                return float4(smoothedColor.rgb, smoothedColor.a * i.vertexColor.a);
            }
            ENDCG
        }
    }
}
