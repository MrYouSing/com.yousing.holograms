/* <!-- Macro.Include
../Private/ScriptableView.cs
 Macro.End --> */
/* <!-- Macro.Table Slider
X,x,
Y,y,
Z,z,
 Macro.End --> */
/* <!-- Macro.Table InputField
X,x,
Y,y,
Z,z,
 Macro.End --> */
/* <!-- Macro.Call  Slider
		public Vector3 range{0};

		protected virtual void Set{0}(float value) {{
			m_Value.{1}=value;Set(m_Value);
		}}

		protected virtual void Set{0}(string value) {{
			if(float.TryParse(value,out var tmp)) {{Set{0}(tmp);}}
		}}

 Macro.End --> */
/* <!-- Macro.BatchCall DeclareKeys Slider
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */

/* <!-- Macro.Call  Slider
			SetSlider(k_{0},range{0});
 Macro.End --> */
/* <!-- Macro.Call BindSet Slider
 Macro.End --> */
/* <!-- Macro.Call BindSet InputField
 Macro.End --> */
/* <!-- Macro.Patch
,Start
 Macro.End --> */
using UnityEngine;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	public class UITransformEditor
		:ScriptableView<Vector3>
	{
		#region Fields

		public Transform target;
		public int mode;
		public string format="0.00";
		[SerializeField]protected Vector3 m_Value=new Vector3(float.NaN,float.NaN,float.NaN);

		#endregion Fields
// <!-- Macro.Patch AutoGen
		public Vector3 rangeX;

		protected virtual void SetX(float value) {
			m_Value.x=value;Set(m_Value);
		}

		protected virtual void SetX(string value) {
			if(float.TryParse(value,out var tmp)) {SetX(tmp);}
		}

		public Vector3 rangeY;

		protected virtual void SetY(float value) {
			m_Value.y=value;Set(m_Value);
		}

		protected virtual void SetY(string value) {
			if(float.TryParse(value,out var tmp)) {SetY(tmp);}
		}

		public Vector3 rangeZ;

		protected virtual void SetZ(float value) {
			m_Value.z=value;Set(m_Value);
		}

		protected virtual void SetZ(string value) {
			if(float.TryParse(value,out var tmp)) {SetZ(tmp);}
		}

		public const int k_X=0;
		public const int k_Y=1;
		public const int k_Z=2;
#if UNITY_EDITOR
		protected virtual string[] GetSliders()=>new string[]{
			"X",
			"Y",
			"Z",
		};
#endif

// Macro.Patch -->

		#region Unity Messages

		protected virtual void Start() {
			if(target==null) {target=transform;}
			if(float.IsNaN(m_Value.x)) {m_Value=Get();}
			Set(m_Value);
// <!-- Macro.Patch Start
			SetSlider(k_X,rangeX);
			SetSlider(k_Y,rangeY);
			SetSlider(k_Z,rangeZ);
			BindSlider(k_X,SetX);
			BindSlider(k_Y,SetY);
			BindSlider(k_Z,SetZ);
			BindInputField(k_X,SetX);
			BindInputField(k_Y,SetY);
			BindInputField(k_Z,SetZ);
// Macro.Patch -->
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
			OnValueChanged(m_Value);
		}

		protected virtual void UpdateUI() {
			float f;string s;
			for(int i=0;i<3;++i) {
				f=m_Value[i];s=string.Format(format,f);
				SetSliderWithoutNotify(i,f);
				SetText(i,s);
			}
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
