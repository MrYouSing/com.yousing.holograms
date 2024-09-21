/* <!-- Macro.Copy File
:Packages/com.yousing.io/Runtime/Modules/MediaModule/Core/MediaExtension.cs,80~89,223,234~258,265
:Packages/com.yousing.io/Runtime/APIs/TextureAPI.cs,418~443
:Packages/com.yousing.input-extensions/Runtime/Internal/LangExtension.cs,15~25
:Packages/com.yousing.ui/Runtime/Internal/LangExtension.cs,238~243,320~331
 Macro.End --> */
/* <!-- Macro.Replace
MediaExtension,UnityExtension
if(FileUtility.Exists(json),json=json.GetFullPath();if(File.Exists(json)
FileUtility,File
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class ArrayElementAttribute:PropertyAttribute {
		public string names;

		public void Foo()=>throw new System.NotImplementedException();
	}

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
			if(thiz==null||texture==null) {return;}
			//
			var rt=thiz.Begin();
				Graphics.Blit(texture,thiz);
			thiz.End(rt);
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

		public static string ToTime(this float thiz,string format="{0}:{1}:{2}") {
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

		public static Vector3 s_DefaultQuilt=new Vector3(8,5,0.5625f);
		public static Camera s_CameraHelper;
		public static GLRenderer s_GLRenderer;

		#endregion Fields

		#region Methods
	
		/// <summary>
		/// <seealso cref="Mathf.Approximately(float,float)"/>
		/// </summary>
		public static bool Approximately(float a,float b,float c=Vector3.kEpsilon) {
			a-=b;return a*a<=c*c;
		}

		/// <summary>
		/// <seealso href="https://docs.lookingglassfactory.com/software-tools/looking-glass-studio/quilt-photo-video"/>
		/// </summary>
		public static Vector3 ParseQuilt(this string thiz) {
			if(!string.IsNullOrEmpty(thiz)) {
				int i=thiz.LastIndexOf("_qs");
				if(i>=0) {
					i+=2;int j=thiz.IndexOf('x',i),k=thiz.IndexOf('a',j),l=thiz.LastIndexOf('.');
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

		public static string ToQuilt(this Vector3 thiz) {
			return $"{thiz.x}x{thiz.y}a{thiz.z}";
		}

		public static Rect ToPreviewRect(this Vector2 thiz) {
			float dw=1.0f/thiz.x,dh=1.0f/thiz.y;
			int w=(int)thiz.x,i=Mathf.FloorToInt(thiz.x*thiz.y*0.5f);
			return new Rect(new Vector2((i%w)*dw,(i/w)*dh),new Vector2(dw,dh));
		}

		public static string GetFullPath(this string thiz) {
			if(thiz?.StartsWith('$')??false) {
				int i=thiz.IndexOf(')');
				string str=thiz.Substring(2,i-2);
				thiz=thiz.Substring(i+1);
				switch(str.ToLower()) {
					//case "@":return Path.Combine(Application.@Path,thiz);
					case "data":return Path.Combine(Application.dataPath,thiz);
					case "persistentdata":return Path.Combine(Application.persistentDataPath,thiz);
					case "streamingassets":return Path.Combine(Application.streamingAssetsPath,thiz);
					case "temporarycache":return Path.Combine(Application.temporaryCachePath,thiz);
					case "appdata":return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),thiz);
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
					it=thiz[i];if(File.Exists(it)) {
						if(func?.Invoke(it)??true){paths.Add(it);}
					}else if(Directory.Exists(it)) {foreach(string fn in Directory.GetFiles(it,"*.*",SearchOption.AllDirectories)) {
						if(func?.Invoke(fn)??true){paths.Add(fn);}
					}}
				}
			}
			return paths;
		}

		//

		public static void LoadSettings(this Object thiz,string path) {
			if(thiz!=null) {path=path.GetFullPath();
			if(File.Exists(path)) {
				JsonConvert.PopulateObject(File.ReadAllText(path),thiz);
			}}
		}

		  //

		public static bool IsNullOrEmpty(this RenderTexture thiz) {
			throw new System.NotImplementedException();
		}

		  //

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

		#endregion Methods
	}
}
