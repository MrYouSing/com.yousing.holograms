// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

using System.Collections.Generic;
using UnityEngine;using YouSingStudio.Holograms;
using UnityEngine.Events;

namespace YouSingStudio.Private {
	public class ActiveTrigger
		:MonoBehaviour
	{
		#region Nested Types

		/// <summary>
		/// <seealso cref="UnityEngine.EventSystems.EventTriggerType"/>
		/// </summary>
		public enum TriggerType {
			 Default
			,Reverse
			,True
			,False
		}

		#endregion Nested Types

		#region Fields

		public bool active;
		[System.NonSerialized]public System.Action<bool> onActive=null;
		public List<TriggerType> types;
		public List<UnityEvent<bool>> events;

		#endregion Fields

		#region Unity Messages

		protected virtual void OnEnable()=>InvokeEvent(true);
		protected virtual void OnDisable()=>InvokeEvent(false);

		#endregion Unity Messages

		#region Methods

		public virtual void InvokeEvent(bool value) {
			active=value;
			//
			onActive?.Invoke(value);
			for(int i=0,imax=types?.Count??0;i<imax;++i) {
				InvokeEvent(types[i],events[i],value);
			}
		}

		protected virtual void InvokeEvent(TriggerType t,UnityEvent<bool> e,bool b) {
			if(e!=null) {switch(t) {
				case TriggerType.Default:e.Invoke(b);break;
				case TriggerType.Reverse:e.Invoke(!b);break;
				case TriggerType.True:if(b) {e.Invoke(b);}break;
				case TriggerType.False:if(!b) {e.Invoke(b);}break;
			}}
		}

		public virtual void ToggleEvent()=>InvokeEvent(!active);

		#endregion Methods
	}
}
