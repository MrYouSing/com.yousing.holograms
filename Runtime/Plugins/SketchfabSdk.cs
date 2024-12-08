using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using YouSingStudio.Holograms;
using YouSingStudio.Private;

namespace Sketchfab {
	#region Nested Types

	public struct SketchfabImage {
		public string uid;
		public string url;
		public int size;
		public int width;
		public int height;
	}

	public abstract class SketchfabTask {
		public SketchfabSdk context;
		public string uid;

		public abstract void Run();
		public abstract void Kill();
	}

	public class DownloadTask:SketchfabTask {
		public string path;
		public System.Action<string> action;

		public SketchfabRequest request;
		public UnityWebRequest preview;
		public UnityWebRequest model;

		public override void Kill() {
			if(context==null) {return;}
			//
			try {
				context.DisposeRequest(request);
				preview?.Dispose();model?.Dispose();
			}catch(System.Exception e) {
				Debug.LogException(e);
			}
			//
			context=null;request=null;
			preview=model=null;
		}

		public override void Run() {
			if(context==null) {return;}
			//
			MessageBox.ShowInfo(context.messages[0],"Fetch uid="+uid);
			//
			request=new SketchfabRequest(SketchfabPlugin.Urls.modelEndPoint+"/"+uid,context.m_Logger.getHeader());
			request.setCallback(OnFetch);request.setFailedCallback(OnFetch);
			context.m_API.registerRequest(request);
		}

		protected virtual void OnFetch(string json) {
			if(context==null) {return;}
			//
			JObject jo=JObject.Parse(json);JToken jt=jo.SelectToken("detail");
			if(jt!=null) {
				context.OnError(this,jt.Value<string>());return;
			}else {
				bool a=jo.SelectToken("isDownloadable")?.Value<bool>()??true;
				bool b=jo.SelectToken("isProtected")?.Value<bool>()??false;
				//if(!a||b) {context.OnError(this,"Can not download this asset!");return;}
				//
				context.SetInfo(uid,jo);
				context.DownloadPreview(this,jo.SelectToken("thumbnails.images")?.ToObject<SketchfabImage[]>()??null);
			}
		}

		public virtual void Pull() {
			if(context==null) {return;}
			//
			MessageBox.ShowInfo(context.messages[0],"Pull uid="+uid);
			//
			request=new SketchfabRequest($"https://sketchfab.com/models/{uid}/embed");
			request.setCallback(OnPull);request.setFailedCallback(OnPull);
			context.m_API.registerRequest(request);
		}

		public virtual void OnPull(string html) {
			if(context==null) {return;}
			//
			const string s="id=\"js-dom-data-prefetched-data\"><!--",e="--></div>";
			int i=html.IndexOf(s),j=i>=0?html.IndexOf(e,i):-1;
			if(i>=0&&j>=0) {
				i+=s.Length;html=html.Substring(i,j-i).Replace("&#34;","\"");
				context.SetInfo(uid,JObject.Parse(html));Debug.Log(html);
			}
			Download();
		}

		public virtual void Download() {
			if(context==null) {return;}
			//
			MessageBox.ShowInfo(context.messages[0],"Download uid="+uid);
			//
			request=new SketchfabRequest(SketchfabPlugin.Urls.modelEndPoint+"/"+uid+"/download",context.m_Logger.getHeader());
			request.setCallback(OnDownload);request.setFailedCallback(OnDownload);
			context.m_API.registerRequest(request);
		}

		protected virtual void OnDownload(string json) {
			if(context==null) {return;}
			//
			JObject jo=JObject.Parse(json);JToken jt=jo.SelectToken("detail");
			if(jt!=null) {context.OnError(this,jt.Value<string>());}
			else {context.DownloadModel(this,jo);}
		}

		public virtual void OnPreview(AsyncOperation ao) {
			if(context==null) {return;}
			//
			if(model==null) {Pull();}
		}

		public virtual void OnModel(AsyncOperation ao) {
			if(context==null) {return;}
			//
			OnDownload(ao);
		}

		protected virtual void OnDownload(AsyncOperation ao) {
			if(context==null) {return;}
			//
			if(context.IsDone(preview)&&context.IsDone(model)) {
				context.OnDownload(this);
			}
		}
	}

	#endregion Nested Types

