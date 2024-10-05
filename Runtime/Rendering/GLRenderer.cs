using UnityEngine;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// A helper for GL functions.
	/// </summary>
	public class GLRenderer
		:MonoBehaviour
	{
		#region Fields

		public Material material;
		public Mesh mesh;
		public Vector3[] vertices;
		public Vector2[] uv;
		public int[] triangles;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(mesh!=null) {
				var tmp=mesh;mesh=null;Set(tmp);
			}
		}

		protected virtual void OnPostRender() {
			Render();
		}

		#endregion Unity Messages

		#region Methods

		public virtual void Clear() {
			GL.Clear(true,true,Color.clear);
		}

		public virtual void Render() {
			Clear();
			GL.PushMatrix();
				GL.LoadOrtho();
				GL.Begin(GL.TRIANGLES);
					if(material!=null) {material.SetPass(0);}
					for(int i=0,imax=triangles?.Length??0,j;i<imax;++i) {
						j=triangles[i];GL.TexCoord(uv[j]);GL.Vertex(vertices[j]);
					}
				GL.End();
			GL.PopMatrix();
		}

		public virtual void Set(Mesh value) {
			if(value!=mesh) {
				mesh=value;
				//
				if(mesh!=null) {
					vertices=mesh.vertices;
					uv=mesh.uv;
					triangles=mesh.triangles;
				}else {
					vertices=null;
					uv=null;
					triangles=null;
				}
			}
		}

		//

		public virtual void Render(RenderTexture rt) {
			var tmp=rt.Begin();
				Render();
			rt.End(tmp);
		}

		#endregion Methods
	}
}
