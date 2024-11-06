/* TODO:
1) Cursor.Icon
2) UI.Scale
 */
using UnityEngine;
using UnityEngine.EventSystems;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="UnityEngine.UIElements.Manipulator"/>
	/// </summary>
	public class UITransformManipulator
		:MonoBehaviour
		,IPointerEnterHandler
		,IPointerExitHandler
		,IPointerDownHandler
		,IPointerUpHandler
	{
		#region Fields

		public static Camera s_Camera;
		public static float s_PlaneScale=1.0f;

		public Transform target;
		[Tooltip("x:T\ny:R\nz:S\nw:Wheel")]
		public Vector4 sensitivity=Vector4.one;
		public int[] buttons=new int[]{0,1,2};
		public KeyCode[] modifiers=new KeyCode[]{
			KeyCode.RightShift+0,KeyCode.RightShift+1,KeyCode.RightShift+2,
			KeyCode.RightShift+3,KeyCode.RightShift+4,KeyCode.RightShift+5,
		};
		[Header("Misc")]// For resolving target role.
		public Transform stage;
		public Transform viewer;
		public new Collider collider;

		[System.NonSerialized]protected int m_Action=-1;
		[System.NonSerialized]protected Vector3 m_Start;
		[System.NonSerialized]protected Pose m_Pose;
		[System.NonSerialized]protected Vector3 m_Scale;
		[System.NonSerialized]protected Vector3 m_Mouse;
		[System.NonSerialized]protected Vector3 m_V;
		[System.NonSerialized]protected Vector3 m_T;
		[System.NonSerialized]protected Quaternion m_R;
		[System.NonSerialized]protected Vector3 m_S;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(viewer!=null) {
				s_Camera=viewer.GetComponent<Camera>();
				m_Start=new Vector3(0.0f,GetPlane(target.position),viewer.localPosition.z);
			}
			SetTarget(target);
		}

		protected virtual void Update() {
			if(m_Action>=0) {
				OnInput();
				float k=s_PlaneScale;
					OnUpdate();
				s_PlaneScale=k;
			}
		}

		protected virtual void OnDisable() {
			if(didStart) {SetAction(-1);ResetTarget();}
		}

		public virtual void OnPointerEnter(PointerEventData e) {
			SetAction(0);
		}

		public virtual void OnPointerExit(PointerEventData e) {
			SetAction(-1);
		}

		public virtual void OnPointerDown(PointerEventData e) {
			SetAction((m_Action&0xFF00)|(1+buttons[(int)e.button]));
		}

		public virtual void OnPointerUp(PointerEventData e) {
			SetAction(m_Action&0xFF00);
		}

		#endregion Unity Messages

		#region Methods

		public static int GetModifiers(KeyCode[] keys) {
			int m=0;for(int i=0,imax=(keys?.Length??0)/2;i<imax;++i) {
				if(Input.GetKey(keys[2*i])||Input.GetKey(keys[2*i+1])) {m|=1<<i;}
			}return m;
		}

		public static float ToScale(float value) {
			return Mathf.Pow(2.0f,value);
		}

		public static float GetPlane(Vector3 point) {
			if(s_Camera==null) {s_Camera=Camera.main;}
			return s_Camera==null?1.0f:s_Camera.GetPlaneHeight
				(s_Camera.worldToCameraMatrix.MultiplyPoint3x4(point).z);
		}

		public virtual void SetTarget(Transform value) {
			target=value;
			if(target!=null) {
				m_Pose=new Pose(target.position,target.rotation);
				m_Scale=target.localScale;
			}
		}

		public virtual void ResetTarget() {
			if(target!=null) {
				target.SetLocalPositionAndRotation(m_Pose.position,m_Pose.rotation);
				target.localScale=m_Scale;
			}
			if(viewer!=null) {
				viewer.localPosition=new Vector3(0.0f,0.0f,m_Start.z);
			}
			int a=m_Action;m_Action=-1;SetAction(a);
		}

		public virtual void FitTarget() {
			if(target!=null&&collider!=null) {
				Bounds a=target.gameObject.GetBounds();
				Bounds b=collider.bounds;int n=0;Vector3 s=a.size;
				Vector3 p=target.InverseTransformPoint(a.center);
				if(collider.isTrigger) {// Clamp : Inside
					if(s.y>s.x) {n=1;}
					if(s.z>s.y) {n=2;}
				}else {// Free : Outside
					if(s.y<s.x) {n=1;}
					if(s.z<s.y) {n=2;}
				}
				target.localScale=Vector3.one*(b.size[n]/s[n]);
				target.position+=b.center-target.TransformPoint(p);
			}
		}

		protected virtual void SetAction(int action) {
			if(action!=m_Action) {
				if(m_Action>=0) {OnExit();}
				m_Action=action;
				if(m_Action>=0) {OnEnter();}
			}
		}

		protected virtual void OnInput() {
			m_Mouse.z-=Input.GetAxisRaw("Mouse ScrollWheel")*sensitivity.w;
			SetAction((GetModifiers(modifiers)<<8)|(m_Action&0xFF));
			if(viewer!=null) {s_PlaneScale=GetPlane(target.position)/m_Start.y;}
		}

		protected virtual void OnEnter() {
			m_Mouse=Input.mousePosition;
			if(target==null) {return;}
			//
			m_T=target.position;
			m_R=target.rotation;
			m_S=target.localScale;
			//
			if(viewer!=null) {m_V=viewer.localPosition;}
			if(stage!=null) {m_R=Quaternion.Inverse(stage.rotation)*m_R;}
		}

		protected virtual void OnUpdate() {
			if(target==null) {return;}
			//
			if(viewer!=null) {UpdateCamera();}
			else {UpdateActor();}
		}

		protected virtual void OnExit() {
			m_Mouse=Vector3.zero;
			m_V=Vector3.zero;
			m_T=Vector3.zero;
			m_R=Quaternion.identity;
			m_S=Vector3.one;
		}

		// Implementation

		protected virtual void SetPosition(float x,float y) {
			Vector3 v=new Vector3(sensitivity.x*x*s_PlaneScale,sensitivity.x*y*s_PlaneScale,0.0f);
				if(stage!=null) {v=stage.rotation*v;}
			target.position=m_T+v;
		}

		protected virtual void SetPosition(float z) {
			Vector3 v=new Vector3(0.0f,0.0f,sensitivity.x*z);//*s_PlaneScale;
				if(stage!=null) {v=stage.rotation*v;}
			target.position=m_T+v;
		}

		protected virtual void SetRotation(float x,float y) {
			Quaternion p=Quaternion.AngleAxis(sensitivity.y*x,Vector3.right);
			Quaternion q=Quaternion.AngleAxis(sensitivity.y*y,Vector3.up);
				if(stage!=null) {q=q*stage.rotation;}
			if(viewer!=null) {target.rotation=q*m_R*p;}
			else {target.rotation=q*p*m_R;}
		}

		protected virtual void SetRotation(float z) {
			Quaternion q=Quaternion.AngleAxis(sensitivity.y*z,Vector3.forward);
				if(stage!=null) {q=stage.rotation*q;}
			target.rotation=q*m_R;
		}

		protected virtual void UpdateCamera() {
			if((m_Action&0xFF00)!=0) {return;}
			//
			Vector3 mouse=Input.mousePosition-m_Mouse;
			switch(m_Action&0xFF) {
				case 1:SetPosition(mouse.x,mouse.y);break;
				case 2:SetRotation(-mouse.y,mouse.x);break;
				case 3:ResetTarget();break;
			}
			mouse=new Vector3(0.0f,0.0f,sensitivity.x*mouse.z);
			if(viewer!=null) {viewer.localPosition=m_V+mouse;}
			else {target.position=m_T+mouse;}
		}

		protected virtual void UpdateActor() {
			Vector3 mouse=Input.mousePosition-m_Mouse;
			switch(m_Action&0xFF00) {
				case 0x0100:// Shift
				switch(m_Action&0xFF) {
					case 1:SetPosition(mouse.x,mouse.y);break;
					case 2:SetRotation(mouse.y,-mouse.x);break;// Inverse
					case 3:ResetTarget();break;
				}
				break;
				case 0x0300:// Shift+Ctrl
				switch(m_Action&0xFF) {
					case 1:SetPosition(mouse.y);break;
					case 2:SetRotation(mouse.y);break;// Clockwise TODO: Two Parts????
					case 3:FitTarget();break;
				}
				break;
				default:return;
			}
			target.localScale=m_S*ToScale(sensitivity.z*mouse.z);
		}

		public virtual string Info(string fmt="0.000") {
			if(target!=null) {
				if(viewer!=null) {
					return $"T:{target.position.ToString(fmt)}\nR:{target.eulerAngles.ToString(fmt)}\nZ:{viewer.localPosition.z.ToString(fmt)}";
				}else {
					return $"T:{target.position.ToString(fmt)}\nR:{target.eulerAngles.ToString(fmt)}\nS:{target.localScale.z.ToString(fmt)}";
				}
			}
			return "None";
		}

		#endregion Methods
	}
}
