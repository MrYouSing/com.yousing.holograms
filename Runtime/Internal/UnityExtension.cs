/* <!-- Macro.Copy File
:Packages/com.yousing.io/Runtime/Modules/MediaModule/Core/MediaExtension.cs,80~89,223,234~258,265
:Packages/com.yousing.io/Runtime/APIs/TextureAPI.cs,136,300~309,314~329,424~448,461~474
:Packages/com.yousing.input-extensions/Runtime/Internal/LangExtension.cs,15~25
:Packages/com.yousing.ui/Runtime/Internal/LangExtension.cs,238~243,320~331
 Macro.End --> */
/* <!-- Macro.Replace
MediaExtension,UnityExtension
if(FileUtility.Exists(json),json=json.GetFullPath();if(File.Exists(json)
FileUtility,File
TempTexture2D,GetTemp2D();}//
else if(texture.GetSizeI()!=s) {,if(texture.GetSizeI()!=s) {
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
#if (UNITY_EDITOR_WIN||UNITY_STANDALONE_WIN)&&!NET_STANDARD
#define DOTNET_GDI_PLUS
#endif
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Video;
#if DOTNET_GDI_PLUS
using System.Drawing;
using System.Drawing.Imaging;
using Color=UnityEngine.Color;
using Graphics=UnityEngine.Graphics;
#endif

namespace YouSingStudio.Holograms {
	public static partial class UnityExtension {
// <!-- Macro.Patch AutoGen
		public static HashSet<string> s_ImageExtensions=new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
		public static bool IsImage(string ext) {
			return s_ImageExtensions.Contains(ext);
		}

		public static HashSet<string> s_VideoExtensions=new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
		public static bool IsVideo(string ext) {
			return s_VideoExtensions.Contains(ext);
		}

		static UnityExtension() {
			// https://docs.unity3d.com/Manual/ImportingTextures.html
			s_ImageExtensions.Add(".bmp");
			s_ImageExtensions.Add(".exr");
			s_ImageExtensions.Add(".gif");
			s_ImageExtensions.Add(".hdr");
			s_ImageExtensions.Add(".iff");
			s_ImageExtensions.Add(".jpg");
			s_ImageExtensions.Add(".pict");
			s_ImageExtensions.Add(".png");
			s_ImageExtensions.Add(".psd");
			s_ImageExtensions.Add(".tga");
			s_ImageExtensions.Add(".tiff");
			// https://docs.unity3d.com/Manual/VideoSources-FileCompatibility.html
			s_VideoExtensions.Add(".asf");
			s_VideoExtensions.Add(".avi");
			s_VideoExtensions.Add(".dv");
			s_VideoExtensions.Add(".m4v");
			s_VideoExtensions.Add(".mov");
			s_VideoExtensions.Add(".mp4");
			s_VideoExtensions.Add(".mpg");
			s_VideoExtensions.Add(".mpeg");
			s_VideoExtensions.Add(".ogv");
			s_VideoExtensions.Add(".vp8");
			s_VideoExtensions.Add(".webm");
			s_VideoExtensions.Add(".wmv");
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
				if(texture==null) {texture=GetTemp2D();}//(s.x,s.y);}
				if(texture.GetSizeI()!=s) {texture.Reinitialize(s.x,s.y);}
				//
				var rt=thiz.Begin();
					texture.ReadPixels(new Rect(0,0,s.x,s.y),0,0,false);
					if(apply) {texture.Apply();}
				thiz.End(rt);
			}
			return texture;
		}

		public static T FromJson<T>(string json) {
			if(json?.EndsWith(".json")??false) {
				json=json.GetFullPath();if(File.Exists(json)) {json=File.ReadAllText(json);}
				else {json=null;}
			}
			if(!string.IsNullOrEmpty(json)) {
				return JsonConvert.DeserializeObject<T>(json);
			}
			return default;
		}

		public static string ToTime(this float thiz,string format="{0:00}:{1:00}:{2:00}") {
			int h=Mathf.FloorToInt(thiz/3600.0f);thiz-=h*3600.0f;
			int m=Mathf.FloorToInt(thiz/60.0f);thiz-=m*60.0f;
			return string.Format(format,h,m,thiz);
		}

		public static void Render<TView,TModel>(
			this IList<TView> thiz,IList<TModel> models,
			System.Action<TView,TModel> action,System.Func<TView> func
		) {
			if(thiz!=null&&action!=null&&func!=null) {
				int i=0,imax=models?.Count??0,icnt=thiz.Count,imin=Mathf.Min(icnt,imax);TView tmp;
				for(;i<imin;++i) {action(thiz[i],models[i]);}// Show
				for(;i<imax;++i) {tmp=func();thiz.Add(tmp);action(tmp,models[i]);}///New
				for(;i<icnt;++i) {action(thiz[i],default);}// Hide
			}
		}

// Macro.Patch -->
		#region Fields

		public const System.StringComparison k_Comparison=System.StringComparison.OrdinalIgnoreCase;
		public static readonly char[] k_Split_Dir=new char[]{'\\','/'};
		/// <summary>
		/// <seealso cref="HideFlags.HideAndDontSave"/>
		/// </summary>
		public static string s_TempTag="HideFlags.HideAndDontSave";
		public static Vector3 s_DefaultQuilt=new Vector3(8,5,0.5625f);
		public static string s_ExeRoot=
#if UNITY_EDITOR_WIN
			"Packages/com.yousing.holograms/GameAssets/StreamingAssets/Windows";
#elif UNITY_STANDALONE_WIN
			Application.streamingAssetsPath+"/Windows";
#else
			"";
#endif
		public static string[] s_Settings=new string[]{
			"",
			"Settings/",
			"$(StreamingAssets)/Settings/"
		};

		public static Texture2D s_Temp2D;
		public static Material s_Unlit;
		public static Camera s_CameraHelper;
		public static GLRenderer s_GLRenderer;
		public static Vector3[] s_Vector3Helper=new Vector3[8];
		public static Dictionary<string,Vector3> s_QuiltMap=new Dictionary<string,Vector3>();

		#endregion Fields

		#region Methods
	
		/// <summary>
		/// <seealso cref="Mathf.Approximately(float,float)"/>
		/// </summary>
		public static bool Approximately(float a,float b,float c=Vector3.kEpsilon) {
			a-=b;return a*a<=c*c;
		}

		public static bool IsSerialNumber(this string thiz) {
			int cnt=thiz?.Length??0;if(cnt>0) {
				int i=thiz.LastIndexOfAny(k_Split_Dir),j=thiz.LastIndexOf('.');
				if(j<0) {j=cnt;}if(i<j) {
					char[] pch=thiz.ToCharArray();
					for(++i;i<j;++i) {if(!char.IsDigit(pch[i])) {return false;}}
					return true;
				}
			}
			return false;
		}

		public static bool TwoPieces(this Vector3 thiz) {
			return Approximately(thiz.x*thiz.y,2.0f);
		}

		public static float ToFloat(this string thiz,string flag,float value=0.0f) {
			if(!string.IsNullOrEmpty(thiz)&&!string.IsNullOrEmpty(flag)) {
				thiz=Path.GetFileNameWithoutExtension(thiz);
				char[] pch=thiz.ToCharArray();
				int i=thiz.LastIndexOf(flag,k_Comparison),jmax=pch.Length;
				if(i>=0) {i+=flag.Length;}else {i=jmax;}
				if(i<jmax&&char.IsDigit(pch[i])) {
					int j=i+1;for(;j<jmax;++j) {
						if(pch[j]!='.'&&!char.IsDigit(pch[j])) {break;}
					}
					return float.Parse(new string(pch,i,j-i));
				}
			}
			return value;
		}

		public static float ToAspect(this string thiz) {
			return thiz.ToFloat("_a",float.NaN);
		}

		public static Video3DLayout To3DLayout(this string thiz) {
			if(!string.IsNullOrEmpty(thiz)) {
				thiz=Path.GetFileName(thiz);
				if(thiz.LastIndexOf("_sbs.",k_Comparison)>=0||thiz.LastIndexOf("_sbs_",k_Comparison)>=0) {
					return Video3DLayout.SideBySide3D;
				}
				if(thiz.LastIndexOf("_ou.",k_Comparison)>=0||thiz.LastIndexOf("_ou_",k_Comparison)>=0) {
					return Video3DLayout.SideBySide3D;
				}
			}
			return Video3DLayout.No3D;
		}

		public static TextureType ToTextureType(this string thiz,TextureType type=TextureType.Default) {
			if(!string.IsNullOrEmpty(thiz)) {
				bool b=thiz.IsSerialNumber();thiz=Path.GetFileNameWithoutExtension(thiz);
				if(thiz.EndsWith("_sbs",k_Comparison)||thiz.EndsWith("_ou",k_Comparison)) {// Video3DLayout
					return TextureType.Stereo;
				}
				if(thiz.EndsWith("_rgb",k_Comparison)||thiz.EndsWith("_rgbd",k_Comparison)) {
					return TextureType.Depth;
				}
				if(thiz.EndsWith("_vr",k_Comparison)||thiz.EndsWith("_xr",k_Comparison)) {
					return TextureType.Panoramic;
				}
				int i=thiz.LastIndexOf("_qs");
				if((i>=0&&char.IsDigit(thiz[i+3]))||b) {
					return TextureType.Quilt;
				}
			}
			return type;
		}

		public static Vector3 ParseLayout(this string thiz) {
			switch(thiz.To3DLayout()) {
				case Video3DLayout.SideBySide3D:
					return new Vector3(2.0f,1.0f,thiz.ToAspect());
				case Video3DLayout.OverUnder3D:
					return new Vector3(1.0f,2.0f,thiz.ToAspect());
			}
			return new Vector3(1.0f,1.0f,-1.0f);
		}

		/// <summary>
		/// <seealso href="https://docs.lookingglassfactory.com/software-tools/looking-glass-studio/quilt-photo-video"/>
		/// </summary>
		public static Vector3 ParseQuilt(this string thiz) {
			if(!string.IsNullOrEmpty(thiz)) {
				//
				if(s_QuiltMap.TryGetValue(Path.GetFileNameWithoutExtension(thiz),out var v)) {return v;}
				//
				int i=thiz.LastIndexOf("_qs");
				if(i>=0) {
					string ext=Path.GetExtension(thiz);bool b=IsImage(ext)||IsVideo(ext);
					i+=2;int j=thiz.IndexOf('x',i),k=thiz.IndexOf('a',j),l=b?thiz.LastIndexOf('.'):thiz.Length;
					if(j>=0&&k>=0&&l>=0) {
						return new Vector3(
							float.Parse(thiz.Substring(i+1,j-i-1)),
							float.Parse(thiz.Substring(j+1,k-j-1)),
							-float.Parse(thiz.Substring(k+1,l-k-1))
						);
					}
				}
			}
			return s_DefaultQuilt;
		}

		public static void SetQuilt(this string thiz,Vector3 value) {
			if(!string.IsNullOrEmpty(thiz)) {
				s_QuiltMap[Path.GetFileNameWithoutExtension(thiz)]=value;
			}
		}

		public static string ToQuilt(this Vector3 thiz) {
			return $"{thiz.x}x{thiz.y}a{thiz.z}";
		}

		public static Rect ToPreview(this Vector3 thiz) {
			float dw=1.0f/thiz.x,dh=1.0f/thiz.y;
			int w=(int)thiz.x,i=Mathf.FloorToInt(thiz.x*thiz.y*0.5f)-1;// HalfLengthToIndex
			return new Rect(new Vector2((i%w)*dw,(i/w)*dh),new Vector2(dw,dh));
		}

		public static Rect GetRect(this Video3DLayout thiz,int index) {
			switch(thiz) {
				case Video3DLayout.SideBySide3D:return new Rect(0.5f*index,0.0f,0.5f,1.0f);
				case Video3DLayout.OverUnder3D:return new Rect(0.0f,0.5f*index,1.0f,0.5f);
				default:return new Rect(Vector2.zero,Vector2.one);
			}
		}

		/// <summary>
		/// <seealso cref="UnityEditor.L10n.Tr(string)"/>
		/// </summary>
		public static string Tr(this string thiz) {
			return thiz;
		}

		/// <summary>
		/// <seealso cref="Path.Combine(string,string)"/>
		/// </summary>
		public static string Path_Combine(string x,string y) {
			if(!x.EndsWith(k_Split_Dir[0])&&!x.EndsWith(k_Split_Dir[1])
			 &&!y.StartsWith(k_Split_Dir[0])&&!y.StartsWith(k_Split_Dir[1])
			) {
				x=Path.Combine(x,y);
			}else {
				x+=y;
			}
			return x.Replace(k_Split_Dir[0],k_Split_Dir[1]);
		}

		/// <summary>
		/// <seealso cref="Path.GetFullPath(string))"/>
		/// </summary>
		public static string GetFullPath(this string thiz) {
			if(thiz?.StartsWith('$')??false) {
				int i=thiz.IndexOf(')');
				string str=thiz.Substring(2,i-2);
				thiz=thiz.Substring(i+1);
				switch(str.ToLower()) {
					//case "@":return Path_Combine(Application.@Path,thiz);
					case "data":return Path_Combine(Application.dataPath,thiz);
					case "persistentdata":return Path_Combine(Application.persistentDataPath,thiz);
					case "streamingassets":return Path_Combine(Application.streamingAssetsPath,thiz);
					case "temporarycache":return Path_Combine(Application.temporaryCachePath,thiz);
					//
					case "document":return Path_Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),thiz);
					case "appdata":return Path_Combine(Path.GetDirectoryName(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)),thiz);
				}
			}
			return thiz;
		}

		public static List<string> UnpackPaths(this List<string> thiz,System.Func<string,bool> func=null,List<string> paths=null) {
			int i=0,imax=thiz?.Count??0;
			paths?.Clear();
			if(imax>0) {
				if(paths==null) {paths=new List<string>();}
				string it;for(;i<imax;++i) {
					it=thiz[i].GetFullPath();if(File.Exists(it)) {
						if(func?.Invoke(it)??true){paths.Add(it);}
					}else if(Directory.Exists(it)) {foreach(string fn in Directory.GetFiles(it,"*.*",SearchOption.AllDirectories)) {
						if(func?.Invoke(fn)??true){paths.Add(fn);}
					}}
				}
			}
			return paths;
		}

		//

		public static bool IsTemporary(this Object thiz) {
			if(thiz!=null) {
				string tmp=thiz.name;
				return tmp?.StartsWith(s_TempTag)??false;
			}
			return false;
		}

		public static void LoadSettings(this object thiz,string path) {
			if(thiz!=null) {
				string it;for(int i=0,imax=s_Settings?.Length??0;i<imax;++i) {
					it=(s_Settings[i]+path).GetFullPath();
					if(File.Exists(it)) {JsonConvert.PopulateObject(File.ReadAllText(it),thiz);return;}
				}
			}
		}

		public static T NewActor<T>(this Component thiz,string key) where T:Component {
			GameObject go=new GameObject(key);
			if(thiz!=null) {go.transform.SetParent(thiz.transform,false);}
			return go.AddComponent<T>();
		}

		  //

		public static Vector2Int GetSizeI(this Texture thiz) {
			if(thiz!=null) {return new Vector2Int(thiz.width,thiz.height);}
			else {return Vector2Int.zero;}
		}

		public static Texture2D NewTexture2D(int width,int height,bool linear) {
			return new Texture2D(width,height,TextureFormat.RGBA32,false,linear) ;
		}

		public static Texture2D NewTexture2D(int width,int height)=>NewTexture2D(width,height,false);

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

		public static RenderTexture GetTexture(this VideoPlayer thiz,bool resize=false) {
			RenderTexture rt=null;
			if(thiz!=null) {
				rt=thiz.targetTexture;Texture tex=thiz.texture;
				if(rt==null) {
					rt=tex as RenderTexture;
				}else if(tex!=null) {Vector2Int a=tex.GetSizeI(),b=rt.GetSizeI();if(a!=b){
					if(resize) {
						if(rt.IsTemporary()) {rt.Free();}
						rt=RenderTexture.GetTemporary(a.x,a.y);rt.name=s_TempTag;
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

		public static float GetPlaneHeight(this Camera thiz,float depth) {
			if(thiz!=null) {
				if(thiz.orthographic) {
					return thiz.orthographicSize*2.0f;
				}else {
					return (Mathf.Tan(thiz.fieldOfView*0.5f*Mathf.Deg2Rad)*depth)*2.0f;
				}
			}
			return 0.0f;
		}

		public static float GetPlaneDepth(this Camera thiz,float height) {
			if(thiz!=null&&!thiz.orthographic) {
				return height*0.5f/(Mathf.Tan(thiz.fieldOfView*0.5f*Mathf.Deg2Rad));
			}
			return 0.0f;
		}

		  //

		/// <summary>
		/// <seealso cref="GameObject.SetActive(bool)"/>
		/// </summary>
		public static void SetActive(this CanvasGroup thiz,bool value) {
			if(thiz!=null) {
				thiz.alpha=value?1.0f:0.0f;
				thiz.blocksRaycasts=value;
				thiz.interactable=value;
			}
		}

		public static void StretchParent(this RectTransform thiz,Transform parent) {
			if(thiz!=null) {
				thiz.SetParent(parent,false);
				//
				thiz.anchorMin=Vector2.zero;
				thiz.anchorMax=Vector2.one;
				thiz.anchoredPosition=Vector2.zero;
				thiz.sizeDelta=Vector2.zero;
			}
		}

		  //

		public static Texture2D GetTemp2D() {
			if(s_Temp2D==null) {
				s_Temp2D=NewTexture2D(1,1);
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
#if UNITY_EDITOR
		// TODO: Copy from the private library.
		public static void FillPlane(this Vector3[] thiz,Vector3 center,Quaternion rotation,Vector2 size,int offset=0) {
			size*=0.5f;
			thiz[offset]=center+rotation*new Vector3(-size.x,-size.y,0.0f);++offset;
			thiz[offset]=center+rotation*new Vector3(-size.x, size.y,0.0f);++offset;
			thiz[offset]=center+rotation*new Vector3( size.x, size.y,0.0f);++offset;
			thiz[offset]=center+rotation*new Vector3( size.x,-size.y,0.0f);++offset;
		}

		/// <summary>
		/// <seealso cref="Gizmos.DrawLineStrip"/>
		/// </summary>
		public static void DrawLines(Vector3[] points,int offset=0,int count=-1,bool loop=false) {
			if(count<0) {count=(points?.Length??0)-offset;}
			if(count<=0) {return;}
			int start=offset;count=offset+count-1;for(;offset<count;++offset) {
				Gizmos.DrawLine(points[offset],points[offset+1]);
			}
			if(loop) {Gizmos.DrawLine(points[offset],points[start]);}
		}

		/// <summary>
		/// <seealso cref="Gizmos.DrawWireCube"/>
		/// </summary>
		public static void DrawWirePlane(Vector3 center,Quaternion rotation,Vector2 size) {
			s_Vector3Helper.FillPlane(center,rotation,size,0);
			DrawLines(s_Vector3Helper,0,4,true);
		}

		/// <summary>
		/// <seealso cref="Gizmos.DrawFrustum"/>
		/// </summary>
		public static void DrawFrustum(Quaternion rotation,Vector3 nearCenter,Vector2 nearSize,Vector3 farCenter,Vector2 farSize) {
			s_Vector3Helper.FillPlane(nearCenter,rotation,nearSize,0);DrawLines(s_Vector3Helper,0,4,true);
			s_Vector3Helper.FillPlane(farCenter,rotation,farSize,4);DrawLines(s_Vector3Helper,4,4,true);
			for(int i=0;i<4;++i) {Gizmos.DrawLine(s_Vector3Helper[i],s_Vector3Helper[4+i]);}
		}
#endif
		#endregion Methods
	}
}
