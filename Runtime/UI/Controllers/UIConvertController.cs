/* <!-- Macro.Define bFixPatch=
true
 Macro.End -->*/
/* <!-- Macro.Table InputField
string,srcVideo,,,,,
string,srcImage,,,,,
string,dstImage,,,,,
string,dstVideo,,,,,
float,framerate,=-1.0f,.ToString(),string s=null;float f=0.0f;if(view.GetInputField(i&#44;ref s)&&float.TryParse(s&#44;out f)) {convert.framerate=f;}++i;//,,
string,encoder,,,,,
string,bitrate,,,,,
 Macro.End -->*/

/* <!-- Macro.Call  InputField
			public {0} {1}{2};
 Macro.End -->*/
/* <!-- Macro.Patch
,Convert
 Macro.End -->*/

/* <!-- Macro.Call  InputField
				view.SetInputFieldWithoutNotify(i,convert.{1}{3});++i;
 Macro.End -->*/
/* <!-- Macro.Patch
,Render
 Macro.End -->*/

/* <!-- Macro.Call  InputField
				{4}view.GetInputField(i,ref convert.{1});++i;
 Macro.End -->*/
/* <!-- Macro.Patch
,Apply
 Macro.End -->*/
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UI;
using UnityEngine.Video;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	public class UIConvertController
		:UIBaseController
	{
		#region Nested Types

		[System.Serializable]
		public class Convert {
			/// <summary>
			/// 0x1:VideoToImage<br/>0x2:ImageToRaw<br/>0x4:RawToImage
			/// </summary>
			public int steps=0x7;
// <!-- Macro.Patch Convert
			public string srcVideo;
			public string srcImage;
			public string dstImage;
			public string dstVideo;
			public float framerate=-1.0f;
			public string encoder;
			public string bitrate;
// Macro.Patch -->
			[System.NonSerialized]internal int m_Steps;
			[System.NonSerialized]internal int m_Frames;
			[System.NonSerialized]internal string m_Path;
			public virtual bool done=>(m_Steps&steps)==steps;
		}

		#endregion Nested Types

		#region Fields

		public bool background;
		public VideoPlayer video;
		public HologramPlayer player;
		public UIHologramPreview preview;
		public DepthRenderer depth;
		public UIAspectSlider aspect;
		public DialogPicker[] pickers=new DialogPicker[3];
		public Convert convert=new Convert();

		[System.NonSerialized]protected bool m_Background;
		[System.NonSerialized]protected Convert m_Convert;
		[System.NonSerialized]protected Coroutine m_Coroutine;
		[System.NonSerialized]protected ImageConverter.FFmpeg m_Task;
		[System.NonSerialized]protected ImageConverter.Stream m_Stream;
		[System.NonSerialized]protected Vector4 m_Slider;
		[System.NonSerialized]protected UISliderView[] m_Sliders;

		#endregion Fields

		#region Unity Messages
		#endregion Unity Messages

		#region Methods

		public static void CheckPath(ref string path,string key) {
			if(string.IsNullOrEmpty(path)) {
				path=Path.Combine(ImageConverter.settings.temporary,key);
			}
			if(Directory.Exists(path)) {
				try {
					var tmp=Directory.GetFiles(path);
					if(tmp!=null) {System.Array.ForEach(tmp,File.Delete);}
				}catch(System.Exception e) {
					Debug.LogException(e);
				}
			}
		}

		public static bool CheckPaths(string src,string dst,int mask) {
			if(string.IsNullOrEmpty(src)||string.IsNullOrEmpty(dst)) {return false;}
			if((mask&0x1)!=0) {
				if(!Directory.Exists(src)) {return false;}
			}else {
				if(!File.Exists(src)) {return false;}
			}
			if((mask&0x2)==0) {dst=Path.GetDirectoryName(dst);}
			if(!Directory.Exists(dst)) {Directory.CreateDirectory(dst);}
			return true;
		}

		public static void CheckFile(string src,string dst,string file) {
			src=Path.Combine(src,file);
			if(File.Exists(src)) {File.Copy(src,Path.Combine(dst,file),true);}
		}

		protected override void InitView() {
			if(!ImageConverter.FFmpegSupported()) {return;}
			base.InitView();
			//
			var e=FindObjectsInactive.Include;
			if(video==null) {video=FindAnyObjectByType<VideoPlayer>(e);}
			if(player==null) {player=FindAnyObjectByType<HologramPlayer>(e);}
			if(preview==null) {preview=FindAnyObjectByType<UIHologramPreview>(e);}
			if(depth==null) {depth=FindAnyObjectByType<DepthRenderer>(e);}
			if(aspect==null) {aspect=FindAnyObjectByType<UIAspectSlider>(e);}
			//
			if(view!=null) {
				m_Sliders=view.m_Sliders.ToViews();
			}
			//
			m_Convert=convert.Clone();
		}

		protected override void SetEvents(bool value) {
			base.SetEvents(value);
			//
			if(view!=null) {
				int i,imax;
				//
				for(i=0,imax=3;i<imax;++i) {view.BindToggle(i,OnToggle,value);}
				for(i=0,imax=3;i<imax;++i) {view.BindInputField(i,OnField,value);}
				view.BindButton(1,Execute,value);
				//
				i=0;
				BindField(i,OnPick0,value);++i;
				BindField(i,OnPick1,value);++i;
				BindField(i,OnPick2,value);++i;
				BindField(i,OnPick3,value);++i;
				++i;
				++i;
				BindField(i,()=>ResetArgument(2),value);++i;
			}
		}

		public override void SetView(bool value) {
			if(!ImageConverter.FFmpegSupported()) {return;}
			//
			if(value) {
				m_Background=ImageConverter.settings.background;
				ImageConverter.settings.background=background;
				//
				EnableView();
			}else {
				ImageConverter.settings.background=m_Background;
			}
			base.SetView(value);
		}

		public override void Render() {
			if(view!=null) {
				int i,imax;bool b;
				//
				for(i=0,imax=3;i<imax;++i) {
					b=(convert.steps&(1<<i))!=0;
					view.SetToggleWithoutNotify(i,b);
					view.SetActive(i,b);
				}
				//
				i=0;
// <!-- Macro.Patch Render
				view.SetInputFieldWithoutNotify(i,convert.srcVideo);++i;
				view.SetInputFieldWithoutNotify(i,convert.srcImage);++i;
				view.SetInputFieldWithoutNotify(i,convert.dstImage);++i;
				view.SetInputFieldWithoutNotify(i,convert.dstVideo);++i;
				view.SetInputFieldWithoutNotify(i,convert.framerate.ToString());++i;
				view.SetInputFieldWithoutNotify(i,convert.encoder);++i;
				view.SetInputFieldWithoutNotify(i,convert.bitrate);++i;
// Macro.Patch -->
			}
		}

		public override void Apply() {
			if(view!=null) {
				int i,imax;
				//
				convert.steps=0;
				for(i=0,imax=3;i<imax;++i) {
					if(view.GetToggle(i)) {convert.steps|=1<<i;}
				}
				//
				i=0;
// <!-- Macro.Patch Apply
				view.GetInputField(i,ref convert.srcVideo);++i;
				view.GetInputField(i,ref convert.srcImage);++i;
				view.GetInputField(i,ref convert.dstImage);++i;
				view.GetInputField(i,ref convert.dstVideo);++i;
				string s=null;float f=0.0f;if(view.GetInputField(i,ref s)&&float.TryParse(s,out f)) {convert.framerate=f;}++i;//view.GetInputField(i,ref convert.framerate);++i;
				view.GetInputField(i,ref convert.encoder);++i;
				view.GetInputField(i,ref convert.bitrate);++i;
// Macro.Patch -->
			}
		}

		public virtual void Pick(int index,System.Action<string> action,string path=null) {
			//
			if(!string.IsNullOrEmpty(path)) {
				action?.Invoke(path);return;
			}
			//
			var p=pickers[index];
			if(p!=null) {
				p.onPicked=action;
				p.ShowDialog();
			}
		}

		public virtual void Busy(int step,string path,int count) {
			if(count<0) {MessageBox.ShowInfo(view.m_Strings[0],view.m_Strings[1+step],null,Abort);}
			else {convert.m_Frames=count;MessageBox.ShowProgress(view.m_Strings[4+step],path,"*.png",-1,count,Abort);}
		}

		public virtual void Abort() {
			if(m_Coroutine!=null) {GetBehaviour().StopCoroutine(m_Coroutine);}
			m_Task?.Dispose();MessageBox.Clear();
			//
			m_Coroutine=null;m_Task=null;m_Stream=null;
			convert.m_Steps=-1;
		}

		public virtual void Execute() {
			Abort();
			convert.m_Steps=0;
			convert.m_Frames=-1;
			convert.m_Path=null;
			Execute(0);
		}

		protected virtual MonoBehaviour GetBehaviour()=>view!=null?view:this;

		protected virtual HologramDevice GetDevice() {
			if(MonoApplication.s_Instance!=null&&MonoApplication.s_Instance.device!=null) {
				return MonoApplication.s_Instance.device;
			}
			return player.player.device;
		}

		protected virtual string GetEncoder() {
			Vector2Int v=GetDevice().resolution;
			//
			string c=convert.encoder;string b=convert.bitrate;
			if(string.IsNullOrEmpty(b)) {b=m_Stream?.bitrate??null;}
			//
			if(string.IsNullOrEmpty(c)) {ImageConverter.GetVideoEncoder(v,out c);}
			if(!string.IsNullOrEmpty(b)) {b=" -b:v "+b;}
			return c+b;
		}

		protected virtual void ResetArgument(int index) {
			if(view!=null) {
			switch(index) {
				case 2:
					view.SetInputField(2,m_Convert.bitrate);
				break;
			}}
		}

		protected virtual void SavePreview(string path) {
			if(preview!=null&&preview.screen!=null) {
				preview.screen.Render();
				preview.screen.Screenshot(path);
			}
		}

		protected virtual void BindField(int index,UnityAction action,bool value) {
			if(view!=null&&index>=0&&index<(view.m_InputFields?.Length??0)) {
				Component com=view.m_InputFields[index];
				Button btn=com!=null?com.GetComponentInChildren<Button>():null;
				if(btn!=null) {
					if(value) {btn.SetOnClick(action);}
					else {btn.SetOnClick(null);}
				}
			}
		}

		protected virtual void EnableView() {
			if(view==null) {return;}
			//
			m_Slider=new Vector4(1.0f,0.0f,0.0f,0.0f);
			if(depth!=null&&depth.isActiveAndEnabled) {
				m_Slider.x=depth.factor.x;
				m_Slider.y=depth.factor.y;
			}
			if(aspect!=null&&aspect.isActiveAndEnabled) {
				m_Slider.z=aspect.GetValue(0.0f);
			}
			for(int i=0,imax=m_Sliders?.Length??0;i<imax;++i) {m_Sliders[i].SetValueWithoutNotify(m_Slider[i]);}
			//
			string path=video!=null?video.url:null;
			if(!string.IsNullOrEmpty(path)) {
				convert.srcVideo=path;
				convert.srcImage=Path.Combine(ImageConverter.settings.temporary,"VideoToImage");
				convert.dstImage=Path.Combine(ImageConverter.settings.temporary,"ImageToRaw");
				convert.dstVideo=Path.Combine(Path.GetDirectoryName(path),path.GetFileName()+"_raw.mp4");
			}else {
				convert.srcVideo=
				convert.srcImage=
				convert.dstImage=
				convert.dstVideo=null;
			}
		}

		protected virtual void EnableSliders(string key) {
			if(view==null) {return;}
			//
			Vector4 slider=new Vector4(1.0f,0.0f,0.0f,0.0f);Slider s;
			for(int i=0,imax=Mathf.Min(view.m_Sliders?.Length??0,4);i<imax;++i) {
				s=view.m_Sliders[i];if(s!=null) {slider[i]=s.value;}
			}
			//
			QuiltRenderer.s_SliderValue=slider.z;
			DepthRenderer.s_Sliders[key]=slider.z;
			DepthRenderer.s_Vectors[key]=new Vector4(slider.x,slider.y,0.0f,1.0f);
		}

		protected virtual void DisableSliders(string key) {
			QuiltRenderer.s_SliderValue=0.0f;
			DepthRenderer.s_Sliders.Remove(key);
			DepthRenderer.s_Vectors.Remove(key);
		}

		protected virtual void Execute(string path,string argument,System.Action<string,string> action,System.Func<string,bool> func) {
			var b=GetBehaviour();
			if(m_Coroutine!=null) {b.StopCoroutine(m_Coroutine);}
			//
			m_Coroutine=b.StartCoroutine(ExecuteDelayed(path,argument,action,func));
		}

		protected virtual System.Collections.IEnumerator ExecuteDelayed(string path,string argument,System.Action<string,string> action,System.Func<string,bool> func) {
			yield return null;
			m_Task=ImageConverter.FFmpeg.Obtain(path,argument,action,func);
		}

		protected virtual void Execute(int step) {
			if(convert.done) {/*OnConvert()*/;return;}
			int m=convert.steps&(1<<step);
			if((convert.m_Steps&m)==m) {Execute(step+1);return;}
			//
			switch(step) {
				case 0:VideoToImage();break;
				case 1:ImageToRaw();break;
				case 2:RawToVideo();break;
			}
		}

		protected virtual void VideoToImage() {
			string path=convert.m_Path;int cnt=-1;
			if(string.IsNullOrEmpty(path)) {path=convert.srcVideo;}
			//
			if(video!=null) {
				if(string.IsNullOrEmpty(path)) {path=video.url;}
				cnt=(int)System.Math.Round(video.frameRate*video.length);
				if(cnt>1) {++cnt;}
			}
			//
			convert.m_Frames=cnt;
			Pick(0,(x)=>{Pick(3,(y)=>{
				VideoToImage(x,y);
			},convert.srcImage);},path);
		}

		protected virtual void VideoToImage(string src,string dst) {
			CheckPath(ref dst,"VideoToImage");
			if(!CheckPaths(src,dst,0x2)) {return;}
			//
			string fmt=src.GetFileExtension();
			if(fmt.StartsWith('_')) {fmt=Path.GetFileNameWithoutExtension(fmt);}
			else {fmt=GetDevice().ParseQuilt().ToQuilt_LKG();}
			//
			Busy(0,dst,convert.m_Frames);
			ImageConverter.BeginStreams();
			Execute(
				src,$"-y -i \"{{0}}\" -report \"{Path.Combine(dst,$"%08d{fmt}.png")}\"",
				(x,y)=>OnVideoToImage(src,dst),(x)=>true
			);
		}

		protected virtual void OnVideoToImage(string src,string dst) {
			m_Stream=null;bool a=false;
			//
			using(ListPool<ImageConverter.Stream>.Get(out var list)) {
				ImageConverter.EndStreams(list);ImageConverter.Stream it;
				for(int i=0,imax=list.Count;i<imax;++i) {
					it=list[i];switch(it.type) {
						case "audio":a=true;break;
						case "video":if(m_Stream==null) {m_Stream=it;}break;
					}
				}
			}
			//
			if(m_Stream!=null) {
				File.WriteAllText(Path.Combine(dst,"stream.json"),JsonUtility.ToJson(m_Stream,true));
			}
			if(a) {VideoToAudio(src,dst);}else {OnComplete(0,dst);}
		}

		protected virtual void VideoToAudio(string src,string dst) {
			Execute(
				src,$"-y -i \"{{0}}\" \"{Path.Combine(dst,"audio.wav")}\"",
				(x,y)=>OnComplete(0,y.GetDirectoryName()),(x)=>true
			);
		}

		protected virtual void ImageToRaw() {
			string path=convert.m_Path;
			if(string.IsNullOrEmpty(path)) {path=convert.srcImage;}
			//
			Pick(1,(x)=>{Pick(3,(y)=>{
				ImageToRaw(x,y);
			},convert.dstImage);},path);
		}

		protected virtual void ImageToRaw(string src,string dst) {
			CheckPath(ref dst,"ImageToRaw");
			if(!CheckPaths(src,dst,0x3)) {return;}
			//
			CheckFile(src,dst,"audio.wav");
			CheckFile(src,dst,"stream.json");
			// TODO: RGB-D????
			string[] tmp=Directory.GetFiles(src,"*.png");
			Busy(1,dst,tmp?.Length??0);
			m_Coroutine=GetBehaviour().StartCoroutine(ImageToRaw(tmp,Path.Combine(dst,"{0:D8}.png")));
		}

		protected virtual System.Collections.IEnumerator ImageToRaw(IList<string> paths,string path) {
			yield return null;
			if(player!=null) {
				string key="Temp"+path.GetFileExtension(),tmp;
				//
				EnableSliders(key);
				player.BeginCapture(GetDevice());
				//
				for(int i=0,imax=paths.Count;i<imax;++i) {
					if(m_Coroutine==null) {break;}
					//
					yield return player.OnCapture(paths[i]);
					tmp=string.Format(path,i);
					//
					if(i==0) {SavePreview(Path.Combine(path.GetDirectoryName(),"preview.png"));}
					player.GetCapture().SaveFile(tmp);
				}
				//
				DisableSliders(key);
				player.EndCapture();
			}
			OnComplete(1,path.GetDirectoryName());
		}

		protected virtual void RawToVideo() {
			string path=convert.m_Path;
			if(string.IsNullOrEmpty(path)) {path=convert.dstImage;}
			//
			Pick(1,(x)=>{Pick(2,(y)=>{
				RawToVideo(x,y);
			},convert.dstVideo);},path);
		}

		protected virtual void RawToVideo(string src,string dst) {
			if(!CheckPaths(src,dst,0x1)) {return;}
			//
			string tmp=Path.Combine(src,"stream.json");
			if(m_Stream==null&&File.Exists(tmp)) {
				m_Stream=JsonUtility.FromJson<ImageConverter.Stream>(File.ReadAllText(tmp));
			}
			tmp=Path.Combine(src,"preview.png");
			if(File.Exists(tmp)) {
				File.Copy(tmp,Path.Combine(Path.GetDirectoryName(dst),
					dst.GetFileName()+"_preview"+Path.GetExtension(tmp)),true);
			}
			tmp=Path.Combine(src,"audio.wav");
			if(!File.Exists(tmp)) {tmp=null;} else {tmp=$"-i \"{tmp}\" ";}
			//
			float f=convert.framerate;string pre=null;
			if(f<0.0f&&m_Stream.framerate!=0.0f) {f=m_Stream.framerate;}
			if(f>=0.0f) {pre=$"-r {f} ";}
			//
			Busy(2,null,-1);
			Execute(
				src,$"-y {pre}-i \"{Path.Combine(src,"%08d.png")}\" {GetEncoder()} {tmp}\"{dst}\"",
				(x,y)=>OnComplete(2,y),(x)=>true
			);
			
		}

		// Events

		protected virtual void OnToggle(bool value) {Apply();Render();}
		protected virtual void OnField(string value)=>Apply();

		protected virtual void OnPick0()=>OnPick(0,0);
		protected virtual void OnPick1()=>OnPick(1,3);
		protected virtual void OnPick2()=>OnPick(2,3);
		protected virtual void OnPick3()=>OnPick(3,2);

		protected virtual void OnPick(int index,int type) {
			Pick(type,(x)=>{
				if(string.IsNullOrEmpty(x)) {return;}
				view.SetInputFieldWithoutNotify(index,x);
				Apply();
			});
		}

		protected virtual void OnComplete(int step,string path) {
			Debug.Log($"Step@{step}:{path}");
			//
			m_Coroutine=null;m_Task=null;m_Stream=null;
			MessageBox.Clear();
			//
			convert.m_Path=path;
			convert.m_Steps&=1<<step;
			Execute(step+1);
		}

		#endregion Methods
	}
}
