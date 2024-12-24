/* <!-- Macro.Table Messages
Info,
Warning,
Error,
Progress,
 Macro.End --> */
/* <!-- Macro.Define ShowDialog
		public static GameObject Show{0}(string style,string text,string caption=null,UnityAction action=null) {{
			//
			if(string.IsNullOrEmpty(caption)) {{caption=s_Caption_{0};}}
			if(!string.IsNullOrEmpty(s_Log)) {{Debug.Log{0}Format(s_Log,caption,text);}}
			//
			ScriptableView tmp=Show<ScriptableView>(style,s_Style_Error);
			if(tmp!=null) {{
				tmp.SetText(0,caption);
				tmp.SetText(1,text);
				SetButton(tmp,0,action);
				//
				return Show(tmp.gameObject);
			}}
			return null;
		}}

 Macro.End --> */
/* <!-- Macro.Include
../UI/Views/UIProgressView.cs
 Macro.End --> */
/* <!-- Macro.Define DeclareProgress_00
		public static GameObject ShowProgress(
			 string style
 Macro.End --> */
/* <!-- Macro.Define DeclareProgress_01
			,{0} {1}={2}
 Macro.End --> */
/* <!-- Macro.Define DeclareProgress_02
		) {{
			UIProgressView tmp=Show<UIProgressView>(style,s_Style_Progress);
			if(tmp!=null) {{
				tmp.Set(UIProgressView.Get(
 Macro.End --> */
/* <!-- Macro.Define DeclareProgress_03
					{3},{1}:{1}
 Macro.End --> */
/* <!-- Macro.Define DeclareProgress_04
				));
				return Show(tmp.gameObject);
			}}
			return null;
		}}

 Macro.End --> */
/* <!-- Macro.Table DeclareProgress
DeclareProgress_00,
DeclareProgress_01
DeclareProgress_02,
DeclareProgress_03
DeclareProgress_04,
 Macro.End --> */

/* <!-- Macro.Call  Messages
		public static string s_Caption_{0}="{0}";
		public static string s_Style_{0}="Prefabs/{0}_00";

 Macro.End --> */
/* <!-- Macro.Call ShowDialog
Info,
Warning,
Error,
 Macro.End --> */
/* <!-- Macro.BatchCall DeclareProgress DelegateProgress
 Macro.End --> */
/* <!-- Macro.BatchCall DeclareProgress WebRequestProgress
 Macro.End --> */
