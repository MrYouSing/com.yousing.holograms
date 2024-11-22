using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	public class VirtualDisplay
		:MonoBehaviour
	{
		#region Fields

		public HologramDevice device;
		public new Camera camera;
		public Material material;
		public Canvas canvas;
		public RawImage image;
#if UNITY_EDITOR
		public RectInt rect;
#endif
		[System.NonSerialized]protected bool m_Active;
		[System.NonSerialized]protected bool m_IsCreated;
		[System.NonSerialized]protected CommandBuffer m_Buffer;

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
			RenderPipelineManager.endContextRendering+=OnEndContextRendering;
			CreateUnityDisplay();
		}

		protected virtual void OnDestroy() {
			RenderPipelineManager.endContextRendering-=OnEndContextRendering;
		}

		protected virtual void OnEndContextRendering(ScriptableRenderContext ctx,List<Camera> list){
			if(m_Active&&list.IndexOf(camera)>=0) {
				Graphics.Blit(device.canvas,material!=null?material:RenderingExtension.GetUnlit());
			}
		}

		#endregion Unity Messages

		#region Methods

		public virtual bool CreateUnityDisplay() {
			if(m_IsCreated) {return device!=null&&device.display>=0;}
			m_IsCreated=true;
			//
			if(CreateDeviceDisplay()) {
				int i=RenderingExtension.GetRenderPipelines();
				if((i&~0x7)==0) {
					return CreateCameraDisplay();
				}else {// Legacy
					return CreateCanvasDisplay();
				}
			}
			return false;
		}

		protected virtual bool CreateDeviceDisplay() {
#if !UNITY_EDITOR
			if(!device.IsPresent()) {device=null;}
#endif
			if(device==null) {return false;}
			device.Init();if(device.display<0) {
				if(camera!=null) {camera.enabled=false;}
				if(canvas!=null) {canvas.enabled=false;}
				return false;
			}
			ScreenManager.Activate(device.display);
			return true;
		}

		protected virtual bool CreateCameraDisplay() {
			//
			if(camera==null) {
				GameObject go=new GameObject(nameof(VirtualDisplay)+"@"+device.display);
				camera=go.AddComponent<Camera>();camera.cullingMask=0;
				camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
			}
			camera.targetDisplay=device.display;
			if(m_Buffer==null) {
				m_Buffer=new CommandBuffer();
				if(material!=null) {m_Buffer.Blit(device.canvas,BuiltinRenderTextureType.CameraTarget,material);}
				else {m_Buffer.Blit(device.canvas,BuiltinRenderTextureType.CameraTarget);}
			}
			camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects,m_Buffer);camera.AddCommandBuffer(CameraEvent.BeforeImageEffects,m_Buffer);
			//
			return m_Active=true;
		}

		protected virtual bool CreateCanvasDisplay() {
			//
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
			image.texture=device.canvas;image.material=material;
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
			//
			if(camera!=null) {camera.enabled=m_Active;}
			if(canvas!=null) {canvas.enabled=m_Active;}
		}

		#endregion Methods
	}
}
