using System.IO;
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class ObjLoader
		:ModelLoader
	{
		#region Fields
		#endregion Fields

		#region Methods

		public override Manifest LoadManifest(string path) {
			if(this.CreateManifest(path,out var tmp,base.LoadManifest)) {
			}
			return tmp;
		}
#if DUMMIESMAN_OBJ_IMPORT
		// https://assetstore.unity.com/packages/tools/modeling/runtime-obj-importer-49547
		protected override void InternalLoad() {
			if(!File.Exists(m_Path)) {return;}
			//
			string mtl=Path.ChangeExtension(m_Path,".mtl");
			var obj=new Dummiesman.OBJLoader();
			var go=obj.Load(m_Path,mtl);
			//
			var t=go.transform;
			this.RebaseTransform(ref t,false);
			//
			TryLoadMaterial(go,mtl);
			this.CreateModel(m_Path,go);
		}
#endif
		protected virtual void TryLoadMaterial(GameObject go,string mat) {
			if(string.IsNullOrEmpty(mat)||go==null) {return;}
			if(File.Exists(mat)) {mat=File.ReadAllText(mat);}
			//
			Shader sh=null;if(!string.IsNullOrEmpty(mat)) {
				string key="illum ";int i=mat.IndexOf(key);
				if(i>=0&&int.TryParse(mat.Substring(i+key.Length,1),out i)) {
				switch(i) {
					case 0:sh=Shader.Find("Legacy Shaders/Transparent/Cutout/Soft Edge Unlit");break;
				}}
			}
			//
			if(sh==null) {return;}
			Renderer r=go.GetComponentInChildren<Renderer>();
			if(r!=null) {r.sharedMaterial.shader=sh;}
		}

		#endregion Methods
	}
}
