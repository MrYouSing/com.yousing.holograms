/* <!-- Macro.Define bFixPatch=
 true
 Macro.End --> */
/* <!-- Macro.Table Slider
s,S,cale,x,
o,O,ffset,y,
 Macro.End --> */

/* <!-- Macro.Call  Slider
		public UISliderView {0}{2};
		public virtual void Reset{1}{2}() {{{0}{2}.SetValueWithoutNotify(depth.{3});UpdateDepth();}}
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */


/* <!-- Macro.Call  Slider
			if({0}{2}!=null) {{{0}{2}.onSliderChanged+=UpdateDepth;}}
 Macro.End --> */
/* <!-- Macro.Patch
,Start
 Macro.End --> */


/* <!-- Macro.Call  Slider
			if({0}{2}!=null) {{{0}{2}.onSliderChanged-=UpdateDepth;}}
 Macro.End --> */
/* <!-- Macro.Patch
,OnDestroy
 Macro.End --> */


/* <!-- Macro.Call  Slider
						{0}{2}.SetRange(Vector2.up);{0}{2}.SetValueWithoutNotify(renderer.factor.{3});
 Macro.End --> */
/* <!-- Macro.Patch
,OnTypeChanged
 Macro.End --> */
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class UIDepthEditor
		:MonoBehaviour
	{
// <!-- Macro.Patch AutoGen
		public UISliderView scale;
		public virtual void ResetScale() {scale.SetValueWithoutNotify(depth.x);UpdateDepth();}
		public UISliderView offset;
		public virtual void ResetOffset() {offset.SetValueWithoutNotify(depth.y);UpdateDepth();}
// Macro.Patch -->
		#region Fields

		public new DepthRenderer renderer;
		public Vector2 depth=new Vector2(1.0f,0.0f);

		[System.NonSerialized]protected GameObject m_SelfV;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			this.LoadSettings(name);
			if(renderer==null) {renderer=FindAnyObjectByType<DepthRenderer>(FindObjectsInactive.Include);}
			if(m_SelfV==null) {m_SelfV=gameObject;}m_SelfV.SetActive(false);
			//
// <!-- Macro.Patch Start
			if(scale!=null) {scale.onSliderChanged+=UpdateDepth;}
			if(offset!=null) {offset.onSliderChanged+=UpdateDepth;}
// Macro.Patch -->
			UnityExtension.SetListener<TextureType>(OnTypeChanged,true);
		}

		protected virtual void OnDestroy() {
// <!-- Macro.Patch OnDestroy
			if(scale!=null) {scale.onSliderChanged-=UpdateDepth;}
			if(offset!=null) {offset.onSliderChanged-=UpdateDepth;}
// Macro.Patch -->
			UnityExtension.SetListener<TextureType>(OnTypeChanged,false);
		}

		#endregion Unity Messages

		#region Methods

		protected virtual void OnTypeChanged(TextureType type) {
			switch(type) {
				case TextureType.Depth:
					m_SelfV.SetActive(true);
					if(renderer!=null) {
// <!-- Macro.Patch OnTypeChanged
						scale.SetRange(Vector2.up);scale.SetValueWithoutNotify(renderer.factor.x);
						offset.SetRange(Vector2.up);offset.SetValueWithoutNotify(renderer.factor.y);
// Macro.Patch -->
						UpdateDepth();
					}
				break;
				default:
					m_SelfV.SetActive(false);
				break;
			}
		}

		protected virtual void UpdateDepth() {
			if(renderer!=null) {
				renderer.factor=new Vector2(scale.GetValue(depth.x),offset.GetValue(depth.y));
				renderer.UpdateVector();
				if(MonoApplication.s_Instance!=null) {MonoApplication.s_Instance.dirty|=0x1000;}
			}
		}

		#endregion Methods
	}
}
