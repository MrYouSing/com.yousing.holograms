/* <!-- Macro.Include
../Private/ScriptableView.cs
 Macro.End --> */
/* <!-- Macro.Table Sprite
Play,
Pause,
 Macro.End --> */
/* <!-- Macro.Table Text
Title,
Time,
 Macro.End --> */
/* <!-- Macro.Table Image
PlayOrPauseUI,
 Macro.End --> */
/* <!-- Macro.Table Button
PlayOrPause,
Stop,
 Macro.End --> */
/* <!-- Macro.Table Toggle
Loop,
Mute,
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
#if UNITY_EDITOR
		protected virtual string[] GetSprites()=>new string[]{
			"Play",
			"Pause",
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
#if UNITY_EDITOR
		protected virtual string[] GetImages()=>new string[]{
			"PlayOrPauseUI",
		};
#endif

		public const int k_PlayOrPause=0;
		public const int k_Stop=1;
#if UNITY_EDITOR
		protected virtual string[] GetButtons()=>new string[]{
			"PlayOrPause",
			"Stop",
		};
#endif

		public const int k_Loop=0;
		public const int k_Mute=1;
#if UNITY_EDITOR
		protected virtual string[] GetToggles()=>new string[]{
			"Loop",
			"Mute",
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

		[System.NonSerialized]protected float m_Time;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
// <!-- Macro.Patch Start
			BindButton(k_PlayOrPause,PlayOrPause);
			BindButton(k_Stop,Stop);
			BindToggle(k_Loop,SetLoop);
			BindToggle(k_Mute,SetMute);
			BindSlider(k_Progress,SetProgress);
			BindSlider(k_Volume,SetVolume);
// Macro.Patch -->
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

		protected virtual void UpdateControls() {
			if(!IsVideo()) {return;}
			//
			string str=video.url;
			float t=(float)video.time,d=(float)video.length;
			if(!string.IsNullOrEmpty(str)) {
				SetImage(k_PlayOrPauseUI,GetSprite(video.isPaused?k_Play:k_Pause));
				SetText(k_Title,Path.GetFileName(str));
				//
				SetToggleWithoutNotify(k_Loop,video.isLooping);
				SetText(k_Time,$"{t.ToTime()}/{d.ToTime()}");
				SetSliderWithoutNotify(k_Progress,t/d);
				//
				SetToggleWithoutNotify(k_Mute,video.GetDirectAudioMute(track));
				SetSliderWithoutNotify(k_Volume,video.GetDirectAudioVolume(track));
			}
		}

		protected virtual void PlayOrPause() {
			if(IsVideo()) {
				if(video.isPaused) {video.Play();}
				else {video.Pause();}
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
				video.time=value*video.length;
			}
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
