using UnityEngine;
using UnityEngine.UI;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	public class UIAccountView
		:MonoBehaviour
	{
		#region Fields

		public const int k_Wait=3;
		public static readonly string[] s_Form=new string[4];

		public OAuthBehaviour context;
		[SerializeField]protected string m_Context;
		public ScriptableView login;
		[SerializeField]protected string m_Login;
		[Header("UI")]
		public bool phone=true;
		public bool silent=true;
		public Text text;
		public RawImage image;

		[System.NonSerialized]protected float m_Time=-1.0f;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			this.CheckInstance(m_Context,ref context);
			this.CheckInstance(m_Login,ref login);
			SetEvents(true);
/*			StartCoroutine(StartDelayed());
		}

		protected virtual System.Collections.IEnumerator StartDelayed() {
			yield return null;*/
			if(context!=null) {Render();}
		}

		protected virtual void OnDestroy() {
			SetEvents(false);
		}

		protected virtual void Update() {
			if(m_Time>=0.0f&&login!=null) {
				float t=Time.realtimeSinceStartup-m_Time;
				if(t>=0.0f) {
					m_Time=-1.0f;
					login.SetActive(k_Wait,false);
				}else {
					login.SetText(3,$"Wait {(int)-t}s.");
				}
			}
		}

		#endregion Unity Messages

		#region Methods

		public virtual void SetEvents(bool value) {
			if(context!=null) {
				context.SetEvent(UnityExtension.k_OAuth_Register,Render,value);
				context.SetEvent(UnityExtension.k_OAuth_Login,Render,value);
				context.SetEvent(UnityExtension.k_OAuth_Logout,Render,value);
				context.SetEvent(UnityExtension.k_OAuth_Verify,OnVerify,value);
				context.SetEvent(UnityExtension.k_OAuth_Error,OnError,value);
				if(login!=null) {
					login.BindButton(UnityExtension.k_OAuth_Register,context.Register,true);
					login.BindButton(UnityExtension.k_OAuth_Login,Login,true);
					login.BindButton(UnityExtension.k_OAuth_Logout,Logout,true);
					login.BindButton(UnityExtension.k_OAuth_Verify,Verify,true);
					login.BindButton(UnityExtension.k_OAuth_Forget,context.Forget,true);
					login.BindButton(5,Login,true);
				}
			}
		}

		public virtual void SetLogin(bool value) {
			if(context==null||!context.enabled) {value=false;}
			if(login!=null) {login.gameObject.SetActive(value);}
		}

		public virtual void Render() {
			if(context!=null) {
				if(text!=null) {text.text=context.displayName;}
				if(image!=null) {image.texture=context.avatarIcon;}
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
						context.GetForm(UnityExtension.k_OAuth_Login,s_Form);
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

		public virtual void Login() {
			if(context!=null&&login!=null) {
				bool b=login.GetActive(0);
				s_Form[0]=login.GetInputField(b?0:2);
				s_Form[1]=!b?null:login.GetInputField(1);
				s_Form[2]=b?null:login.GetInputField(3);
				context.SetForm(UnityExtension.k_OAuth_Login,s_Form);context.Login();
			}
		}

		public virtual void Logout() {
			if(context!=null&&login!=null) {
				if(true) {// TODO: Clean the password.
				context.GetForm(UnityExtension.k_OAuth_Login,s_Form);
					s_Form[1]=null;
				context.SetForm(UnityExtension.k_OAuth_Login,s_Form);
				}
				//
				context.Logout();
			}
		}

		public virtual void Verify() {
			if(context!=null&&login!=null) {
				s_Form[0]=login.GetInputField(2);
				s_Form[1]="login";
				context.SetForm(UnityExtension.k_OAuth_Verify,s_Form);context.Verify();
			}
		}

		protected virtual void OnVerify() {
			OnMessage(null);
			if(login!=null) {
			if(login.GetGameObject(k_Wait)!=null) {
				login.SetActive(k_Wait,true);
				m_Time=Time.realtimeSinceStartup+60.0f;
			}}
		}

		protected virtual void OnMessage(string color) {
			context.GetForm(UnityExtension.k_OAuth_Error,s_Form);
			if(login!=null) {
				string str=string.IsNullOrEmpty(color)?s_Form[0]:$"<color=#{color}>{s_Form[0]}</color>";
				login.SetText(1,str);
				login.SetText(2,str);
			}
		}

		protected virtual void OnError()=>OnMessage("FF0000FF");

		#endregion Methods
	}
}
