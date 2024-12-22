/* <!-- Macro.Table Shortcut
Prev,
Next,
PageUp,
PageDown,
 Macro.End --> */
/* <!-- Macro.Call  Shortcut
			sm.Add(name+".{0}",{0},keys[i]);++i;
 Macro.End --> */
/* <!-- Macro.Patch
,Start
 Macro.End --> */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YouSingStudio.Private;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Holograms {
	public class UISelectorView
		:MonoBehaviour
	{
		#region Fields

		public Transform container;
		public GameObject prefab;
		public Transform arrow;
		public bool loop=true;
		public Key[] keys=new Key[4];

		[System.NonSerialized]public System.Func<GameObject,int,bool> onCreate=null;
		[System.NonSerialized]public System.Func<GameObject,string,bool> onRender=null;
		[System.NonSerialized]public System.Action<int> onSelect=null;
		[System.NonSerialized]protected int m_Index;
		[System.NonSerialized]protected int m_Count;
		[System.NonSerialized]protected int m_Page;
		[System.NonSerialized]protected List<GameObject> m_Views=new List<GameObject>();
		[System.NonSerialized]protected ScrollRect m_Scroll;
		[System.NonSerialized]protected GridLayoutGroup m_Layout;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			var sm=ShortcutManager.instance;int i=0;
// <!-- Macro.Patch Start
			sm.Add(name+".Prev",Prev,keys[i]);++i;
			sm.Add(name+".Next",Next,keys[i]);++i;
			sm.Add(name+".PageUp",PageUp,keys[i]);++i;
			sm.Add(name+".PageDown",PageDown,keys[i]);++i;
// Macro.Patch -->
			m_Scroll=container.GetComponentInParent<ScrollRect>();
			m_Layout=container.GetComponent<GridLayoutGroup>();
			if(m_Layout!=null) {m_Page=m_Layout.constraintCount;}
		}

		#endregion Unity Messages

		#region Methods

		public virtual void Render(IList<string> list) {
			m_Index=-1;m_Count=0;
			m_Views.Render(list,RenderView,CreateView);
			if(m_Scroll!=null) {
				m_Scroll.normalizedPosition=Vector2.up;
			}
		}

		public virtual void Add(string model) {
			GameObject view=null;
			//
			if(m_Count<m_Views.Count) {view=m_Views[m_Count];}
			else {view=CreateView();m_Views.Add(view);}
			//
			RenderView(view,model);
		}

		public virtual void Select(int index) {
			//
			if(loop) {
				if(index<0) {index=m_Count-1;}
				else if(index>=m_Count) {index=0;}
			}else {
				index=Mathf.Clamp(index,0,m_Count-1);
			}
			//
			m_Index=index;
			onSelect?.Invoke(m_Index);
		}

		public virtual void Highlight(int index) {
			m_Index=index;
			//
			var es=EventSystem.current;// Avoid space key to submit.
			if(es!=null) {es.SetSelectedGameObject(null);}
			if(arrow!=null) {arrow.SetParent(m_Views[m_Index].transform,false);}
			UpdateScroll();
		}

		public virtual void Prev() {
			Select(m_Index-1);
		}

		public virtual void Next() {
			Select(m_Index+1);
		}

		public virtual void PageUp() {
			Select(m_Index-m_Page);
		}

		public virtual void PageDown() {
			Select(m_Index+m_Page);
		}

		protected virtual void UpdateScroll() {
			StopCoroutine("UpdateScrollDelayed");StartCoroutine(UpdateScrollDelayed());
		}

		protected virtual IEnumerator UpdateScrollDelayed() {
			yield return null;
			if(m_Scroll!=null) {
				RectTransform rt=m_Views[m_Index].transform as RectTransform;
				Vector2 u,v;if(m_Layout==null) {
					u=0.5f*rt.sizeDelta;v=Vector2.zero;
				}else {
					u=0.5f*m_Layout.cellSize;v=m_Layout.spacing;
				}
				m_Scroll.normalizedPosition=m_Scroll.GetNormalizedPoint(rt,u,v);
			}
		}

		protected virtual GameObject CreateView() {
			GameObject view=GameObject.Instantiate(prefab);
			view.transform.SetParent(container,false);
			//
			int i=m_Views.Count;
			if(!(onCreate?.Invoke(view,i)??false)) {
				Button b=view.GetComponentInChildren<Button>();
				if(b!=null) {b.onClick.AddListener(()=>Select(i));}
			}
			return view;
		}

		protected virtual void RenderView(GameObject view,string model) {
			if(view==null) {return;}
			if(string.IsNullOrEmpty(model)) {view.SetActive(false);return;}
			//
			if(!(onRender?.Invoke(view,model)??false)) {
				Text txt=view.GetComponentInChildren<Text>();
				if(txt!=null) {txt.text=model;}
			}
			view.SetActive(true);++m_Count;
		}

		#endregion Methods
	}
}
