Shader "Custom/ProtoSonar" {
	Properties {
		_MainTex ("-", 2D) = "" {}
	}
	SubShader {
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			GpuProgramID 24626
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float4x4 _Camera2World;
			float _SonarPingDistance;
			float _SonarNearPlane;
			float _BorderStartPoint;
			fixed4 _SonarOutlineColor;
			fixed4 _CrossHatchColor;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _CameraDepthTexture;
			sampler2D _MainTex;
			sampler2D _CameraGBufferTexture2;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.texcoord.xy = v.texcoord.xy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
				// Far plane / near plane
                tmp0.x = _ProjectionParams.z * _ProjectionParams.y;
				// (Far plane, Near Plane) - (Near plane, 1 / Far Plane)
                tmp0.yz = _ProjectionParams.zy - _ProjectionParams.yz;
				// W component now holding the inverted depth
                float nonLinearDepth = 1.0 - tex2D(_CameraDepthTexture, inp.texcoord.xy).x;
				// Inverse depth * (Far plane - Near plane, Near plane - 1 / Far Plane) + (Near Plane, Far Plane)
                tmp0.yz = nonLinearDepth * tmp0.yz + _ProjectionParams.yz;
				// (Far plane / near plane) / (Inverse depth * (Near plane - 1/far plane) + Far plane)
                tmp0.x = tmp0.x / tmp0.z;
                tmp0.y = tmp0.y - tmp0.x;
                tmp0.x = unity_OrthoParams.w * tmp0.y + tmp0.x;
                tmp0.y = 1.0 - tmp0.x;
				/// tmp0.y will be 1 if orthographic mode is enabled, and tmp0.x if not
                tmp0.y = unity_OrthoParams.w * tmp0.y + tmp0.x;
                float4 crossHatches;
				// Map texcoord to -1 to 1 and put it in crossHatches.xy
				crossHatches.xy = inp.texcoord.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
				// Subracting (0, 2 * near plane / (top frustum - bottom frustum))
                crossHatches.zw = crossHatches.xy - unity_CameraProjection._m02_m12;
				float4 tmp3;
                tmp3.x = unity_CameraProjection._m00;
                tmp3.y = unity_CameraProjection._m11;
                crossHatches.zw /= tmp3.xy;
                tmp3.xy = crossHatches.xy / tmp3.xy;
                tmp0.yz = tmp0.yy * crossHatches.zw;
                crossHatches.xyz = tmp0.zzz * _Camera2World._m01_m11_m21;
                crossHatches.xyz = _Camera2World._m00_m10_m20 * tmp0.yyy + crossHatches.xyz;
                tmp0.xyz = _Camera2World._m02_m12_m22 * -tmp0.xxx + crossHatches.xyz;
                tmp0.xyz = tmp0.xyz + _Camera2World._m03_m13_m23;
                tmp0.xyz = _Time.yyy * 0.3 + tmp0.xyz;
                tmp0.xyz = tmp0.xyz * 1.3;
                tmp0.xyz = frac(tmp0.xyz);
				
				// At this point, tmp0.x has vertical contour lines across the whole scren, tmp0.y has distance lines, 
				// and tmp0.z has horizontal contour lines 
				
				// Combining the crossHatch masks
                tmp0.x = min(tmp0.y, tmp0.x);
                tmp0.x = min(tmp0.z, tmp0.x);
                tmp0.x = 1.0 - tmp0.x;
                tmp0.x = log(tmp0.x);
                tmp0.x = tmp0.x * 7.0;
                tmp0.x = exp(tmp0.x);
				
				// tmp0.y has the distance scan lines at this point
				
                tmp3.z = -1.0;
                tmp0.z = dot(tmp3.xyz, tmp3.xyz);
                tmp0.z = rsqrt(tmp0.z);
                crossHatches.xyz = tmp0.zzz * tmp3.xyz;
                tmp0.z = dot(-crossHatches.xyz, -crossHatches.xyz);
                tmp0.z = rsqrt(tmp0.z);
                crossHatches.xyz = tmp0.zzz * -crossHatches.xyz;
				
				float3 oldHatches = tmp0.xyz;
				
                float4 worldNormals = tex2D(_CameraGBufferTexture2, inp.texcoord.xy);
                worldNormals.xyz = worldNormals.xyz - 0.5;
                float crossHatchBrightness = smoothstep(0, 1, (dot(crossHatches.xyz, worldNormals.xyz) + 1) / 2) * 2 - 1;
                crossHatchBrightness = abs(crossHatchBrightness) * abs(crossHatchBrightness);
				crossHatchBrightness *= 6;
				tmp0.y = tmp0.x * 2;
                crossHatchBrightness = min(crossHatchBrightness, 1.0);
                crossHatches = crossHatchBrightness * tmp0.y * _CrossHatchColor;
                tmp0.y = -crossHatchBrightness * 0.5 + 1.0;
                tmp0.z = 0.9999 - nonLinearDepth;
				
				// Convert the non-linear depth to linear 
                nonLinearDepth = log(nonLinearDepth);
                nonLinearDepth = nonLinearDepth * 2000.0;
                nonLinearDepth = exp(nonLinearDepth);
                nonLinearDepth = min(nonLinearDepth, 1.0);
				float linearDepth = nonLinearDepth;
				
                tmp0.z = ceil(tmp0.z);
				float3 distanceMask; // Not too sure if this is the whole mask
                crossHatches.w =  _SonarPingDistance * 2.0 - linearDepth;
                distanceMask.x = crossHatches.w * 100.0;
                crossHatches.w = saturate(crossHatches.w * 4.0 + lerp(2, -5, _BorderStartPoint));
                //crossHatches.w = 1.0 - crossHatches.w; // This line adds the fade out as _SonarPingDistance increases
                distanceMask.x = saturate(distanceMask.x);
                distanceMask.x = crossHatches.w * distanceMask.x;
                distanceMask.x = (0.9 - linearDepth) * distanceMask.x;
                distanceMask.y = saturate(min(linearDepth * 1000, 1) * exp(1 / _SonarNearPlane));
                distanceMask.x = distanceMask.y * distanceMask.x;
				float oldDist = crossHatches.w;
                tmp0.z = saturate(tmp0.z * distanceMask.x);
                tmp0.xy = tmp0.zz * tmp0.xy;
                tmp0.y = linearDepth * tmp0.y;
				
				// Combine the linear depth with the overall effect mask
                float effectMask = tmp0.z + linearDepth;
                effectMask = min(effectMask, 1.0);
                float3 blueOutline = effectMask * -_SonarOutlineColor + _SonarOutlineColor;//float3(0.0, -0.1, -0.8) + float3(0.0, 0.1, 0.8);
				// The blue outline currently is has a cutout section where the effect will go
				
				// tmp0.z has the far and near plane masks with fade out at the near plane & some edge highlight
				// crossHatches.w has the far plane of the sonar mask and some slight fade out at the near plane
				
				float3 oldEffect = effectMask;
				// tmp0.w is changed to the original pixel color mask
                effectMask = tmp0.z * crossHatches.w;
				float oldW = tmp0.z;
				
				// Negating, halving, then adding one makes the effect part of the image slightly dark and everything else white
                effectMask = -effectMask * 0.5 + 1.0;
				
                crossHatches.w = effectMask * crossHatches.w;
                crossHatches.w = log(crossHatches.w);
                crossHatches.w = crossHatches.w * 20.0;
                crossHatches.w = exp(crossHatches.w);
                crossHatches.w = tmp0.z * crossHatches.w;
				
                tmp0.y = crossHatches.w * 10.0 + tmp0.y;
				// tmp0.y has the blueOutline mask
                blueOutline = blueOutline * tmp0.yyy;
                float4 originalPixelCol = tex2D(_MainTex, inp.texcoord.xy);
                tmp0.yzw = originalPixelCol.xyz * effectMask + blueOutline;
                o.sv_target.w = originalPixelCol.w;
                crossHatches.xyz = crossHatches.xyz - tmp0.yzw;
				// tmp0.x has the crosshatch mask
                o.sv_target.xyz = tmp0.xxx * crossHatches.xyz + tmp0.yzw;
                return o;
			}
			ENDCG
		}
	}
}