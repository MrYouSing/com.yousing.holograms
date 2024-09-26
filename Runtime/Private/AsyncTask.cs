// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

using System.Threading;
using UnityEngine;using YouSingStudio.Holograms;
using UnityEngine.Pool;

namespace YouSingStudio.Private {
	public class AsyncTask
		:System.IDisposable
	{
		#region Nested Types

		public class Behaviour:MonoBehaviour {
			public System.Action action=null;
			protected virtual void Update() {
				var tmp=action;action=null;tmp?.Invoke();
			}
		}

		#endregion Nested Types

		#region Fields

		public static Behaviour s_Behaviour=null;
		public static int s_ID=-1;

		public int id;
		public float delay;
		public Thread thread;
		public bool unity;

		#endregion Fields

		#region Methods

		public static Behaviour GetBehaviour() {
			if(s_Behaviour==null) {
				GameObject go=new GameObject(typeof(Behaviour).FullName);
				s_Behaviour=go.AddComponent<Behaviour>();
			}
			return s_Behaviour;
		}

		public static void OnEvent(System.Action action,bool main) {
			if(main) {GetBehaviour().action+=action;}
			else {action?.Invoke();}
		}

		public virtual void Dispose() {
			if(id>=0) {
				id=-(id+1);delay=0.0f;
				/*thread?.Abort();*/thread=null;
			}else {
				Debug.LogWarning("It is disposed.");
			}
		}

		public virtual void OnComplete() {
			Debug.Log("Complete the AsyncTask@"+id);
		}

		public virtual void OnKill() {
			Debug.Log("Kill the AsyncTask@"+(id<0?(-id-1):id));
		}

		public virtual System.Collections.IEnumerator Run() {
			int i=id;float t=Time.time+delay;
			while(Time.time<t) {yield return null;}
			//
			if(id==i) {OnComplete();}
			else {OnKill();}
		}

		public virtual Coroutine StartAsCoroutine() {
			return GetBehaviour().StartCoroutine(Run());
		}

		public virtual void Run(object arg) {bool b=(bool)arg;
			int i=id;int t=(int)(System.Environment.TickCount+delay*1000);
			while(System.Environment.TickCount<t) {}
			//
			if(id==i) {OnEvent(OnComplete,b);}
			else {OnEvent(OnKill,b);}
		}

		public virtual Thread StartAsThread(bool main=true) {
			thread=new Thread(Run);thread.Start(main);return thread;
		}

		#endregion Methods
	}
	public class AsyncTask<T>
		:AsyncTask
		where T:AsyncTask<T>,new()
	{
		#region Methods

		public static AsyncTask<T> Obtain() {
			AsyncTask<T> tmp=GenericPool<T>.Get();
			++s_ID;tmp.id=s_ID;return tmp;
		}

		public override void Dispose() {
			if(id>=0) {
				base.Dispose();
				GenericPool<AsyncTask<T>>.Release(this);
			}
		}

		public override void OnComplete() {
			base.OnComplete();Dispose();
		}

		public override void OnKill() {
			base.OnKill();Dispose();
		}

		#endregion Methods
	}
}
