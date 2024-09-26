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
using UnityEngine;
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
			public string ffmpeg;
			public int maxTasks=4;
			public float taskWait=1.0f;
		}

		public class FFmpeg
			:AsyncTask<FFmpeg>
		{
			public static FFmpeg Obtain(string path,System.Action<string,string> action) {
				FFmpeg tmp=(FFmpeg)Obtain();
				tmp.delay=settings.taskWait;
				tmp.path=path;tmp.action=action;
				tmp.StartAsThread();return tmp;
			}

			public string path;
			public System.Action<string,string> action;

			public override void OnComplete() {
				int i=id;string t=Path.Combine(settings.temporary,(i%settings.maxTasks).ToString("0000")+".png");
				//if(File.Exists(t)) {File.Delete(t);}
				int tc=System.Environment.TickCount;
				//
				Process p=new Process();var s=p.StartInfo;
				s.FileName=settings.ffmpeg;
				s.Arguments=$"-i {path} -y {t}";
				s.UseShellExecute=false;
				s.CreateNoWindow=true;
				p.Start();p.WaitForExit();
				//
				if(id!=i||!File.Exists(t)) {
					OnKill();
				}else {
					if(settings.debug) {Debug.Log("FFmpeg uses "+((System.Environment.TickCount-tc)/1000f)+"s.");}
					//
					OnEvent(()=>action?.Invoke(path,t),true);
					OnFFmpegConvert(path);base.OnComplete();
				}
			}

			public override void OnKill() {
				base.OnKill();
			}
		}

		#endregion Nested Types

		#region Fields

		public static Settings settings=new Settings();
		public static bool s_IsInited;

		#endregion Fields

		#region Methods

		public static void Init() {
			if(s_IsInited) {return;}
			s_IsInited=true;
			//
			settings.LoadSettings(nameof(ImageConverter)+".json");
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
			}
			if(!File.Exists(settings.ffmpeg)) {settings.ffmpeg=null;}
			//
			Debug.Log("Init\n\ttmp:"+settings.temporary+"\n\texe:"+settings.ffmpeg);
		}

		public static void Exit() {
			if(!s_IsInited) {return;}
			s_IsInited=false;
			//
			Debug.Log($"Exit {nameof(ImageConverter)}\n\t{nameof(VideoPlayer)}:{s_VideoPlayer}\n\t{nameof(FFmpeg)}:{s_FFmpeg}");
		}

		public static bool IsSupported() {
			if(!s_IsInited) {Init();}
			//
			return !string.IsNullOrEmpty(settings.ffmpeg);
		}

		public static System.IDisposable VideoToImage(string path,System.Action<string,string> action) {
			if(!s_IsInited) {Init();}
			//
			if(File.Exists(path)) {return FFmpeg.Obtain(path,action);}
			else  {action?.Invoke(path,null);return null;}
		}

		#endregion Methods
	}
}