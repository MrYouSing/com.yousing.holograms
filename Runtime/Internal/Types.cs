using UnityEngine;

namespace YouSingStudio.Holograms {
	public class ArrayElementAttribute:PropertyAttribute {
		public string names;

		public void Foo()=>throw new System.NotImplementedException();
	}

	/// <summary>
	/// <seealso cref="UnityEditor.TextureImporterShape"/>
	/// </summary>
	public enum TextureType {
		/// <summary>
		/// <seealso cref="Screen"/>
		/// </summary>
		Raw=-1,
		/// <summary>
		/// <seealso cref="Texture2D"/>
		/// </summary>
		Default,
		/// <summary>
		/// <seealso cref="UnityEngine.Video.Video3DLayout"/>
		/// </summary>
		Stereo,
		/// <summary>
		/// <seealso href="https://docs.lookingglassfactory.com/software-tools/looking-glass-studio/rgbd-photo-video"/>
		/// </summary>
		Depth,
		/// <summary>
		/// <seealso href="https://docs.unity3d.com/Manual/VideoPanoramic.html"/>
		/// </summary>
		Panoramic,
		/// <summary>
		/// <seealso href="https://docs.lookingglassfactory.com/software-tools/looking-glass-studio/quilt-photo-video"/>
		/// </summary>
		Quilt,
		/// <summary>
		/// <seealso cref="GameObject"/>
		/// </summary>
		Model,
	}

	/// <summary>
	/// <seealso cref="UnityEngine.UI.Slider"/>
	/// </summary>
	public interface ISlider {
		Vector2 Range{get;}
		float Value{get;set;}
	}

	/// <summary>
	/// <seealso cref="UnityEngine.Plane"/>
	/// </summary>
	public interface IPlane {
		public Vector3 position{get;set;}
		public Quaternion rotation{get;set;}
		public float aspect{get;set;}
		public float size{get;set;}
	}
}