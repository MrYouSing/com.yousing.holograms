// TODO: RotationOrder????
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class UITransformEditor
		:MonoBehaviour
	{
		#region Fields

		public Transform target;
		public int mode;
		public float threshold=-1.0f;
		public UISliderView[] views=new UISliderView[3];
		[SerializeField]protected Vector3 m_Value=new Vector3(float.NaN,float.NaN,float.NaN);

		[System.NonSerialized]protected bool m_Enabling;
		[System.NonSerialized]protected IPlane m_Plane;
		[System.NonSerialized]protected Vector2[] m_Ranges;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(target==null) {target=transform;}
			if(float.IsNaN(m_Value.x)) {m_Value=Get();}
			//
			int i=0,imax=views?.Length??0;m_Ranges=new Vector2[imax];
			UISliderView v;for(;i<imax;++i) {
				v=views[i];if(v!=null) {
					m_Ranges[i]=v.GetRange();
					v.onSliderChanged+=OnValueChanged;
				}
			}
			//
			Set(m_Value);
		}

		protected virtual void OnEnable() {
			m_Enabling=true;
			for(int i=0,imax=m_Ranges?.Length??0;i<imax;++i) {
				views[i].SetRange(m_Ranges[i]);
			}
			m_Enabling=false;
		}

		protected virtual void LateUpdate() {
			if(threshold>=0.0f&&target!=null) {
				Vector3 v=Get();
				if((m_Value-v).sqrMagnitude>threshold*threshold) {Set(v);}
			}
		}

		#endregion Unity Messages

		#region Methods

		public static void FixAngle(ref float value,float min,float max) {
			if(min!=max) {
			if(value<min||value>max) {
				value=min+Mathf.Repeat(value-min,max-min);
			}}
		}

		public static Vector3 FixAngles(Vector3 value,Vector2 x,Vector2 y,Vector2 z) {
			FixAngle(ref value.x,x.x,x.y);
			FixAngle(ref value.y,y.x,y.y);
			FixAngle(ref value.z,z.x,z.y);
			return value;
		}

		protected virtual void FixAngles() {
			if(!didStart) {return;}
			//
			m_Value=FixAngles(m_Value,m_Ranges[0],m_Ranges[1],m_Ranges[2]);
		}

		public virtual Vector3 Get() {
			if(target!=null) {
			switch(mode) {
				case 0x0:
					m_Value=target.localPosition;
				break;
				case 0x1:
					m_Value=target.localEulerAngles;
					FixAngles();
				break;
				case 0x2:
					m_Value=target.localScale;
				break;
				case 0x3:
					m_Value=m_Plane!=null?new Vector3(m_Plane.size
						,float.NaN,float.NaN):target.localScale;
				break;
				case 0x100:
					m_Value=target.position;
				break;
				case 0x101:
					m_Value=target.eulerAngles;
					FixAngles();
				break;
				case 0x102:
					m_Value=target.lossyScale;
				break;
			}}
			return m_Value;
		}

		public virtual void Set(Vector3 value) {
			m_Value=value;
			if(target!=null) {
			switch(mode) {
				case 0x0:
					target.localPosition=m_Value;
				break;
				case 0x1:
					FixAngles();
					target.localEulerAngles=m_Value;
				break;
				case 0x2:
					target.localScale=m_Value;
				break;
				case 0x3:
					if(m_Plane!=null) {m_Plane.size=m_Value.x;}
					else {target.localScale=m_Value;}
				break;
				case 0x100:
					target.position=m_Value;
				break;
				case 0x101:
					FixAngles();
					target.eulerAngles=m_Value;
				break;
				case 0x102:
					target.localScale=m_Value;
				break;
			}}
			Render();
		}

		public virtual void Render() {
			for(int i=0,imax=views?.Length??0;i<imax;++i) {
				views[i].SetValueWithoutNotify(m_Value[i]);
			}
		}

		protected virtual void OnValueChanged() {
			if(m_Enabling) {return;}
			//
			Vector3 v=Vector3.zero;
			for(int i=0,imax=views?.Length??0;i<imax;++i) {
				v[i]=views[i].GetValue(0.0f);
			}
			Set(v);
		}

		protected virtual void OnPlaneChanged() {
			bool b=m_Plane==null;Vector2 v=views[0].GetRange();
			UISliderView it;for(int i=1;i<3;++i) {
				it=views[i];if(it!=null) {
					it.SetActive(b);
					it.SetRange(v);
				}
			}
		}

		public virtual void Set(Transform value) {
			target=value;
			if(target!=null) {
				//
				switch(mode) {
					case 0x3:
						m_Plane=target.GetComponent<IPlane>();
						OnPlaneChanged();
					break;
				}
				//
				m_Value=Get();
				Render();
			}
		}

		#endregion Methods
	}
}
