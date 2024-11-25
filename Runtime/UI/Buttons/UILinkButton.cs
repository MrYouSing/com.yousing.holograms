using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YouSingStudio.Holograms {
	public class UILinkButton
		:MonoBehaviour
		,IPointerClickHandler
	{
		#region Fields

		public static Dictionary<string,string> s_Links=null;

		public string url;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(s_Links==null) {
				s_Links=new Dictionary<string,string>();
				s_Links.LoadSettings("Links");
			}
		}

		public virtual void OnPointerClick(PointerEventData eventData) {
			if(!s_Links.TryGetValue(name,out var tmp)||string.IsNullOrEmpty(tmp)) {
				tmp=url;
			}
			if(!string.IsNullOrEmpty(tmp)) {Application.OpenURL(tmp);}
		}

		#endregion Unity Messages
	}
}