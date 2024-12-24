namespace YouSingStudio.Holograms {
	public class PlyLoader
		:ModelLoader
	{
		#region Fields
		#endregion Fields

		#region Unity Messages
#if THREE_DEE_BEAR_PLY
		protected override void Start() {
			this.AddToStage(".ply");
			//
			base.Start();
		}
#endif
		#endregion Unity Messages

		#region Methods
#if THREE_DEE_BEAR_PLY
		protected override void InternalLoad() {
			if(!File.Exists(m_Path)) {return;}
			//
			var ply=ThreeDeeBear.Models.Ply.PlyHandler.GetVerticesAndTriangles(m_Path);
			if((ply.Vertices?.Count??0)<=0) {return;}GameObject go=null;
			//
			if((ply.Triangles?.Count??0)<=0) {// PointCloud
			}else {// Mesh
				Mesh mesh=new Mesh();
				mesh.SetVertices(ply.Vertices);mesh.SetTriangles(ply.Triangles,0);
				if((ply.Colors?.Count??0)>0) {mesh.SetColors(ply.Colors);}
				mesh.RecalculateNormals();mesh.RecalculateBounds();
				//
				go=new GameObject(Path.GetFileNameWithoutExtension(m_Path));
				go.AddComponent<MeshFilter>().sharedMesh=mesh;
				go.AddComponent<MeshRenderer>().sharedMaterial=new Material(Shader.Find("Standard"));
			}
			//
			if(go==null) {return;}
			Model m=new Model();m.path=m_Path;// TODO:
			LoadModel(m,go);
		}
#endif
		#endregion Methods
	}
}
