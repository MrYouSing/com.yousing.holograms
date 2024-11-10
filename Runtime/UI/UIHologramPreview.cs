using UnityEngine;
using UnityEngine.UI;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// A preview on non-hologram screen.<br/>
	/// Value:View index.
	/// </summary>
	public class UIHologramPreview
		:UISliderView
	{
		#region Fields

		[Header("Hologram")]
		public ScreenDevice screen;
		public WrapMode wrap;
		public float duration=1.0f;
		[Header("UI")]
		public CanvasGroup[] groups;
		public RawImage[] images;
		public GameObject durationUI;

		[System.NonSerialized]protected float m_Time;
		[System.NonSerialized]protected WrapMode m_Wrap;

		#endregion Fields

		#region Unity Messages

		protected override void Start() {
			base.Start();
			//
			if(screen==null) {screen=FindAnyObjectByType<ScreenDevice>();}
			if(screen!=null) {screen.onDefaultChanged+=()=>SetWrap(wrap);}
			m_Wrap=(WrapMode)PlayerPrefs.GetInt(name+".DefaultWrap");
			SetWrap(wrap);
		}

		protected virtual void OnDestroy() {
			PlayerPrefs.SetInt(name+".DefaultWrap",(int)m_Wrap);
		}

		protected virtual void Update() {
			if(screen==null) {return;}
			//
			switch(wrap) {
				case WrapMode.Loop:
					OnSliderChanged(GetIndex(Time.time-m_Time,false));
				break;
				case WrapMode.PingPong:
					OnSliderChanged(GetIndex(Time.time-m_Time,true));
				break;
				case WrapMode.ClampForever+1:
					OnSliderChanged(GetIndex(-(Time.time-m_Time),false));
				break;
				default:
				break;
			}
		}

		#endregion Unity Messages

		#region Methods

		protected virtual int GetIndex(float time,bool pingpong) {
			int cnt=screen.quiltSize.x*screen.quiltSize.y;
			time=(time/duration)*cnt;
			if(pingpong) {time=Mathf.PingPong(time,cnt);}
			else {time=Mathf.Repeat(time,cnt);}
			return Mathf.FloorToInt(time);
		}

		protected virtual void SetActive(byte value) {
			for(int i=0,imax=images?.Length??0;i<imax;++i) {
				if((value&(1<<i))!=0) {
					images[i].enabled=true;
					groups[i].SetActive(true);
				}else {
					images[i].enabled=false;
					groups[i].SetActive(false);
				}
			}
		}

		protected virtual void SetSlider(bool value) {
			m_SliderV.SetActive(value);
			SetActive(0x2);
		}

		protected virtual void SetAnimation(bool value) {
			if(durationUI!=null) {durationUI.SetActive(value);}
			if(m_FieldV!=null) {m_FieldV.SetActive(!value);}
			//
			if(value) {Update();}
			else {UpdateSlider();}
		}

		public virtual void SetParent(Transform p) {
			images[0].transform.SetParent(p,false);
		}

		public virtual void SetWrap(WrapMode value) {
			if(value<0) {UIWrapMode.current.OnValueChanged(m_Wrap);return;}
			wrap=value;if(!didStart) {return;}
			//
			m_Time=Time.time;
			switch(wrap) {
				case WrapMode.ClampForever:// Quilt views.
					SetActive(0x1);
				break;
				case WrapMode.Once:// One view.
					m_Wrap=wrap;SetSlider(false);
					screen.quiltIndex=-1;SetAnimation(false);
					if(m_FieldV!=null) {m_FieldV.SetActive(false);}
				break;
				default:// Change view automatically or manually.
					m_Wrap=wrap;SetSlider(true);
					SetAnimation(wrap!=WrapMode.Default);
				break;
			}
		}

		public virtual void SetDuration(float value)=>duration=value;

		protected virtual void UpdateSlider() {
			int cnt=screen.quiltSize.x*screen.quiltSize.y;
			int idx=screen.GetIndex();
			if(slider!=null) {
				slider.wholeNumbers=true;
				if(screen!=null) {
					string str=(idx+1).ToString();// C2H
					slider.maxValue=cnt-1;slider.SetValueWithoutNotify(idx);// H2C
					SetText(0,1);SetText(1,str);SetText(2,cnt);
					if(field!=null) {field.SetTextWithoutNotify(str);}
				}
			}
		}

		protected override void OnSliderChanged(float value) {
			screen.quiltIndex=(int)value;
			UpdateSlider();
		}

		protected override void OnSliderChanged(string value) {
			if(int.TryParse(value,out int idx)) {
				screen.quiltIndex=idx;
				UpdateSlider();
			}
		}

		#endregion Methods
	}
}
