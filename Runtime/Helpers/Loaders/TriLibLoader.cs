using System.IO;
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class TriLibLoader
		:ModelLoader
	{
		#region Fields

		public string[] extensions;
#if TRI_LIB_CORE
		public TriLibCore.AssetLoaderOptions options;
#endif
		#endregion Fields

		#region Unity Messages
#if TRI_LIB_CORE
		protected override void Start() {
			this.AddToStage(extensions);
			//
			base.Start();
		}
#endif
		#endregion Unity Messages

		#region Methods

		public override Manifest LoadManifest(string path) {
			if(this.CreateManifest(path,out var m,base.LoadManifest)) {
			}
			return m;
		}
#if TRI_LIB_CORE
		protected override void InternalLoad() {
			if(!File.Exists(m_Path)) {return;}
			//
			Model m=new Model();
			var ctx=TriLibCore.AssetLoader.LoadModelFromFile(
				m_Path,
				//
				haltTask:true,
				isZipFile:Path.GetExtension(m_Path)!=".zip",
				assetLoaderOptions:options,
				//
				onLoad:LoadModel,
				onMaterialsLoad:LoadMaterials
			);
			/*
			string txt="Loading "+Path.GetFileNameWithoutExtension(m_Path);
			MessageBox.ShowProgress("Prefabs/Model_Loading",txt,()=>ctx.Completed?1.0f:ctx.LoadingProgress);
			*/
			this.ShowBusyDialog(m_Path);
		}

		protected virtual void LoadModel(TriLibCore.AssetLoaderContext ctx) {
			this.HideBusyDialog();
			//
			GameObject go=ctx.RootGameObject;
			Animation a=go.GetComponent<Animation>();
			if(a!=null) {
				a.playAutomatically=true;
				a.wrapMode=WrapMode.Loop;
				//
				var e=a.GetEnumerator();e.Reset();
				if(e.MoveNext()) {a.clip=(e.Current as AnimationState).clip;}
			}
			//
			this.CreateModel(ctx.Filename,go);
		}

		protected virtual void LoadMaterials(TriLibCore.AssetLoaderContext ctx) {
		}
#endif
		#endregion Methods
	}
}
