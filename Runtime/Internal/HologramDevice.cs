using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace YouSingStudio.Holograms {
	public class HologramDevice
		:MonoBehaviour
	{
		#region Fields

		public static GraphicsFormat s_GraphicsFormat=GraphicsFormat.R8G8B8A8_SRGB;

		public int display=-1;
		public Vector2Int resolution;
		public RenderTexture canvas;
		public Material material;
		[Header("Quilt")]
		public Texture quiltTexture;
		public Vector2Int quiltSize;

		[System.NonSerialized]public System.Action onPreRender=null;
		[System.NonSerialized]public System.Action onPostRender=null;
		[System.NonSerialized]protected bool m_IsInited;

		#endregion Fields

		#region Unity Messages

		protected virtual void LateUpdate() {
			Render();
		}

		protected virtual void OnDestroy() {
			if(canvas.IsTemporary()) {
				canvas.Free();
				canvas=null;
			}
		}


		#endregion Unity Messages

		#region Methods

		public static int FindDisplay(Vector2Int size) {
			var tmp=Private.ScreenManager.GetRects();RectInt it;
			for(int i=0,imax=tmp?.Length??0;i<imax;++i) {it=tmp[i];
				if(it.width==size.x&&it.height==size.y) {
#if UNITY_EDITOR
					if(Private.ScreenManager.IndexOf(Display.main)!=i) {// Sub
						i=Private.ScreenManager.SetupDisplay(i,it);
					}
#endif
					return i;
				}
			}
			return -1;
		}

		protected virtual void InternalRender() {
			if(material!=null) {
				var tmp=canvas.Begin();
					Graphics.Blit(quiltTexture,material);
				canvas.End(tmp);
			}else {
				Graphics.Blit(quiltTexture,canvas);
			}
		}

		/// <summary>
		/// <seealso cref="Input.mousePresent"/>
		/// </summary>
		public virtual bool IsPresent() {
			return true;
		}

		public virtual void Init() {
			if(m_IsInited) {return;}
			m_IsInited=true;
			//
			this.LoadSettings(name+".json");
			//
			if(resolution.sqrMagnitude==0) {
				resolution.Set(Screen.width,Screen.height);
			}
			if(display<0) {display=FindDisplay(resolution);}
			if(canvas==null) {
				canvas=RenderTexture.GetTemporary(resolution.x,resolution.y,0,s_GraphicsFormat);
				canvas.name=UnityExtension.s_TempTag;
			}
		}

		public virtual void Quilt(Texture texture,Vector2Int size) {
			if(!m_IsInited) {Init();}
			//
			if(size.sqrMagnitude!=0) {
				quiltSize=size;
			}
			if(texture!=quiltTexture) {
				quiltTexture=texture;
				//
				if(!isActiveAndEnabled) {Render();}
			}
		}

		public virtual void Render() {
			if(!m_IsInited) {Init();}
			//
			onPreRender?.Invoke();
			InternalRender();
			onPostRender?.Invoke();
		}

		/// <summary>
		/// <seealso cref="UnityExtension.ParseQuilt(string)"/>
		/// </summary>
		public virtual Vector3 ParseQuilt() {
			if(!m_IsInited) {Init();}
			//
			return new Vector3(quiltSize.x,quiltSize.y,
				(float)quiltTexture.width/quiltSize.x/quiltTexture.height*quiltSize.y);
		}

		public virtual Vector4 PreferredSize() {
			if(!m_IsInited) {Init();}
			//
			return new Vector4(quiltTexture.width,quiltTexture.height,quiltSize.x,quiltSize.y);
		}

		#endregion Methods
	}
}
