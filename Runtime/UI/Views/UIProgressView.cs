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
		[System.NonSerialized]public IProgress progress;

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
				float f=progress.value;Render();
				if(f<0.0f||f>=1.0f) {Set(null);}
			}
		}

		#endregion Unity Messages

		#region Methods

		public static void Set(string path,IProgress progress) {
			UIProgressView v=UnityExtension.GetResourceInstance<UIProgressView>(path);
			if(v!=null) {v.Set(progress);}s_Size=0;
		}

		public static void Set(string path,UnityWebRequest www,AsyncOperation ao) {
			if(www!=null&&!www.isDone&&ao!=null) {
				var tmp=new WebRequestProgress{www=www,ao=ao,size=s_Size};
				Set(path,tmp);
			}
		}

		public virtual void Set(IProgress value) {
			//
			progress=value;bool b=progress!=null;
			enabled=b;actor.SetActive(b);
			Render();
		}

		public virtual void Render() {
			if(progress!=null) {
				float f=progress.value;
				if(slider!=null) {slider.value=f;}
				if(image!=null) {image.fillAmount=f;}
				//
				if(texts[0]!=null) {
					texts[0].text=string.Format(formats[0],progress.name);
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