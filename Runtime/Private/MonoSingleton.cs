// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

using UnityEngine;using YouSingStudio.Holograms;

namespace YouSingStudio.Private {
	public class MonoSingleton<T>
		:MonoBehaviour
		where T:MonoSingleton<T>
	{
		#region Singleton

		public static bool s_InstanceCached;
		public static T s_Instance;
		public static T instance {
			get {
				if(!s_InstanceCached) {
					s_InstanceCached=true;
					//
					if(s_Instance==null) {
						s_Instance=Object.FindAnyObjectByType<T>();
						if(s_Instance==null) {
							s_Instance=new GameObject(typeof(T).FullName)
								.AddComponent<T>();
						}
					}
				}
				return s_Instance;
			}
			protected set{
				s_Instance=value;
				s_InstanceCached=s_Instance!=null;
			}
		}

		protected virtual void Awake() {
			if(s_Instance==null) {instance=(T)this;}
			else if(s_Instance!=this) {Object.Destroy(this);}
		}

		protected virtual void OnDestroy() {
			if(s_Instance==this) {instance=null;}
		}

		#endregion Singleton
	}
}
