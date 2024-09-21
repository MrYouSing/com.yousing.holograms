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
/* <!-- Macro.Define Bind
			Bind$(Table.Name)(k_{0},{0});
 Macro.End --> */
/* <!-- Macro.Define BindSet
			Bind$(Table.Name)(k_{0},Set{0});
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

		public virtual void Bind{0}(int index,UnityAction<{1}> action) {{
			if($(InRange.Begin)m_{0}s$(InRange.End)) {{
				var tmp=m_{0}s[index];if(tmp!=null) {{tmp.onValueChanged.AddListener(action);}}//{0}
			}}
		}}
 Macro.End --> */

/* <!-- Macro.Table Get
string,
Sprite,
Texture,
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
			else return default;//{0}
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
 Macro.End --> */

/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
using UnityEngine;using YouSingStudio.Holograms;
using UnityEngine.Events;
using UnityEngine.UI;

namespace YouSingStudio.Private {
	/// <summary>
	/// A scriptable interface For UGUI system.<br/>
	/// <seealso cref="ScriptableObject"/>
	/// </summary>
	public class ScriptableView
		:MonoBehaviour
	{
// <!-- Macro.Patch AutoGen
		[SerializeField,ArrayElement(names="GetStrings")]public string[] m_Strings;
		[SerializeField,ArrayElement(names="GetSprites")]public Sprite[] m_Sprites;
		[SerializeField,ArrayElement(names="GetTextures")]public Texture[] m_Textures;
		[SerializeField,ArrayElement(names="GetTexts")]public Text[] m_Texts;
		[SerializeField,ArrayElement(names="GetImages")]public Image[] m_Images;
		[SerializeField,ArrayElement(names="GetRawImages")]public RawImage[] m_RawImages;
		[SerializeField,ArrayElement(names="GetButtons")]public Button[] m_Buttons;
		[SerializeField,ArrayElement(names="GetToggles")]public Toggle[] m_Toggles;
		[SerializeField,ArrayElement(names="GetSliders")]public Slider[] m_Sliders;
		[SerializeField,ArrayElement(names="GetDropdowns")]public Dropdown[] m_Dropdowns;

		public virtual string GetString(int index) {
			if(index>=0&&index<(m_Strings?.Length??0)) return m_Strings[index];
			else return default;//string
		}

		public virtual Sprite GetSprite(int index) {
			if(index>=0&&index<(m_Sprites?.Length??0)) return m_Sprites[index];
			else return default;//Sprite
		}

		public virtual Texture GetTexture(int index) {
			if(index>=0&&index<(m_Textures?.Length??0)) return m_Textures[index];
			else return default;//Texture
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

		public virtual void BindButton(int index,UnityAction action) {
			if(index>=0&&index<(m_Buttons?.Length??0)) {
				var tmp=m_Buttons[index];if(tmp!=null) {tmp.onClick.AddListener(action);}
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

		public virtual void BindToggle(int index,UnityAction<bool> action) {
			if(index>=0&&index<(m_Toggles?.Length??0)) {
				var tmp=m_Toggles[index];if(tmp!=null) {tmp.onValueChanged.AddListener(action);}//Toggle
			}
		}

		public virtual void BindSlider(int index,UnityAction<float> action) {
			if(index>=0&&index<(m_Sliders?.Length??0)) {
				var tmp=m_Sliders[index];if(tmp!=null) {tmp.onValueChanged.AddListener(action);}//Slider
			}
		}

		public virtual void BindDropdown(int index,UnityAction<int> action) {
			if(index>=0&&index<(m_Dropdowns?.Length??0)) {
				var tmp=m_Dropdowns[index];if(tmp!=null) {tmp.onValueChanged.AddListener(action);}//Dropdown
			}
		}
// Macro.Patch -->
	}

	public class ScriptableView<T>:ScriptableView {
		public System.Action<T> onValueChanged=null;
		[System.NonSerialized]protected UnityEvent<T> m_OnValueChanged=null;

		public virtual void OnValueChanged(T value) {
			onValueChanged?.Invoke(value);
			m_OnValueChanged?.Invoke(value);
		}
	}
}
