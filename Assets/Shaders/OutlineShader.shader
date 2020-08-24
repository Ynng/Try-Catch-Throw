Shader "Custom/OutlineShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_OutlineWidth("Outline width", Range(.002, 0.03)) = .005

		[HideInInspector] _Mode("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
	}


		CGINCLUDE
#include "UnityCG.cginc"

			struct appdata
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
		};

		struct v2f
		{
			float4 pos : SV_POSITION;
			fixed4 color : COLOR;
		};

		uniform float _OutlineWidth;
		uniform float4 _OutlineColor;
		uniform float4x4 _ObjectToWorldFixed;

		v2f vert(appdata v)
		{
			v2f o;

			float4 objectCenterWorld = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0));
			float4 vertWorld = mul(unity_ObjectToWorld, v.vertex);

			float3 offsetDir = vertWorld.xyz - objectCenterWorld.xyz;
			offsetDir = normalize(offsetDir) * _OutlineWidth;

			o.pos = UnityWorldToClipPos(vertWorld + offsetDir);

			o.color = _OutlineColor;
			return o;
		}
		ENDCG

			SubShader
		{
			Tags { "Queue" = "Transparent" }
			Pass
			{
				Name "OUTLINE"
				ZWrite Off
				Blend SrcAlpha OneMinusSrcAlpha

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				fixed4 frag(v2f i) : SV_Target
				{
				return i.color;
			}
			ENDCG
		}
			UsePass "Standard/FORWARD"
		}

			Fallback Off
}