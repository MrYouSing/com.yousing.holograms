/* <!-- Macro.Include
../Internal/DeviceSdk.cs
LookingGlass.csv
 Macro.End --> */
/* <!-- Macro.Copy Define
Instance
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
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

// Macro.Patch -->
		#region Nested Types

		public class Value<T> {
			public T value;
		}

		public class Template {
			public Regex serial;
			public int cone;
			public Vector2Int resolution;
			public Vector2Int quilt;
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
			//
			string fn=config.GetFullPath();
			if(File.Exists(fn)) {
				m_DeviceConfig=File.ReadAllText(fn);
			}else {
			}
		}

		public virtual Calibration GetCalibration() {
			string json=null,it;int i,imax;
			DriveInfo[] list=DriveInfo.GetDrives();
			//
			if(string.IsNullOrEmpty(json)) {
			for(i=0,imax=paths?.Count??0;i<imax;++i) {
				it=Path.Combine(paths[i],"visual.json").GetFullPath();
				if(File.Exists(it)) {json=File.ReadAllText(it);break;}
			}}
			//
			if(string.IsNullOrEmpty(json)) {
			for(i=0,imax=list?.Length??0;i<imax;++i) {
				it=list[i].Name;it=Path.Combine(it,".visual.json");
				if(File.Exists(it)) {json=File.ReadAllText(it);break;}
			}}
			//
			return string.IsNullOrEmpty(json)?null:
				JsonUtility.FromJson<Calibration>(json);
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
