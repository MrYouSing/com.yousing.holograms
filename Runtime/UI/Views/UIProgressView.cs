/* <!-- Macro.Define DeclareStruct_00
		public partial class $(Table.Name):IProgress {{
 Macro.End --> */
/* <!-- Macro.Define DeclareStruct_01
			public {0} {1};
 Macro.End --> */
/* <!-- Macro.Define DeclareStruct_02

			public virtual void Reset() {{
 Macro.End --> */
/* <!-- Macro.Define DeclareStruct_03
				{1}={2};
 Macro.End --> */
/* <!-- Macro.Define DeclareStruct_04
			}}
		}}

 Macro.End --> */
/* <!-- Macro.Table DeclareStruct
DeclareStruct_00,
DeclareStruct_01
DeclareStruct_02,
DeclareStruct_03
DeclareStruct_04,
 Macro.End --> */
/* <!-- Macro.Define DeclareGet_00
		public static IProgress Get(
 Macro.End --> */
/* <!-- Macro.Define DeclareGet_01
			{3},{0} {1}={2}
 Macro.End --> */
/* <!-- Macro.Define DeclareGet_02
		) {{
			return new $(Table.Name){{
 Macro.End --> */
/* <!-- Macro.Define DeclareGet_03
				{3},{1}={1}
 Macro.End --> */
/* <!-- Macro.Define DeclareGet_04
			}};
		}}
 Macro.End --> */
/* <!-- Macro.Table DeclareGet
DeclareGet_00,
DeclareGet_01
DeclareGet_02,
DeclareGet_03
DeclareGet_04,
 Macro.End --> */

/* <!-- Macro.Table DelegateProgress
System.Func<string>,name,null,#,
System.Func<float>,value,null,,
System.Func<string>,text,null,,
System.Action,dispose,null,,
 Macro.End --> */
/* <!-- Macro.Table FileProgress
string,path,null,#,
string,key,null,,
int,start,0,,
int,count,0,,
System.Action,dispose,null,,
 Macro.End --> */
/* <!-- Macro.Table WebRequestProgress
UnityWebRequest,www,null,#,
AsyncOperation,ao,null,,
ulong,size,0,,
 Macro.End --> */

/* <!-- Macro.BatchCall DeclareStruct DelegateProgress
 Macro.End --> */
/* <!-- Macro.BatchCall DeclareGet DelegateProgress
 Macro.End --> */
/* <!-- Macro.BatchCall DeclareStruct FileProgress
 Macro.End --> */
/* <!-- Macro.BatchCall DeclareGet FileProgress
 Macro.End --> */
/* <!-- Macro.Replace
#&#44;, 
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
using System.Collections.Generic;
using System.IO;
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
// <!-- Macro.Patch AutoGen
		public partial class DelegateProgress:IProgress {
			public System.Func<string> name;
			public System.Func<float> value;
			public System.Func<string> text;
			public System.Action dispose;

			public virtual void Reset() {
				name=null;
				value=null;
				text=null;
				dispose=null;
			}
		}

		public static IProgress Get(
			 System.Func<string> name=null
			,System.Func<float> value=null
			,System.Func<string> text=null
			,System.Action dispose=null
		) {
			return new DelegateProgress{
				 name=name
				,value=value
				,text=text
				,dispose=dispose
			};
		}
		public partial class FileProgress:IProgress {
			public string path;
			public string key;
			public int start;
			public int count;
			public System.Action dispose;

			public virtual void Reset() {
				path=null;
				key=null;
				start=0;
				count=0;
				dispose=null;
			}
		}

		public static IProgress Get(
			 string path=null
			,string key=null
			,int start=0
			,int count=0
			,System.Action dispose=null
		) {
			return new FileProgress{
				 path=path
				,key=key
				,start=start
				,count=count
				,dispose=dispose
			};
		}
// Macro.Patch -->
		#region Nested Types

		public interface IProgress
			:System.IDisposable
		{
			string name{get;}
			float value{get;}
			string text{get;}
		}

		public partial class DelegateProgress
		{
			string IProgress.name=>name?.Invoke()??null;
			float IProgress.value=>value?.Invoke()??-1.0f;
			string IProgress.text=>text?.Invoke()??null;
			public virtual void Dispose() {
				dispose?.Invoke();
				Reset();
			}
		}

		public partial class FileProgress
		{
			string IProgress.name=>path;
			float IProgress.value=>(GetCount()-start)/(float)count;
			string IProgress.text=>$"{GetCount()-start}/{count}";
			public virtual void Dispose() {
				dispose?.Invoke();
				Reset();
			}

			protected virtual int GetCount() {
				int ret=Directory.GetFiles(path,key,SearchOption.TopDirectoryOnly)?.Length??0;
				if(start<0) {start=ret;}return ret;
			}
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
						ulong s=size;float f=value;
						if(s==0&&f>=0.1f) {s=(ulong)(a/f);}
						//
						if(a<=0) {return "Unknown";}
						else if(s>0) {return a.ToByteSize()+"/"+s.ToByteSize();}
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