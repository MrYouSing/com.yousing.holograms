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
		[System.NonSerialized]protected string m_Path;
		[System.NonSerialized]protected RenderTexture m_RT;
		[System.NonSerialized]protected YieldInstruction m_Wait=new WaitForSeconds(0.1f);

		#endregion Fields

		#region Unity Messages
		#endregion Unity Messages

		#region Methods

		public virtual void Log(string msg) {
			Debug.Log(msg);
			if(text!=null) {text.text=msg;}
		}

		protected virtual void Init() {
			if(m_IsInited) {return;}
			m_IsInited=true;
			//
			this.LoadSettings(name+".json");
			if(device==null) {device=GetComponent<HologramDevice>();}
			if(canvas==null) {canvas=FindAnyObjectByType<Canvas>();}
			if(video==null) {video=FindAnyObjectByType<VideoPlayer>();}
			//
			if(video!=null) {
				video.prepareCompleted+=OnVideoPrepared;
				video.loopPointReached+=OnVideoLooped;
				if(quilt==null) {quilt=video.GetComponent<QuiltTexture>();}
			}
			if(quilt!=null) {
				m_RT=quilt.destination;
				images[0].texture=m_RT;
			}
			if(screen!=null) {
				screen.Init();
				images[1].texture=screen.canvas;
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
				images[2].texture=device.canvas;
			}
		}

		public virtual void Quilt(Texture texture,string path) {
			if(!m_IsInited) {Init();}
			//
			if(texture!=null) {
				device.quiltTexture=m_RT;device.enabled=true;
				quilt.enabled=true;
				quilt.SetDestination(device);
				quilt.SetSource(texture,path);
			}else {// Free
				video.url=null;
				quilt.enabled=false;
			}
		}
		

		public virtual void Play(string path) {
			if(!m_IsInited) {Init();}
			//
			Stop();m_Path=path;
			if(string.IsNullOrEmpty(m_Path)) {return;}
			//
			TextureManager tm=TextureManager.instance;
			string ext=Path.GetExtension(m_Path);Texture tex=null;
			if(UnityExtension.IsImage(ext)) {
				tex=tm.Get(m_Path);
				Log("Load a picture at "+m_Path);
				if(m_Path.ParseQuilt()!=device.ParseQuilt()) {
					Quilt(tex,m_Path);
				}else {
					device.Quilt(tex,Vector2Int.zero);
					quilt.destination.CopyFrom(tex);
				}
			}else if(tm.assets?.TryGetValue(m_Path,out tex)??false) {
				Log("Reuse cache at "+m_Path);
				device.canvas.CopyFrom(tex);
				if(tm.assets.TryGetValue(m_Path+"_quilt",out tex)){
					quilt.destination.CopyFrom(tex);
				}
			}else if(UnityExtension.IsVideo(ext)) {
				video.url=m_Path;
				video.Play();
			}
		}

		public virtual void Stop() {
			if(!m_IsInited) {Init();}
			//
			StopCoroutine("OnVideoTicked");
			m_Loop=false;video.url=m_Path=null;
			(video.texture as RenderTexture).Clear();
			//
			device.enabled=false;
			device.canvas.Clear();
			quilt.enabled=false;
			quilt.destination.Clear();
		}

		// Events

		protected virtual void OnVideoPrepared(VideoPlayer vp) {
			Log("Prepare for playing "+m_Path);
			Quilt(vp.texture,m_Path);
			//
			double num=vp.length*vp.frameRate;// Fixed a picture.
			if(num<=s_PictureFrames) {
				m_Loop=false;
				vp.skipOnDrop=false;
				StartCoroutine(OnVideoTicked());
			}else {
				m_Loop=true;
				vp.skipOnDrop=true;
				Log("Play "+m_Path);
			}
		}

		protected virtual void OnVideoLooped(VideoPlayer vp) {
			if(!m_Loop) {return;}
			//
			Log("Replay "+m_Path);
			vp.Stop();vp.Play();
		}

		protected virtual IEnumerator OnVideoTicked() {
			// Wait for the first render frame.
			int i=10;bool b;
			while((b=device.canvas.IsNullOrEmpty())&&i-->0) {
				yield return m_Wait;
			}
			//
			if(b) {
				Log("Refresh "+m_Path);
				Play(m_Path);
			}else {
				Log("Load a picture at "+m_Path);
				Quilt(null,null);
				// Use cache system????
			}
		}

		#endregion Methods
	}
}
