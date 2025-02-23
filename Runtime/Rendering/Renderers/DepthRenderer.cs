using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
		public static readonly int _Falloff=Shader.PropertyToID("_Falloff");
		public static readonly int _Offset=Shader.PropertyToID("_Offset");
		public static bool s_IsInited;
		public static Dictionary<string,Vector4> s_Vectors=new Dictionary<string,Vector4>();
		public static Dictionary<string,float> s_Sliders=new Dictionary<string,float>();
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
		[System.NonSerialized]protected Vector3 m_Point;
		[System.NonSerialized]protected Vector2 m_Vector;
		[System.NonSerialized]protected Vector2Int m_Size;

		#endregion Fields

		#region Unity Messages

		protected override void Start() {
			if(!m_IsInited) {
				if(m_Path=="*") {}
				else if(!string.IsNullOrEmpty(m_Path)) {Play(m_Path);}
				else if(texture!=null) {Render(m_Path,texture,depth);}
			}
		}

		protected virtual void OnDestroy() {
			if(s_IsInited) {ExitStatic();}
		}

		#endregion Unity Messages

		#region Methods

		public static void InitStatic() {
			if(s_IsInited) {return;}
			s_IsInited=true;
			//
			string tmp="$(SaveData)/DepthRenderer.json".GetFullPath();
			if(File.Exists(tmp)) {
				tmp=File.ReadAllText(tmp);if(string.IsNullOrEmpty(tmp)) {return;}
				JObject jo=JObject.Parse(tmp);JToken jt;
				jt=jo.SelectToken("Sliders");if(jt!=null) {JsonConvert.PopulateObject(jt.ToString(),s_Sliders);}
				jt=jo.SelectToken("Vectors");if(jt!=null) {JsonConvert.PopulateObject(jt.ToString(),s_Vectors);}
			}
		}

		public static void ExitStatic() {
			if(!s_IsInited) {return;}
			s_IsInited=false;
			//
			s_Sliders.TryGetValue("*",out var s);s_Vectors.TryGetValue("*",out var v);
			StringBuilder sb=new StringBuilder();int i;
			sb.AppendLine("{\"Sliders\":{");i=0;
			foreach(var it in s_Sliders) {
				var tmp=it.Value;if(tmp!=s) {
					if(i>0) {sb.AppendLine(",");}++i;
					sb.Append($"  \"{it.Key}\":{tmp}");
				}
			}
			if(i>0) {sb.AppendLine();}
			sb.AppendLine("},\"Vectors\":{");i=0;
			foreach(var it in s_Vectors) {
				var tmp=it.Value;if(tmp!=v) {
					if(i>0) {sb.AppendLine(",");}++i;
					sb.Append($"  \"{it.Key}\":{{\"x\":{tmp.x},\"y\":{tmp.y},\"z\":{tmp.z},\"w\":{tmp.w}}}");
				}
			}
			if(i>0) {sb.AppendLine();}sb.Append("}}");
			File.WriteAllText("$(SaveData)/DepthRenderer.json".GetFullPath(),sb.ToString());
		}

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
			// Find
			foreach(string it in UnityExtension.s_ImageExtensions) {
				ext=Path.Combine(dir,path+"_depth"+it);if(File.Exists(ext)) {return ext;}
			}return null;
		}

		public override void PlayImage(string path) {
			TextureManager tm=TextureManager.instance;
			Texture rgb=tm.Get(m_Path=path);if(rgb==null) {return;}
			Texture d=tm.Get(ToDepth(m_Path));Render(m_Path,rgb,d);
		}

		public override void PlayVideo(string path) {
			LoadVector(path);
			base.PlayVideo(path);
		}

		protected override void OnAspectUpdated() {
			if(depth==null&&texture==null) {return;}
			//
			UpdateMaterial();
			UpdateTransform();
		}

		public override void Render(Texture value)=>Render(m_Path,value,null);

		protected override void Init() {
			if(m_IsInited) {return;}
			m_IsInited=true;
			if(!s_IsInited) {InitStatic();}
			//
			this.LoadSettings(name);
			if(material==null) {material=new Material(Shader.Find("Unlit/Offset By Depth"));}
			if(renderer==null) {renderer=GetComponentInChildren<Renderer>();}
			if(device==null) {
				if(MonoApplication.s_Instance!=null) {device=MonoApplication.s_Instance.device;}
				else {device=FindAnyObjectByType<HologramDevice>();}
			}
			//
			renderer.sharedMaterial=material;m_Renderer=renderer.transform;
			if(device!=null&&resolution.x<0.0f&&resolution.y<0.0f) {// Resample
				Vector4 v=device.PreferredSize();
				v.x*=-resolution.x;v.y*=-resolution.y;
				resolution=new Vector2Int((int)(v.x/v.z),(int)(v.y/v.w));
			}
			if(root==null) {root=transform;}
			m_Point=root.localPosition;
			//
			if(float.IsNaN(factor.x)) {
				if(MonoApplication.s_Instance==null) {factor=Vector2.right;}
				else {Vector2 v=MonoApplication.s_Instance.depth;factor.Set(v.y-v.x,v.x);}
			}
			if(m_Path=="*") {
				s_Vectors[m_Path]=new Vector4(range.x,range.y,factor.x,factor.y);
			}
		}

		public virtual Vector2 GetRange(Texture texture,bool half) {
			if(!m_IsInited) {Init();}
			//
			Vector2 v;
			if(fixRange&&texture is Texture2D tex) {
				v=new Vector2(1.0f,0.0f);
				int x=0,y=0,w=tex.width,h=tex.height;
				Color[] c=tex.GetPixels();float f;
				switch(layout) {
					case Video3DLayout.SideBySide3D:
						for(;y<h;++y) {
						for(x=half?w/2:0;x<w;++x) {
							f=c[w*y+x].r;
							if(f>0.0f) {
								if(f<v.x){v.x=f;}
								else if(f>v.y) {v.y=f;}
							}
						}}
					break;
					case Video3DLayout.OverUnder3D:
						for(h=half?h/2:h;y<h;++y) {
						for(x=0;x<w;++x) {
							f=c[w*y+x].r;
							if(f>0.0f) {
								if(f<v.x){v.x=f;}
								else if(f>v.y) {v.y=f;}
							}
						}}
					break;
					default:
						v=Vector2.up;
					break;
				}
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
			}
			UpdateMaterial();
			if(renderer!=null) {renderer.enabled=true;}
			//
			LoadVector(m_Path);
			UpdateMesh();
			UpdateTransform();
			UpdateVector();
		}

		public virtual void LoadVector(string path) {
			if(!m_IsInited) {Init();}
			//
			s_Sliders.TryGetValue(path,out m_Value);m_Vector=Vector2.zero;
			if(s_Vectors.TryGetValue(path,out var v)||s_Vectors.TryGetValue("*",out v)) {
				range.Set(v.x,v.y);factor.Set(v.z,v.w);
				if(range.sqrMagnitude==0.0f) {range=GetRange(depth,depth==texture);}
			}else {
				range=GetRange(depth,depth==texture);factor=Vector2.right;
			}
		}

		public virtual void UpdateMaterial() {
			if(!m_IsInited) {Init();}
			//
			if(depth==texture) {
				material.Set(_MainTex,layout.GetRect(0));
				material.Set(_Depth,layout.GetRect(1));
			}else {
				material.Set(_MainTex,Vector2.zero,Vector2.one);
				material.Set(_Depth,Vector2.zero,Vector2.one);
			}
			texture.wrapMode=TextureWrapMode.Clamp;material.SetTexture(_MainTex,texture);
			depth.wrapMode=TextureWrapMode.Clamp;material.SetTexture(_Depth,depth);
		}

		public virtual void UpdateMesh() {
			if(!m_IsInited) {Init();}
			//
			Vector2Int v=new Vector2Int((int)resolution.x,(int)resolution.y);
			int r=GetResolution(v,m_Size);
			if(!s_Meshes.TryGetValue(r,out var m)) {
				m=GetMesh(r);s_Meshes[r]=m;
			}
			renderer.Set(m);
		}

		public virtual void UpdateTransform() {
			if(!m_IsInited) {Init();}
			//
			float f;if(camera!=null) {// Fit width and height.
				Vector3 t=m_Point;Transform p=root.parent;
				if(p!=null) {t=p.TransformPoint(t);}
				f=camera.WorldToDepth(t);
				f=camera.GetPlaneHeight(f);
				//
				float s;if(device!=null) {// Fit depth.
					s=device.HeightToDepth()*f;
					//
					t=m_Point+new Vector3(0.0f,0.0f,device.HeightToForward()*f);
					if(p!=null) {t=p.position+p.rotation*t;}root.position=t;
					//
					float x=f;f=camera.GetPlaneHeight(camera.WorldToDepth(t));
					if(material!=null) {
						t.x=(x-f)/f;t.y=device.size.z/(device.size.z-device.size.w);
						material.SetFloat(_Falloff,t.x/t.y);
					}
				}else {
					s=root.localScale.z;
				}
				//
				if(p!=null) {f/=p.lossyScale.x;}
				root.localScale=new Vector3(f,f,s);
			}
			//
			f=device!=null?Mathf.Abs(device.ParseQuilt().z):((float)Screen.width/Screen.height);
			float a=m_Path.ToAspect();if(float.IsNaN(a)) {a=(float)m_Size.x/m_Size.y;}
			Vector2 v=UnityExtension.FitScale(new Vector2(a,1.0f),new Vector2(f,1.0f),aspect);
			m_Renderer.localScale=new Vector3(f,1.0f,1.0f);
			if(v.x*v.y<=f) {// Inside Fit.
				m_Vector=Vector2.zero;
			}else if(material!=null) {// Outside Fit.
				Vector2 u=new Vector2(f,1.0f);
				Vector2 w=material.GetTextureScale(_MainTex);
				//
				m_Vector=(v-u)/u*0.5f;w*=u/v;
				material.SetTextureScale(_MainTex,w);
				material.SetTextureScale(_Depth,w);
			}
		}

		public virtual void UpdateVector() {
			if(!m_IsInited) {Init();}
			//
			Vector4 v=new Vector4(range.x,range.y,factor.x,factor.y);
			if(v.sqrMagnitude==0.0f) {return;}
			if(material!=null) {material.SetVector(_Vector,v);}
			if(saveVector&&!string.IsNullOrEmpty(m_Path)) {s_Vectors[m_Path]=v;}
			//
			float f=m_Value;m_Value=-1024.0f;Value=f;
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
				if(saveVector&&!string.IsNullOrEmpty(m_Path)) {s_Sliders[m_Path]=m_Value;}
				if(material!=null) {material.SetVector(_Offset,m_Vector*(1.0f-m_Value));}
			}
		}

		#endregion Methods
	}
}
