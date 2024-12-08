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

		public override void Set(int index) {
			if(index==m_Index) {return;}
			if(m_Index>=0) {stages[m_Index]?.SetActive(false);}
			m_Index=index;
			for(int i=0,imax=stages?.Count??0;i<imax;++i) {
				stages[i]?.SetActive(i==m_Index);
			}
		}

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
