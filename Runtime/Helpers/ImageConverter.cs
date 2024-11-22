/* <!-- Macro.Table API
VideoPlayer,
FFmpeg,
 Macro.End --> */
/* <!-- Macro.Call  API
		public static int s_{0};
		public static void On{0}Convert(string path) {{
			if(!s_IsInited) {{Init();}}
			//
			++s_{0};
			Debug.Log(nameof({0})+" convert "+path);
		}}

 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using YouSingStudio.Private;
using Debug=UnityEngine.Debug;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="ImageConversion"/><br/>
	/// <seealso cref="System.BitConverter"/>
	/// </summary>
	public class ImageConverter {
// <!-- Macro.Patch AutoGen
		public static int s_VideoPlayer;
		public static void OnVideoPlayerConvert(string path) {
			if(!s_IsInited) {Init();}
			//
			++s_VideoPlayer;
			Debug.Log(nameof(VideoPlayer)+" convert "+path);
		}

		public static int s_FFmpeg;
		public static void OnFFmpegConvert(string path) {
			if(!s_IsInited) {Init();}
			//
			++s_FFmpeg;
			Debug.Log(nameof(FFmpeg)+" convert "+path);
		}

// Macro.Patch -->
		#region Nested Types

		public class Settings {
			public bool debug=
#if DEBUG
				true;
#else
				false;
#endif
			public string temporary;
			public string download;
			public string[] progresses;
			public string ffmpeg;
			public int maxTasks=4;
			public float taskWait=1.0f;
			public int videoEncoder=4000;
		}

		public class FFmpeg
			:AsyncTask<FFmpeg>
		{
			public static FFmpeg Obtain(string path,string argument,System.Action<string,string> action) {
				FFmpeg tmp=(FFmpeg)Obtain();
				tmp.delay=settings.taskWait;
				tmp.path=path;tmp.argument=argument;tmp.action=action;
				tmp.StartAsThread();return tmp;
			}

			public string path;
			public string argument;
			public System.Action<string,string> action;

			protected virtual void GetOutput(out string tmp,out string ext) {
				bool b=argument.EndsWith('"');int n=argument.Length;
				if(argument.IndexOf("{1}")>=0) {
					tmp=Path.Combine(settings.temporary,(id%settings.maxTasks).ToString("0000")).FixPath();
					int i=argument.LastIndexOf('.');ext=argument.Substring(i,n-i-(b?1:0));
				}else {
					char c=b?'"':' ';int i=argument.LastIndexOf(c,n-2);++i;
					tmp=argument.Substring(i,n-i-(b?1:0));ext=null;
				}
			}

			protected override void OnComplete() {
				int i=id;int tc=System.Environment.TickCount;
				GetOutput(out var tmp,out var ext);
				//
				Process p=new Process();var s=p.StartInfo;
					s.FileName=settings.ffmpeg;
					s.Arguments=string.Format(argument,path,tmp);
					s.UseShellExecute=false;
					s.CreateNoWindow=true;
				p.Start();p.WaitForExit();
				if(!string.IsNullOrEmpty(ext)) {tmp+=ext;}
				//
				if(id!=i||!File.Exists(tmp)) {
					OnKill();
				}else {
					if(settings.debug) {Debug.Log("FFmpeg uses "+((System.Environment.TickCount-tc)/1000f)+"s.");}
					//
					OnEvent(()=>action?.Invoke(path,tmp),true);
					OnFFmpegConvert(path);base.OnComplete();
				}
			}
		}

		#endregion Nested Types

		#region Fields

		public static Settings settings=new Settings();
		public static bool s_IsInited;

		#endregion Fields

		#region Methods

		//

		public static void FlipQuiltY(Texture2D texture,int rows) {
			if(texture!=null) {
				int xmax=texture.width,ymax=texture.height/rows;int x,y,j,k;
				Color[] tmp=texture.GetPixels();int a,b;Color c;
				for(int i=0,imax=rows/2;i<imax;++i) {
				j=i*ymax;k=(rows-1-i)*ymax;
				for(y=0;y<ymax;++y) {for(x=0;x<xmax;++x) {
					a=(j+y)*xmax+x;b=(k+y)*xmax+x;
					c=tmp[a];tmp[a]=tmp[b];tmp[b]=c;
				}}}
				texture.SetPixels(tmp);//texture.Apply();
			}
		}

		// Web

		public static bool IsError(UnityWebRequest www) {
			if(www==null) {return true;}
			try{
				if(!www.isDone) {
					if(settings.debug) {Debug.LogWarning("It is not done.");}
					return true;
				}
				if(!string.IsNullOrEmpty(www.error)) {
					if(settings.debug) {Debug.LogError(www.error);}
					return true;
				}
			}catch(System.Exception e) {
				if(settings.debug) {Debug.LogException(e);}
				return true;
			}
			return false;
		}

		public static void DownloadTexture(string url,System.Action<string> action,string path=null) {
			if(string.IsNullOrEmpty(url)||action==null) {return;}
			//
			UnityWebRequest www=UnityWebRequest.Get(url);
			if(string.IsNullOrEmpty(path)) {path=Path.Combine(GetDownloadPath(),"Images",Path.GetFileName(url));}
			var ao=www.SendWebRequest();SetProgress(www,ao);
			ao.completed+=(x)=>DownloadTexture(www,action,path);
		}

		public static void DownloadTexture(UnityWebRequest www,System.Action<string> action,string path) {
			if(string.IsNullOrEmpty(path)||action==null) {return;}
			//
			SetProgress(null,null);if(!IsError(www)) {
				string dir=Path.GetDirectoryName(path);
				if(!Directory.Exists(dir)) {Directory.CreateDirectory(dir);}
				//
				File.WriteAllBytes(path,www.downloadHandler.data);
				action?.Invoke(path);www.Dispose();// Clean up.
			}
		}

		/// <summary>
		/// <seealso href="https://blocks.glass/"/>
		/// </summary>
		public static void DownloadBlocksLKG(string id,System.Action<string> action) {
			if(string.IsNullOrEmpty(id)||action==null) {return;}
			//
			UnityWebRequest www=UnityWebRequest.Get("https://blocks.glass/embed/"+id);
			var ao=www.SendWebRequest();SetProgress(www,ao);
			ao.completed+=x=>DownloadBlocksLKG(www,action);
		}

		public static void DownloadBlocksLKG(UnityWebRequest www,System.Action<string> action) {
			SetProgress(null,null);if(IsError(www)||action==null) {return;}
			// Json
			string text=www.downloadHandler.text;www.Dispose();// Clean up.
			const string s="<script id=\"__NEXT_DATA__\" type=\"application/json\">";
			const string e="</script>";
			int i=text.IndexOf(s);if(i<0) {return;}i+=s.Length;
			int j=text.IndexOf(e,i);if(j<0) {return;}j-=i;
			text=text.Substring(i,j);JToken jo=JObject.Parse(text);float f;
			// Data
			jo=jo.SelectToken("props.pageProps.trpcState.json.queries[0].state.data");
			uint.TryParse(jo.SelectToken("id")?.Value<string>()??"0",out var l);
			i=jo.SelectToken("quiltCols")?.Value<int>()??(int)UnityExtension.s_DefaultQuilt.x;
			j=jo.SelectToken("quiltRows")?.Value<int>()??(int)UnityExtension.s_DefaultQuilt.y;
			f=jo.SelectToken("aspectRatio")?.Value<float>()??UnityExtension.s_DefaultQuilt.z;
			// Texture
			JArray list=(JArray)jo.SelectToken("sourceImages");jo=list[list.Count-1];
			string url=jo.SelectToken("url")?.Value<string>();if(string.IsNullOrEmpty(url)) {return;}
			UIProgressView.s_Size=jo.SelectToken("fileSize")?.Value<ulong>()??0;
			  // Rename
			text=Path.GetFileNameWithoutExtension(url);int k=text.LastIndexOf("_qs");
			if(k>=0&&char.IsDigit(text[k+3])) {text=text.Substring(0,k);}
			else {text=jo.SelectToken("title")?.Value<string>().ValidatePath()??text;}
			  // Fix
			text=$"{l.ToString("D8")}_{text}_qs{i}x{j}a{f.ToString("0.00")}{Path.GetExtension(url)}";
			DownloadTexture(url,action,Path.Combine(GetDownloadPath(),"blocks.glass",text));
		}

		public static bool ConvertFromWeb(string url,System.Action<string> action) {
			if(string.IsNullOrEmpty(url)||action==null) {return false;}
			//
			if(url.IndexOf("/blocks.glass/",UnityExtension.k_Comparison)>=0) {DownloadBlocksLKG(Path.GetFileName(url.Trim()),action);return true;}
			if(UnityExtension.IsImage(Path.GetExtension(url))) {DownloadTexture(url,action);return true;}
			return false;
		}

		public static void Init() {
			if(s_IsInited) {return;}
			s_IsInited=true;
			//
			settings.LoadSettings(nameof(ImageConverter));
			s_VideoPlayer=s_FFmpeg=0;
			// Create a temporary path.
			if(string.IsNullOrEmpty(settings.temporary)) {
				settings.temporary=Path.Combine(Application.temporaryCachePath,nameof(ImageConverter));
			}
			if(Directory.Exists(settings.temporary)) {Directory.Delete(settings.temporary,true);}
			settings.temporary=Path.Combine(settings.temporary,Random.Range(0,10000).ToString("0000"));
			Directory.CreateDirectory(settings.temporary);
			// Initialize FFmpeg environment.
			AsyncTask.GetBehaviour();
			if(string.IsNullOrEmpty(settings.ffmpeg)) {
				settings.ffmpeg=Path.Combine(Application.streamingAssetsPath,"FFmpegOut/Windows/ffmpeg.exe");
				if(!File.Exists(settings.ffmpeg)) {
					settings.ffmpeg=Path.Combine(UnityExtension.s_ExeRoot,"ffmpeg.exe");
				}
				if(((settings.videoEncoder>>16)&0xFFFF)==0) {
					settings.videoEncoder|=0x00050000;
				}
			}
			if(!File.Exists(settings.ffmpeg)) {settings.ffmpeg=null;}
			//
			if(string.IsNullOrEmpty(settings.download)) {
				settings.download=Path.Combine(Application.persistentDataPath,"Downloads");
			}
			if((settings.progresses?.Length??0)<=0) {
				settings.progresses=new string[]{
					"Prefabs/DownloadView"
				};
			}
			//
			Debug.Log("Init\n\ttmp:"+settings.temporary+"\n\tdownload:"+settings.download+"\n\tFFmpeg:"+settings.ffmpeg);
		}

		public static void Exit() {
			if(!s_IsInited) {return;}
			s_IsInited=false;
			//
			Debug.Log($"Exit {nameof(ImageConverter)}\n\t{nameof(VideoPlayer)}:{s_VideoPlayer}\n\t{nameof(FFmpeg)}:{s_FFmpeg}");
		}

		public static bool FFmpegSupported() {
			if(!s_IsInited) {Init();}
			//
			return !string.IsNullOrEmpty(settings.ffmpeg);
		}

		public static AsyncTask VideoToImage(string path,System.Action<string,string> action) {
			if(!s_IsInited) {Init();}
			//
			if(File.Exists(path)) {return FFmpeg.Obtain(path,"-i \"{0}\" -y \"{1}.png\"",action);}
			else {action?.Invoke(path,null);return null;}
		}

		public static void GetVideoEncoder(Vector2Int size,out string encoder) {
			if(!s_IsInited) {Init();}
			// Flag:[0:x264][1:x265][2:NPOT]
			string cv="h264_nvenc";int max=settings.videoEncoder&0xFFFF;
			int flag=(settings.videoEncoder>>16)&0xFFFF;
			if(size.x>=max||size.y>=max) {
				if((flag&0x2)==0) {
					if((flag&0x4)==0) {
						size.Set(max,max);
					}else {
						float f=max/(float)Mathf.Max(size.x,size.y);
						size.x=Mathf.RoundToInt(size.x*f);size.y=Mathf.RoundToInt(size.y*f);
					}
				}else {
					cv="hevc_nvenc";
				}
			}
			encoder=$"-vf scale={size.x}:{size.y} -c:v {cv} -pix_fmt yuv420p";
		}

		public static AsyncTask ImageToVideo(string path,Vector2Int size,string video) {
			if(!s_IsInited) {Init();}
			//
			if(File.Exists(path)) {
				GetVideoEncoder(size,out var encoder);
				return FFmpeg.Obtain(path,$"-i \"{{0}}\" {encoder} -r 1 -t 1 -y \"{video}\"",null);
			}else {
				return null;
			}
		}

		public static string GetDownloadPath() {
			if(!s_IsInited) {Init();}
			//
			return settings.download;
		}

		public static void SetProgress(UnityWebRequest www,AsyncOperation ao) {
			if(!s_IsInited) {Init();}
			//
			string str=settings.progresses[0];
			if(www!=null) {UIProgressView.Set(str,www,ao);}
			else {UIProgressView.Set(str,null);}
		}

		#endregion Methods
	}
}