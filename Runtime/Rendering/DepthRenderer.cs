using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace YouSingStudio.Holograms {
	public class DepthRenderer
		:MonoBehaviour
	{
		#region Fields

		public static readonly int _MainTex=Shader.PropertyToID("_MainTex");
		public static readonly int _Depth=Shader.PropertyToID("_Depth");
		public static readonly int _Vector=Shader.PropertyToID("_Vector");
		public static Dictionary<string,Vector4> s_Vectors=new Dictionary<string,Vector4>();
		public static Dictionary<int,Mesh> s_Meshes=new Dictionary<int,Mesh>();

		[Header("Arguments")]
		[SerializeField]protected string m_Path;
		public Vector2Int resolution=Vector2Int.zero;
		public Video3DLayout layout;
		public bool fixRange;
		public bool saveVector;
		[Header("Components")]
		public HologramDevice device;
		public Transform root;
		public new Renderer renderer;
		[Header("Rendering")]
		public Material material;
		public Texture color;
		public Texture depth;
		public Vector2 range;
		public Vector2 factor;

		[System.NonSerialized]protected bool m_IsInited;
		[System.NonSerialized]protected Transform m_Renderer;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(!m_IsInited) {
				//
				bool b=saveVector;saveVector=true;
					UpdateVector();// Write start vector.
				saveVector=b;
				//
				if(color!=null) {Play(m_Path,color,depth);}
				else if(!string.IsNullOrEmpty(m_Path)) {Play(m_Path);}
			}
		}

		#endregion Unity Messages

		#region Methods

		public static int GetResolution(Vector2Int a,Vector2Int b) {
			if(a.sqrMagnitude==0) {a=b;}
			if(a.x==a.y) {return a.x;}
			else {return (int)((a.y<<16)&0xFFFF0000)|(a.x&0xFFFF);}
		}

		public virtual void Play(string path) {
			TextureManager tm=TextureManager.instance;
			Texture rgb=tm.Get(m_Path=path);if(rgb==null) {Stop();return;}
			path=Path.Combine(Path.GetDirectoryName(m_Path),Path.
				GetFileNameWithoutExtension(m_Path)+"_Depth"+Path.GetExtension(m_Path));
			Texture d=tm.Get(path);Play(m_Path,rgb,d);
		}

		protected virtual void Init() {
			if(m_IsInited) {return;}
			m_IsInited=true;
			//
			this.LoadSettings(name+".json");
			if(material==null) {material=new Material(Shader.Find("Unlit/Offset By Depth"));}
			if(renderer==null) {renderer=GetComponentInChildren<Renderer>();}
			renderer.sharedMaterial=material;m_Renderer=renderer.transform;
			if(device!=null&&resolution.sqrMagnitude==0) {
				Vector4 v=device.PreferredSize();
				resolution=new Vector2Int((int)(v.x/v.z),(int)(v.y/v.w));
			}
			if(root==null) {root=transform;}
		}

		public virtual Vector2 GetRange(Texture texture,bool half) {
			if(!m_IsInited) {Init();}
			//
			Vector2 v;
			if(fixRange&&texture is Texture2D tex) {
				v=new Vector2(1.0f,0.0f);
				int x=0,y=0,w=tex.width,h=tex.height;
				Color[] c=tex.GetPixels();float f;
				for(;y<h;++y) {
				for(x=half?w/2:0;x<w;++x) {
					f=c[w*y+x].r;
					if(f>0.0f) {
						if(f<v.x){v.x=f;}
						else if(f>v.y) {v.y=f;}
					}
				}}
			}else {
				v=Vector2.up;
			}
			return v;
		}

		public virtual Mesh GetMesh(int r) {
			if(!m_IsInited) {Init();}
			//
			Mesh m=null;
			int h=(r>>16)&0xFFFF,w=r&0xFFFF;
			if(h==0) {m=Resources.Load<Mesh>("Models/Plane_"+w);h=w;}
			else {m=Resources.Load<Mesh>("Models/Plane_"+w+"_"+h);}
			if(m==null) {m=m.CreatePlane(w,h);m.name="Plane_"+w+(w!=h?"_"+h:null);}
			return m;
		}

		public virtual void Stop() {
			if(!m_IsInited) {Init();}
			//
			if(renderer!=null) {renderer.enabled=false;}
			range=factor=Vector2.zero;
		}

		public virtual void Play(string path,Texture rgb,Texture d) {
			if(!m_IsInited) {Init();}
			m_Path=path;color=rgb;depth=d;
			//
			Vector2Int size=color.GetSizeI();
			if(depth==null) {
				depth=color;size.x/=2;
				material.Set(_MainTex,layout.GetRect(0));
				material.Set(_Depth,layout.GetRect(1));
			}else {
				material.Set(_MainTex,Vector2.zero,Vector2.one);
				material.Set(_Depth,Vector2.zero,Vector2.one);
			}
			color.wrapMode=TextureWrapMode.Clamp;material.SetTexture(_MainTex,color);
			depth.wrapMode=TextureWrapMode.Clamp;material.SetTexture(_Depth,depth);
			if(renderer!=null) {renderer.enabled=true;}
			//
			if(s_Vectors.TryGetValue(m_Path,out var v)) {
				range.Set(v.x,v.y);factor.Set(v.z,v.w);
				if(range.sqrMagnitude==0.0f) {range=GetRange(depth,depth==color);}
			}else {
				range=GetRange(depth,depth==color);factor=Vector2.right;
			}
			UpdateVector();
			//
			int r=GetResolution(resolution,size);
			if(!s_Meshes.TryGetValue(r,out var m)) {
				m=GetMesh(r);s_Meshes[r]=m;
			}
			renderer.Set(m);
			m_Renderer.localScale=new Vector3((float)size.x/size.y,1.0f,1.0f);
		}

		public virtual void UpdateVector() {
			if(!m_IsInited) {Init();}
			//
			Vector4 v=new Vector4(range.x,range.y,factor.x,factor.y);
			if(v.sqrMagnitude==0.0f) {return;}
			if(material!=null) {material.SetVector(_Vector,v);}
			if(saveVector&&m_Path!=null) {s_Vectors[m_Path]=v;}
		}

		#endregion Methods
	}
}
