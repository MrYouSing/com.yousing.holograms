// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

/* <!-- Macro.Copy
			Window it=null;
			if(display>=0&&display<(s_Displays?.Length??0)) {it=s_Windows[display];}
			if(it!=null) {if(it.index<0) {Activate(display);}}else return
 Macro.End --> */
/* <!-- Macro.Patch
,DisplayToWindow
 Macro.End --> */
#if UNITY_EDITOR_WIN||UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;using YouSingStudio.Holograms;

namespace YouSingStudio.Private {
	public static partial class ScreenManager
	{
		#region Nested Types

		public struct RECT {
			public int left;
			public int top;
			public int right;
			public int bottom;

			public int width=>right-left;
			public int height=>bottom-top;
			public Vector2Int center=>new Vector2Int((left+right)/2,(top+bottom)/2);

			public RectInt ToRectInt()=>new RectInt(left,top,width,height);
			public void CopyFrom(RectInt other) {
				left=other.xMin;right=other.xMax;
				top=other.yMin;bottom=other.yMax;
			}
		}

		// https://referencesource.microsoft.com/#System.Windows.Forms/winforms/Managed/System/WinForms/Screen.cs

		public struct MONITORINFOEX {
			public int cbSize;
			public RECT rcMonitor;
			public RECT rcWork;
			public int dwFlags;
			[MarshalAs(UnmanagedType.ByValArray,SizeConst=32)]
			public char[] szDevice;
		}

		public class Window {
			public string name;
			public int index;
			public int port;
			// Unity
			public Display display;
			public bool fullScreen;
			public int width;
			public int height;
			// WinAPI
			public System.IntPtr handle;
			public bool fixWindow;
			public RECT rect;

			public virtual void StartAsScreen() {
				display=Display.main;string key="Screenmanager Fullscreen mode";
				fullScreen=PlayerPrefs.HasKey(key)?PlayerPrefs.GetInt(key)!=3:display.FullScreen();
				//
				Start();
			}

			public virtual void StartAsDisplay() {
				index=s_Index++;name=s_Titles[port];
				display.Activate(0,0,-1);fullScreen=true;
				handle=FindWindow(null,name);
				//
				Start();
			}

			public virtual void Start() {
#if UNITY_EDITOR
				if(port>=0) {
					RectInt tmp=GetRects()[port];
					rect.left=tmp.xMin;rect.right=tmp.xMax;
					rect.top=tmp.yMin;rect.bottom=tmp.yMax;
				}
#else// TODO: Fallback to Unity.
				if(GetWindowRect(handle,out rect)) {
					Vector2Int p=rect.center;RectInt[] rects=null;
					int i=0,imax=GetRects(ref rects);
					for(;i<imax;++i) {if(rects[i].Contains(p)) {
						rect.CopyFrom(rects[i]);break;
					}}
				}
#endif
				Debug.Log($"{name}:{rect.left},{rect.top},{rect.width},{rect.height}");
			}

			public virtual void Title(string value) {
				name=value;
				if(handle==System.IntPtr.Zero) {return;}
				SetWindowText(handle,value);
			}

			public virtual System.Collections.IEnumerator FixWindow() {
				int i=5;while(i-->0) {yield return null;}
				if(fullScreen) {
					if(!fixWindow) {Start();}// Fix the rect.
				}else {// FullScreenMode.Windowed
					if(handle==System.IntPtr.Zero) {yield break;}
					fixWindow=true;
					//
					SetWindowLong(handle,GWL_STYLE,s_Style);
					SetResolution(handle,rect,width,height,fullScreen);
				}
			}
		}

		#endregion Nested Types

		#region Fields

		public const int GWL_STYLE=-16;
		public static int s_Index;
		public static int s_Style=0x14CA0000;
		public static int s_Monitor;
		public static Vector4 s_Border=new Vector4(8,31,8,8);
		public static System.Text.StringBuilder s_StringBuilder;
		public static Window[] s_Windows=null;

		#endregion Fields

		#region Methods

		[DllImport("user32.dll")]
		public static extern System.IntPtr FindWindow(string lpClassName,string lpWindowName);
		[DllImport("user32.dll")]
		public static extern int GetWindowLong(System.IntPtr hWnd,int nIndex);
		[DllImport("user32.dll")]
		public static extern int SetWindowLong(System.IntPtr hWnd,int nIndex,int dwNewLong);
		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(System.IntPtr hWnd,out RECT lpRect);
		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(System.IntPtr hWnd,System.IntPtr hWndInsertAfter,int X,int Y,int cx,int cy,uint uFlags);
		[DllImport("user32.dll")]
		public static extern int GetWindowText(System.IntPtr hWnd,System.Text.StringBuilder lpString,int nMaxCount);
		[DllImport("user32.dll")]
		public static extern int SetWindowText(System.IntPtr hWnd,string lpString);
		[DllImport("user32.dll")]
		public static extern int GetSystemMetrics(int nIndex);
		[DllImport("user32.dll")]
		public static extern bool EnumDisplayMonitors(System.IntPtr hdc,System.IntPtr rcClip,MonitorEnumProc lpfnEnum,System.IntPtr dwData);
		[DllImport("user32.dll")]
		public static extern bool GetMonitorInfo(System.IntPtr hmonitor,ref MONITORINFOEX info);

