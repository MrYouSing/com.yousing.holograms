using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YouSingStudio.Holograms {
	public class UIAnimationDirector
		:MonoBehaviour
	{
		#region Nested Types

		public class Listener
			:MonoBehaviour
		{
			public UIAnimationDirector context;
			public virtual void OnLoopPointReached() {
				if(context!=null) {context.OnLoopPointReached();}
			}
		}

		#endregion Nested Types

		#region Fields

		public new Animation animation;
		public Animator animator;
		public List<string> animations=new List<string>();
		[Header("UI")]
		// Button
		public Text text;
		public Image image;
		// Timeline
		public Slider slider;
		public Text time;
		[SerializeField]protected string[] m_Times=new string[]{"{0}/{1}","{1:00}:{2:00}"};
		public Button[] buttons;
		// Menu
		public UIPopupView popup;
		public UIWrapMode wrap;
		public UIFloatSelector speed;
		public UISelectorView selector;

		[System.NonSerialized]public System.IProgress<string> progress;
		[System.NonSerialized]protected bool m_IsPlaying;
		[System.NonSerialized]protected int m_Index;
		/// <summary>
		/// 0x0:Default<br/>0x1:Current<br/>0x2:Wait
		/// </summary>
		[System.NonSerialized]protected byte m_State;
		[System.NonSerialized]protected int m_Hash;
		[System.NonSerialized]protected bool m_IsJumping;
		[System.NonSerialized]protected GameObject m_PopupV;
		[System.NonSerialized]protected GameObject m_SliderV;
		[System.NonSerialized]protected GameObject[] m_ButtonsV;

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			UnityExtension.SetListener<TextureType>(OnTypeChanged,true);
			UnityExtension.SetListener<ModelLoader>(OnModelLoaded,true);
			//
			if(popup==null) {popup=GetComponentInChildren<UIPopupView>();}
			if(wrap==null) {wrap=GetComponentInChildren<UIWrapMode>();}
			if(speed==null) {speed=GetComponentInChildren<UIFloatSelector>();}
			if(selector==null) {selector=GetComponentInChildren<UISelectorView>();}
			//
			if(popup!=null) {m_PopupV=popup.gameObject;popup.onActive+=OnPopupChanged;}
			if(wrap!=null) {wrap.onValueChanged+=OnWrapChanged;}
			if(speed!=null) {speed.onValueChanged+=OnSpeedChanged;}
			if(selector!=null) {selector.onSelect+=Play;}
			//
			if(slider!=null) {
				m_SliderV=slider.gameObject;
				slider.onValueChanged.AddListener(JumpTo);
				//
				var et=slider.AddMissingComponent<EventTrigger>();
				et.AddTrigger(EventTriggerType.BeginDrag,BeginJump);
				et.AddTrigger(EventTriggerType.EndDrag,EndJump);
				et.AddTrigger(EventTriggerType.PointerDown,BeginJump);
				et.AddTrigger(EventTriggerType.PointerUp,EndJump);
			}
			int i=0,imax=buttons?.Length??0;m_ButtonsV=new GameObject[imax];
			SetButton(0,Pause);SetButton(1,Resume);
		}

		protected virtual void Update() {
			if(m_Hash!=0) {UpdateAnimator();}
			if(m_Index>=0) {UpdateSlider();}
		}

		protected virtual void OnDestroy() {
			UnityExtension.SetListener<TextureType>(OnTypeChanged,false);
			UnityExtension.SetListener<ModelLoader>(OnModelLoaded,false);
		}

		#endregion Unity Messages

		#region Methods

		public virtual AnimationState animationState {
			get=>animation!=null&&m_Index>=0?animation[animations[m_Index]]:null;
		}

		public virtual AnimatorStateInfo animatorState {
			get {
				if(animator!=null) {
					if(animator.IsInTransition(0)) {return animator.GetNextAnimatorStateInfo(0);}
					else {return animator.GetCurrentAnimatorStateInfo(0);}
				}
				return default;
			}
		}

		public virtual void Clear() {
			//
			Popup(false);
			animation=null;
			animator=null;
			progress=null;
			animations.Clear();
			Button(null,null);
			//
			if(m_SliderV!=null) {m_SliderV.SetActive(false);}
			if(time!=null) {time.text=null;}
			SetPlay(0);
			//
			m_IsPlaying=false;
			m_Index=-1;
			m_State=0x0;
			m_Hash=0;
		}

		public virtual void Load(GameObject actor) {
			if(actor==null) {return;}
			//
			animation=actor.GetComponentInChildren<Animation>();
			if(animation!=null) {
				Popup(true);
				//
				Listener l=animation.GetComponent<Listener>();
				if(l==null) {l=animation.gameObject.AddComponent<Listener>();}
				l.context=this;
				//
				animation.wrapMode=WrapMode.ClampForever;
				foreach(AnimationState it in animation) {
					OnClipLoaded(it.clip);
					animations.Add(it.name);
				}
				return;
			}
			animator=actor.GetComponentInChildren<Animator>();
			if(animator!=null) {
				Popup(true);
				//
				var tmp=animator.parameters;AnimatorControllerParameter it;
				for(int i=0,imax=tmp?.Length??0;i<imax;++i) {
					it=tmp[i];if(it.type==AnimatorControllerParameterType.Trigger) {
						animations.Add(it.name);
					}
				}
				return;
			}
			//
			progress=actor.GetComponentInChildren<System.IProgress<string>>();
			if(progress!=null) {
				Popup(true);
				if(progress is IEnumerable<string> tmp) {
					animations.AddRange(tmp);return;
				}
			}
		}

		public virtual void Popup(bool value) {
			if(popup!=null) {
				m_PopupV.SetActive(value);
				popup.SetButton(value);
			}
		}

		public virtual void Button(string txt,Sprite img) {
			if(text!=null) {text.text=txt;}
			if(image!=null) {image.sprite=img;image.enabled=img!=null;}
		}

		public virtual void Render() {
			if(wrap!=null) {
				wrap.SetValueWithoutNotify(wrap.values[wrap.start]);
			}
			if(speed!=null) {
				speed.SetValueWithoutNotify(speed.values[speed.start]);
			}
			if(selector!=null) {
				selector.Render(animations);
			}
		}

		public virtual void Play(int index) {
			if(index>=0&&index<animations.Count) {
				m_Index=index;string key=animations[m_Index];
				if(selector!=null&&(popup==null||popup.GetActive())) {selector.Highlight(m_Index);}// TODO: ToExtension????
				//
				if(animation!=null) {
					animation.Play(key);
					var tmp=animationState;if(tmp!=null) {tmp.speed=1.0f;}
				}
				if(animator!=null) {
					//
					if(m_Index==0&&m_Hash==0) {
						m_Hash=animatorState.shortNameHash;
					}
					animator.enabled=true;animator.speed=1.0f;m_State=0x0;
					//
					if(m_Index>0&&animatorState.shortNameHash!=m_Hash) {
						animator.SetTrigger(animations[0]);m_State=0x2;
					}
					animator.SetTrigger(key);
				}
				if(progress!=null) {
					progress.Report(key);
				}
				//
				m_IsPlaying=true;
				Button(key,null);
				OnSpeedChanged(GetSpeed(false));
			}
		}

		public virtual void Stop() {
			if(animation!=null) {animation.Stop();}
			if(animator!=null) {animator.enabled=false;}
			if(progress!=null) {progress.Report(null);}
		}

		public virtual void Resume() {
			if(m_Index<0) {return;}
			//
			SetSpeed(GetSpeed(false));
		}

		public virtual void Pause() {
			if(m_Index<0) {return;}
			//
			SetSpeed(0.0f);
		}

		public virtual void Seek(float position) {
			if(m_Index<0) {return;}
			//
			var tmp=animationState;
			if(tmp!=null) {tmp.time=position;}
		}

		protected virtual float GetSpeed(bool realtime) {
			if(realtime) {
				var tmp=animationState;
				if(tmp!=null) {return tmp.speed;}
				else if(animator!=null) {return animator.speed;}
				return 0.0f;
			}else {
				return speed!=null?speed.value:1.0f;
			}
		}

		protected virtual void SetSpeed(float value) {
			var tmp=animationState;
			if(tmp!=null) {tmp.speed=value;}
			else if(animator!=null) {animator.speed=value;}
		}

		// Timeline

		protected virtual void SetButton(int index,UnityAction action) {
			var tmp=buttons[index];if(tmp!=null) {
				m_ButtonsV[index]=tmp.gameObject;
				tmp.onClick.AddListener(action);
			}
		}

		protected virtual void UpdateAnimator() {
			if(animator==null||!animator.isActiveAndEnabled||animator.speed==0.0f) {return;}
			//
			var si=animatorState;
			bool a=si.shortNameHash==m_Hash,b=false;
			if(!animator.IsInTransition(0)) {
				//
				if(!a&&m_State==0x0&&si.normalizedTime>=0.5f) {
					m_State=0x1;return;
				}
				//
				if(m_Index==0) {b=si.normalizedTime>=1.0f;}
				else if(m_State==0x1&&a) {b=true;}
				else if(si.loop) {b=si.normalizedTime>=1.0f;}
			}else {
				switch(m_State) {
					case 0x2:if(a) {m_State=0x0;}break;
				}
			}
			//
			if(b) {OnLoopPointReached();}
		}

		protected virtual void UpdateSlider() {
			//
			float t=0.0f,d=1.0f;bool b=false;
			var tmp=animationState;
			if(tmp!=null) {
				t=tmp.time;d=tmp.length;b=tmp.speed>0.0f;
				t=Mathf.Clamp(t,0.0f,d);
			}else if(animator!=null) {
				var si=animatorState;d=si.length;b=animator.speed>0.0f;
				if(float.IsInfinity(d)||!b) {SetPlay(0x2);return;}
				t=d*Mathf.Repeat(si.normalizedTime,1.0f);
			}else {
				return;
			}
			//
			if(slider!=null) {
				m_SliderV.SetActive(true);
				if(!m_IsJumping) {slider.SetValueWithoutNotify(t/d);}
			}
			if(time!=null) {
				time.text=string.Format(m_Times[0],
					UnityExtension.ToTime(t,m_Times[1]),
					UnityExtension.ToTime(d,m_Times[1])
				);
			}
			SetPlay(b?0x1:0x2);
		}

		protected virtual void SetPlay(int button) {
			GameObject it;int i=0,imax=m_ButtonsV?.Length??0;
			for(;i<imax;++i) {
				it=m_ButtonsV[i];if(it!=null) {it.SetActive((button&(1<<i))!=0);}
			}
		}

		protected virtual void JumpTo(float value) {
			var tmp=animationState;
			if(tmp!=null) {
				Seek(tmp.length*value);
			}
		}

		protected virtual void BeginJump(BaseEventData e)=>m_IsJumping=true;
		protected virtual void EndJump(BaseEventData e)=>m_IsJumping=false;

		// Events

		protected virtual void OnTypeChanged(TextureType type) {
			Clear();
		}

		protected virtual void OnModelLoaded(ModelLoader model) {
			Clear();
			Load(model.GetActor());
			Render();
			Play(0);
		}

		protected virtual void OnPopupChanged(bool value) {
			if(value) {
				if(selector!=null) {selector.Highlight(m_Index);}
			}
		}

		protected virtual void OnClipLoaded(AnimationClip clip) {
			if(clip!=null) {
				clip.wrapMode=WrapMode.ClampForever;
				//
				var e=new AnimationEvent();
				e.time=clip.length;
				e.functionName="OnLoopPointReached";
				e.messageOptions=SendMessageOptions.DontRequireReceiver;
#if false&&UNITY_EDITOR
#else
				clip.AddEvent(e);
#endif
			}
		}

		protected virtual void OnWrapChanged(WrapMode value) {
			if(m_Index>=0&&!m_IsPlaying) {OnLoopPointReached();}
		}

		protected virtual void OnSpeedChanged(float value) {
			if(m_Index<0) {return;}
			//
			if(GetSpeed(true)!=0.0f) {SetSpeed(value);}
			if(progress!=null) {
				((System.IProgress<float>)progress)?.Report(value);
			}
		}

		/// <summary>
		/// <seealso cref="UnityEngine.Video.VideoPlayer.loopPointReached"/>
		/// </summary>
		protected virtual void OnLoopPointReached() {
			m_IsPlaying=false;
			//
			bool a=animation!=null,b=animator!=null;
			if(a||b) {
				if(wrap!=null) {
					switch(wrap.value) {
						case WrapMode.ClampForever:
							if(b) {Pause();}
						return;
						case WrapMode.Loop:
							Stop();Play(m_Index);
						break;
						case WrapMode.Default:
							Stop();Play((m_Index+1)%animations.Count);
						break;
					}
				}
			}
		}

		#endregion Methods
	}
}
