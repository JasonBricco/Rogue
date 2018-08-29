Shader "Custom/Sprite Array"
{
	Properties
	{
		_TexArray("Array", 2DArray) = "" {}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "False"
		}

		Lighting Off
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"

			UNITY_DECLARE_TEX2DARRAY(_TexArray);

			struct VertexIn
			{
				float4 vert : POSITION;
				float3 uv : TEXCOORD0;
				float4 col : COLOR;
			};

			struct VertexOut
			{
				float4 vert : SV_POSITION;
				float3 uv : TEXCOORD0;
				float4 col : COLOR;
			};

			VertexOut vert(VertexIn i)
			{
				VertexOut o;
				o.vert = UnityObjectToClipPos(i.vert);
				o.uv = i.uv;
				o.col = i.col;
				return o;
			}

			fixed4 frag(VertexOut i) : SV_Target
			{
				fixed4 result = UNITY_SAMPLE_TEX2DARRAY(_TexArray, i.uv.xyz);
				clip(result.a - 0.01f);
				return result;
			}

			ENDCG
		}
	}
}
