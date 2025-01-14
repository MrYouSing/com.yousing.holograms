using UnityEngine;
using YouSingStudio.Private;

namespace YouSingStudio.Holograms {
	public class SkyboxLoader
		:MonoBehaviour
	{
		#region Fields

		public static readonly int _Rotation=Shader.PropertyToID("_Rotation");
		public static readonly int _Exposure=Shader.PropertyToID("_Exposure");

		public Material[] materials;
		public DialogPicker picker;

		[System.NonSerialized]public int type;
		[System.NonSerialized]public System.Action onLoaded=null;

		#endregion Fields

		#region Unity Messages
		#endregion Unity Messages

		#region Methods

		public virtual void Open() {
			if(picker!=null) {
				picker.onPicked=Load;
				picker.ShowDialog();
			}
		}

		public virtual void Load(string path) {
			if(string.IsNullOrEmpty(path)) {return;}
			//
			Load(TextureManager.instance.Get(path.GetFilePath()));
		}

		public virtual void Load(Texture texture,int type=-1) {
			if(texture==null) {Load(materials[0]);return;}
			if(type<0) {
				int w=texture.width,h=texture.height;
				if(w==2.0f*h) {type=1;}
			}
			if(type<0) {Load(materials[0]);return;}
			//
			Material material=materials[type];
			material.mainTexture=texture;
			switch(type) {
				case 1:
					if(texture.mipmapCount>1) {
						Debug.LogWarning($"{texture.name} has mipmap.");
					}
					texture.wrapMode=TextureWrapMode.Clamp;
					//
					material.SetFloat(_Rotation,0.0f);
					material.SetFloat(_Exposure,1.0f);
				break;
			}
			//
			Load(material);
		}

		public virtual void Load(Material material) {
			RenderSettings.skybox=material;
			type=System.Array.IndexOf(materials,material);
			//
			onLoaded?.Invoke();
		}

		#endregion Methods
	}
}
