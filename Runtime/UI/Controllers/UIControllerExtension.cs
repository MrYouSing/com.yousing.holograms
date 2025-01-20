using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	public static partial class UIControllerExtension
	{
		#region Fields
		#endregion Fields

		#region Methods

		public static UISliderView[] ToViews(this IList<Slider> thiz,int start=0) {
			int imax=(thiz?.Count??0)-start;UISliderView[] tmp=null;
			if(imax>0) {
				tmp=new UISliderView[imax];
				Slider s;for(int i=0;i<imax;++i) {
					s=thiz[start+i];if(s==null) {continue;}
					tmp[i]=s.transform.parent.GetComponent<UISliderView>();
				}
			}
			return tmp;
		}

		public static void SetEvent(this IList<UISliderView> thiz,System.Action action,bool value) {
			UISliderView s;for(int i=0,imax=thiz?.Count??0;i<imax;++i) {
				s=thiz[i];if(s==null) {continue;}
				if(value) {s.onSliderChanged+=action;}
				else {s.onSliderChanged-=action;}
			}
		}

		public static void BindFieldButton(this ScriptableView thiz,int index,UnityAction action,bool value) {
			if(thiz!=null&&index>=0&&index<(thiz.m_InputFields?.Length??0)) {
				Component com=thiz.m_InputFields[index];
				Button btn=com!=null?com.GetComponentInChildren<Button>():null;
				if(btn!=null) {
					if(value) {btn.SetOnClick(action);}
					else {btn.SetOnClick(null);}
				}
			}
		}

		public static void SetToggleBetter(this ScriptableView thiz,int index,bool value) {
			if(thiz!=null) {
				if(index<0||index>=(thiz.m_Toggles?.Length??0)) {return;}
				//
				Toggle toggle=thiz.m_Toggles[index];
				if(toggle==null) {return;}
				toggle.SetIsOnWithoutNotify(value);
				//
				var trigger=toggle.GetComponent<ActiveTrigger>();
				if(trigger==null) {return;}
				trigger.InvokeEvent(value);
			}
		}

		#endregion Methods
	}
}