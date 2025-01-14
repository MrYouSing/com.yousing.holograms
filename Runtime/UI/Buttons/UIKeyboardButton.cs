using UnityEngine;
using UnityEngine.UI;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="TouchScreenKeyboard"/>
	/// </summary>
	public class UIKeyboardButton
		:MonoBehaviour
	{
		#region Fields

		public Button button;
		public bool trigger=true;
		public Key[] keys;
		[System.NonSerialized]protected bool m_IsDown;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			string key=name;if(key.IndexOf('.')>=0) {this.LoadSettings(key);}
			//
			if(button==null) {button=GetComponent<Button>();}
		}

		protected virtual void OnEnable() {
			m_IsDown=false;
			button.SetPressed(false);
		}

		protected virtual void Update() {
			var sm=Private.ShortcutManager.s_Instance;
			if(sm!=null&&(sm.locks?.Count??0)>0) {return;}
			//
			bool b=false;
			for(int i=0,imax=keys?.Length??0;i<imax;++i) {
				if(Input.GetKey(keys[i])) {b=true;break;}
			}
			//
			if(b!=m_IsDown) {
				m_IsDown=b;
				if(button!=null) {
					b=false;
					if(m_IsDown) {
						b=trigger;
						button.SetPressed(true);
					}else {
						b=!trigger;
						button.SetPressed(false);
					}
					if(b) {button.onClick?.Invoke();}
				}
			}
		}

		#endregion Unity Messages

		#region Methods
		#endregion Methods
	}
}
