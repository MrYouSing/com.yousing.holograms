using System.Collections.Generic;
using UnityEngine;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="LightingSettings"/>
	/// </summary>
	public class LightingDirector
		:MonoDirector
	{
		#region Nested Types

		[System.Serializable]
		public class _Light {
			public Pose pose=Pose.identity;
			public LightType type;
			public LightShadows shadow;
			public Color color;
			public Vector4 vector;

			public System.Action<Light> action;
		}

		[System.Serializable]
		public class Snapshot {
			public string name;
			public Color color;
			public Material skybox;
			public List<_Light> lights=new List<_Light>();

			public System.Action<LightingDirector> action;
		}

		#endregion Nested Types

		#region Fields

		public static LightingDirector s_Main;

		[Header("Lighting")]
		[SerializeField]protected bool m_Main;
		public new Camera camera;
		public List<Light> lights;
		public List<Snapshot> snapshots;

		#endregion Fields

		#region Unity Messages

		protected override void Awake() {
			if(m_Main) {
				s_Main=this;
				UnityExtension.SetListener<TextureType>(OnTextureTypeChanged,true);
			}
			base.Awake();
		}

		protected virtual void OnDestroy() {
			if(m_Main) {
				UnityExtension.SetListener<TextureType>(OnTextureTypeChanged,false);
				if(s_Main==this) {s_Main=null;}
			}
		}

		#endregion Unity Messages

		#region Methods

		public override int Count=>snapshots.Count;
		public override string KeyOf(int index)=>snapshots[index].name;

		public override void Set(int index) {
			if(index>=-1) {ApplySnapshot(snapshots[index>0?index:0]);}
		}

		protected virtual void OnTextureTypeChanged(TextureType type) {
			Set(nameof(TextureType)+"."+type);
		}

		public virtual _Light GetLight(string key,int index) {
			_Light l=null;Snapshot s=GetSnapshot(key,true);
			if(s!=null) {
				if(index>=0&&index<s.lights.Count) {
					l=s.lights[index];
				}else {
					l=new _Light();
					l.type=LightType.Directional;
					l.shadow=LightShadows.None;
					l.color=Color.white;
					l.vector=new Vector4(1.0f,1.0f,0.0f,0.0f);
					s.lights.Add(l);
				}
			}
			return l;
		}

		public virtual void ApplyLight(Light a,_Light b) {
			if(a==null) {return;}
			if(b==null) {a.enabled=false;return;}
			//
			a.enabled=true;
			a.type=b.type;
			a.shadows=b.shadow;
			a.transform.SetLocalPositionAndRotation(b.pose.position,b.pose.rotation);
			a.color=b.color;
			//
			a.intensity=b.vector.x;
			a.bounceIntensity=b.vector.y;
			a.range=b.vector.z;
			switch(b.type) {
#if UNITY_EDITOR
				case LightType.Area:
					a.areaSize=new Vector2(b.vector.w,b.vector.w);
				break;
#endif
				case LightType.Spot:
					a.spotAngle=b.vector.w;
				break;
			}
			//
			b.action?.Invoke(a);
		}

		public virtual Snapshot GetSnapshot(string key,bool add=false) {
			Snapshot s=null;int i=IndexOf(key);
			if(i>=0) {s=snapshots[i];}
			else if(add) {s=new Snapshot();s.name=key;AddSnapshot(s);}
			return s;
		}

		public virtual void AddSnapshot(Snapshot s) {
			if(s!=null&&snapshots.IndexOf(s)<0) {
				base.Add(s.name);
				snapshots.Add(s);
			}
		}

		public virtual void ApplySnapshot(Snapshot s) {
			if(s!=null) {
				if(camera!=null) {
					if(s.skybox!=null) {
						camera.clearFlags=CameraClearFlags.Skybox;
						RenderSettings.skybox=s.skybox;
					}else {
						camera.clearFlags=CameraClearFlags.SolidColor;
						camera.backgroundColor=s.color;
					}
				}
				int i=0,imax=lights?.Count??0,icnt=s.lights?.Count??0;
				for(;i<icnt;++i) {ApplyLight(lights[i],s.lights[i]);}
				for(;i<imax;++i) {ApplyLight(lights[i],null);}
				//
				s.action?.Invoke(this);
			}
		}

		#endregion Methods
	}
}
