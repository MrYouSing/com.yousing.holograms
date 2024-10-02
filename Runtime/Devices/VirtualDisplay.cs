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
			//
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
			return true;
		}

		#endregion Methods
	}
}
