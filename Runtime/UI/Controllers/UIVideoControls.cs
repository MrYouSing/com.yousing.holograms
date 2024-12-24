/* <!-- Macro.Include
../Private/ScriptableView.cs
 Macro.End --> */
/* <!-- Macro.Table Sprite
Play,
Pause,
Mute,
Unmute,
 Macro.End --> */
/* <!-- Macro.Table Text
Title,
Time,
 Macro.End --> */
/* <!-- Macro.Table Image
PlayOrPauseUI,
MuteOrUnmuteUI,
 Macro.End --> */
/* <!-- Macro.Table Button
PlayOrPause,
MuteOrUnmute,
Stop,
 Macro.End --> */
/* <!-- Macro.Table Toggle
Loop,
 Macro.End --> */
/* <!-- Macro.Table Slider
Progress,
Volume,
 Macro.End --> */

/* <!-- Macro.BatchCall DeclareKeys Sprite
 Macro.End --> */
/* <!-- Macro.BatchCall DeclareKeys Text
 Macro.End --> */
/* <!-- Macro.BatchCall DeclareKeys Image
 Macro.End --> */
/* <!-- Macro.BatchCall DeclareKeys Button
 Macro.End --> */
/* <!-- Macro.BatchCall DeclareKeys Toggle
 Macro.End --> */
/* <!-- Macro.BatchCall DeclareKeys Slider
 Macro.End --> */

/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */

/* <!-- Macro.Call Bind Button
 Macro.End --> */
/* <!-- Macro.Call BindSet Toggle
 Macro.End --> */
/* <!-- Macro.Call BindSet Slider
 Macro.End --> */
/* <!-- Macro.Replace
Button(Set,Button
 Macro.End --> */
