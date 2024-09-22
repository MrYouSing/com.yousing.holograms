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
					var tex=UnityExtension.NewTexture2D(1,1);
					tex.LoadImage(File.ReadAllBytes(path));return tex;
				}else {
					return AssetManager.Load<Texture>(path);
				}
			}
			return base.Load(path);
		}

		public virtual bool IsLinear(RenderTexture rt) {
			return !rt.name.StartsWith("TempBuffer");
		}

		/// <summary>
		/// <seealso cref="File.WriteAllBytes(string,byte[])"/>
		/// </summary>
		public virtual void Save(string path,RenderTexture texture) {
			if(texture==null) {return;}
			//
			int w=texture.width,h=texture.height;
			Texture2D tex=UnityExtension.NewTexture2D(w,h,IsLinear(texture));
			var tmp=texture.Begin();
				tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();
			texture.End(tmp);
			//
			tex.name=UnityExtension.s_TempTag+"/"+Path.GetFileName(path);
			Set(path,tex);
		}

		#endregion Methods
	}
}
