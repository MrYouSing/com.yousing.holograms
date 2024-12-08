using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YouSingStudio.Holograms {
	public class UIMenuButton
		:MonoBehaviour
		,IPointerClickHandler
	{
		#region Fields

		public int show=0x2;
		public int hide=0x7;
		public GameObject menu;
		public RectTransform main;
		public RectTransform content;
		public Button button;
		public List<Button> buttons;
		[Header("Animation")]
		public Vector2 direction=Vector2.down;
		public Vector2 step=Vector2.down*100.0f;
		public Vector2 duration=Vector2.right;
		public AnimationCurve curve=AnimationCurve.Linear(0.0f,0.0f,1.0f,1.0f);

		[System.NonSerialized]protected bool m_Active;
		[System.NonSerialized]protected float m_Time=-1.0f;
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

		protected virtual void Update() {
			if(!UpdateAnimation()) {return;}
			if(!UpdateHide()) {return;}
		}

		public virtual void OnPointerClick(PointerEventData e) {
			if((show&(1<<(int)e.button))!=0) {SetActive(!m_Active);}
			else if(button!=null) {button.OnPointerClick(e);}
		}

		#endregion Unity Messages

		#region Methods

		protected virtual bool UpdateAnimation() {
			if(m_Time>=0.0f&&content!=null) {
				float d=m_Active?duration.x:duration.y,t=Mathf.Clamp01((Time.time-m_Time)/d);
				content.anchoredPosition=direction*Mathf.Lerp(m_Length.x,m_Length.y,curve.Evaluate(m_Active?t:(1.0f+t)));
				if(t>=1.0f) {
					m_Time=-1.0f;
					//
					if(menu!=null&&!m_Active) {menu.SetActive(false);}
				}
				return false;
			}
			return true;
		}

		protected virtual bool UpdateHide() {
			if(m_Active) {
			for(int i=0;i<3;++i) {
				if((hide&(1<<i))!=0&&Input.GetMouseButtonDown(i)) {
					int imax=m_Buttons?.Length??0;Vector2 v=Input.mousePosition;
					RectTransform it;for(i=0;i<imax;++i) {it=m_Buttons[i];
						if(it!=null&&RectTransformUtility.RectangleContainsScreenPoint(it,v)) {break;}
					}
					if(i>=imax) {SetActive(false);}return false;
				}
			}}
			return true;
		}

		public virtual void SetActive(bool value) {
			if(value==m_Active) {return;}
			m_Active=value;
			//
			int n=buttons.Count;if(buttons.IndexOf(button)>=0) {--n;}
			m_Length=step*n;float d=m_Active?duration.x:duration.y;
			//
			if(d>=0.0f) {
				enabled=true;
				m_Time=Time.time;
				if(menu!=null) {menu.SetActive(true);}
			}else {
				if(menu!=null) {menu.SetActive(m_Active);}
			}
		}

		public virtual void Show()=>SetActive(true);
		public virtual void Hide()=>SetActive(false);

		public virtual void SetButton(Button value) {
			if(button!=null) {button.transform.SetParent(content,false);}
			button=value;
			if(button!=null) {button.transform.SetParent(main,false);}
			//
			int i=0,imax=m_Buttons?.Length??0;
			Transform it;for(;i<imax;++i) {
				it=m_Buttons[i];if(it!=null) {it.SetAsLastSibling();}
			}
		}

		#endregion Methods
	}
}
