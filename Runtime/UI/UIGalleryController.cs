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
		public Key[] keys=new Key[7];

		[System.NonSerialized]protected int m_Index;
		[System.NonSerialized]protected List<string> m_Paths;
		[System.NonSerialized]protected List<GameObject> m_Views=new List<GameObject>();
		[System.NonSerialized]protected ScrollRect m_Scroll;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			this.LoadSettings(name+".json");
			//
			var sm=ShortcutManager.instance;int i=0;
			sm.Add(name+".Quit",Quit,keys[i]);++i;
			sm.Add(name+".FullScreen",FullScreen,keys[i]);++i;
			sm.Add(name+".Refresh",Refresh,keys[i]);++i;
			sm.Add(name+".Play",Play,keys[i]);++i;
			sm.Add(name+".Open",Open,keys[i]);++i;
			sm.Add(name+".Prev",Prev,keys[i]);++i;
			sm.Add(name+".Next",Next,keys[i]);++i;
			//
			m_Scroll=container.GetComponentInParent<ScrollRect>();
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
			if(value) {
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
			m_Paths=paths.UnpackPaths(CanPlay,m_Paths);
			m_Views.Render(m_Paths,RenderView,CreateView);
			if(m_Scroll!=null) {
				m_Scroll.normalizedPosition=Vector2.up;
			}
		}

		public virtual void Set(int index) {
			int len=m_Paths?.Count??0;if(len<=0) {return;}
			m_Index=(index+len)%len;
			//
			var es=EventSystem.current;
			if(es!=null) {es.SetSelectedGameObject(null);}
			if(player!=null) {player.Play(m_Paths[index]);}
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

		protected virtual void LoadIcon(RawImage image,string path) {
			Texture2D tex=image.texture as Texture2D;
			if(tex!=null&&tex.name==UnityExtension.s_TempTag) {Texture2D.Destroy(tex);}
			//
			string ext=Path.GetExtension(path);float a=1.0f;
			if(UnityExtension.IsImage(ext)) {ext="Icon/Image";}
			else if(UnityExtension.IsImage(ext)) {ext="Icon/Video";}
			else {ext=null;}image.texture=TextureManager.instance.Get(ext);
			//
			if(preview>0) {
				Vector3 count=path.ParseQuilt();a=Mathf.Abs(count.z);
#if UNITY_EDITOR_WIN||UNITY_STANDALONE_WIN
				int size=Mathf.ClosestPowerOfTwo((int)Mathf.Min(preview*count.x,preview*count.y));
				Rect rect=count.ToPreviewRect();
				using(var bm=ShellThumbs.WindowsThumbnailProvider.GetThumbnail(
					path,size,size,ShellThumbs.ThumbnailOptions.None
				)) {
					count.x=bm.Width;count.y=bm.Height;
					tex=UnityExtension.NewTexture2D(1,1);tex.name=UnityExtension.s_TempTag;
					tex.LoadBitmap(bm,new RectInt(
						(int)(count.x*rect.x),
						(int)(count.y*rect.y),
						(int)(count.x*rect.width),
						(int)(count.y*rect.height)
					));image.texture=tex;
				}
#else
				throw new System.NotImplementedException();
#endif
			}
			//
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
