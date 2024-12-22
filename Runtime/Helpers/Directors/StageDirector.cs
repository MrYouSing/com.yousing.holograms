/* <!-- Macro.Table Events
Start,
Show,
Hide,
 Macro.End --> */
/* <!-- Macro.Table Active
Activate,true,
Deactivate,false,
 Macro.End --> */

/* <!-- Macro.Call  Events
		public virtual void InvokeOn{0}(string key) {{
			int i=IndexOf(key);
			if(i>=0) {{stages[i].on{0}?.Invoke();}}
		}}

 Macro.End --> */
/* <!-- Macro.Call  Active
		public virtual void {0}(string key) {{
			int i=IndexOf(key);
			if(i>=0) {{stages[i].SetActive({1});}}
		}}

 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */

/* <!-- Macro.Call  Events
			public UnityEvent on{0}=null;
 Macro.End --> */
/* <!-- Macro.Patch
,Stage
 Macro.End --> */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// A virtual scene manager.<br/>
	/// <seealso cref="UnityEngine.SceneManagement.SceneManager"/>
	/// </summary>
	public class StageDirector
		:MonoDirector
	{
// <!-- Macro.Patch AutoGen
		public virtual void InvokeOnStart(string key) {
			int i=IndexOf(key);
			if(i>=0) {stages[i].onStart?.Invoke();}
		}

		public virtual void InvokeOnShow(string key) {
			int i=IndexOf(key);
			if(i>=0) {stages[i].onShow?.Invoke();}
		}

		public virtual void InvokeOnHide(string key) {
			int i=IndexOf(key);
			if(i>=0) {stages[i].onHide?.Invoke();}
		}

		public virtual void Activate(string key) {
			int i=IndexOf(key);
			if(i>=0) {stages[i].SetActive(true);}
		}

		public virtual void Deactivate(string key) {
			int i=IndexOf(key);
			if(i>=0) {stages[i].SetActive(false);}
		}

// Macro.Patch -->
		#region Nested Types

		[System.Serializable]
		public class Stage {
			public string name;
			public List<GameObject> actors;
			public List<Behaviour> behaviours;
// <!-- Macro.Patch Stage
			public UnityEvent onStart=null;
			public UnityEvent onShow=null;
			public UnityEvent onHide=null;
// Macro.Patch -->
			public UnityEvent<string> onOpen=null;

			[System.NonSerialized]protected bool m_Start=false;
			[System.NonSerialized]protected bool m_Value=true;

			public virtual void SetActive(bool value) {
				if(value==m_Value) {return;}
				m_Value=value;
				//
				GameObject a;for(int i=0,imax=actors?.Count??0;i<imax;++i) {
					a=actors[i];if(a!=null) {a.SetActive(value);}
				}
				Behaviour b;for(int i=0,imax=behaviours?.Count??0;i<imax;++i) {
					b=behaviours[i];if(b!=null) {b.enabled=value;}
				}
				if(value) {
					if(!m_Start) {m_Start=true;onStart?.Invoke();}
					onShow?.Invoke();
				}else if(m_Start) {
					onHide?.Invoke();
				}
			}
		}

		#endregion Nested Types

		#region Fields

		public List<Stage> stages=new List<Stage>();

		#endregion Fields

		#region Methods

		public override int Count=>stages.Count;
		public override string KeyOf(int index)=>stages[index].name;

		public virtual void Add(Stage stage) {
			if(stage!=null&&stages.IndexOf(stage)<0) {
				base.Add(stage.name);
				stages.Add(stage);
			}
		}

		protected virtual void Activate() {
			bool b=!didStart;// On Awake().
			Stage it;for(int i=0,imax=stages?.Count??0;i<imax;++i) {
				it=stages[i];
				if(it!=null&&(b||!it.name.StartsWith('$'))) {
					it.SetActive(i==m_Index);
				}
			}
		}

		protected virtual void Deactivate() {
			if(m_Index>=0) {stages[m_Index]?.SetActive(false);}
			m_Index=-1;
		}

		public override void Set(int index) {
			if(index==m_Index) {return;}
			//
			Deactivate();
			m_Index=index;
			Activate();
		}

		public virtual void Open(string key,string path) {
			int i=IndexOf(key);
			if(i>=0) {
				Deactivate();Set(i);// Reload.
				stages[i].onOpen?.Invoke(path);
			}
		}

		#endregion Methods
	}
}
