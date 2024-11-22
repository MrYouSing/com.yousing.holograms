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

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(target==null) {target=transform;}
			if(float.IsNaN(m_Value.x)) {m_Value=Get();}
			Set(m_Value);
			for(int i=0,imax=views?.Length??0;i<imax;++i) {
				views[i].onSliderChanged+=OnValueChanged;
			}
		}

		protected virtual void LateUpdate() {
			if(threshold>=0.0f&&target!=null) {
				Vector3 v=Get();
				if((m_Value-v).sqrMagnitude>threshold*threshold) {Set(v);}
			}
		}

		#endregion Unity Messages

		#region Methods

		public virtual Vector3 Get() {
			if(target!=null) {
			switch(mode) {
				case 0x0:
					m_Value=target.localPosition;
				break;
				case 0x1:
					m_Value=target.localEulerAngles;
				break;
				case 0x2:
					m_Value=target.localScale;
				break;
				case 0x100:
					m_Value=target.position;
				break;
				case 0x101:
					m_Value=target.eulerAngles;
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
					target.localEulerAngles=m_Value;
				break;
				case 0x2:
					target.localScale=m_Value;
				break;
				case 0x100:
					target.position=m_Value;
				break;
				case 0x101:
					target.eulerAngles=m_Value;
				break;
				case 0x102:
					target.localScale=m_Value;
				break;
			}}
			UpdateUI();
		}

		protected virtual void UpdateUI() {
			for(int i=0,imax=views?.Length??0;i<imax;++i) {
				views[i].SetValueWithoutNotify(m_Value[i]);
			}
		}

		protected virtual void OnValueChanged() {
			Vector3 v=Vector3.zero;
			for(int i=0,imax=views?.Length??0;i<imax;++i) {
				m_Value[i]=views[i].GetValue(0.0f);
			}
			Set(v);
		}

		public virtual void Set(Transform value) {
			target=value;
			if(target!=null) {
				Set(Get());
			}
		}

		#endregion Methods
	}
}
