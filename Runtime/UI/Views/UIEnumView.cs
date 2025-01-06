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

		[System.NonSerialized]public int start;
		[System.NonSerialized]protected bool m_Dirty;

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			start=index;
			//
			var sm=ShortcutManager.instance;var tg=GetComponent<ToggleGroup>();
			var op=(m_Dropdowns?.Length??0)>0?m_Dropdowns[0].options:null;
			if(op!=null) {op.Clear();}string key=name+".";
			for(int i=0,imax=keys?.Length??0,icnt=m_Toggles?.Length??0;i<imax;++i) {
				int n=i;T v=values[i];string s=v.ToString();
				sm.Add(key+s,()=>OnIndexChanged(n),ShortcutManager.GetKeys(keys[i],modifiers));
				if(string.IsNullOrEmpty(GetString(i))) {s=s.Tr();}else {s=GetString(i);}
				SetText(i,s);BindToggle(i,(x)=>OnToggleChanged(n,x));
				//
				if(op!=null) {op.Add(new Dropdown.OptionData(s,GetSprite(i)));}
				if(i<icnt&&m_Toggles[i]!=null) {m_Toggles[i].group=tg;}
			}
			BindDropdown(0,OnIndexChanged);
		}

		protected virtual void OnEnable() {
			string key=name;
			if(!key.StartsWith('.')) {OnIndexChanged(PlayerPrefs.GetInt(key,index));}
		}

		protected virtual void Update() {
			if(m_Dirty) {
				m_Dirty=false;
				//
				for(int i=0,imax=keys?.Length??0;i<imax;++i) {
					if(GetToggle(i)) {OnIndexChanged(i);return;}
				}
			}
		}

		#endregion Unity Messages

		#region Methods

		protected virtual void OnToggleChanged(int i,bool b) {
			if(isActiveAndEnabled) {
				m_Dirty=true;
			}else if(b) {
				OnIndexChanged(i);
			}
		}

		protected virtual void OnIndexChanged(int i)=>OnValueChanged(values[index=i]);
		public virtual T value=>index>=0?values[index]:default;

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
				if(b) {index=i;
					SetDropdownWithoutNotify(0,index);
					//
					string key=name;
					if(!key.StartsWith('.')) {PlayerPrefs.SetInt(name,index);}
				}
			}
		}

		#endregion Methods
	}
}
