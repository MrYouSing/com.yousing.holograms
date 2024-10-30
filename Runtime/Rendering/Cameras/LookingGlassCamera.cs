#if ENABLE_LKG
using System.Collections;
using LookingGlass;
using LookingGlass.Toolkit;
#endif
using UnityEngine;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// Use LookingGlass SDK to render other devices.
	/// </summary>
	public class LookingGlassCamera
		:MonoCamera
	{
		#region Fields
#if ENABLE_LKG
		public new HologramCamera camera;
#else
		public new Object camera;
#endif
		#endregion Fields

		#region Methods

		public override void SetupCamera() {
#if ENABLE_LKG
			if(camera==null) {camera=FindAnyObjectByType<HologramCamera>();}
			//
			//camera.ForceDisplayIndex=false;
			HologramDevice.s_GraphicsFormat=camera.QuiltTexture.graphicsFormat;// GetQuiltFormat().
			if(camera.UseQuiltAsset) {
				StartCoroutine(SetupCameraDelayed(0));
			}else {
				StartCoroutine(SetupCameraDelayed(0x0501));
			}
		}

		public virtual IEnumerator SetupCameraDelayed(int mask) {
			bool b=(mask&0x1)!=0;int i=(mask>>8)&0xFF;
			while(b&&LKGDisplaySystem.IsLoading) {yield return null;}
			while(i-->0) {yield return null;}
			//
			Vector3 u=device.ParseQuilt();Vector4 v=device.PreferredSize();
			QuiltSettings qs=new QuiltSettings();
			qs.quiltWidth=(int)v.x;qs.quiltHeight=(int)v.y;
			qs.columns=(int)v.z;qs.rows=(int)v.w;qs.ResetTileCount();
			qs.renderAspect=Mathf.Abs(u.z);u.z=-qs.renderAspect;
			//
			camera.UseCustomQuiltSettings(qs);
			SetupCamera(camera.QuiltTexture,u);
#endif
		}

		#endregion Methods
	}
}
