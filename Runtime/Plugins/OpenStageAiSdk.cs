/* Mini/Portable SDK:
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
	#region Nested Types

	[System.Flags]
	public enum Feature {
		OfficialSDK     =0x01,
		CustomSDK       =0x02,
		SaveDeviceJson  =0x04,
		SaveDeviceQR    =0x08,
	}

	#endregion Nested Types

	#region Fields

	public static OpenStageAiSdk s_Instance;

	[HideInInspector]public string phone;
	[HideInInspector]public string password;
	public Feature features;
	public string hardwareSN;

	[System.NonSerialized]public string deviceConfig;
	[System.NonSerialized]protected System.Action<string> m_OnDeviceUpdated=null;
	[System.NonSerialized]protected AsyncTask m_Task;

	#endregion Fields

	#region Unity Messages

	protected virtual void Reset() {
		features=Feature.OfficialSDK|Feature.CustomSDK|Feature.SaveDeviceJson;
		expiration=60*60*24*30;// One month.
		url="https://app.realplaybox.cn/{0}";
		urls=new string[]{
			"",
			"jeecg-boot/user/auth/login",
			"jeecg-boot/user/auth/logout",
			"jeecg-boot/user/auth/getVerifyCode",
			"",
			"jeecg-boot/openStage/device/queryScreenParam?hardwareSN="
		};
		texts=new string[]{
			"",// user name
			"",// password
			"",// verification code
			"application/json;charset=utf-8",
			"{{\"account\":\"{0}\",\"password\":\"{1}\",\"verifyCode\":\"{2}\",\"loginType\":\"{3}\",\"accountType\":\"phone\"}}",
			"success",
			"message",
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
		InitSDK();
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
		// Fix auto-login.
		SetString(".Phone",texts[0]);
		SetString(".Password",texts[1]);
		//
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
		string[] ports=SerialPort.GetPortNames();
		int len=ports?.Length??0;return len>0?ports[len-1]:null;
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
				"Roaming/OpenstageAI"
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

	// Official SDK

	public virtual bool portable {
		get {
			return (features&Feature.OfficialSDK)==0&&(features&Feature.CustomSDK)!=0;
		}
	}

	protected virtual void InitSDK() {
		if(!Directory.Exists(settingsPath)
#if !UNITY_EDITOR&&(UNITY_ANDROID||UNITY_IOS)
			||true
#endif
		) {
			if((features&Feature.CustomSDK)!=0) {features&=~Feature.OfficialSDK;}
		}
		if((features&Feature.OfficialSDK)!=0) {
			// Cleanup
			enabled=false;
			hardwareSN="null";
			displayName="已接入官方SDK";
			avatarIcon=m_AvatarIcon;
			//
			var client=GetComponent<PipeClient>();
			if(client==null) {gameObject.AddComponent<PipeClient>();}
			PipeClient.eventObj.MyEvent+=OnDeviceUpdated;
			OnDeviceUpdated(null,PipeClient.deviceConfig);
		}
	}

	protected virtual void OnDeviceUpdated(object sender,DeviceConfig e) {
		if(e!=null) {
			JObject jo=JObject.FromObject(e.config);
			jo["sdkType"]="OfficialSDK";
			if(!string.IsNullOrEmpty(jo["deviceId"]?.Value<string>())) {
				Log($"Load {e.type} config from official sdk.");
				deviceConfig=jo.ToString();// TODO : 4th Priority.
				SetString(".DeviceConfig",deviceConfig);
				//
				OnDeviceUpdated();
				// Fire a fake event to update ui.
				displayName=jo["deviceNumber"]?.Value<string>()??"已获取设备参数";
				InvokeEvent(k_Type_Login);
			}else {
				Log("Pass the invalid confg.");
			}
		}
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
		if(!string.IsNullOrEmpty(deviceConfig)) {return;}
		//
		deviceConfig=GetString(".DeviceConfig",deviceConfig);// TODO : 3rd Priority.
		string fn="deviceConfig.json";// TODO : 1st Priority.
		if(!File.Exists(fn)) {
			fn=Path.Combine(settingsPath,fn);// TODO : 2nd Priority.
		}
		if(File.Exists(fn)) {deviceConfig=File.ReadAllText(fn);}
	}

	protected virtual void OnDeviceUpdated() {
		if(!string.IsNullOrEmpty(deviceConfig)) {
			if(m_OnDeviceUpdated==null) {Log(deviceConfig);}
			else {m_OnDeviceUpdated.Invoke(deviceConfig);}
			//
			if((features&Feature.SaveDeviceJson)!=0) {
				string fn="deviceConfig.json";
				JToken jt=JObject.Parse(deviceConfig).SelectToken("deviceNumber");
				if(jt!=null) {fn=jt.Value<string>()+".json";}
				File.WriteAllText(fn,deviceConfig);
			}
			if((features&Feature.SaveDeviceQR)!=0) {}// TODO: QRCode for mobile????
		}
	}

	public virtual void Find() {
#if true//HAS_OFFICIAL_SDK
		if(!portable) {return;}
#endif
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

	public override void SetForm(int type,string[] table) {
		switch(type) {
			case k_Type_Login:
			case k_Type_Verify:
				SetString(".Phone",table[0]);
			break;
		}
		base.SetForm(type,table);
	}

	public override void Login() {
#if true//HAS_OFFICIAL_SDK
		if(!portable) {return;}
#endif
		Logout();
		if(string.IsNullOrEmpty(texts[0])||(string.IsNullOrEmpty(texts[1])&&string.IsNullOrEmpty(texts[2]))) {return;}
		if(texts[0].Length!=11||!texts[0].IsSerialNumber()) {return;}// Phone
		//
		Log($"Login account {texts[0]}.");
		int offset=k_Offset_DoLogin;
		string str=string.Format(texts[offset+1],texts[0],texts[1],texts[2],string.IsNullOrEmpty(texts[2])?"password":"verifyCode");
		var www=UnityWebRequest.Post(GetUrl(k_Type_Login),str,texts[offset+0]);
		SendRequest(www,OnLogin);
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
		}
	}

	public override void Logout() {
#if true//HAS_OFFICIAL_SDK
		if(!portable) {return;}
#endif
		if(authorized) {Log("Logout.");}
		base.Logout();
	}

	public override void Verify() {
#if true//HAS_OFFICIAL_SDK
		if(!portable) {return;}
#endif
		if(string.IsNullOrEmpty(texts[0])) {return;}
		//
		var www=UnityWebRequest.Get(GetUrl(k_Type_Verify)+$"?account={texts[0]}&type={texts[2]}&accountType=phone");
		SendRequest(www,OnVerify);
	}

	protected virtual void OnVerify(string text) {
		Log("Verify :"+text);
		JObject jo=JObject.Parse(text);
		m_Message=jo?.SelectToken(texts[k_Offset_OnLogin+1])?.Value<string>();
		InvokeEvent(k_Type_Verify);
	}

	public virtual void Query() {
#if true//HAS_OFFICIAL_SDK
		if(!portable) {return;}
#endif
		if(authorized&&!string.IsNullOrEmpty(hardwareSN)) {
			Log("Query the device(SN:"+hardwareSN+")");
			//
			var www=UnityWebRequest.Get(GetUrl(5)+hardwareSN);
			www.SetRequestHeader("X-Access-Token",m_Token);
			www.SetRequestHeader("X-Sign",m_Token);
			SendRequest(www,OnQuery);
		}
	}

	protected virtual void OnQuery(string text) {
		if(!string.IsNullOrEmpty(text)) {
			JObject jo=JObject.Parse(text);
			if(IsSuccess(jo.SelectToken(texts[k_Offset_OnLogin])?.Value<string>())) {
				JToken jt=jo.SelectToken("result");jt["sdkType"]="CustomSDK";
				deviceConfig=jt.ToString();// TODO : 4th Priority.
				SetString(".DeviceConfig",deviceConfig);
				//
				OnDeviceUpdated();
			}else {
				Debug.LogError(text);
				//if(expired) {Logout();}
			}
		}
	}

	#endregion Methods
}
