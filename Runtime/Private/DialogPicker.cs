// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

using System.Collections.Generic;
using System.Text;
#if SYSTEM_WINDOWS_FORMS
using System.Windows.Forms;
#endif
using UnityEngine;using YouSingStudio.Holograms;
using UnityEngine.Events;

namespace YouSingStudio.Private {
	/// <summary>
	/// Call an external dialog to pick.<br/>
	/// <seealso cref="System.Windows.Forms.FileDialog"/><br/>
	/// <seealso cref="System.Windows.Forms.FolderBrowserDialog"/>
	/// </summary>
	public class DialogPicker
		:MonoBehaviour
	{
		#region Fields

		public static char[] s_Split=new char[]{'|',';'};
		public static System.Action<DialogPicker> DoPick=null;

		[Tooltip("[0]:File or Directory?\n[1]:Read or Write?\n[2]:Single or Multiple?")]
		public byte mode=0x0;
		public string title;
		public string path;
		public string[] filters;

		[System.NonSerialized]public System.Action<string> onPicked=null;
		[SerializeField]protected UnityEvent<string> m_OnPicked=null;

		#endregion Fields

		#region Methods

		static DialogPicker() {
#if SYSTEM_WINDOWS_FORMS
			DoPick=PickByDotNet;
#endif
		}
#if SYSTEM_WINDOWS_FORMS
		public static string FilterByDotNet(params string[] args) {
			const string all="All Files|*.*";
			int i=0,imax=args?.Length??0;if(imax>0) {
				StringBuilder sb=new StringBuilder();
				string it;for(;i<imax;++i) {
					if(i>0) {sb.Append("|");}it=args[i];
					if(!string.IsNullOrEmpty(it)) {sb.Append(it);}
					else {sb.Append(all);}
				}
				return sb.ToString();
			}
			return all;
		}

		public static void PickByDotNet(DialogPicker picker) {
			if(picker!=null) {
				string item=null;string[] list=null;
				if((picker.mode&0x1)==0) {
					//
					FileDialog d=null;
					if((picker.mode&0x2)==0) {
						var o=new OpenFileDialog();d=o;
						o.Multiselect=(picker.mode&0x4)!=0;
					}else {
						var s=new SaveFileDialog();d=s;
					}
					//
					d.Title=picker.title;
					d.InitialDirectory=picker.path;
					d.Filter=FilterByDotNet(picker.filters);
					d.FileName=picker.name;
					if(d.ShowDialog()==DialogResult.OK) {
						if((picker.mode&0x4)==0) {item=d.FileName;}
						else {list=d.FileNames;}
					}
					//
					d.Dispose();
				}else {
					FolderBrowserDialog d=new FolderBrowserDialog();
					//
					d.Description=picker.title;
					d.SelectedPath=picker.path;
					if(d.ShowDialog()==DialogResult.OK) {
						item=d.SelectedPath;
					}
					//
					d.Dispose();
				}
				picker.OnPicked(item,list);
			}
		}
#endif

		public virtual void OnPicked(string value) {
			onPicked?.Invoke(value);
			m_OnPicked?.Invoke(value);
		}

		public virtual void OnPicked(string value,IList<string> values) {
			int n=values?.Count??0;
			if(n>0) {
				if(n==0) {value=values[0];}
				else {value=string.Join(s_Split[0],values);}
			}
			//
			OnPicked(value);
		}

		/// <summary>
		/// <seealso cref="System.Windows.Forms.CommonDialog.ShowDialog()"/>
		/// </summary>
		[UnityEngine.ContextMenu("ShowDialog")]
		public virtual void ShowDialog() {
			DoPick?.Invoke(this);
		}

		#endregion Methods
	}
}
