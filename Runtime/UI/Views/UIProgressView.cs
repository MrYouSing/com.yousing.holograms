using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="System.IProgress{T}"/><br/>
	/// <seealso cref="AsyncOperation"/>
	/// </summary>
	public class UIProgressView
		:MonoBehaviour
	{
		#region Nested Types

		public interface IProgress
			:System.IDisposable
		{
			string name{get;}
			float value{get;}
			string text{get;}
		}

		public class WebRequestProgress
			:IProgress
		{
			public UnityWebRequest www;
			public AsyncOperation ao;
			public ulong size;

			public virtual void Dispose() {
				www?.Dispose();
				//
				www=null;ao=null;size=0;
			}
			public virtual string name=>www?.url;
			public virtual float value=>ao?.progress??-1.0f;
			public virtual string text {
				get {
					if(www!=null&&ao!=null) {
						ulong a=www.downloadedBytes;
						ulong b=www.uploadedBytes;
						if(b>a) {a=b;}
						//
						if(a<=0) {return "Unknown";}
						else if(size>0) {return a.ToByteSize()+"/"+size.ToByteSize();}
						else {return a.ToByteSize();}
					}
					return null;
				}
			}
		}

		#endregion Nested Types

		#region Fields

		public static ulong s_Size;
		public static List<UIProgressView> s_Instances=new List<UIProgressView>();

		public GameObject actor;
		public Image image;
		public Slider slider;
		public Text[] texts;
		public string[] formats;
		[Header("Misc")]
		public float renderRate;
		public int shortCount=0;
		public string shortLink="~";

		[System.NonSerialized]public IProgress progress;
		[System.NonSerialized]protected float m_RenderTime;

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			if(actor==null) {actor=gameObject;}
			s_Instances.Add(this);
		}

		protected virtual void OnDestroy() {
			s_Instances.Remove(this);
		}

		protected virtual void Update() {
			if(progress!=null) {
				float f=progress.value;
				//
				m_RenderTime-=Time.deltaTime;
				if(m_RenderTime<=0.0f) {Render();}
				//
				if(f<0.0f||f>=1.0f) {Set(null);}
			}
		}

		#endregion Unity Messages

		#region Methods

		public static IProgress Get(UnityWebRequest www,AsyncOperation ao,ulong size=0) {
			IProgress tmp=null;
			if(www!=null&&!www.isDone&&ao!=null) {
				tmp=new WebRequestProgress{www=www,ao=ao,size=size>0?size:s_Size};s_Size=0;
			}
			return tmp;
		}

		public virtual void Set(IProgress value) {
			//
			progress=value;bool b=progress!=null;
			enabled=b;actor.SetActive(b);
			Render();
		}

		public virtual void Render() {
			m_RenderTime=renderRate;
			if(progress!=null) {
				float f=progress.value;
				if(slider!=null) {slider.value=f;}
				if(image!=null) {image.fillAmount=f;}
				//
				if(texts[0]!=null) {
					texts[0].text=string.Format(formats[0],progress.name).ToShorty(shortCount,shortLink);
				}
				if(texts[1]!=null) {
					texts[1].text=string.Format(formats[1],progress.text,f);
				}
			}else {
				if(slider!=null) {slider.value=0.0f;}
				if(image!=null) {image.fillAmount=0.0f;}
				if(texts[0]!=null) {texts[0].text=null;}
				if(texts[1]!=null) {texts[1].text=null;}
			}
		}

		public virtual void Close() {
			progress?.Dispose();
			Set(null);
		}

		#endregion Methods
	}
}