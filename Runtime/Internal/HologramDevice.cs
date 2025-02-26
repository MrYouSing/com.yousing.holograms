/* <!-- Macro.Define bFixPatch=
true
 Macro.End --> */
/* <!-- Macro.Define Width=
thiz.size.x
 Macro.End --> */
/* <!-- Macro.Define Height=
thiz.size.y
 Macro.End --> */
/* <!-- Macro.Define Depth=
(thiz.size.z-thiz.size.w)
 Macro.End --> */
/* <!-- Macro.Define Forward=
thiz.size.z
 Macro.End --> */
/* <!-- Macro.Define Back=
thiz.size.w
 Macro.End --> */
/* <!-- Macro.Define SizeToSize
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float {0}To{1}(this HologramDevice thiz)=>$({1})/$({0});
 Macro.End --> */

/* <!-- Macro.Call SizeToSize
Height,Width,
Height,Depth,
Height,Forward,
 Macro.End --> */
/* <!-- Macro.Patch
,UnityExtension
 Macro.End --> */
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace YouSingStudio.Holograms {
	public static partial class UnityExtension {
// <!-- Macro.Patch UnityExtension
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float HeightToWidth(this HologramDevice thiz)=>thiz.size.x/thiz.size.y;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float HeightToDepth(this HologramDevice thiz)=>(thiz.size.z-thiz.size.w)/thiz.size.y;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float HeightToForward(this HologramDevice thiz)=>thiz.size.z/thiz.size.y;
// Macro.Patch -->
	}
	public class HologramDevice
		:MonoBehaviour
	{
		#region Fields

		public static GraphicsFormat s_GraphicsFormat=GraphicsFormat.R8G8B8A8_SRGB;

		public int display=-1;
		public Vector2Int resolution;
		[Tooltip("The rendering arguments.\nx:Aspect\ny:ViewCone")]
		public Vector4 lens;
		[Tooltip("The size in real world.\nxy:Screen\nz:In Depth\nw:Out Depth")]
		public Vector4 size;
		public RenderTexture canvas;
		public Material material;
		[Header("Quilt")]
		public Vector2Int quiltResolution;
		public Vector2Int quiltSize;
		public Texture quiltTexture;

		[System.NonSerialized]public System.Action onPreRender=null;
		[System.NonSerialized]public System.Action onPostRender=null;
		[System.NonSerialized]protected bool m_IsInited;

		#endregion Fields

		#region Unity Messages

		protected virtual void OnEnable() {
			SetRenderEvent(true);
		}

		protected virtual void OnDisable() {
			SetRenderEvent(false);
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
			for(int i=(tmp?.Length??0)-1;i>=0;--i) {it=tmp[i];
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

		public static void CheckRenderTexture<T>(ref T texture,Vector2Int size) where T:Texture {
			if(texture==null&&size.sqrMagnitude!=0) {
				RenderTexture rt=RenderTexture.GetTemporary(size.x,size.y,0,s_GraphicsFormat);
				rt.name=UnityExtension.s_TempTag;texture=rt as T;
			}
		}

		// https://docs.unity3d.com/Manual/execution-order.html
		protected virtual void SetRenderEvent(bool value) {
#if UNITY_EDITOR
			Application.onBeforeRender-=OnRender;m_RenderCount=-1;
			if(m_IsInited&&value) {Application.onBeforeRender+=OnRender;}
		}

		[System.NonSerialized]protected int m_RenderCount;

		protected virtual void OnRender() {
			int n=Time.frameCount;
			if(n!=m_RenderCount) {
				m_RenderCount=n;
				//
				Render();
			}
		}
#else
			++m_RenderId;
			if(m_IsInited&&value) {StartCoroutine(RenderCoroutine());}
		}

		[System.NonSerialized]protected int m_RenderId;
		[System.NonSerialized]protected WaitForEndOfFrame m_RenderWait;

		protected virtual System.Collections.IEnumerator RenderCoroutine() {
			int id=++m_RenderId;
			if(m_RenderWait==null) {m_RenderWait=new WaitForEndOfFrame();}
			while(true) {
				yield return m_RenderWait;
				if(m_RenderId!=id) {yield break;}
				Render();
			}
		}
#endif
		protected virtual void InternalRender() {
			if(canvas==null) {
				// TODO: Raw Mode.
			}else if(material!=null) {
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

		/// <summary>
		/// <seealso cref="ThreadPriority"/>
		/// </summary>
		public virtual int GetPriority() {
			return 0;
		}

		public virtual void FromJson(string json) {
			if(string.IsNullOrEmpty(json)) {return;}
			Newtonsoft.Json.JsonConvert.PopulateObject(json,this);
			//
			if(!isActiveAndEnabled) {Render();}
		}

		public virtual string ToJson() {
			return null;
		}

		public virtual void Init() {
			if(m_IsInited) {return;}
			m_IsInited=true;
			//
			this.LoadSettings(name);
			//
			if(resolution.sqrMagnitude==0) {
				resolution.Set(Screen.width,Screen.height);
			}
			if(display<0) {display=FindDisplay(resolution);}
			CheckRenderTexture(ref canvas,resolution);
			CheckRenderTexture(ref quiltTexture,quiltResolution);
			if(lens.x==0.0f) {lens.x=ParseQuilt().z;}
			SetRenderEvent(true);
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
		/// <seealso cref="ScreenCapture.CaptureScreenshot(string)"/>
		/// </summary>
		public virtual void Screenshot(string path,int mask=-1) {
			string dir=Path.GetDirectoryName(path);
			if(!Directory.Exists(dir)) {Directory.CreateDirectory(dir);}
			//
			Texture2D tmp=null;bool del=false;
			if((mask&0x2)!=0&&quiltTexture!=null) {
				//
				tmp=quiltTexture as Texture2D;
				if(tmp!=null) {tmp=Instantiate(tmp);del=true;}
				else {tmp=(quiltTexture as RenderTexture).ToTexture2D(tmp);}
				//
				Vector3 v=ParseQuilt();if(v.z>0.0f) {
					if((mask&0x4)!=0) {tmp.SaveFile($"{path}_quilt.png");}
					ImageConverter.FlipQuiltY(tmp,(int)v.y);
				}else {
					v.z*=-1.0f;
				}
				tmp.SaveFile($"{path}_qs{v.x}x{v.y}a{v.z.ToString("0.00")}.png");
			}
			if((mask&0x1)!=0&&canvas!=null) {
				tmp=canvas.ToTexture2D(tmp);
				tmp.SaveFile($"{path}_raw.png");
			}
			if(del) {Texture2D.Destroy(tmp);}
		}

		/// <summary>
		/// <seealso cref="UnityExtension.ParseQuilt(string)"/>
		/// </summary>
		public virtual Vector3 ParseQuilt() {
			if(!m_IsInited) {Init();}
			//
			float f;if(lens.x!=0.0f) {f=lens.x;}
			else if(canvas!=null) {f=(float)canvas.width/canvas.height;}
			else {f=(float)resolution.x/resolution.y;}
			return new Vector3(quiltSize.x,quiltSize.y,f);
		}

		public virtual Vector4 PreferredSize() {
			if(!m_IsInited) {Init();}
			//
			return new Vector4(quiltTexture.width,quiltTexture.height,quiltSize.x,quiltSize.y);
		}

		#endregion Methods
	}
}
