// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

/* <!-- Macro.Define OpenURL
		public virtual void {0}() {{
			string url=GetUrl(OAuthAPI.k_Type_{0});
			if(!string.IsNullOrEmpty(url)) {{UnityEngine.Application.OpenURL(url);}}
			InvokeEvent(OAuthAPI.k_Type_{0});
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
// <!-- Macro.Patch AutoGen
		public virtual void Register() {
			string url=GetUrl(0);
			if(!string.IsNullOrEmpty(url)) {UnityEngine.Application.OpenURL(url);}
			InvokeEvent(0);
		}

		public virtual void Verify() {
			string url=GetUrl(3);
			if(!string.IsNullOrEmpty(url)) {UnityEngine.Application.OpenURL(url);}
			InvokeEvent(3);
		}

		public virtual void Forget() {
			string url=GetUrl(4);
			if(!string.IsNullOrEmpty(url)) {UnityEngine.Application.OpenURL(url);}
			InvokeEvent(4);
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

		[System.NonSerialized]protected string m_Token;
		[System.NonSerialized]protected System.Action[] m_Actions;

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			this.SetRealName();
			if(m_Actions==null) {m_Actions=new System.Action[4+1];}
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
				Texture2D tex=UnityExtension.NewTexture2D(1,1);
				tex.LoadImage(File.ReadAllBytes(url));
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
			PlayerPrefs.SetString(name+key,value);
		}

		public virtual string GetUrl(int index) {
			string tmp=urls[index];
			if(!tmp.StartsWith("http")) {tmp=string.Format(url,tmp);}
			return tmp;
		}

		protected virtual void OnLogin(string text) {
			m_Token=null;
			if(string.IsNullOrEmpty(text)) {return;}
			//
			int offset=k_Offset_OnLogin;JObject jo=JObject.Parse(text);
			if(!IsSuccess(jo.SelectToken(texts[offset+0])?.Value<string>())) {
				Debug.LogError(text);
				return;
			}
			m_Token=jo.SelectToken(texts[offset+1])?.Value<string>();
			string str=jo.SelectToken(texts[offset+2])?.Value<string>();
			string img=jo.SelectToken(texts[offset+3])?.Value<string>();
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
					avatarIcon=m_AvatarIcon;InvokeEvent(1);
				}else {
					LoadTexture(img,OnIcon);
				}
			}else {
				Logout();
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
			avatarIcon=icon;InvokeEvent(1);
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
			switch(type) {
				case 1:
					if(table!=null) {System.Array.Copy(texts,0,table,0,3);}
				return 2;
			}
			return -1;
		}

		public virtual void SetForm(int type,string[] table) {
			switch(type) {
				case 1:
					if(table!=null) {System.Array.Copy(table,0,texts,0,3);}
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
			if(string.IsNullOrEmpty(texts[0])||(string.IsNullOrEmpty(texts[1])&&string.IsNullOrEmpty(texts[2]))) {return;}
			//
			int offset=k_Offset_DoLogin;
			string str=string.Format(texts[offset+1],texts[0],texts[1]);
			var www=UnityWebRequest.Post(GetUrl(1),str,texts[offset+0]);
			SendRequest(www,OnLogin);
		}

		public virtual void Logout() {
			m_Token=null;
			displayName=m_DisplayName;
			avatarIcon=m_AvatarIcon;
			InvokeEvent(2);
		}

		#endregion Methods
	}
}
