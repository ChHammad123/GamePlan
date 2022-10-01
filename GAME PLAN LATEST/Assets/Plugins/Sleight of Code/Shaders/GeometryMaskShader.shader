Shader "Sleight of Code/Geometry Mask" {
   Properties
   {
      _MainTex ("Culling Mask", 2D) = "white" {}
      _Cutoff ("Alpha cutoff", Range (0,1)) = 0.5
   }
 
   SubShader
   {
        Tags {"Queue" = "Transparent+1"}
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        ZWrite On
        ZTest Always
        Alphatest LEqual [_Cutoff]
        Pass
        {
            SetTexture [_MainTex] {combine texture}
        }
   }
}