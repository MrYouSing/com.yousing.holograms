using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using YouSingStudio.Private;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Holograms {
	public class UIGalleryController
		:MonoBehaviour
	{
		#region Fields

		[Header("Gallery")]
		public Transform container;
		public GameObject prefab;
		public int preview;
		public string[] filters=new string[]{
			// Image
			".jpg",
			".jpeg",
			".png",
			// Video https://docs.lookingglassfactory.com/keyconcepts/quilts/quilt-video-encoding
			".mp4",
			".webm",
		};
		public List<string> paths=new List<string>();
		[Header("Controller")]
		public MediaPlayer player;
		public Key[] keys=new Key[4];

		[System.NonSerialized]protected int m_Index;
		[System.NonSerialized]protected List<string> m_Paths;
		[System.NonSerialized]protected List<GameObject> m_Views=new List<GameObject>();

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			this.LoadSettings(name+".json");
			//
			var sm=ShortcutManager.instance;int i=0;
			sm.Add(name+".Refresh",Refresh,keys[i]);++i;
			sm.Add(name+".Play",Play,keys[i]);++i;
			sm.Add(name+".Open",Open,keys[i]);++i;
			sm.Add(name+".Prev",Prev,keys[i]);++i;
			sm.Add(name+".Next",Next,keys[i]);++i;
			//
			Refresh();Set(0);
		}

		#endregion Unity Messages

		#region Methods

		public virtual bool CanPlay(string path) {
			if(!string.IsNullOrEmpty(path)) {for(int i=0,imax=filters?.Length??0;i<imax;++i) {
				if(path.EndsWith(filters[i],System.StringComparison.OrdinalIgnoreCase)) {return true;}
			}}
			return false;
		}

		public virtual void Refresh() {
			m_Paths=paths.UnpackPaths(CanPlay,m_Paths);
			m_Views.Render(m_Paths,RenderView,CreateView);
		}

		public virtual void Set(int index) {
			int len=m_Paths?.Count??0;if(len<=0) {return;}
			m_Index=(index+len)%len;
			//
			if(player!=null) {player.Play(m_Paths[index]);}
		}

		public virtual void Play() {
			Set(m_Index);
		}

		public virtual void Open() {
			// TODO: Dialog APIs
			throw new System.NotImplementedException();
		}

		public virtual void Prev() {
			Set(m_Index-1);
		}

		public virtual void Next() {
			Set(m_Index+1);
		}

		protected virtual void LoadIcon(RawImage image,string path) {
			string ext=Path.GetExtension(path);
			if(UnityExtension.IsImage(ext)) {ext="Icon/Image";}
			else if(UnityExtension.IsImage(ext)) {ext="Icon/Video";}
			else {ext=null;}
			image.texture=TextureManager.instance.Get(ext);
			//
			if(preview>0) {
				throw new System.NotImplementedException();
				if(true) {// TODO: Crop a thumbnail.
					Vector2 count=path.ParseQuilt();
					int size=Mathf.ClosestPowerOfTwo((int)Mathf.Min(preview*count.x,preview*count.y));
				}else {
				}
			}
		}

		protected virtual GameObject CreateView() {
			GameObject go=GameObject.Instantiate(prefab);
			go.transform.SetParent(container,false);
			//
			int i=m_Views.Count;
			Button b=go.GetComponentInChildren<Button>();
			if(b!=null) {b.onClick.AddListener(()=>Set(i));}
			return go;
		}

		protected virtual void RenderView(GameObject view,string path) {
			if(view==null) {return;}
			if(string.IsNullOrEmpty(path)) {view.SetActive(false);return;}
			//
			Text txt=view.GetComponentInChildren<Text>();
			if(txt!=null) {
				txt.text=Path.GetFileName(path);
			}
			//
			RawImage img=view.GetComponentInChildren<RawImage>();
			if(img!=null) {
				LoadIcon(img,path);
			}
			//
			view.SetActive(true);
		}

		#endregion Methods
	}
}
