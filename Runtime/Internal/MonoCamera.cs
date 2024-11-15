using UnityEngine;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="Camera"/>
	/// </summary>
	public class MonoCamera
		:MonoBehaviour
	{
		#region Fields

		public static bool s_AllowDummy=false;

		public HologramDevice device;
		public QuiltRenderer quilt;
		public VirtualDisplay display;
		public HologramDevice[] devices;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			PrepareCamera();
			if(device!=null&&display!=null) {
#if !UNITY_EDITOR
				if(s_AllowDummy||device.IsPresent())
#endif
				SetupCamera();
			}
		}

		protected virtual void OnDestroy() {
			DestroyCamera();
		}

		protected virtual void OnEnable (){
			if(device!=null) {device.enabled=true;}
		}

		protected virtual void OnDisable (){
			if(device!=null) {device.enabled=false;}
		}
#if UNITY_EDITOR
		protected virtual void OnDrawGizmos()=>DrawGizmos(false);
		protected virtual void OnDrawGizmosSelected()=>DrawGizmos(true);

		protected virtual void DrawGizmos(bool selected) {
			Color c=Gizmos.color;
			Matrix4x4 m=Gizmos.matrix;
				Gizmos.color=selected?Color.green:Color.gray;
				InternalDrawGizmos(selected);
			Gizmos.color=c;
			Gizmos.matrix=m;
		}

		protected virtual void InternalDrawGizmos(bool selected) {}
#endif
		#endregion Unity Messages

		#region Methods

		public virtual void PrepareCamera() {
			//
			HologramDevice it;for(int i=0,imax=devices?.Length??0;i<imax;++i) {
				it=devices[i];if(it!=null) {
					if(it.IsPresent()) {
						if(device!=null) {device.gameObject.SetActive(false);}
						device=it;break;
					}else {
						it.gameObject.SetActive(false);
					}
				}
			}
			//
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
					quilt=this.NewActor<QuiltRenderer>("Quilt");
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
				if(texture!=null) {device.quiltTexture=texture;}
				if(quilt!=null) {quilt.enabled=false;}
			}
			if(MonoApplication.s_Instance!=null) {
				MonoApplication.s_Instance.SetupCamera(this);
			}
		}

		public virtual void DestroyCamera() {
			if(device!=null&&device.quiltTexture.IsTemporary()) {
				(device.quiltTexture as RenderTexture).Free();
				device.quiltTexture=null;
			}
		}

		#endregion Methods
	}
}
