/* <!-- Macro.Include
../Internal/DeviceSdk.cs
 Macro.End --> */
/* <!-- Macro.Define Src=
C:/Users/Administrator/Documents/GitHub/CubeVi-Swizzle-Unity/Scripts
 Macro.End --> */
/* <!-- Macro.Copy Define
Instance
 Macro.End --> */
/* <!-- Macro.Copy File
$(Src)/DeviceData.cs,6~44
$(Src)/BatchCameraManager.cs,137~160,170~277
 Macro.End --> */
/* <!-- Macro.Replace
    ,	
SwizzleLog,Debug
private void InitDeviceData(),public static DeviceData NewDeviceData()
_device = ,return 
_device,m_DeviceData
 Macro.End --> */
/* <!-- Macro.Patch
,AutoGen
 Macro.End --> */
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso href="https://github.com/CubeVi/CubeVi-Swizzle-Unity"/>
	/// </summary>
	public class CubeViSdk
		:DeviceSdk
	{
// <!-- Macro.Patch AutoGen
		public static CubeViSdk s_Instance;
#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
#else
		[UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
		public static void InitializeOnLoadMethod() {
			s_Instance=Register<CubeViSdk>();
		}

		public static CubeViSdk instance {
			get {
				if(s_Instance==null) {InitializeOnLoadMethod();}
				return s_Instance;
			}
		}

	public class DeviceData
	{
		public string name;
		public int imgs_count_x;
		public int imgs_count_y;
		public int viewnum;
		public float theta;
		public float output_size_X;
		public float output_size_Y;
		public int subimg_width;
		public int subimg_height;
		public float f_cam;
		public float tan_alpha_2;
		public float x0;
		public float interval;
		public float slope;
		public float nearrate;
		public float farrate;
	}

	[System.Serializable]
	public class SwizzleConfig
	{
		public float lineNumber;
		public float obliquity;
		public float deviation;
	}

	[System.Serializable]
	public class DeviceConfig
	{
		public SwizzleConfig config;
	}

	[System.Serializable]
	public class JsonWrapper
	{
		public string config;
	}
		public static DeviceData NewDeviceData()
		{
			return new DeviceData()
			{
				// Default parameters
				name = "5.7",
				imgs_count_x = 8,
				imgs_count_y = 5,
				viewnum = 40,
				theta = 40f,
				output_size_X = 1440f,
				output_size_Y = 2560f,
				subimg_width = 540,
				subimg_height = 960,
				f_cam = 3806f,
				tan_alpha_2 = 0.071f,
				x0 = 3.59f,
				interval = 19.6169f,
				slope = 0.1021f,
				nearrate = 0.96f,
				farrate = 1.08f
			};
		}

		private void LoadSwzzleConfig()
		{
			SwizzleConfig config = new SwizzleConfig();
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string OAinstallPath = Path.Combine(appDataPath, "OpenstageAI", "deviceConfig.json");

			if (File.Exists(OAinstallPath))
			{
				string jsonContent = File.ReadAllText(OAinstallPath);
				var json = JsonUtility.FromJson<JsonWrapper>(jsonContent);

				string decryptedConfig = DecryptAes(json.config);
				var decryptedData = JsonUtility.FromJson<DeviceConfig>(decryptedConfig);

				m_DeviceData.slope = decryptedData.config.obliquity;
				m_DeviceData.interval = decryptedData.config.lineNumber;
				m_DeviceData.x0 = decryptedData.config.deviation;
			}
			else
			{
				Debug.LogError("Please check if OpenstageAI platform is correctly installed or if Companion 01 device is properly connected");
			}
		}

		public static void DeriveKeyAndIv(byte[] passphrase, byte[] salt, int iterations, out byte[] key, out byte[] iv)
		{
			var hashList = new List<byte>();

			var preHashLength = passphrase.Length + (salt?.Length ?? 0);
			var preHash = new byte[preHashLength];

			Buffer.BlockCopy(passphrase, 0, preHash, 0, passphrase.Length);
			if (salt != null)
				Buffer.BlockCopy(salt, 0, preHash, passphrase.Length, salt.Length);

			var hash = MD5.Create();
			var currentHash = hash.ComputeHash(preHash);

			for (var i = 1; i < iterations; i++)
			{
				currentHash = hash.ComputeHash(currentHash);
			}

			hashList.AddRange(currentHash);

			while (hashList.Count < 48) // for 32-byte key and 16-byte iv
			{
				preHashLength = currentHash.Length + passphrase.Length + (salt?.Length ?? 0);
				preHash = new byte[preHashLength];

				Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
				Buffer.BlockCopy(passphrase, 0, preHash, currentHash.Length, passphrase.Length);
				if (salt != null)
					Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + passphrase.Length, salt.Length);

				currentHash = hash.ComputeHash(preHash);

				for (var i = 1; i < iterations; i++)
				{
					currentHash = hash.ComputeHash(currentHash);
				}

				hashList.AddRange(currentHash);
			}

			hash.Clear();
			key = new byte[32];
			iv = new byte[16];
			hashList.CopyTo(0, key, 0, 32);
			hashList.CopyTo(32, iv, 0, 16);
		}

		public static string DecryptAes(string encryptedString)
		{
			var passphrase = "3f5e1a2b4c6d7e8f9a0b1c2d3e4f5a6b";
			// encryptedString is a base64-encoded string starting with "Salted__" followed by a 8-byte salt and the
			// actual ciphertext. Split them here to get the salted and the ciphertext
			var base64Bytes = Convert.FromBase64String(encryptedString);
			var saltBytes = base64Bytes[8..16];
			var cipherTextBytes = base64Bytes[16..];

			// get the byte array of the passphrase
			var passphraseBytes = Encoding.UTF8.GetBytes(passphrase);

			// derive the key and the iv from the passphrase and the salt, using 1 iteration
			// (cryptojs uses 1 iteration by default)
			DeriveKeyAndIv(passphraseBytes, saltBytes, 1, out var keyBytes, out var ivBytes);

			// create the AES decryptor
			using var aes = Aes.Create();
			aes.Key = keyBytes;
			aes.IV = ivBytes;
			// here are the config that cryptojs uses by default
			// https://cryptojs.gitbook.io/docs/#ciphers
			aes.KeySize = 256;
			aes.Padding = PaddingMode.PKCS7;
			aes.Mode = CipherMode.CBC;
			var decryptor = aes.CreateDecryptor(keyBytes, ivBytes);

			// example code on MSDN https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=net-5.0
			using var msDecrypt = new MemoryStream(cipherTextBytes);
			using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
			using var srDecrypt = new StreamReader(csDecrypt);

			// read the decrypted bytes from the decrypting stream and place them in a string.
			return srDecrypt.ReadToEnd();
		}

// Macro.Patch -->
		#region Nested Types
		#endregion Nested Types

		#region Fields

		public static readonly DeviceData k_Device=NewDeviceData();

		[System.NonSerialized]protected DeviceData m_DeviceData;

		#endregion Fields

		#region Methods

		public CubeViSdk() {
			file="$(AppData)/Roaming/OpenstageAI/deviceConfig.json";
			app="OpenstageAI.exe";
			config=$"$(SaveData)/{nameof(CubeViSdk)}.json";
			//
			string fn=config.GetFullPath();
			if(File.Exists(fn)) {
				m_DeviceConfig=File.ReadAllText(fn);
			}else {
				m_DeviceData=NewDeviceData();LoadSwzzleConfig();
				m_DeviceConfig=JsonUtility.ToJson(m_DeviceData);
			}
		}

		public override void LoadDeviceConfig(JObject value) {
			var tmp=config;
				var jt=value.SelectToken("deviceNumber");
				if(jt!=null) {config=jt.Value<string>()+".json";}
				base.LoadDeviceConfig(value);
			config=tmp;
		}

		#endregion Methods
	}
}
