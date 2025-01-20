using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seelaso cref="UnityEngine.UI.ToggleGroup"/>
	/// </summary>
	public class UIControllerGroup
		:MonoBehaviour
	{
		#region Fields

		public List<GameObject> views=new List<GameObject>();

		[System.NonSerialized]protected Dictionary<GameObject,UIBaseController> m_MapV2C=new Dictionary<GameObject,UIBaseController>();

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(views.Count<=0){
				foreach(Transform it in transform) {views.Add(it.gameObject);}
			}
		}

		protected virtual void LateUpdate() {
			using(ListPool<GameObject>.Get(out var list)) {
				GameObject it;for(int i=0,imax=views.Count;i<imax;++i) {
					it=views[i];if(it!=null&&it.activeSelf){list.Add(it);}
				}
				SetViews(list);
			}
		}

		#endregion Unity Messages

		#region Methods

		public static int Compare(GameObject x,GameObject y) {
			int xx=x!=null?x.transform.GetSiblingIndex():-1;
			int yy=y!=null?y.transform.GetSiblingIndex():-1;
			return xx-yy;
		}

		public virtual UIBaseController GetController(GameObject go) {
			UIBaseController tmp=null;
			if(go!=null&&!m_MapV2C.TryGetValue(go,out tmp)) {
				UIBaseController[] all=FindObjectsByType<UIBaseController>(FindObjectsInactive.Include,FindObjectsSortMode.None);
				UIBaseController it;for(int i=0,imax=all?.Length??0;i<imax;++i) {
					it=all[i];if(it==null||it.view==null) {continue;}
					if(it.view.gameObject==go) {
						tmp=it;m_MapV2C[go]=tmp;break;
					}
				}
			}
			return tmp;
		}

		public virtual void SetViews(List<GameObject> value) {
			int imax=value?.Count??0;
			switch(imax) {
				case 0:
					//SetView(null);
				break;
				case 1:
					//SetView(value[0]);
				break;
				default:
					value.Sort(Compare);SetView(value[imax-1]);
				break;
			}
		}

		public virtual void SetView(GameObject value) {
			//
			int i=0,imax=views.Count;bool a,b;
			GameObject it;for(;i<imax;++i) {
				it=views[i];if(it==null) {continue;}
				//
				a=it.activeSelf;b=it!=value;
				if(a&&b) {
					var c=GetController(it);
					if(c!=null) {c.HideView();}
					else {it.SetActive(false);}
				}
			}
		}

		#endregion Methods
	}
}
