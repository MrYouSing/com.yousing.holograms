/* <!-- Macro.Define SrcLKG=
:C:/Users/Administrator/Documents/GitHub/holoplaycore.js/src/
 Macro.End --> */
/* <!-- Macro.Copy File
$(SrcLKG)/HoloPlayCore.js,468~485,494~495,500~508,524~552
 Macro.End --> */
/* <!-- Macro.Replace
    ,	
vec2,float2
vec3,float3
vec4,float4
mod,fmod
fract,frac
texture(u_texture,tex2D(_MainTex
float3(0.0),0.0
float3(1.0),1.0
float3(0.00001),0.00001
float3(0.999999),0.999999
${tilesX},_InputSize.x
${tilesY},_InputSize.y
quiltWidth,_InputSize.x
quiltHeight,_InputSize.y
tileCount,_InputSize.z
${subp},_InputSize.w
float3(${pitch}),_Arguments.x
${slope},_Arguments.y
float3(${center}),_Arguments.z
quiltViewPortion.,_OutputSize.
focused_uv.x,//focused_uv.x
 Macro.End --> */
/* <!-- Macro.Patch
,LKG
 Macro.End --> */

struct appdata
{
	float4 vertex:POSITION;
	float2 uv:TEXCOORD0;
};

struct v2f
{
	float2 uv:TEXCOORD0;
	float4 vertex:SV_POSITION;
};

sampler2D _MainTex;
float4 _MainTex_ST;

float4 _InputSize;
float2 _OutputSize;
float4 _Arguments;

v2f vert(appdata v)
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	v.uv=lerp(v.uv,float2(1.0-v.uv.x,1.0-v.uv.y),_Arguments.w);
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	return o;
}

// <!-- Macro.Patch LKG
	float2 GetQuiltCoordinates(float2 tile_uv, int viewIndex)
	{
		float totalTiles = _InputSize.z;
		float floaty = float(viewIndex);
		float view = clamp(floaty, 0.0, totalTiles);
		// on some platforms this is required to fix some precision issue???
		float tx = _InputSize.x - 0.00001; // just an incredibly dumb bugfix
		float tileXIndex = fmod(view, tx);
		float tileYIndex = floor(view / tx);
	
		float quiltCoordU = ((tileXIndex + tile_uv.x) / tx) * _OutputSize.x;
		float quiltCoordV = ((tileYIndex + tile_uv.y) / _InputSize.y) * _OutputSize.y;
	
		float2 quilt_uv = float2(quiltCoordU, quiltCoordV);
	
		return quilt_uv;
	}

	float3 GetSubpixelViews(float2 screen_uv) {
		float3 views = 0.0;
			views[0] = screen_uv.x + _InputSize.w * 0.0;
			views[1] = screen_uv.x + _InputSize.w * 1.0;
			views[2] = screen_uv.x + _InputSize.w * 2.0;
				
	
			// calculate y contribution for each cell
			views[0] += screen_uv.y * _Arguments.y;
			views[1] += screen_uv.y * _Arguments.y;
			views[2] += screen_uv.y * _Arguments.y;
		views *= _Arguments.x;
		views -= _Arguments.z;
		views = 1.0 - frac(views);

		views = clamp(views, 0.00001, 0.999999);
	
		return views;
	}
	
	// this is the simplest sampling fmode where we just cast the viewIndex to int and take the color from that tile.
	float4 GetViewsColors(float2 tile_uv, float3 views)
	{
		float4 color = float4(0, 0, 0, 1);
	
		for(int channel = 0; channel < 3; channel++)
		{
			int viewIndex = int(views[channel] * _InputSize.z);
	
			float viewDir = views[channel] * 2.0 - 1.0;
			float2 focused_uv = tile_uv;
			//focused_uv.x += viewDir * focus;
	
			float2 quilt_uv = GetQuiltCoordinates(focused_uv, viewIndex);
			color[channel] = tex2D(_MainTex, quilt_uv)[channel];
		}
	
		return color;
	}

// Macro.Patch -->
fixed4 fragLKG(v2f i):SV_Target
{
	fixed4 col=GetViewsColors(i.uv,GetSubpixelViews(i.uv));
	return col;
}