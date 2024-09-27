using UnityEngine;
using UnityEngine.UI;
using YouSingStudio.Private;
using Key=UnityEngine.KeyCode;

namespace YouSingStudio.Holograms {
	/// <summary>
	/// <seealso cref="System.Enum"/>
	/// </summary>
	public class UIEnumView<T>
		:ScriptableView<T>
	{
		#region Fields

		public static UIEnumView<T> current;

		public Key[] modifiers;
		public int index;
		public Key[] keys;
		public T[] values;

		[System.NonSerialized]protected bool m_Dirty;

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			var sm=ShortcutManager.instance;
			var tg=GetComponent<ToggleGroup>();
			var op=(m_Dropdowns?.Length??0)>0?m_Dropdowns[0].options:null;
			if(op!=null) {op.Clear();}
			for(int i=0,imax=keys?.Length??0,icnt=m_Toggles?.Length??0;i<imax;++i) {
				int n=i;T v=values[i];string s=v.ToString();
				sm.Add(name+"."+s,()=>OnValueChanged(v),GetKeys(keys[i]));
				if(string.IsNullOrEmpty(GetString(i))) {s=s.Tr();}else {s=GetString(i);}
				SetText(i,s);BindToggle(i,(x)=>OnToggleChanged(n,x));
				//
				if(op!=null) {op.Add(new Dropdown.OptionData(s,GetSprite(i)));}
				if(i<icnt&&m_Toggles[i]!=null) {m_Toggles[i].group=tg;}
			}
			BindDropdown(0,OnDropdownChanged);
		}
		
		protected virtual void OnEnable() {
			OnValueChanged(values[PlayerPrefs.GetInt(name,index)]);
		}

		protected virtual void Update() {
			if(m_Dirty) {
				m_Dirty=false;
				//
				for(int i=0,imax=keys?.Length??0;i<imax;++i) {
					if(GetToggle(i)) {OnValueChanged(values[i]);return;}
				}
			}
		}

		#endregion Unity Messages

		#region Methods

		protected virtual Key[] GetKeys(Key key) {
			if(key!=Key.None) {
				int i=0,imax=modifiers?.Length??0;Key[] tmp=new Key[imax+1];
				for(;i<imax;++i) {tmp[i]=modifiers[i];}tmp[i]=key;return tmp;
			}else {return null;}
		}

		protected virtual void OnToggleChanged(int i,bool b) {
			if(isActiveAndEnabled) {
				m_Dirty=true;
			}else if(b) {
				OnValueChanged(values[i]);
			}
		}

		protected virtual void OnDropdownChanged(int i)=>OnValueChanged(values[i]);

		public override void OnValueChanged(T value) {
			var tmp=current;current=this;
				base.OnValueChanged(value);
				SetValueWithoutNotify(value);
			current=tmp;
		}

		public virtual void SetValueWithoutNotify(T value) {
			bool b;for(int i=0,imax=keys?.Length??0;i<imax;++i) {
				b=object.Equals(values[i],value);
				SetToggleWithoutNotify(i,b);
				if(b) {
					SetDropdownWithoutNotify(0,i);
					PlayerPrefs.SetInt(name,i);
				}
			}
		}

		#endregion Methods
	}
}
