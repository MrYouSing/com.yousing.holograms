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

		#endregion Fields

		#region Unity Messages

		protected virtual void Reset() {
			resolution=new Vector2Int(1440,2560);
			quiltSize=new Vector2Int(8,5);
		}

		#endregion Unity Messages

		#region Methods

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
		}

		#endregion Methods
	}
}
