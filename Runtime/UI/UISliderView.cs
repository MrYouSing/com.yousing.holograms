/* <!-- Macro.Define bFixPatch=
true
 Macro.End --> */
/* <!-- Macro.Define SetText
		protected virtual void SetText(int index,{0} value) {{
			Text tmp=texts[index];if(tmp==null) {{return;}}
			{1}
		}}

 Macro.End --> */

/* <!-- Macro.Call SetText
string,bool b=string.IsNullOrEmpty(value);tmp.enabled=b;if(b) {tmp.text=value;},
float,tmp.text=value.ToString(format);,
int,tmp.text=value.ToString();,
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
using UnityEngine;
using UnityEngine.UI;

namespace YouSingStudio.Holograms {
	public class UISliderView
		:MonoBehaviour
	{
// <!-- Macro.Patch AutoGen
		protected virtual void SetText(int index,string value) {
			Text tmp=texts[index];if(tmp==null) {return;}
			bool b=string.IsNullOrEmpty(value);tmp.enabled=b;if(b) {tmp.text=value;}
		}

		protected virtual void SetText(int index,float value) {
			Text tmp=texts[index];if(tmp==null) {return;}
			tmp.text=string.Format(format,value);
		}

		protected virtual void SetText(int index,int value) {
			Text tmp=texts[index];if(tmp==null) {return;}
			tmp.text=value.ToString();
		}

// Macro.Patch -->
		#region Fields

		[Header("Slider")]
		public Slider slider;
		public InputField field;
		public string format="0.00";
		public Text[] texts=new Text[3];

		[System.NonSerialized]protected GameObject m_SelfV;
		[System.NonSerialized]protected GameObject m_SliderV;
		[System.NonSerialized]protected GameObject m_FieldV;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(slider==null) {slider=GetComponentInChildren<Slider>();}
			if(field==null) {field=GetComponentInChildren<InputField>();}
			//
			m_SelfV=gameObject;
			if(slider!=null) {
				slider.onValueChanged.AddListener(OnSliderChanged);
				m_SliderV=slider.gameObject;
			}
			if(field!=null) {
				field.onSubmit.AddListener(OnSliderChanged);
				m_FieldV=field.gameObject;
			}
		}

		#endregion Unity Messages

		#region Methods

		protected virtual void UpdateSlider(float value) {
			string s=value.ToString(format);
			if(slider!=null) {
				SetText(0,slider.minValue);
				SetText(1,s);
				SetText(2,slider.maxValue);
				slider.SetValueWithoutNotify(value);
			}
			if(field!=null) {field.SetTextWithoutNotify(s);}
			//
			if(m_SliderV!=null) {m_SliderV.SetActive(true);}
			if(m_FieldV!=null) {m_FieldV.SetActive(true);}
		}

		protected virtual void HideSlider() {
			SetText(0,null);SetText(1,null);SetText(2,null);
			if(m_SliderV!=null) {m_SliderV.SetActive(false);}
			if(m_FieldV!=null) {m_FieldV.SetActive(false);}
		}

		protected virtual void OnSliderChanged(float value) {
			UpdateSlider(value);
		}

		protected virtual void OnSliderChanged(string value) {
			if(float.TryParse(value,out float f)){UpdateSlider(f);}
		}

		#endregion Methods
	}
}
