// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

using UnityEngine;using YouSingStudio.Holograms;
using UnityEngine.EventSystems;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Private {
	public class ShortcutBehaviour
		:MonoBehaviour
	{
		#region Fields

		public static PointerEventData s_PointerEventData=null;

		public Key[] keys;
		[System.NonSerialized]public ShortcutManager.Shortcut shortcut;

		#endregion Fields

		#region Unity Messages

		protected virtual void OnEnable()=>SetActive(true);
		protected virtual void OnDisable()=>SetActive(false);

		#endregion Unity Messages

		#region Methods

		public virtual void SetActive(bool value) {
			if(value) {
				if(shortcut==null) {shortcut=ShortcutManager.instance.Add(name,OnClick,keys);}
			}else {
				if(shortcut!=null) {shortcut.Dispose();}shortcut=null;
			}
		}

		public virtual void OnClick() {
			// TODO: Added ugui extensions?
			if(s_PointerEventData==null) {s_PointerEventData=new PointerEventData(null);}
			ExecuteEvents.Execute(gameObject,s_PointerEventData,ExecuteEvents.pointerClickHandler);
		}

		#endregion Methods
	}
}
