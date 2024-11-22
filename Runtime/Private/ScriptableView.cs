// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

/* <!-- Macro.Define DeclareField
		[SerializeField,ArrayElement(names="Get{0}s")]public {0}[] m_{0}s;
 Macro.End --> */
/* <!-- Macro.Define DeclareKeys_0
		public const int k_{0}=$(Table.Row);
 Macro.End --> */
/* <!-- Macro.Define DeclareKeys_1
#if UNITY_EDITOR
		protected virtual string[] Get$(Table.Name)s()=>new string[]{{
 Macro.End --> */
/* <!-- Macro.Define DeclareKeys_2
			"{0}",
 Macro.End --> */
/* <!-- Macro.Define DeclareKeys_3
		}};
#endif

 Macro.End --> */
/* <!-- Macro.Table DeclareKeys
DeclareKeys_0
DeclareKeys_1,
DeclareKeys_2
DeclareKeys_3,
 Macro.End --> */
/* <!-- Macro.Define Bind=
true
 Macro.End --> */
/* <!-- Macro.Define Bind
			Bind$(Table.Name)(k_{0},{0},$(Bind));
 Macro.End --> */
/* <!-- Macro.Define BindSet
			Bind$(Table.Name)(k_{0},Set{0},$(Bind));
 Macro.End --> */

/* <!-- Macro.Define GetSet

		public virtual {1} Get{0}(int index) {{
			if($(InRange.Begin)m_{0}s$(InRange.End)) {{
				var tmp=m_{0}s[index];if(tmp!=null) {{return tmp.{2};}}
			}}
			return default;//{1}
		}}

		public virtual void Set{0}(int index,{1} value) {{
			if($(InRange.Begin)m_{0}s$(InRange.End)) {{
				var tmp=m_{0}s[index];if(tmp!=null) {{tmp.{2}=value;}}
			}}
		}}

		{3}public virtual void Set{0}WithoutNotify(int index,{1} value) {{
		{3}	if($(InRange.Begin)m_{0}s$(InRange.End)) {{
		{3}		var tmp=m_{0}s[index];if(tmp!=null) {{tmp.Set{4}WithoutNotify(value);}}
		{3}	}}
		{3}}}
 Macro.End --> */
/* <!-- Macro.Define Event

		public virtual void Bind{0}(int index,UnityAction<{1}> action,bool value=true) {{
			if($(InRange.Begin)m_{0}s$(InRange.End)) {{
				var tmp=m_{0}s[index];if(tmp!=null) {{
					if(value) {{tmp.onValueChanged.AddListener(action);}}//{0}
					else {{tmp.onValueChanged.RemoveListener(action);}}//{0}
				}}
			}}
		}}
 Macro.End --> */

/* <!-- Macro.Table Get
string,
Sprite,
Texture,
GameObject,
 Macro.End --> */
/* <!-- Macro.Table GetSet
Text,string,text,//,,
Image,Sprite,sprite,//,,
RawImage,Texture,texture,//,,
 Macro.End --> */
/* <!-- Macro.Table GetSetEvent
Toggle,bool,isOn,,IsOn,
Slider,float,value,,Value,
Dropdown,int,value,,Value,
InputField,string,text,,Text,
 Macro.End --> */

/* <!-- Macro.Call DeclareField Get
 Macro.End --> */
/* <!-- Macro.Call DeclareField GetSet
 Macro.End --> */
/* <!-- Macro.Call DeclareField
Button
 Macro.End --> */
/* <!-- Macro.Call DeclareField GetSetEvent
 Macro.End --> */
/* <!-- Macro.Call  Get

		public virtual {0} Get{0}(int index) {{
			if($(InRange.Begin)m_{0}s$(InRange.End)) return m_{0}s[index];
			return default;//{0}
		}}
 Macro.End --> */
/* <!-- Macro.Call GetSet GetSet
 Macro.End --> */
