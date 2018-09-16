Shader "Custom/LightProcessor"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Lightmap("Lightmap", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			Cull Off
			ZWrite Off
			ZTest Always

			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _Lightmap;

			float4 frag(v2f_img o) : COLOR
			{
				float3 amb = UNITY_LIGHTMODEL_AMBIENT.rgb;
				float3 light = tex2D(_Lightmap, o.uv).rgb;

				float4 highest = float4(max(UNITY_LIGHTMODEL_AMBIENT.rgb, light.rgb), 1.0);
				return tex2D(_MainTex, o.uv) * highest;
			}

			ENDCG
		}
	}
}
