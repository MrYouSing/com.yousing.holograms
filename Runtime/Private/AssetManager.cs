// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

/* <!-- Macro.Copy File
../Internal/MonoSingleton.cs,9~39
 Macro.End -->*/
/* <!-- Macro.Replace
T,TManager
TManagerype,Type
 Macro.End -->*/
/* <!-- Macro.Patch
,Singleton
 Macro.End -->*/
using System.Collections.Generic;
using System.IO;
using UnityEngine;using YouSingStudio.Holograms;

namespace YouSingStudio.Private {
	public class AssetManager
		:MonoBehaviour
	{
		#region Fields

		[SerializeField]protected Object[] m_Assets;

		#endregion Fields

		#region Methods

		/// <summary>
		/// Load an asset from storage.<br/>
		/// - Powered by UnityEngine.AssetBundle.
		/// </summary>
		public static T Load<T>(string path) where T:Object {
			var ab=File.Exists(path)?AssetBundle.LoadFromFile(Path.GetFullPath(path)):null;
			if(ab!=null) {return ab.LoadAsset<T>(Path.GetFileNameWithoutExtension(path));}//<T>(out path,out T asset);return asset;}
			return null;
		}

		#endregion Methods
	}

	public class AssetManager<TAsset,TManager>
		:AssetManager
		where TManager:AssetManager<TAsset,TManager>
	{
// <!-- Macro.Patch Singleton
		public static bool s_InstanceCached;
		public static TManager s_Instance;
		public static TManager instance {
			get {
				if(!s_InstanceCached) {
					s_InstanceCached=true;
					//
					if(s_Instance==null) {
						s_Instance=Object.FindAnyObjectByType<TManager>();
						if(s_Instance==null) {
							s_Instance=new GameObject(typeof(TManager).FullName)
								.AddComponent<TManager>();
						}
					}
				}
				return s_Instance;
			}
			protected set{
				s_Instance=value;
				s_InstanceCached=s_Instance!=null;
			}
		}

		protected virtual void Awake() {
			if(s_Instance==null) {instance=(TManager)this;}
			else if(s_Instance!=this) {Object.Destroy(this);}
		}

		protected virtual void OnDestroy() {
			if(s_Instance==this) {instance=null;}
		}
// Macro.Patch -->

		#region Fields

		[System.NonSerialized]public Dictionary<string,TAsset> assets;
		[System.NonSerialized]protected bool m_IsInited;

		#endregion Fields

		#region Methods

		protected virtual void Init() {
			if(m_IsInited) {
				return;
			}
			m_IsInited=true;
			//
			if(assets==null) {
				assets=new Dictionary<string,TAsset>(System.StringComparer.OrdinalIgnoreCase);
			}
			for(int i=0,imax=m_Assets?.Length??0;i<imax;++i) {
				Add(m_Assets[i]);
			}
		}

		public virtual TAsset Load(string path) {
			if(!m_IsInited) {Init();}
			//
			return default;
		}

		public virtual void Add(Object asset) {
			if(!m_IsInited) {Init();}
			//
			//sset.Cast<IDictionary<string,TAsset>>();
			//=null) {assets.AddRange(d);}
			if(asset is TAsset a) {Set(asset.name,a);}
		}

		public virtual TAsset Get(string path) {
			path=path.GetFullPath();
			if(string.IsNullOrEmpty(path)) {return default;}
			//
			if(!m_IsInited) {Init();}
			//
			if(!assets.TryGetValue(path,out var tmp)||tmp==null) {
				tmp=Load(path);
				assets[path]=tmp;
			}
			return tmp;
		}

		public virtual void Set(string path,TAsset asset) {
			path=path.GetFullPath();
			if(string.IsNullOrEmpty(path)) {return;}
			//
			if(!m_IsInited) {Init();}
			//
			assets[path]=asset;
		}

		#endregion Methods
	}
}