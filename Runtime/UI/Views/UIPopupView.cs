using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="UnityEditor.EditorGUI.Popup"/><br/>
	/// <seealso cref="UnityEngine.UI.Dropdown"/>
	/// </summary>
	public class UIPopupView
		:MonoBehaviour
		,IPointerClickHandler
	{
		#region Fields

		public int show=0x7;
		public int hide=0x7;
		public Button button;
		public GameObject view;
		public RectTransform content;
		[Header("Animation")]
		public Vector2 duration=Vector2.right;
		public AnimationCurve curve=AnimationCurve.Linear(0.0f,0.0f,1.0f,1.0f);

		[System.NonSerialized]protected bool m_Active;
		[System.NonSerialized]protected float m_Time=-1.0f;
		[System.NonSerialized]protected CanvasGroup m_Canvas;
		[System.NonSerialized]protected GameObject m_ButtonV;
		[System.NonSerialized]protected RectTransform m_ButtonT;

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			if(view!=null) {
				if(content==null) {content=view.transform as RectTransform;}
				m_Canvas=view.GetComponent<CanvasGroup>();
			}
			SetButton(button);
		}

		protected virtual void Update() {
			if(!UpdateAnimation()) {return;}
			if(!UpdateHide()) {return;}
		}

		public virtual void OnPointerClick(PointerEventData e) {
			int mask=m_Active?hide:show;
			if((mask&(1<<(int)e.button))!=0) {SetActive(!m_Active);}
		}

		#endregion Unity Messages

		#region Methods

		protected virtual bool OutOfRange(Vector2 point) {
			if(content!=null) {
				if(content.ContainsScreenPoint(point,false)) {return false;}
				if(m_ButtonT.ContainsScreenPoint(point,false)) {return false;}
			}
			return true;
		}

		protected virtual bool UpdateAnimation() {
			if(m_Time>=0.0f) {
				float f=m_Active?duration.x:duration.y,t=Mathf.Clamp01((Time.time-m_Time)/f);
				f=curve.Evaluate(m_Active?t:(1.0f+t));
				if(m_Canvas!=null) {m_Canvas.alpha=f;}
				else if(content!=null) {content.localScale=Vector3.one*f;}
				if(t>=1.0f) {
					m_Time=-1.0f;
					//
					if(view!=null&&!m_Active) {view.SetActive(false);}
				}
				return false;
			}
			return true;
		}

		protected virtual bool UpdateHide() {
			if(m_Active) {
			for(int i=0;i<3;++i) {
				if((hide&(1<<i))!=0&&Input.GetMouseButtonDown(i)) {
					if(OutOfRange(Input.mousePosition)) {SetActive(false);}
					return false;
				}
			}}
			return true;
		}

		public virtual void SetActive(bool value,bool instant=false) {
			if(value==m_Active) {return;}
			m_Active=value;
			m_Time=-1.0f;
			if(m_Canvas!=null) {m_Canvas.blocksRaycasts=m_Active;}
			//
			float f=m_Active?duration.x:duration.y;
			if(instant||f==0.0f) {
				if(view!=null) {view.SetActive(value);}
			}else {
				enabled=true;
				m_Time=Time.time;
				if(view!=null) {view.SetActive(true);}
			}
		}

		public virtual void Show()=>SetActive(true);
		public virtual void Hide()=>SetActive(false);

		public virtual void SetButton(Button value) {
			button=value;
			if(button!=null) {
				m_ButtonV=button.gameObject;
				m_ButtonT=button.transform as RectTransform;
			}
		}

		public virtual void SetButton(bool value) {
			SetActive(false,true);
			if(m_ButtonV!=null) {m_ButtonV.SetActive(value);}
		}

		#endregion Methods
	}
}
