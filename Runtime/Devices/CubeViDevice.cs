using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class CubeViDevice
		:LenticularDevice
	{
		#region Fields

		[Header("Companion One")]
		public string deviceId;
		public string deviceNumber;
		public string sdkType;

		#endregion Fields

		#region Unity Messages

		protected virtual void Reset() {
			resolution=new Vector2Int(1440,2560);
			float f=5.7f*0.0254f;f=f*f/(9*9+16*16);f=Mathf.Sqrt(f);
			size=new Vector4(9.0f*f,16.0f*f,.03f,-.03f);
			Vector4 v=PreferredSize();
			quiltResolution=new Vector2Int((int)v.x,(int)v.y);
			quiltSize=new Vector2Int((int)v.z,(int)v.w);
		}

		protected override void OnDestroy() {
			base.OnDestroy();
			//
			var sdk=CubeViSdk.s_Instance;
			if(sdk!=null) {
				sdk.onDeviceUpdated-=OnDeviceUpdated;
			}
		}

		#endregion Unity Messages

		#region Methods

		public virtual float obliquity{set=>slope=value;}
		[JsonProperty("lineNumber")]
		public virtual float interval{set=>pitch=value;}
		[JsonProperty("deviation")]
		public virtual float x0{set=>center=value;}

		public override Vector3 ParseQuilt() {
			bool play=true;
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying) {play=false;}
#endif
			return play&&quiltTexture!=null?base.ParseQuilt():new Vector3(8.0f,5.0f,0.5625f);
		}

		public override Vector4 PreferredSize() {
			bool play=true;
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying) {play=false;}
#endif
			return play&&quiltTexture!=null?base.PreferredSize():new Vector4(4320.0f,4800.0f,8.0f,5.0f);
		}

		public override bool IsPresent() {
			var sdk=CubeViSdk.instance;
			if(sdk!=null&&!sdk.IsDetected) {return false;}
			return FindDisplay(resolution)>=0;
		}

		public override int GetPriority() {
			var sdk=CubeViSdk.instance;
			if(sdk!=null&&sdk.IsDetected) {return 1000;}
			return base.GetPriority();
		}

		public override void FromJson(string json) {
			if(string.IsNullOrEmpty(json)) {
				var sdk=CubeViSdk.instance;
				if(sdk!=null) {sdk.LoadDeviceConfig(json);}
				return;
			}
			//
			base.FromJson(json);
		}

		protected override void FromJson(JObject jo) {
			base.FromJson(jo);
			//
			jo.Remove(nameof(name));
			jo[nameof(sdkType)]="manual";
			JsonConvert.PopulateObject(jo.ToString(),this);
			//
			var sdk=CubeViSdk.instance;
			if(sdk!=null) {
				jo["interval"]=pitch;
				jo["slope"]=slope;
				jo["x0"]=center;
				//
				jo[nameof(display)]=m_Display;
				sdk.LoadDeviceConfig(jo);
			}
			jo.RemoveAll();
		}

		protected override void ToJson(JObject jo) {
			base.ToJson(jo);
			//
			if(!string.IsNullOrEmpty(deviceNumber)) {jo[nameof(name)]=deviceNumber;}
		}

		public override void Init() {
			if(m_IsInited) {return;}
			//
			var sdk=CubeViSdk.instance;
			if(sdk!=null) {
				slope=CubeViSdk.k_Device.slope;
				interval=CubeViSdk.k_Device.interval;
				x0=CubeViSdk.k_Device.x0;
				//
				sdk.onDeviceUpdated+=OnDeviceUpdated;
			}
			var app=MonoApplication.s_Instance;
			if(app!=null) {
				if(app.depth.sqrMagnitude==0.0f) {app.depth=new Vector2(.25f,.75f);}
			}
			//
			base.Init();
			if(material==null) {
				material=new Material(Shader.Find("Hidden/C1_Blit"));
			}
		}

		protected virtual void OnDeviceUpdated(string text) {
			if(!string.IsNullOrEmpty(text)) {
				string n=name;
					JsonConvert.PopulateObject(text,this);
				name=n;
				if(string.IsNullOrEmpty(sdkType)) {sdkType="local";}
				if(string.Equals(sdkType,"manual",UnityExtension.k_Comparison)) {return;}
				Debug.Log($"Load {sdkType} settings : {text}");
			}
		}

		#endregion Methods
	}
}
