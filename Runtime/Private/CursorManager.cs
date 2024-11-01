// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Runtime/Private/Private.ms")

using System.Collections.Generic;
using System.IO;
using UnityEngine;using YouSingStudio.Holograms;
using UnityEngine.Events;


namespace YouSingStudio.Private {
	public class CursorManager
		:MonoSingleton<CursorManager>
	{
		#region Nested Types

		[System.Serializable]// [Record] in one line.
		public class Cursor {
			public string name;
			public string path;
			public Texture2D texture;
			public Vector2 hotspot;
		}

		#endregion Nested Types

		#region Fields

		public string settings;
		[Header("Cursor")]
		public CursorMode mode;
		/// <summary>
		/// <seealso cref="UnityEngine.Cursor"/>
		/// </summary>
		public string cursor;
		public List<Cursor> cursors;
		/// <summary>
		/// <seealso cref="GUIContent.tooltip"/>
		/// </summary>
		[Header("Tooltip")]
		public string tooltip;
		[System.NonSerialized]public System.Action<string> onTooltip=null;
		[SerializeField]protected UnityEvent<string> m_OnTooltip=null;

		[System.NonSerialized]protected bool m_IsInited;
		[System.NonSerialized]protected string m_Cursor;
		[System.NonSerialized]protected string m_Tooltip;

		#endregion Fields

		#region Unity Messages

		protected virtual void OnEnable() {
			m_Cursor=m_Tooltip="$(InvalidKey)";
		}

		protected virtual void OnDisable() {
			Clear();
		}

		protected virtual void LateUpdate() {
			Render();
		}

		#endregion Unity Messages

		#region Methods

		public static Texture2D LoadTexture(string path,Texture2D texture) {
			Texture2D tmp=null;//IO.TextureAPI.Lo(path);
			return tmp!=null?tmp:texture;
		}

		protected virtual void Init() {
			if(m_IsInited) {return;}
			m_IsInited=true;
			//
			if(!string.IsNullOrEmpty(settings)&&File.Exists(settings)) {
				JsonUtility.FromJsonOverwrite(File.ReadAllText(settings),this);
			}
			Cursor it;for(int i=0,imax=cursors.Count;i<imax;++i) {
				it=cursors[i];if(it!=null&&!string.IsNullOrEmpty(it.path)) {
					it.texture=LoadTexture(it.path,it.texture);
				}
			}
		}

		public virtual Cursor GetCursor(string key) {
			if(!m_IsInited) {Init();}
			//
			Cursor it;for(int i=0,imax=cursors.Count;i<imax;++i) {
				it=cursors[i];if(it!=null&&string.Equals(it.name,key,System.StringComparison.OrdinalIgnoreCase)) {return it;}
			}
			return cursors[0];
		}

		public virtual void Render() {
			if(!m_IsInited) {Init();}
			//
			if(cursor!=m_Cursor) {
				m_Cursor=cursor;
				//
				Cursor c=GetCursor(m_Cursor);
				UnityEngine.Cursor.SetCursor(texture:c.texture,hotspot:c.hotspot,mode);
			}
			if(tooltip!=m_Tooltip) {
				m_Tooltip=tooltip;
				//
				onTooltip?.Invoke(m_Tooltip);
				m_OnTooltip?.Invoke(m_Tooltip);
			}
		}

		/// <summary>
		/// Clear instantly.
		/// </summary>
		public virtual void Clear() {
			if(!m_IsInited) {Init();}
			//
			string c=cursor,t=tooltip;
				cursor=tooltip=null;
				Render();
			cursor=c;tooltip=t;
		}

		#endregion Methods
	}
}
