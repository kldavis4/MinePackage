// Upgrade NOTE: replaced 'PositionFog()' with multiply of UNITY_MATRIX_MVP by position
// Upgrade NOTE: replaced 'V2F_POS_FOG' with 'float4 pos : SV_POSITION'

Shader "TestShader" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,0.5)
		_MainTex ("Base (RGB)", 2D) = "grass" {}
		_LightPower ("Light Power", Float) = 0.0
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_fog_exp2
			#include "UnityCG.cginc"

			float4 _Color;
			sampler2D _MainTex;
			float _LightPower;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 color : COLOR0;
				//float4 lighting : TEXCOORD1;
			};

			struct indata
			{
				float4 vertex;
				float3 normal;
				float4 texcoord;
				float4 color;
			};

			v2f vert(indata v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_UV(0);
				o.color = v.color; //_Color; //v.normal * 0.5 + 0.5;
				//o.lighting = v.color;
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				
				return half4(i.color*tex2D(_MainTex, i.uv).rgb, 1);//*i.lighting;
			}
			ENDCG

			//Material {
			//	Diffuse [_Color]
			//}
			//Lighting On
			//SetTexture [_MainTex] {
			//	constantColor[_Color]
			//}
		}
	} 
}
