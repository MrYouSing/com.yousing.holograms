using System.Collections.Generic;
using System.IO;
using UnityEngine;
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
			public string srcVideo;
			public string srcImage;
			public string dstImage;
			public string dstVideo;
			// Video
			public float framerate=-1.0f;
			public string encoder;
			public string bitrate;

			[System.NonSerialized]internal int m_Steps;
			[System.NonSerialized]internal int m_Frames;
			[System.NonSerialized]internal string m_Path;
			public virtual bool done=>(m_Steps&steps)==steps;
		}

		#endregion Nested Types

		#region Fields

		public VideoPlayer video;
		public HologramPlayer player;
		public UIHologramPreview preview;
		public DialogPicker[] pickers=new DialogPicker[3];
		public Convert convert=new Convert();

		[System.NonSerialized]protected bool m_Background;
		[System.NonSerialized]protected Convert m_Convert;
		[System.NonSerialized]protected Coroutine m_Coroutine;
		[System.NonSerialized]protected ImageConverter.FFmpeg m_Task;

		#endregion Fields

		#region Unity Messages
		#endregion Unity Messages

		#region Methods

		public static void CheckPath(ref string path,string key) {
			if(string.IsNullOrEmpty(path)) {
				path=Path.Combine(ImageConverter.settings.temporary,key);
			}
			if(Directory.Exists(path)) {
				try {Directory.Delete(path,true);}
				catch(System.Exception e) {Debug.LogException(e);}
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

		protected override void InitView() {
			if(!ImageConverter.FFmpegSupported()) {return;}
			base.InitView();
			//
			if(player==null) {player=FindAnyObjectByType<HologramPlayer>();}
			if(preview==null) {preview=FindAnyObjectByType<UIHologramPreview>();}
			//
			m_Convert=convert.Clone();
		}

		protected override void SetEvents(bool value) {
			base.SetEvents(value);
			//
			if(view!=null) {
				int i,imax;
				//
				for(i=0,imax=3;i<imax;++i) {view.BindToggle(i,OnToggle,true);}
				for(i=0,imax=2;i<imax;++i) {view.BindInputField(i,OnField,true);}
				view.BindButton(1,Execute,value);
				//
				int cnt=view.m_InputFields?.Length??0;
				Component comp=2<cnt?view.m_InputFields[2]:null;
				Button btn=comp!=null?comp.GetComponentInChildren<Button>():null;
				if(btn!=null) {
					if(value) {btn.SetOnClick(()=>ResetArgument(2));}
				}
			}
		}

		public override void SetView(bool value) {
			if(!ImageConverter.FFmpegSupported()) {return;}
			//
			if(value) {
				m_Background=ImageConverter.settings.background;
				ImageConverter.settings.background=false;
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
					view.SetActive(0,b);
				}
				//
				view.SetInputFieldWithoutNotify(0,convert.framerate.ToString());
				view.SetInputFieldWithoutNotify(1,convert.encoder);
				view.SetInputFieldWithoutNotify(2,convert.bitrate);
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
				if(float.TryParse(view.GetInputField(0),out var f)) {convert.framerate=f;}
				convert.encoder=view.GetInputField(1);
				convert.bitrate=view.GetInputField(2);
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

		public virtual void Busy(int step,int count) {
			if(count<0) {MessageBox.ShowInfo(view.m_Strings[0],view.m_Strings[1+step],null,Abort);}
			else {convert.m_Frames=count;MessageBox.ShowProgress(view.m_Strings[1+2*step],null,null,Abort);}
		}

		public virtual void Abort() {
			if(m_Coroutine!=null) {GetBehaviour().StopCoroutine(m_Coroutine);}
			m_Task?.Dispose();MessageBox.Clear();
			//
			m_Coroutine=null;m_Task=null;
			convert.m_Steps=-1;
		}

		public virtual void Execute() {
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
			string path=convert.m_Path;
			if(string.IsNullOrEmpty(path)) {path=convert.srcVideo;}
			//
			if(video!=null) {
				if(string.IsNullOrEmpty(path)) {path=video.url;}
				convert.m_Frames=(int)System.Math.Round(video.frameRate*video.length);
			}
			//
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
			Busy(0,convert.m_Frames);
			m_Task=ImageConverter.FFmpeg.Obtain(
				src,$"-y -i \"{{0}}\" \"{Path.Combine(dst,$"%08d{fmt}.png")}\"",
				(x,y)=>VideoToAudio(src,dst),(x)=>true
			);
		}

		protected virtual void VideoToAudio(string src,string dst) {
			m_Task=ImageConverter.FFmpeg.Obtain(
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
			string audio=Path.Combine(src,"audio.wav");
			if(File.Exists(audio)) {audio.CopyToDirectory(dst);}
			// TODO: RGB-D????
			string[] tmp=Directory.GetFiles(src,"*.*");
			Busy(1,-1+0*(tmp?.Length??0));
			m_Coroutine=GetBehaviour().StartCoroutine(ImageToRaw(tmp,Path.Combine(dst,"{0:D8}.png")));
		}

		protected virtual System.Collections.IEnumerator ImageToRaw(IList<string> paths,string path) {
			if(player!=null) {
				player.BeginCapture(GetDevice());
				string dst;
				for(int i=0,imax=paths.Count;i<imax;++i) {
					yield return player.OnCapture(paths[i]);
					dst=string.Format(path,i);
					//
					if(i==0) {SavePreview(Path.Combine(path.GetDirectoryName(),"preview.png"));}
					player.GetCapture().SaveFile(dst);
					
				}
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
			string tmp=Path.Combine(src,"preview.png");
			if(File.Exists(tmp)) {
				File.Copy(tmp,Path.Combine(Path.GetDirectoryName(dst),
					dst.GetFileName()+"_preview"+Path.GetExtension(tmp)),true);
			}
			tmp=Path.Combine(src,"audio.wav");
			if(!File.Exists(tmp)) {tmp=null;} else {tmp=$"-i \"{tmp}\" ";}
			//
			float f=convert.framerate;string pre=null;
			if(f>=0.0f) {pre=$"-r {f} ";}
			//
			Busy(2,-1);
			m_Task=ImageConverter.FFmpeg.Obtain(
				src,$"-y {pre}-i \"{Path.Combine(src,"%08d.png")}\" {GetEncoder()} {tmp}\"{dst}\"",
				(x,y)=>OnComplete(2,y),(x)=>true
			);
			
		}

		// Events

		protected virtual void OnToggle(bool value)=>Apply();
		protected virtual void OnField(string value)=>Apply();

		protected virtual void OnComplete(int step,string path) {
			Debug.Log($"Step@{step}:{path}");
			//
			m_Coroutine=null;m_Task=null;
			MessageBox.Clear();
			//
			convert.m_Path=path;
			convert.m_Steps&=1<<step;
			Execute(step+1);
		}

		#endregion Methods
	}
}
