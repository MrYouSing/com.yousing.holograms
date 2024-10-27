using UnityEngine;
using UnityEngine.UI;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	public class VirtualDisplay
		:MonoBehaviour
	{
		#region Fields

		public HologramDevice device;
		public Canvas canvas;
		public RawImage image;
#if UNITY_EDITOR
		public RectInt rect;
#endif
		[System.NonSerialized]protected bool m_Active;
		[System.NonSerialized]protected bool m_IsCreated;

		#endregion Fields

		#region Unity Messages
#if UNITY_EDITOR
		protected virtual void Awake() {
			if(device!=null&&device.display<0) {
				device.display=ScreenManager.SetupDisplay(-1,rect);
			}
		}
#endif
		protected virtual void Start() {
			CreateUnityDisplay();
		}

		#endregion Unity Messages

		#region Methods

		public virtual bool CreateUnityDisplay() {
			if(m_IsCreated) {return device!=null&&device.display>=0;}
			m_IsCreated=true;
			//
#if !UNITY_EDITOR
			if(!device.IsPresent()) {device=null;}
#endif
			if(device==null) {return false;}device.Init();
			if(device.display<0) {
				if(canvas!=null) {canvas.enabled=false;}return false;
			}
			//
			ScreenManager.Activate(device.display);
			if(canvas==null) {
				GameObject go=new GameObject(nameof(VirtualDisplay)+"@"+device.display);
				canvas=go.AddComponent<Canvas>();
				canvas.renderMode=RenderMode.ScreenSpaceOverlay;
				canvas.sortingOrder=1000;
			}
			canvas.targetDisplay=device.display;
			if(image==null) {
				image=canvas.GetComponentInChildren<RawImage>();
				if(image==null) {
					GameObject go=new GameObject("RawImage");
					image=go.AddComponent<RawImage>();
				}
			}
			image.rectTransform.StretchParent(canvas.transform);
			image.texture=device.canvas;
			//
			return m_Active=true;
		}

		public virtual bool isActive=>m_IsCreated?m_Active:false;

		public virtual void SetActive(bool value) {
			if(!m_IsCreated) {CreateUnityDisplay();}
			if(device==null) {return;}
			if(value==m_Active) {return;}
			//
			if(m_Active=value) {ScreenManager.Activate(device.display);}
			else {ScreenManager.Deactivate(device.display);}
			canvas.enabled=m_Active;
		}

		#endregion Methods
	}
}
