using System.Collections.Generic;
using System.IO;
using YouSingStudio.Private;

namespace YouSingStudio.Samples {
	public class DialogPickerImpl
	{
		#region Fields
		#endregion Fields

		#region Methods

#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
#else
		[UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
		public static void InitializeOnLoadMethod() {
#if UNITY_STANDALONE_FILE_BROWSER
			DialogPicker.DoPick=PickBySFB;
#elif UNITY_SIMPLE_FILE_BROWSER
			DialogPicker.OnPick=null;
#endif
		}
#if UNITY_STANDALONE_FILE_BROWSER
		public static SFB.ExtensionFilter s_SFB_All=new SFB.ExtensionFilter("All Files","*");
		public static Dictionary<string,SFB.ExtensionFilter> s_SFB_Map=new Dictionary<string,SFB.ExtensionFilter>();

		public static SFB.ExtensionFilter FilterBySFB(string filter) {
			if(!string.IsNullOrEmpty(filter)) {
				if(!s_SFB_Map.TryGetValue(filter,out var tmp)) {
					int i=filter.IndexOf(DialogPicker.s_Split[0]);
					tmp=new SFB.ExtensionFilter(filter.Substring(0,i),filter.Substring(i+1).Split(DialogPicker.s_Split));
					int imax=tmp.Extensions?.Length??0;for(i=0;i<imax;++i) {tmp.Extensions[i]=tmp.Extensions[i].Substring(1);}
					s_SFB_Map[filter]=tmp;
				}
				return tmp;
			}
			return s_SFB_All;
		}

		public static void PickBySFB(DialogPicker picker) {
			if(picker!=null) {
				string tmp=null;string[] list=null;
				if((picker.mode&0x1)==0) {
					//
					int i=0,imax=picker.filters?.Length??0;
					SFB.ExtensionFilter[] filters=null;
					if(imax>0) {filters=new SFB.ExtensionFilter[imax];
					for(;i<imax;++i) {
						filters[i]=FilterBySFB(picker.filters[i]);
					}}
					//
					if((picker.mode&0x2)==0) {
						list=SFB.StandaloneFileBrowser.OpenFilePanel(picker.title,picker.path,filters,(picker.mode&0x04)!=0);
					}else {
						tmp=SFB.StandaloneFileBrowser.SaveFilePanel(picker.title,picker.path,picker.name,filters);
					}
				}else {
					list=SFB.StandaloneFileBrowser.OpenFolderPanel(picker.title,picker.path,(picker.mode&0x04)!=0);
				}
				picker.OnPicked(tmp,list);
			}
		}
#endif
		#endregion Methods
	}
}
