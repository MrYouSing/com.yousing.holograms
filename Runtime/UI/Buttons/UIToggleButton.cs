/* <!-- Macro.Include
../../Internal/UnityExtension.cs
 Macro.End --> */
/* <!-- Macro.Call DeclareEvent
protected,<bool>,ValueChanged,,bool value,value,
 Macro.End --> */
/* <!-- Macro.Patch
,Events
 Macro.End --> */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="Toggle"/>
	/// </summary>
	public class UIToggleButton
		:MonoBehaviour
		,IPointerClickHandler
	{
		#region Fields

		public Dictionary<Transform,UIToggleButton[]> s_Others=new Dictionary<Transform,UIToggleButton[]>();

		[SerializeField]protected int m_IsOn;
		[SerializeField]protected GameObject[] m_Actors=new GameObject[2];
		[SerializeField]protected bool m_IsTrigger;
		[SerializeField]protected Key[] m_Keys;
		/// <summary>
		/// <seealso cref="ToggleGroup"/>
		/// </summary>
		[SerializeField]protected bool m_IsGroup;
		/// <summary>
		/// <seealso cref="ToggleGroup.allowSwitchOff"/>
		/// </summary>
		[SerializeField]protected bool m_AllowOff;

		[System.NonSerialized]public bool isOn;
		[System.NonSerialized]protected bool m_IsDown;
		[System.NonSerialized]protected UIToggleButton[] m_Others;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			string key=name;if(key.IndexOf('.')>=0) {this.LoadSettings(key);}
			//
			if(m_IsGroup) {
				Transform p=transform.parent;if(!s_Others.TryGetValue(p,out m_Others)) {
					s_Others[p]=m_Others=p.GetComponentsInChildren<UIToggleButton>();
				}
			}
			//
			SetIsOnWithoutNotify((m_IsOn%2)==1);
		}

		protected virtual void OnEnable() {
			if(didStart) {switch(m_IsOn) {
				case 2:SetIsOn(false);break;
				case 3:SetIsOn(true);break;
			}}
		}

		protected virtual void Update() {
			var sm=Private.ShortcutManager.s_Instance;
			if(sm!=null&&(sm.locks?.Count??0)>0) {return;}
			//
			bool b=false;
			for(int i=0,imax=m_Keys?.Length??0;i<imax;++i) {
				if(Input.GetKey(m_Keys[i])) {b=true;break;}
			}
			//
			if(b!=m_IsDown) {
				m_IsDown=b;
				//
				if(m_IsTrigger) {if(m_IsDown) {SetIsOn(!isOn);}}
				else {SetIsOn(m_IsDown);}
			}
		}

		public virtual void OnPointerClick(PointerEventData eventData) {
			SetIsOn(!isOn);
			//
			UnityExtension.BlurUI();
		}

		#endregion Unity Messages

		#region Methods

		public virtual void SetIsOn(bool value) {
			if(value!=isOn) {
				SetIsOnWithoutNotify(value);
				OnValueChanged(isOn);
			}
		}

		public virtual void SetIsOnWithoutNotify(bool value) {
			//
			if(m_IsGroup) {
				if(value) {
					UIToggleButton it;bool b;
					for(int i=0,imax=m_Others?.Length??0;i<imax;++i) {
						it=m_Others[i];if(it!=null&&it.m_IsGroup&&it!=this) {
							b=it.m_AllowOff;it.m_AllowOff=true;
								it.SetIsOn(false);
							it.m_AllowOff=b;
						}
					}
				}else if(!m_AllowOff) {
					return;
				}
			}
			//
			isOn=value;int j=isOn?1:0;GameObject go;
			for(int i=0,imax=Mathf.Min(m_Actors?.Length??0,2);i<imax;++i) {
				go=m_Actors[i];if(go!=null) {go.SetActive(i==j);}
			}
		}

		#endregion Methods
// <!-- Macro.Patch Events
		[System.NonSerialized]public System.Action<bool> onValueChanged=null;
		[SerializeField]protected UnityEngine.Events.UnityEvent<bool> m_OnValueChanged=null;

		protected virtual void OnValueChanged(bool value) {
			onValueChanged?.Invoke(value);
			m_OnValueChanged?.Invoke(value);
		}

// Macro.Patch -->
	}
}
