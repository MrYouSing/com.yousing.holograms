// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

/* <!-- Macro.Copy File
:Packages/com.yousing.io/Runtime/Internal/MonoSingleton.cs,9~39
 Macro.End --> */
/* <!-- Macro.Replace
T,ShortcutManager
ShortcutManagerype,Type
 Macro.End -->*/
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;using YouSingStudio.Holograms;
using UnityEngine.Events;
using UnityEngine.Pool;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Private {
	public class ShortcutManager
		:MonoBehaviour
	{
// <!-- Macro.Patch AutoGen
		public static bool s_InstanceCached;
		public static ShortcutManager s_Instance;
		public static ShortcutManager instance {
			get {
				if(!s_InstanceCached) {
					s_InstanceCached=true;
					//
					if(s_Instance==null) {
						s_Instance=Object.FindAnyObjectByType<ShortcutManager>();
						if(s_Instance==null) {
							s_Instance=new GameObject(typeof(ShortcutManager).FullName)
								.AddComponent<ShortcutManager>();
						}
					}
				}
				return s_Instance;
			}
			protected set{
				s_Instance=value;
				s_InstanceCached=s_Instance!=null;
			}
		}

		protected virtual void Awake() {
			if(s_Instance==null) {instance=(ShortcutManager)this;}
			else if(s_Instance!=this) {Object.Destroy(this);}
		}

		protected virtual void OnDestroy() {
			if(s_Instance==this) {instance=null;}
		}
// Macro.Patch -->
		#region Nested Types

		public class Shortcut:
			System.IDisposable
		{
			public string name;
			public Key[] keys;
			public System.Action action;

			[System.NonSerialized]public ShortcutManager context;

			public static Shortcut Obtain()=>GenericPool<Shortcut>.Get();

			public virtual void Recycle() {
				name=null;
				keys=null;
				action=null;
				context=null;
				GenericPool<Shortcut>.Release(this);
			}

			public virtual void Dispose() {
				context.Remove(this);
			}

			public virtual void Invoke() {
				action?.Invoke();
			}
		}

		[System.Serializable]
		protected class _Shortcut:
			Shortcut
		{
			[SerializeField]protected UnityEvent m_Action;

			public override void Recycle() {
			}

			public override void Dispose() {
				action=null;
			}

			public override void Invoke() {
				base.Invoke();
				m_Action?.Invoke();
			}
		}

		#endregion Nested Types

		#region Fields

		[JsonIgnore,SerializeField]protected _Shortcut[] m_Shortcuts;
		public List<Shortcut> shortcuts=new List<Shortcut>();
		[System.NonSerialized]protected string m_Mappings;
		/// <summary>
		/// <seealso cref="UnityEditor.InputManager"/><br/>
		/// <seealso href="https://dev.epicgames.com/documentation/zh-cn/unreal-engine/input-settings-in-the-unreal-engine-project-settings"/>
		/// </summary>
		public Dictionary<string,Key[]> mappings;
		public Dictionary<Key,Key> remap;

		[System.NonSerialized]public Shortcut current;
		[System.NonSerialized]protected HashSet<Key> m_Keys=new HashSet<Key>();

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(!string.IsNullOrEmpty(m_Mappings)) {
				mappings=UnityExtension.FromJson<Dictionary<string,Key[]>>(m_Mappings);
			}
			if((m_Shortcuts?.Length??0)>0) {System.Array.ForEach(m_Shortcuts,Add);}
		}

		protected virtual void Update() {
			m_Keys.Clear();
			//
			int i=0,imax=shortcuts.Count,j=0;
			Shortcut it;for(;i<imax;++i) {
				it=shortcuts[i];if(it!=null) {
					if(Evaluate(it)) {Process(it);}
					if(it==shortcuts[i]) {
						shortcuts[j]=it;++j;
					}
				}
			}
			if(j<imax) {shortcuts.RemoveRange(j,imax-j);}
		}

		#endregion Unity Messages

		#region Methods

		public static bool IsModifier(Key key) {
			return key>=Key.RightShift&&key<=Key.AltGr;
		}

		public static Key[] GetKeys(Key key,params Key[] modifiers) {
			if(key!=Key.None) {
				int i=0,imax=modifiers?.Length??0;Key[] tmp=new Key[imax+1];
				for(;i<imax;++i) {tmp[i]=modifiers[i];}tmp[i]=key;return tmp;
			}else {return null;}
		}

		protected virtual void Add(Shortcut shortcut) {
			if(shortcut!=null&&shortcuts.IndexOf(shortcut)<0) {
				if(mappings!=null&&mappings.TryGetValue(shortcut.name,out var tmp)) {
					shortcut.keys=tmp;
				}
				//
				shortcut.context=this;
				shortcuts.Add(shortcut);
			}
		}

		public virtual Shortcut Get(string key) {
			int i=0,imax=shortcuts.Count;
			Shortcut it;for(;i<imax;++i) {
				it=shortcuts[i];if(it!=null&&it.name==key) {return it;}
			}
			return null;
		}

		public virtual Shortcut Add(string key,System.Action action,params Key[] keys) {
			Shortcut shortcut=Shortcut.Obtain();
				shortcut.name=key;
				shortcut.keys=keys;
				shortcut.action=action;
				//
				Add(shortcut);
			return shortcut;
		}

		public virtual void Remove(Shortcut shortcut) {
			int i=shortcuts.IndexOf(shortcut);
			if(i>=0){
				shortcuts[i]=null;
				shortcut.Recycle();
			}else {
			}
		}

		public virtual Shortcut GetOrAdd(string key,System.Action action,params Key[] keys) {
			Shortcut shortcut=Get(key);
			if(shortcut!=null) {shortcut.action+=action;return shortcut;}
			else {return Add(key,action,keys);}
		}

		public virtual bool Evaluate(Shortcut shortcut) {
			if(shortcut==null) {return false;}
			//
			int i=0,imax=shortcut.keys?.Length??0,j=0;bool b=false;
			Key it;for(;i<imax;++i) {
				//
				it=shortcut.keys[i];
				if(remap!=null&&remap.TryGetValue(it,out var tmp)) {it=tmp;}
				if(m_Keys.Contains(it)) {continue;}
				//
				if(Input.GetKeyDown(it)) {++j;b=true;}
				else if(Input.GetKey(it)) {++j;}
			}
			return j==imax&&b;
		}

		public virtual void Process(Shortcut shortcut) {
			if(shortcut==null) {return;}
			//
			Key it;for(int i=0,imax=shortcut.keys?.Length??0;i<imax;++i) {
				it=shortcut.keys[i];
				if(!IsModifier(it)) {m_Keys.Add(it);}
			}
			var tmp=current;current=shortcut;
				shortcut.Invoke();
			current=tmp;
		}

		#endregion Methods
	}
}