	public class SketchfabSdk
		:OAuthBehaviour
	{
		#region Fields

		public static SketchfabSdk s_Instance;

		[Header("Sketchfab")]
		public string username;
		public string password;

		public int preview;
		public string download;
		public string[] formats=new string[]{"gltf","glb"};
		public string[] messages=new string[4];

		[System.NonSerialized]public SketchfabTask current;
		[System.NonSerialized]internal SketchfabAPI m_API;
		[System.NonSerialized]internal SketchfabLogger m_Logger;
		[System.NonSerialized]protected bool m_Updating;
		[System.NonSerialized]protected int m_Logged;
		[System.NonSerialized]protected List<SketchfabTask> m_Tasks;
		[System.NonSerialized]protected Dictionary<string,JToken> m_Infos;

		#endregion Fields

		#region Unity Messages

		protected virtual void Reset() {
			expiration=60*60*24*30;// One month.
			url="";
			urls=new string[]{
			};
			texts=new string[]{
				"",// user name
				"",// password
				"",// verification code
			};
		}

		protected override void Awake() {
			s_Instance=this;this.SetRealName();
			// Trick by settings.
			username=GetString(".Username",texts[0]);
			password=GetString(".Password",texts[1]);
				this.LoadSettings(name);
			texts[0]=username;
			texts[1]=password;
			base.Awake();
			//
#if !CSHARP_ZLIB
			for(int i=0,imax=formats?.Length??0;i<imax;++i) {
				switch(formats[i]) {
					case "gltf":
						formats[i]=null;
					break;
				}
			}
#endif
		}

		protected override void Start() {
			string key=name+".Token";
			SketchfabLogger.accessTokenKey=key;
			//
			m_API=SketchfabPlugin.getAPI();
			m_Logger=SketchfabPlugin.getLogger();
			//
			m_Logged=authorized?1:0;
			if(m_Logged==0) {Login();}
			else {m_Logger.checkAccessTokenValidity();m_Logged=0;}
		}

		protected virtual void Update() {
			m_Updating=true;
			CheckLogger();
			m_API?.Update();
			m_Updating=false;
		}

		#endregion Unity Messages

		#region Methods

		public static SketchfabSdk instance {
			get{
				if(s_Instance==null) {
					s_Instance=UnityExtension.GetResourceInstance<SketchfabSdk>("Prefabs/Sketchfab");
				}
				return s_Instance;
			}
		}

		public static int FindIndex(IList<SketchfabImage> images,int size,System.Func<int,int,int> func) {
			SketchfabImage it;int sqr,bestIdx=-1,bestSqr=int.MaxValue;
			for(int i=0,imax=images?.Count??0;i<imax;++i) {
				it=images[i];sqr=func(it.width,it.height);
				sqr-=size;sqr*=sqr;
				if(sqr<bestSqr) {bestSqr=sqr;bestIdx=i;}
			}
			return bestIdx;
		}

		public virtual string GetUid(string path) {
			if(path.IsWebsite()&&string.IsNullOrEmpty(Path.GetExtension(path))) {
				int l=path.Length;if(path.EndsWith("/embed",UnityExtension.k_Comparison)) {l-=6;}
				int i=path.LastIndexOf('/',l-1),j=path.LastIndexOf('-',l-1);if(i<j) {i=j;}
				if(i>=0) {path=path.Substring(i+1,l-i-1);}
			}
			return path;
		}

		public virtual string GetPath(string path,string uid,string url) {
			int i=url.IndexOf('?'),j=-1;if(i>=0) {j=url.LastIndexOf('/',i);}
			url=url.Substring(j+1,i-j-1);string ext=Path.GetExtension(url);
			if(url!=uid) {url=uid.Substring(0,8)+"_"+url;}
			//
			if(string.IsNullOrEmpty(path)) {path=Path.Combine(download,url);}
			else {path=Path.ChangeExtension(path,ext);}
			//
			return path;
		}

		// Member APIs

		public virtual JToken GetInfo(string uid) {
			if(m_Infos==null||string.IsNullOrEmpty(uid)) {return null;}
			//
			m_Infos.TryGetValue(uid,out var info);return info;
		}

		public virtual void SetInfo(string uid,JToken info) {
			if(string.IsNullOrEmpty(uid)) {return;}
			//
			if(info!=null) {
				if(m_Infos==null) {m_Infos=new Dictionary<string,JToken>();}
				m_Infos[uid]=info;
			}else {
				m_Infos?.Remove(uid);
			}
		}

		public virtual void DisposeRequest(SketchfabRequest request) {
			if(request!=null) {
				if(m_Updating) {return;}
				int i=m_API._requests.IndexOf(request);
				if(i>=0) {m_API._requests[i]=null;}
				request.dispose();
			}
		}

		protected virtual void AddTask(SketchfabTask task) {
			if(task==null) {return;}task.context=this;
			if(m_Logged>0) {task.Run();return;}
			//
			if(m_Tasks==null) {m_Tasks=new List<SketchfabTask>();}
			m_Tasks.Add(task);
		}

		protected virtual void RunTask() {
			if(string.IsNullOrEmpty(m_Token)) {
				m_Token=PlayerPrefs.GetString(SketchfabLogger.accessTokenKey);
			}
			//
			int i=0,imax=m_Tasks?.Count??0;
			if(imax>0) {
				for(;i<imax;++i) {m_Tasks[i]?.Run();}
				m_Tasks.Clear();
			}
		}

		// Web APIs

		public virtual bool IsDone(UnityWebRequest www) {
			return www==null||www.isDone;
		}

		public virtual bool IsError(SketchfabTask task,UnityWebRequest www) {
			if(www==null) {return true;}string e=www.error;
			if(!string.IsNullOrEmpty(e)) {OnError(task,e);return true;}
			return false;
		}

		public virtual void OnError(SketchfabTask task,string error) {
			if(task==null) {return;}
			//
			MessageBox.ShowError(messages[2],error);
			task.Kill();
		}

		public virtual SketchfabTask Download(string uid,System.Action<string> action) {
			if(!string.IsNullOrEmpty(uid)&&action!=null) {
				DownloadTask d=new DownloadTask();
				d.uid=uid;
				d.action=action;
				AddTask(d);return d;
			}
			return null;
		}

		public virtual void DownloadPreview(DownloadTask task,IList<SketchfabImage> images) {
			int i=-1,imax=images?.Count??0;
			if(task==null||imax<=0) {return;}
			//
			switch(preview) {
				case 0:break;
				case 1:i=0;break;
				case -1:i=imax-1;break;
				default:
					i=FindIndex(images,Mathf.Abs(preview),preview>0?Mathf.Max:Mathf.Min);
				break;
			}
			//
			if(i>=0) {
				UnityWebRequest www=UnityWebRequest.Get(images[i].url);var ao=www.SendWebRequest();
				MessageBox.ShowProgress(messages[3],www,ao,(ulong)images[i].size);
				task.preview=www;ao.completed+=task.OnPreview;
			}else {
				task.OnPreview(null);
			}
		}

		public virtual void DownloadModel(DownloadTask task,JObject json) {
			if(task==null||json==null) {return;}
			//
			string it;JToken jt;
			for(int i=0,imax=formats?.Length??0;i<imax;++i) {
				it=formats[i];if(string.IsNullOrEmpty(it)) {continue;}
				jt=json.SelectToken(it);if(jt!=null) {
					string url=jt["url"]?.Value<string>();
					it=GetPath(task.path,task.uid,url);
					if(File.Exists(it)) {
						task.path=it;OnDownload(task);
					}else {
						UnityWebRequest www=UnityWebRequest.Get(url);var ao=www.SendWebRequest();
						MessageBox.ShowProgress(messages[3],www,ao,jt["size"]?.Value<ulong>()??0);
						task.model=www;ao.completed+=task.OnModel;
					}
					return;
				}
			}
		}

		public virtual void OnDownload(DownloadTask task) {
			if(task==null) {return;}
			//
			if(!IsError(task,task.model)) {
				task.path=GetPath(task.path,task.uid,task.model.url);
				//
				string str=Path.GetDirectoryName(task.path);
				if(!string.IsNullOrEmpty(str)&&!Directory.Exists(str)) {Directory.CreateDirectory(str);}
				//
				File.WriteAllBytes(task.path,task.model.downloadHandler.data);
#if CSHARP_ZLIB
				if(ext==".zip") {Unzip(path,path.Substring(0,path.Length-4));}
#endif
			}
			if(!IsError(task,task.preview)) {
				string str=Path.GetFileNameWithoutExtension(task.path)+
					Path.GetExtension(task.preview.url);
				File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(task.path),str),
					task.preview.downloadHandler.data);
			}
			//
			var tmp=current;current=task;
				task.action?.Invoke(task.path);
			current=tmp;
			//
			task.Kill();
		}

		// OAuth APIs

		protected virtual void CheckLogger() {
			bool b=m_Logger.isUserLogged();int i=0;
			if(b) {
				var p=m_Logger.getCurrentSession();
				if(p==null||!p.hasAvatar||!p.isDisplayable()) {i=1;}
				else {i=2;}
			}
			if(i!=m_Logged) {
				Debug.Log($"{m_Logged}->{i}");
				switch(i) {
					case 0:
						Logout();
					break;
					case 1:
						if(m_Logged==0) {RunTask();}
					break;
					case 2:
						if(m_Logged==0) {RunTask();}
						OnLogin();
					break;
				}
				m_Logged=i;
			}
		}

		public override void Login() {
			if(string.IsNullOrEmpty(texts[0])||string.IsNullOrEmpty(texts[1])) {return;}
			m_Logged=0;
			// Hook a request.
			int i=m_API._requests.Count;
				m_Logger.requestAccessToken(texts[0],texts[1]);
			int j=m_API._requests.Count;if(j!=i) {
				m_API._requests[j-1].setFailedCallback((TextRequestCallback)OnLogin);
			}
		}

		protected override void OnLogin() {
			var p=m_Logger.getCurrentSession();
				displayName=p?.displayName??m_DisplayName;
				avatarIcon=p?.avatar??m_AvatarIcon;
			InvokeEvent(k_Type_Login);
		}

		protected override void OnLogin(string msg) {
			JObject jo=JObject.Parse(msg);
			msg=jo.SelectToken("error")?.Value<string>()??msg;
			//
			string tmp=m_Message;
				m_Message=msg;InvokeEvent(k_Type_Error);
			m_Message=tmp;
		}

		public override void Logout() {
			m_Logged=0;
			m_Logger.logout();
			//
			base.Logout();
		}

		#endregion Methods
	}
}
