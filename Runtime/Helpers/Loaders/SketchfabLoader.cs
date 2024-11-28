using Sketchfab;
using System.IO;
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class SketchfabLoader
		:GltfLoader
	{
		#region Fields

		[System.NonSerialized]protected string m_Uid;

		#endregion Fields

		#region Methods

		protected override void InternalLoad() {
			m_Uid=null;// TODO: BeginUid
			//
			var sdk=SketchfabSdk.instance;
			if(sdk!=null&&sdk.current!=null) {m_Uid=sdk.current.uid;}
			base.InternalLoad();
		}

		public override Manifest LoadManifest(string path) {
			var tmp=base.LoadManifest(path);
			if(!string.IsNullOrEmpty(m_Uid)&&tmp!=null) {
			var sdk=SketchfabSdk.instance;if(sdk!=null) {
			var info=sdk.GetInfo(m_Uid);if(info!=null) {
				tmp.json=info.ToString();
				//
				File.WriteAllText(Path.ChangeExtension(path,".json"),JsonUtility.ToJson(tmp));
			}}}
			return tmp;
		}

		public override void LoadModel(Model model,GameObject prefab) {
			if(model!=null&&!string.IsNullOrEmpty(m_Uid)) {s_Models[m_Uid]=model;}
			base.LoadModel(model,prefab);
			//
			m_Uid=null;// TODO: EndUid
		}

		#endregion Methods
	}
}