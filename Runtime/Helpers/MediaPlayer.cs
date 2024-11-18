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
		[System.NonSerialized]protected RenderTexture m_RT;
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
				m_RT=quilt.destination;
				SetTexture(0,m_RT);
			}
			if(screen!=null) {
				screen.Init();
				if(screen.quiltTexture==null) {screen.quiltTexture=m_RT;}
				SetTexture(1,screen.canvas);
			}
		}

		public virtual void Quilt(Texture texture,string path) {
			if(!m_IsInited) {Init();}
			//
			if(texture!=null) {
				device.enabled=true;
				if(//UnityExtension.IsVideo(Path.GetExtension(path))||
					device.ParseQuilt()!=path.ParseQuilt()
				) {
					device.quiltTexture=m_RT;
					//
					quilt.enabled=true;
					quilt.SetDestination(device);
					quilt.SetSource(texture,path.ParseQuilt());
				}else {
					device.Quilt(texture,Vector2Int.zero);
					//
					quilt.enabled=false;// Sync for hologram preview.
					if((images?.Length??0)>=0) {quilt.destination.CopyFrom(texture);}
				}
			}else {// Free
				if(device.quiltTexture!=quilt.destination) {// Sync for hologram preview.
					if((images?.Length??0)>=0) {quilt.destination.CopyFrom(device.quiltTexture);}
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
				screen.SetDefaultIndex(path.ToTextureType()!=TextureType.Quilt?0:-1);
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
				video.isLooping=false;// For shared VideoPlayer.
				video.skipOnDrop=false;
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
			device.canvas.Clear();device.quiltTexture=m_RT;
			quilt.enabled=false;
			quilt.destination.Clear();
			//
			if(screen!=null) {screen.SetDefaultIndex(-1);}
		}

		// Events

		protected virtual void OnVideoPrepared(VideoPlayer _) {
			if(!isActiveAndEnabled) {return;}// For shared VideoPlayer.
			Log("Prepare for playing "+m_Path);
			Quilt(video.GetTexture(),m_Path);
			//
			double num=video.length*video.frameRate;// Fixed a picture.
			if(num<=s_PictureFrames) {
				m_Loop=false;
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
				video.isLooping=true;
				Log("Play "+m_Path);
			}
		}

		protected virtual void OnVideoLooped(VideoPlayer _) {
			if(!isActiveAndEnabled) {return;}// For shared VideoPlayer.
			if(!m_Loop) {return;}
			//
			Log("Replay "+m_Path);
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
