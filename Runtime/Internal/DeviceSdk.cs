/* <!-- Macro.Define bFixPatch=
true
 Macro.End --> */
/* <!-- Macro.Define Instance
		public static $(FileName) s_Instance;
#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
#else
		[UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
		public static void InitializeOnLoadMethod() {
			s_Instance=Register<$(FileName)>();
		}

		public static $(FileName) instance {
			get {
				if(s_Instance==null) {InitializeOnLoadMethod();}
				return s_Instance;
			}
		}

 Macro.End --> */
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace YouSingStudio.Holograms {
	public class DeviceSdk
	{
		#region Fields

		public static List<DeviceSdk> s_All=new List<DeviceSdk>();

		public string file;
		public string app;
		public string config;

		[System.NonSerialized]protected string m_DeviceConfig;
		[System.NonSerialized]protected System.Action<string> m_OnDeviceUpdated;

		#endregion Fields

		#region Methods
#if ENABLE_IL2CPP&&UNITY_STANDALONE_WIN
		// https://learn.microsoft.com/en-us/windows/win32/psapi/enumerating-all-processes
		// dotnet451/Source/ndp/fx/src/Services/Monitoring/system/Diagnosticts/ProcessManager.cs
		[DllImport("kernel32.dll")]
		public static extern bool EnumProcesses(int[] lpidProcess,int cb,out int lpcbNeeded);
		[DllImport("kernel32.dll")]
		public static extern System.IntPtr OpenProcess(int dwDesiredAccess,bool bInheritHandle,int dwProcessId);
		[DllImport("kernel32.dll")]
		public static extern bool EnumProcessModulesEx(System.IntPtr hProcess,System.IntPtr[] lphModule,int cb,out int lpcbNeeded,int dwFilterFlag);
		[DllImport("kernel32.dll")]
		public static extern int GetModuleBaseNameA(System.IntPtr hProcess,System.IntPtr hModule,System.IntPtr lpBaseName,int nSize);

		public static int[] GetProcessIds() {
			int[] processIds=new int[1024];
			int size;
			for(;;){
				if(!EnumProcesses(processIds,processIds.Length*4,out size))
					throw new System.ComponentModel.Win32Exception();
				if(size==processIds.Length*4) {
					processIds=new int[processIds.Length*2];
					continue;
				}
				break;
			}
			int[] ids=new int[size/4];
			System.Array.Copy(processIds,ids,ids.Length);
			return ids;
		}

		public static string GetProcessName(int id) {
			string n=null;
			// PROCESS_QUERY_INFORMATION = 0x0400;PROCESS_VM_READ = 0x0010;
			System.IntPtr p=OpenProcess(0x0410,false,id);
			if(p!=System.IntPtr.Zero) {
				System.IntPtr[] h=new System.IntPtr[1];
				if(EnumProcessModulesEx(p,h,4*h.Length,out _,0x03)) {
					System.IntPtr s=Marshal.AllocHGlobal(1024);// MAX_PATH
						GetModuleBaseNameA(p,h[0],s,1024);
						n=Marshal.PtrToStringAnsi(s);
					Marshal.FreeHGlobal(s);
				}
			}
			return n;
		}
#endif
		public static bool IsRunning(string name) {
#if ENABLE_IL2CPP&&UNITY_STANDALONE_WIN
			int[] tmp=GetProcessIds();
			for(int i=0,imax=tmp?.Length??0;i<imax;++i) {
				if(string.Equals(name,GetProcessName(tmp[i]),UnityExtension.k_Comparison)) {return true;}
			}
#else
			var tmp=System.Diagnostics.Process.GetProcesses();
			System.Diagnostics.Process it;
			for(int i=0,imax=tmp?.Length??0;i<imax;++i) {
				it=tmp[i];if(it.HasExited) {continue;}
				if(string.Equals(name,it.MainModule?.ModuleName,UnityExtension.k_Comparison)) {return true;}
			}
#endif
			return false;
		}

		public static T Register<T>() where T:DeviceSdk,new() {
			T tmp=null,it;
			for(int i=0,imax=s_All.Count;i<imax;++i) {
				it=s_All[i] as T;
				if(it!=null) {tmp=it;break;}
			}
			if(tmp==null) {
				tmp=new T();s_All.Add(tmp);
			}
			return tmp;
		}

		public virtual bool IsInstalled {
			get=>!string.IsNullOrEmpty(file)&&File.Exists(file.GetFullPath());
		}

		public virtual bool IsDetected {
			get{
				DeviceSdk it;int j=0;
				for(int i=0,imax=s_All.Count;i<imax;++i) {
					it=s_All[i];if(it!=null&&it!=this) {
						if(it.IsInstalled) {++j;}
					}
				}
				return j==0||IsRunning(app);
			}
		}

		public virtual event System.Action<string> onDeviceUpdated {
			add{
				m_OnDeviceUpdated+=value;
				if(!string.IsNullOrEmpty(m_DeviceConfig)) {value?.Invoke(m_DeviceConfig);}
			}
			remove=>m_OnDeviceUpdated-=value;
		}

		public virtual void LoadDeviceConfig(string value) {
			m_DeviceConfig=value;
			//
			if(!string.IsNullOrEmpty(config)) {
				string path=config.GetFullPath();
				if(string.IsNullOrEmpty(m_DeviceConfig)) {
					if(File.Exists(path)) {File.Delete(path);}
				}else {
					File.WriteAllText(path,m_DeviceConfig);
				}
			}
			//
			m_OnDeviceUpdated?.Invoke(m_DeviceConfig);
		}

		public virtual void LoadDeviceConfig(JObject value) {
			JToken jt=value.SelectToken("sdkType");
			if(jt!=null&&string.Equals(jt.Value<string>(),"manual",UnityExtension.k_Comparison)) {
				value.Remove("sdkType");
			}
			//
			LoadDeviceConfig(value.ToString());
		}

		#endregion Methods
	}
}
