using UnityEngine;

namespace YouSingStudio.Holograms {
	public class UIDisplaySelector
		:UIEnumView<int>
	{
		#region Fields
		#endregion Fields

		#region Unity Messages

		protected override void Awake() {
			SetupDisplays();
			base.Awake();
		}

		#endregion Unity Messages

		#region Methods

		protected virtual void SetupDisplays() {
			var tmp=Display.displays;int i=0,imax=tmp.Length+1;
			m_Strings=new string[imax];values=new int[imax];
			for(;i<imax;++i) {
				m_Strings[i]=ToString(i,i>0?tmp[i-1]:null);values[i]=i-1;
			}
			//
			System.Array.Resize(ref keys,imax);
			if((m_Toggles?.Length??0)>0) {System.Array.Resize(ref m_Toggles,imax);}
		}

		protected virtual string ToString(int index,Display display) {
			if(display!=null) {return $"Display#{index}({display.systemWidth}x{display.systemHeight})";}
			else {return "Display#0(Auto)";}
		}

		#endregion Methods
	}
}
