/* <!-- Macro.Table Shortcut
Quit,
Reload,
FullScreen,
SubScreen,
 Macro.End --> */
/* <!-- Macro.Call  Shortcut
			sm.Add(name+".{0}",{0},keys[i]);++i;
 Macro.End --> */
/* <!-- Macro.Patch
,Start
 Macro.End --> */

/* <!-- Macro.Include
UnityExtension.cs
 Macro.End --> */
/* <!-- Macro.Call DeclareEvent
protected,,Startup,,,,
 Macro.End --> */
/* <!-- Macro.Patch
,Events
 Macro.End --> */
using System.Collections;
using UnityEngine;
using YouSingStudio.Private;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Holograms {
	public class MonoApplication
		:MonoBehaviour
	{
		#region Fields

		public static MonoApplication s_Instance;

		public VirtualDisplay display;
		public int fullscreen=-1;
		/// <summary>
		/// <seealso cref="RefreshRate"/>
		/// </summary>
		public int refresh=-1;
		public Vector4 resolution=new Vector4(1280,720,1920,1080);
		public Key[] keys=new Key[4];

		[System.NonSerialized]public new MonoCamera camera;
		[System.NonSerialized]public HologramDevice device;
		/// <summary>
		/// The normalized depth range in object space.
		/// </summary>
		[System.NonSerialized]public Vector2 depth;
		[System.NonSerialized]public int dirty;
		[System.NonSerialized]protected int m_Display;
		[System.NonSerialized]protected Camera m_Camera;

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			s_Instance=this;
		}

		protected virtual void Start() {
			this.LoadSettings(name);
			//
			var sm=ShortcutManager.instance;int i=0;
// <!-- Macro.Patch Start
			sm.Add(name+".Quit",Quit,keys[i]);++i;
			sm.Add(name+".Reload",Reload,keys[i]);++i;
			sm.Add(name+".FullScreen",FullScreen,keys[i]);++i;
			sm.Add(name+".SubScreen",SubScreen,keys[i]);++i;
// Macro.Patch -->
			m_Display=ScreenManager.IndexOf(Display.main);
			if(display==null) {display=FindAnyObjectByType<VirtualDisplay>();}
			if(refresh>=0) {Application.targetFrameRate=refresh!=0?refresh:
				(int)System.Math.Round(Screen.currentResolution.refreshRateRatio.value);}
			//
			StartCoroutine(StartDelayed());
		}

		protected virtual IEnumerator StartDelayed() {
			yield return new WaitForSeconds(1.0f);
			//
			bool b=PlayerPrefs.GetInt("Screenmanager Fullscreen mode")!=3;
			b=PlayerPrefs.GetInt(name+".FullScreen",b?1:0)==1;
			FullScreen(fullscreen>=0?fullscreen==1:b);
			//
			OnStartup();
		}

		#endregion Unity Messages

		#region Methods

		public static MonoApplication instance {
			get {
				if(s_Instance==null) {
					Debug.LogWarning("Add MonoApplication manually for better performance.");
				}
				return s_Instance;
			}
		}

		public virtual void Quit() {
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying=false;
#else
			Application.Quit();
#endif
		}

		[System.Obsolete]
		public virtual void Reload() {
			if(TextureManager.s_Instance!=null) {
				TextureManager.s_Instance.Clear();
			}
			Application.LoadLevel(Application.loadedLevel);
		}

		public virtual void FullScreen(bool value) {
			if(resolution.sqrMagnitude==0.0f) {return;}
			if(!value) {
				ScreenManager.SetResolution(m_Display,(int)resolution.x,(int)resolution.y,false);
			}else {
				ScreenManager.SetResolution(m_Display,(int)resolution.z,(int)resolution.w,true);
			}
			PlayerPrefs.SetInt(name+".FullScreen",value?1:0);
		}

		public virtual void FullScreen()=>FullScreen(!ScreenManager.FullScreen(m_Display));

		public virtual void SubScreen(bool value) {
			if(display!=null) {display.SetActive(value);}
		}

		public virtual void SubScreen() {
			if(display!=null) {SubScreen(!display.isActive);}
		}

		// Misc

		public virtual void SetupCamera(MonoCamera value) {
			if(value!=null) {
				camera=value;device=camera.device;
				if(display==null) {display=camera.display;}
				m_Camera=camera.GetComponentInChildren<Camera>();
				Debug.Log($"{Application.productName} uses {camera.GetFriendlyName()} to render {device.GetFriendlyName()}.");
			}
		}

		public virtual Bounds GetBounds(Vector3 point) {
			Vector3 size=Vector3.zero;
			if(m_Camera!=null&&device!=null&&depth.sqrMagnitude!=0.0f) {
				float h=m_Camera.GetPlaneHeight(m_Camera.WorldToDepth(point));
				size.Set(device.HeightToWidth()*h,h,device.HeightToDepth()*h);
				Vector3 d=(point-m_Camera.cameraToWorldMatrix.GetPosition()).normalized;
				point+=d*(((depth.x+depth.y)*0.5f-0.5f)*size.z);size.z*=depth.y-depth.x;
			}
			return new Bounds(point,size);
		}

		#endregion Methods
// <!-- Macro.Patch Events
		[System.NonSerialized]public System.Action onStartup=null;
		[SerializeField]protected UnityEngine.Events.UnityEvent m_OnStartup=null;

		protected virtual void OnStartup() {
			onStartup?.Invoke();
			m_OnStartup?.Invoke();
		}

// Macro.Patch -->
	}
}
