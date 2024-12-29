using UnityEngine;
using UnityEngine.EventSystems;
using YouSingStudio.Private;

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
		,SnapshotManager.IActor
	{
		#region Fields

		public static Camera s_Camera;
		public static float s_PlaneScale=1.0f;
		public static bool s_AutoReset=false;

		public Transform target;
		public Transform stage;
		[Tooltip("x:T\ny:R\nz:S\nw:Wheel")]
		public Vector4 sensitivity=Vector4.one;
		[Tooltip("x:T\ny:R")]
		public Vector2 scrolling=Vector2.one;
		public int[] buttons=new int[]{0,1,2};
		public UIToggleButton[] modifiers;
		public int[] types=new int[]{0,0};
		public string[] cursors;

		[System.NonSerialized]protected int m_Action=-1;
		[System.NonSerialized]protected Pose m_Pose;
		[System.NonSerialized]protected Vector3 m_Scale;
		[System.NonSerialized]protected Vector3 m_Mouse;
		[System.NonSerialized]protected float m_Scroll;
		[System.NonSerialized]protected Vector3 m_T;
		[System.NonSerialized]protected Quaternion m_R;
		[System.NonSerialized]protected Vector3 m_S;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(target==null) {return;}
			SetTarget(target);
		}

		protected virtual void Update() {
			if(m_Action>=0) {
				OnInput();
				float k=s_PlaneScale;
					s_PlaneScale/=GetScreen();
					OnUpdate();
				s_PlaneScale=k;
			}
		}

		protected virtual void OnDisable() {
			if(didStart&&s_AutoReset) {SetAction(-1);ResetTarget();}
		}

		public virtual void OnPointerEnter(PointerEventData e) {
			SetAction(0);
		}

		public virtual void OnPointerExit(PointerEventData e) {
			SetAction(-1);SetCursor(-1);
		}

		public virtual void OnPointerDown(PointerEventData e) {
			SetAction((m_Action&0xFF00)|(1+buttons[(int)e.button]));
		}

		public virtual void OnPointerUp(PointerEventData e) {
			SetAction(m_Action&0xFF00);
		}

		#endregion Unity Messages

		#region Methods

		public static int GetModifiers(UIToggleButton[] keys) {
			int m=0;UIToggleButton it;
			for(int i=0,imax=keys?.Length??0;i<imax;++i) {
				it=keys[i];if(it!=null&&it.isActiveAndEnabled&&it.isOn) {m|=1<<i;}
			}
			return m;
		}

		public static float ToScale(float value) {
			return Mathf.Pow(2.0f,value);
		}

		public static float GetPlane(Vector3 point) {
			if(s_Camera==null) {s_Camera=Camera.main;}
			return s_Camera==null?1.0f:s_Camera.GetPlaneHeight
				(-s_Camera.worldToCameraMatrix.MultiplyPoint3x4(point).z);
		}

		public static float GetScreen() {
			return Mathf.Min(Screen.width,Screen.height)/1080.0f;
		}

		public static void SaveSnapshot(SnapshotManager.Snapshot thiz,string key,Transform value,float scale) {
			thiz.SetVector(key+"T",value.localPosition);
			thiz.SetVector(key+"R",value.localRotation.eulerAngles);
			thiz.SetFloat(key+"S",scale);
		}

		public static void LoadSnapshot(SnapshotManager.Snapshot thiz,string key,Transform value,float scale,System.Action<float> action) {
			value.localPosition=thiz.GetVector(key+"T",value.localPosition);
			value.localRotation=Quaternion.Euler(thiz.GetVector(key+"R",value.localRotation.eulerAngles));
			action?.Invoke(thiz.GetFloat(key+"S",scale));
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
			// Re-Enter.
			int a=m_Action;m_Action=-1;SetAction(a);
		}

		public virtual void SpaceTarget() {
			if(System.Array.IndexOf(types,GetModifiers(modifiers))>=0) {
				ResetTarget();
			}
		}

		public virtual void StartTarget() {
			if(!didStart) {return;}
			//
			ResetTarget();
		}

		public virtual void Save(SnapshotManager.Snapshot value) {
			if(target!=null&&value!=null) {
				SaveSnapshot(value,"Transform.",target,target.localScale.x);
			}
		}

		public virtual void Load(SnapshotManager.Snapshot value) {
			if(target!=null&&value!=null) {
				LoadSnapshot(value,"Transform.",target,target.localScale.x,(x)=>target.localScale=Vector3.one*x);
			}
		}

		protected virtual bool IsScrolling(float value) {
			if(!Mathf.Approximately(value,m_Scroll)) {
				m_Scroll=Mathf.Lerp(m_Scroll,value,0.75f);return true;
			}else {
				m_Scroll=value;return false;
			}
		}

		protected virtual void SetAction(int action) {
			if(action!=m_Action) {
				if(m_Action>=0) {OnExit();}
				m_Action=action;
				if(m_Action>=0) {OnEnter();}
			}
		}

		protected virtual void SetCursor(int cursor) {
			if(CursorManager.s_Instance!=null) {
				CursorManager.s_Instance.cursor=
					(cursor>=0&&cursor<(cursors?.Length??0))
					?cursors[cursor]:null;
			}
		}

		public virtual string Info(string fmt="0.000") {
			if(target!=null) {
				return $"T:{target.position.ToString(fmt)}\nR:{target.eulerAngles.ToString(fmt)}\nS:{target.localScale.z.ToString(fmt)}";
			}
			return "None";
		}

		protected virtual void OnInput() {
			m_Mouse.z-=Input.GetAxisRaw("Mouse ScrollWheel")*sensitivity.w;
			if(Input.GetKey(KeyCode.Space)) {m_Action=buttons.Length;}
			SetAction((GetModifiers(modifiers)<<8)|(m_Action&0xFF));
		}

		protected virtual void OnEnter() {
			m_Mouse=Input.mousePosition;m_Scroll=0.0f;
			if(target==null) {return;}
			//
			m_T=target.position;
			m_R=target.rotation;
			m_S=target.localScale;
			//
			if(stage!=null) {m_R=Quaternion.Inverse(stage.rotation)*m_R;}
		}

		protected virtual void OnUpdate() {
			if(target==null) {return;}
			//
			UpdateTransform();
		}

		protected virtual void OnExit() {
			m_Mouse=Vector3.zero;m_Scroll=0.0f;
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

		protected virtual void SetRotation(float x,float y,int m) {
			Quaternion p=Quaternion.AngleAxis(sensitivity.y*x,Vector3.right);
			Quaternion q=Quaternion.AngleAxis(sensitivity.y*y,Vector3.up);
				if(stage!=null) {q=q*stage.rotation;}
			switch(m) {
				case 0:target.rotation=q*p*m_R;break;
				case 1:target.rotation=q*m_R*p;break;
			}
		}

		protected virtual void SetRotation(float z) {
			Quaternion q=Quaternion.AngleAxis(sensitivity.y*z,Vector3.forward);
				if(stage!=null) {q=stage.rotation*q;}
			target.rotation=q*m_R;
		}

		protected virtual bool PrepareTransform(out int index,out Vector3 mouse) {
			int type=(m_Action>>8)&0xFF;
			index=System.Array.IndexOf(types,type);
			if(index<0||index>=3) {mouse=Vector3.zero;return false;}
			//
			SetCursor(0);
			mouse=Input.mousePosition-m_Mouse;
			switch(index) {
				case 1:mouse.y=0.0f;break;
				case 2:mouse.x=0.0f;break;
			}
			return true;
		}

		protected virtual void UpdateTransform() {
			if(!PrepareTransform(out var index,out var mouse)) {return;}
			//
			switch(m_Action&0xFF) {
				case 1:SetPosition(mouse.x,mouse.y);SetCursor(1);break;
				case 2:SetRotation(mouse.y,-mouse.x,0);SetCursor(2);break;// Inverse
				case 4:ResetTarget();SetCursor(4);break;
			}
			if(IsScrolling(mouse.z)) {
			switch(index) {
				case 0:
					target.localScale=m_S*ToScale(sensitivity.z*mouse.z);
					SetCursor(3);
				break;
				case 1:
					SetPosition(scrolling.x*mouse.z);
					SetCursor(5);
				break;
				case 2:
					SetRotation(scrolling.y*mouse.z);
					SetCursor(6);
				break;
			}}
		}

		#endregion Methods
	}
}
