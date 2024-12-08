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

		public int capacity=1;

		#endregion Fields

		#region Unity Messages
#if !UNITY_EDITOR&&UNITY_STANDALONE_WIN
		protected virtual void OnEnable() {
			B83.Win32.UnityDragAndDropHook.InstallHook();
			B83.Win32.UnityDragAndDropHook.OnDroppedFiles+=OnFiles;
		}

		protected virtual void OnDisable() {
			B83.Win32.UnityDragAndDropHook.OnDroppedFiles-=OnFiles;
			B83.Win32.UnityDragAndDropHook.UninstallHook();
		}
#endif
		#endregion Unity Messages

		#region Methods
#if !UNITY_EDITOR&&UNITY_STANDALONE_WIN
		protected virtual void OnFiles(List<string> list,B83.Win32.POINT point) {
			int i=0,imax=Mathf.Min(list.Count,capacity);
			for(;i<imax;++i) {OnReceive(list[i]);}
		}
#endif
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