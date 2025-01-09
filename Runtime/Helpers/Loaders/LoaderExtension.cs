using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace YouSingStudio.Holograms {
	using static ModelLoader;

	public static partial class LoaderExtension {
		#region Fields

		public static string s_Txt_Busying="{0} is loading {1}";

		#endregion Fields

		#region Methods

		public static bool CreateManifest(this ModelLoader thiz,string path,out Manifest manifest,System.Func<string,Manifest> func=null) {
			// Load by path.
			manifest=null;if(!string.IsNullOrEmpty(path)) {
				if(func==null&&thiz!=null) {func=thiz.LoadManifest;}
				manifest=func?.Invoke(path)??null;
			}
			// Create new one.
			if(manifest==null&&thiz!=null) {
				manifest=new Manifest{
					R=new Vector3(0.0f,180.0f,0.0f),
					S=new Vector3(float.NaN,float.NaN,float.NaN)
				};
				return true;
			}else {
				return false;
			}
		}

		public static Model CreateModel(this ModelLoader thiz,string path,GameObject prefab=null) {
			//
			var model=new Model();model.path=path;
			model.manifest=thiz.LoadManifest(path);
			//
			if(prefab!=null) {thiz.LoadModel(model,prefab);}
			return model;
		}

		public static void RebaseTransform(this ModelLoader thiz,ref Transform value,bool dummy) {
			if(value!=null) {
				if(dummy) {
					GameObject go=new GameObject(value.name+".Dummy");Transform t=go.transform;
					value.SetParent(t,false);value=t;
				}else {
					using(ListPool<Transform>.Get(out var list)) {
						foreach(Transform c in value) {if(c!=null) {c.SetParent(null,true);list.Add(c);}}
						//
						value.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
						value.localScale=Vector3.one;
						//
						for(int i=0,imax=list.Count;i<imax;++i) {list[i].SetParent(value,true);}
					}
				}
			}
		}

		public static void AddToStage(this ModelLoader thiz,params string[] extensions) {
			if(thiz!=null) {
				int i,imax=extensions?.Length??0;
				if(imax>0) {
					for(i=0;i<imax;++i) {UnityExtension.s_ModelExtensions.Add(extensions[i]);}
					StageDirector sd=thiz.GetComponentInParent<StageDirector>();
					if(sd!=null) {
						//
						string key="Open Model";
						StringBuilder sb=new StringBuilder(key).Append(' ').Append(extensions[0]);
						for(i=1;i<imax;++i) {sb.Append(';').Append(key).Append(' ').Append(extensions[i]);}
						//
						var s=new StageDirector.Stage();key="$"+key;
						s.name=sb.ToString();s.SetActive(false);
						s.onShow=new UnityEvent();s.onShow.AddListener(()=>sd.Activate(key));
						s.onHide=new UnityEvent();s.onHide.AddListener(()=>sd.Deactivate(key));
						s.onOpen=new UnityEvent<string>();s.onOpen.AddListener(thiz.Load);
						s.actors=new List<GameObject>{thiz.gameObject};sd.Add(s);
					}
					UIGalleryController gc=Object.FindAnyObjectByType<UIGalleryController>();
					if(gc!=null) {
						gc.filters.AddRange(extensions);
					}
				}
			}
		}

		public static void ShowBusyDialog(this ModelLoader thiz,string path) {
			MessageBox.ShowInfo(
				"Prefabs/Model_Busying",
				string.Format(s_Txt_Busying,thiz.name,Path.GetFileName(path)),
				null,()=>{thiz.gameObject.SetActive(false);MessageBox.Clear();}
			);
		}

		public static void HideBusyDialog(this ModelLoader thiz) {
			MessageBox.Clear();
		}

		#endregion Methods
	}
}