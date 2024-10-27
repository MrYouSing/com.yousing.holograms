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
	}
}