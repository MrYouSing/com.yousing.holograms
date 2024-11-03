using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using YouSingStudio.Private;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// A virtual scene manager.<br/>
	/// <seealso cref="UnityEngine.SceneManagement.SceneManager"/>
	/// </summary>
	public class StageDirector
		:MonoBehaviour
	{
		#region Nested Types

		[System.Serializable]
		public class Stage {
			public string name;
			public List<GameObject> actors;
			public List<Behaviour> behaviours;

			public UnityEvent onStart=null;
			public UnityEvent onShow=null;
			public UnityEvent onHide=null;
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
				}else {
					onHide?.Invoke();
				}
			}
		}

		#endregion Nested Types

		#region Fields

		public Key[] modifiers;
		public Key[] keys;
		public List<Stage> stages=new List<Stage>();

		[System.NonSerialized]protected int m_Index=-1;
		[System.NonSerialized]protected Dictionary<string,int> m_Key2Index;

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			Set(-2);
		}

		protected virtual void Start() {
			var sm=ShortcutManager.instance;
			for(int i=0,imax=Mathf.Min(keys?.Length??0,stages?.Count??0);i<imax;++i) {
				int ii=i;
				sm.Add(name+" Show "+stages[i]?.name??("Stage#"+i),()=>Set(ii),ShortcutManager.GetKeys(keys[i],modifiers));
			}
			Set(0);
		}

		#endregion Unity Messages

		#region Methods

		public virtual int IndexOf(string key) {
			//
			int i=0,imax=stages?.Count??0;if(m_Key2Index==null) {
				m_Key2Index=new Dictionary<string,int>(imax);
				for(;i<imax;++i) {m_Key2Index.AddRange(stages[i].name,i);}
			}
			//
			if(m_Key2Index.TryGetValue(key,out i)&&i>=0) {return i;}
			for(i=m_Key2Index.Count;i<imax;++i) {
				if(string.Equals(stages[i]?.name,key,UnityExtension.k_Comparison)) {return i;}
			}
			return -1;
		}

		public virtual void Add(Stage stage) {
			if(stage!=null) {
				m_Key2Index.AddRange(stage.name,stages.Count);
				stages.Add(stage);
			}
		}

		public virtual void Set(int index) {
			if(index==m_Index) {return;}
			if(m_Index>=0) {stages[m_Index]?.SetActive(false);}
			m_Index=index;
			for(int i=0,imax=stages?.Count??0;i<imax;++i) {
				stages[i]?.SetActive(i==m_Index);
			}
		}

		public virtual void Set(string key)=>Set(IndexOf(key));

		public virtual void Open(string key,string path) {
			int i=IndexOf(key);
			if(i>=0) {
				Set(-1);Set(i);// Reload.
				stages[i].onOpen?.Invoke(path);
			}
		}

		#endregion Methods
	}
}
