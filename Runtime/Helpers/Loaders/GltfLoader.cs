﻿using System.IO;
using UnityEngine;
using UnityEngine.Pool;
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
#if ENABLE_GLTF_UTILITY
#elif ENABLE_GLTFAST
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
		protected override async void InternalLoad() {
			if(!File.Exists(m_Path)) {return;}
			//
			Model m=new Model();m.path=m_Path;
			m.manifest=LoadManifest(m.path);
#if ENABLE_GLTF_UTILITY
			string txt="Loading "+Path.GetFileNameWithoutExtension(m_Path);
			MessageBox.ShowInfo("Prefab/Model_Loading",txt,null,MessageBox.Clear);
			Importer.LoadFromFileAsync(m.path,import,(x,c)=>{
				MessageBox.Clear();
				LoadGltfModel(m,x,c);
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
#if ENABLE_GLTF_UTILITY
		protected virtual void LoadGltfModel(Model m,GameObject x,AnimationClip[] c) {
			// Fix transform.
			Transform t=x.transform;Quaternion q=t.rotation;
			if(q!=Quaternion.identity) {
				q=q*Quaternion.AngleAxis(180.0f,Vector3.right);
				m.manifest.R=(q*Quaternion.Euler(m.manifest.R)).eulerAngles;
			}
			// Import animations.
			int i=0,imax=c?.Length??0;if(imax>0) {
				var a=x.AddComponent<Animation>();AnimationClip it;string key;// Rename clips.
				for(;i<imax;++i) {it=c[i];key=$"{i}:{it.name}";it.name=key;a.AddClip(it,key);}
				a.wrapMode=WrapMode.Loop;a.clip=c[0];
			}
			// Disable unused components
			using(ListPool<Camera>.Get(out var list)) {
				x.GetComponentsInChildren(true,list);
				for(i=0,imax=list.Count;i<imax;++i) {list[i].enabled=false;}
			}
			//
			LoadModel(m,x);
		}
#endif
		#endregion Methods
	}
}