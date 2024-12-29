using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace YouSingStudio.Holograms {
	public class UISnapshotDirector
		:Private.SnapshotManager
	{
		#region Fields

		[Header("UI")]
		public Text text;
		public InputField field;
		public Button[] buttons;
		public UIPopupView popup;
		public UISelectorView selector;

		[System.NonSerialized]protected bool m_Popup;
		[System.NonSerialized]protected string m_Key;
		[System.NonSerialized]protected GameObject m_FieldV;
		[System.NonSerialized]protected GameObject[] m_ButtonsV;

		#endregion Fields

		#region Unity Messages

		protected override void Awake() {
			m_Path=UnityExtension.GetFullPath(m_Path);
			//
			base.Awake();
			UnityExtension.SetListener<TextureType>(OnTypeChanged,true);
			UnityExtension.SetListener<ModelLoader>(OnModelLoaded,true);
			//
			if(popup==null) {popup=GetComponentInChildren<UIPopupView>();}
			if(selector==null) {selector=GetComponentInChildren<UISelectorView>();}
			//
			if(popup!=null) {popup.onActive+=OnPopupChanged;}
			if(selector!=null) {selector.onSelect+=SelectSnapshot;}
			//
			int i=0,imax=buttons?.Length??0;
			m_ButtonsV=new GameObject[imax];
			if(i<imax) {SetButton(i,SaveSnapshot);}++i;
			if(i<imax) {SetButton(i,LoadSnapshot);}++i;
			if(i<imax) {SetButton(i,AddSnapshot);}++i;
			if(i<imax) {SetButton(i,RenameSnapshot);}++i;
			if(i<imax) {SetButton(i,RemoveSnapshot);}++i;
			if(i<imax) {SetButton(i,ResetScene);}++i;
			if(i<imax) {SetButton(i,AddSnapshot);}++i;
			if(field!=null) {
				m_FieldV=field.gameObject;
				field.onSubmit.AddListener(OnSnapshotRenamed);
			}
		}

		protected override void OnDestroy() {
			base.OnDestroy();
			UnityExtension.SetListener<TextureType>(OnTypeChanged,false);
			UnityExtension.SetListener<ModelLoader>(OnModelLoaded,false);
			//
		}

		#endregion Unity Messages

		#region Methods

		public virtual void SetButton(int index,UnityAction action) {
			var tmp=buttons[index];if(tmp!=null) {
				m_ButtonsV[index]=tmp.gameObject;
				tmp.onClick.AddListener(action);
			}
		}

		protected virtual void SetButton(int button) {
			GameObject it;int i=0,imax=m_ButtonsV?.Length??0;
			for(;i<imax;++i) {
				it=m_ButtonsV[i];if(it!=null) {it.SetActive((button&(1<<i))!=0);}
			}
		}

		public override void AddSnapshot() {
			scene.path=m_Key;
			//
			m_Popup=popup.GetActive();
				base.AddSnapshot();
			m_Popup=false;
		}

		public override void RemoveSnapshot() {
			//
			m_Popup=popup.GetActive();
				base.RemoveSnapshot();
			m_Popup=false;
		}

		public override void SelectSnapshot(int index) {
			scene.index=-1;
			base.SelectSnapshot(index);
		}

		protected virtual void RenameSnapshot() {
			if(text!=null) {
				text.text=null;
			}
			if(field!=null) {
				field.SetTextWithoutNotify(text.text);
				m_FieldV.SetActive(true);
			}
			//
			var sm=Private.ShortcutManager.s_Instance;
			if(sm!=null) {sm.Lock(this);}
		}

		protected virtual void UpdateText() {
			if(field!=null) {m_FieldV.SetActive(false);}
			if(text!=null) {text.text=snapshot!=null?snapshot.name:"None";}
			//
			var sm=Private.ShortcutManager.s_Instance;
			if(sm!=null) {sm.Unlock(this);}
		}

		// Events

		protected virtual void OnTypeChanged(TextureType type) {
			OnSceneLoaded(null);
			SetButton(0x0);
		}

		protected virtual void OnModelLoaded(ModelLoader model) {
			if(model==null) {return;}
			var m=model.GetManifest();
			//
			m_Key=m?.context?.path??null;
			if(string.IsNullOrEmpty(m_Key)) {m_Key="$(Unknown)";}
			else {m_Key=Path.GetFileName(Path.GetDirectoryName(m_Key))+"/"+Path.GetFileName(m_Key);}// Two short names.
			//
			int i=IndexOfScene(m_Key);
			OnSceneLoaded(i>=0?scenes[i]:null);
		}

		protected virtual void OnPopupChanged(bool value) {
			if(value) {
				if(selector!=null) {selector.Highlight(scene.index);}
			}else {
				UpdateText();
			}
		}

		protected virtual void OnSnapshotRenamed(string value) {
			snapshot.name=value;
			//
			var es=UnityEngine.EventSystems.EventSystem.current;
			if(es!=null) {es.SetSelectedGameObject(null);}
			//
			m_Popup=popup.GetActive();
				OnSceneLoaded(scene);
			m_Popup=false;
		}

		protected override void OnSceneLoaded(Scene value) {
			if(value==null) {
				scene=m_Scene;
				//
				if(popup!=null) {popup.SetButton(false);}
				OnSnapshotSelected(-1);
			}else {
				scene=value;
				//
				int i=0,imax=scene.snapshots?.Count??0;
				using(ListPool<string>.Get(out var list)) {
					for(;i<imax;++i) {
						list.Add(scene.snapshots[i].name);
					}
					selector.Render(list);
				}
				if(popup!=null&&!m_Popup) {popup.SetButton(imax>0);}
				OnSnapshotSelected(imax>0?scene.index:-1);
			}
		}

		protected override void OnSnapshotSelected(int index) {
			scene.index=index;
			//
			if(scene.index<0) {
				snapshot=null;
				UpdateText();
				SetButton(0x4);
			}else {
				snapshot=scene.snapshots[scene.index];
				UpdateText();
				SetButton(0xFF&~0x4);
				//
				if(selector!=null&&(popup==null||popup.GetActive())) {selector.Highlight(scene.index);}// TODO: ToExtension????
				LoadSnapshot();
			}
		}

		#endregion Methods
	}
}
