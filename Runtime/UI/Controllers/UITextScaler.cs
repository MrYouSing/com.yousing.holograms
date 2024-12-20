using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouSingStudio.Holograms {
	public class UITextScaler
		:MonoBehaviour
	{
		#region Fields

		public float scale=1.0f;
		public Transform root;
		public List<Text> texts=new List<Text>();

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(root==null) {
				Canvas c=GetComponentInParent<Canvas>();
				if(c!=null) {root=c.transform;}
				else {root=transform;}
			}
			//
			if(texts.Count<=0) {GetComponentsInChildren(true,texts);}
			texts.ForEach(Scale);
		}

		#endregion Unity Messages

		#region Methods

		public virtual void Scale(Text text) {
			if(text==null) {return;}
			//
			RectTransform t=text.transform as RectTransform;
			Vector3 p=root.lossyScale;Vector3 s=t.lossyScale;
			float f=(s.x/p.x+s.y/p.y)*0.5f;f=scale*f;// RenderScale->FinalScale
			text.fontSize=Mathf.RoundToInt(text.fontSize*f);
			//
			Rect r=t.rect;s=t.localScale;
			t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,r.width*f);
			t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,r.height*f);
			f=1.0f/f;t.localScale=new Vector3(f,f,s.z);
		}

		#endregion Methods
	}
}