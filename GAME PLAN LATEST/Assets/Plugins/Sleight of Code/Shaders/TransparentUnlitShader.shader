Shader "Sleight of Code/Unlit Alpha-Blended" {
	Properties {
		_Color ("Color Tint", Color) = (1,1,1,1)
		_MainTex ("Color (RGB) Alpha (A)", 2D) = "white"
	}
	Category {
		Lighting Off
		ZWrite Off
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		Tags {"Queue"="Transparent"}
		SubShader {
			//Material {
			//	Emission [_Color]
			//}
			Pass {
				SetTexture [_MainTex] {
					constantColor [_Color]
					Combine texture * constant, texture * constant 
				} 

				//SetTexture [_MainTex] {
				//	Combine Texture * Primary, Texture * Primary
				//}
			}
		} 
	}
}