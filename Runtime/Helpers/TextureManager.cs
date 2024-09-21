using System.IO;
using UnityEngine;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	public class TextureManager
		:AssetManager<Texture,TextureManager>
	{
		#region Methods

		public override Texture Load(string path) {
			path=path.GetFullPath();
			if(File.Exists(path)) {
				if(UnityExtension.IsImage(Path.GetExtension(path))) {
					var tex=new Texture2D(1,1);
					tex.LoadImage(File.ReadAllBytes(path));return tex;
				}else {
					return AssetManager.Load<Texture>(path);
				}
			}
			return base.Load(path);
		}

		/// <summary>
		/// <seealso cref="File.WriteAllBytes(string,byte[])"/>
		/// </summary>
		public virtual void Save(string path,RenderTexture texture) {
			//Texture2D tmp=TextureAPI.NewTexture2D(texture.width,texture.height);
			//texture.ToTexture2D(tmp,true);Set(path,tmp);
		}

		#endregion Methods
	}
}
