/* <!-- Macro.Include
../Internal/DeviceSdk.cs
LookingGlassSdk.csv
 Macro.End --> */
/* <!-- Macro.Copy Define
Instance
 Macro.End --> */
/* <!-- Macro.Table Shader
pitch,
slope,
center,
subpixelSize,
screenW,
screenH,
tileCount,
viewPortion,
tile,
subpixelCellCount,
filterMode,
filterEdge,
filterEnd,
filterSize,
gaussianSigma,
edgeThreshold,
aspect,
 Macro.End --> */
/* <!-- Macro.Call  Shader
		public static readonly int _{0}=Shader.PropertyToID("{0}");
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
/* <!-- Macro.Call  LKG
					new Template{{
						name="{0}",
						serial=new Regex({1}),
						aspect={10}f,
						cone={2},
						dpi={5}f,
						screen=new Vector2Int({3},{4}),
						quilt=new Vector2Int({6},{7}),
						tile=new Vector2Int({8},{9}),
					}},
 Macro.End --> */
/* <!-- Macro.Patch
,LKG
 Macro.End --> */
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class LookingGlassSdk
		:DeviceSdk
	{
// <!-- Macro.Patch AutoGen
		public static LookingGlassSdk s_Instance;
#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
#else
		[UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
		public static void InitializeOnLoadMethod() {
			s_Instance=Register<LookingGlassSdk>();
		}

		public static LookingGlassSdk instance {
			get {
				if(s_Instance==null) {InitializeOnLoadMethod();}
				return s_Instance;
			}
		}

		public static readonly int _pitch=Shader.PropertyToID("pitch");
		public static readonly int _slope=Shader.PropertyToID("slope");
		public static readonly int _center=Shader.PropertyToID("center");
		public static readonly int _subpixelSize=Shader.PropertyToID("subpixelSize");
		public static readonly int _screenW=Shader.PropertyToID("screenW");
		public static readonly int _screenH=Shader.PropertyToID("screenH");
		public static readonly int _tileCount=Shader.PropertyToID("tileCount");
		public static readonly int _viewPortion=Shader.PropertyToID("viewPortion");
		public static readonly int _tile=Shader.PropertyToID("tile");
		public static readonly int _subpixelCellCount=Shader.PropertyToID("subpixelCellCount");
		public static readonly int _filterMode=Shader.PropertyToID("filterMode");
		public static readonly int _filterEdge=Shader.PropertyToID("filterEdge");
		public static readonly int _filterEnd=Shader.PropertyToID("filterEnd");
		public static readonly int _filterSize=Shader.PropertyToID("filterSize");
		public static readonly int _gaussianSigma=Shader.PropertyToID("gaussianSigma");
		public static readonly int _edgeThreshold=Shader.PropertyToID("edgeThreshold");
		public static readonly int _aspect=Shader.PropertyToID("aspect");
// Macro.Patch -->
		#region Nested Types

		public class Value<T> {
			public T value;
		}

		public class Template {
			public string name;
			public Regex serial;
			public float aspect;
			public int cone;
			public float dpi;
			public Vector2Int screen;
			public Vector2Int quilt;
			public Vector2Int tile;
		}

		public class Calibration {
			public string configVersion;
			public string serial;
			public Value<float> pitch;
			public Value<float> slope;
			public Value<float> center;
		}

		#endregion Nested Types

		#region Fields

		public List<string> paths;
		public List<Template> templates;

		#endregion Fields

		#region Methods

		public LookingGlassSdk() {
			file="$(AppData)/Roaming/Looking Glass/Bridge/settings.json";
			app="LookingGlassBridge.exe";
			config=$"$(SaveData)/{nameof(LookingGlassSdk)}.json";
			if(templates==null) {
				templates=new List<Template>(){
// <!-- Macro.Patch LKG
					new Template{
						name="_8_9inGen1",
						serial=new Regex("(?i)(LKG-2K)"),
						aspect=1.6f,
						cone=40,
						dpi=338f,
						screen=new Vector2Int(2560,1600),
						quilt=new Vector2Int(4096,4096),
						tile=new Vector2Int(5,9),
					},
					new Template{
						name="_15_6inGen1",
						serial=new Regex("(?i)(LKG-4K)"),
						aspect=1.77778f,
						cone=40,
						dpi=283f,
						screen=new Vector2Int(3840,2160),
						quilt=new Vector2Int(4096,4096),
						tile=new Vector2Int(5,9),
					},
					new Template{
						name="_8KGen1",
						serial=new Regex("(?i)(LKG-8K)"),
						aspect=1.77778f,
						cone=40,
						dpi=280f,
						screen=new Vector2Int(7680,4320),
						quilt=new Vector2Int(8192,8192),
						tile=new Vector2Int(5,9),
					},
					new Template{
						name="PortraitGen2",
						serial=new Regex("(?i)(LKG-P)"),
						aspect=0.75f,
						cone=40,
						dpi=324f,
						screen=new Vector2Int(1536,2048),
						quilt=new Vector2Int(3360,3360),
						tile=new Vector2Int(8,6),
					},
					new Template{
						name="_16inGen2",
						serial=new Regex("(?i)(LKG-A)"),
						aspect=1.77778f,
						cone=40,
						dpi=283f,
						screen=new Vector2Int(3840,2160),
						quilt=new Vector2Int(4096,4096),
						tile=new Vector2Int(5,9),
					},
					new Template{
						name="_32inGen2",
						serial=new Regex("(?i)(LKG-B)"),
						aspect=1.77778f,
						cone=40,
						dpi=280f,
						screen=new Vector2Int(7680,4320),
						quilt=new Vector2Int(8192,8192),
						tile=new Vector2Int(5,9),
					},
					new Template{
						name="_65inLandscapeGen2",
						serial=new Regex("(?i)(LKG-D)"),
						aspect=1.77778f,
						cone=40,
						dpi=136f,
						screen=new Vector2Int(7680,4320),
						quilt=new Vector2Int(8192,8192),
						tile=new Vector2Int(8,9),
					},
					new Template{
						name="Prototype",
						serial=new Regex("(?i)(LKG-Q)"),
						aspect=1.77778f,
						cone=40,
						dpi=136.6f,
						screen=new Vector2Int(7680,4320),
						quilt=new Vector2Int(4096,4096),
						tile=new Vector2Int(5,9),
					},
					new Template{
						name="GoPortrait",
						serial=new Regex("(?i)(LKG-E)"),
						aspect=0.5625f,
						cone=54,
						dpi=491f,
						screen=new Vector2Int(1440,2560),
						quilt=new Vector2Int(4092,4092),
						tile=new Vector2Int(11,6),
					},
					new Template{
						name="Kiosk",
						serial=new Regex("(?i)(LKG-F)"),
						aspect=0.5625f,
						cone=40,
						dpi=324f,
						screen=new Vector2Int(1536,2048),
						quilt=new Vector2Int(4092,4092),
						tile=new Vector2Int(11,6),
					},
					new Template{
						name="_16inPortraitGen3",
						serial=new Regex("(?i)(LKG-H)"),
						aspect=0.5625f,
						cone=50,
						dpi=283f,
						screen=new Vector2Int(2160,3840),
						quilt=new Vector2Int(5995,6000),
						tile=new Vector2Int(11,6),
					},
					new Template{
						name="_16inLandscapeGen3",
						serial=new Regex("(?i)(LKG-J)"),
						aspect=1.77778f,
						cone=50,
						dpi=283f,
						screen=new Vector2Int(3840,2160),
						quilt=new Vector2Int(5999,5999),
						tile=new Vector2Int(7,7),
					},
					new Template{
						name="_32inPortraitGen3",
						serial=new Regex("(?i)(LKG-K)"),
						aspect=0.5625f,
						cone=54,
						dpi=280f,
						screen=new Vector2Int(4320,7680),
						quilt=new Vector2Int(8184,8184),
						tile=new Vector2Int(11,6),
					},
					new Template{
						name="_32inLandscapeGen3",
						serial=new Regex("(?i)(LKG-L)"),
						aspect=1.77778f,
						cone=50,
						dpi=280f,
						screen=new Vector2Int(7680,4320),
						quilt=new Vector2Int(8190,8190),
						tile=new Vector2Int(7,7),
					},
// Macro.Patch -->
				};
			}
			//
			string fn=config.GetFullPath();
			if(File.Exists(fn)) {
				m_DeviceConfig=File.ReadAllText(fn);
			}else {
			}
		}

		public virtual Template GetTemplate(string serial) {
			if(!string.IsNullOrEmpty(serial)) {Template it;
			for(int i=0,imax=templates?.Count??0;i<imax;++i) {
				it=templates[i];if(it?.serial?.IsMatch(serial)??false) {return it;}
			}}
			return GetTemplate("LKG-E00000");
		}

		public virtual Calibration GetCalibration() {
			string json=null,it;int i,imax;
			string [] list=Directory.GetLogicalDrives();
			//
			if(string.IsNullOrEmpty(json)) {
			for(i=0,imax=paths?.Count??0;i<imax;++i) {
				it=Path.Combine(paths[i],"visual.json").GetFullPath();
				if(File.Exists(it)) {json=File.ReadAllText(it);break;}
			}}
			//
			if(string.IsNullOrEmpty(json)) {
			for(i=0,imax=list?.Length??0;i<imax;++i) {
				it=Path.Combine(list[i],"LKG_calibration/visual.json");
				if(File.Exists(it)) {json=File.ReadAllText(it);break;}
			}}
			//
			return string.IsNullOrEmpty(json)?null:
				JsonConvert.DeserializeObject<Calibration>(json);
		}

		public virtual Vector4 CalculateSize(Template value) {
			if(value!=null) {
				float s=1.0f/value.dpi*0.0254f;
				float l=Mathf.Max(value.screen.x,value.screen.y)*0.25f;
				return new Vector4(
					value.screen.x*s,
					value.screen.y*s,
					l*s,-l*s
				);
			}
			return Vector4.zero;
		}

		public virtual Vector2 CalculateDepth(Template value) { 
			return new Vector2(.25f,.75f);
		}

		public override void LoadDeviceConfig(JObject value) {
			var tmp=config;
				JToken jt=value.SelectToken("sdkType");
				if(jt==null||!string.Equals(jt.Value<string>(),"manual",UnityExtension.k_Comparison)) {
					jt=value.SelectToken("serial");
					if(jt!=null) {config=jt.Value<string>()+".json";}
				}
				base.LoadDeviceConfig(value);
			config=tmp;
		}

		#endregion Methods
	}
}
