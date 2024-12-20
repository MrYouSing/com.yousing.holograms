/* <!-- Macro.Include
../Internal/UnityExtension.cs
 Macro.End --> */
/* <!-- Macro.Call DeclareEvent
protected,<string>,Receive,,string value,value,
 Macro.End --> */
/* <!-- Macro.Patch
,Events
 Macro.End --> */
using System.Collections.Generic;
using UnityEngine;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="UnityEditor.DragAndDrop"/>
	/// </summary>
	public class FileReceiver
		:MonoBehaviour
	{
		#region Fields

		public static bool s_Hack;

		public int capacity=1;
		public GameObject hint;

		[System.NonSerialized]protected bool m_Hint;
		[System.NonSerialized]protected List<string> m_Files=new List<string>();

		#endregion Fields

		#region Unity Messages

		protected virtual void OnEnable() {
			SetActive(true);
		}

		protected virtual void OnDisable() {
			SetActive(false);
		}

		protected virtual void Update() {// Safe thread.
			//
			if(hint!=null) {if(m_Hint!=hint.activeSelf) {
				hint.SetActive(m_Hint);
			}}
			//
			int i=0,imax=Mathf.Min(m_Files.Count,capacity);
			if(imax>0) {
				for(;i<imax;++i) {OnReceive(m_Files[i]);}
				m_Files.Clear();
			}
		}

		#endregion Unity Messages

		#region Methods

		protected virtual bool SetExperimentalActive(bool value) {
			System.Type type=System.Type.GetType("YouSingStudio.Events.DragDropManager,YouSingStudio.Events");
			if(type!=null) {// TODO: Experimental????
				if(!s_Hack) {
					s_Hack=true;
					type.GetMethod("SetScreen")?.Invoke(null,new object[]{typeof(Private.ScreenManager)});
				}
				if(value) {
					type.GetMethod("Register",new System.Type[]{typeof(int),typeof(object)})
						?.Invoke(null,new object[]{0,this});
				}else {
					type.GetMethod("Unregister",new System.Type[]{typeof(int),typeof(object)})
						?.Invoke(null,new object[]{0,this});
				}
				return true;
			}
			return false;
		}
#if !UNITY_EDITOR&&UNITY_STANDALONE_WIN
		protected virtual void SetActive(bool value) {
			if(!SetExperimentalActive(value)) {
				if(value) {
					B83.Win32.UnityDragAndDropHook.InstallHook();
					B83.Win32.UnityDragAndDropHook.OnDroppedFiles+=OnFiles;
				}else {
					B83.Win32.UnityDragAndDropHook.OnDroppedFiles-=OnFiles;
					B83.Win32.UnityDragAndDropHook.UninstallHook();
				}
			}
		}

		protected virtual void OnFiles(List<string> list,B83.Win32.POINT point) {
			m_Files.AddRange(list);
		}
#else
		protected virtual void SetActive(bool value) {
			//SetExperimentalActive(value);
		}
#endif
		public virtual void OnDragEnter() {
			m_Hint=true;
		}

		public virtual void OnDragLeave() {
			m_Hint=false;
		}

		public virtual void OnDragOver() {
			m_Hint=true;
		}

		public virtual void OnDragDrop(IList<string> list) {
			Debug.Log(list!=null?string.Join(';',list):"Null");
			//
			m_Hint=false;
			m_Files.AddRange(list);
		}

		#endregion Methods
// <!-- Macro.Patch Events
		[System.NonSerialized]public System.Action<string> onReceive=null;
		[SerializeField]protected UnityEngine.Events.UnityEvent<string> m_OnReceive=null;

		protected virtual void OnReceive(string value) {
			onReceive?.Invoke(value);
			m_OnReceive?.Invoke(value);
		}

// Macro.Patch -->
	}
}