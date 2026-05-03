Shader "Indigocoder/EmissiveArea"
{
    Properties
    {
        _EmissiveTex ("Emissive Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _EmissiveColor ("Emissive Color", Color) = (1, 1, 1, 1)
        _EmissiveStrength ("Emissive Strength", Range(0, 20)) = 1
        _UVTarget ("Progress", Range(0, 1)) = 0.5
        _UVSpread ("Range", Range(0, 2)) = 0.1
        _Falloff ("Falloff", Float) = 1
        _ObjMinY ("Object Min Coord", Float) = 0
        _ObjMaxY ("Object Max Coord", Float) = 1
        _AlphaMultiplier ("Alpha Multiplier", Float) = 2
        _NoiseMultiplier ("Noise Multiplier", Range(0, 1)) = 0.1
        [Toggle] _UseMatrix ("Use Custom Matrix", Float) = 0
        [Toggle] _DiscardNoAlpha ("Discard if no alpha", Float) = 1
        [Enum(X, 0, Y, 1, Z, 2)] _Axis ("Axis", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };

            sampler2D _EmissiveTex;
            float4 _EmissiveTex_ST;

            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;

            float4x4 _EmissiveAreaTransformMatrix;
            float3 _MatrixPosOffset;
            fixed4 _EmissiveColor;
            half _EmissiveStrength;
            half _UseMatrix;
            half _Axis;
            half _UVTarget;
            fixed _UVSpread;
            
            float _ObjMaxY;
            float _ObjMinY;
            float _Falloff;
            half _AlphaMultiplier;
            fixed _NoiseMultiplier;
            fixed _DiscardNoAlpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _EmissiveTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            float invLerp(float from, float to, float value)
            {
                return (value - from) / (to - from);
            }

            fixed4 triplanarTex(float3 worldPos, sampler2D _NoiseTex)
            {
                float2 uvFront = TRANSFORM_TEX(worldPos.xy, _NoiseTex);
                float2 uvSide = TRANSFORM_TEX(worldPos.zy, _NoiseTex);
                float2 uvTop = TRANSFORM_TEX(worldPos.xz, _NoiseTex);

                fixed4 colFront = tex2D(_NoiseTex, uvFront);
                fixed4 colSide = tex2D(_NoiseTex, uvSide);
                fixed4 colTop = tex2D(_NoiseTex, uvTop);

                return (colFront + colSide + colTop) / 3;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 emissive = tex2D(_EmissiveTex, i.uv);
                fixed4 noise = triplanarTex(i.worldPos, _NoiseTex);
                
                float noiseVal = noise.r * _NoiseMultiplier;
                float posMin = _UVTarget - _UVSpread;
                float posMax = _UVTarget + _UVSpread;

                float3 coord = i.worldPos * (1 - _UseMatrix) + mul(_EmissiveAreaTransformMatrix, i.worldPos + _MatrixPosOffset) * _UseMatrix;
                float scalar =
                              coord.x * saturate(1 - _Axis)
                            + coord.y * saturate(2 - _Axis) * saturate(abs(_Axis - 2)) * saturate(_Axis)
                            + coord.z * saturate(3 - _Axis) * saturate(abs(_Axis - 1)) * saturate(_Axis);
                float uvCurrent = invLerp(_ObjMinY - _UVSpread, _ObjMaxY + _UVSpread * 10 + _NoiseMultiplier * 4, scalar) + (noiseVal * _NoiseMultiplier);
                
                if (uvCurrent < posMin || uvCurrent > posMax)
                {
                    if (_DiscardNoAlpha == 1) discard;
                    
                    return fixed4(0,0,0,1);
                }

                float stepMax = smoothstep(1, 0, invLerp(_UVTarget, posMax, uvCurrent));
                float stepMin = smoothstep(0, 1, invLerp(posMin, _UVTarget, uvCurrent));
                float smoothStep = min(stepMax, stepMin);

                fixed4 col = emissive * _EmissiveColor * _EmissiveStrength;

                float val = dot(emissive.rgb, fixed3(0.3, 0.59, 0.11));
                float emissionStrength = pow(smoothStep, _Falloff) * val * _AlphaMultiplier;
                return fixed4(col.rgb * emissionStrength, emissionStrength * _DiscardNoAlpha + 1 - _DiscardNoAlpha);
            }
            ENDCG
        }
    }
}
