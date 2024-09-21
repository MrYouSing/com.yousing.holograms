using System.IO;
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class HologramDevice
		:MonoBehaviour
	{
		#region Fields

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
			if(canvas!=null) {
			if(canvas.name=="RenderTexture.Temporary") {
				RenderTexture.ReleaseTemporary(canvas);
				canvas=null;
			}}
		}


		#endregion Unity Messages

		#region Methods

		public static int FindDisplay(Vector2Int size) {
			var tmp=Display.displays;Display it;
			for(int i=0,imax=tmp?.Length??0;i<imax;++i) {it=tmp[i];
				if(it.systemWidth==size.x
				&&it.systemHeight==size.y
				) {
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

		public virtual void Init() {
			if(m_IsInited) {return;}
			m_IsInited=true;
			//
			string fn=Path.Combine(
				Application.streamingAssetsPath//Path.GetDirectoryName(Application.dataPath)
				,"Settings",name+".json"
			);
			this.LoadSettings(fn);
			//
			if(resolution.sqrMagnitude==0) {
				resolution.Set(Screen.width,Screen.height);
			}
			if(display<0) {display=FindDisplay(resolution);}
			if(canvas==null) {
				canvas=RenderTexture.GetTemporary(resolution.x,resolution.y);
				canvas.name="RenderTexture.Temporary";
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

		#endregion Methods
	}
}
