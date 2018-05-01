// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Stencil/Mask OneZLess"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Geometry"
				"IgnoreProjector" = "True"
				"RenderType" = "Opaque"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Fog{ Mode Off }
			Blend One OneMinusSrcAlpha
			ColorMask 0

			
			Stencil
			{
				Ref 1
				Comp always
				Pass replace
			}

			Pass
			{
				Cull Back
				ZTest Less

				CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		sampler2D _MainTex;

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv: TEXCOORD0;
		};
	
		struct v2f
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};
	
		v2f vert(appdata v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;
			return o;
		}
	
		float4 frag(v2f i) : COLOR
		{
			float4 c = tex2D(_MainTex, i.uv);
			if (c.a < 0.03) discard;
			c.rgb *= c.a;
			return c;
		}
		ENDCG
		}
	}
}