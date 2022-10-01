// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/UVScrollLightmap"
{
	Properties
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0.0, 1.0)) = 0.5
		[KeywordEnum(Linear, Step)] _Type("Type", Float) = 0 // 0 is Continuous, 1 is Discrete
		_ScrollSpeed("ScrollSpeed", Vector) = (0.1, 0.1, 0, 0)
		_TimeSpeed("TimeSpeed", Float) = 1.0
		_Step("StepUV", Vector) = (0.25, 0.25, 0, 0)
		_SecPerStep("SecPerStep", Vector) = (1.0, 1.0, 0, 0)
		_LightmapTex ("Lightmap (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 150

		Pass
		{
			Tags {"LightMode"="ForwardBase"}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#include "AutoLight.cginc"
			
			//#define UNITY_NO_SCREENSPACE_SHADOWS

			struct appdata
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
			};

			struct v2f
			{
				fixed4 color : COLOR;
				float2 uvMain : TEXCOORD0;
				float2 uvLightmap : TEXCOORD1;
				SHADOW_COORDS(2)
				
				float4 vertex : SV_POSITION;
			};

			//sampler2D _MainTex;
			//float4 _MainTex_ST;
			
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.uvMain = v.uv0;
				o.uvLightmap = v.uv1;
				//TRANSFER_SHADOW(o)
				//UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _LightmapTex;
			float _Cutoff;
			fixed _Type;
			float2 _ScrollSpeed;
			float _TimeSpeed;
			float2 _Step;
			float2 _SecPerStep;
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 scroll = (1 - _Type) * frac(_Time.y * _ScrollSpeed * _TimeSpeed) + _Type * frac(_Time.y * _Step / _SecPerStep * _TimeSpeed);
				scroll = (1 - _Type) * scroll + _Type * floor(scroll / _Step) * _Step;
				float2 uv = frac(i.uvMain + scroll);
				fixed4 c = tex2D(_MainTex, uv);
				fixed4 l = tex2D(_LightmapTex, i.uvLightmap);
				fixed shadow = SHADOW_ATTENUATION(i);
				fixed4 col;
				col.rgb = c.rgb * i.color.rgb * l.rgb * shadow;
				col.a = c.a;
				clip(col.a - _Cutoff);
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
