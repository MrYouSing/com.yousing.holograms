using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace YouSingStudio.Holograms {
	public class DepthRenderer
		:TextureRenderer
		,ISlider
	{
		#region Fields

		public static readonly int _MainTex=Shader.PropertyToID("_MainTex");
		public static readonly int _Depth=Shader.PropertyToID("_Depth");
		public static readonly int _Vector=Shader.PropertyToID("_Vector");
		public static Dictionary<string,Vector4> s_Vectors=new Dictionary<string,Vector4>();
		public static Dictionary<int,Mesh> s_Meshes=new Dictionary<int,Mesh>();

		[Header("Arguments")]
		public bool fixRange;
		public bool saveVector;
		[Header("Components")]
		public new Camera camera;
		public HologramDevice device;
		[Header("Rendering")]
		public Texture depth;
		public Vector2 range;
		public Vector2 factor;

		[System.NonSerialized]protected float m_Value;
		[System.NonSerialized]protected Vector3 m_Vector;
		[System.NonSerialized]protected Vector2Int m_Size;

		#endregion Fields

		#region Unity Messages

		protected override void Start() {
			if(!m_IsInited) {
				//
				bool b=saveVector;saveVector=true;
					UpdateVector();// Write start vector.
				saveVector=b;
				//
				if(!string.IsNullOrEmpty(m_Path)) {Play(m_Path);}
				else if(texture!=null) {Render(m_Path,texture,depth);}
			}
		}

		#endregion Unity Messages

		#region Methods

		public static int GetResolution(Vector2Int a,Vector2Int b) {
			if(a.sqrMagnitude==0) {a=b;}
			if(a.x==a.y) {return a.x;}
			else {return (int)((a.y<<16)&0xFFFF0000)|(a.x&0xFFFF);}
		}

		public static string ToDepth(string path) {
			string dir=Path.GetDirectoryName(path);
			string ext=Path.GetExtension(path);
			path=Path.GetFileNameWithoutExtension(path);
			if(path.EndsWith("_rgbd",UnityExtension.k_Comparison)) {
				return null;
			}else if(path.EndsWith("_rgb",UnityExtension.k_Comparison)) {
				path=path.Substring(0,path.Length-4);
			}
			return Path.Combine(dir,path+"_depth"+ext);
		}

		public override void PlayImage(string path) {
			TextureManager tm=TextureManager.instance;
			Texture rgb=tm.Get(m_Path=path);if(rgb==null) {return;}
			Texture d=tm.Get(ToDepth(m_Path));Render(m_Path,rgb,d);
		}

		public override void Render(Texture value)=>Render(m_Path,value,null);

		protected override void Init() {
			if(m_IsInited) {return;}
			m_IsInited=true;
			//
			this.LoadSettings(name);
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

		public override void Stop() {
			base.Stop();
			range=factor=Vector2.zero;
		}

		public virtual void Render(string path,Texture rgb,Texture d) {
			if(!m_IsInited) {Init();}
			//
			m_Path=path;texture=rgb;depth=d;m_Size=texture.GetSizeI();
			if(!string.IsNullOrEmpty(m_Path))layout=m_Path.To3DLayout();
			if(depth==null) {
				depth=texture;m_Size.x/=2;
				if(layout==Video3DLayout.No3D) {layout=Video3DLayout.SideBySide3D;}
				material.Set(_MainTex,layout.GetRect(0));
				material.Set(_Depth,layout.GetRect(1));
			}else {
				material.Set(_MainTex,Vector2.zero,Vector2.one);
				material.Set(_Depth,Vector2.zero,Vector2.one);
			}
			texture.wrapMode=TextureWrapMode.Clamp;material.SetTexture(_MainTex,texture);
			depth.wrapMode=TextureWrapMode.Clamp;material.SetTexture(_Depth,depth);
			if(renderer!=null) {renderer.enabled=true;}
			//
			if(s_Vectors.TryGetValue(m_Path,out var v)) {
				range.Set(v.x,v.y);factor.Set(v.z,v.w);
				if(range.sqrMagnitude==0.0f) {range=GetRange(depth,depth==texture);}
			}else {
				range=GetRange(depth,depth==texture);factor=Vector2.right;
			}
			UpdateVector();
			UpdateMesh();
			UpdateTransform();
		}

		public virtual void UpdateVector() {
			if(!m_IsInited) {Init();}
			//
			Vector4 v=new Vector4(range.x,range.y,factor.x,factor.y);
			if(v.sqrMagnitude==0.0f) {return;}
			if(material!=null) {material.SetVector(_Vector,v);}
			if(saveVector&&m_Path!=null) {s_Vectors[m_Path]=v;}
		}

		public virtual void UpdateMesh() {
			if(!m_IsInited) {Init();}
			//
			int r=GetResolution(resolution,m_Size);
			if(!s_Meshes.TryGetValue(r,out var m)) {
				m=GetMesh(r);s_Meshes[r]=m;
			}
			renderer.Set(m);
		}

		public override void UpdateTransform() {
			if(!m_IsInited) {Init();}
			//
			float f;if(camera!=null) {
				f=camera.WorldToDepth(root.position);
				f=camera.GetPlaneHeight(f);
				Transform p=root.parent;if(p!=null) {f/=p.lossyScale.x;}
				root.localScale=new Vector3(f,f,root.localScale.z);
			}
			f=device!=null?Mathf.Abs(device.ParseQuilt().z):((float)Screen.width/Screen.height);
			Vector2 v=UnityExtension.FitScale(new Vector2((float)m_Size.x/m_Size.y,1.0f),new Vector2(f,1.0f),aspect);
			m_Renderer.localScale=new Vector3(v.x,v.y,1.0f);
			m_Vector=new Vector3(v.x-f,1.0f-v.y,0.0f)*0.5f;
			if(false||false) {// TODO: Inside????
				m_Vector=Vector2.zero;
			}
			m_Value=-1.0f;Value=0.0f;
		}

		Vector2 ISlider.Range=>new Vector2(-1.0f,1.0f);

		public virtual float Value {
			get {
				bool b=isActiveAndEnabled;
				if(b) {b=!UnityExtension.Approximately(m_Vector.sqrMagnitude,0.0f);}
				return b?m_Value:float.NaN;
			}
			set {
				if(value==m_Value) {return;}
				m_Value=value;
				//
				if(m_Renderer!=null) {m_Renderer.localPosition=m_Vector*m_Value;}
			}
		}

		#endregion Methods
	}
}
