// TODO: Other Properties:Vectors,Colors and Textures
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class UIMaterialController
		:UIBaseController<Material>
	{
		#region Fields

		[System.NonSerialized]protected int[] m_IDs;
		[System.NonSerialized]protected UISliderView[] m_Sliders;

		#endregion Fields

		#region Methods

		protected override void InitView() {
			if(view==null) {this.CheckInstance(m_View,ref view);}
			if(view!=null) {
				m_IDs=System.Array.ConvertAll(view.m_Strings,Shader.PropertyToID);
				m_Sliders=view.m_Sliders.ToViews();
			}
			base.InitView();
		}

		protected override void SetEvents(bool value) {
			base.SetEvents(value);
			//
			if(view!=null) {
				m_Sliders.SetEvent(OnSlider,value);
			}
		}

		public override void SetView(bool value) {
			if(model==null||model.mainTexture==null) {
				value=false;
			}
			base.SetView(value);
		}

		public override void Render() {
			if(model==null||view==null) {return;}
			//
			int j=0,i,imax;
			for(i=0,imax=m_Sliders?.Length??0;i<imax;++i,++j) {
				m_Sliders[i].SetValueWithoutNotify(model.GetFloat(m_IDs[j]));
			}
		}

		public override void Apply() {
			if(model==null||view==null) {return;}
			//
			int j=0,i,imax;
			for(i=0,imax=m_Sliders?.Length??0;i<imax;++i,++j) {
				model.SetFloat(m_IDs[j],m_Sliders[i].GetValue(0.0f));
			}
		}

		public virtual void OnSlider()=>Apply();

		#endregion Methods
	}
}
