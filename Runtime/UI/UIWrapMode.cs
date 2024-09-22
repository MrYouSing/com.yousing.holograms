using UnityEngine;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="WrapMode"/>
	/// </summary>
	public class UIWrapMode
		:UIEnumView<WrapMode>
	{
		#region Methods

		public override void OnValueChanged(WrapMode value) {
			if(value>=0) {
				base.OnValueChanged(value);
			}else {
				var tmp=current;current=this;
					m_OnValueChanged?.Invoke(value);
				current=tmp;
			}
		}

		public virtual void Set(int value)=>OnValueChanged((WrapMode)value);

		#endregion Methods
	}
}
