using System.IO;
using UnityEngine;
#if ENABLE_GLTF_UTILITY
using Siccity.GLTFUtility;
#elif ENABLE_GLTFAST
using GLTFast;
#else
using ImportSettings=System.Object;
#endif
namespace YouSingStudio.Holograms {
	public class GltfLoader
		:ModelLoader
	{
		#region Fields

		public ImportSettings import=new ImportSettings();
#if ENABLE_GLTFAST
		public InstantiationSettings instantiation=new InstantiationSettings();
#endif
		#endregion Fields

		#region Methods

		protected override void Start() {
			this.SetRealName();this.LoadSettings(name);
			base.Start();
		}

		public override Manifest LoadManifest(string path) {
			Manifest tmp=base.LoadManifest(path);
			if(tmp==null) {
				tmp=new Manifest{
					R=new Vector3(0.0f,180.0f,0.0f),
					S=new Vector3(float.NaN,float.NaN,float.NaN)
				};
			}
			return tmp;
		}
#if ENABLE_GLTF_UTILITY||ENABLE_GLTFAST
		protected override
#if ENABLE_GLTFAST
			async
#endif
		void InternalLoad() {
			if(!File.Exists(m_Path)) {return;}
			//
			Model m=new Model();m.path=m_Path;
			m.manifest=LoadManifest(m.path);
#if ENABLE_GLTF_UTILITY
			Importer.LoadFromFileAsync(m.path,import,(x,y)=>{
				LoadModel(m,x);
			});
#elif ENABLE_GLTFAST
			var tmp=new GltfImport();m.onDispose+=tmp.Dispose;
			string fn=Path.GetFullPath(m.path).FixPath();
			bool b=await tmp.LoadFile(fn,new System.Uri("file:///"+fn),import);if(b) {
				GameObject x=new GameObject(m.Name);
				var ins=new GameObjectInstantiator(tmp,x.transform,null,instantiation);
				b=await tmp.InstantiateMainSceneAsync(ins);if(b) {LoadModel(m,x);}
			}
#endif
		}
#endif
		#endregion Methods
	}
}