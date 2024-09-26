using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace YouSingStudio.Holograms {
	public class QuiltTexture
		:MonoBehaviour
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
		[System.NonSerialized]protected Vector3 m_Args;
		[System.NonSerialized]protected Dictionary<string,Mesh> m_Meshes=new Dictionary<string,Mesh>();

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
				SetSource(source,source.name);
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
			if(value!=aspect) {
				aspect=value;
				//
				if(m_Args.sqrMagnitude!=0.0f) {RefreshMesh();}
			}
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
			m.name=GetName(from,to);
			m.vertices=v;
			m.uv=u;
			m.triangles=t;
			m.RecalculateBounds();
			return m;
		}

		public virtual void RefreshMesh() {
			VideoAspectRatio a=aspect;float z=size.z;
			//
			m_Id=0;aspect=GetAspect(m_Args.z,size.z);
			string key=GetName(m_Args,size);
			if(!m_Meshes.TryGetValue(key,out mesh)||mesh==null) {
				int[] ids=null;
				TextAsset ta=Resources.Load<TextAsset>($"Settings/{name}_{m_Args.ToQuilt()}"+
					(m_Args!=size?("_To_"+size.ToQuilt()):null));
				if(ta!=null) {
					ids=Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(ta.text);
				}else {
					float x=m_Args.x*m_Args.y,y=size.x*size.y;
					if(!Mathf.Approximately(x,y)) {
						m_Id=Mathf.FloorToInt((x-y)*0.5f);
						Debug.Log("Offset:"+m_Id);
					}
				}
				if(mesh==null) {
					if(ids!=null) {size.z=System.MathF.Sign(m_Args.z)*Mathf.Abs(z);}
					mesh=CreateMesh(m_Args,size,ids);m_Meshes[key]=mesh;
				}
			}
			//
			size.z=z;aspect=a;
		}

		public virtual void SetSource(Texture texture,string path) {
			if(texture!=null) {
				source=texture;
				//
				if(material==null) {material=new Material(Shader.Find("Sprites/Default"));}
				material.mainTexture=source;
				//
				Vector3 v=path.ParseQuilt();
				if(v!=m_Args) {
					m_Args=v;
					//
					RefreshMesh();
				}
			}
		}

		public virtual void SetSource(VideoPlayer player) {
			if(player!=null) {
				SetSource(player.texture,player.url);
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

		#endregion Methods
	}
}
