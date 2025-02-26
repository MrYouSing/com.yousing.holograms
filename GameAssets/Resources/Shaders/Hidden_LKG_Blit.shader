Shader "Hidden/LKG_Blit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_InputSize ("InputSize", Vector) = (11,6,4092,4092)
		_OutputSize ("Output Size", Vector) = (1440,2560,0,0)
		_Arguments ("Arguments", Vector) = (0,0,0,0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragLKG

			#include "UnityCG.cginc"
			#include "Lenticular.cginc"
			ENDCG
		}
	}
}
