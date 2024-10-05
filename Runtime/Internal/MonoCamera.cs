using UnityEngine;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="Camera"/>
	/// </summary>
	public class MonoCamera
		:MonoBehaviour
	{
		#region Fields

		public HologramDevice device;
		public QuiltTexture quilt;
		public VirtualDisplay display;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			PrepareCamera();
			if(device!=null&&display!=null) {SetupCamera();}
		}

		protected virtual void OnDestroy() {
			DestroyCamera();
		}

		#endregion Unity Messages

		#region Methods

		public virtual void PrepareCamera() {
			if(device==null) {device=FindAnyObjectByType<HologramDevice>();}
			if(display==null) {
				display=FindAnyObjectByType<VirtualDisplay>();
				if(display==null) {
					display=this.NewActor<VirtualDisplay>("Display");
					display.device=device;
				}
			}
		}

		public virtual void SetupCamera()=>throw new System.NotImplementedException();

		public virtual void SetupCamera(Texture texture,Vector3 vector) {
			//
			display.device=device;
			if(!display.CreateUnityDisplay()) {
#if !UNITY_EDITOR
				return;
#endif
			}
			//
			Vector3 v=device.ParseQuilt();if(vector!=v) {
				if(quilt==null) {
					quilt=this.NewActor<QuiltTexture>("Quilt");
					//quilt.aspect=UnityEngine.Video.VideoAspectRatio.FitOutside;
				}
				//
				if(quilt.destination==null) {
				if(!(device.quiltTexture is RenderTexture)) {
					Vector4 w=device.PreferredSize();
					device.quiltTexture=RenderTexture.GetTemporary((int)w.x,(int)w.y,0,HologramDevice.s_GraphicsFormat);
					device.quiltTexture.name=UnityExtension.s_TempTag;
				}}else {
					device.quiltTexture=quilt.destination;
				}
				//
				quilt.enabled=true;
				quilt.SetDestination(device);
				quilt.SetSource(texture,vector);
			}else {
				device.quiltTexture=texture;
				if(quilt!=null) {quilt.enabled=false;}
			} 
		}

		public virtual void DestroyCamera() {
			if(device!=null&&device.quiltTexture.IsTemporary()) {
				RenderTexture.ReleaseTemporary(device.quiltTexture as RenderTexture);
				device.quiltTexture=null;
			}
		}

		#endregion Methods
	}
}
