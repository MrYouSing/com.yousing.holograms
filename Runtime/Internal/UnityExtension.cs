/* <!-- Macro.Define DeclareEvent
		[System.NonSerialized]public System.Action{1} on{2}=null;
		[SerializeField]protected UnityEngine.Events.UnityEvent{1} m_On{2}=null;

		{0} virtual void {3}On{2}({4}) {{
			on{2}?.Invoke({5});
			m_On{2}?.Invoke({5});
		}}

 Macro.End --> */
/* <!-- Macro.Copy File
:Packages/com.yousing.io/Runtime/Modules/MediaModule/Core/MediaExtension.cs,81~95,229,240~271,278
:Packages/com.yousing.io/Runtime/Internal/UnityExtension.cs,25~32,11,73~90
:Packages/com.yousing.input-extensions/Runtime/Internal/LangExtension.cs,15~25
:Packages/com.yousing.ui/Runtime/Internal/LangExtension.cs,238~243,320~331
:Packages/com.yousing.ui/Runtime/Internal/UnityExtension.cs,302~324,366~375,424~462
:Packages/com.yousing.rendering/Runtime/Internal/CameraExtension.cs,81~84,97~114
:Packages/com.yousing.rendering/Runtime/Internal/GizmoUtility.cs,54,131~141,175~184,190~194,219~226
:Packages/com.yousing.rendering/Runtime/Geometry/Bounds/GenericBounds.cs,8~21
:Packages/com.yousing.rendering/Runtime/Geometry/Bounds/UnityBounds.cs,89~100
 Macro.End --> */
