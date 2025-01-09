using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class HologramPlayer
		:MonoBehaviour
	{
		#region Fields

		public static List<string> s_Whitelist=new List<string>{
			// For General
			"_preview",
			// For RGB-D
			"_depth",
		};

		[Header("Player")]
		public TextureType texture=TextureType.Quilt;
		public UnityEngine.Video.Video3DLayout layout=UnityEngine.Video.Video3DLayout.SideBySide3D;
		public MediaPlayer player;
		public StageDirector director;
		public List<string> filters=new List<string>{
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

		[System.NonSerialized]protected bool m_Dirty;
		[System.NonSerialized]protected string m_Path;
		[System.NonSerialized]protected string m_Last;
		[System.NonSerialized]protected HologramDevice m_Device;
		[System.NonSerialized]protected RenderTexture m_DstRT;
		[System.NonSerialized]protected Texture2D m_Src2D;
		[System.NonSerialized]protected Texture2D m_Dst2D;
		[System.NonSerialized]protected WaitForEndOfFrame m_WaitEof;

		#endregion Fields

		#region Unity Messages
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
				for(int i=0,imax=filters?.Count??0;i<imax;++i) {
					if(path.EndsWith(filters[i],UnityExtension.k_Comparison)) {return true;}
				}
			}
			return false;
		}

		public virtual void Play(string path) {
			if(File.Exists(path)&&CanPlay(path)) {InternalPlay(path);}
		}

		public virtual bool TryPlay(string path) {
			if(!string.IsNullOrEmpty(path)) {
				if(path.IsWebsite()) {
					if(ImageConverter.ConvertFromWeb(path,Play)) {return true;}
					if(path.IndexOf("sketchfab",UnityExtension.k_Comparison)>=0) {
						var sdk=Sketchfab.SketchfabSdk.instance;
						if(sdk!=null) {sdk.Download(sdk.GetUid(path),OnSketchfab);}
						return true;
					}
				}else {
					if(File.Exists(path)) {Play(path);return true;}
				}
			}
			return false;
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
					type.InvokeEvent();
					InternalPlay("Open Model "+Path.GetExtension(path),path);
				return;
			}
			type.InvokeEvent();
		}

		protected virtual void InternalPlay(string stage,string path) {
			m_Path=path;
			if(!string.IsNullOrEmpty(stage)) {
				if(player!=null) {player.Stop();}// Stop the other.
				if(director!=null) {director.Open(stage,m_Path);}
			}else {
				if(director!=null) {director.Set("Open Media");}// Stop the other.
				if(player!=null) {player.Play(m_Path);}
			}
		}

		protected virtual void OnSketchfab(string path) {
			TextureType.Model.InvokeEvent();
			InternalPlay("Open Sketchfab",path);
		}

		// Capture Methods

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
			if(m_WaitEof==null) {m_WaitEof=new WaitForEndOfFrame();}
			yield return m_WaitEof;d.Screenshot(path);
			// For C1 devices.
			if(ImageConverter.FFmpegSupported()) {
				string fn=path+"_quilt.png";if(File.Exists(fn)) {
					ImageConverter.ImageToVideo(Path.GetFullPath(fn),d.quiltTexture.GetSizeI(),path+".mp4");
				}
			}
		}

		public virtual void BeginCapture(HologramDevice device) {
			m_Last=m_Path;
			//
			m_Device=device;
			if(m_Device!=null) {
				m_Device.onPostRender-=OnCapture;
				m_Device.onPostRender+=OnCapture;
				m_DstRT=m_Device.canvas;
			}
			if(m_DstRT!=null) {
				bool b=false;
				m_Src2D=RenderingExtension.NewTexture2D(1,1,b);
				m_Dst2D=RenderingExtension.NewTexture2D(m_DstRT.width,m_DstRT.height,b);
			}
		}

		public virtual IEnumerator OnCapture(string path) {
			if(m_Dst2D==null||!File.Exists(path)) {yield break;}
			//
			m_Src2D.LoadImage(File.ReadAllBytes(path));
			path="Temp"+path.GetFileExtension();
			TextureManager.instance.Set(path,m_Src2D);
			//
			m_Dirty=true;InternalPlay(path);
			if(m_Dirty) {m_Device.Render();}
		}

		public virtual void OnCapture() {
			if(!m_Dirty) {return;}
			//
			var tmp=m_DstRT.Begin();
				m_Dst2D.ReadPixels(new Rect(0,0,m_Dst2D.width,m_Dst2D.height),0,0);m_Dst2D.Apply();
			m_DstRT.End(tmp);
			m_Dirty=false;
		}

		public virtual void EndCapture() {
			m_Path=m_Last;
			m_Device.onPostRender-=OnCapture;
			//
			m_Dirty=false;m_Last=null;
			m_Device=null;m_DstRT=null;
			RenderingExtension.Free(ref m_Src2D);
			RenderingExtension.Free(ref m_Dst2D);
			//
			Play(m_Path);
		}

		public virtual Texture2D GetCapture()=>m_Dirty?null:m_Dst2D;

		#endregion Methods
	}
}
