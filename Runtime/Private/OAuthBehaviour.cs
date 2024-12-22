// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

/* <!-- Macro.Define OpenURL
		public virtual void {0}() {{
			string url=GetUrl(k_Type_{0});
			if(!string.IsNullOrEmpty(url)) {{UnityEngine.Application.OpenURL(url);}}
			InvokeEvent(k_Type_{0});
		}}

 Macro.End --> */
/* <!-- Macro.Call OpenURL
Register,
Verify,
Forget,
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;using YouSingStudio.Holograms;
using UnityEngine.Networking;

namespace YouSingStudio.Private {
	/// <summary>
	/// Authorization for other systems.
	/// </summary>
	public class OAuthBehaviour
		:MonoBehaviour
		
	{
		public const int k_Type_Register=0;
		public const int k_Type_Login=1;
		public const int k_Type_Logout=2;
		public const int k_Type_Verify=3;
		public const int k_Type_Forget=4;
		public const int k_Type_Error=k_Type_Forget+1;
// <!-- Macro.Patch AutoGen
		public virtual void Register() {
			string url=GetUrl(k_Type_Register);
			if(!string.IsNullOrEmpty(url)) {UnityEngine.Application.OpenURL(url);}
			InvokeEvent(k_Type_Register);
		}

		public virtual void Verify() {
			string url=GetUrl(k_Type_Verify);
			if(!string.IsNullOrEmpty(url)) {UnityEngine.Application.OpenURL(url);}
			InvokeEvent(k_Type_Verify);
		}

		public virtual void Forget() {
			string url=GetUrl(k_Type_Forget);
			if(!string.IsNullOrEmpty(url)) {UnityEngine.Application.OpenURL(url);}
			InvokeEvent(k_Type_Forget);
		}

// Macro.Patch -->
		#region Fields

		public const int k_Offset_DoLogin=3;
		public const int k_Offset_OnLogin=5;
		public static HashSet<string> s_SuccessWords=new HashSet<string>(System.StringComparer.OrdinalIgnoreCase){
			"ok","true","success"
		};

		[SerializeField]protected string m_DisplayName;
		[SerializeField]protected Texture m_AvatarIcon;
		/// <summary>
		/// <seealso cref="Cache.expirationDelay"/>
		/// </summary>
		public int expiration=-1;
		public string url;
		public string[] urls;
		public string[] texts;

		[System.NonSerialized]protected string m_Message;
		[System.NonSerialized]protected string m_Token;
		[System.NonSerialized]protected System.Action[] m_Actions;

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			this.SetRealName();
			displayName=m_DisplayName;avatarIcon=m_AvatarIcon;
			if(m_Actions==null) {m_Actions=new System.Action[k_Type_Error+1];}
			m_Token=GetString(".Token",null);
		}

		protected virtual void Start() {
			if(authorized) {
				if(expired) {Logout();}
				else {OnLogin();}
			}else {
				Login();
			}
		}

		protected virtual void OnEnable() {
			//t(name,null);
		}

		protected virtual void OnDisable() {
			//t(name,null);
		}

		#endregion Unity Messages

		#region Methods

		public static bool IsSuccess(string x) {
			return string.IsNullOrEmpty(x)||s_SuccessWords.Contains(x);
		}

		public static void LoadTexture(string url,System.Action<Texture2D> action) {
			if(string.IsNullOrEmpty(url)||action==null) {return;}
			//
			if(url.StartsWith("http")||url.StartsWith("www")) {// Web
				UnityWebRequest www=UnityWebRequestTexture.GetTexture(url);
				var ao=www.SendWebRequest();
				ao.completed+=(x)=>{
					action.Invoke(DownloadHandlerTexture.GetContent(www));
				};
			}else if(File.Exists(url)) {// File
				Texture2D tex=RenderingExtension.NewTexture2D(1,1);
				tex.LoadImage(File.ReadAllBytes(url));
				action.Invoke(tex);
			}else {
				action.Invoke(null);
			}
		}

		public static void SendRequest(UnityWebRequest www,System.Action<string> action) {
			var ao=www.SendWebRequest();ao.completed+=x=>{
				string e=www.error;
				if(string.IsNullOrEmpty(e)) {
					action?.Invoke(www.downloadHandler.text);
				}else {
					Debug.LogError(www.url+"\n"+e);
				}
			};
		}

		public virtual string GetString(string key,string value) {
			return PlayerPrefs.GetString(name+key,value);
		}

		public virtual void SetString(string key,string value) {
			if(string.IsNullOrEmpty(value)) {PlayerPrefs.DeleteKey(name+key);}
			else {PlayerPrefs.SetString(name+key,value);}
		}

		public virtual string GetUrl(int index) {
			string tmp=urls[index];
			if(!tmp.StartsWith("http")) {tmp=string.Format(url,tmp);}
			return tmp;
		}

		protected virtual void OnError(string msg) {
			string tmp=m_Message;
				m_Message=msg;InvokeEvent(k_Type_Error);
			m_Message=tmp;
		}

		protected virtual void OnLogin(string text) {
			m_Token=null;
			if(string.IsNullOrEmpty(text)) {return;}
			//
			int offset=k_Offset_OnLogin;JObject jo=JObject.Parse(text);
			if(!IsSuccess(jo.SelectToken(texts[offset+0])?.Value<string>())) {
				Debug.LogError(text);OnError(jo.SelectToken(texts[offset+1])?.Value<string>());
				return;
			}
			m_Message=jo.SelectToken(texts[offset+1])?.Value<string>();
			m_Token=jo.SelectToken(texts[offset+2])?.Value<string>();
			string str=jo.SelectToken(texts[offset+3])?.Value<string>();
			string img=jo.SelectToken(texts[offset+4])?.Value<string>();
			//
			if(!string.IsNullOrEmpty(m_Token)) {SetString(".Token",m_Token);}
			if(!string.IsNullOrEmpty(str)) {SetString(".Name",str);}
			if(!string.IsNullOrEmpty(img)) {SetString(".Icon",img);}
			SetString(".Time",System.DateTime.Now.ToString("yy/MM/dd HH:mm:ss"));
			OnLogin();
		}

		protected virtual void OnLogin() {
			if(authorized) {
				displayName=GetString(".Name",m_DisplayName);
				string img=GetString(".Icon",null);
				if(string.IsNullOrEmpty(img)) {
					avatarIcon=m_AvatarIcon;InvokeEvent(k_Type_Login);
				}else {
					LoadTexture(img,OnIcon);
				}
			}
		}

		protected virtual void OnIcon(Texture2D icon) {
			// Create cache.
			try {
				string fn=Path.Combine(UnityEngine.Application.persistentDataPath,name+"_Icon.png");
				File.WriteAllBytes(fn,icon.EncodeToPNG());SetString(".Icon",fn);
			}catch(System.Exception e) {
				Debug.LogException(e);
			}
			//
			avatarIcon=icon;InvokeEvent(k_Type_Login);
		}

		/// <summary>
		/// <seealso cref="System.Net.Cookie.Expired"/>
		/// </summary>
		public virtual bool expired {
			get {
				if(expiration>=0) {
					string str=GetString(".Time",null);
					if(!string.IsNullOrEmpty(str)) {
					if(System.DateTime.TryParse(str,out var dt)) {
						if((System.DateTime.Now-dt).Seconds<=expiration) {return false;}
					}}
					return true;
				}
				return false;
			}
		}

		// API Methods

		public virtual bool authorized {
			get=>!string.IsNullOrEmpty(m_Token);
		}

		public virtual string displayName{get;set;}

		public virtual Texture avatarIcon{get;set;}

		public virtual int GetForm(int type,string[] table) {
			int n=-1;
			switch(type) {
				case k_Type_Login:
					n=3;if(table!=null) {System.Array.Copy(texts,0,table,0,n);}
				break;
				case k_Type_Verify:
					n=2;if(table!=null) {table[0]=texts[0];table[1]=texts[2];}
				break;
				case k_Type_Error:
					n=1;if(table!=null) {table[0]=m_Message;}
				break;
			}
			return n;
		}

		public virtual void SetForm(int type,string[] table) {
			switch(type) {
				case k_Type_Login:
					if(table!=null) {System.Array.Copy(table,0,texts,0,3);}
				break;
				case k_Type_Verify:
					if(table!=null) {texts[0]=table[0];texts[2]=table[1];}
				break;
				case k_Type_Error:
					if(table!=null) {m_Message=table[0];}
				break;
			}
		}

		public virtual void InvokeEvent(int type) {
			if(type>=0&&type<(m_Actions?.Length??0)) {
				m_Actions[type]?.Invoke();
			}
		}

		public virtual void SetEvent(int type,System.Action action,bool value) {
			if(type>=0&&type<(m_Actions?.Length??0)) {
				System.Action tmp=m_Actions[type];
					if(value) {tmp+=action;}else {tmp-=action;}
				m_Actions[type]=tmp;
			}
		}

		public virtual T ToAPI<T>() {
			return default;
		}

		public virtual void Login() {
			Logout();
			if(string.IsNullOrEmpty(texts[0])||(string.IsNullOrEmpty(texts[1])&&string.IsNullOrEmpty(texts[2]))) {return;}
			//
			int offset=k_Offset_DoLogin;
			string str=string.Format(texts[offset+1],texts[0],texts[1]);
			var www=UnityWebRequest.Post(GetUrl(k_Type_Login),str,texts[offset+0]);
			SendRequest(www,OnLogin);
		}

		public virtual void Logout() {
			m_Message=null;
			m_Token=null;SetString(".Token",null);
			displayName=m_DisplayName;SetString(".Name",null);
			avatarIcon=m_AvatarIcon;SetString(".Icon",null);
			InvokeEvent(k_Type_Logout);
		}

		#endregion Methods
	}
}
