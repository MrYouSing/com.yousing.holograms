using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YouSingStudio.Holograms {
	public static partial class UnityExtension {
		#region Fields

		public static Vector3 s_DefaultQuilt=new Vector3(8,5,0.5625f);
		public static Camera s_CameraHelper;

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

		public static void UnpackPaths(this List<string> thiz) {
			int i=0,imax=thiz?.Count??0;
			if(imax>0) {
				string[] tmp=thiz.ToArray();thiz.Clear();
				string it;for(;i<imax;++i) {
					it=tmp[i];if(File.Exists(it)){thiz.Add(it);}
					else if(Directory.Exists(it)) {thiz.AddRange(Directory.GetFiles(it));}
				}
			}
		}

		//

		public static void LoadSettings(this Object thiz,string path) {
			if(thiz!=null) {
			if(File.Exists(path)) {
				Newtonsoft.Json.JsonConvert.PopulateObject(File.ReadAllText(path),thiz);
			}}
		}

		public static void Begin(this RenderTexture thiz,out RenderTexture rt) {
			rt=RenderTexture.active;
				RenderTexture.active=thiz;
		}

		public static void End(this RenderTexture thiz,RenderTexture rt) {
			RenderTexture.active=rt;
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

		#endregion Methods
	}
}
