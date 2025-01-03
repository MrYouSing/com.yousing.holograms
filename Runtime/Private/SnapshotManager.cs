// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

/* <!-- Macro.Table Item
Scene,Load,null,scenes,m_Scene.path=key;,
Snapshot,Select,,scene.snapshots,m_Name=key;,
 Macro.End --> */
/* <!-- Macro.Call  Item
		public virtual {0} New{0}()=>new {0}({2});
		public virtual void {1}{0}(string key)=>{1}{0}(IndexOf{0}(key));

		public virtual void Access{0}(int index) {{
			if(index>=0&&index<{3}.Count) {{{1}{0}(index);}}
			else {{Add{0}();}}
		}}

		public virtual void Access{0}(string key) {{
			int index=IndexOf{0}(key);
			if(index>=0) {{{1}{0}(index);}}
			else {{{4}Add{0}();}}
		}}

 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */

/* <!-- Macro.Table BaseTypes
int,Int,
float,Float,
string,String,
Vector4,Vector,
 Macro.End --> */
/* <!-- Macro.Table UnityTypes
 Macro.End --> */

/* <!-- Macro.Call  BaseTypes
			public Dictionary<string,{0}> map{1};

			public virtual {0} Get{1}(string key,{0} value) {{
				if(map{1}!=null&&map{1}.TryGetValue(key,out var tmp)) {{return tmp;}}
				return value;
			}}

			public virtual void Set{1}(string key,{0} value) {{
				if(map{1}==null) {{map{1}=new Dictionary<string,{0}>();}}
				map{1}[key]=value;
			}}

 Macro.End --> */
/* <!-- Macro.Patch
,Snapshot
 Macro.End --> */
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;using YouSingStudio.Holograms;

