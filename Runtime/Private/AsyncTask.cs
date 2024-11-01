// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

/* <!-- Macro.Table Events
Execute,
Complete,
Kill,
 Macro.End --> */

/* <!-- Macro.Call  Events
				tmp.on{0}=on{0};
 Macro.End --> */
/* <!-- Macro.Patch
,Obtain
 Macro.End --> */
 
/* <!-- Macro.Call  Events
		public System.Action on{0};
		protected override void On{0}() {{on{0}?.Invoke();base.On{0}();}}

 Macro.End --> */
/* <!-- Macro.Copy
		protected override void Reset() {
			base.Reset();
 Macro.End --> */
/* <!-- Macro.Call  Events
			on{0}=null;
 Macro.End --> */
/* <!-- Macro.Copy
		}

 Macro.End --> */
/* <!-- Macro.Patch
,_AsyncTask
 Macro.End --> */
#if DEBUG
#define _DEBUG
#endif
using System.Threading;
using UnityEngine;using YouSingStudio.Holograms;
using UnityEngine.Pool;

namespace YouSingStudio.Private {
	/// <summary>
	/// <seealso href="https://developer.android.google.cn/reference/android/os/AsyncTask"/>
	/// </summary>
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
		public static int s_ID=0;

		public int id=-1;
		public float delay;
		public Thread thread;

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

		public static AsyncTask Obtain(float delay,System.Action onExecute=null,System.Action onComplete=null,System.Action onKill=null) {
			var tmp=GenericPool<_AsyncTask>.Get();
				tmp.delay=delay;
// <!-- Macro.Patch Obtain
				tmp.onExecute=onExecute;
				tmp.onComplete=onComplete;
				tmp.onKill=onKill;
// Macro.Patch -->
			return tmp;
		}

		public AsyncTask() {
			++s_ID;id=s_ID;// TODO: <0:Disposed,0:Ready to be killed,[1,id]:Valid instance.
		}

		protected virtual void Reset() {
			id=-(id+1);delay=0.0f;
			/*thread?.Abort();*/thread=null;
		}

		protected virtual void Recycle() {
		}

		/// <summary>
		/// Stop the task hardly.
		/// </summary>
		public virtual void Dispose() {
			if(id>=0) {// TODO: Valid instance.
				Reset();Recycle();
			}else {
				Debug.LogWarning("It is disposed.");
			}
		}

		protected virtual void OnExecute() {
#if _DEBUG
			Debug.Log("Execute the AsyncTask@"+id);
#endif
		}

		protected virtual void OnComplete() {
#if _DEBUG
			Debug.Log("Complete the AsyncTask@"+id);
#endif
			Dispose();
		}

		protected virtual void OnKill() {
#if _DEBUG
			Debug.Log("Kill the AsyncTask@"+(id<0?(-id-1):id));
#endif
			Dispose();
		}

		public virtual System.Collections.IEnumerator Run() {
			int i=id;float t=Time.time+delay;
			while(Time.time<t) {yield return null;}
			//
			if(id==i) {OnExecute();}
			//
			if(id<0) {}// TODO: Disposed.
			else if(id==i) {OnComplete();}
			else if(id<i) {OnKill();}
			// TODO: else if(i<id) New Instance.
		}

		public virtual Coroutine StartAsCoroutine() {
			if(id<0) {// TODO: Disposed.
				throw new System.ObjectDisposedException(nameof(AsyncTask));
			}
			return GetBehaviour().StartCoroutine(Run());
		}

		public virtual void Run(object arg) {bool b=(bool)arg;
			int i=id;int t=(int)(System.Environment.TickCount+delay*1000);
			while(System.Environment.TickCount<t) {}
			//
			if(id==i) {OnExecute();}
			//
			if(id<0) {}// TODO: Disposed.
			else if(id==i) {OnEvent(OnComplete,b);}
			else if(id<i) {OnEvent(OnKill,b);}
			// TODO: else if(i<id) New Instance.
		}

		public virtual Thread StartAsThread(bool main=true) {
			if(id<0) {// TODO: Disposed.
				throw new System.ObjectDisposedException(nameof(AsyncTask));
			}
			thread=new Thread(Run);thread.Start(main);return thread;
		}

		/// <summary>
		/// Stop the task softly.
		/// </summary>
		public virtual void Kill() {
			if(id>0) {--id;}// TODO: <0:Disposed,==0:Ready to be killed.
		}

		#endregion Methods
	}
	public class AsyncTask<T>
		:AsyncTask
		where T:AsyncTask<T>,new()
	{
		#region Methods

		public static AsyncTask<T> Obtain()=>GenericPool<T>.Get();
		protected override void Recycle()=>GenericPool<AsyncTask<T>>.Release(this);

		#endregion Methods
	}

	public class _AsyncTask
		:AsyncTask
	{
// <!-- Macro.Patch _AsyncTask
		public System.Action onExecute;
		protected override void OnExecute() {onExecute?.Invoke();base.OnExecute();}

		public System.Action onComplete;
		protected override void OnComplete() {onComplete?.Invoke();base.OnComplete();}

		public System.Action onKill;
		protected override void OnKill() {onKill?.Invoke();base.OnKill();}

		protected override void Reset() {
			base.Reset();
			onExecute=null;
			onComplete=null;
			onKill=null;
		}

// Macro.Patch -->
		protected override void Recycle() {
			GenericPool<_AsyncTask>.Release(this);
		}
	}
}
