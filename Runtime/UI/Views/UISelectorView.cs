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
// TODO: Pages or Loop????
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YouSingStudio.Private;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Holograms {
	public class UISelectorView
		:MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// <seealso cref="ScrollRect.content"/>
		/// </summary>
		[FormerlySerializedAs("container")]
		public Transform content;
		public GameObject prefab;
		public int capacity=-1;
		/// <summary>
		/// <seealso cref="ScrollRect.viewport"/>
		/// </summary>
		public RectTransform viewport;
		[Tooltip("x:Scale\ny:Padding\nz:Min\nw:Max")]
		public Vector4 size;
		public Transform arrow;
		public int depth=-1;
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
			int i=capacity-m_Views.Count;
			while(i-->0) {m_Views.Add(CreateView());}
			//
			var sm=ShortcutManager.instance;i=0;
// <!-- Macro.Patch Start
			sm.Add(name+".Prev",Prev,keys[i]);++i;
			sm.Add(name+".Next",Next,keys[i]);++i;
			sm.Add(name+".PageUp",PageUp,keys[i]);++i;
			sm.Add(name+".PageDown",PageDown,keys[i]);++i;
// Macro.Patch -->
			m_Scroll=content.GetComponentInParent<ScrollRect>();
			m_Layout=content.GetComponent<GridLayoutGroup>();
			if(m_Layout!=null) {m_Page=m_Layout.constraintCount;}
		}

		#endregion Unity Messages

		#region Methods

		public virtual void Render(IList<string> list) {
			m_Index=-1;m_Count=0;
			m_Views.Render(list,RenderView,CreateView);
			UpdateViewport();
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
			if(m_Index<0) {UpdateArrow(null);return;}
			//
			UnityExtension.BlurUI();
			UpdateArrow(m_Views[m_Index].transform);
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

		protected virtual void UpdateViewport() {
			if(viewport!=null) {
				RectTransform.Axis a=m_Layout.constraint==GridLayoutGroup.Constraint.FixedRowCount
					?RectTransform.Axis.Horizontal:RectTransform.Axis.Vertical;
				float f=Mathf.Ceil(m_Count/(float)m_Page)*size.x+size.y;
				if(size.z!=size.w) {f=Mathf.Clamp(f,size.z,size.w);}
				viewport.SetSizeWithCurrentAnchors(a,f);
			}
		}

		protected virtual void UpdateArrow(Transform parent) {
			if(arrow==null) {return;}
			arrow.SetParent(parent,false);
			//
			if(parent!=null&&depth>=0) {
				arrow.SetSiblingIndex(depth);
			}
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
			view.transform.SetParent(content,false);
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
