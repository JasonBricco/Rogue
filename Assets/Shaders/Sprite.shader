Shader "Custom/Sprite"
{
	Properties
	{
		_MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1, 1, 1, 1)
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

			sampler2D _MainTex;
			fixed4 _Color;

			struct VertexIn
			{
				float4 vert : POSITION;
				float4 col : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct VertexOut
			{
				float4 vert : SV_POSITION;
				fixed4 col : COLOR;
				float2 uv : TEXCOORD0;
			};

			VertexOut vert(VertexIn i)
			{
				VertexOut o;
				o.vert = UnityObjectToClipPos(i.vert);
				o.uv = i.uv;
				o.col = i.col * _Color;
				return o;
			}

			fixed4 frag(VertexOut i) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, i.uv) * i.col;
				c.rgb *= c.a;
				return c;
			}

			ENDCG
		}
	}
}
