#if !true
#region C# Codes
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace VirtualDisplay {
	public partial class Form1:Form {
		public static Screen GetScreen(Size size) {
			Screen[] screens=Screen.AllScreens;Screen it;
			for(int i=0,imax=screens?.Length??0;i<imax;++i) {
				it=screens[i];if(it!=null&&it.Bounds.Size==size) {
					return it;
				}
			}
			return null;
		}

		public bool exit;
		public string path="test.png";
		public int width=1920,height=1080;
		//
		public PictureBox box;
		public Bitmap bitmap;
		public Thread thread;
		public NamedPipeServerStream server;
		public byte[] buffer;

		public Form1() {
			//
			CheckForIllegalCrossThreadCalls=false;
			string[] args=System.Environment.GetCommandLineArgs();int cnt=args?.Length??0;
			if(1<cnt) {path=args[1];}
			if(2<cnt) {int.TryParse(args[2],out width);}
			if(3<cnt) {int.TryParse(args[3],out height);}
			exit=true;
			//
			InitializeComponent();
			BackColor=Color.Black;
			FormBorderStyle=FormBorderStyle.None;
			Load+=OnLoad;
			KeyDown+=OnKeyDown;
			//
			box=new PictureBox();
			Controls.Add(box);box.Dock=DockStyle.Fill;
			box.SizeMode=PictureBoxSizeMode.StretchImage;
			if(!string.IsNullOrEmpty(path)) {
				Image img=null;
				if(path.StartsWith("/tmp/")) {
					img=bitmap=new Bitmap(width,height);
					buffer=new byte[width*height*4*2];
					thread=new Thread(OnServer);
					thread.Start();
				}else if(path.StartsWith("base64:")) {
					path=path.Substring("base64:".Length);
					using(MemoryStream ms=new MemoryStream(System.Convert.FromBase64String(path))) {
						img=Image.FromStream(ms);
					}
				}else if(File.Exists(path)) {
					img=Image.FromFile(path);
				}
				if(img!=null) {box.Image=img;exit=false;return;}
			}
		}

		public virtual void OnLoad(object sender,System.EventArgs e) {
			if(exit) {Close();return;}
			//
			Screen s=GetScreen(new Size(width,height));
			if(s!=null) {var b=s.Bounds;Location=b.Location;Size=b.Size;}
			else {Location=Point.Empty;Size=new Size(width,height);}
		}

		public virtual void OnKeyDown(object sender,KeyEventArgs e) {
			switch(e.KeyCode) {
				case Keys.Escape:exit=true;Close();break;
			}
		}

		public virtual void OnServer() {
			while(!exit) {
				server=new NamedPipeServerStream(path,PipeDirection.In);
				server.WaitForConnection();
				while(server.IsConnected) {try{
					int cnt=server.Read(buffer,0,width*height*4);
					if(cnt>0) {
						var bmd=bitmap.LockBits(new Rectangle(0,0,width,height),ImageLockMode.WriteOnly,PixelFormat.Format32bppRgb);
							Marshal.Copy(buffer,0,bmd.Scan0,width*height*4);
						bitmap.UnlockBits(bmd);bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
						box.Refresh();
					}
				}catch(System.Exception e) {System.Console.WriteLine(e);}}
				server.Close();
			}
		}
	}
}
#endregion C# Codes
#else
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using YouSingStudio.Private;
using Debug=UnityEngine.Debug;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Holograms {
	public class VirtualDisplay
		:MonoBehaviour
	{
		#region Nested Types
		#endregion Nested Types

		#region Fields

		public HologramDevice device;
		public Canvas canvas;
		public RawImage image;
#if UNITY_EDITOR_WIN
		/// <summary>
		/// <seealso cref="Application.targetFrameRate"/>
		/// </summary>
		public float frameRate=10.0f;
		public Key updateKey;
		public int colorSpace=0;

		[System.NonSerialized]protected float m_Time;
		[System.NonSerialized]protected Texture2D m_Texture;
		[System.NonSerialized]protected int m_Version;
		[System.NonSerialized]protected byte[] m_Buffer;
		
		[System.NonSerialized]protected string m_Path;
		[System.NonSerialized]protected string m_Pipe;
		[System.NonSerialized]protected Process m_Server;

		[System.NonSerialized]protected Thread m_Thread;
		[System.NonSerialized]protected int m_Id;
		[System.NonSerialized]protected NamedPipeClientStream m_Client;
#endif
		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
#if UNITY_EDITOR_WIN
			m_Path=Path.Combine(UnityExtension.s_ExeRoot,"VirtualDisplay.exe");
			if(!File.Exists(m_Path)) {m_Path=null;}
			if(CreateUnityDisplay()) {
				enabled=false;
			}else {
				CreateCSharpDisplay();
				ShortcutManager.instance.Add(name+".Update",UpdateCSharpDisplay,updateKey);

			}
		}

		protected virtual void OnDestroy() {
			DestroyCSharpDisplay();
		}

		protected virtual void Update() {
			if(frameRate>0.0f) {
				float dt=1.0f/frameRate;
				m_Time+=Time.deltaTime;
				if(m_Time>=dt) {
					m_Time-=dt;
					//
					UpdateCSharpDisplay();
				}
			}else {// Use a shortcut.
			}
#else
			enabled=false;
			CreateUnityDisplay();
#endif
		}

		#endregion Unity Messages

		#region Methods

		public virtual bool CreateUnityDisplay() {
			//
			if(device==null) {return false;}device.Init();
			if(device.display<0) {
				if(canvas!=null) {canvas.enabled=false;}return false;
			}
			//
			Display.displays[device.display].Activate();
			if(canvas==null) {
				GameObject go=new GameObject(nameof(VirtualDisplay)+"@"+device.display);
				canvas=go.AddComponent<Canvas>();
				canvas.renderMode=RenderMode.ScreenSpaceOverlay;
				canvas.sortingOrder=1000;
			}
			canvas.targetDisplay=device.display;
			if(image==null) {
				image=canvas.GetComponentInChildren<RawImage>();
				if(image==null) {
					GameObject go=new GameObject("RawImage");
					image=go.AddComponent<RawImage>();
				}
			}
			image.rectTransform.StretchParent(canvas.transform);
			image.texture=device.canvas;
			//
			return true;
		}
#if UNITY_EDITOR_WIN
		public virtual void CreateCSharpDisplay() {
			if(device==null) {return;}
			DestroyCSharpDisplay();
			// Create rendering.
			if(!device.canvas.sRGB) {
				Debug.LogWarning(device.canvas.graphicsFormat+" is not supported.");
				// Fallback.
				var desc=device.canvas.descriptor;desc.graphicsFormat=GraphicsFormat.R8G8B8A8_SRGB;
				device.canvas=RenderTexture.GetTemporary(desc);device.canvas.name=UnityExtension.s_TempTag;
			}
			if(m_Texture==null) {
				m_Texture=new Texture2D(device.resolution.x,device.resolution.y,TextureFormat.BGRA32,false,false);
			}
			// Create IPC.
			m_Pipe="/tmp/yousing/display/"+device.display;
			if(!string.IsNullOrEmpty(m_Path)) {
				m_Server=new Process();
				var s=m_Server.StartInfo;
				s.FileName=m_Path;
				s.Arguments=m_Pipe+" "+device.resolution.x+" "+device.resolution.y;
				m_Server.Start();
			}
			m_Thread=new Thread(OnCSharpDisplay);
			m_Thread.Start();
		}

		public virtual void OnCSharpDisplay() {
			int i=++m_Id,j=m_Version;
			while(m_Id==i) {
				m_Client=new NamedPipeClientStream(".",m_Pipe,PipeDirection.Out);
				m_Client.Connect();
				while(m_Client.IsConnected) {try{
					if(m_Version!=j&&m_Buffer!=null) {j=m_Version;
						m_Client.Write(m_Buffer,0,m_Buffer.Length);
					}
				}catch(System.Exception e) {Debug.Log(e);}}
				m_Client.Close();
			}
		}

		[ContextMenu("Update Display")]
		public virtual void UpdateCSharpDisplay() {
			if(device==null) {return;}
			//
			device.canvas.ToTexture2D(m_Texture);
			m_Buffer=m_Texture.GetRawTextureData();++m_Version;
		}

		public virtual void DestroyCSharpDisplay() {
			++m_Id;
			if(m_Server!=null&&!m_Server.HasExited) {m_Server.Kill();}
			if(m_Thread!=null&&m_Thread.IsAlive) {m_Thread.Abort();}
			if(m_Client!=null&&m_Client.IsConnected) {m_Client.Close();}
			//
			m_Server=null;
			m_Thread=null;
			m_Client=null;
		}
#endif
		#endregion Methods
	}
}
#endif