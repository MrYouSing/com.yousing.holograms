using UnityEngine;

namespace YouSingStudio.Holograms {
	public class ScreenDevice
		:HologramDevice
	{
		#region Fields

		public bool quiltFlip=false;
		public int quiltIndex=-1;
		public int[] quiltIndexes;

		public System.Action onDefaultChanged=null;
		[System.NonSerialized]protected int m_DefaultIndex=-1;

		#endregion Fields

		#region Unity Messages
		#endregion Unity Messages

		#region Methods

		public virtual int GetIndex() {
			int i=quiltIndexes?.Length??0;
			if(i<=0) {
				if(quiltIndex<0) {
					if(m_DefaultIndex>=0) {i=m_DefaultIndex;}
					else {i=(quiltSize.x*quiltSize.y).GetMiddle();}
				}else {
					i=quiltIndex;
				}
			}else {
				i=quiltIndex>=0?Mathf.Clamp(quiltIndex,0,i-1):(i.GetMiddle());
				i=quiltIndexes[i];
			}
			return i;
		}

		public virtual void SetDefaultIndex(int value) {
			if(value==m_DefaultIndex) {return;}
			//
			m_DefaultIndex=value;
			onDefaultChanged?.Invoke();
		}

		protected override void InternalRender() {
			//
			int x=quiltSize.x,y=quiltSize.y;
			float w=1.0f/x,h=1.0f/y;
			int i=GetIndex();
			//
			i=Mathf.Clamp(i,0,x*y-1);
			y=quiltFlip?(y-1-i/x):(i/x);x=i%x;
			Graphics.Blit(quiltTexture,canvas,new Vector2(w,h),new Vector2(x*w,y*h));
		}

		public override void Screenshot(string path,int mask=-1) {
			if(canvas==null) {return;}
			// TODO: ColorFormat
			Texture2D tex=RenderingExtension.NewTexture2D(canvas.width,canvas.height);
				canvas.ToTexture2D(tex).SaveFile(path);
			tex.Free();
		}

		#endregion Methods
	}
}
