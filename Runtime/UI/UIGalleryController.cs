/* <!-- Macro.Table Shortcut
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

		public static List<string> s_Whitelist=new List<string>{
			// For General
			"_preview",
			// For RGB-D
			"_depth",
		};

		[Header("Gallery")]
		public Transform container;
		public GameObject prefab;
		public Transform arrow;
		public TextureType texture=TextureType.Quilt;
		public UnityEngine.Video.Video3DLayout layout=UnityEngine.Video.Video3DLayout.SideBySide3D;
		public int preview;
		public string[] filters=new string[]{
			// Image
			".jpg",
			".jpeg",
			".png",
			// Video https://docs.lookingglassfactory.com/keyconcepts/quilts/quilt-video-encoding
			".mp4",
			".webm",
			// Model
			".unity",
			".ab",// AssetBundle
			".gltf",
			".glb",
			".obj",
			".fbx",
		};
		public List<string> paths=new List<string>();
		[Header("Controller")]
		public MediaPlayer player;
		public StageDirector director;
		public Key[] keys=new Key[7];

		[System.NonSerialized]protected int m_Index;
		[System.NonSerialized]protected int m_Page;
		[System.NonSerialized]protected List<string> m_Paths;
		[System.NonSerialized]protected List<GameObject> m_Views=new List<GameObject>();
		[System.NonSerialized]protected ScrollRect m_Scroll;
		[System.NonSerialized]protected GridLayoutGroup m_Layout;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			this.LoadSettings(name);
			//
			MonoCamera.s_AllowDummy=true;
			var sm=ShortcutManager.instance;int i=0;
// <!-- Macro.Patch Start
			sm.Add(name+".Refresh",Refresh,keys[i]);++i;
			sm.Add(name+".Play",Play,keys[i]);++i;
			sm.Add(name+".Open",Open,keys[i]);++i;
			sm.Add(name+".Prev",Prev,keys[i]);++i;
			sm.Add(name+".Next",Next,keys[i]);++i;
			sm.Add(name+".PageUp",PageUp,keys[i]);++i;
			sm.Add(name+".PageDown",PageDown,keys[i]);++i;
// Macro.Patch -->
			m_Scroll=container.GetComponentInParent<ScrollRect>();
			m_Layout=container.GetComponent<GridLayoutGroup>();
			if(m_Layout!=null) {m_Page=m_Layout.constraintCount;}
			//
			Refresh();
			StartCoroutine(StartDelayed());
		}

		protected virtual IEnumerator StartDelayed() {
			yield return null;
			// After all started.
			if(m_Index==0) {Set(0);}
		}

		#endregion Unity Messages

		#region Methods

		public virtual bool CanPlay(string path) {
			if(!string.IsNullOrEmpty(path)) {
				//
				string fn=Path.GetFileNameWithoutExtension(path);
				for(int i=0,imax=s_Whitelist.Count;i<imax;++i) {
					if(fn.EndsWith(s_Whitelist[i],UnityExtension.k_Comparison)) {return false;}
				}
				//
				for(int i=0,imax=filters?.Length??0;i<imax;++i) {
					if(path.EndsWith(filters[i],System.StringComparison.OrdinalIgnoreCase)) {return true;}
				}
			}
			return false;
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
			if(arrow!=null) {arrow.SetParent(m_Views[m_Index].transform,false);}
			if(m_Scroll!=null) {
				RectTransform rt=m_Views[m_Index].transform as RectTransform;
				Vector2 u,v;if(m_Layout==null) {
					u=0.5f*rt.sizeDelta;v=Vector2.zero;
				}else {
					u=0.5f*m_Layout.cellSize;v=m_Layout.spacing;
				}
				m_Scroll.normalizedPosition=m_Scroll.GetNormalizedPoint(rt,u,v);
			}
			//
			InternalPlay(m_Paths[m_Index]);
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

		protected virtual void InternalPlay(string path) {
			TextureType type=path.ToTextureType(texture);
			switch(type) {
				case TextureType.Depth:
					if(player!=null) {player.Stop();}// Stop the other.
					if(director!=null) {director.Open("Open RGB-D",path);}
				break;
				case TextureType.Stereo:{
					Vector3 count=path.ParseQuilt();
					if(!count.TwoPieces()) {path.SetQuilt(path.ParseLayout());}
					if(director!=null) {director.Set("Open Media");}// Stop the other.
					if(player!=null) {player.Play(path);}
				}break;
				case TextureType.Quilt:
					if(director!=null) {director.Set("Open Media");}// Stop the other.
					if(player!=null) {player.Play(path);}
				break;
				case TextureType.Model:
					if(player!=null) {player.Stop();}// Stop the other.
					if(director!=null) {director.Open("Open Model "+Path.GetExtension(path),path);}
				break;
			}
			type.InvokeEvent();
		}

		protected virtual string GetPreview(string path) {
			path=Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path)+"_preview");string
			pv=path+".png";if(File.Exists(pv)) {return pv;}
			pv=path+".jpg";if(File.Exists(pv)) {return pv;}
			pv=path+".jpeg";if(File.Exists(pv)) {return pv;}
			return null;
		}

		protected virtual void LoadIcon(RawImage image,string path) {
			TextureManager tm=TextureManager.instance;
			Texture tex=null;float a=0.0f;
			//
			Vector3 count=new Vector3(1.0f,1.0f,float.NaN);
			TextureType type=path.ToTextureType(texture);
			switch(type) {
				case TextureType.Stereo:
				case TextureType.Depth:
					count=path.ParseQuilt();
					if(!count.TwoPieces()) {count=path.ParseLayout(layout);}
				break;
				case TextureType.Quilt:
					count=path.ParseQuilt();
				break;
			}
			// TODO: Icons may be wrong because of caches.
			bool b=tm.assets?.TryGetValue(UnityExtension.s_TempTag+path,out tex)??false;
			if(!b||tex==null) {
				b=false;string icon=Path.GetExtension(path);
				string pv=GetPreview(path);
				if(!string.IsNullOrEmpty(pv)) {icon=pv;b=true;}// Highest priority.
				else {icon=tm.ToIconKey(icon);}
				//
				tex=tm.Get(icon);if(tex!=null) {
					a=(float)tex.width/tex.height;
				}
			}
			//
			if(!b&&preview>0) {
#if (UNITY_EDITOR_WIN||UNITY_STANDALONE_WIN)&&!NET_STANDARD
				a=0.0f;int size=Mathf.ClosestPowerOfTwo((int)Mathf.Min(preview*count.x,preview*count.y));
				Rect rect=count.ToPreview();
				using(var bm=ShellThumbs.WindowsThumbnailProvider.GetThumbnail(
					path,size,size,ShellThumbs.ThumbnailOptions.None
				)) {
					int bw=bm.Width,bh=bm.Height;
					var tmp=UnityExtension.NewTexture2D(1,1);
					tmp.LoadBitmap(bm,new RectInt(
						(int)(bw*rect.x),
						(int)(bh*rect.y),
						(int)(bw*rect.width),
						(int)(bh*rect.height)
					));
					tmp.name=UnityExtension.s_TempTag+path;
					tm.Set(tmp.name,tmp);tex=tmp;
					if(float.IsNaN(count.z)) {
						count.z=-(bw*rect.width)/(bh*rect.height);
						path.SetQuilt(count);// Approximately
					}
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
