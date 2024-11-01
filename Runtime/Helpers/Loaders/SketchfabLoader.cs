using UnityEngine;

namespace YouSingStudio.Holograms {
	[System.Obsolete("NotImplemented Loader")]
	public class SketchfabLoader
		:GltfLoader
	{
		#region Fields

		[Header("Sketchfab")]
		public string email;
		public string password;

		#endregion Fields

		#region Unity Messages

		protected override void Start() {
			this.LoadSettings(name);
#if ENABLE_SKETCHFAB_API
			if(SketchfabAPI.Authorized) {OnAccess(null);}
			else {SketchfabAPI.GetAccessToken(email,password,OnAccess);}
#endif
		}

		#endregion Unity Messages

		#region Methods
#if ENABLE_SKETCHFAB_API
		protected virtual void OnAccess(SketchfabResponse<SketchfabAccessToken> r) {
			if(r!=null) {
				if(r.Success) {SketchfabAPI.AuthorizeWithAccessToken(r.Object);}
				else {return;}
			}
			if(!string.IsNullOrEmpty(m_Path)) {Load(m_Path);}
		}

		protected override void InternalLoad() {
			SketchfabAPI.GetModel(path,(x)=>{
				SketchfabModelImporter.Import(x.Object,(obj) => {
					if(obj!=null) {Instantiate(obj);}
				},true);
			},true);
		}
#endif
		#endregion Methods
	}
}