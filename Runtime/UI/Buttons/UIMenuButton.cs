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

		public int show=0x3;
		public int hide=0x7;
		public GameObject menu;
		public RectTransform content;
		public Button button;
		public List<Button> buttons;
		[Header("Animation")]
		public Vector2 direction=Vector2.down;
		public float step=100.0f;
		public float duration=1.0f;
		public AnimationCurve curve=AnimationCurve.Linear(0.0f,0.0f,1.0f,1.0f);

		[System.NonSerialized]protected bool m_Active;
		[System.NonSerialized]protected float m_Time=-1.0f;
		[System.NonSerialized]protected float m_Length=0.0f;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			Button it;for(int i=0,imax=buttons.Count;i<imax;++i) {
				it=buttons[i];
				if(it!=null&&it.GetComponent<UIMenuButton>()==null) {
					it.onClick.AddListener(Hide);
				}
			}
		}

		protected virtual void Update() {
			//
			if(m_Time>=0.0f&&content!=null) {
				float t=Mathf.Clamp01((Time.time-m_Time)/duration);
				content.anchoredPosition=direction*(curve.Evaluate(m_Active?t:(1.0f-t))*m_Length);
				if(t>=1.0f) {
					m_Time=-1.0f;
					//
					if(menu!=null&&!m_Active) {menu.SetActive(false);}
				}else {return;}
			}
			//
			if(m_Active) {
			for(int i=0;i<3;++i) {
				if((hide&(1<<i))!=0&&Input.GetMouseButtonDown(i)) {
					SetActive(false);break;
				}
			}}
		}

		public virtual void OnPointerClick(PointerEventData e) {
			if((show&(1<<(int)e.button))!=0) {SetActive(true);}
			else if(button!=null) {button.OnPointerClick(e);}
		}

		#endregion Unity Messages

		#region Methods

		public virtual void SetActive(bool value) {
			if(value==m_Active) {return;}
			m_Active=value;
			//
			int n=buttons.Count;if(buttons.IndexOf(button)>=0) {--n;}
			m_Length=step*n;
			//
			if(duration>=0.0f) {
				enabled=true;
				m_Time=Time.time;
				if(menu!=null) {menu.SetActive(true);}
			}else {
				if(menu!=null) {menu.SetActive(m_Active);}
			}
		}

		public virtual void Show()=>SetActive(true);
		public virtual void Hide()=>SetActive(false);

		#endregion Methods
	}
}
