using UnityEditor;
using UnityEngine;

namespace YouSingStudio.Holograms.Editor {
	public class SliderInspector
		:UnityEditor.Editor
	{
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			if(target is ISlider tmp) {
				float f=tmp.Value;Vector2 v=tmp.Range;
				if(!float.IsNaN(f)) {tmp.Value=EditorGUILayout.Slider(f,v.x,v.y);}
			}
		}
	}

	[CustomEditor(typeof(QuiltRenderer),true)]
	public partial class QuiltRendererInspector
		:SliderInspector
	{
	}

	[CustomEditor(typeof(DepthRenderer),true)]
	public partial class DepthRendererInspector
		:SliderInspector
	{
	}
}
