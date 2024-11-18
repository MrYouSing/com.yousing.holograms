using System.IO;
using UnityEngine;

namespace YouSingStudio.Holograms {
	public class TextureManager
		:Private.AssetManager<Texture,TextureManager>
	{
		#region Fields
		#endregion Fields

		#region Methods

		public override Texture Load(string path) {
			path=path.GetFullPath();
			if(File.Exists(path)) {
				if(UnityExtension.IsImage(Path.GetExtension(path))) {
					var tex=UnityExtension.NewTexture2D(1,1);
					tex.LoadImage(File.ReadAllBytes(path));
					tex.name=path;return tex;
				}else {
					return Private.AssetManager.Load<Texture>(path);
				}
			}
			return base.Load(path);
		}

		public virtual bool IsLinear(RenderTexture rt) {
			return !rt.sRGB;
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

		public virtual void Clear() {
			if(!m_IsInited) {return;}
			//
			foreach(var it in assets.Values) {
				if(it==null) {}
				else if(it is RenderTexture rt) {rt.Free();}
				else {Texture.Destroy(it);}
			}
			assets.Clear();
		}

		public virtual string ToIconKey(string ext) {
			string key="Icon_"+ext.Substring(1);
			if(assets?.ContainsKey(key)??false) {}
			else if(UnityExtension.IsImage(ext)) {key="image_00";}
			else if(UnityExtension.IsVideo(ext)) {key="movie_00";}
			else if(UnityExtension.IsModel(ext)) {key="model_00";}
			return key;
		}

		public virtual void SetupFormats() {
			if(UnityExtension.s_ImageExtensions.Count==4) {return;}
			//Texture2D.LoadImage();
			UnityExtension.s_ImageExtensions.Clear();
			UnityExtension.s_ImageExtensions.Add(".png");
			UnityExtension.s_ImageExtensions.Add(".jpeg");
			UnityExtension.s_ImageExtensions.Add(".jpg");
			UnityExtension.s_ImageExtensions.Add(".exr");
		}

		#endregion Methods
	}
}
