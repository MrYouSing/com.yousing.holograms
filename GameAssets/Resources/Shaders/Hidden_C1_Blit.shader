/* E:/Program Files/3DGallery/_temp/assets/index-FdLfgE8_.js , up = `
vec2,float2
vec4,float4
mod,fmod
*/
Shader "Hidden/C1_Blit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_InputSize ("InputSize", Vector) = (8,5,4320,4800)
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
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _InputSize;
			float2 _OutputSize;
			float3 _Arguments;

			// Core Functions

			float2 get_choice(float2 pos, float bias) 
			{
				// TODO: Translation
				float slope=_Arguments.x;
				float interval=_Arguments.y;
				float x0=_Arguments.z;
				float row_img_num=_InputSize.x;
				float col_img_num=_InputSize.y;
				float num_of_view=row_img_num*col_img_num;
				float gridSizeX=_OutputSize.x;
				float gridSizeY=_OutputSize.y;
				// Convert position to grid coordinates
				float x = floor(pos.x * gridSizeX)+1.;
				float y = floor((1.0 - pos.y) * gridSizeY)+1.;

				// Compute a local x coordinate based on grid position, slope, and bias
				float x1 = (x + y * slope) * 3.0 + bias;
				float x_local = fmod(x1 + x0, interval);

				// Determine the choice index based on the local x coordinate
				int choice = int(floor(
					(x_local / interval) * num_of_view
				));

				// Calculate row and column choices
				float2 choice_vec = float2(
					row_img_num - fmod(float(choice), row_img_num) - 1., // col_choice 第几列, 修改为适配从左到右排列的阵列图
					floor(float(choice) / row_img_num) // row_choice 第几行
				);

				// Precompute reciprocals to avoid division in the loop
				float2 reciprocals = float2(1.0 / row_img_num, 1.0 / col_img_num);

				// Calculate texture coordinates and return
				float2 uv = (choice_vec.xy + pos) * reciprocals; // Note the .yx swizzle to match row/col order
				return uv;
			}

			// Unity Functions

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col= fixed4(tex2D(_MainTex, get_choice(i.uv,0)).r,
					tex2D(_MainTex, get_choice(i.uv,1)).g,
					tex2D(_MainTex, get_choice(i.uv,2)).b,
				1.0);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