/* <!-- Macro.Replace
MediaExtension,UnityExtension
if(FileUtility.Exists(json),json=json.GetFullPath();if(File.Exists(json)
FileUtility,File
else if(texture.GetSizeI()!=s) {,if(texture.GetSizeI()!=s) {
GetFieldOfView(),fieldOfView
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;
using UnityEngine.Video;

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

		public static HashSet<string> s_ModelExtensions=new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
		public static bool IsModel(string ext) {
			return s_ModelExtensions.Contains(ext);
		}

		static UnityExtension() {
			// https://docs.unity3d.com/Manual/ImportingTextures.html
			s_ImageExtensions.Add(".png");
			s_ImageExtensions.Add(".jpg");s_ImageExtensions.Add(".jpeg");
			s_ImageExtensions.Add(".exr");
			s_ImageExtensions.Add(".bmp");
			s_ImageExtensions.Add(".gif");
			s_ImageExtensions.Add(".hdr");
			s_ImageExtensions.Add(".iff");
			s_ImageExtensions.Add(".pict");
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
			//
			s_ModelExtensions.Add(".ab");
			s_ModelExtensions.Add(".unity");// AssetBundle
			s_ModelExtensions.Add(".gltf");
			s_ModelExtensions.Add(".glb");
			s_ModelExtensions.Add(".obj");
			s_ModelExtensions.Add(".fbx");
		}
		public static void SetRealName(this Object thiz) {// TODO: Fix the "(Clone)".
			if(thiz!=null) {
				string key=thiz.name;if(key.EndsWith("(Clone)")) {
					thiz.name=key.Substring(0,key.Length-"(Clone)".Length).Trim();
				}
			}
		}

		public static IDictionary<string,Object> s_ResourceInstances=null;
		/// <summary>
		/// <seealso cref="Object.FindAnyObjectByType{T}()"/><br/>
		/// <seealso cref="Resources.FindObjectsOfTypeAll{T}()"/>
		/// </summary>
		public static T GetResourceInstance<T>(string path) where T:Object {
			// Find Instance.
			Object obj=null;bool found;
			if(s_ResourceInstances!=null) {found=s_ResourceInstances.TryGetValue(path,out obj)&&obj!=null;}
			else {found=false;s_ResourceInstances=new Dictionary<string,Object>();}
			// New Instance.
			if(!found) {T tmp=Resources.Load<T>(path);if(tmp!=null) {
				string key=tmp.name;// TODO: Fix the "(Clone)".
					tmp=Object.Instantiate(tmp);Object.DontDestroyOnLoad(tmp);
				tmp.name=key;s_ResourceInstances[path]=tmp;return tmp;
			}}
			return (T)obj;
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

		public static Vector2 FitScale(Vector2 from,Vector2 to,UnityEngine.Video.VideoAspectRatio mode) {
			float a=from.x/from.y,b=to.x/to.y;
			switch(mode) {
				case UnityEngine.Video.VideoAspectRatio.NoScaling:to=from;break;
				case UnityEngine.Video.VideoAspectRatio.FitHorizontally:
					to.y=to.x/a;
				break;
				case UnityEngine.Video.VideoAspectRatio.FitVertically:
					to.x=to.y*a;
				break;
				case UnityEngine.Video.VideoAspectRatio.FitOutside:
					if(a<=b){to.y=to.x/a;}
					else if(a>b){to.x=to.y*a;}
				break;
				case UnityEngine.Video.VideoAspectRatio.FitInside:
					if(a>=b){to.y=to.x/a;}
					else if(a<b){to.x=to.y*a;}
				break;
				case UnityEngine.Video.VideoAspectRatio.Stretch:break;
			}
			return to;
		}

		public static Vector3 GetWorldPoint(this RectTransform thiz,Vector3 pivot,Vector3 offset) {
			if(thiz!=null) {
				Rect r=thiz.rect;return thiz.TransformPoint(new Vector3(
					Mathf.Lerp(r.xMin,r.xMax,pivot.x)+offset.x,
					Mathf.Lerp(r.yMin,r.yMax,pivot.y)+offset.y,
				offset.z));
			}
			return Vector3.zero;
		}

		/// <summary>
		/// <seealso cref="Camera.WorldToViewportPoint(Vector3)"/>
		/// </summary>
		public static Vector2 WorldToNormalizedPoint(this ScrollRect thiz,Vector3 point) {
			Vector2 v=Vector2.up;// Default.
			if(thiz!=null) {RectTransform 
				rt=thiz.content;if(rt==null) {return v;}Rect a=rt.rect;
				rt=thiz.viewport;if(rt==null) {return v;}Rect b=rt.rect;
				v=(Vector2)rt.InverseTransformPoint(point)-b.min;
				v.x/=(a.width-b.width);v.y/=(a.height-b.height);
				v+=thiz.normalizedPosition;
			}
			return v;
		}

		/// <summary>
		/// <seealso cref="LayoutGroup"/>
		/// </summary>
		public static Vector2 GetNormalizedPoint(this ScrollRect thiz,RectTransform rect,Vector2 padding,Vector2 spacing) {
			if(thiz!=null) {Vector2 v=thiz.normalizedPosition;
				if(rect!=null) {
					RectTransform rt=thiz.viewport;Rect a=rt.rect;Rect b=rect.rect;
					Vector3 o=rt.InverseTransformPoint(rect.TransformPoint(b.center))-(Vector3)a.center;
					float f;int x=0,y=0;Vector2 s=a.size;
					f=s.x*0.5f-padding.x;if(o.x*o.x>f*f) {x=System.MathF.Sign(o.x);}
					f=s.y*0.5f-padding.y;if(o.y*o.y>f*f) {y=System.MathF.Sign(o.y);}
					//
					if(x!=0||y!=0) {
						o.x=x*0.5f+0.5f;spacing.x*=x;
						o.y=y*0.5f+0.5f;spacing.y*=y;
						v=thiz.WorldToNormalizedPoint(rect.GetWorldPoint(
							new Vector2(o.x,o.y),new Vector3(-s.x*o.x+spacing.x,-s.y*o.y+spacing.y,0.0f)
						));
					}
				}
				return v;
			}else {return Vector2.up;}// Default.
		}

		public static float WorldToDepth(this Camera thiz,Vector3 point) {// OpenGL to Unity.
			return (thiz!=null)?-thiz.worldToCameraMatrix.MultiplyPoint3x4(point).z:0.0f;
		}

		public static float GetPlaneHeight(this Camera thiz,float depth) {
			if(thiz!=null) {
				if(thiz.orthographic) {
					return thiz.orthographicSize*2.0f;
				}else {
					return Mathf.Tan(thiz.fieldOfView*0.5f*Mathf.Deg2Rad)*depth*2.0f;
				}
			}
			return 0.0f;
		}

		public static float GetPlaneDepth(this Camera thiz,float height) {
			if(thiz!=null&&!thiz.orthographic) {
				return height*0.5f/Mathf.Tan(thiz.fieldOfView*0.5f*Mathf.Deg2Rad);
			}
			return 0.0f;
		}

		public static readonly Vector3[] s_Vector3Array=new Vector3[8];
		public static int SetQuad(this IList<Vector3> thiz,Vector3 center,Quaternion rotation,Vector2 size,int offset=0) {
			int imax=4;
			if(offset+imax<=(thiz?.Count??0)) {size*=0.5f;// Clockwise.
				thiz[offset]=center+rotation*new Vector3(-size.x,-size.y,0.0f);++offset;
				thiz[offset]=center+rotation*new Vector3(-size.x, size.y,0.0f);++offset;
				thiz[offset]=center+rotation*new Vector3( size.x, size.y,0.0f);++offset;
				thiz[offset]=center+rotation*new Vector3( size.x,-size.y,0.0f);++offset;
			}
			return imax;
		}

		public static void DrawLines(IList<Vector3> points,int offset=0,int count=-1,bool loop=false) {
			if(count<0) {count=(points?.Count??0)-offset;}
			if(count>0) {count=offset+count-1;int start=offset;
				for(;offset<count;++offset) {
					Gizmos.DrawLine(points[offset],points[offset+1]);
				}
				if(loop) {Gizmos.DrawLine(points[offset],points[start]);}
			}
		}

		public static void DrawWireQuad(Vector3 center,Quaternion rotation,Vector2 size) {
			int cnt=s_Vector3Array.SetQuad(center,rotation,size,0);
			DrawLines(s_Vector3Array,0,cnt,true);
		}

		public static void DrawWireBox(Quaternion rotation,Vector3 nearCenter,Vector2 nearSize,Vector3 farCenter,Vector2 farSize) {
			s_Vector3Array.SetQuad(nearCenter,rotation,nearSize,0);
			s_Vector3Array.SetQuad( farCenter,rotation, farSize,4);
			DrawLines(s_Vector3Array,0,4,true);DrawLines(s_Vector3Array,4,4,true);
			for(int i=0;i<4;++i) {
				Gizmos.DrawLine(s_Vector3Array[i],s_Vector3Array[4+i]);
			}
		}
		public static Bounds GetBounds<T>(this IList<T> thiz,System.Func<T,Bounds> func) {
			Bounds b=new Bounds();
			if(thiz!=null&&func!=null) {bool f=false;
				T it;for(int i=0,imax=thiz.Count;i<imax;++i) {
					it=thiz[i];if(it!=null) {Bounds a=func(it);
					if(a.size.sqrMagnitude>0.0f) {
						if(!f) {f=true;b=a;}// First
						else {b.Encapsulate(a);}// More
					}}
				}
			}
			return b;
		}

		public static Bounds GetBounds(this GameObject thiz,bool includeInactive=false) {
			if(thiz!=null&&(includeInactive||thiz.activeInHierarchy)) {
			using(ListPool<Collider>.Get(out var c)) {
			using(ListPool<Renderer>.Get(out var r)) {
				thiz.GetComponentsInChildren(includeInactive:includeInactive,c);
				thiz.GetComponentsInChildren(includeInactive:includeInactive,r);
				Bounds b=c.GetBounds(x=>x.bounds);b.Encapsulate(r.GetBounds(x=>x.bounds));
				return b;
			}}}
			return new Bounds();
		}

// Macro.Patch -->
		#region Fields

		public const System.StringComparison k_Comparison=System.StringComparison.OrdinalIgnoreCase;
		public static readonly char[] k_Split_Dir=new char[]{'\\','/'};
		public static readonly char[] k_Split_Tag=new char[]{',',';'};
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
		public static string[] s_SettingPaths=new string[]{
			"{0}.json",
			"Settings/{0}.json",
			"$(StreamingAssets)/Settings/{0}.json"
		};
		public static char[] s_InvalidPathChars=Path.GetInvalidPathChars();
		public static string[] s_ByteSizes={"Byte","KB","MB","GB","TB"};

		public static JObject s_Settings;
		public static Dictionary<string,Vector3> s_QuiltMap=new Dictionary<string,Vector3>();
		public static Dictionary<System.Type,System.Delegate> s_Events=new Dictionary<System.Type,System.Delegate>();

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

		public static bool IsWebsite(this string thiz) {
			if(string.IsNullOrEmpty(thiz)) {return false;}
			if(!thiz.StartsWith("http",k_Comparison)) {return false;}
			return true;
		}

		public static bool TwoPieces(this Vector3 thiz) {
			return Approximately(thiz.x*thiz.y,2.0f);
		}

		public static int GetMiddle(this int thiz) {
			return Mathf.CeilToInt(thiz*0.5f)-1;
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

		public static Video3DLayout To3DLayout(this string thiz,Video3DLayout layout=Video3DLayout.No3D) {
			if(!string.IsNullOrEmpty(thiz)) {
				thiz=Path.GetFileName(thiz);
				if(thiz.LastIndexOf("_sbs.",k_Comparison)>=0||thiz.LastIndexOf("_sbs_",k_Comparison)>=0) {
					return Video3DLayout.SideBySide3D;
				}
				if(thiz.LastIndexOf("_ou.",k_Comparison)>=0||thiz.LastIndexOf("_ou_",k_Comparison)>=0) {
					return Video3DLayout.SideBySide3D;
				}
			}
			return layout;
		}

		public static TextureType ToTextureType(this string thiz,TextureType type=TextureType.Default) {
			if(!string.IsNullOrEmpty(thiz)) {
				if(IsModel(Path.GetExtension(thiz))) {return TextureType.Model;}
				//
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
				if(thiz.EndsWith("_raw",k_Comparison)) {
					return TextureType.Raw;
				}
				int i=thiz.LastIndexOf("_qs");
				if((i>=0&&char.IsDigit(thiz[i+3]))||b) {
					return TextureType.Quilt;
				}
			}
			return type;
		}

		public static Vector3 ParseLayout(this string thiz,Video3DLayout layout=Video3DLayout.No3D) {
			switch(thiz.To3DLayout(layout)) {
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
				int l=thiz.LastIndexOf('.');if(thiz.IndexOf("_raw",k_Comparison)==l-4) {return Vector3.zero;}
				int i=thiz.LastIndexOf("_qs");
				if(i>=0) {
					string ext=l>=0?thiz.Substring(l):string.Empty;bool b=IsImage(ext)||IsVideo(ext);
					i+=2;int j=thiz.IndexOf('x',i),k=thiz.IndexOf('a',j);if(!b) {l=thiz.Length;}
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
			int w=(int)thiz.x,i=(int)(thiz.x*thiz.y);i=i.GetMiddle();
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

		public static string ToShorty(this string thiz,int length=0xFF,string link="~") {
			int cnt=(thiz?.Length??0);if(length>0&&cnt>length) {
				length=(length-link.Length)/2;
				thiz=thiz.Substring(0,length)+link+thiz.Substring(cnt-length);
			}
			return thiz;
		}

		// Taken from https://blog.csdn.net/u013354943/article/details/86674509
		public static string ToByteSize(this ulong thiz) {
			int i=0;double d=thiz;
			if(thiz>1024) {
			for(i=0;(thiz/1024)>0;i++,thiz/=1024) {
				d=thiz/1024.0;
			}}
			return string.Format("{0:0.##}{1}",d,s_ByteSizes[i]);
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
			return x.FixPath();
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
					case "savedata":
#if !UNITY_EDITOR&&UNITY_STANDALONE_WIN
					return Path_Combine(Application.dataPath,thiz);
#else
					return Path_Combine(Application.persistentDataPath,thiz);
#endif
					case "document":return Path_Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),thiz);
					case "appdata":return Path_Combine(Path.GetDirectoryName(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)),thiz);
				}
			}
			return thiz.FixPath();
		}

		public unsafe static string FixPath(this string thiz) {
			int cnt=thiz?.Length??0;
			if(cnt>0) {fixed(char* pch=thiz) {
			for(int i=0;i<cnt;++i) {
				if(pch[i]==k_Split_Dir[0]) {pch[i]=k_Split_Dir[1];}
			}}}
			return thiz;
		}

		public unsafe static string ValidatePath(this string thiz) {
			int cnt=thiz?.Length??0;
			if(cnt>0) {fixed(char* pch=thiz) {
			for(int i=0;i<cnt;++i) {
				if(System.Array.IndexOf(s_InvalidPathChars,pch[i])>=0) {pch[i]='_';}
			}}}
			return thiz;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetFilePath(this string thiz)=>Path.GetFullPath(thiz).FixPath();

		internal static void UnpackPath(this string thiz,System.Func<string,bool> func,List<string> paths) {
			bool b=false;
			using(ListPool<string>.Get(out var list)) {
				foreach(string fn in Directory.GetFiles(thiz,"*.*",SearchOption.TopDirectoryOnly)) {
					if(IsModel(Path.GetExtension(fn))) {// Model as bundle.
						b=true;if(func?.Invoke(fn)??true){paths.Add(fn.GetFilePath());}
					}else {list.Add(fn);}
				}
				if(!b) {string it;for(int i=0,imax=list?.Count??0;i<imax;++i) {
					it=list[i];if(func?.Invoke(it)??true){paths.Add(it.GetFilePath());}
				}}
			}
			if(!b) {foreach(string dn in Directory.GetDirectories(thiz)) {
				dn.UnpackPath(func,paths);
			}}
		}

		public static List<string> UnpackPaths(this List<string> thiz,System.Func<string,bool> func=null,List<string> paths=null) {
			int i=0,imax=thiz?.Count??0;
			paths?.Clear();
			if(imax>0) {
				if(paths==null) {paths=new List<string>();}
				string it;for(;i<imax;++i) {
					it=thiz[i].GetFullPath();if(File.Exists(it)) {
						if(func?.Invoke(it)??true){paths.Add(it.GetFilePath());}
					}else if(Directory.Exists(it)) {
						it.UnpackPath(func,paths);
					}
				}
			}
			return paths;
		}

		public static string CopyToDirectory(this string thiz,string directory) {
			if(!string.IsNullOrEmpty(thiz)&&File.Exists(thiz)) {
				if(!Directory.Exists(directory)) {Directory.CreateDirectory(directory);}
				string dst=Path.Combine(directory,Path.GetFileName(thiz));
				File.Copy(thiz,dst,true);File.SetLastWriteTime(dst,File.GetLastWriteTime(thiz));
				thiz=dst;
			}
			return thiz;
		}

		public static void AddRange<T>(this IDictionary<string,T> thiz,string key,T value) {
			if(thiz!=null&&!string.IsNullOrEmpty(key)) {
				if(key.IndexOfAny(k_Split_Tag)>=0) {
					var tmp=key.Split(k_Split_Tag,System.StringSplitOptions.RemoveEmptyEntries);
					for(int i=0,imax=tmp.Length;i<imax;++i) {thiz[tmp[i]]=value;}
				}else {thiz[key]=value;}
			}
		}

		//

		public static bool IsTemporary(this Object thiz) {
			if(thiz!=null) {
				string tmp=thiz.name;
				return tmp?.StartsWith(s_TempTag)??false;
			}
			return false;
		}

		public static string GetFriendlyName(this Object thiz) {
			if(thiz!=null) {return thiz.GetType().Name+"@"+thiz.name;}
			else {return "null";}
		}

		public static void LoadSettings(this object thiz,string path) {
			if(thiz!=null) {
				string it;int imax=s_SettingPaths?.Length??0,i=path.IndexOf('.')>=0?imax:0;
				for(;i<imax;++i) {
					it=string.Format(s_SettingPaths[i],path).GetFullPath();
					//
					if(File.Exists(it)) {
						it=File.ReadAllText(it);
						if(!string.IsNullOrEmpty(it)) {JsonConvert.PopulateObject(it,thiz);}
						return;
					}
				}
				JObject jo=GetSettings();if(jo!=null)  {
					path=path.Replace(' ','_');JToken jt=jo.SelectToken(path);
					if(jt==null) {jo.SelectToken(thiz.GetType().Name);}
					if(jt!=null) {
						it=jt.ToString();
						if(!string.IsNullOrEmpty(it)) {JsonConvert.PopulateObject(it,thiz);}
						return;
					}
				}
			}
		}

		public static T AddMissingComponent<T>(this Component thiz) where T:Component {
			T tmp=null;
			if(thiz!=null) {
				tmp=thiz.GetComponent<T>();
				if(tmp==null) {tmp=thiz.gameObject.AddComponent<T>();}
			}
			return tmp;
		}

		public static T NewActor<T>(this Component thiz,string key) where T:Component {
			GameObject go=new GameObject(key);
			if(thiz!=null) {go.transform.SetParent(thiz.transform,false);}
			return go.AddComponent<T>();
		}

		public static void CheckInstance<T>(this Component thiz,string path,ref T value) where T:Object {
			if(thiz!=null&&value==null) {
				value=thiz.GetComponentInChildren<T>();
				if(value==null) {value=GetResourceInstance<T>(path);}
			}
		}

		public static void SetListener<T>(this System.Action<T> thiz,bool value) {
			System.Type key=typeof(T);
			if(!s_Events.TryGetValue(key,out var d)||d==null) {if(value) {d=thiz;}}
			else {d=value?System.Delegate.Combine(d,thiz):System.Delegate.Remove(d,thiz);}
			//
			if(d==null) {s_Events.Remove(key);}
			else {s_Events[key]=d;}
		}

		public static void InvokeEvent<T>(this T thiz) {
			if(s_Events.TryGetValue(typeof(T),out var d)&&d!=null) {
				((System.Action<T>)d)?.Invoke(thiz);
			}
		}

		  //

		public static void SetLoop(this VideoPlayer thiz,bool value) {
			if(thiz!=null) {
				thiz.isLooping=value;thiz.skipOnDrop=value;
			}
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

		public static bool ContainsScreenPoint(this RectTransform thiz,Vector2 point,bool value=false) {
			if(thiz!=null) {value=RectTransformUtility.RectangleContainsScreenPoint(thiz,point);}
			return value;
		}

		public static void AddTrigger(this EventTrigger thiz,EventTriggerType type,UnityAction<BaseEventData> action) {
			if(thiz!=null&&action!=null) {
				EventTrigger.Entry e;var tmp=thiz.triggers;
				int i=tmp.FindIndex(x=>x.eventID==type);
				if(i>=0) {e=tmp[i];}
				else {e=new EventTrigger.Entry();tmp.Add(e);}
				e.callback.AddListener(action);
			}
		}

		  //

		public static JObject GetSettings() {
			if(s_Settings==null) {
				string fn="settings.json";
				if(File.Exists(fn)) {s_Settings=JObject.Parse(File.ReadAllText(fn));}
				else {s_Settings=new JObject();}
			}
			return s_Settings;
		}

		#endregion Methods
	}
}
