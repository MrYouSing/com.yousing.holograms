using UnityEngine;

namespace YouSingStudio.Holograms {
	public class MultiViewCamera
		:MonoCamera
	{
		#region Fields

		[Header("Multi-View")]
		public new Camera camera;
		public Transform focus;
		public bool preview;
		public int cone=40;
		public float plane=1.0f;
		[Tooltip("x:Pivot\ny:Scale\nz:Amount")]
		public Vector3 depth=new Vector3(0.5f,1.0f,0.0f);

		[System.NonSerialized]protected bool m_Preview;
		[System.NonSerialized]protected float m_Sweep;
		[System.NonSerialized]protected float m_Modifier;
		[System.NonSerialized]protected Vector2Int m_Size;
		[System.NonSerialized]protected Matrix4x4 m_View;
		[System.NonSerialized]protected Matrix4x4 m_Proj;
		[System.NonSerialized]protected Transform m_Viewer;
		[System.NonSerialized]protected RenderTexture m_RT0;
		[System.NonSerialized]protected RenderTexture m_RT1;

		#endregion Fields

		#region Methods
#if UNITY_EDITOR
		protected virtual Vector3 GetCameraPoint() {
			Vector3 v=new Vector3(0.0f,0.0f,-camera.GetPlaneDepth(plane));
			Transform t=focus!=null?focus:transform;
			return t.TransformPoint(v);
		}

		protected override void InternalDrawGizmos(bool selected) {
			bool d=false;
			if(camera==null) {camera=GetComponentInChildren<Camera>();d=true;}
			if(camera==null) {return;}
			if(device==null) {device=GetComponentInChildren<HologramDevice>();d=true;}
			//
			float k=GetAspect();
			Transform t=focus!=null?focus:transform;Gizmos.matrix=t.localToWorldMatrix;
			Bounds b=GetBounds(t.position);Vector3 c=b.center,e=b.extents;
			float z=camera.GetPlaneDepth(e.y*2.0f);Quaternion q=Quaternion.identity;
			float a=camera.GetPlaneHeight(z-e.z);z=camera.GetPlaneHeight(z+e.z);
			UnityExtension.DrawFrustum(q,
				new Vector3(0.0f,0.0f,c.z-e.z),new Vector2(a*k,a),// Near
				new Vector3(0.0f,0.0f,c.z+e.z),new Vector2(z*k,z));// Far
			if(selected) {Gizmos.color=Color.blue;}
			UnityExtension.DrawWirePlane(Vector3.zero,q,new Vector2(e.x*2.0f,e.y*2.0f));// Focus
			//
			if(d) {UnityEditor.EditorUtility.SetDirty(this);}
			t=camera.transform;t.hasChanged=false;
			t.position=GetCameraPoint();
			if(t.hasChanged) {UnityEditor.EditorUtility.SetDirty(t);}
		}

		[ContextMenu("Preset/Companion 1")]
		protected virtual void Preset_Companion1() {
			float f=5.7f*0.0254f;
			f=f*f/(9*9+16*16);
			f=0.06f/(16*Mathf.Sqrt(f));
			depth=new Vector3(0.5f,f,0.0f);
			cone=54;
			if(camera!=null) {
				Transform t=camera.transform;
				camera.fieldOfView=14.0f;
				t.position=GetCameraPoint();
				UnityEditor.EditorUtility.SetDirty(t);
				UnityEditor.EditorUtility.SetDirty(camera);
			}
			UnityEditor.EditorUtility.SetDirty(this);
		}
#endif
		public override void SetupCamera() {
			if(camera==null) {camera=GetComponentInChildren<Camera>();}
			if(device==null||camera==null) {return;}
			if(focus==null) {focus=transform;}
			m_Viewer=camera.transform;
			//
			Vector4 w=device.PreferredSize();int d=0;
			m_RT0=RenderTexture.GetTemporary((int)w.x,(int)w.y,d,HologramDevice.s_GraphicsFormat);
			m_RT1=RenderTexture.GetTemporary((int)(w.x/w.z),(int)(w.y/w.w),d,HologramDevice.s_GraphicsFormat);
			m_Preview=!preview;
			device.onPreRender+=Render;
			camera.enabled=false;
			camera.targetTexture=m_RT1;
			camera.aspect=device.ParseQuilt().z;
			//
			SetupCamera(m_RT0,device.ParseQuilt());
		}

		public override void DestroyCamera() {
			//base.DestroyCamera();
			//
			m_RT0.Free();m_RT1.Free();
			m_RT0=m_RT1=null;
		}

		protected virtual Rect ScreenRect(int x,int y,float w,float h) {
			//y=device.quiltSize.y-1-y;
			return new Rect(x*w,y*h,w,h);
		}

		public virtual float GetAspect() {
			if(device!=null) {return device.ParseQuilt().z;}
			if(camera!=null) {return camera.aspect;}
			return (float)Screen.width/Screen.height;
		}

		public virtual Bounds GetBounds(Vector3 point) {
			Transform t=m_Viewer!=null?m_Viewer:camera.transform;
			float h=camera.GetPlaneHeight(t.InverseTransformPoint(point).z),d=h*depth.y+depth.z;
			point=new Vector3(0.0f,0.0f,d*(0.5f-depth.x));
			return new Bounds(point,new Vector3(h*GetAspect(),h,d));
		}

		/// <summary>
		/// The bounds on focus space.
		/// </summary>
		public virtual Bounds GetBounds()=>GetBounds(focus!=null?focus.position:transform.position);

		public virtual void Render() {
			if(!isActiveAndEnabled||device==null||camera==null) {return;}
			if(preview!=m_Preview) {
				if(m_Preview=preview) {camera.targetTexture=null;}
				else {camera.targetTexture=m_RT1;}camera.enabled=m_Preview;
			}
			if(preview) {return;}
			InternalRender();
		}

		protected virtual void InternalRender() {
			m_View=camera.worldToCameraMatrix;m_Proj=camera.projectionMatrix;
			Vector3 v=m_Viewer.position;Quaternion q=m_Viewer.rotation;
			RenderTexture tmp=m_RT0.Begin();
			GL.PushMatrix();
				Vector3 arm=Quaternion.Inverse(focus.rotation)*(v-focus.position);
				m_Size=m_RT0.GetSizeI();
				m_Sweep=arm.z*Mathf.Tan(cone*0.5f*Mathf.Deg2Rad)*2;
				m_Modifier=1.0f/(plane*0.5f*GetAspect());
				GL.LoadPixelMatrix(0.0f,m_Size.x,m_Size.y,0.0f);
				int x,y,xmax=device.quiltSize.x,ymax=device.quiltSize.y;
				float w=m_Size.x/(float)xmax,h=m_Size.y/(float)ymax;int i=0,imax=xmax*ymax;
				for(y=0;y<ymax;++y) {for(x=0;x<xmax;++x,++i) {
					Place(i,imax);Render(x,y,w,h);
				}}
			GL.PopMatrix();
			m_RT0.End(tmp);
			//m_Viewer.SetPositionAndRotation(v,q);
			camera.ResetWorldToCameraMatrix();camera.ResetProjectionMatrix();
		}

		protected virtual void Place(int index,int count) {
			float a=index/(count-1.0f)-0.5f;
			Matrix4x4 v=m_View,p=m_Proj;
			v.m03+=a*m_Sweep;
			p.m02+=a*m_Sweep*m_Modifier;
			camera.worldToCameraMatrix=v;
			camera.projectionMatrix=p;
		}

		protected virtual void Render(int x,int y,float w,float h) {
			camera.Render();
			Graphics.DrawTexture(ScreenRect(x,y,w,h),m_RT1);
		}

		#endregion Methods
	}
}