/* <!-- Macro.Patch
,Start
 Macro.End --> */
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using YouSingStudio.Private;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="VideoPlayer"/><br/>
	/// <seealso href="https://www.w3school.com.cn/html5/att_video_controls.asp"/><br/>
	/// <seealso href="https://developer.mozilla.org/zh-CN/docs/Web/API/HTMLMediaElement/controls"/>
	/// </summary>
	public class UIVideoControls
		:ScriptableView
	{
// <!-- Macro.Patch AutoGen
		public const int k_Play=0;
		public const int k_Pause=1;
		public const int k_Mute=2;
		public const int k_Unmute=3;
#if UNITY_EDITOR
		protected virtual string[] GetSprites()=>new string[]{
			"Play",
			"Pause",
			"Mute",
			"Unmute",
		};
#endif

		public const int k_Title=0;
		public const int k_Time=1;
#if UNITY_EDITOR
		protected virtual string[] GetTexts()=>new string[]{
			"Title",
			"Time",
		};
#endif

		public const int k_PlayOrPauseUI=0;
		public const int k_MuteOrUnmuteUI=1;
#if UNITY_EDITOR
		protected virtual string[] GetImages()=>new string[]{
			"PlayOrPauseUI",
			"MuteOrUnmuteUI",
		};
#endif

		public const int k_PlayOrPause=0;
		public const int k_MuteOrUnmute=1;
		public const int k_Stop=2;
#if UNITY_EDITOR
		protected virtual string[] GetButtons()=>new string[]{
			"PlayOrPause",
			"MuteOrUnmute",
			"Stop",
		};
#endif

		public const int k_Loop=0;
#if UNITY_EDITOR
		protected virtual string[] GetToggles()=>new string[]{
			"Loop",
		};
#endif

		public const int k_Progress=0;
		public const int k_Volume=1;
#if UNITY_EDITOR
		protected virtual string[] GetSliders()=>new string[]{
			"Progress",
			"Volume",
		};
#endif

// Macro.Patch -->
		#region Fields

		public VideoPlayer video;
		public ushort track;
		public float rate=1.0f;
		public CanvasGroup canvas;

		[System.NonSerialized]protected bool m_Jump;
		[System.NonSerialized]protected float m_Time;
		[System.NonSerialized]protected List<ShortcutBehaviour> m_Shortcuts;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
// <!-- Macro.Patch Start
			BindButton(k_PlayOrPause,PlayOrPause);
			BindButton(k_MuteOrUnmute,MuteOrUnmute);
			BindButton(k_Stop,Stop);
			BindToggle(k_Loop,SetLoop);
			BindSlider(k_Progress,SetProgress);
			BindSlider(k_Volume,SetVolume);
// Macro.Patch -->
			if(m_Shortcuts==null) {
				m_Shortcuts=new List<ShortcutBehaviour>();
				this.GetComponentsInChildren(true,m_Shortcuts);
			}
			//
			if(canvas==null) {canvas=GetComponent<CanvasGroup>();}
			SetActive(false);
		}

		protected virtual void Update() {
			m_Time+=Time.deltaTime;
			if(m_Time>=rate) {
				m_Time-=rate;
				//
				if(video!=null) {UpdateControls();}
			}
		}

		protected virtual void OnDisable() {
			m_Time=0.0f;
		}

		#endregion Unity Messages

		#region Methods

		protected virtual bool IsVideo() {
			return video!=null&&!string.IsNullOrEmpty(video.url)&&video.isPrepared;
		}

		protected virtual bool IsPause() {
			return (video.isPaused||!video.isPlaying);
		}

		protected virtual void SetActive(bool value) {
			canvas.SetActive(value);
			//
			ShortcutBehaviour it;for(int i=0,imax=m_Shortcuts?.Count??0;i<imax;++i) {
				it=m_Shortcuts[i];if(it!=null) {it.enabled=value;}
			}
		}

		protected virtual void UpdateControls() {
			if(IsVideo()) {
				string str=video.url;
				float t=(float)video.time,d=(float)video.length;
				if(!string.IsNullOrEmpty(str)) {
					SetImage(k_PlayOrPauseUI,GetSprite(IsPause()?k_Play:k_Pause));
					SetImage(k_MuteOrUnmuteUI,GetSprite(video.GetDirectAudioMute(track)?k_Unmute:k_Mute));
					SetText(k_Title,Path.GetFileName(str));
					//
					SetToggleWithoutNotify(k_Loop,video.isLooping);
					SetText(k_Time,$"{t.ToTime()}/{d.ToTime()}");
					if(!m_Jump) {SetSliderWithoutNotify(k_Progress,t/d);}
					//
					SetToggleWithoutNotify(k_Mute,video.GetDirectAudioMute(track));
					SetSliderWithoutNotify(k_Volume,video.GetDirectAudioVolume(track));
					//
					SetActive(true);return;
				}
			}
			SetActive(false);
		}

		protected virtual void PlayOrPause() {
			if(IsVideo()) {
				if(IsPause()) {video.Play();}
				else {video.Pause();}
			}
		}

		protected virtual void MuteOrUnmute() {
			if(IsVideo()) {
				bool b=video.GetDirectAudioMute(track);
				video.SetDirectAudioMute(track,!b);
			}
		}

		protected virtual void Stop() {
			if(IsVideo()) {
				video.Stop();
			}
		}

		protected virtual void SetLoop(bool value) {
			if(IsVideo()) {
				video.isLooping=value;
			}
		}

		protected virtual void SetProgress(float value) {
			if(IsVideo()) {
				m_Jump=true;
				video.seekCompleted-=OnVideoSeek;
				video.seekCompleted+=OnVideoSeek;
				//
				video.time=value*video.length;
			}
		}

		protected virtual void OnVideoSeek(VideoPlayer vp) {
			vp.seekCompleted-=OnVideoSeek;
			//
			m_Jump=false;
		}

		protected virtual void SetMute(bool value) {
			if(IsVideo()) {
				video.SetDirectAudioMute(track,value);
			}
		}

		protected virtual void SetVolume(float value) {
			if(IsVideo()) {
				video.SetDirectAudioVolume(track,value);
			}
		}

		#endregion Methods
	}
}
