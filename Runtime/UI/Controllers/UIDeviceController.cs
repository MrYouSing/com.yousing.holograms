using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine;
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

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			if(view==null) {view=GetComponent<ScriptableView>();}
			if(view==null) {view=UnityExtension.GetResourceInstance<ScriptableView>(m_View);}
			if(view!=null) {SetEvents(true);}
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
			/*/
			i=0;
			view.BindDropdown(0,SetDisplay,value);
			++i;
			view.BindSlider(i,SetArgumentX,value);
			view.BindInputField(i,SetArgumentX,value);
			view.BindSubmit(i,SetArgumentX,value);
			++i;
			view.BindSlider(i,SetArgumentY,value);
			view.BindInputField(i,SetArgumentY,value);
			view.BindSubmit(i,SetArgumentY,value);
			++i;
			view.BindSlider(i,SetArgumentZ,value);
			view.BindInputField(i,SetArgumentZ,value);
			view.BindSubmit(i,SetArgumentZ,value);
		*/}

		protected virtual HologramDevice GetDevice() {
			HologramDevice tmp=device;
			if(MonoApplication.s_Instance!=null) {
				tmp=MonoApplication.s_Instance.device;
			}
			return tmp;
		}

		public virtual void SetView(bool value) {
			HologramDevice tmp=device;
			if(tmp==null||!tmp.enabled) {value=false;}
			if(view!=null) {view.gameObject.SetActive(value);if(value) {Render();}}
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
		}

		public virtual void Render() {
			if(view==null) {return;}
			string tmp=GetText();
			if(string.IsNullOrEmpty(tmp)) {return;}
			//
			JObject jo=JObject.Parse(tmp);int i;float f;
			i=jo[view.m_Strings[0]]?.Value<int>()??-1;
			view.SetDropdownWithoutNotify(0,i);
			for(i=0;i<3;++i) {
				f=jo[view.m_Strings[1+i]].Value<float>();
				view.SetSliderWithoutNotify(i,f);
				view.SetInputFieldWithoutNotify(i,f.ToString());
			}
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
			DialogPicker dlg=pickers[1];
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

		protected virtual void OnPicked(int type,string path) {
			if(string.IsNullOrEmpty(path)) {return;}
			//
			switch(type) {
				case 0:if(File.Exists(path)) SetText(File.ReadAllText(path));break;
				case 1:File.WriteAllText(path,GetText());break;
			}
		}

		#endregion Methods
	}
}
