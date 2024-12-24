using System.IO;

namespace YouSingStudio.Holograms {
	public class ObjLoader
		:ModelLoader
	{
		#region Fields
		#endregion Fields

		#region Methods
#if DUMMIESMAN_OBJ_IMPORT
		// https://assetstore.unity.com/packages/tools/modeling/runtime-obj-importer-49547
		protected override void InternalLoad() {
			if(!File.Exists(m_Path)) {return;}
			//
			var obj=new Dummiesman.OBJLoader();
			var go=obj.Load(m_Path,Path.ChangeExtension(m_Path,".mtl"));
			this.CreateModel(m_Path,go);
		}
#endif
		#endregion Methods
	}
}
