using UnityEngine;

namespace YouSingStudio.Holograms {
	public class ScreenDevice
		:HologramDevice
	{
		#region Fields

		public bool quiltFlip=false;
		public int quiltIndex=-1;
		public int[] quiltIndexes;

		#endregion Fields

		#region Unity Messages
		#endregion Unity Messages

		#region Methods

		public virtual int GetIndex() {
			int i=quiltIndexes?.Length??0;
			if(i<=0) {
				i=quiltIndex>=0?quiltIndex:(quiltSize.x*quiltSize.y/2-1);
			}else {
				i=quiltIndex>=0?Mathf.Clamp(quiltIndex,0,i-1):(i/2-1);
				i=quiltIndexes[i];
			}
			return i;
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

		#endregion Methods
	}
}
