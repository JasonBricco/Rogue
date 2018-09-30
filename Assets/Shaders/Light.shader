Shader "Custom/Light"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	Subshader
	{
		Pass
		{
			Tags { "Queue" = "Opaque" "IgnoreProjector" = "True" "RenderType" = "Opaque" "PreviewType" = "Plane" }
			Blend OneMinusDstColor One
			Lighting Off
			ZWrite Off

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0

			uniform sampler2D _MainTex;

			struct VertexIn
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct VertexOut
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			VertexOut vert(VertexIn v)
			{
				VertexOut o;

				o.pos = UnityObjectToClipPos(v.pos);
				o.uv = v.uv;

				return o;
			}

			float4 frag(VertexOut i) : COLOR
			{
				return tex2D(_MainTex, i.uv);
			}

			ENDCG
		}
	}
}
