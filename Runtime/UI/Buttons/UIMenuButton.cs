using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YouSingStudio.Holograms {
	public class UIMenuButton
		:UIPopupView
		,IPointerDownHandler
		,IPointerUpHandler
		,IPointerEnterHandler
		,IPointerMoveHandler
		,IPointerExitHandler
	{
		#region Fields

		public Vector2 direction=Vector2.down;
		public Vector2 step=Vector2.down*100.0f;
		[Header("Menu")]
		public RectTransform main;
		public List<Button> buttons;

		[System.NonSerialized]protected bool m_Hover;
		[System.NonSerialized]protected Vector2 m_Length=Vector2.zero;
		[System.NonSerialized]protected RectTransform[] m_Buttons;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			//
			if(button!=null&&buttons.IndexOf(button)<0) {
				buttons.Insert(0,button);
			}
			int i=PlayerPrefs.GetInt(name,-1);
			if(i>=0) {SetButton(buttons[i]);}
			//
			int imax=buttons.Count;
			m_Buttons=new RectTransform[imax];
			Button it;for(i=0;i<imax;++i) {
				it=buttons[i];
				if(it!=null&&it.GetComponent<UIMenuButton>()==null) {
					m_Buttons[i]=it.transform as RectTransform;Button b=it;
					it.AddMissingComponent<EventTrigger>().AddTrigger(
						EventTriggerType.PointerClick,(e)=>OnMenuClick(b,e));
				}
			}
			it=button;button=null;SetButton(it);
		}

		protected virtual void OnEnable() {
			m_Hover=false;
			button.SetPressed(false);
		}

		public virtual void OnPointerEnter(PointerEventData e) {
			if(!m_Click) {return;}
			if(!RectTransformUtility.RectangleContainsScreenPoint(m_ButtonT,e.position)) {
				SetHover(false);return;
			}
			//
			SetHover(true);
		}

		public virtual void OnPointerMove(PointerEventData e) {
			if(!m_Click) {return;}
			//
			bool b=RectTransformUtility.RectangleContainsScreenPoint(m_ButtonT,e.position);
			if(b!=m_Hover) {SetHover(b);}
		}

		public virtual void OnPointerExit(PointerEventData e) {
			if(!m_Click) {return;}
			//
			SetHover(false);
		}

		public virtual void OnPointerDown(PointerEventData e) {
			if(!m_Click) {return;}
			//
			if((show&(1<<(int)e.button))!=0) {}
			else {button.SetPressed(true);}
		}

		public virtual void OnPointerUp(PointerEventData e) {
			if(!m_Click) {return;}
			//
			if((show&(1<<(int)e.button))!=0) {}
			else {button.SetPressed(false);}
		}

		public override void OnPointerClick(PointerEventData e) {
			if(!m_Click) {return;}
			//
			if((show&(1<<(int)e.button))!=0) {SetActive(!m_Active);}
			else if(button!=null) {ExecuteEvents.Execute(button.gameObject,e,ExecuteEvents.pointerClickHandler);}
		}

		protected virtual void OnMenuClick(Button b,BaseEventData e) {
			if(((PointerEventData)e).button==PointerEventData.InputButton.Right) {
				int i=buttons.IndexOf(b);PlayerPrefs.SetInt(name,i);SetButton(b);
			}
			Hide();
		}

		#endregion Unity Messages

		#region Methods

		protected override bool UpdateAnimation() {
			if(m_Time>=0.0f&&content!=null) {
				float f=m_Active?duration.x:duration.y,t=Mathf.Clamp01((Time.time-m_Time)/f);
				content.anchoredPosition=direction*Mathf.Lerp(m_Length.x,m_Length.y,curve.Evaluate(m_Active?t:(1.0f+t)));
				if(t>=1.0f) {
					m_Time=-1.0f;
					//
					if(view!=null&&!m_Active) {view.SetActive(false);}
				}
				return false;
			}
			return true;
		}

		public override void SetActive(bool value,bool instant=false) {
			if(value==m_Active) {return;}
			m_Active=value;
			m_Time=-1.0f;
			if(m_Canvas!=null) {m_Canvas.blocksRaycasts=m_Active;}
			//
			int n=buttons.Count;if(buttons.IndexOf(button)>=0) {--n;}
			m_Length=step*n;float f=m_Active?duration.x:duration.y;
			if(instant||m_Length.sqrMagnitude==0.0f) {f=0.0f;}
			//
			if(f>0.0f) {
				enabled=true;
				m_Time=Time.time;
				if(view!=null) {view.SetActive(true);}
				//
				onActive?.Invoke(m_Active);
			}else {
				m_Active=!value;
				base.SetActive(value,instant);
			}
		}

		public override void SetButton(Button value) {
			if(value==button) {return;}
			if(button!=null) {button.transform.SetParent(content,false);button.SetPressed(false);}
			button=value;
			if(button!=null) {button.transform.SetParent(main,false);}
			base.SetButton(button);m_Click=true;
			//
			int i=0,imax=m_Buttons?.Length??0;
			RectTransform it;for(;i<imax;++i) {
				it=m_Buttons[i];if(it!=null) {
					it.SetAsLastSibling();it.ResetAnchor();
				}
			}
		}

		public virtual void SetHover(bool value) {
			m_Hover=value;
			if(button!=null) {button.SetHighlighted(m_Hover);}
		}

		#endregion Methods
	}
}
