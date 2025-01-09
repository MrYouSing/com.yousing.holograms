using System.IO;
using UnityEngine;
using UnityEngine.Pool;

namespace YouSingStudio.Holograms.Samples {
	public class YgoMateLoader
		:ModelLoader
	{
		#region Nested Types

		public class Listener
			:MonoBehaviour
		{
			public YgoMateLoader context;

			public virtual void PlayAnimationEventSe(string key) {
				if(context!=null) {context.PlaySE(key);}
			}
		}

		#endregion Nested Types

		#region Fields
		#endregion Fields

		#region Unity Messages

		protected override System.Collections.IEnumerator Start() {
			this.AddToStage(".ygomate");
			yield return base.Start();
		}

		protected virtual void OnDisable() {
			RenderingExtension.SetRenderPipeline(-1);
		}

		#endregion Unity Messages

		#region Methods

		public override Manifest LoadManifest(string path) {
			if(this.CreateManifest(path,out var tmp,base.LoadManifest)) {
			}
			return tmp;
		}

		protected override void InternalLoad() {
			if(!File.Exists(m_Path)) {return;}
			//
			AssetBundle ab=AssetBundle.LoadFromFile(m_Path);
			GameObject go=ab.LoadAsset<GameObject>(ab.GetAllAssetNames()[0]);
			if(go==null) {return;}
			//
			var a=go.GetComponentInChildren<Animator>();
			if(a!=null) {var l=a.gameObject.AddComponent<Listener>();l.context=this;}
			using(ListPool<ParticleSystem>.Get(out var ps)) {
				go.GetComponentsInChildren<ParticleSystem>(true,ps);
				for(int i=0,imax=ps.Count;i<imax;++i) {
					var tmp=ps[i].main;tmp.scalingMode=ParticleSystemScalingMode.Hierarchy;
				}
			}
			//
			this.CreateModel(m_Path,go);
		}

		public override void Load(GameObject model,Manifest manifest=null) {
			if(model!=null) {RenderingExtension.SetRenderPipeline(1);}
			//
			base.Load(model,manifest);
		}

		public virtual void PlaySE(string key) {
		}

		#endregion Methods
	}
}