namespace YouSingStudio.Private {
	/// <summary>
	/// <seealso cref="PlayerPrefs"/><br/>
	/// <seealso cref="UnityEngine.Audio.AudioMixerSnapshot"/>
	/// </summary>
	public class SnapshotManager
		:MonoBehaviour
	{
// <!-- Macro.Patch AutoGen
		public virtual Scene NewScene()=>new Scene(null);
		public virtual void LoadScene(string key)=>LoadScene(IndexOfScene(key));

		public virtual void AccessScene(int index) {
			if(index>=0&&index<scenes.Count) {LoadScene(index);}
			else {AddScene();}
		}

		public virtual void AccessScene(string key) {
			int index=IndexOfScene(key);
			if(index>=0) {LoadScene(index);}
			else {m_Scene.path=key;AddScene();}
		}

		public virtual Snapshot NewSnapshot()=>new Snapshot();
		public virtual void SelectSnapshot(string key)=>SelectSnapshot(IndexOfSnapshot(key));

		public virtual void AccessSnapshot(int index) {
			if(index>=0&&index<scene.snapshots.Count) {SelectSnapshot(index);}
			else {AddSnapshot();}
		}

		public virtual void AccessSnapshot(string key) {
			int index=IndexOfSnapshot(key);
			if(index>=0) {SelectSnapshot(index);}
			else {m_Name=key;AddSnapshot();}
		}

// Macro.Patch -->
		#region Nested Types

		public interface IActor {
			string name{get;}
			void Save(Snapshot value);
			void Load(Snapshot value);
		}

		[System.Serializable]
		public class Snapshot {
			public string name;
			[System.NonSerialized]public Scene context;
// <!-- Macro.Patch Snapshot
			public Dictionary<string,int> mapInt;

			public virtual int GetInt(string key,int value) {
				if(mapInt!=null&&mapInt.TryGetValue(key,out var tmp)) {return tmp;}
				return value;
			}

			public virtual void SetInt(string key,int value) {
				if(mapInt==null) {mapInt=new Dictionary<string,int>();}
				mapInt[key]=value;
			}

			public Dictionary<string,float> mapFloat;

			public virtual float GetFloat(string key,float value) {
				if(mapFloat!=null&&mapFloat.TryGetValue(key,out var tmp)) {return tmp;}
				return value;
			}

			public virtual void SetFloat(string key,float value) {
				if(mapFloat==null) {mapFloat=new Dictionary<string,float>();}
				mapFloat[key]=value;
			}

			public Dictionary<string,string> mapString;

			public virtual string GetString(string key,string value) {
				if(mapString!=null&&mapString.TryGetValue(key,out var tmp)) {return tmp;}
				return value;
			}

			public virtual void SetString(string key,string value) {
				if(mapString==null) {mapString=new Dictionary<string,string>();}
				mapString[key]=value;
			}

			public Dictionary<string,Vector4> mapVector;

			public virtual Vector4 GetVector(string key,Vector4 value) {
				if(mapVector!=null&&mapVector.TryGetValue(key,out var tmp)) {return tmp;}
				return value;
			}

			public virtual void SetVector(string key,Vector4 value) {
				if(mapVector==null) {mapVector=new Dictionary<string,Vector4>();}
				mapVector[key]=value;
			}

// Macro.Patch -->
		}

		[System.Serializable]
		public class Scene {
			public string path;
			public int index;
			public List<Snapshot> snapshots;
			[System.NonSerialized]public SnapshotManager context;

			public Scene() {
			}

			public Scene(string path) {
				this.path=path;index=0;
				snapshots=new List<Snapshot>();
			}

			public virtual int IndexOf(string key) {
				for(int i=0,imax=snapshots?.Count??0;i<imax;++i) {
					if(string.Equals(snapshots[i].name,key,context.comparison)) {return i;}
				}
				return -1;
			}
		}

		public class VectorMapConverter:JsonConverter<Dictionary<string,Vector4>> {
			public override Dictionary<string,Vector4> ReadJson(JsonReader reader,System.Type objectType,Dictionary<string,Vector4> existingValue,bool hasExistingValue,JsonSerializer serializer) {
				return existingValue;
			}

			public override void WriteJson(JsonWriter writer,Dictionary<string,Vector4> value,JsonSerializer serializer) {
				if(value==null) {writer.WriteNull();return;}
				//
				writer.WriteStartObject();
					foreach(var it in value) {
						writer.WritePropertyName(it.Key);
						writer.WriteRawValue(JsonUtility.ToJson(it.Value,false));
					}
				writer.WriteEndObject();
			}
		}

		[System.Serializable]
		public class Document {
			public List<Scene> scenes;
		}

		#endregion Nested Types

		#region Fields

		public static JsonConverter[] s_JsonConverters=null;

		[SerializeField]protected string m_Path;
		[SerializeField]protected Object[] m_Actors;

		[System.NonSerialized]public System.StringComparison comparison=System.StringComparison.OrdinalIgnoreCase;
		[System.NonSerialized]public List<Scene> scenes;
		[System.NonSerialized]public List<IActor> actors;
		[System.NonSerialized]public Scene scene;
		[System.NonSerialized]public Snapshot snapshot;

		[System.NonSerialized]protected Scene m_Scene;
		[System.NonSerialized]protected string m_Name;

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			LoadDocument(m_Path);
			if(actors==null) {
				int i=0,imax=m_Actors?.Length??0;
				actors=new List<IActor>();
				IActor it;for(;i<imax;++i) {
					it=m_Actors[i] as IActor;
					if(it!=null) {actors.Add(it);}
				}
			}
			if(m_Scene==null) {m_Scene=NewScene();}
			if(scene==null) {scene=m_Scene;}
			if(scenes==null) {scenes=new List<Scene>();}
		}

		protected virtual void OnDestroy() {
			SaveDocument(m_Path);
		}

		#endregion Unity Messages

		#region Methods

		// Document APIs

