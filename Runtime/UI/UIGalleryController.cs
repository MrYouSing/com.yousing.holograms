/* <!-- Macro.Table Shortcut
Quit,
FullScreen,
Refresh,
Play,
Open,
Prev,
Next,
PageUp,
PageDown,
 Macro.End --> */
/* <!-- Macro.Call  Shortcut
			sm.Add(name+".{0}",{0},keys[i]);++i;
 Macro.End --> */
/* <!-- Macro.Patch
,Start
 Macro.End --> */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YouSingStudio.Private;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Holograms {
	public class UIGalleryController
		:MonoBehaviour
	{
		#region Fields

		[Header("Gallery")]
		public Transform container;
		public GameObject prefab;
		public Transform arrow;
		public Vector4 resolution=new Vector4(1280,720,1920,1080);
		public int preview;
		public string[] filters=new string[]{
			// Image
			".jpg",
			".jpeg",
			".png",
			// Video https://docs.lookingglassfactory.com/keyconcepts/quilts/quilt-video-encoding
			".mp4",
			".webm",
		};
		public List<string> paths=new List<string>();
		[Header("Controller")]
		public MediaPlayer player;
		public Key[] keys=new Key[9];

		[System.NonSerialized]protected int m_Index;
		[System.NonSerialized]protected int m_Page;
		[System.NonSerialized]protected List<string> m_Paths;
		[System.NonSerialized]protected List<GameObject> m_Views=new List<GameObject>();
		[System.NonSerialized]protected ScrollRect m_Scroll;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			this.LoadSettings(name+".json");
			//
			var sm=ShortcutManager.instance;int i=0;
// <!-- Macro.Patch Start
			sm.Add(name+".Quit",Quit,keys[i]);++i;
			sm.Add(name+".FullScreen",FullScreen,keys[i]);++i;
			sm.Add(name+".Refresh",Refresh,keys[i]);++i;
			sm.Add(name+".Play",Play,keys[i]);++i;
			sm.Add(name+".Open",Open,keys[i]);++i;
			sm.Add(name+".Prev",Prev,keys[i]);++i;
			sm.Add(name+".Next",Next,keys[i]);++i;
			sm.Add(name+".PageUp",PageUp,keys[i]);++i;
			sm.Add(name+".PageDown",PageDown,keys[i]);++i;
// Macro.Patch -->
			m_Scroll=container.GetComponentInParent<ScrollRect>();
			var g=container.GetComponent<GridLayoutGroup>();
			if(g!=null) {m_Page=g.constraintCount;}
			//
			Refresh();
			StartCoroutine(StartDelayed());
		}

		protected virtual IEnumerator StartDelayed() {
			yield return new WaitForSeconds(2.5f);
			//
			//FullScreen(Screen.fullScreen);
			if(m_Index==0) {Set(0);}
		}

		#endregion Unity Messages

		#region Methods

		public virtual bool CanPlay(string path) {
			if(!string.IsNullOrEmpty(path)) {for(int i=0,imax=filters?.Length??0;i<imax;++i) {
				if(path.EndsWith(filters[i],System.StringComparison.OrdinalIgnoreCase)) {return true;}
			}}
			return false;
		}

		public virtual void Quit() {
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying=false;
#else
			Application.Quit();
#endif
		}

		public virtual void FullScreen(bool value) {
			if(resolution.sqrMagnitude==0.0f) {return;}
			// TODO: Other display maximum.
#if UNITY_EDITOR
#else
			if(!value) {
				Screen.SetResolution((int)resolution.x,(int)resolution.y,FullScreenMode.Windowed);
			}else {
				Screen.SetResolution((int)resolution.z,(int)resolution.w,FullScreenMode.FullScreenWindow);
			}
#endif
		}

		public virtual void FullScreen()=>FullScreen(!Screen.fullScreen);

		[System.Obsolete]
		public virtual void Reload() {
			Application.LoadLevel(Application.loadedLevel);
		}

		public virtual void Refresh() {
			string tmp=m_Index<(m_Paths?.Count??0)?m_Paths[m_Index]:null;
			//
			m_Paths=paths.UnpackPaths(CanPlay,m_Paths);
			m_Views.Render(m_Paths,RenderView,CreateView);
			if(m_Scroll!=null) {
				m_Scroll.normalizedPosition=Vector2.up;
			}
			//
			if(!string.IsNullOrEmpty(tmp)) {Set(tmp);}
		}

		public virtual int Add(string path) {
			int i=m_Paths.Count;m_Paths.Add(path);
			GameObject v=null;
			if(i<m_Views.Count) {v=m_Views[i];}
			else {v=CreateView();m_Views.Add(v);}
			RenderView(v,path);return i;
		}

		public virtual void Set(int index) {
			int len=m_Paths?.Count??0;if(len<=0) {return;}
			m_Index=(index+len)%len;
			//
			var es=EventSystem.current;if(es!=null) {es.SetSelectedGameObject(null);}
			if(arrow!=null) {arrow.SetParent(m_Views[index].transform,false);}
			if(player!=null) {player.Play(m_Paths[index]);}
			// TODO: Select in ScrollRect.
		}

		public virtual void Set(string path) {
			int i=m_Paths.IndexOf(path);
			if(i<0&&CanPlay(path)) {i=Add(path);}
			if(i>=0) {Set(i);}
		}

		public virtual void Play() {
			Set(m_Index);
		}

		public virtual void Open() {
			// TODO: Dialog APIs
			throw new System.NotImplementedException();
		}

		public virtual void Prev() {
			Set(m_Index-1);
		}

		public virtual void Next() {
			Set(m_Index+1);
		}

		public virtual void PageUp() {
			Set(m_Index-m_Page);
		}

		public virtual void PageDown() {
			Set(m_Index+m_Page);
		}

		protected virtual void LoadIcon(RawImage image,string path) {
			TextureManager tm=TextureManager.instance;
			Texture tex=null;float a=0.0f;
			Vector3 count=path.ParseQuilt();
			// TODO: Icons may be wrong because of caches.
			bool b=tm.assets?.TryGetValue(UnityExtension.s_TempTag+path,out tex)??false;
			if(!b||tex==null) {
				b=false;string ext=Path.GetExtension(path);
				if(UnityExtension.IsImage(ext)) {ext="image_00";}
				else if(UnityExtension.IsVideo(ext)) {ext="movie_00";}
				else {ext="Icon_"+ext.Substring(1);}
				//
				tex=tm.Get(ext);if(tex!=null) {
					a=(float)tex.width/tex.height;
				}
			}
			//
			if(!b&&preview>0) {
#if (UNITY_EDITOR_WIN||UNITY_STANDALONE_WIN)&&!NET_STANDARD
				a=0.0f;int size=Mathf.ClosestPowerOfTwo((int)Mathf.Min(preview*count.x,preview*count.y));
				Rect rect=count.ToPreviewRect();
				using(var bm=ShellThumbs.WindowsThumbnailProvider.GetThumbnail(
					path,size,size,ShellThumbs.ThumbnailOptions.None
				)) {
					count.x=bm.Width;count.y=bm.Height;
					var tmp=UnityExtension.NewTexture2D(1,1);
					tmp.LoadBitmap(bm,new RectInt(
						(int)(count.x*rect.x),
						(int)(count.y*rect.y),
						(int)(count.x*rect.width),
						(int)(count.y*rect.height)
					));
					tmp.name=UnityExtension.s_TempTag+path;
					tm.Set(tmp.name,tmp);tex=tmp;
				}
#endif
			}
			//
			if(tex==null) {image.enabled=false;}
			else {image.enabled=true;image.texture=tex;}
			if(a==0.0f) {a=Mathf.Abs(count.z);}
			var fit=image.GetComponent<AspectRatioFitter>();
			if(fit!=null) {fit.aspectRatio=a;}
		}

		protected virtual GameObject CreateView() {
			GameObject go=GameObject.Instantiate(prefab);
			go.transform.SetParent(container,false);
			//
			int i=m_Views.Count;
			Button b=go.GetComponentInChildren<Button>();
			if(b!=null) {b.onClick.AddListener(()=>Set(i));}
			return go;
		}

		protected virtual void RenderView(GameObject view,string path) {
			if(view==null) {return;}
			if(string.IsNullOrEmpty(path)) {view.SetActive(false);return;}
			//
			Text txt=view.GetComponentInChildren<Text>();
			if(txt!=null) {
				txt.text=Path.GetFileName(path);
			}
			//
			RawImage img=view.GetComponentInChildren<RawImage>();
			if(img!=null) {
				LoadIcon(img,path);
			}
			//
			view.SetActive(true);
		}

		#endregion Methods
	}
}
