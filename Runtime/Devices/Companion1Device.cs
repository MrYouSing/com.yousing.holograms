using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class Companion1Device
		:HologramDevice
	{
		#region Fields

		public static readonly int _InputSize=Shader.PropertyToID("_InputSize");
		public static readonly int _OutputSize=Shader.PropertyToID("_OutputSize");
		public static readonly int _Arguments=Shader.PropertyToID("_Arguments");

		[Header("Parameters")]
		[JsonProperty("obliquity")]
		public float slope;
		[JsonProperty("lineNumber")]
		public float interval;
		[JsonProperty("deviation")]
		public float x0;
		[Header("Others")]
		[SerializeField]protected GameObject m_Actor;

		#endregion Fields

		#region Unity Messages

		protected virtual void Reset() {
			resolution=new Vector2Int(1440,2560);
			Vector4 v=PreferredSize();
			quiltSize=new Vector2Int((int)v.z,(int)v.w);
		}

		#endregion Unity Messages

		#region Methods

		public override Vector3 ParseQuilt()=>quiltTexture!=null?base.ParseQuilt():new Vector3(8.0f,5.0f,0.5625f);
		public override Vector4 PreferredSize()=>quiltTexture!=null?base.PreferredSize():new Vector4(4320.0f,4800.0f,8.0f,5.0f);

		protected override void InternalRender() {
			if(material!=null) {
				material.SetVector(_InputSize,new Vector4(quiltSize.x,quiltSize.y));
				material.SetVector(_OutputSize,new Vector4(resolution.x,resolution.y));
				material.SetVector(_Arguments,new Vector4(slope,interval,x0));
			}
			base.InternalRender();
		}

		public override void Init() {
			if(m_IsInited) {return;}
			base.Init();
			//
			if(material==null) {
				material=new Material(Shader.Find("Hidden/C1_Blit"));
			}
			string fn="$(AppData)/Roaming/realplayplatform/deviceConfig.json".GetFullPath();
			if(File.Exists(fn)) {
				JsonConvert.PopulateObject(File.ReadAllText(fn),this);
				Debug.Log("Load official settings at "+fn);
				if(m_Actor!=null) {m_Actor.SetActive(true);}
			}
		}

		#endregion Methods
	}
}
