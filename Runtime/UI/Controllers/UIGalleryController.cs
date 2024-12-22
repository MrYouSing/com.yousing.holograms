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
using UnityEngine.UI;
using YouSingStudio.Private;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Holograms {
	public class UIGalleryController
		:HologramPlayer
	{
		#region Fields

		[Header("Gallery")]
		public int preview;
		public List<string> paths=new List<string>();
		[Header("Controller")]
		public UISelectorView selector;
		public DialogPicker[] pickers=new DialogPicker[2];
		public Key[] keys=new Key[4];

		[System.NonSerialized]protected int m_Index;
		[System.NonSerialized]protected List<string> m_Paths;

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
// Macro.Patch -->
			if(selector==null) {selector=GetComponent<UISelectorView>();}
			//
			if(selector!=null) {
				selector.onRender=RenderView;
				selector.onSelect=Play;
			}
			// You need wait until app startup.if not,you will
			// get an error hwnd on windows because of GDI+.
			var app=MonoApplication.instance;
			if(app!=null) {app.onStartup+=StartDelayed;}
			else {AsyncTask.Obtain(0.1f,StartDelayed).StartAsCoroutine();}
		}

		protected virtual void StartDelayed() {
			Refresh();
			if(m_Index==0) {Play(0);}
		}

		#endregion Unity Messages

		#region Methods

		public virtual void Refresh() {
			string tmp=m_Index<(m_Paths?.Count??0)?m_Paths[m_Index]:null;
			//
			m_Paths=paths.UnpackPaths(CanPlay,m_Paths);
			selector.Render(m_Paths);
			//
			if(!string.IsNullOrEmpty(tmp)) {Play(tmp);}
		}

		public virtual int Add(string path) {
			int i=m_Paths.Count;m_Paths.Add(path);
			selector.Add(path);return i;
		}

		public virtual void Play(int index) {
			int len=m_Paths?.Count??0;if(len<=0) {return;}
			m_Index=(index+len)%len;
			//
			selector.Highlight(index);
			InternalPlay(m_Paths[m_Index]);
		}

		public override void Play(string path) {
			path=path.GetFilePath();
			//
			int i=m_Paths.IndexOf(path);
			if(i<0&&CanPlay(path)) {i=Add(path);}
			if(i>=0) {Play(i);}
		}

		public virtual void Play() {
			Play(m_Index);
		}

		public virtual void Load() {
			string url=GUIUtility.systemCopyBuffer;
			if(TryPlay(url)) {return;}
			//
			var p=pickers[0];if(p!=null) {
				p.onPicked=Play;p.ShowDialog();
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

		protected virtual bool RenderView(GameObject view,string path) {
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
			return true;
		}

		#endregion Methods
	}
}
