/* <!-- Macro.Include
../Private/ScriptableView.cs
 Macro.End --> */
/* <!-- Macro.Table Slider
Scale,
Offset,
 Macro.End --> */
/* <!-- Macro.Table InputField
Scale,
Offset,
 Macro.End --> */
/* <!-- Macro.BatchCall DeclareKeys Slider
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */

/* <!-- Macro.Call BindSet Slider
 Macro.End --> */

/* <!-- Macro.Call BindSet InputField
 Macro.End --> */
/* <!-- Macro.Patch
,Start
 Macro.End --> */
using UnityEngine;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	public class UIDepthEditor
		:ScriptableView
	{
// <!-- Macro.Patch AutoGen
		public const int k_Scale=0;
		public const int k_Offset=1;
#if UNITY_EDITOR
		protected virtual string[] GetSliders()=>new string[]{
			"Scale",
			"Offset",
		};
#endif

// Macro.Patch -->
		#region Fields

		public new DepthRenderer renderer;
		public string format="0.00";
		public Vector3 scale=Vector3.up;
		public Vector3 offset=Vector3.up;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			this.LoadSettings(name);
// <!-- Macro.Patch Start
			BindSlider(k_Scale,SetScale);
			BindSlider(k_Offset,SetOffset);
			BindInputField(k_Scale,SetScale);
			BindInputField(k_Offset,SetOffset);
// Macro.Patch -->
			SetSlider(k_Scale,scale);
			SetSlider(k_Offset,offset);
		}

		#endregion Unity Messages

		#region Methods

		protected virtual void SetScale(string value) {
			if(float.TryParse(value,out var f)) {SetScale(f);}
		}

		protected virtual void SetOffset(string value) {
			if(float.TryParse(value,out var f)) {SetOffset(f);}
		}

		protected virtual void SetScale(float value) {
			if(renderer!=null) {
				renderer.factor.x=value;
				UpdateDepth();
			}
		}

		protected virtual void SetOffset(float value) {
			if(renderer!=null) {
				renderer.factor.y=value;
				UpdateDepth();
			}
		}

		protected virtual void UpdateDepth() {
			if(renderer!=null) {
				SetText(k_Scale,renderer.factor.ToString(format));
				SetText(k_Offset,renderer.factor.ToString(format));
				renderer.UpdateVector();
			}
		}

		#endregion Methods
	}
}
