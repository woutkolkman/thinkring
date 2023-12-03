using UnityEngine;

namespace ThinkRing
{
    public class NoneHalo : BaseHalo
    {
        public NoneHalo(BodyPart owner) : base(owner)
        {
        }


        public override void Update(bool eu)
        {
            //destroy and return if owner is deleted or moves to another room
            if (owner?.owner?.owner?.slatedForDeletetion != false || 
                this.room != owner.owner?.owner?.room || !HaloManager.cosmeticIsActive || 
                (HaloManager.activeType == Options.ActivateTypes.Dragging && connectionPos == null) || //remove halo when not dragging
                (HaloManager.activeType == Options.ActivateTypes.ToolsActive && !MouseDrag.State.activated)) //remove halo when mousedrag is not active
            {
                this.Destroy();
                this.room?.RemoveObject(this);
                this.RemoveFromRoom();
                return;
            }

            this.lastPos = this.pos;
            pos = (overridePos ?? owner.pos) + new Vector2(0f, Options.haloOffset.Value ? 100f : 0f);
            if (HaloManager.colorType == Options.ColorTypes.Black)
                pos += new Vector2(-7.5f, 15.5f); //adjust to background
            radius = 0f;
            base.Update(eu); //lightning bolts and color cycle
        }
    }
}