		public virtual void LoadDocument(string path) {
			m_Path=path;
			if(!string.IsNullOrEmpty(m_Path)&&File.Exists(m_Path)) {
				var tmp=JsonConvert.DeserializeObject<Document>(File.ReadAllText(m_Path));
				scenes=tmp.scenes;
			}
		}

		public virtual void SaveDocument(string path,bool force=false) {
			m_Path=path;
			if(!string.IsNullOrEmpty(m_Path)) {
				if(s_JsonConverters==null) {s_JsonConverters=new JsonConverter[]{new VectorMapConverter()};}
				var tmp=new Document();tmp.scenes=scenes;
				string text=JsonConvert.SerializeObject(tmp,Formatting.Indented,s_JsonConverters);
				if(!force&&File.Exists(m_Path)&&string.Equals(File.ReadAllText(m_Path),text)) {return;}
				File.WriteAllText(m_Path,text);
			}
		}

		public virtual string GetDocument()=>m_Path;

		// Scene APIs

		public virtual int IndexOfScene(string key) {
			for(int i=0,imax=scenes?.Count??0;i<imax;++i) {
				if(string.Equals(scenes[i].path,key,comparison)) {return i;}
			}
			return -1;
		}

		public virtual void LoadScene(int index) {
			Scene s=index>=0?scenes[index]:null;
			if(s!=scene) {OnSceneLoaded(s);}
		}

		public virtual void AddScene() {
			bool b=m_Scene!=scene;if(b) {scene=m_Scene;}
			//
			scenes.Add(scene);scene.context=this;m_Scene=NewScene();
			//
			if(b) {scene.index=-1;OnSceneLoaded(scene);}
		}

		public virtual void ResetScene() {
			if(scene!=m_Scene&&scenes.Remove(scene)) {OnSceneLoaded(null);}
		}

		// Snapshot API

		public virtual int IndexOfSnapshot(string key) {
			return scene?.IndexOf(key)??-1;
		}

		public virtual void SelectSnapshot(int index) {
			if(scene.index!=index) {OnSnapshotSelected(index);}
		}

		public virtual void SaveSnapshot() {
			if(scene.snapshots.Count<=0) {AddSnapshot();return;}
			//
			for(int i=0,imax=actors?.Count??0;i<imax;++i) {
				actors[i]?.Save(snapshot);
			}
		}

		public virtual void LoadSnapshot() {
			//
			for(int i=0,imax=actors?.Count??0;i<imax;++i) {
				actors[i]?.Load(snapshot);
			}
		}

		public virtual void AddSnapshot() {
			// New Scene?
			int cnt=scene.snapshots.Count;
			if(scene==m_Scene) {AddScene();}
			// New Snapshot.
			var tmp=NewSnapshot();
			if(string.IsNullOrEmpty(tmp.name)) {
				tmp.name=!string.IsNullOrEmpty(m_Name)?m_Name:("Snapshot ("+cnt+")");
			}
			m_Name=null;
			// Apply Snapshot.
			scene.snapshots.Add(tmp);tmp.context=scene;
			snapshot=tmp;SaveSnapshot();
			scene.index=cnt;OnSceneLoaded(scene);
		}

		public virtual void RemoveSnapshot() {
			int cnt=scene.snapshots.Count;
			if(cnt<=0) {return;}
			//
			scene.snapshots.RemoveAt(scene.index);
			scene.index=Mathf.Min(scene.index,cnt-2);OnSceneLoaded(scene);
		}

		// Events

		protected virtual void OnSceneLoaded(Scene value) {
			if(value==null) {
				scene=m_Scene;
				OnSnapshotSelected(-1);
			}else {
				scene=value;
				OnSnapshotSelected(scene.index);
			}
		}

		protected virtual void OnSnapshotSelected(int index) {
			scene.index=index;
			if(scene.index<0) {
				snapshot=null;
			}else {
				snapshot=scene.snapshots[scene.index];LoadSnapshot();
			}
		}

		#endregion Methods
	}
}
