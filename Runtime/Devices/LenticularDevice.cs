using UnityEngine;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso href="https://en.wikipedia.org/wiki/Lenticular_printing"/>
	/// </summary>
	public class LenticularDevice
		:HologramDevice
	{
		#region Nested Types
		#endregion Nested Types

		#region Fields

		public static readonly int _InputSize=Shader.PropertyToID("_InputSize");
		public static readonly int _OutputSize=Shader.PropertyToID("_OutputSize");
		public static readonly int _Arguments=Shader.PropertyToID("_Arguments");

		[Header("Lenticular")]
		public float pitch;
		public float slope;
		public float center;

		#endregion Fields

		#region Unity Messages
		#endregion Unity Messages

		#region Methods

		protected override void InternalRender() {
			if(material!=null) {
				material.SetVector(_InputSize,new Vector4(quiltSize.x,quiltSize.y));
				material.SetVector(_OutputSize,new Vector4(resolution.x,resolution.y));
				material.SetVector(_Arguments,new Vector4(slope,pitch,center));
			}
			base.InternalRender();
		}

		public override string ToJson() {
			if(!m_IsInited) {Init();}
			return $"{{\"display\":{display},\"pitch\":{pitch},\"slope\":{slope},\"center\":{center}}}";
		}

		#endregion Methods
	}
}
