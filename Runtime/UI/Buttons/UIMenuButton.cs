using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YouSingStudio.Holograms {
	public class UIMenuButton
		:UIPopupView
	{
		#region Fields

		public Vector2 direction=Vector2.down;
		public Vector2 step=Vector2.down*100.0f;
		[Header("Menu")]
		public RectTransform main;
		public List<Button> buttons;

		[System.NonSerialized]protected Vector2 m_Length=Vector2.zero;
		[System.NonSerialized]protected RectTransform[] m_Buttons;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			//
			if(button!=null&&buttons.IndexOf(button)<0) {
				buttons.Add(button);
			}
			//
			int i=0,imax=buttons.Count;
			m_Buttons=new RectTransform[imax];
			Button it;for(;i<imax;++i) {
				it=buttons[i];
				if(it!=null&&it.GetComponent<UIMenuButton>()==null) {
					m_Buttons[i]=it.transform as RectTransform;
					it.onClick.AddListener(Hide);
				}
			}
			SetButton(button);
		}

		public override void OnPointerClick(PointerEventData e) {
			if((show&(1<<(int)e.button))!=0) {SetActive(!m_Active);}
			else if(button!=null) {button.OnPointerClick(e);}
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
			}else {
				m_Active=!value;
				base.SetActive(value,instant);
			}
		}

		public override void SetButton(Button value) {
			if(button!=null) {button.transform.SetParent(content,false);}
			button=value;
			if(button!=null) {button.transform.SetParent(main,false);}
			base.SetButton(button);
			//
			int i=0,imax=m_Buttons?.Length??0;
			Transform it;for(;i<imax;++i) {
				it=m_Buttons[i];if(it!=null) {it.SetAsLastSibling();}
			}
		}

		#endregion Methods
	}
}
