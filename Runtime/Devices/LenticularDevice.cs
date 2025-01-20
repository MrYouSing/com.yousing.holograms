using Newtonsoft.Json.Linq;
using UnityEngine;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso href="https://en.wikipedia.org/wiki/Lenticular_printing"/>
	/// </summary>
	public class LenticularDevice
		:HologramDevice
	{
		#region Nested Types
		#endregion Nested Types

		#region Fields

		public static readonly int _InputSize=Shader.PropertyToID("_InputSize");
		public static readonly int _OutputSize=Shader.PropertyToID("_OutputSize");
		public static readonly int _Arguments=Shader.PropertyToID("_Arguments");

		[Header("Lenticular")]
		public float pitch;
		public float slope;
		public float center;
		public bool rotated;

		[System.NonSerialized]protected int m_Display=int.MinValue;// User Display.

		#endregion Fields

		#region Unity Messages
		#endregion Unity Messages

		#region Methods

		protected override void InternalRender() {
			if(material!=null) {
				material.SetVector(_InputSize,new Vector4(quiltSize.x,quiltSize.y));
				material.SetVector(_OutputSize,new Vector4(resolution.x,resolution.y));
				material.SetVector(_Arguments,new Vector4(pitch,slope,center,rotated?1.0f:0.0f));
			}
			base.InternalRender();
		}

		protected virtual void InitDisplay() {
			string key=name+".display";
			//
			if(display<0&&PlayerPrefs.HasKey(key)) {display=PlayerPrefs.GetInt(key);}
			if(m_Display==int.MinValue) {m_Display=display;}
		}

		protected virtual void SetDisplay(int value) {
			string key=name+".display";
			//
			PlayerPrefs.SetInt(key,m_Display=value);
			if(MonoApplication.s_Instance!=null) {}// TODO: Restart app????
		}

		protected virtual void FromJson(JObject jo) {
			JToken jt=jo.SelectToken(nameof(display));
			if(jt!=null) {
				SetDisplay(jt.Value<int>());
				jo.Remove(nameof(display));
			}
		}

		protected virtual void ToJson(JObject jo) {
			jo[nameof(name)]=name;
			jo[nameof(display)]=m_Display;
			jo[nameof(rotated)]=rotated;
			jo[nameof(pitch)]=pitch;
			jo[nameof(slope)]=slope;
			jo[nameof(center)]=center;
		}

		public override void Init() {
			if(m_IsInited) {return;}
			InitDisplay();base.Init();
		}

		public override void FromJson(string json) {
			if(string.IsNullOrEmpty(json)) {return;}
			if(!m_IsInited) {Init();}
			//
			JObject jo=JObject.Parse(json);
			FromJson(jo);if(jo.Count>0) {base.FromJson(jo.ToString());}
		}

		public override string ToJson() {
			if(!m_IsInited) {Init();}
			//
			JObject jo=new JObject();
			ToJson(jo);return jo.ToString(Newtonsoft.Json.Formatting.Indented);
		}

		#endregion Methods
	}
}
