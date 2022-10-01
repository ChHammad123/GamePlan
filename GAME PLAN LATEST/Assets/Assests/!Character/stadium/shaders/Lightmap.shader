// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Lightmap"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LightmapTex ("Lightmap (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 150//

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
				UNITY_FOG_COORDS(3)
				float4 vertex : SV_POSITION;
			};
			
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
			
			fixed4 frag(v2f i) : SV_Target
			
			{
				fixed4 c = tex2D(_MainTex, i.uvMain);
				fixed4 l = tex2D(_LightmapTex, i.uvLightmap);
				fixed shadow = SHADOW_ATTENUATION(i);
				fixed4 col;
				col.rgb = c.rgb * i.color.rgb * l.rgb * shadow;
				col.a = 1;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
