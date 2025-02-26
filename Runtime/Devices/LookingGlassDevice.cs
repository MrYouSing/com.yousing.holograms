/* <!-- Macro.Define SrcLKG=
:C:/Users/Administrator/Documents/GitHub/looking-glass-webxr/src/
 Macro.End --> */
/* <!-- Macro.Define ShLKG=
:C:/Users/Administrator/Documents/GitHub/holoplaycore.js/src/
 Macro.End --> */
/* <!-- Macro.Copy File
$(SrcLKG)/LookingGlassConfig.ts,276~278,518~521,329~332,611~628
$(ShLKG)/HoloPlayCore.js,427~430
 Macro.End --> */
/* <!-- Macro.Replace
    ,	
get n,virtual float GetN
get p,virtual float GetP
get s,virtual float GetS
get t,virtual float GetT
Math.a,Mathf.A
Math.c,Mathf.C
Math.r,Mathf.R
(this._calibration.flipImageX.value ? -1 : 1),1.0
1.0,1.0f
* 3),* 3.0f)
	virtual,	public virtual
	},	;}
_calibration.pitch.value,pitch
_calibration.slope.value,slope
_calibration.screenW.value,template.screen.x
_calibration.screenH.value,template.screen.y
_calibration.DPI.value,template.dpi
framebufferWidth,template.quilt.x
framebufferHeight,template.quilt.y
quiltWidth,template.tile.x
quiltHeight,template.tile.y
	const vec2 q,	public virtual Vector2 GetQ
 = vec2(,()=>new Vector2(
  ${,	
: number,
* tileWidth,* GetTileWidth()
* tileHeight,* GetTileHeight()
template.quilt.x},template.quilt.x
template.quilt.y},template.quilt.y
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class LookingGlassDevice
		:LenticularDevice
	{
// <!-- Macro.Patch AutoGen
	public virtual float GetTileHeight() {
		return Mathf.Round(this.template.quilt.y / this.template.tile.y)
	;}
	public virtual float GetTileWidth() {
		return Mathf.Round(this.template.quilt.x / this.template.tile.x)
	;}

	public virtual float GetNumViews() {
		return this.template.tile.x * this.template.tile.y
	;}

	public virtual float GetTilt() {
		return (
			(this.template.screen.y / (this.template.screen.x * this.slope)) *
			1.0f
		)
	;}

	public virtual float GetSubp() {
		return (1 / (this.template.screen.x * 3.0f)) * 1.0f
	;}

	public virtual float GetPitch() {
		return (
			((this.pitch * this.template.screen.x) / this.template.dpi) *
			Mathf.Cos(Mathf.Atan(1.0f / this.slope))
		)
	;}

	public virtual Vector2 GetQuiltViewPortion()=>new Vector2(
		(template.tile.x * GetTileWidth()) / template.quilt.x,
		(template.tile.y * GetTileHeight()) / template.quilt.y);

// Macro.Patch -->
		#region Fields

		[Header("LKG")]
		public string serial;

		[System.NonSerialized]public LookingGlassSdk.Template template;
		[System.NonSerialized]protected bool m_UseOfficial;

		#endregion Fields

		#region Unity Messages

		protected virtual void Reset() {
			InitLKG(false);
		}

		protected override void OnDestroy() {
			base.OnDestroy();
			//
			var sdk=LookingGlassSdk.s_Instance;
			if(sdk!=null) {
				sdk.onDeviceUpdated-=OnDeviceUpdated;
			}
		}

		#endregion Unity Messages

		#region Methods

		public override bool IsPresent() {
			var sdk=LookingGlassSdk.instance;
			if(sdk!=null&&!sdk.IsDetected) {return false;}
			return FindDisplay(resolution)>=0;
		}

		public override int GetPriority() {
			var sdk=LookingGlassSdk.instance;
			if(sdk!=null&&sdk.IsDetected) {return 2000;}
			return base.GetPriority();
		}

		public override void FromJson(string json) {
			if(string.IsNullOrEmpty(json)) {
				var sdk=LookingGlassSdk.instance;
				if(sdk!=null) {sdk.LoadDeviceConfig(json);}
				return;
			}
			//
			base.FromJson(json);
		}

		protected override void FromJson(JObject jo) {
			base.FromJson(jo);
			//
			var sdk=LookingGlassSdk.instance;
			if(sdk!=null) {
				jo["sdkType"]="manual";
				jo[nameof(display)]=m_Display;
				sdk.LoadDeviceConfig(jo);
			}
			jo.RemoveAll();
		}

		protected virtual void InitLKG(bool play) {
			var sdk=LookingGlassSdk.instance;
			if(sdk!=null) {
				LookingGlassSdk.Calibration cal=null;
				if(play) {
					cal=sdk.GetCalibration();
					if(cal!=null) {serial=cal.serial;}
				}
				//
				template=sdk.GetTemplate(serial);
				resolution=template.screen;
				lens=new Vector4(-template.aspect,template.cone,0.0f,0.0f);
				quiltResolution=template.quilt;
				quiltSize=template.tile;
				size=sdk.CalculateSize(template);
				//
				if(play) {
					//
					var app=MonoApplication.s_Instance;
					if(app!=null) {
						if(app.depth.sqrMagnitude==0.0f) {app.depth=sdk.CalculateDepth(template);}
					}
					//
					if(cal!=null) {
						pitch=cal.pitch.value;
						slope=cal.slope.value;
						center=cal.center.value;
					}
					sdk.onDeviceUpdated+=OnDeviceUpdated;
				}
			}
		}

		protected override void UpdateMaterial() {
			var sdk=LookingGlassSdk.instance;
			if(sdk!=null) {
				float cnt=GetNumViews();
				if(m_UseOfficial) {
					material.SetFloat(LookingGlassSdk._pitch,GetPitch());
					material.SetFloat(LookingGlassSdk._slope,GetTilt());
					material.SetFloat(LookingGlassSdk._center,center);
					material.SetFloat(LookingGlassSdk._subpixelSize,GetSubp());
					material.SetFloat(LookingGlassSdk._screenW,resolution.x);
					material.SetFloat(LookingGlassSdk._screenH,resolution.y);
					material.SetFloat(LookingGlassSdk._tileCount,cnt);
					material.SetVector(LookingGlassSdk._viewPortion,GetQuiltViewPortion());
					material.SetVector(LookingGlassSdk._tile,new Vector4(quiltSize.x,quiltSize.y,cnt,cnt));
					material.SetInt(LookingGlassSdk._subpixelCellCount,0);
					material.SetInt(LookingGlassSdk._filterMode,2);
					material.SetInt(LookingGlassSdk._filterEdge,1);
					material.SetFloat(LookingGlassSdk._filterEnd,0.05f);
					material.SetFloat(LookingGlassSdk._filterSize,0.15f);
					material.SetFloat(LookingGlassSdk._gaussianSigma,0.01f);
					material.SetFloat(LookingGlassSdk._edgeThreshold,0.01f);
					material.SetVector(LookingGlassSdk._aspect,new Vector4(lens.x,lens.x));
				}else {
					material.SetVector(_InputSize,new Vector4(quiltSize.x,quiltSize.y,cnt,GetSubp()));
					material.SetVector(_OutputSize,GetQuiltViewPortion());
					material.SetVector(_Arguments,new Vector4(GetPitch(),GetTilt(),center));
				}
			}
		}

		protected virtual void OnDeviceUpdated(string text) {
			if(!string.IsNullOrEmpty(text)) {
				JsonConvert.PopulateObject(text,this);
			}
		}

		public override void Init() {
			if(m_IsInited) {return;}
			InitLKG(true);base.Init();
			//
			if(material==null) {
				Shader sh=Shader.Find("LookingGlass/Lenticular");
				m_UseOfficial=sh!=null;
				if(!m_UseOfficial) {sh=Shader.Find("Hidden/LKG_Blit");}
				//
				material=new Material(sh);
			}
		}

		#endregion Methods
	}
}
