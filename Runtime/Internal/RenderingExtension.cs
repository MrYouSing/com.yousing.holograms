/* <!-- Macro.Copy File
:Packages/com.yousing.io/Runtime/APIs/TextureAPI.cs,144~153,169,345~354,359~374,479~503,516~529
 Macro.End --> */
/* <!-- Macro.Replace
TempTexture2D,GetTemp2D();}if(false){}//
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
#if (UNITY_EDITOR_WIN||UNITY_STANDALONE_WIN)&&!NET_STANDARD
#define DOTNET_GDI_PLUS
#endif
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Video;
#if DOTNET_GDI_PLUS
using System.Drawing;
using System.Drawing.Imaging;
using Color=UnityEngine.Color;
using Graphics=UnityEngine.Graphics;
#endif

namespace YouSingStudio.Holograms {
	public static partial class RenderingExtension {
// <!-- Macro.Patch AutoGen
		public static GraphicsFormat ToStandard(this GraphicsFormat thiz) {
			string key=thiz.ToString();var com=System.StringComparison.OrdinalIgnoreCase;
			if(key.IndexOf("_SRGB",com)>=0) {return GraphicsFormat.R8G8B8A8_SRGB;}
			if(key.IndexOf("_UNorm",com)>=0) {return GraphicsFormat.R8G8B8A8_UNorm;}
			if(key.IndexOf("_SNorm",com)>=0) {return GraphicsFormat.R8G8B8A8_SNorm;}
			if(key.IndexOf("_UInt",com)>=0) {return GraphicsFormat.R8G8B8A8_UInt;}
			if(key.IndexOf("_SInt",com)>=0) {return GraphicsFormat.R8G8B8A8_SInt;}
			return thiz;
		}

		public static int[] s_Bitmap;
		public static void LoadBitmap<T>(this Texture2D thiz,T[] bitmap,int width,int height,TextureFormat format=TextureFormat.RGBA32) {
			if(thiz!=null&&bitmap!=null) {
				if(thiz.width!=width||thiz.height!=height||thiz.format!=format) {
					thiz.Reinitialize(width,height,format,thiz.mipmapCount>1);
				}
				System.IntPtr ptr=Marshal.UnsafeAddrOfPinnedArrayElement(bitmap,0);
				thiz.LoadRawTextureData(ptr,width*height*4);thiz.Apply();
			}
		}

#if DOTNET_GDI_PLUS
		// https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/about-gdi-managed-code
		public static void LoadBitmap(this Texture2D thiz,Bitmap bitmap,RectInt rect) {
			if(thiz!=null&&bitmap!=null) {
				int x,y,w,h;if(rect.size.sqrMagnitude==0) {x=y=0;w=bitmap.Width;h=bitmap.Height;}
				else {x=rect.x;y=rect.y;w=rect.width;h=rect.height;}
				int s=w*h;if((s_Bitmap?.Length??0)<s) {s_Bitmap=new int[s];}
				var bmd=bitmap.LockBits(new Rectangle(x,y,w,h),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
					System.IntPtr ptr=bmd.Scan0;int o=bmd.Stride;
					for(int i=h-1;i>=0;--i,ptr+=o) {Marshal.Copy(ptr,s_Bitmap,i*w,w);}
				bitmap.UnlockBits(bmd);
				thiz.LoadBitmap(s_Bitmap,w,h,TextureFormat.BGRA32);
			}
		}
#endif
		
		public static RenderTexture Begin(this RenderTexture thiz) {
			RenderTexture rt=RenderTexture.active;
				RenderTexture.active=thiz;return rt;
		}

		public static void End(this RenderTexture thiz,RenderTexture rt) {
			RenderTexture.active=rt;
		}

		public static void Clear(this RenderTexture thiz) {
			if(thiz==null) {return;}
			//
			var rt=thiz.Begin();
				GL.Clear(true,true,Color.clear);
			thiz.End(rt);
		}

		public static void CopyFrom(this RenderTexture thiz,Texture texture) {
			if(thiz==null||texture==null||thiz==texture) {return;}
			//
			var rt=thiz.Begin();
				Graphics.Blit(texture,thiz);
			thiz.End(rt);
		}

		public static Texture2D ToTexture2D(this RenderTexture thiz,Texture2D texture=null,bool apply=true) {
			if(thiz!=null) {
				Vector2Int s=thiz.GetSizeI();
				if(texture==null) {texture=GetTemp2D();}if(false){}//(s.x,s.y);}
				else if(texture.GetSizeI()!=s) {texture.Reinitialize(s.x,s.y);}
				//
				var rt=thiz.Begin();
					texture.ReadPixels(new Rect(0,0,s.x,s.y),0,0,false);
					if(apply) {texture.Apply();}
				thiz.End(rt);
			}
			return texture;
		}

// Macro.Patch -->
		#region Fields

		public static Texture2D s_Temp2D;
		public static Material s_Unlit;
		public static Camera s_CameraHelper;
		public static GLRenderer s_GLRenderer;
		public static FieldInfo s_RenderUIField;
		public static object s_RenderUIEvent;
		public static int s_RenderPipelines;
		public static int s_RenderPipeline=-1;
		public static RenderPipelineAsset[] s_RenderPipelineAssets;

		#endregion Fields

		#region Methods

		//

		public static void Free(this Texture thiz) {
			if(thiz!=null) {
				if(thiz is RenderTexture rt) {RenderTexture.ReleaseTemporary(rt);}
				else {Object.Destroy(thiz);}
			}
		}

		public static void Free<T>(ref T thiz) where T:Texture {
			thiz.Free();thiz=null;
		}

		public static Vector2Int GetSizeI(this Texture thiz) {
			if(thiz!=null) {return new Vector2Int(thiz.width,thiz.height);}
			else {return Vector2Int.zero;}
		}

		public static Texture2D NewTexture2D(int width,int height,bool linear) {
			return new Texture2D(width,height,TextureFormat.RGBA32,false,linear) ;
		}

		public static Texture2D NewTexture2D(int width,int height,GraphicsFormat format) {
			return new Texture2D(width,height,format,0,TextureCreationFlags.None) ;
		}

		public static Texture2D NewTexture2D(int width,int height)=>NewTexture2D(width,height,false);

		public static void SaveFile(this Texture2D thiz,string path) {
			if(thiz!=null&&!string.IsNullOrEmpty(path)) {
				byte[] tmp=null;
				if(path.EndsWith(".png",UnityExtension.k_Comparison)) {tmp=thiz.EncodeToPNG();}
				else if(path.EndsWith(".jpg",UnityExtension.k_Comparison)) {tmp=thiz.EncodeToJPG();}
				else if(path.EndsWith(".tga",UnityExtension.k_Comparison)) {tmp=thiz.EncodeToTGA();}
				else if(path.EndsWith(".exr",UnityExtension.k_Comparison)) {tmp=thiz.EncodeToEXR();}
				File.WriteAllBytes(path,tmp);
			}
		}

		public static bool IsNullOrEmpty(this RenderTexture thiz) {
			if(thiz!=null) {
				int w=thiz.width,h=thiz.height;var tex=GetTemp2D();
				if(tex.width!=w||tex.height!=h) {tex.Reinitialize(w,h);}
				var tmp=thiz.Begin();
					tex.ReadPixels(new Rect(0,0,w,h),0,0,false);tex.Apply();
				thiz.End(tmp);
				Color[] colors=tex.GetPixels();Color it;
				for(int i=0,imax=colors?.Length??0;i<imax;++i) {
					it=colors[i];if(it.r>0.0f||it.g>0.0f||it.b>0.0f) {
						return false;
					}
				}
			}
			return true;
		}

		public static void Free(this RenderTexture thiz) {
			if(thiz!=null) {RenderTexture.ReleaseTemporary(thiz);}
		}

		public static void Blit(this RenderTexture thiz,Texture texture,int cols,int rows) {
			if(thiz==null||texture==null) {return;}
			//
			var tmp=thiz.Begin();
			GL.PushMatrix();
				int x,y,w=thiz.width,h=thiz.height;float dw=w/(float)cols,dh=h/(float)rows;
				GL.LoadPixelMatrix(0.0f,w,h,0.0f);for(y=0;y<rows;++y) {for(x=0;x<cols;++x) {
					Graphics.DrawTexture(new Rect(x*dw,y*dh,dw,dh),texture);
				}}
			GL.PopMatrix();
			thiz.End(tmp);
		}

		public static RenderTexture GetTexture(this VideoPlayer thiz,bool resize=false) {
			RenderTexture rt=null;
			if(thiz!=null) {
				rt=thiz.targetTexture;Texture tex=thiz.texture;
				if(rt==null) {
					rt=tex as RenderTexture;
				}else if(tex!=null) {Vector2Int a=tex.GetSizeI(),b=rt.GetSizeI();if(a!=b){
					if(resize) {
						if(rt.IsTemporary()) {rt.Free();}
						rt=RenderTexture.GetTemporary(a.x,a.y);rt.name=UnityExtension.s_TempTag;
						thiz.targetTexture=rt;
					}else {
						Debug.Log($"({a.x}x{a.y}) to ({b.x}x{b.y}).");
					}
				}}
			}
			return rt;
		}

		public static void SetResolution(this VideoPlayer thiz,Vector2Int value) {
			if(thiz==null) {return;}
			//
			if(value.sqrMagnitude>0) {
				if(thiz.targetTexture==null) {thiz.targetTexture=RenderTexture.GetTemporary(value.x,value.y);}
				else {Debug.Log("Exists a target texture.");}
				thiz.renderMode=VideoRenderMode.RenderTexture;
			}else {
				thiz.targetTexture=null;
				thiz.renderMode=VideoRenderMode.APIOnly;
			}
		}

		public static void Set(this Material thiz,int id,Vector2 offset,Vector2 scale) {
			if(thiz!=null) {
				thiz.SetTextureOffset(id,offset);
				thiz.SetTextureScale(id,scale);
			}
		}

		public static void Set(this Material thiz,int id,Rect rect) {
			if(thiz!=null) {
				thiz.SetTextureOffset(id,rect.position);
				thiz.SetTextureScale(id,rect.size);
			}
		}

		public static void Set(this Renderer thiz,Mesh mesh) {
			if(thiz!=null) {
				if(thiz is SkinnedMeshRenderer sr) {sr.sharedMesh=mesh;}
				else {
					var mf=thiz.GetComponent<MeshFilter>();
					if(mf!=null) {mf.sharedMesh=mesh;}
				}
			}
		}

		public static Mesh CreatePlane(this Mesh thiz,int cols,int rows) {
			if(thiz==null) {thiz=new Mesh();}else {thiz.Clear();}
			float w=1.0f/cols,h=1.0f/rows;int i=0,imax=(cols+1)*(rows+1),j=0;
			Vector3[] vertices=new Vector3[imax];
			Vector2[] uv=new Vector2[imax];
			int[] triangles=new int[cols*rows*6];
			for(int y=0;y<=rows;++y) {for(int x=0;x<=cols;++x) {
				vertices[i]=new Vector3(-x*w+0.5f,y*h-0.5f,0.0f);// Flip X
				uv[i]=new Vector2(x*w,y*h);
				if(x<cols&&y<rows) {
					triangles[j++]=i;
					triangles[j++]=i+cols+2;
					triangles[j++]=i+1;
					triangles[j++]=i+cols+1;
					triangles[j++]=i+cols+2;
					triangles[j++]=i;
				}++i;
			}}
			if(imax>=uint.MaxValue) {
				Debug.LogError($"{vertices.Length}>={uint.MaxValue}");
			}
			thiz.indexFormat=imax>=ushort.MaxValue
				?UnityEngine.Rendering.IndexFormat.UInt32
				:UnityEngine.Rendering.IndexFormat.UInt16;
			thiz.vertices=vertices;
			thiz.uv=uv;
			thiz.triangles=triangles;
			thiz.RecalculateBounds();
			return thiz;
		}

		//

		public static Texture2D GetTemp2D() {
			if(s_Temp2D==null) {
				s_Temp2D=NewTexture2D(1,1);
				s_Temp2D.name=UnityExtension.s_TempTag;
			}
			return s_Temp2D;
		}

		public static Material GetUnlit() {
			if(s_Unlit==null) {
				s_Unlit=new Material(Shader.Find("Sprites/Default"));
			}
			return s_Unlit;
		}

		public static Camera GetCameraHelper() {
			if(s_CameraHelper==null) {
				GameObject go=new GameObject(typeof(UnityExtension).FullName+".CameraHelper");
				go.layer=31;GameObject.DontDestroyOnLoad(go);
				//
				s_CameraHelper=go.AddComponent<Camera>();
				s_CameraHelper.transform.localPosition=new Vector3(0.5f,0.5f,0.5f);
				s_CameraHelper.enabled=false;
				s_CameraHelper.cullingMask=1<<go.layer;
				s_CameraHelper.orthographic=true;
				s_CameraHelper.orthographicSize=0.5f;
				s_CameraHelper.aspect=1.0f;
				s_CameraHelper.nearClipPlane=-1.0f;
				s_CameraHelper.clearFlags=CameraClearFlags.SolidColor;
				s_CameraHelper.backgroundColor=Color.clear;
			}
			return s_CameraHelper;
		}

		public static GLRenderer GetGLRenderer() {
			if(s_GLRenderer==null) {
				GameObject go=new GameObject(typeof(UnityExtension).FullName+"."+nameof(GLRenderer));
				go.layer=31;GameObject.DontDestroyOnLoad(go);
				//
				s_GLRenderer=go.AddComponent<GLRenderer>();
				s_GLRenderer.enabled=false;
			}
			return s_GLRenderer;
		}

		// Render Pipeline APIs

		// Taken from https://discussions.unity.com/t/camera-render-seems-to-trigger-canvas-sendwillrendercanvases/658970
		public static void BeginRenderUI(Canvas.WillRenderCanvases ui=null) {
			if(s_RenderUIField==null) {
				s_RenderUIField=typeof(Canvas).GetField("willRenderCanvases",BindingFlags.NonPublic|BindingFlags.Static);
			}
				s_RenderUIEvent=s_RenderUIField.GetValue(null);s_RenderUIField.SetValue(null,ui);
		}

		public static void EndRenderUI() {
			if(s_RenderUIField!=null) {
				s_RenderUIField.SetValue(null,s_RenderUIEvent);s_RenderUIEvent=null;
			}
		}

		public static int GetRenderPipelines() {
			if(s_RenderPipelines==0) {
				s_RenderPipelines=1;
				if(System.Type.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset,Unity.RenderPipelines.Universal.Runtime")!=null) {s_RenderPipelines|=2;}
				if(System.Type.GetType("UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset,Unity.RenderPipelines.HighDefinition.Runtime")!=null) {s_RenderPipelines|=4;}
			}
			return s_RenderPipelines;
		}

		public static int GetRenderPipeline() {
			var type=GraphicsSettings.currentRenderPipelineAssetType;
			if(type!=null) {
				string ns=type.Namespace;
				if(ns=="UnityEngine.Rendering.Universal") {return 1;}
				if(ns=="UnityEngine.Rendering.HighDefinition") {return 2;}
				return -1;
			}
			return 0;
		}

		public static void SetRenderPipeline(int index) {
			if(s_RenderPipeline<0) {
				s_RenderPipeline=GetRenderPipeline();
			}
			if(s_RenderPipelineAssets==null) {
				s_RenderPipelineAssets=new RenderPipelineAsset[3]{null
					,Resources.Load<RenderPipelineAsset>("Settings/RPAsset_URP")
					,Resources.Load<RenderPipelineAsset>("Settings/RPAsset_HDRP")
				};
			}
			//
			if(index<0) {index=s_RenderPipeline;}
			GraphicsSettings.renderPipelineAsset=s_RenderPipelineAssets[index];
		}

		#endregion Methods
	}
}