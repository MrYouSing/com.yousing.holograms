#if DEBUG
#define _DEBUG
#endif
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// A hologram player for images and videos.
	/// </summary>
	public class MediaPlayer
		:MonoBehaviour
	{
		#region Fields

		public static int s_PictureFrames=24;

		public bool cache=true;
		public Vector2Int resolution=Vector2Int.zero;
		[Tooltip("x:Rate\ny:Time\nz:Wait\nw:Count")]
		public Vector4 refresh=new Vector4(10.0f,1.0f,1.0f,10.0f);
		[Header("UI")]
		public Text text;
		public RawImage[] images=new RawImage[2];
		[Header("Media")]
		public QuiltRenderer quilt;
		public VideoPlayer video;
		public ScreenDevice screen;
		public HologramDevice device;
		
		[System.NonSerialized]protected bool m_IsInited;
		[System.NonSerialized]protected bool m_Loop;
		[System.NonSerialized]protected int m_Lock;
		[System.NonSerialized]protected int m_Retry;
		[System.NonSerialized]protected string m_Path;
		[System.NonSerialized]protected RenderTexture m_RT0;
		[System.NonSerialized]protected RenderTexture m_RT1;
		[System.NonSerialized]protected YieldInstruction m_Wait;
		[System.NonSerialized]protected Private.AsyncTask m_FFmpeg;

		#endregion Fields

		#region Unity Messages

		protected virtual void OnDestroy() {
			ImageConverter.Exit();
		}

		#endregion Unity Messages

		#region Methods

		public virtual void Log(string msg) {
#if _DEBUG
			Debug.Log(msg);
#endif
			if(text!=null) {text.text=msg;}
		}

		public virtual string GetPath(string path) {
			return path;
		}

		public virtual void SetTexture(int index,Texture texture) {
			RawImage img=images[index];
			if(img!=null) {img.texture=texture;}
		}

		protected virtual void Init() {
			if(m_IsInited) {return;}
			m_IsInited=true;
			//
			this.LoadSettings(name);
			if(device==null) {device=GetComponent<HologramDevice>();}
			if(video==null) {video=FindAnyObjectByType<VideoPlayer>();}
			m_RT1=device.canvas;
			m_Wait=new WaitForSeconds(1.0f/refresh.x);
			//
			if(video!=null) {
				video.waitForFirstFrame=true;
				video.timeUpdateMode=VideoTimeUpdateMode.GameTime;
				//
				video.prepareCompleted+=OnVideoPrepared;
				video.loopPointReached+=OnVideoLooped;
				//
				video.SetResolution(resolution);
				if(quilt==null) {quilt=video.GetComponent<QuiltRenderer>();}
			}
			if(quilt!=null) {
				m_RT0=quilt.destination;
			}
			if(screen!=null) {
				screen.Init();
				SetTexture(1,screen.canvas);
			}
		}

		public virtual void Preview(Texture texture,int type) {
			if(!m_IsInited) {Init();}
			//
			quilt.enabled=false;// Sync for hologram preview.
			if(texture!=null&&screen!=null) {
			switch(type) {
				case 0:
					SetTexture(0,texture);screen.quiltTexture=texture;
				break;
				case 1:
					m_RT0.CopyFrom(texture);SetTexture(0,m_RT0);screen.quiltTexture=m_RT0;
				break;
				case 2:
					m_RT0.Blit(texture,device.quiltSize.x,device.quiltSize.y);
				break;
			}}
		}

		public virtual void Quilt(Texture texture,string path) {
			if(!m_IsInited) {Init();}
			//
			if(texture!=null) {
				bool v=UnityExtension.IsVideo(Path.GetExtension(path));
				Vector3 q=path.ParseQuilt();device.enabled=v;
				if(q.sqrMagnitude==0.0f) {// Raw
					device.canvas=null;device.Quilt(texture,Vector2Int.zero);
					//
					var tm=TextureManager.instance;
					Preview(tm.Get(tm.GetPreview(path)),2);
				}else if(device.ParseQuilt()!=q) {
					device.quiltTexture=m_RT0;
					//
					quilt.enabled=v;
					quilt.SetDestination(device);
					quilt.SetSource(texture,q);
					if(!v) {quilt.RenderDestination();device.Render();}
				}else {
					device.Quilt(texture,Vector2Int.zero);
					//
					Preview(texture,0);
				}
			}else {// Free
				if(device.quiltTexture!=m_RT0) {
					Preview(device.quiltTexture,1);
				}
				//
				video.url=null;
				quilt.enabled=false;
				device.enabled=false;
			}
		}

		public virtual void Play(string path) {
			if(!m_IsInited) {Init();}
			//
			Stop();m_Path=GetPath(path);
			if(string.IsNullOrEmpty(m_Path)) {return;}
			//
			if(screen!=null) {
				var t=path.ToTextureType();
				screen.SetDefaultIndex(t!=TextureType.Quilt&&t!=TextureType.Raw?0:-1);
			}
			TextureManager tm=TextureManager.instance;
			string ext=Path.GetExtension(m_Path);Texture tex=null;
			if(UnityExtension.IsImage(ext)) {
				tex=tm.Get(m_Path);
				Log("Load a picture at "+m_Path);
				Quilt(tex,m_Path);
			}else if((tm.assets?.TryGetValue(m_Path,out tex)??false)&&tex!=null) {
				Log("Reuse a cache at "+m_Path);
				Quilt(tex,m_Path);
			}else if(UnityExtension.IsVideo(ext)) {
				video.url=m_Path;
				video.SetLoop(false);// For shared VideoPlayer.
				video.Play();
			}
		}

		public virtual void Play() {
			if(!quilt.enabled&&!string.IsNullOrEmpty(m_Path)) {Play(m_Path);}
		}

		public virtual void Stop() {
			if(!m_IsInited) {Init();}
			//
			StopCoroutine("OnVideoTicked");++m_Lock;m_Retry=0;
			m_FFmpeg?.Kill();m_FFmpeg=null;
			m_Loop=false;video.url=m_Path=null;
			video.GetTexture().Clear();
			//
			device.enabled=false;
			device.canvas=m_RT1;m_RT1.Clear();
			device.quiltTexture=m_RT0;m_RT0.Clear();
			quilt.enabled=false;
			//
			if(screen!=null) {
				SetTexture(0,m_RT0);
				screen.quiltTexture=m_RT0;
				screen.SetDefaultIndex(-1);
			}
		}

		// Video Methods

		protected virtual bool IsMyVideo()=>!string.IsNullOrEmpty(m_Path)&&isActiveAndEnabled;// TODO: For Shared VideoPlayer.

		protected virtual bool IsPicVideo() {
			if(!string.IsNullOrEmpty(m_Path)) {
				return video.length*video.frameRate<=s_PictureFrames;
			}
			return false;
		}

		protected virtual bool IsBigVideo() {
			if(!string.IsNullOrEmpty(m_Path)) {
				var s=video.GetTexture().GetSizeI();
				return (s.x*s.y>=2048*2048)
					&&video.length>=60.0f
					&&video.frameRate>=60.0f;
			}
			return false;
		}

		protected virtual void OnVideoPrepared(VideoPlayer _) {
			if(!IsMyVideo()) {return;}
			Log("Prepare for playing "+m_Path);
			Quilt(video.GetTexture(),m_Path);
			//
			if(IsPicVideo()) {
				m_Loop=false;
				video.SetLoop(false);// Drop for rendering.
				//
				if(true) {StartCoroutine(OnVideoTicked());}
				else {video.Stop();}
				//
				if(ImageConverter.FFmpegSupported()) {
					//ImageConverter.settings.taskWait=refresh.x+refresh.y;
					m_FFmpeg=ImageConverter.VideoToImage(m_Path,OnVideoConverted);
				}
			}else {
				m_Loop=true;
				if(IsBigVideo()) {video.SetLoop(false);}// Get faster decoding.
				else {video.SetLoop(true);}// Fix drop in loop.
				//
				Log("Play "+m_Path);
			}
		}

		protected virtual void OnVideoLooped(VideoPlayer _) {
			if(!IsMyVideo()) {return;}
			if(!m_Loop) {return;}
			//
			Log("Replay "+m_Path);
			if(!video.isLooping) {video.Stop();video.Play();}
		}

		protected virtual IEnumerator OnVideoTicked() {
			int id=++m_Lock;
			// Wait for the first render frame.
			int i=Mathf.RoundToInt(refresh.x*refresh.y);bool b;
			while((b=device.canvas.IsNullOrEmpty())&&i-->0) {
				yield return m_Wait;
			}
			//
			if(b) {
				if(m_Retry>=refresh.w) {
					Log("Failed to Load "+m_Path);
					Stop();
				}else {
					i=Mathf.RoundToInt(refresh.x*refresh.z);
					while(i-->0) {yield return m_Wait;}
					//
					if(id==m_Lock) {OnVideoRefreshed();}
					else {OnVideoSkipped();}
				}
			}else {
				if(id==m_Lock) {OnVideoConverted();}
				else {OnVideoSkipped();}
			}
		}

		protected virtual void OnVideoSkipped() {
			video.url=null;Log("Skip "+m_Path);
		}

		protected virtual void OnVideoRefreshed() {
			Log("Refresh "+m_Path);
			int i=m_Retry;var d=m_FFmpeg;m_FFmpeg=null;// VideoPlayer.Begin
				Play(m_Path);
			m_Retry=i+1;m_FFmpeg=d;// VideoPlayer.End
		}

		protected virtual void OnVideoConverted() {
			m_FFmpeg?.Kill();m_FFmpeg=null;// Stop other.
			//
			if(m_Retry>0) {Log("Retry "+m_Retry+"/"+refresh.w);}
			Log("VideoPlayer load a picture at "+m_Path);
			Quilt(null,null);ImageConverter.OnVideoPlayerConvert(m_Path);
			if(cache) {
				Log("Create a cache at "+m_Path);
				TextureManager tm=TextureManager.instance;
				tm.Save(m_Path,device.quiltTexture as RenderTexture);
			}
		}

		  // External

		protected virtual void OnVideoConverted(string video,string image) {
			if(m_Path!=video) {return;}
			StopCoroutine("OnVideoTicked");++m_Lock;m_FFmpeg=null;// Stop other.
			//
			Log("FFmpeg load a picture at "+m_Path);
			TextureManager tm=TextureManager.instance;
			Texture tex=tm.Load(image);Quilt(tex,m_Path);
			if(cache) {
				Log("Create a cache at "+m_Path);
				tm.Set(video,tex);
			}
		}

		#endregion Methods
	}
}
