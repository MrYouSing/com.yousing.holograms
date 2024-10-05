using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Rendering;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// Attempt to convert 3d/vr media to hologram.<br/>
	/// <seealso cref="Camera.stereoTargetEye"/>
	/// </summary>
	public class StereoCamera
		:MonoCamera
	{
		#region Fields

		public static readonly int _StereoEyeIndex=Shader.PropertyToID("_StereoEyeIndex");

		[Header("Stereo")]
		public RenderTexture texture;
		public float aspect=1.0f;
		public Video3DLayout layout;
		public Camera[] cameras;

		#endregion Fields

		#region Methods

		public override void SetupCamera() {
			if(GraphicsSettings.renderPipelineAsset!=null) {
				GraphicsSettings.renderPipelineAsset=null;
				Debug.Log("Disable SRP for camera messages");
			}
			Shader.SetGlobalFloat(_StereoEyeIndex,0.0f);
			texture.Clear();
			for(int i=0,imax=cameras?.Length??0;i<imax;++i) {
				SetupCamera(cameras[i],i);
			}
			SetupCamera(texture,new Vector3(2.0f,1.0f,aspect));
		}

		protected virtual Rect GetRect(int index) {
			switch(layout) {
				case Video3DLayout.SideBySide3D:return new Rect(0.5f*index,0.0f,0.5f,1.0f);
				case Video3DLayout.OverUnder3D:return new Rect(0.0f,0.5f*index,1.0f,0.5f);
				default:return new Rect(Vector2.zero,Vector2.one);
			}
		}

		protected virtual void SetupCamera(Camera camera,int index) {
			if(camera!=null) {
				if(index>0) {
					Camera c=cameras[0];camera.CopyFrom(c);
					Vector3 v=c.transform.localPosition;
					v.x*=-1.0f;camera.transform.localPosition=v;
				}
				Rect rect=GetRect(index);
				camera.depth=index;
				camera.stereoTargetEye=(StereoTargetEyeMask)(1+index);
				camera.targetTexture=texture;
				camera.pixelRect=new Rect(0.0f,0.0f,texture.width*rect.width,texture.height*rect.height);
				camera.rect=rect;
				camera.aspect=aspect;
				//
				CommandBuffer cb=new CommandBuffer();
				cb.SetGlobalFloat(_StereoEyeIndex,index);
				camera.AddCommandBuffer(CameraEvent.BeforeDepthTexture,cb);
			}
		}

		#endregion Methods
	}
}