		public delegate bool MonitorEnumProc(System.IntPtr monitor,System.IntPtr hdc,System.IntPtr lprcMonitor,System.IntPtr lParam);
		[MonoPInvokeCallback(typeof(MonitorEnumProc))]
		public static bool EnumMonitorCallBack(System.IntPtr monitor,System.IntPtr hdc,System.IntPtr lprcMonitor,System.IntPtr lParam) {
			MONITORINFOEX info=new MONITORINFOEX();info.cbSize=Marshal.SizeOf(typeof(MONITORINFOEX));
			if(GetMonitorInfo(monitor,ref info)) {
				int i=s_Monitor;bool b=info.rcMonitor.left==0&&info.rcMonitor.top==0;
				if((info.dwFlags&0x1)!=0||b) {i=0;--s_Monitor;}// Fix primary or other.
				s_Rects[i]=info.rcMonitor.ToRectInt();
			}
			++s_Monitor;return true;// Next
		}

		// https://blog.csdn.net/smile_Ho/article/details/108881674

		public delegate bool WNDENUMPROC(System.IntPtr hwnd,uint lParam);

		[DllImport("user32.dll",SetLastError = true)]
		public static extern bool EnumWindows(WNDENUMPROC lpEnumFunc,uint lParam);

		[DllImport("user32.dll",SetLastError = true)]
		public static extern System.IntPtr GetParent(System.IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern uint GetWindowThreadProcessId(System.IntPtr hWnd,ref uint lpdwProcessId);

		[MonoPInvokeCallback(typeof(WNDENUMPROC))]
		public static bool EnumWindowCallBack(System.IntPtr hwnd,uint lParam) {
			uint id=0;
			if(GetParent(hwnd)==System.IntPtr.Zero) {
				GetWindowThreadProcessId(hwnd,ref id);
				if(id==lParam) {
					int i=IndexOf(Display.main);
					Window it=s_Windows[i];
					it.name=GetWindowText(hwnd);
					it.index=0;it.handle=hwnd;
					it.StartAsScreen();return false;
				}
			}
			return true;
		}

		// Managed

		public static string GetWindowText(System.IntPtr hWnd) {
			if(s_StringBuilder!=null) {s_StringBuilder.Clear();}
			else {s_StringBuilder=new System.Text.StringBuilder(0xFF);}
			//
			if(GetWindowText(hWnd,s_StringBuilder,s_StringBuilder.Capacity)!=0) {}
			return s_StringBuilder.ToString();
		}

		public static void SetResolution(System.IntPtr hwnd,RECT rect,int width,int height,bool fullscreen) {
			int x=rect.left-(int)s_Border.x,y=rect.top-(int)s_Border.y;
			if(!fullscreen) {
				x+=(rect.width-width)/2;y+=(rect.height-height)/2;
				y+=(int)((s_Border.y-s_Border.w)*0.5f);// Align with title.
			}
			width+=(int)(s_Border.x+s_Border.z);height+=(int)(s_Border.y+s_Border.w);
			SetWindowPos(hwnd,System.IntPtr.Zero,x,y,width,height,0);
		}

		public static bool SetEditorWindow(string title,RectInt rect) {
			System.IntPtr hwnd=FindWindow(null,title);
			if(hwnd!=System.IntPtr.Zero) {
				int h=42;
				SetWindowPos(hwnd,System.IntPtr.Zero// HWND_TOP
					,rect.x-(int)s_Border.x
					,rect.y-(int)s_Border.y-h
					,rect.width+(int)(s_Border.x+s_Border.z)
					,rect.height+(int)(s_Border.y+s_Border.w+h)
				,0);
				return true;
			}
			return false;
		}

		public static int GetRects(ref RectInt[] rects) {
			RectInt[] tmp=s_Rects;s_Monitor=1;
				int imax=GetSystemMetrics(80);// SM_CMONITORS
				if((rects?.Length??0)<imax) {rects=new RectInt[imax];}s_Rects=rects;
				if(!EnumDisplayMonitors(System.IntPtr.Zero,System.IntPtr.Zero,EnumMonitorCallBack,System.IntPtr.Zero)) {
					s_Monitor=0;
				}
			s_Rects=tmp;return s_Monitor;
		}

		public static void Init() {
			if(s_IsInited) {return;}
			s_IsInited=true;
			//
			Display.onDisplaysUpdated-=OnDeviceChanged;
			Display.onDisplaysUpdated+=OnDeviceChanged;
			OnDeviceChanged();
			//s_Type_Screen=System.Type.GetType("System.Windows.Forms.Screen,System.Windows.Forms");// TODO: Mono returns a fake result.
			int i=0,imax=s_Displays?.Length??0;
			s_Windows=new Window[imax];
			Window it;for(;i<imax;++i) {
				it=new Window();
					it.index=-1;it.port=i;
					it.display=s_Displays[i];
				s_Windows[i]=it;
			}
			//
			uint pid=(uint)System.Diagnostics.Process.GetCurrentProcess().Id;
			bool b=EnumWindows(EnumWindowCallBack,pid);
			s_Index=1;
		}

		public static Window GetWindow(int index) {
			if(!s_IsInited) {Init();}
			//
			Window it;for(int i=0,imax=s_Windows?.Length??0;i<imax;++i) {
				it=s_Windows[i];if(it!=null&&it.index==index) {return it;}
			}
			return null;
		}

		public static RectInt[] GetRects() {
			if(!s_IsInited) {Init();}
			//
			if(s_Rects==null) {
#if UNITY_EDITOR
				RectInt[] tmp=null;if(GetRects(ref tmp)>0) {
					s_Rects=tmp;
				}else {
#else// TODO: Fallback to Unity.
				if(true) {
#endif
					s_Rects=(s_Displays?.Length??0)>0?System.Array.ConvertAll(s_Displays,
						x=>new RectInt(0,0,x.systemWidth,x.systemHeight)):null;
				}
			}
			return s_Rects;
		}

		public static void Activate(int display) {
			if(!s_IsInited) {Init();}
			//
			Window it=null;
			if(display>=0&&display<(s_Displays?.Length??0)) {it=s_Windows[display];}
			//
			if(it==null||it.index>=0){return;}it.StartAsDisplay();
			if(it.index==1) {GetWindow(0).Start();}// Fix the rect.
		}

		public static string GetTitle(int display) {
			if(!s_IsInited) {Init();}
			//
// <!-- Macro.Patch DisplayToWindow
			Window it=null;
			if(display>=0&&display<(s_Displays?.Length??0)) {it=s_Windows[display];}
			if(it!=null) {if(it.index<0) {Activate(display);}}else return
// Macro.Patch -->
			null;return it.name;
		}

		public static void SetTitle(int display,string text) {
			if(!s_IsInited) {Init();}
			//
// <!-- Macro.Patch DisplayToWindow
			Window it=null;
			if(display>=0&&display<(s_Displays?.Length??0)) {it=s_Windows[display];}
			if(it!=null) {if(it.index<0) {Activate(display);}}else return
// Macro.Patch -->
			;it.Title(text);
		}

		public static bool FullScreen(int display) {
			if(!s_IsInited) {Init();}
			//
// <!-- Macro.Patch DisplayToWindow
			Window it=null;
			if(display>=0&&display<(s_Displays?.Length??0)) {it=s_Windows[display];}
			if(it!=null) {if(it.index<0) {Activate(display);}}else return
// Macro.Patch -->
			false;return it.display.FullScreen();
		}

		public static void SetResolution(int display,int width,int height,bool fullscreen) {
			if(!s_IsInited) {Init();}
			//
// <!-- Macro.Patch DisplayToWindow
			Window it=null;
			if(display>=0&&display<(s_Displays?.Length??0)) {it=s_Windows[display];}
			if(it!=null) {if(it.index<0) {Activate(display);}}else return
// Macro.Patch -->
			;it.fullScreen=fullscreen;
			it.width=width;it.height=height;
			if(it.index==0) {PlayerPrefs.SetInt("Screenmanager Fullscreen mode",fullscreen?1:3);}
			//
			if(it.fixWindow) {// FullScreenMode.Windowed
				SetResolution(it.handle,it.rect,width,height,fullscreen);
				it.display.SetRenderingResolution(width,height);
			}else if(it.index==0) {// Main display.
				Screen.SetResolution(width,height,fullscreen);
				AsyncTask.GetBehaviour().StartCoroutine(it.FixWindow());
			}else {// Sub display.
				if(!it.display.active) {Activate(display);}
				it.display.SetResolution(width,height,fullscreen);
			}
		}

		#endregion Methods
	}
}
#endif