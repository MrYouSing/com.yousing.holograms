// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

using UnityEngine;using YouSingStudio.Holograms;

namespace YouSingStudio.Private {
	public static partial class ScreenManager
	{
		#region Nested Types
		#endregion Nested Types

		#region Fields

		public static bool s_IsInited;
		public static Display[] s_Displays=null;
		public static RectInt[] s_Rects=null;
		public static string[] s_Titles=new string[]{
			 "Unity Primary Display"
			,"Unity Secondary Display"
			,"Unity Tertiary Display"
			,"Unity Quaternary Display"
			,"Unity Quinary Display"
			,"Unity Senary Display"
			,"Unity Septenary Display"
			,"Unity Octonary Display"
			,"Unity Nonary Display"
			,"Unity Denary Display"
		};

		internal static int s_HiddenSize=2;
		internal static System.Type s_Type_Screen=null;

		#endregion Fields

		#region Methods

		public static void OnDeviceChanged() {
			s_Displays=Display.displays;
			s_Rects=null;
		}

		public static bool FullScreen(this Display thiz) {
			return thiz!=null&&thiz.active
				&&thiz.renderingWidth==thiz.systemWidth
				&&thiz.renderingHeight==thiz.systemHeight;
		}

		public static void SetResolution(this Display thiz,int width,int height,bool fullscreen) {
			if(thiz!=null) {
				if(width<0) {width=thiz.systemWidth;}
				if(height<0) {height=thiz.systemHeight;}
				//
				thiz.SetRenderingResolution(width,height);int x=0,y=0;
				if(fullscreen) {
					width=thiz.systemWidth;
					height=thiz.systemHeight;
				}else {
					x=(thiz.systemWidth-width)/2;
					y=(thiz.systemHeight-height)/2;
				}
				thiz.SetParams(width,height,x,y);
			}
		}
#if UNITY_EDITOR
		public static int SetupDisplay(int display,RectInt rect) {
			if(rect.size.sqrMagnitude==0) {return -1;}
			//
			var bf=(System.Reflection.BindingFlags)(-1)&~System.Reflection.BindingFlags.DeclaredOnly;
			UnityEditor.EditorWindow game=null;int idx=0;
			var type=System.Type.GetType("UnityEditor.GameView,UnityEditor");
			var size=type.GetProperty("targetSize",bf);
			foreach(UnityEditor.EditorWindow it in Resources.FindObjectsOfTypeAll(type)) {
				Vector2 v=(Vector2)size.GetValue(it);
				if(v.x==rect.width&&v.y==rect.height) {game=it;break;}++idx;
			}
			if(game==null&&display>=-1) {
				var CreateWindow=type.GetMethod("CreateWindow",bf,null,new System.Type[]{typeof(System.Type[])},null);
				CreateWindow=CreateWindow.MakeGenericMethod(new System.Type[]{type});
				game=CreateWindow.Invoke(null,new object[]{System.Type.EmptyTypes}) as UnityEditor.EditorWindow ;
				type.GetMethod("SetCustomResolution",bf).Invoke(game,new object[]{new Vector2(rect.width,rect.height),$"{rect.width}x{rect.height}"});
			}
			if(game!=null) {// TODO: UnityEditor snaps position in other platforms.
				if(display<0) {display=Mathf.Min(idx,7);}
				string key=s_Titles[display];
					game.titleContent=new GUIContent(key);
					game.position=new Rect(rect.x,rect.y,rect.width-16,rect.height);
					type.GetField("m_NoCameraWarning",bf).SetValue(game,false);
					type.GetMethod("SetTargetDisplay",bf).Invoke(game,new object[]{display});
					type.GetMethod("SnapZoom",bf).Invoke(game,new object[]{1.0f});
#if UNITY_EDITOR_WIN||UNITY_STANDALONE_WIN
					AsyncTask.Obtain(1.0f,()=>{
						if(!SetEditorWindow(key,rect)) {
							Debug.LogWarning("Please close other GameViews manually and play again.");
						}
					},null).StartAsCoroutine();
#endif
				return display;
			}
			return -1;
		}
#endif

