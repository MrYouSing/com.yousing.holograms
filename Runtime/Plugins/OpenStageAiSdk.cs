/*
You can get the device config by three steps:
	1) Read hardware SN by SerialPort.
	2) Login the OpenStageAI account to get token.
	3) Query screen params by WebRequest.GET.
*/
#if DEBUG
#define _DEBUG
#endif
using Newtonsoft.Json.Linq;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using YouSingStudio.Holograms;
using YouSingStudio.Private;

/// <summary>
/// Simplified sdk for CompanionOne devices.<br/>
/// <seealso href="https://www.openstageai.com/"/>
/// </summary>
public class OpenStageAiSdk
	:OAuthBehaviour
{
	#region Fields

	public static OpenStageAiSdk s_Instance;

	[HideInInspector]public string phone;
	[HideInInspector]public string password;
	public string hardwareSN;

	[System.NonSerialized]public string deviceConfig;
	[System.NonSerialized]protected System.Action<string> m_OnDeviceUpdated=null;
	[System.NonSerialized]protected AsyncTask m_Task;

	#endregion Fields

	#region Unity Messages

	protected virtual void Reset() {
		url="https://app.realplaybox.cn/{0}";
		urls=new string[]{
			"",
			"",
			"jeecg-boot/user/auth/login",
			"",
			"jeecg-boot/openStage/device/queryScreenParam?hardwareSN="
		};
		texts=new string[]{
			"",// user name
			"",// password
			"",// verification code
			"application/json;charset=utf-8",
			"{{\"account\":\"{0}\",\"password\":\"{1}\",\"loginType\":\"password\"}}",
			"success",
			"result.token",
			"result.userInfo.nickName",
			"result.userInfo.photo",
		};
	}

	protected override void Awake() {
		s_Instance=this;this.SetRealName();
		// Trick by settings.
		phone=GetString(".Phone",texts[0]);
		password=GetString(".Password",texts[1]);
			this.LoadSettings(name);
		texts[0]=phone;
		texts[1]=password;
		//
		if(string.IsNullOrEmpty(hardwareSN)) {Find();}
		base.Awake();
	}

	protected override void Start() {
		LoadDeviceConfig();OnDeviceUpdated();
		//
		base.Start();
	}

	protected virtual void OnDestroy() {
		m_Task?.Kill();m_Task=null;
	}

	#endregion Unity Messages

	#region Methods

	// TODO: Move to SerialPortManager????
#if UNITY_EDITOR_WIN||UNITY_STANDALONE_WIN
	// Taken from https://blog.csdn.net/qq635968970/article/details/135979635
	// https://github.com/libyal/winsps-kb/tree/main/docs/sources/property-sets/a45c254e-df1c-4efd-8020-67d146a850e0.md
	[DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)]
	protected static extern System.IntPtr SetupDiGetClassDevs(ref System.Guid classGuid,string enumerator,System.IntPtr hwndParent,uint flags);

	[DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)]
	protected static extern bool SetupDiEnumDeviceInfo(System.IntPtr deviceInfoSet,uint memberIndex,ref SP_DEVINFO_DATA deviceInfoData);

	[DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)]
	protected static extern bool SetupDiGetDeviceProperty(System.IntPtr deviceInfoSet,ref SP_DEVINFO_DATA deviceInfoData,ref DEVPROPKEY propertyKey,out int propertyType,System.IntPtr propertyBuffer,int propertyBufferSize,out int requiredSize,int flags);

	[DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)]
	protected static extern bool SetupDiDestroyDeviceInfoList(System.IntPtr deviceInfoSet);

	[StructLayout(LayoutKind.Sequential)]
	protected struct SP_DEVINFO_DATA {
		public int cbSize;
		public System.Guid classGuid;
		public int devInst;
		public System.IntPtr reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	protected struct DEVPROPKEY {
		public System.Guid fmtid;
		public uint pid;
	}

	protected static bool GetDeviceID(System.IntPtr deviceInfoSet,SP_DEVINFO_DATA devInfoData,out ushort vid,out ushort pid) {
		vid=pid=0;
		//
		DEVPROPKEY devPropKey=new DEVPROPKEY();
		devPropKey.fmtid=new System.Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"); // DEVPKEY_Device_HardwareIds 的格式 ID
		devPropKey.pid=3; // DEVPKEY_Device_HardwareIds 的属性 ID

		int propertyType;int requiredSize;
		SetupDiGetDeviceProperty(deviceInfoSet,ref devInfoData,ref devPropKey,out propertyType,System.IntPtr.Zero,0,out requiredSize,0);

		System.IntPtr propertyBuffer=Marshal.AllocHGlobal(requiredSize);
		if(SetupDiGetDeviceProperty(deviceInfoSet,ref devInfoData,ref devPropKey,out propertyType,propertyBuffer,requiredSize,out requiredSize,0)) {
			string propertyValue=Marshal.PtrToStringUni(propertyBuffer);
			int i=propertyValue.IndexOf("USB\\VID_");
			vid=System.Convert.ToUInt16(propertyValue.Substring(i+"USB\\VID_".Length,4),16);
			i=propertyValue.IndexOf("&PID_",i);
			pid=System.Convert.ToUInt16(propertyValue.Substring(i+"&PID_".Length,4),16);
		}
		Marshal.FreeHGlobal(propertyBuffer);
		//
		return vid!=0&&pid!=0;
	}

	protected static string GetDeviceName(System.IntPtr deviceInfoSet,SP_DEVINFO_DATA devInfoData) {
		string propertyValue=null;
		//
		DEVPROPKEY propertyKey=new DEVPROPKEY();
		propertyKey.fmtid=new System.Guid("a45c254e-df1c-4efd-8020-67d146a850e0");// DEVPKEY_Device_FriendlyName
		propertyKey.pid=14;// PKEY_Device_FriendlyName

		int propertyType;int requiredSize;
		SetupDiGetDeviceProperty(deviceInfoSet,ref devInfoData,ref propertyKey,out propertyType,System.IntPtr.Zero,0,out requiredSize,0);

		System.IntPtr propertyBuffer=Marshal.AllocHGlobal(requiredSize);
		if(SetupDiGetDeviceProperty(deviceInfoSet,ref devInfoData,ref propertyKey,out propertyType,propertyBuffer,requiredSize,out requiredSize,0)) {
			propertyValue=Marshal.PtrToStringUni(propertyBuffer);
			int i=propertyValue.LastIndexOf("(COM"),j=propertyValue.LastIndexOf(")");
			if(i>=0&&j>=0) {i+=1;propertyValue=propertyValue.Substring(i,j-i);}
		}
		Marshal.FreeHGlobal(propertyBuffer);
		//
		return propertyValue;
	}

	public static string FindSerialPort(ushort vid,ushort pid) {
		ushort v,p;string com=null;
		//
		System.Guid guid=new System.Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED"); // USB 设备类的 GUID
		System.IntPtr deviceInfoSet=SetupDiGetClassDevs(ref guid,null,System.IntPtr.Zero,0x12);

		if(deviceInfoSet.ToInt64()!=-1) {
			SP_DEVINFO_DATA devInfoData=new SP_DEVINFO_DATA();
			devInfoData.cbSize=Marshal.SizeOf(devInfoData);

			for(uint i=0;SetupDiEnumDeviceInfo(deviceInfoSet,i,ref devInfoData);i++) {
				if(GetDeviceID(deviceInfoSet,devInfoData,out v,out p)) {
					if(vid==v&&pid==p) {com=GetDeviceName(deviceInfoSet,devInfoData);break;}
				}
			}
			SetupDiDestroyDeviceInfoList(deviceInfoSet);
		}
		return com;
	}
#else
	public static string FindSerialPort(int vid,int pid) {
		return null;
	}
#endif
	public static OpenStageAiSdk instance {
		get{
			if(s_Instance==null) {
				s_Instance=UnityExtension.GetResourceInstance<OpenStageAiSdk>("Prefabs/OpenStageAI");
			}
			return s_Instance;
		}
	}

	public static string settingsPath {
		get {
#if UNITY_EDITOR_WIN||UNITY_STANDALONE_WIN
			return Path.Combine(Path.GetDirectoryName(
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)),
				"Roaming/realplayplatform"
			);
#elif !UNITY_EDITOR&&(UNITY_ANDROID||UNITY_IOS)// Mobile
			return Application.persistentDataPath;
#endif
		}
	}

	public virtual void Log(string msg) {
#if _DEBUG
		Debug.Log(name+":"+msg);
#endif
	}

	// Device Methods

	public virtual event System.Action<string> onDeviceUpdated {
		add{
			m_OnDeviceUpdated+=value;
			if(value!=null&&!string.IsNullOrEmpty(deviceConfig)) {value.Invoke(deviceConfig);}
		}
		remove{m_OnDeviceUpdated-=value;}
	}

	protected virtual void LoadDeviceConfig() {
		deviceConfig=GetString(".DeviceConfig",deviceConfig);// TODO : 3rd Priority.
		string fn="deviceConfig.json";// TODO : 1st Priority.
		if(!File.Exists(fn)) {
			fn=Path.Combine(settingsPath,fn);// TODO : 2nd Priority.
		}
		if(File.Exists(fn)) {deviceConfig=File.ReadAllText(fn);}
	}

	protected void OnDeviceUpdated() {
		if(!string.IsNullOrEmpty(deviceConfig)) {
			if(m_OnDeviceUpdated==null) {Log(deviceConfig);}
			else {m_OnDeviceUpdated.Invoke(deviceConfig);}
			//
			File.WriteAllText("deviceConfig.json",deviceConfig);
			// TODO: QrCode for mobile????
		}
	}

	public virtual void Find() {
		string tmp=FindSerialPort(0x1A86,0xFE0C);
		if(string.IsNullOrEmpty(tmp)) {return;}Log("Open SerialPort "+tmp);
		byte[] buffer=new byte[40];
		try{using(SerialPort sp=new SerialPort(tmp,baudRate:115200,Parity.None,dataBits:8,stopBits:(StopBits)1)) {
			// Write
			tmp=null;sp.Open();
				buffer[0]=4;buffer[1]=0;buffer[2]=0;
				buffer[3]=48;buffer[4]=1;
			sp.Write(buffer,0,5);
			// Read
			int i=0,imax=buffer.Length;while(i<17) {
				i+=sp.Read(buffer,i,imax-i);
			}
			// Convert and slice.
			StringBuilder sb=new StringBuilder();
			for(imax=Mathf.Min(i,15),i=3;i<imax;++i) {
				sb.Append(buffer[i].ToString("x2"));
			}
			tmp=sb.ToString();
		}}catch (System.Exception e) {
			tmp=null;
			Debug.LogException(e);
		}
		if(!string.IsNullOrEmpty(tmp)) {
			hardwareSN=tmp;
			Query();
		}
	}

	// WebRequest Methods

	public override void Login() {
		Log($"Login account {texts[0]}.");base.Login();
	}

	protected override void OnLogin(string text) {
		Log($"On login:\n{text}");base.OnLogin(text);
	}

	protected override void OnLogin() {
		if(authorized) {
			base.OnLogin();Log($"Current account {displayName}.");
			SetString(".Phone",texts[0]);
			SetString(".Password",texts[1]);
			//
			Query();
		}else { 
			Logout();
		}
	}

	public override void Logout() {
		base.Logout();Log("Logout.");
		Login();// Delayed????
	}

	public virtual void Query() {
		if(authorized&&!string.IsNullOrEmpty(hardwareSN)) {
			Log("Query the device(SN:"+hardwareSN+")");
			//
			var www=UnityWebRequest.Get(GetUrl(4)+hardwareSN);
			www.SetRequestHeader("X-Access-Token",m_Token);
			www.SetRequestHeader("X-Sign",m_Token);
			SendRequest(www,OnQuery);
		}
	}

	protected virtual void OnQuery(string text) {
		if(!string.IsNullOrEmpty(text)) {
			JObject jo=JObject.Parse(text);
			if(IsSuccess(jo.SelectToken(texts[k_Offset_OnLogin])?.Value<string>())) {
				deviceConfig=jo.SelectToken("result").ToString();// TODO : 4th Priority.
				SetString(".DeviceConfig",deviceConfig);
				//
				OnDeviceUpdated();
			}else {
				Debug.LogError(text);
			}
		}
	}

	#endregion Methods
}
