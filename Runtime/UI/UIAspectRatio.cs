using UnityEngine;
using UnityEngine.Video;
using YouSingStudio.Private;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="VideoAspectRatio"/>
	/// </summary>
	public class UIAspectRatio
		:ScriptableView<VideoAspectRatio>
	{
		#region Fields

		public Key[] keys;
		public VideoAspectRatio[] values;

		[System.NonSerialized]protected bool m_Dirty;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			var sm=ShortcutManager.instance;
			for(int i=0,imax=keys?.Length??0;i<imax;++i) {
				int n=i;VideoAspectRatio v=values[i];
				sm.Add(name+"."+v,()=>OnValueChanged(v),GetKeys(keys[i]));
				BindToggle(i,(x)=>OnToggleChanged(n,x));
			}
		}

		protected virtual void Update() {
			if(m_Dirty) {
				m_Dirty=false;
				//
				for(int i=0,imax=keys?.Length??0;i<imax;++i) {
					if(GetToggle(i)) {OnValueChanged(values[i]);return;}
				}
			}
		}

		#endregion Unity Messages

		#region Methods

		protected virtual Key[] GetKeys(Key key) {
			if(key!=Key.None) {return new Key[]{key};}
			else {return null;}
		}

		protected virtual void OnToggleChanged(int i,bool b) {
			if(isActiveAndEnabled) {
				m_Dirty=true;
			}else if(b) {
				OnValueChanged(values[i]);
			}
		}

		#endregion Methods
	}
}