		//

		public static int IndexOf(Display display) {
			if(!s_IsInited) {Init();}
			//
			return System.Array.IndexOf(s_Displays,display);
		}
#if !UNITY_EDITOR_WIN&&!UNITY_STANDALONE_WIN
		public static void Init() {
			if(s_IsInited) {return;}
			s_IsInited=true;
			//
			Display.onDisplaysUpdated-=OnDeviceChanged;
			Display.onDisplaysUpdated+=OnDeviceChanged;
			OnDeviceChanged();
			//s_Type_Screen=System.Type.GetType("System.Windows.Forms.Screen,System.Windows.Forms");// TODO: Mono returns a fake result.
			s_Titles[0]=UnityEngine.Application.productName;
		}

		public static Display GetDisplay(int display) {
			if(!s_IsInited) {Init();}
			//
			Display it=null;
			if(display>=0&&display<(s_Displays?.Length??0)) {
				it=s_Displays[display];
				if(it!=null&&!it.active) {it.Activate();}
			}
			return it;
		}

		public static RectInt[] GetRects() {
			if(!s_IsInited) {Init();}
			//
			if(s_Rects==null) {
				var type=s_Type_Screen;if(type!=null) {
					var bf=(System.Reflection.BindingFlags)(-1)&~System.Reflection.BindingFlags.DeclaredOnly;
					object[] screens=(object[])type.GetProperty("AllScreens",bf).GetValue(null);
					var pb=type.GetProperty("Bounds",bf);
					type=System.Type.GetType("System.Drawing.Rectangle,System.Drawing");
					var px=type.GetProperty("X",bf);var pw=type.GetProperty("Width",bf);
					var py=type.GetProperty("Y",bf);var ph=type.GetProperty("Height",bf);
					int i=0,imax=screens?.Length??0;s_Rects=new RectInt[imax];
					object it;for(;i<imax;++i) {
						it=pb.GetValue(screens[i]);
						s_Rects[i]=new RectInt(
							(int)px.GetValue(it),(int)py.GetValue(it),
							(int)pw.GetValue(it),(int)ph.GetValue(it)
						);
					}
				}else {
					s_Rects=(s_Displays?.Length??0)>0?System.Array.ConvertAll(s_Displays,
						x=>new RectInt(0,0,x.systemWidth,x.systemHeight)):null;
				}
			}
			return s_Rects;
		}

		public static void Activate(int display) {
			if(!s_IsInited) {Init();}
			//
			Display tmp=null;
			if(display>=0&&display<(s_Displays?.Length??0)) {
				tmp=s_Displays[display];
			}
			if(tmp!=null) {
				if(tmp.active) {tmp.SetResolution(-1,-1,true);}
				else {tmp.Activate();}
			}
		}

		public static void Deactivate(int display) {
			if(!s_IsInited) {Init();}
			//
			Display tmp=GetDisplay(display);if(tmp==null||!tmp.active) {return;}
			tmp.SetParams(s_HiddenSize,s_HiddenSize,0,0);
		}

		public static string GetTitle(int display) {
			if(!s_IsInited) {Init();}
			//
			return s_Titles[display];
		}

		public static void SetTitle(int display,string text) {
			if(!s_IsInited) {Init();}
			//
			s_Titles[display]=text;
		}

		public static bool FullScreen(int display) {
			if(!s_IsInited) {Init();}
			//
			Display tmp=GetDisplay(display);if(tmp==null) {return false;}
			return tmp.FullScreen();
		}

		public static void SetResolution(int display,int width,int height,bool fullscreen) {
			if(!s_IsInited) {Init();}
			//
			Display tmp=GetDisplay(display);if(tmp==null) {return;}
			tmp.SetResolution(width,height,fullscreen);
		}
#endif
		#endregion Methods
	}
}
