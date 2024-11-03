using UnityEngine;
using UnityEngine.UI;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	public class UIAccountView
		:MonoBehaviour
	{
		#region Fields

		public OAuthBehaviour context;
		[SerializeField]protected string m_Context;
		public ScriptableView login;
		[SerializeField]protected string m_Login;
		[Header("UI")]
		public bool silent=true;
		public Text text;
		public RawImage image;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			this.CheckInstance(m_Context,ref context);
			this.CheckInstance(m_Login,ref login);
			SetEvents(true);
			if(context!=null) {Render();}
		}

		protected virtual void OnDestroy() {
			SetEvents(false);
		}

		#endregion Unity Messages

		#region Methods

		public virtual void SetEvents(bool value) {
			if(context!=null) {
				context.SetEvent(0,Render,value);
				context.SetEvent(1,Render,value);
				context.SetEvent(2,Render,value);
				context.SetEvent(3,Render,value);
				if(login!=null) {
					if(value) {
						login.BindButton(0,context.Register);
						login.BindButton(1,context.Login);
						login.BindButton(2,context.Logout);
						login.BindButton(3,context.Verify);
						login.BindButton(4,context.Forget);
					}else {
						login.UnbindButton(0,context.Register);
						login.UnbindButton(1,context.Login);
						login.UnbindButton(2,context.Logout);
						login.UnbindButton(3,context.Verify);
						login.UnbindButton(4,context.Forget);
					}
				}
			}
		}

		public virtual void Login(bool value) {
			if(login!=null) {login.gameObject.SetActive(value);}
		}

		public virtual void Render() {
			if(context!=null) {
				if(text!=null) {text.text=context.displayName;}
				if(image!=null) {image.texture=context.avatarIcon;}
				if(context.authorized) {
					if(login!=null) {
						login.SetCanvasGroup(0,true);
						login.SetCanvasGroup(1,false);
					}
					if(!silent) {Login(true);}
				}else {
					if(login!=null) {
						login.SetCanvasGroup(0,false);
						login.SetCanvasGroup(1,true);
					}
				}
			}
		}

		#endregion Methods
	}
}