/* <!-- Macro.Replace
#&#44;, 
LogInfo,Log
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="System.Windows.Forms.MessageBox"/>
	/// </summary>
	public partial class MessageBox
	{
// <!-- Macro.Patch AutoGen
		public static string s_Caption_Info="Info";
		public static string s_Style_Info="Prefabs/Info_00";

		public static string s_Caption_Warning="Warning";
		public static string s_Style_Warning="Prefabs/Warning_00";

		public static string s_Caption_Error="Error";
		public static string s_Style_Error="Prefabs/Error_00";

		public static string s_Caption_Progress="Progress";
		public static string s_Style_Progress="Prefabs/Progress_00";

		public static GameObject ShowInfo(string style,string text,string caption=null,UnityAction action=null) {
			//
			if(string.IsNullOrEmpty(caption)) {caption=s_Caption_Info;}
			if(!string.IsNullOrEmpty(s_Log)) {Debug.LogFormat(s_Log,caption,text);}
			//
			ScriptableView tmp=Show<ScriptableView>(style,s_Style_Error);
			if(tmp!=null) {
				tmp.SetText(0,caption);
				tmp.SetText(1,text);
				SetButton(tmp,0,action);
				//
				return Show(tmp.gameObject);
			}
			return null;
		}

		public static GameObject ShowWarning(string style,string text,string caption=null,UnityAction action=null) {
			//
			if(string.IsNullOrEmpty(caption)) {caption=s_Caption_Warning;}
			if(!string.IsNullOrEmpty(s_Log)) {Debug.LogWarningFormat(s_Log,caption,text);}
			//
			ScriptableView tmp=Show<ScriptableView>(style,s_Style_Error);
			if(tmp!=null) {
				tmp.SetText(0,caption);
				tmp.SetText(1,text);
				SetButton(tmp,0,action);
				//
				return Show(tmp.gameObject);
			}
			return null;
		}

		public static GameObject ShowError(string style,string text,string caption=null,UnityAction action=null) {
			//
			if(string.IsNullOrEmpty(caption)) {caption=s_Caption_Error;}
			if(!string.IsNullOrEmpty(s_Log)) {Debug.LogErrorFormat(s_Log,caption,text);}
			//
			ScriptableView tmp=Show<ScriptableView>(style,s_Style_Error);
			if(tmp!=null) {
				tmp.SetText(0,caption);
				tmp.SetText(1,text);
				SetButton(tmp,0,action);
				//
				return Show(tmp.gameObject);
			}
			return null;
		}

		public static GameObject ShowProgress(
			 string style
			,System.Func<string> name=null
			,System.Func<float> value=null
			,System.Func<string> text=null
			,System.Action dispose=null
		) {
			UIProgressView tmp=Show<UIProgressView>(style,s_Style_Progress);
			if(tmp!=null) {
				tmp.Set(UIProgressView.Get(
					 name:name
					,value:value
					,text:text
					,dispose:dispose
				));
				return Show(tmp.gameObject);
			}
			return null;
		}

		public static GameObject ShowProgress(
			 string style
			,UnityWebRequest www=null
			,AsyncOperation ao=null
			,ulong size=0
		) {
			UIProgressView tmp=Show<UIProgressView>(style,s_Style_Progress);
			if(tmp!=null) {
				tmp.Set(UIProgressView.Get(
					 www:www
					,ao:ao
					,size:size
				));
				return Show(tmp.gameObject);
			}
			return null;
		}

// Macro.Patch -->
		#region Fields

		public static string s_Log="[{0}]:{1}";
		public static GameObject s_Actor;

		#endregion Fields

		#region Methods

		public static T Show<T>(string style,string fallback) where T:Object {
			bool a=string.IsNullOrEmpty(style),b=string.IsNullOrEmpty(fallback);
			T tmp=a?null:UnityExtension.GetResourceInstance<T>(style);
			//
			if(tmp==null) {
				tmp=b?null:UnityExtension.GetResourceInstance<T>(fallback);
				if(!a&&tmp!=null) {UnityExtension.s_ResourceInstances[style]=tmp;}
			}
			//
			return tmp;
		}

		public static GameObject Show(GameObject actor) {
			if(s_Actor!=null) {s_Actor.SetActive(false);}
			s_Actor=actor;
			if(s_Actor!=null) {s_Actor.SetActive(true);}
			return s_Actor;
		}

		public static void Clear() {
			if(s_Actor!=null) {s_Actor.SetActive(false);}
			s_Actor=null;
		}

		public static void SetButton(ScriptableView view,int index,UnityAction action) {
			if(view==null) {return;}
			//
			var btn=index<(view.m_Buttons?.Length??0)
				?view.m_Buttons[index]:null;
			if(btn!=null) {
				btn.onClick.RemoveAllListeners();
				if(action!=null) {
					btn.onClick.AddListener(action);
					//
					btn.gameObject.SetActive(true);
				}else {
					btn.gameObject.SetActive(false);
				}
			}
		}

		public static GameObject ShowProgress(string style,UIProgressView.IProgress progress) {
			UIProgressView tmp=Show<UIProgressView>(style,s_Style_Progress);
			if(tmp!=null) {
				tmp.Set(progress);
				return Show(tmp.gameObject);
			}
			return null;
		}

		public static GameObject ShowProgress(string style,string key,System.Func<float> func,System.Action action=null) {
			UIProgressView tmp=Show<UIProgressView>(style,s_Style_Progress);
			if(tmp!=null) {
				tmp.Set(UIProgressView.Get(name:()=>key,value:func,dispose:action));
				return Show(tmp.gameObject);
			}
			return null;
		}

		#endregion Methods
	}
}
