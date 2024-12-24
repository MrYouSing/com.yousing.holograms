using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class ModelLoader
		:MonoBehaviour
	{
		#region Nested Types

		public class Manifest {
			public string name;
			public Vector3 T;
			public Vector3 R;
			public Vector3 S;
			//
			public string style;// PBR or Toon
			public string json;
		}

		public class Model:System.IDisposable {
			public string path;
			public GameObject prefab;
			public Manifest manifest;
			public System.Action onDispose=null;

			public virtual void Dispose() {
				if(prefab!=null) {GameObject.Destroy(prefab);}
				onDispose?.Invoke();
				//
				path=null;prefab=null;
				manifest=null;onDispose=null;
			}

			public virtual string Name=>!string.IsNullOrEmpty(manifest?.name)
				?manifest.name:Path.GetFileNameWithoutExtension(path);
		}

		#endregion Nested Types

		#region Fields

		public static Transform s_Hidden;
		public static Dictionary<string,Model> s_Models=new Dictionary<string,Model>(System.StringComparer.OrdinalIgnoreCase);

		public Transform container;
		[SerializeField]protected string m_Path;
		[System.NonSerialized]public System.Action onUnload=null;

		[System.NonSerialized]protected Manifest m_Manifest;
		[System.NonSerialized]protected GameObject m_Actor;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(!string.IsNullOrEmpty(m_Path)) {Load(m_Path);}
		}

		#endregion Unity Messages

		#region Methods

		public static void Hide(GameObject go) {
			if(go!=null) {
				Transform t=go.transform;
				if(s_Hidden==null) {
					go=new GameObject(nameof(ModelLoader)+".Hidden");go.SetActive(false);
					GameObject.DontDestroyOnLoad(go);s_Hidden=go.transform;
				}
				t.SetParent(s_Hidden,false);
			}
		}

		public virtual void Load(string path) {
			if(m_Actor!=null) {Unload();}
			m_Path=path;if(string.IsNullOrEmpty(m_Path)||!didStart) {return;}
			//
			if(s_Models.TryGetValue(m_Path.GetFilePath(),out var tmp)&&tmp!=null) {
				Load(tmp.prefab,tmp.manifest);
			}else {
				InternalLoad();
			}
		}

		public virtual void Load(GameObject model,Manifest manifest=null) {
			if(m_Actor!=null) {Unload();}
			if(!isActiveAndEnabled||model==null) {return;}
			m_Manifest=manifest;
			//
			m_Actor=GameObject.Instantiate(model);m_Actor.name=model.name;
			Transform t=m_Actor.transform;t.SetParent(container,false);
			if(m_Manifest!=null) {
				if(float.IsNaN(m_Manifest.S.x)) {FitStage(m_Manifest,m_Actor);}
				//
				t.SetLocalPositionAndRotation(m_Manifest.T,Quaternion.Euler(m_Manifest.R));
				Vector3 s=m_Manifest.S.sqrMagnitude!=0.0f?m_Manifest.S:Vector3.one;
				if(s.y*s.z==0.0f) {s.y=s.z=s.x;}t.localScale=s;
			}
			//
			if(isActiveAndEnabled) {this.InvokeEvent();}
		}

		public virtual void Unload() {
			if(m_Actor!=null) {GameObject.Destroy(m_Actor);}
			onUnload?.Invoke();
			//
			m_Manifest=null;m_Actor=null;
		}

		public virtual Manifest LoadManifest(string path) {
			// Get the manifest text.
			string s=Path.ChangeExtension(path,".json");
			if(File.Exists(s)) {
				s=File.ReadAllText(s);
			}else {// TODO: Load it from other sources????
				s=null;
			}
			if(!string.IsNullOrEmpty(s)) {
				JsonUtility.FromJsonOverwrite(s,this);
				return JsonUtility.FromJson<Manifest>(s);
			}
			return null;
		}

		public virtual void LoadModel(Model model,GameObject prefab) {
			if(model==null||prefab==null) {return;}
			//
			if(prefab.scene.IsValid()) {Hide(prefab);}model.prefab=prefab;
			if(model.manifest!=null) {
			}
			s_Models[model.path.GetFilePath()]=model;Load(model.prefab,model.manifest);
		}

		protected virtual void InternalLoad() {
			if(File.Exists(m_Path)) {
				AssetBundle ab=AssetBundle.LoadFromFile(m_Path);
				if(ab!=null) {
					Model m=new Model();m.path=m_Path;
					m.onDispose+=()=>ab.Unload(true);
					// Get the manifest text.
					string s=Path.ChangeExtension(m.path,".json");
					s=File.Exists(s)?File.ReadAllText(s):null;
					if(string.IsNullOrEmpty(s)) {
						TextAsset ta=ab.LoadAsset<TextAsset>("manifest.json");
						if(ta!=null) {s=ta.text;}
					}
					//
					m.manifest=!string.IsNullOrEmpty(s)?JsonUtility.FromJson<Manifest>(s):null;
					LoadModel(m,ab.LoadAsset<GameObject>(m.Name));
				}
			}
		}

		public virtual GameObject GetActor()=>m_Actor;
		public virtual Manifest GetManifest()=>m_Manifest;

		//

		protected virtual void FitStage(Manifest manifest,GameObject actor) {
			Transform t=actor.transform;t.rotation=Quaternion.Euler(manifest.R);
			Bounds a=actor.GetBounds();manifest.T=-a.center;
			if(MonoApplication.s_Instance!=null) {
				Bounds b=MonoApplication.s_Instance.GetBounds(container.position);
				if(b.size.sqrMagnitude!=0.0f) {
					Vector3 p=t.InverseTransformPoint(a.center);
					Vector3 s=a.size;int n=0;
					if(true) {// Clamp : Inside
						if(s.y>s.x) {n=1;}
						if(s.z>s.y) {n=2;}
					}else {// Free : Outside
						if(s.y<s.x) {n=1;}
						if(s.z<s.y) {n=2;}
					}
					manifest.S=Vector3.one*(b.size[n]/s[n]);
					manifest.T=b.center-Matrix4x4.TRS(t.position,t.rotation,manifest.S).MultiplyPoint3x4(p);
				}
			}
		}

		#endregion Methods
	}
}
