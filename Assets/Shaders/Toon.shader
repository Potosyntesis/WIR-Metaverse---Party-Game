Shader "Roystan/Toon"
{
	Properties
	{
		_Color("Color", Color) = (0.5, 0.65, 1, 1)
		_MainTex("Main Texture", 2D) = "white" {}
		[NoScaleOffset] _NormalMap("Normal Map", 2D) = "bump" {} // adding the normal map function without any offset scale
		[HDR] _AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
		[HDR] _SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)
		_Glossiness("Glossiness", Float) = 32
		[HDR] _RimColor("Rim Color", Color) = (1,1,1,1)
		_RimAmount("Rim Amount", Range(0,1)) = 0.716
		_RimThreshold("Rim Threshold", Range(0,1)) = 0.1
	}
	SubShader
	{
		Pass
		{
			Tags
			{
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;				
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT; // tangents have xyzw where w = normals facing toward/away from the camera
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldNormal : NORMAL;
				float3 viewDir : TEXCOORD1;
				float3 tangent : TEXCOORD2; // assign tangent
				float3 bitangent : TEXCOORD3; // assign bitangent
				SHADOW_COORDS(4)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.tangent = UnityObjectToWorldDir(v.tangent.xyz); // calculate object tangent to world space
				o.bitangent = cross(o.worldNormal, o.tangent); // calculate bitangent as the cross product of the normal and binormal
				o.bitangent *= v.tangent.w * unity_WorldTransformParams.w; // flips the normal maps correctly based on scaling
				o.viewDir = WorldSpaceViewDir(v.vertex);
				TRANSFER_SHADOW(o)
				return o;
			}
			
			float4 _Color;
			float4 _AmbientColor;
			float4 _SpecularColor;
			float _Glossiness;
			float4 _RimColor;
			float _RimAmount;
			float _RimThreshold;
			sampler2D _NormalMap;

			float4 frag(v2f i) : SV_Target
			{
				// implemented normal maps into the world space
				float3 tangentSpaceNormal = UnpackNormal(tex2D(_NormalMap, i.uv));
				float3x3 mtxTangToWorld = {
					i.tangent.x, i.bitangent.x, i.worldNormal.x,
					i.tangent.y, i.bitangent.y, i.worldNormal.y,
					i.tangent.z, i.bitangent.z, i.worldNormal.z
				};
				float3 normal = mul(mtxTangToWorld, tangentSpaceNormal);

				// rendering the albedo and shadow as flat colours (can add stepped shadow rendering later)
				//float3 normal = normalize(i.worldNormal); // original world normal calc from the toon shader tutorial, overwritten
				float NdotL = dot(_WorldSpaceLightPos0, normal); // the built-in unity function to convert light data to the dot product
				float shadow = SHADOW_ATTENUATION(i); // apply shadows from the world objects
				float lightIntensity = smoothstep(0, 0.02, NdotL * shadow); // light falloff
				float4 light = lightIntensity * _LightColor0;
				
				

				// rendering the specularity
				float3 viewDir = normalize(i.viewDir);
				float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
				float NdotH = dot(normal, halfVector); // finding the dot product of the specularity

				float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
				float specularIntensitySmooth = smoothstep(0.005, 0.02, specularIntensity);
				float4 specular = specularIntensitySmooth * _SpecularColor;

				// rendering the rim lighting
				float4 rimDot = 1 - dot(viewDir, normal); // finding the dot product of the rim (inverted dot product)
				float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
				 rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity * shadow);
				float4 rim = rimIntensity * rimDot;


				float4 sample = tex2D(_MainTex, i.uv);

				return _Color * sample * (_AmbientColor + light + specular + rim);
			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}