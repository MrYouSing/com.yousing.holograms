using UnityEngine;

namespace YouSingStudio.Holograms {
	public class LookingGlassDevice
		:LenticularDevice
	{
		#region Fields
		#endregion Fields

		#region Methods

		public override bool IsPresent() {
			var sdk=LookingGlassSdk.s_Instance;
			if(sdk!=null&&!sdk.IsDetected) {return false;}
			return FindDisplay(resolution)>=0;
		}

		#endregion Methods
	}
}
