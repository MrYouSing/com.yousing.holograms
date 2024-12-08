/* <!-- Macro.Table Shortcut
Refresh,
Play,
Load,
Save,
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
		public DialogPicker[] pickers=new DialogPicker[2];
		public Key[] keys=new Key[7];

		[System.NonSerialized]protected int m_Index;
		[System.NonSerialized]protected int m_Page;
		[System.NonSerialized]protected List<string> m_Paths;
		[System.NonSerialized]protected List<GameObject> m_Views=new List<GameObject>();
		[System.NonSerialized]protected ScrollRect m_Scroll;
		[System.NonSerialized]protected GridLayoutGroup m_Layout;
		[System.NonSerialized]protected WaitForEndOfFrame m_WaitEof;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			this.LoadSettings(name);
			//
			MonoCamera.s_AllowDummy=true;
			ImageConverter.settings.download=UnityExtension.Path_Combine(paths[0],"Downloads");
			TextureManager.instance.SetupFormats();
			//
			var sdk=Sketchfab.SketchfabSdk.instance;
			if(sdk!=null) {sdk.download=UnityExtension.Path_Combine(paths[0],"Downloads/sketchfab.com");}
			//
			var sm=ShortcutManager.instance;int i=0;
// <!-- Macro.Patch Start
			sm.Add(name+".Refresh",Refresh,keys[i]);++i;
			sm.Add(name+".Play",Play,keys[i]);++i;
			sm.Add(name+".Load",Load,keys[i]);++i;
			sm.Add(name+".Save",Save,keys[i]);++i;
			sm.Add(name+".Prev",Prev,keys[i]);++i;
			sm.Add(name+".Next",Next,keys[i]);++i;
			sm.Add(name+".PageUp",PageUp,keys[i]);++i;
			sm.Add(name+".PageDown",PageDown,keys[i]);++i;
// Macro.Patch -->
			m_Scroll=container.GetComponentInParent<ScrollRect>();
			m_Layout=container.GetComponent<GridLayoutGroup>();
			if(m_Layout!=null) {m_Page=m_Layout.constraintCount;}
			// You need wait until app startup.if not,you will
			// get an error hwnd on windows because of GDI+.
			var app=MonoApplication.instance;
			if(app!=null) {app.onStartup+=StartDelayed;}
			else {AsyncTask.Obtain(0.1f,StartDelayed).StartAsCoroutine();}
		}

		protected virtual void StartDelayed() {
			Refresh();
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
			int i=m_Paths.Count;m_Paths.Add(Path.GetFullPath(path).FixPath());
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
			UpdateScroll();
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

		public virtual void Load() {
			//
			string url=GUIUtility.systemCopyBuffer;
			if(!string.IsNullOrEmpty(url)) {
				if(url.IsWebsite()) {
					if(ImageConverter.ConvertFromWeb(url,Set)) {return;}
					if(url.IndexOf("sketchfab",UnityExtension.k_Comparison)>=0) {
						var sdk=Sketchfab.SketchfabSdk.instance;
						if(sdk!=null) {sdk.Download(sdk.GetUid(url),OnSketchfab);}
						return;
					}
				}else {
					if(File.Exists(url)) {Set(url);return;}
				}
			}
			//
			var p=pickers[0];if(p!=null) {
				p.onPicked=Set;p.ShowDialog();
			}
		}

		protected virtual void OnDrop(string value) {
			bool b=Input.GetKey(Key.LeftControl)||Input.GetKey(Key.RightControl);
			if(b&&File.Exists(value)) {
				//
				string pv=TextureManager.instance.GetPreview(value);
				if(!string.IsNullOrEmpty(pv)&&File.Exists(pv)) {
					pv.CopyToDirectory(paths[0]);
				}
				//
				value=value.CopyToDirectory(paths[0]);
			}
			//
			DialogPicker tmpP=pickers[0];
			string tmpS=GUIUtility.systemCopyBuffer;
				GUIUtility.systemCopyBuffer=value;
				pickers[0]=null;Load();
			pickers[0]=tmpP;
			GUIUtility.systemCopyBuffer=tmpS;
		}

		public virtual void Save() {
			// TODO: Add Toolbox????
			bool b=ShortcutManager.s_Instance==null||ShortcutManager.s_Instance.current==null;
			//
			var p=pickers[1];if(b&&p!=null) {// Click open dialog.
				p.onPicked=Screenshot;p.ShowDialog();
			}else {
				Screenshot("*");
			}
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

		protected virtual void UpdateScroll() {
			StopCoroutine("UpdateScrollDelayed");StartCoroutine(UpdateScrollDelayed());
		}

		protected virtual IEnumerator UpdateScrollDelayed() {
			yield return null;
			if(m_Scroll!=null) {
				RectTransform rt=m_Views[m_Index].transform as RectTransform;
				Vector2 u,v;if(m_Layout==null) {
					u=0.5f*rt.sizeDelta;v=Vector2.zero;
				}else {
					u=0.5f*m_Layout.cellSize;v=m_Layout.spacing;
				}
				m_Scroll.normalizedPosition=m_Scroll.GetNormalizedPoint(rt,u,v);
			}
		}

		/// <summary>
		/// <seealso cref="ScreenCapture.CaptureScreenshot(string)"/>
		/// </summary>
		public virtual void Screenshot(string path) {
			if(string.IsNullOrEmpty(path)) {return;}
			if(path=="*") {
				path=System.DateTime.Now.ToString("yyyyMMddHHmmss")+Random.Range(0,10000).ToString("0000");
				path=Path.Combine("Screenshots",path);
			}
			StopCoroutine("ScreenshotDelayed");StartCoroutine(ScreenshotDelayed(path));
		}

		protected virtual IEnumerator ScreenshotDelayed(string path) {
			var d=player!=null?player.device:null;if(d==null) {yield break;}
			yield return m_WaitEof;d.Screenshot(path);
			// For C1 devices.
			if(ImageConverter.FFmpegSupported()) {
				string fn=path+"_quilt.png";if(File.Exists(fn)) {
					ImageConverter.ImageToVideo(Path.GetFullPath(fn),d.quiltTexture.GetSizeI(),path+".mp4");
				}
			}
		}

		protected virtual void InternalPlay(string path) {
			TextureType type=path.ToTextureType(texture);
			switch(type) {
				case TextureType.Depth:
					InternalPlay("Open RGB-D",path);
				break;
				case TextureType.Stereo:{
					Vector3 count=path.ParseQuilt();
					if(!count.TwoPieces()) {path.SetQuilt(path.ParseLayout());}
					//
					InternalPlay(null,path);
				}break;
				case TextureType.Raw:
				case TextureType.Quilt:
					InternalPlay(null,path);
				break;
				case TextureType.Model:
					InternalPlay("Open Model "+Path.GetExtension(path),path);
				break;
			}
			type.InvokeEvent();
		}

		protected virtual void InternalPlay(string stage,string path) {
			if(!string.IsNullOrEmpty(stage)) {
				if(player!=null) {player.Stop();}// Stop the other.
				if(director!=null) {director.Open(stage,path);}
			}else {
				if(director!=null) {director.Set("Open Media");}// Stop the other.
				if(player!=null) {player.Play(path);}
			}
		}

		protected virtual void OnSketchfab(string path) {
			InternalPlay("Open Sketchfab",path);TextureType.Model.InvokeEvent();
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
				case TextureType.Raw:// Only $(FileName)_preview.$(Extension)
				case TextureType.Quilt:
					count=path.ParseQuilt();
				break;
			}
			// TODO: Icons may be wrong because of caches.
			bool b=tm.assets?.TryGetValue(UnityExtension.s_TempTag+path,out tex)??false;
			if(!b||tex==null) {
				b=false;string icon=Path.GetExtension(path);
				string pv=tm.GetPreview(path);
				if(!string.IsNullOrEmpty(pv)) {icon=pv;b=true;}// Highest priority.
				else {icon=tm.ToIconKey(icon);}
				//
				tex=tm.Get(icon);if(tex!=null) {
					a=(float)tex.width/tex.height;
				}
			}
			//
			if(!b&&preview*count.x*count.y>0.0f) {
#if (UNITY_EDITOR_WIN||UNITY_STANDALONE_WIN)&&!NET_STANDARD
				a=0.0f;int size=Mathf.ClosestPowerOfTwo((int)Mathf.Min(preview*count.x,preview*count.y));
				Rect rect=count.ToPreview();
				using(var bm=ShellThumbs.WindowsThumbnailProvider.GetThumbnail(
					path,size,size,ShellThumbs.ThumbnailOptions.None
				)) {
					int bw=bm.Width,bh=bm.Height;
					var tmp=RenderingExtension.NewTexture2D(1,1);
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
