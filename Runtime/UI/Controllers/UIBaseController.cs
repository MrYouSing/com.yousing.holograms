using UnityEngine;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	public class UIBaseController
		:MonoBehaviour
	{
		#region Fields

		public ScriptableView view;
		[SerializeField]protected string m_View;

		[System.NonSerialized]protected GameObject m_Actor;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			InitView();
		}

		protected virtual void OnDestroy() {
			ExitView();
		}

		#endregion Unity Messages

		#region Methods

		public virtual void ShowView()=>SetView(true);
		public virtual void HideView()=>SetView(false);
		public virtual void ToggleView()=>SetView(!(view!=null&&view.isActiveAndEnabled));

		protected virtual void InitView() {
			if(view==null) {this.CheckInstance(m_View,ref view);}
			if(view!=null) {SetEvents(true);}
		}

		protected virtual void ExitView() {
			if(view!=null) {SetEvents(false);}
		}

		protected virtual void SetEvents(bool value) {
			int cnt=view!=null?(view.m_Buttons?.Length??0):0;
			if(0<cnt) {
				var btn=view.m_Buttons[0];
				if(btn!=null) {
					if(value) {btn.SetOnClick(HideView);}
					else {btn.SetOnClick(null);}
				}
			}
		}

		public virtual void SetView(bool value) {
			int n=view!=null?(view.didStart?0x2:0x1):0x0;
			if(m_Actor==null&&n!=0) {m_Actor=view.gameObject;}
			//
			if(n==0x1) {m_Actor.SetActive(value);}
				if(value) {Render();}
			if(n==0x2) {m_Actor.SetActive(value);}
			//
			this.LockShortcuts(value);
		}

		/// <summary>
		/// Model->View
		/// </summary>
		public virtual void Render() {
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// View->Model
		/// </summary>
		public virtual void Apply() {
			throw new System.NotImplementedException();
		}

		#endregion Methods
	}

	public class UIBaseController<T>
		:UIBaseController
		where T:Object
	{
		#region Fields

		public T model;
		[SerializeField]protected string m_Model;

		#endregion Fields

		#region Unity Messages

		protected override void Start() {
			InitModel();
			base.Start();
		}

		protected override void OnDestroy() {
			ExitModel();
			base.OnDestroy();
		}

		#endregion Unity Messages

		#region Methods

		protected virtual void InitModel() {
			if(model==null) {this.CheckInstance<T>(m_Model,ref model);}
		}

		protected virtual void ExitModel() {
			if(model!=null) {}
		}

		#endregion Methods
	}
}
