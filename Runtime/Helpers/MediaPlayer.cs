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
		public Canvas canvas;
		public Text text;
		public RawImage[] images=new RawImage[3];
		[Header("Media")]
		public QuiltTexture quilt;
		public VideoPlayer video;
		public ScreenDevice screen;
		public HologramDevice device;
		
		[System.NonSerialized]protected bool m_IsInited;
		[System.NonSerialized]protected bool m_Loop;
		[System.NonSerialized]protected int m_Retry;
		[System.NonSerialized]protected string m_Path;
		[System.NonSerialized]protected RenderTexture m_RT;
		[System.NonSerialized]protected YieldInstruction m_Wait;

		#endregion Fields

		#region Unity Messages
		#endregion Unity Messages

		#region Methods

		public virtual void Log(string msg) {
			Debug.Log(msg);
			if(text!=null) {text.text=msg;}
		}

		public virtual string GetPath(string path) {
			// TODO: For Mp4ToPng.exe
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
			this.LoadSettings(name+".json");
			if(device==null) {device=GetComponent<HologramDevice>();}
			if(canvas==null) {canvas=images[2].GetComponentInParent<Canvas>();}
			if(video==null) {video=FindAnyObjectByType<VideoPlayer>();}
			m_Wait=new WaitForSeconds(1.0f/refresh.x);
			//
			if(video!=null) {
				video.isLooping=false;
				video.waitForFirstFrame=true;
				video.timeUpdateMode=VideoTimeUpdateMode.GameTime;
				//
				video.prepareCompleted+=OnVideoPrepared;
				video.loopPointReached+=OnVideoLooped;
				//
				video.SetResolution(resolution);
				if(quilt==null) {quilt=video.GetComponent<QuiltTexture>();}
			}
			if(quilt!=null) {
				m_RT=quilt.destination;
				SetTexture(0,m_RT);
			}
			if(screen!=null) {
				screen.Init();
				SetTexture(1,screen.canvas);
			}
			if(device!=null)  {
				device.Init();
				if(device.display>=0) {
					Log($"Display:{device.display}");
					Display.displays[device.display].Activate();
					canvas.targetDisplay=device.display;
				}else {
					canvas.enabled=false;
				}
				SetTexture(2,device.canvas);
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
					quilt.SetSource(texture,path);
				}else {
					device.Quilt(texture,Vector2Int.zero);
					//
					quilt.enabled=false;
					quilt.destination.CopyFrom(texture);
				}
			}else {// Free
				if(device.quiltTexture!=quilt.destination) {// Sync for hologram preview.
					quilt.destination.CopyFrom(device.quiltTexture);
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
			TextureManager tm=TextureManager.instance;
			string ext=Path.GetExtension(m_Path);Texture tex=null;
			if(UnityExtension.IsImage(ext)) {
				tex=tm.Get(m_Path);
				Log("Load a picture at "+m_Path);
				Quilt(tex,m_Path);
			}else if(tm.assets?.TryGetValue(m_Path,out tex)??false) {
				Log("Reuse a cache at "+m_Path);
				Quilt(tex,m_Path);
			}else if(UnityExtension.IsVideo(ext)) {
				video.url=m_Path;
				video.skipOnDrop=false;
				video.Play();
			}
		}

		public virtual void Play() {
			if(!quilt.enabled) {Play(m_Path);}
		}

		public virtual void Stop() {
			if(!m_IsInited) {Init();}
			//
			StopCoroutine("OnVideoTicked");m_Retry=0;
			m_Loop=false;video.url=m_Path=null;
			video.GetTexture().Clear();
			//
			device.enabled=false;
			device.canvas.Clear();device.quiltTexture=m_RT;
			quilt.enabled=false;
			quilt.destination.Clear();
		}

		// Events

		protected virtual void OnVideoPrepared(VideoPlayer _) {
			Log("Prepare for playing "+m_Path);
			Quilt(video.GetTexture(),m_Path);
			//
			double num=video.length*video.frameRate;// Fixed a picture.
			if(num<=s_PictureFrames) {
				m_Loop=false;
				video.skipOnDrop=false;
				StartCoroutine(OnVideoTicked());
				// TODO: Use Mp4ToPng.exe
			}else {
				m_Loop=true;
				video.skipOnDrop=true;
				Log("Play "+m_Path);
			}
		}

		protected virtual void OnVideoLooped(VideoPlayer _) {
			if(!m_Loop) {return;}
			//
			Log("Replay "+m_Path);
			video.Stop();video.Play();
		}

		protected virtual IEnumerator OnVideoTicked() {
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
					Log("Refresh "+m_Path);
					i=m_Retry;Play(m_Path);m_Retry=i+1;
				}
			}else {
				if(m_Retry>0) {Log("Retry "+m_Retry+"/"+refresh.w);}
				Log("Load a picture at "+m_Path);
				Quilt(null,null);
				if(cache) {
					Log("Create a cache at "+m_Path);
					var tm=TextureManager.instance;
					tm.Save(m_Path,device.quiltTexture as RenderTexture);
				}
			}
		}

		#endregion Methods
	}
}
