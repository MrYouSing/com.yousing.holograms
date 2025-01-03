using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace YouSingStudio.Holograms {
	public class TextureRenderer
		:MonoBehaviour
	{
		#region Fields

		[Header("Texture")]
		[SerializeField]protected string m_Path;
		public Texture texture;
		public Vector2 resolution=Vector2.zero;
		public Video3DLayout layout;
		public VideoAspectRatio aspect;
		[Header("Video")]
		public VideoPlayer video;
		public bool loop=true;
		[Header("Components")]
		public Transform root;
		public new Renderer renderer;
		public Material material;

		[System.NonSerialized]protected bool m_IsInited;
		[System.NonSerialized]protected Transform m_Renderer;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(!m_IsInited) {
				if(!string.IsNullOrEmpty(m_Path)) {Play(m_Path);}
				else if(texture!=null) {Render(texture);}
			}
		}

		#endregion Unity Messages

		#region Methods

		public virtual void Play(string path) {
			Stop();
			string ext=Path.GetExtension(path);
			if(UnityExtension.IsImage(ext)) {
				PlayImage(path);
			}else if(UnityExtension.IsVideo(ext)) {
				PlayVideo(path);
			}
		}

		public virtual void PlayImage(string path) {
			TextureManager tm=TextureManager.instance;
			Texture tex=tm.Get(m_Path=path);
			if(tex!=null) {Render(tex);}
		}

		public virtual void PlayVideo(string path) {
			if(video!=null) {
				video.prepareCompleted-=OnVideoPrepared;
				video.prepareCompleted+=OnVideoPrepared;
				video.SetLoop(loop);
				video.url=m_Path=path;video.Play();
			}
		}

		protected virtual void OnVideoPrepared(VideoPlayer _) {
			if(string.IsNullOrEmpty(m_Path)||!isActiveAndEnabled) {return;}// TODO: For Shared VideoPlayer.
			//
			if(_!=null) {_.prepareCompleted-=OnVideoPrepared;}
			if(video!=null) {Render(video.GetTexture());}
		}

		public virtual void SetAspect(VideoAspectRatio value) {
			if(!isActiveAndEnabled) {aspect=value;return;}
			//
			if(value!=aspect) {
				aspect=value;
			}
			OnAspectUpdated();
		}

		protected virtual void Init() {
			if(m_IsInited) {return;}
			m_IsInited=true;
			//
			this.LoadSettings(name);
			if(material==null) {material=Instantiate(RenderingExtension.GetUnlit());}
			if(renderer==null) {renderer=GetComponentInChildren<Renderer>();}
			renderer.sharedMaterial=material;m_Renderer=renderer.transform;
			if(root==null) {root=transform;}
		}

		public virtual void Stop() {
			if(!m_IsInited) {Init();}
			//
			if(video!=null) {
				video.prepareCompleted-=OnVideoPrepared;
				video.Stop();
			}
			if(renderer!=null) {
				renderer.enabled=false;
			}
		}

		public virtual void Render(Texture value) {
			if(!m_IsInited) {Init();}
			//
			texture=value;
			material.mainTexture=texture;
			if(renderer!=null) {renderer.enabled=texture!=null;}
			OnAspectUpdated();
		}

		protected virtual void OnAspectUpdated() {
			if(!m_IsInited) {Init();}
			//
		}

		#endregion Methods
	}
}
