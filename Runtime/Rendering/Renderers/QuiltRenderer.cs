using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace YouSingStudio.Holograms {
	public class QuiltRenderer
		:MonoBehaviour
		,ISlider
	{
		#region Fields

		public const float k_AspectError=0.005f;

		public Texture source;
		public Material material;
		public Mesh mesh;
		public RenderTexture destination;
		public Vector3 size;
		public VideoAspectRatio aspect;

		[System.NonSerialized]protected int m_Id;
		[System.NonSerialized]protected float m_Value;
		[System.NonSerialized]protected Vector2 m_Vector;
		[System.NonSerialized]protected Vector3 m_Args;
		[System.NonSerialized]protected Dictionary<string,Mesh> m_Meshes=new Dictionary<string,Mesh>();
		[System.NonSerialized]protected Dictionary<string,Vector2> m_Vectors=new Dictionary<string,Vector2>();

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(m_Args.sqrMagnitude!=0.0f) {return;}
			//
			var device=GetComponent<HologramDevice>();
			if(device!=null) {
				SetDestination(device);
			}
			//
			var video=GetComponent<VideoPlayer>();
			if(video!=null) {
				SetSource(video);
			}else if(source!=null) {
				SetSource(source,source.name.ParseQuilt());
			}
		}

		protected virtual void Update() {
			RenderDestination();
		}

		#endregion Unity Messages

		#region Methods
#if UNITY_EDITOR
		[ContextMenu("Save Mesh")]
		protected virtual void SaveMesh() {
			if(mesh!=null) {
				UnityEditor.AssetDatabase.CreateAsset(mesh,$"Assets/{mesh.name}.asset");
			}
		}
#endif
		public virtual string GetName(Vector3 from,Vector3 to) {
			if(from==to) {return $"{name}_{from.ToQuilt()}";}
			else {return $"{name}_{aspect}_{from.ToQuilt()}_To_{to.ToQuilt()}";}
		}

		public virtual VideoAspectRatio GetAspect(float from,float to) {
			float f=Mathf.Abs(from)-Mathf.Abs(to);
			if(UnityExtension.Approximately(f,0.0f,k_AspectError)) {
				return VideoAspectRatio.Stretch;
			}else {
				switch(aspect) {
					case VideoAspectRatio.FitHorizontally:
					return f>0.0f?VideoAspectRatio.FitInside:VideoAspectRatio.FitOutside;
					case VideoAspectRatio.FitVertically:
					return f<0.0f?VideoAspectRatio.FitInside:VideoAspectRatio.FitOutside;
					default:return aspect;
				}
			}
		}

		public virtual void SetAspect(VideoAspectRatio value) {
			if(!isActiveAndEnabled) {aspect=value;return;}
			//
			if(value!=aspect) {
				aspect=value;
				//
				if(m_Args.sqrMagnitude!=0.0f) {RefreshMesh();return;}
			}
			UpdateSlider();
		}

		public virtual Vector4 GetQuad(
			int x,int y,float dw,float dh,
			int mode,float from,float to
		) {
			from=Mathf.Abs(from);to=Mathf.Abs(to);
			//
			if(mode!=0&&!UnityExtension.Approximately(from,to,k_AspectError)) {
				Vector4 v=new Vector4((x+0.5f)*dw,(y+0.5f)*dh,dw*0.5f,dh*0.5f);
				float f=Mathf.Min(from,to)/Mathf.Max(from,to);
				if((from-to)*mode>0) {v.z*=f;}
				else {v.w*=f;}
				return new Vector4(v.x-v.z,v.y-v.w,v.x+v.z,v.y+v.w);
			}else {
				return new Vector4(x*dw,y*dh,(x+1)*dw,(y+1)*dh);
			}
		}

		public virtual Vector4 GetUV(Vector3 from,Vector3 to,int id) {
			//
			int w,h;bool b=from.z*to.z<0.0f;
			if(b) {// For looking glass media.
				w=(int)to.x;h=(int)to.y;
				id=w*(h-1-id/w)+id%w;
			}
			//
			id+=m_Id;w=(int)from.x;h=(int)from.y;
			if(id<0||id>=w*h) {return Vector4.zero;}
			int x=id%w,y=id/w;
			float dw=1.0f/w,dh=1.0f/h;
			//
			return GetQuad(x,y,dw,dh,aspect==VideoAspectRatio.FitOutside?1:0,from.z,to.z);
		}

		public virtual Mesh CreateMesh(Vector3 from,Vector3 to,params int[] ids) {
			int w=(int)to.x,h=(int)to.y,i=0,j=0,x,y;
			float dw=1.0f/w,dh=1.0f/h;
			//
			Mesh m=new Mesh();Vector4 quad;
			Vector2[] u=new Vector2[w*h*4];
			Vector3[] v=new Vector3[u.Length];
			int[] t=new int[w*h*6];
			for(y=0;y<h;++y) {
			for(x=0;x<w;++x,i+=4) {
				quad=GetQuad(x,y,dw,dh,aspect==VideoAspectRatio.FitInside?-1:0,from.z,to.z);
				v[i+0]=new Vector3(quad.x,quad.y,0.0f);
				v[i+1]=new Vector3(quad.z,quad.y,0.0f);
				v[i+2]=new Vector3(quad.x,quad.w,0.0f);
				v[i+3]=new Vector3(quad.z,quad.w,0.0f);
				quad=GetUV(from,to,ids!=null?ids[i/4]:(i/4));
				u[i+0]=new Vector2(quad.x,quad.y);
				u[i+1]=new Vector2(quad.z,quad.y);
				u[i+2]=new Vector2(quad.x,quad.w);
				u[i+3]=new Vector2(quad.z,quad.w);
				//
				if(quad.sqrMagnitude==0.0f) {j+=6;continue;}
				t[j]=i+0;++j;
				t[j]=i+2;++j;
				t[j]=i+3;++j;
				t[j]=i+0;++j;
				t[j]=i+3;++j;
				t[j]=i+1;++j;
			}}
			//
			quad=GetUV(from,to,0);m_Vector=new Vector2(quad.z-quad.x,quad.w-quad.y);
			m_Vector=m_Vector-new Vector2(1.0f/from.x,1.0f/from.y);m_Vector*=0.5f;
			//
			m.vertices=v;
			m.uv=u;
			m.triangles=t;
			m.RecalculateBounds();
			return m;
		}

		public virtual int[] CreateIndexes(Vector3 from,Vector3 to) {
			int x=(int)(from.x*from.y),y=(int)(to.x*to.y);
			int[] ids=new int[y];float p=0.0f,d=(x-1.0f)/(y-1.0f);
			if(from.z*to.z>=0.0f) {
				for(x=0;x<y;++x) {ids[x]=(int)p;p+=d;}
			}else {
				int xmax=(int)to.x,ymax=(int)to.y;
				for(y=ymax-1;y>=0;--y) {
				for(x=0;x<xmax;++x) {
					ids[y*xmax+x]=(int)p;p+=d;
				}}
			}
			return ids;
		}

		public virtual void RefreshMesh() {
			VideoAspectRatio a=aspect;float z=size.z;
			//
			m_Id=0;aspect=GetAspect(m_Args.z,size.z);
			string key=GetName(m_Args,size);
			if(!m_Meshes.TryGetValue(key,out mesh)||mesh==null) {
				int[] ids=null;int x=(int)(m_Args.x*m_Args.y),y=(int)(size.x*size.y);
				Vector3 u=new Vector3(x,1.0f,System.MathF.Sign(m_Args.z));
				Vector3 v=new Vector3(y,1.0f,System.MathF.Sign(size.z));
				TextAsset ta=Resources.Load<TextAsset>($"Settings/{name}_{u.ToQuilt()}"+
					(u!=v?("_To_"+v.ToQuilt()):null));
				if(ta!=null) {
					ids=Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(ta.text);
				}else {
					if(x==y) {
					}else if((float)x/y>=1.7f) {// (11*6)/(8*5)=1.65
						ids=CreateIndexes(m_Args,size);
					}else {
						m_Id=Mathf.FloorToInt((x-y)*0.5f);
						Debug.Log("Offset:"+m_Id);
					}
				}
				if(mesh==null) {
					if(ids!=null) {size.z=System.MathF.Sign(m_Args.z)*Mathf.Abs(z);}
					mesh=CreateMesh(m_Args,size,ids);mesh.name=key;
					m_Meshes[key]=mesh;m_Vectors[key]=m_Vector;
				}
			}
			//
			size.z=z;aspect=a;
			UpdateSlider();
		}

		public virtual void SetSource(Texture texture,Vector3 args) {
			if(texture!=null) {
				source=texture;
				//
				if(material==null) {material=UnityExtension.GetUnlit();}
				material.mainTexture=source;
				if(float.IsNaN(args.z)||(args.z<0.0f&&args.TwoPieces())) {
					args.z=(source.width/args.x)/(source.height/args.y);
				}
				//
				if(args!=m_Args) {
					m_Args=args;
					//
					RefreshMesh();
				}
			}
		}

		public virtual void SetSource(VideoPlayer player) {
			if(player!=null) {
				SetSource(player.texture,player.url.ParseQuilt());
			}
		}

		public virtual void SetDestination(HologramDevice device) {
			if(device!=null) {device.Init();
				destination=device.quiltTexture as RenderTexture;
				if(destination!=null) {size=device.ParseQuilt();}
			}
		}

		public virtual void RenderDestination() {
			if(mesh!=null&&destination!=null) {
#if false
				Camera cam=UnityExtension.GetCameraHelper();
				cam.targetTexture=destination;
				Graphics.DrawMesh(mesh,Matrix4x4.identity,material,cam.gameObject.layer,cam);
				cam.Render();
#else
				GLRenderer gl=UnityExtension.GetGLRenderer();
				gl.Set(mesh);
				gl.material=material;
				gl.Render(destination);
#endif
			}
		}

		public virtual void UpdateSlider() {
			if(mesh==null||!m_Vectors.TryGetValue(mesh.name,out m_Vector)) {
				m_Vector=Vector2.zero;
			}
			m_Value=-1024.0f;Value=0.0f;
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
				if(material!=null) {material.mainTextureOffset=m_Vector*m_Value;}
			}
		}

		#endregion Methods
	}
}
