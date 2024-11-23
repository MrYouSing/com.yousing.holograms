using System.Collections.Generic;
using UnityEngine;
using YouSingStudio.Private;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// A helper class to control some system.<br/>
	/// <seealso cref="UnityEngine.Animator"/><br/>
	/// <seealso cref="UnityEngine.Playables.PlayableDirector"/>
	/// </summary>
	public class MonoDirector
		:MonoBehaviour
	{
		#region Fields

		public Key[] modifiers;
		public Key[] keys;

		[System.NonSerialized]protected int m_Index=-1;
		[System.NonSerialized]protected Dictionary<string,int> m_Key2Index;

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			Set(-2);
		}

		protected virtual void Start() {
			var sm=ShortcutManager.instance;string key;
			for(int i=0,imax=keys?.Length??0;i<imax;++i) {
				int ii=i;key=KeyOf(i);if(string.IsNullOrEmpty(key)) {key=$"{name}#{i}";}
				sm.Add(name+" Show "+key,()=>Set(ii),ShortcutManager.GetKeys(keys[i],modifiers));
			}
			Set(0);
		}

		#endregion Unity Messages

		#region Methods

		public virtual int Count=>0;
		public virtual string KeyOf(int index)=>null;
		public virtual void Set(string key)=>Set(IndexOf(key));

		public virtual int IndexOf(string key) {
			//
			int i=0,imax=Count;if(m_Key2Index==null) {
				m_Key2Index=new Dictionary<string,int>(imax);
				for(;i<imax;++i) {m_Key2Index.AddRange(KeyOf(i),i);}
			}
			//
			if(m_Key2Index.TryGetValue(key,out i)&&i>=0) {return i;}
			for(i=m_Key2Index.Count;i<imax;++i) {
				if(string.Equals(KeyOf(i),key,UnityExtension.k_Comparison)) {return i;}
			}
			return -1;
		}

		public virtual void Add(string key) {
			m_Key2Index.AddRange(key,Count);
		}

		public virtual void Set(int index) {
			throw new System.NotImplementedException();
		}

		#endregion Methods
	}
}