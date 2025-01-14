using System.Collections.Generic;
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class UISceneController
		:UIBaseController
	{
		#region Fields

		public UIToggleButton button;
		[Header("Transform")]
		public UIToggleButton toggle;
		public List<Transform> targets;
		public List<UITransformEditor> editors;
		[Header("Rendering")]
		public SkyboxLoader skyboxLoader;
		public UIMaterialController skyboxController;

		[System.NonSerialized]protected Transform m_Target;

		#endregion Fields

		#region Unity Messages

		protected virtual void Update() {
			Transform t=GetTarget();
			if(t!=m_Target) {SetTarget(t);}
			if(t!=null&&t.hasChanged) {SetTarget(t);}
		}

		#endregion Unity Messages

		#region Methods

		public override void HideView() {
			if(button!=null) {button.SetIsOn(false);}
			else {base.HideView();}
		}

		protected override void InitView() {
			var e=FindObjectsInactive.Include;
			if(button==null) {button=GetComponent<UIToggleButton>();}
			if(skyboxLoader==null) {skyboxLoader=FindAnyObjectByType<SkyboxLoader>(e);}
			base.InitView();
			//
			if(view!=null) {
				m_Actor=view.gameObject;
				//
				int j=0,i,imax;
				for(i=0,imax=editors?.Count??0;i<imax;++i,++j) {
					if(editors[i]==null) {editors[i]=view.m_GameObjects[j].GetComponent<UITransformEditor>();}
				}
				if(skyboxController==null) {
					skyboxController=view.m_GameObjects[j].GetComponent<UIMaterialController>();
				}++j;
			}
		}

		public override void SetView(bool value) {
			if(m_Actor!=null) {m_Actor.SetActive(value);}
			if(value) {
				if(skyboxController!=null) {
					if(skyboxLoader!=null) {
						skyboxController.SetView(skyboxLoader.type==1);
					}
					skyboxController.LockShortcuts(false);
				}
				//
				Render();
			}
		}

		protected override void SetEvents(bool value) {
			base.SetEvents(value);
			if(skyboxLoader!=null) {
				skyboxLoader.onLoaded-=OnSkybox;
				if(value) {skyboxLoader.onLoaded+=OnSkybox;}
			}
		}

		public override void Render() {
			SetTarget(m_Target);
		}

		protected virtual bool IsView()=>view!=null&&view.isActiveAndEnabled;

		protected virtual Transform GetTarget() {
			if(IsView()) {
			if(toggle!=null&&(targets?.Count??0)==2) {
				return targets[toggle.isOn?1:0];
			}}
			return m_Target;
		}

		protected virtual void SetTarget(Transform value) {
			m_Target=value;
			//
			UITransformEditor it;for(int i=0,imax=editors?.Count??0;i<imax;++i) {
				it=editors[i];if(it!=null) {it.Set(m_Target);}
			}
			//
			if(m_Target!=null) {m_Target.hasChanged=false;}
		}

		protected virtual void OnSkybox() {
			if(IsView()) {SetView(true);}
		}

		#endregion Methods
	}
}