/* <!-- Macro.Call Event
Button,,
 Macro.End --> */
/* <!-- Macro.Call GetSet GetSetEvent
 Macro.End --> */
/* <!-- Macro.Call Event GetSetEvent
 Macro.End --> */
/* <!-- Macro.Replace
$(InRange.Begin),index>=0&&index<(
$(InRange.End),?.Length??0)
strings,Strings
etstring,etString
<>,
onValueChanged.AddListener(action);}//Button,onClick.AddListener(action);}
onValueChanged.RemoveListener(action);}//Button,onClick.RemoveListener(action);}
 Macro.End --> */

/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
using UnityEngine;using YouSingStudio.Holograms;
using UnityEngine.Events;
using UnityEngine.UI;

namespace YouSingStudio.Private {
	/// <summary>
	/// A scriptable interface for UGUI system.<br/>
	/// <seealso cref="ScriptableObject"/>
	/// </summary>
	public class ScriptableView
		:MonoBehaviour
	{
// <!-- Macro.Patch AutoGen
		[SerializeField,ArrayElement(names="GetStrings")]public string[] m_Strings;
		[SerializeField,ArrayElement(names="GetSprites")]public Sprite[] m_Sprites;
		[SerializeField,ArrayElement(names="GetTextures")]public Texture[] m_Textures;
		[SerializeField,ArrayElement(names="GetGameObjects")]public GameObject[] m_GameObjects;
		[SerializeField,ArrayElement(names="GetTexts")]public Text[] m_Texts;
		[SerializeField,ArrayElement(names="GetImages")]public Image[] m_Images;
		[SerializeField,ArrayElement(names="GetRawImages")]public RawImage[] m_RawImages;
		[SerializeField,ArrayElement(names="GetButtons")]public Button[] m_Buttons;
		[SerializeField,ArrayElement(names="GetToggles")]public Toggle[] m_Toggles;
		[SerializeField,ArrayElement(names="GetSliders")]public Slider[] m_Sliders;
		[SerializeField,ArrayElement(names="GetDropdowns")]public Dropdown[] m_Dropdowns;
		[SerializeField,ArrayElement(names="GetInputFields")]public InputField[] m_InputFields;

		public virtual string GetString(int index) {
			if(index>=0&&index<(m_Strings?.Length??0)) return m_Strings[index];
			return default;//string
		}

		public virtual Sprite GetSprite(int index) {
			if(index>=0&&index<(m_Sprites?.Length??0)) return m_Sprites[index];
			return default;//Sprite
		}

		public virtual Texture GetTexture(int index) {
			if(index>=0&&index<(m_Textures?.Length??0)) return m_Textures[index];
			return default;//Texture
		}

		public virtual GameObject GetGameObject(int index) {
			if(index>=0&&index<(m_GameObjects?.Length??0)) return m_GameObjects[index];
			return default;//GameObject
		}

		public virtual string GetText(int index) {
			if(index>=0&&index<(m_Texts?.Length??0)) {
				var tmp=m_Texts[index];if(tmp!=null) {return tmp.text;}
			}
			return default;//string
		}

		public virtual void SetText(int index,string value) {
			if(index>=0&&index<(m_Texts?.Length??0)) {
				var tmp=m_Texts[index];if(tmp!=null) {tmp.text=value;}
			}
		}

		//public virtual void SetTextWithoutNotify(int index,string value) {
		//	if(index>=0&&index<(m_Texts?.Length??0)) {
		//		var tmp=m_Texts[index];if(tmp!=null) {tmp.SetWithoutNotify(value);}
		//	}
		//}

		public virtual Sprite GetImage(int index) {
			if(index>=0&&index<(m_Images?.Length??0)) {
				var tmp=m_Images[index];if(tmp!=null) {return tmp.sprite;}
			}
			return default;//Sprite
		}

		public virtual void SetImage(int index,Sprite value) {
			if(index>=0&&index<(m_Images?.Length??0)) {
				var tmp=m_Images[index];if(tmp!=null) {tmp.sprite=value;}
			}
		}

		//public virtual void SetImageWithoutNotify(int index,Sprite value) {
		//	if(index>=0&&index<(m_Images?.Length??0)) {
		//		var tmp=m_Images[index];if(tmp!=null) {tmp.SetWithoutNotify(value);}
		//	}
		//}

		public virtual Texture GetRawImage(int index) {
			if(index>=0&&index<(m_RawImages?.Length??0)) {
				var tmp=m_RawImages[index];if(tmp!=null) {return tmp.texture;}
			}
			return default;//Texture
		}

		public virtual void SetRawImage(int index,Texture value) {
			if(index>=0&&index<(m_RawImages?.Length??0)) {
				var tmp=m_RawImages[index];if(tmp!=null) {tmp.texture=value;}
			}
		}

		//public virtual void SetRawImageWithoutNotify(int index,Texture value) {
		//	if(index>=0&&index<(m_RawImages?.Length??0)) {
		//		var tmp=m_RawImages[index];if(tmp!=null) {tmp.SetWithoutNotify(value);}
		//	}
		//}

		public virtual void BindButton(int index,UnityAction action,bool value=true) {
			if(index>=0&&index<(m_Buttons?.Length??0)) {
				var tmp=m_Buttons[index];if(tmp!=null) {
					if(value) {tmp.onClick.AddListener(action);}
					else {tmp.onClick.RemoveListener(action);}
				}
			}
		}

		public virtual bool GetToggle(int index) {
			if(index>=0&&index<(m_Toggles?.Length??0)) {
				var tmp=m_Toggles[index];if(tmp!=null) {return tmp.isOn;}
			}
			return default;//bool
		}

		public virtual void SetToggle(int index,bool value) {
			if(index>=0&&index<(m_Toggles?.Length??0)) {
				var tmp=m_Toggles[index];if(tmp!=null) {tmp.isOn=value;}
			}
		}

		public virtual void SetToggleWithoutNotify(int index,bool value) {
			if(index>=0&&index<(m_Toggles?.Length??0)) {
				var tmp=m_Toggles[index];if(tmp!=null) {tmp.SetIsOnWithoutNotify(value);}
			}
		}

		public virtual float GetSlider(int index) {
			if(index>=0&&index<(m_Sliders?.Length??0)) {
				var tmp=m_Sliders[index];if(tmp!=null) {return tmp.value;}
			}
			return default;//float
		}

		public virtual void SetSlider(int index,float value) {
			if(index>=0&&index<(m_Sliders?.Length??0)) {
				var tmp=m_Sliders[index];if(tmp!=null) {tmp.value=value;}
			}
		}

		public virtual void SetSliderWithoutNotify(int index,float value) {
			if(index>=0&&index<(m_Sliders?.Length??0)) {
				var tmp=m_Sliders[index];if(tmp!=null) {tmp.SetValueWithoutNotify(value);}
			}
		}

		public virtual int GetDropdown(int index) {
			if(index>=0&&index<(m_Dropdowns?.Length??0)) {
				var tmp=m_Dropdowns[index];if(tmp!=null) {return tmp.value;}
			}
			return default;//int
		}

		public virtual void SetDropdown(int index,int value) {
			if(index>=0&&index<(m_Dropdowns?.Length??0)) {
				var tmp=m_Dropdowns[index];if(tmp!=null) {tmp.value=value;}
			}
		}

		public virtual void SetDropdownWithoutNotify(int index,int value) {
			if(index>=0&&index<(m_Dropdowns?.Length??0)) {
				var tmp=m_Dropdowns[index];if(tmp!=null) {tmp.SetValueWithoutNotify(value);}
			}
		}

		public virtual string GetInputField(int index) {
			if(index>=0&&index<(m_InputFields?.Length??0)) {
				var tmp=m_InputFields[index];if(tmp!=null) {return tmp.text;}
			}
			return default;//string
		}

		public virtual void SetInputField(int index,string value) {
			if(index>=0&&index<(m_InputFields?.Length??0)) {
				var tmp=m_InputFields[index];if(tmp!=null) {tmp.text=value;}
			}
		}

		public virtual void SetInputFieldWithoutNotify(int index,string value) {
			if(index>=0&&index<(m_InputFields?.Length??0)) {
				var tmp=m_InputFields[index];if(tmp!=null) {tmp.SetTextWithoutNotify(value);}
			}
		}

		public virtual void BindToggle(int index,UnityAction<bool> action,bool value=true) {
			if(index>=0&&index<(m_Toggles?.Length??0)) {
				var tmp=m_Toggles[index];if(tmp!=null) {
					if(value) {tmp.onValueChanged.AddListener(action);}//Toggle
					else {tmp.onValueChanged.RemoveListener(action);}//Toggle
				}
			}
		}

		public virtual void BindSlider(int index,UnityAction<float> action,bool value=true) {
			if(index>=0&&index<(m_Sliders?.Length??0)) {
				var tmp=m_Sliders[index];if(tmp!=null) {
					if(value) {tmp.onValueChanged.AddListener(action);}//Slider
					else {tmp.onValueChanged.RemoveListener(action);}//Slider
				}
			}
		}

		public virtual void BindDropdown(int index,UnityAction<int> action,bool value=true) {
			if(index>=0&&index<(m_Dropdowns?.Length??0)) {
				var tmp=m_Dropdowns[index];if(tmp!=null) {
					if(value) {tmp.onValueChanged.AddListener(action);}//Dropdown
					else {tmp.onValueChanged.RemoveListener(action);}//Dropdown
				}
			}
		}

		public virtual void BindInputField(int index,UnityAction<string> action,bool value=true) {
			if(index>=0&&index<(m_InputFields?.Length??0)) {
				var tmp=m_InputFields[index];if(tmp!=null) {
					if(value) {tmp.onValueChanged.AddListener(action);}//InputField
					else {tmp.onValueChanged.RemoveListener(action);}//InputField
				}
			}
		}
// Macro.Patch -->
		#region Methods

		public virtual bool GetActive(int index,bool value=false) {
			if(index>=0&&index<(m_GameObjects?.Length??0)) {
				var tmp=m_GameObjects[index];if(tmp!=null) {return tmp.activeSelf;}
			}
			return value;
		}

		public virtual void SetActive(int index,bool value) {
			if(index>=0&&index<(m_GameObjects?.Length??0)) {
				var tmp=m_GameObjects[index];if(tmp!=null) {tmp.SetActive(value);}
			}
		}

		public virtual void SetSlider(int index,Vector3 value) {
			if(index>=0&&index<(m_Sliders?.Length??0)) {
				var tmp=m_Sliders[index];if(tmp!=null) {
					if(value.x!=value.y) {tmp.minValue=value.x;tmp.maxValue=value.y;}
					if(value.z!=0.0f) {tmp.wholeNumbers=value.z==1.0f;}
				}
			}
		}

		public virtual void BindSubmit(int index,UnityAction<string> action,bool value=true) {
			if(index>=0&&index<(m_InputFields?.Length??0)) {
				var tmp=m_InputFields[index];if(tmp!=null) {
					if(value) {tmp.onSubmit.AddListener(action);}//InputField
					else {tmp.onSubmit.RemoveListener(action);}//InputField
				}
			}
		}

		#endregion Methods
	}

	public class ScriptableView<T>:ScriptableView {
		[System.NonSerialized]public System.Action<T> onValueChanged=null;
		[SerializeField]protected UnityEvent<T> m_OnValueChanged=null;

		public virtual void OnValueChanged(T value) {
			onValueChanged?.Invoke(value);
			m_OnValueChanged?.Invoke(value);
		}
	}
}
