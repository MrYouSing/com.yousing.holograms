using System.Collections.Generic;
using UnityEngine.UI;

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

		#endregion Methods
	}
}