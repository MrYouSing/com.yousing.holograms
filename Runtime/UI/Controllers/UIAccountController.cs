using UnityEngine;
using UnityEngine.UI;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	public class UIAccountController
		:UIBaseController<OAuthBehaviour>
	{
		#region Fields

		public const int k_Wait=3;
		public static readonly string[] s_Form=new string[4];

		[Header("UI")]
		public bool phone=true;
		public bool silent=true;
		public Text text;
		public RawImage rawImage;
		public Image image;

		[System.NonSerialized]protected float m_Time=-1.0f;

		#endregion Fields

		#region Unity Messages

		protected virtual void Update() {
			if(m_Time>=0.0f&&view!=null) {
				float t=Time.realtimeSinceStartup-m_Time;
				if(t>=0.0f) {
					m_Time=-1.0f;
					view.SetActive(k_Wait,false);
				}else {
					view.SetText(3,$"Wait {(int)-t}s.");
				}
			}
		}

		#endregion Unity Messages

		#region Methods

		protected override void InitView() {
			base.InitView();
			//
			SetLogin(false);
			if(model!=null) {Render();}
		}

		protected override void SetEvents(bool value) {
			var context=model;var login=view;
			//
			if(context!=null) {
				context.SetEvent(OAuthBehaviour.k_Type_Register,Render,value);
				context.SetEvent(OAuthBehaviour.k_Type_Login,Render,value);
				context.SetEvent(OAuthBehaviour.k_Type_Logout,Render,value);
				context.SetEvent(OAuthBehaviour.k_Type_Verify,OnVerify,value);
				context.SetEvent(OAuthBehaviour.k_Type_Error,OnError,value);
				if(login!=null) {
					login.BindButton(OAuthBehaviour.k_Type_Register,context.Register,value);
					login.BindButton(OAuthBehaviour.k_Type_Login,Login,value);
					login.BindButton(OAuthBehaviour.k_Type_Logout,Logout,value);
					login.BindButton(OAuthBehaviour.k_Type_Verify,Verify,value);
					login.BindButton(OAuthBehaviour.k_Type_Forget,context.Forget,value);
					login.BindButton(5,Login,value);
					// Hack Buttons
					int len=login.m_Buttons?.Length??0;
					if(6<len) {// Close
						Button btn=login.m_Buttons[6];
						if(btn!=null) {
							if(value) {btn.SetOnClick(()=>SetView(false));}
							else {btn.SetOnClick(null);}
						}
					}
				}
			}
		}

		public virtual void SetLogin(bool value) {
			if(model==null||!model.enabled) {value=false;}
			if(view!=null) {view.gameObject.SetActive(value);}
			//
			this.LockShortcuts(value);
		}

		public override void Render() {
			var context=model;var login=view;
			//
			if(context!=null) {
				if(text!=null) {text.text=context.displayName;}
				if(rawImage!=null) {rawImage.texture=context.avatarIcon;}
				if(image!=null) {
					Sprite tmp=context.statusIcon;
					if(tmp!=null) {image.sprite=tmp;image.enabled=tmp!=null;}
					else {image.enabled=context.authorized&&image.sprite!=null;}
				}
				//
				if(login!=null) {
					login.SetText(0,context.displayName);
					login.SetRawImage(0,context.avatarIcon);
				}
				OnMessage(null);
				if(context.authorized) {
					if(login!=null) {
						login.SetActive(0,false);
						login.SetActive(1,true);
						login.SetActive(2,false);
					}
					SetLogin(false);
				}else {
					if(login!=null) {
						context.GetForm(OAuthBehaviour.k_Type_Login,s_Form);
						login.SetInputField(0,s_Form[0]);
						login.SetInputField(1,s_Form[1]);
						login.SetInputField(2,s_Form[0]);
						login.SetInputField(3,null);//s_Form[2]); Never save verify code.
						//
						bool b=string.IsNullOrEmpty(s_Form[1]);
						if(b&&string.IsNullOrEmpty(s_Form[2])) {b=phone;}
						//
						login.SetActive(0,!b);
						login.SetActive(1,false);
						login.SetActive(2,b);
					}
					if(!silent) {SetLogin(true);}
				}
			}
		}

		public override void SetView(bool value) {
			if(value) {Render();}SetLogin(value);
		}

		public virtual void Login() {
			if(model!=null&&view!=null) {
				bool b=view.GetActive(0);
				s_Form[0]=view.GetInputField(b?0:2);
				s_Form[1]=!b?null:view.GetInputField(1);
				s_Form[2]=b?null:view.GetInputField(3);
				model.SetForm(OAuthBehaviour.k_Type_Login,s_Form);model.Login();
			}
		}

		public virtual void Logout() {
			if(model!=null&&view!=null) {
				if(true) {// TODO: Clean the password.
				model.GetForm(OAuthBehaviour.k_Type_Login,s_Form);
					s_Form[1]=null;PlayerPrefs.DeleteKey(model.name+".Password");
				model.SetForm(OAuthBehaviour.k_Type_Login,s_Form);
				}
				//
				model.Logout();
			}
		}

		public virtual void Verify() {
			if(model!=null&&view!=null) {
				s_Form[0]=view.GetInputField(2);
				s_Form[1]="login";
				model.SetForm(OAuthBehaviour.k_Type_Verify,s_Form);model.Verify();
			}
		}

		protected virtual void OnVerify() {
			OnMessage(null);
			if(view!=null) {
			if(view.GetGameObject(k_Wait)!=null) {
				view.SetActive(k_Wait,true);
				m_Time=Time.realtimeSinceStartup+60.0f;
			}}
		}

		protected virtual void OnMessage(string color) {
			model.GetForm(OAuthBehaviour.k_Type_Error,s_Form);
			//
			if(s_Form[0]=="$(ShowLogin)") {SetLogin(true);return;}
			//
			if(view!=null) {
				string str=string.IsNullOrEmpty(color)?s_Form[0]:$"<color=#{color}>{s_Form[0]}</color>";
				view.SetText(1,str);
				view.SetText(2,str);
			}
		}

		protected virtual void OnError()=>OnMessage("FF0000FF");

		#endregion Methods
	}
}
