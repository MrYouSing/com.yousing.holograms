// TODO: Timeline for animations????
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouSingStudio.Holograms {
	public class UIAnimationController
		:MonoBehaviour
	{
		#region Nested Types

		public class Listener
			:MonoBehaviour
		{
			public UIAnimationController context;
			public virtual void OnLoopPointReached() {
				if(context!=null) {context.OnLoopPointReached();}
			}
		}

		#endregion Nested Types

		#region Fields

		public new Animation animation;
		public List<string> animations=new List<string>();
		[Header("UI")]
		//
		public Text text;
		public Image image;
		//
		public UIPopupView popup;
		public UIWrapMode wrap;
		public UIFloatSelector speed;
		public UISelectorView selector;

		[System.NonSerialized]public System.IProgress<string> progress;
		[System.NonSerialized]protected bool m_IsPlaying;
		[System.NonSerialized]protected int m_Index;

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
			if(wrap!=null) {wrap.onValueChanged+=OnWrapChanged;}
			if(speed!=null) {speed.onValueChanged+=OnSpeedChanged;}
			if(selector!=null) {selector.onSelect+=Play;}
		}

		protected virtual void OnDestroy() {
			UnityExtension.SetListener<TextureType>(OnTypeChanged,false);
			UnityExtension.SetListener<ModelLoader>(OnModelLoaded,false);
		}

		#endregion Unity Messages

		#region Methods

		public virtual void Clear() {
			//
			if(popup!=null) {popup.SetButton(false);}
			animation=null;
			progress=null;
			animations.Clear();
			Button(null,null);
			//selector
			m_IsPlaying=false;
			m_Index=-1;
		}

		public virtual void Load(GameObject actor) {
			if(actor==null) {return;}
			//
			animation=actor.GetComponentInChildren<Animation>();
			if(animation!=null) {
				if(popup!=null) {popup.SetButton(true);}
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
			progress=actor.GetComponentInChildren<System.IProgress<string>>();
			if(progress!=null) {
				if(popup!=null) {popup.SetButton(true);}
				if(progress is IEnumerable<string> tmp) {
					animations.AddRange(tmp);return;
				}
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
				if(animation!=null) {animation.Play(key);}
				if(progress!=null) {progress.Report(key);}
				if(selector!=null) {selector.Highlight(m_Index);}
				//
				m_IsPlaying=true;
				Button(key,null);
				OnSpeedChanged(speed!=null?speed.value:1.0f);
			}
		}

		public virtual void Stop() {
			if(animation!=null) {animation.Stop();}
			if(progress!=null) {progress.Report(null);}
		}

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
			if(!m_IsPlaying) {OnLoopPointReached();}
		}

		protected virtual void OnSpeedChanged(float value) {
			if(m_Index<0) {return;}
			//
			if(animation!=null) {
				var s=animation[animations[m_Index]];s.speed=value;
			}
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
			if(animation!=null) {
				if(wrap!=null) {
					switch(wrap.value) {
						case WrapMode.ClampForever:
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
