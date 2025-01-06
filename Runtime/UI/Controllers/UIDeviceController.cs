using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	public class UIDeviceController
		:MonoBehaviour
	{
		#region Fields

		public HologramDevice device;
		[SerializeField]protected string m_View;
		public ScriptableView view;
		public DialogPicker[] pickers=new DialogPicker[2];

		[System.NonSerialized]protected bool m_Awaking;
		[System.NonSerialized]protected UIDisplaySelector m_Display;
		[System.NonSerialized]protected UISliderView[] m_Sliders;
		[System.NonSerialized]protected Vector4 m_Arguments=Vector4.one*float.NaN;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(device==null) {device=FindAnyObjectByType<LenticularDevice>();}
			if(view==null) {view=GetComponent<ScriptableView>();}
			if(view==null) {view=UnityExtension.GetResourceInstance<ScriptableView>(m_View);}
			//
			if(view!=null) {
				m_Display=view.m_GameObjects[0].GetComponent<UIDisplaySelector>();
				int i=0,imax=3;m_Sliders=new UISliderView[imax];
				for(;i<imax;++i) {m_Sliders[i]=view.m_GameObjects[1+i].GetComponent<UISliderView>();}
				//
				SetEvents(true);
			}
		}

		protected virtual void OnDestroy() {
			if(view!=null) {SetEvents(false);}
		}

		#endregion Unity Messages

		#region Methods

		protected virtual void SetEvents(bool value) {
			int i=0;
			view.BindButton(i,Copy,value);++i;
			view.BindButton(i,Paste,value);++i;
			view.BindButton(i,Open,value);++i;
			view.BindButton(i,Save,value);++i;
			// Hack Buttons
			int len=view.m_Buttons?.Length??0;Button btn;
			if(i<len) {// Close
				btn=view.m_Buttons[i];if(btn!=null) {
					if(value) {btn.SetOnClick(()=>SetView(false));}
					else {btn.SetOnClick(null);}
				}
			}
			//
			if(m_Display!=null) {
				if(value) {m_Display.onValueChanged+=OnDisplay;}
				else {m_Display.onValueChanged-=OnDisplay;}
			}
			int imax=m_Sliders?.Length??0;
			UISliderView s;for(i=0;i<imax;++i) {
				s=m_Sliders[i];if(s==null) {continue;}
				if(value) {s.onSliderChanged+=OnSlider;}
				else {s.onSliderChanged-=OnSlider;}
				//
				btn=s.field.GetComponentInChildren<Button>();if(btn!=null) {
					int ii=i;
					if(value) {btn.SetOnClick(()=>ResetArgument(ii));}
					else {btn.SetOnClick(null);}
				}
			}
		}

		protected virtual HologramDevice GetDevice() {
			HologramDevice tmp=device;
			if(MonoApplication.s_Instance!=null&&MonoApplication.s_Instance.device!=null) {
				tmp=MonoApplication.s_Instance.device;
			}
			return tmp;
		}

		public virtual void SetView(bool value) {
			HologramDevice tmp=GetDevice();
			if(tmp==null||!tmp.enabled) {value=false;}
			//
			var b=m_Awaking;m_Awaking=value;
				if(view!=null) {view.gameObject.SetActive(value);}
				if(value) {
					// TODO: Reset sliders????
					Render();
				}
			m_Awaking=b;
			//
			this.LockShortcuts(value);
		}

		protected virtual string GetText() {
			HologramDevice tmp=GetDevice();
			if(tmp!=null) {return tmp.ToJson();}
			return null;
		}

		protected virtual void SetText(string text) {
			HologramDevice tmp=GetDevice();
			if(tmp!=null) {tmp.FromJson(text);}
			Render();
			//
			if(float.IsNaN(m_Arguments.x)) {
				int i,imax=m_Sliders?.Length??0;
					JObject jo=JObject.Parse(text);
				//
				UISliderView s;for(i=0;i<imax;++i) {
					s=m_Sliders[i];
					m_Arguments[i]=jo[view.m_Strings[2+i]]
						?.Value<float>()??s.GetValue(0.0f);
				}
			}
		}

		protected virtual void ResetArgument(int index) {
			if(float.IsNaN(m_Arguments[index])) {return;}
			string tmp=GetText();
			if(view==null||string.IsNullOrEmpty(tmp)) {return;}
			//
			JObject jo=JObject.Parse(tmp);
			jo[view.m_Strings[2+index]]=m_Arguments[index];
			//
			SetText(jo.ToString());
		}

		public virtual void Render() {
			string tmp=GetText();
			if(view==null||string.IsNullOrEmpty(tmp)) {return;}
			//
			int i,imax=m_Sliders?.Length??0;
			JObject jo=JObject.Parse(tmp);float f;
			view.SetText(0,jo[view.m_Strings[0]]?.Value<string>()??GetDevice().name);
			i=jo[view.m_Strings[1]]?.Value<int>()??-1;
			if(m_Display!=null) {m_Display.SetValueWithoutNotify(i);}
			//
			UISliderView s;for(i=0;i<imax;++i) {
				s=m_Sliders[i];if(s==null) {continue;}
				f=jo[view.m_Strings[2+i]].Value<float>();
				s.SetValueWithoutNotify(f);
			}
		}

		public virtual void Apply() {
			if(m_Awaking) {return;}
			string tmp=GetText();
			if(view==null||string.IsNullOrEmpty(tmp)) {return;}
			//
			int i,imax=m_Sliders?.Length??0;
			JObject jo=JObject.Parse(tmp);
			if(m_Display!=null) {jo[view.m_Strings[1]]=m_Display.value;}
			//
			UISliderView s;for(i=0;i<imax;++i) {
				s=m_Sliders[i];if(s==null) {continue;}
				jo[view.m_Strings[2+i]]=s.GetValue(0.0f);
			}
			//
			SetText(jo.ToString());
		}

		public virtual void Copy() {
			string tmp=GetText();
			if(!string.IsNullOrEmpty(tmp)) {
				GUIUtility.systemCopyBuffer=tmp;
			}
		}

		public virtual void Paste() {
			SetText(GUIUtility.systemCopyBuffer);
		}

		public virtual void Open() {
			HologramDevice tmp=GetDevice();
			DialogPicker dlg=pickers[0];
			if(tmp!=null&&dlg!=null) {
				dlg.onPicked=(x)=>OnPicked(0,x);
				dlg.ShowDialog();
			}
		}

		public virtual void Save() {
			HologramDevice tmp=GetDevice();
			DialogPicker dlg=pickers[1];
			if(tmp!=null&&dlg!=null) {
				dlg.onPicked=(x)=>OnPicked(1,x);
				dlg.ShowDialog();
			}
		}

		protected virtual void OnDisplay(int index) {
			Apply();
		}

		protected virtual void OnSlider() {
			Apply();
		}

		protected virtual void OnPicked(int type,string path) {
			if(string.IsNullOrEmpty(path)) {return;}
			//
			switch(type) {
				case 0:
					if(File.Exists(path)) {
						m_Arguments.x=float.NaN;
						SetText(File.ReadAllText(path));
					}
				break;
				case 1:
					File.WriteAllText(path,GetText());
				break;
			}
		}

		#endregion Methods
	}
}
