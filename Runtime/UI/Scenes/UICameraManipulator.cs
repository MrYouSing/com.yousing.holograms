using UnityEngine;

namespace YouSingStudio.Holograms {
	public class UICameraManipulator
		:UITransformManipulator
	{
		#region Fields

		[Header("Camera")]
		public Transform viewer;
		[SerializeField]protected Object m_Plane;
		[System.NonSerialized]public IPlane plane;

		[System.NonSerialized]protected float m_V;
		[System.NonSerialized]protected Vector3 m_Start;

		#endregion Fields

		#region Unity Messages

		protected override void Start() {
			if(target==null) {return;}
			//
			if(viewer!=null) {s_Camera=viewer.GetComponent<Camera>();}
			if(plane==null&&m_Plane!=null) {plane=m_Plane as IPlane;}
			m_Start=new Vector3(0.0f,GetPlane(target.position),GetFov());
			//
			base.Start();
		}

		#endregion Unity Messages

		#region Methods

		public override void ResetTarget() {
			base.ResetTarget();
			SetFov(m_Start.z);
		}

		public override void Save(Private.SnapshotManager.Snapshot value) {
			if(target!=null&&value!=null) {
				SaveSnapshot(value,"Camera.",target,GetFov());
			}
		}

		public override void Load(Private.SnapshotManager.Snapshot value) {
			if(target!=null&&value!=null) {
				LoadSnapshot(value,"Camera.",target,GetFov(),SetFov);
			}
		}

		public override string Info(string fmt="0.000") {
			if(target!=null) {
				return $"T:{target.position.ToString(fmt)}\nR:{target.eulerAngles.ToString(fmt)}\nV:{GetFov().ToString(fmt)}";
			}
			return "None";
		}

		protected override void OnInput() {
			base.OnInput();
			//
			s_PlaneScale=GetPlane(target.position)/m_Start.y;
		}

		protected override void OnEnter() {
			base.OnEnter();
			//
			m_V=GetFov();
		}

		protected override void OnExit() {
			base.OnExit();
			//
			m_V=0.0f;
		}

		// Implementation

		protected virtual float GetFov() {
			if(plane!=null) {return plane.size;}
			else if(viewer!=null) {return viewer.localPosition.z;}
			return 0.0f;
		}

		protected virtual void SetFov(float value) {
			if(plane!=null) {plane.size=value;}
			else if(viewer!=null) {viewer.localPosition=new Vector3(0.0f,0.0f,value);}
		}

		protected virtual void MoveFov(float value) {
			if(plane!=null) {plane.size=m_V/ToScale(value);}
			else if(viewer!=null) {viewer.localPosition=new Vector3(0.0f,0.0f,m_V+value);}
		}

		protected override void UpdateTransform() {
			if(!PrepareTransform(out var index,out var mouse)) {return;}
			//
			switch(m_Action&0xFF) {
				case 1:SetPosition(mouse.x,mouse.y);SetCursor(1);break;
				case 2:SetRotation(-mouse.y,mouse.x,1);SetCursor(2);break;
				case 4:ResetTarget();SetCursor(4);break;
			}
			if(IsScrolling(mouse.z)) {
				MoveFov(sensitivity.x*mouse.z);
				SetCursor(3);
			}
		}

		#endregion Methods
	}
}