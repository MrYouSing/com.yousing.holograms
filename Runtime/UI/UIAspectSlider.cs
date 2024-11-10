using UnityEngine;
using UnityEngine.Video;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// Value:Aspect offset.
	/// </summary>
	public class UIAspectSlider
		:UISliderView
	{
		#region Fields

		[Header("Aspect")]
		public UIAspectRatio aspect;
		[SerializeField]protected Object[] m_Sliders;

		[System.NonSerialized]public ISlider[] sliders;
		[System.NonSerialized]public ISlider current;

		#endregion Fields

		#region Unity Messages

		protected override void Start() {
			if(aspect==null) {aspect=GetComponentInChildren<UIAspectRatio>();}
			base.Start();
			//
			int i=0,imax=m_Sliders?.Length??0;if(imax>0) {
				sliders=new ISlider[imax];
				ISlider it;for(;i<imax;++i) {
					it=m_Sliders[i] as ISlider;
					if(it!=null) {
						sliders[i]=it;aspect.onValueChanged+=GetListener(it);
					}
				}
			}
			//
			HideSlider();
			aspect.onValueChanged+=OnAspectChanged;
			aspect.OnValueChanged(aspect.value);
			UnityExtension.SetListener<TextureType>(OnTypeChanged,true);
		}

		protected virtual void OnDestroy() {
			UnityExtension.SetListener<TextureType>(OnTypeChanged,false);
		}

		protected virtual void Update() {
			OnAspectChanged((VideoAspectRatio)(-1));
		}

		#endregion Unity Messages

		#region Methods

		protected virtual System.Action<VideoAspectRatio> GetListener(object target) {
			if(target!=null) {
				System.Type type=target.GetType();
				var mi=type.GetMethod("SetAspect");
				if(mi!=null) {
					return System.Delegate.CreateDelegate(typeof(System.Action<VideoAspectRatio>),
						target,mi) as System.Action<VideoAspectRatio>;
				}
			}
			return null;
		}

		protected virtual void OnTypeChanged(TextureType type) {
			switch(type) {
				case TextureType.Panoramic:
				case TextureType.Model:
					m_SelfV.SetActive(false);
				break;
				default:
					current=null;HideSlider();
					m_SelfV.SetActive(true);
				break;
			}
		}

		protected virtual void OnAspectChanged(VideoAspectRatio value) {
			ISlider it,best=null;for(int i=0,imax=sliders?.Length??0;i<imax;++i) {
				it=sliders[i];if(it!=null&&!float.IsNaN(it.Value)) {best=it;}
			}
			if(best!=current) {
				current=best;
				//
				UpdateSlider(current?.Value??0.0f);
			}
		}

		protected override void UpdateSlider(float value) {
			if(current!=null) {
				current.Value=value;Vector2 v=current.Range;
				if(slider!=null) {
					slider.minValue=v.x;
					slider.maxValue=v.y;
				}
				base.UpdateSlider(value);
			}else {
				HideSlider();
			}
		}

		#endregion Methods
	}
}