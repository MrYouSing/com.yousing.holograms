using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	public class UIDeviceController
		:UIBaseController
	{
		#region Fields

		public HologramDevice device;
		public DialogPicker[] pickers=new DialogPicker[2];

		[System.NonSerialized]protected bool m_Awaking;
		[System.NonSerialized]protected UIDisplaySelector m_Display;
		[System.NonSerialized]protected UISliderView[] m_Sliders;
		[System.NonSerialized]protected Vector4 m_Arguments=Vector4.one*float.NaN;

		#endregion Fields

		#region Unity Messages
		#endregion Unity Messages

		#region Methods

		protected override void InitView() {
			if(view==null) {this.CheckInstance(m_View,ref view);}
			if(device==null) {device=FindAnyObjectByType<LenticularDevice>();}
			//
			if(view!=null) {
				m_Actor=view.gameObject;
				m_Display=view.m_GameObjects[0].GetComponent<UIDisplaySelector>();
				int i=0,imax=3;m_Sliders=new UISliderView[imax];
				for(;i<imax;++i) {m_Sliders[i]=view.m_GameObjects[1+i].GetComponent<UISliderView>();}
				//
				SetEvents(true);
			}
		}

		protected override void SetEvents(bool value) {
			base.SetEvents(value);
			int i=1;
			view.BindButton(i,Copy,value);++i;
			view.BindButton(i,Paste,value);++i;
			view.BindButton(i,Open,value);++i;
			view.BindButton(i,Save,value);++i;
			view.BindButton(i,Delete,value);++i;
			//
			if(m_Display!=null) {
				if(value) {m_Display.onValueChanged+=OnDisplay;}
				else {m_Display.onValueChanged-=OnDisplay;}
			}
			int imax=m_Sliders?.Length??0;Button btn;
			UISliderView s;for(i=0;i<imax;++i) {
				s=m_Sliders[i];if(s==null) {continue;}
				if(value) {s.onSliderChanged+=OnSlider;}
				else {s.onSliderChanged-=OnSlider;}
				//
				btn=s.field!=null?s.field.GetComponentInChildren<Button>():null;
				if(btn!=null) {
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

		public override void SetView(bool value) {
			HologramDevice tmp=GetDevice();
			if(tmp==null||!tmp.enabled) {value=false;}
			//
			var b=m_Awaking;m_Awaking=value;
				base.SetView(value);
			m_Awaking=b;
		}

		protected virtual string GetText() {
			HologramDevice tmp=GetDevice();
			if(tmp!=null) {return tmp.ToJson();}
			return null;
		}

		protected virtual void SetText(string text) {
			HologramDevice tmp=GetDevice();
			if(tmp==null||string.IsNullOrEmpty(text)) {return;}
			//
			tmp.FromJson(text);
			Render();
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

		public override void Render() {
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
				if(float.IsNaN(m_Arguments[i])) {m_Arguments[i]=f;}
				s.SetValueWithoutNotify(f);
			}
		}

		public override void Apply() {
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
			string tmp=GUIUtility.systemCopyBuffer;
			if(!string.IsNullOrEmpty(tmp)) {
				m_Arguments=Vector4.one*float.NaN;
				SetText(tmp);
			}
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

		public virtual void Delete() {
			HologramDevice tmp=GetDevice();
			if(tmp!=null) {
				tmp.FromJson(null);
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
						m_Arguments=Vector4.one*float.NaN;
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
