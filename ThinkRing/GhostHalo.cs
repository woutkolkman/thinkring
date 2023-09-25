using UnityEngine;
using RWCustom;

namespace ThinkRing
{
    public class GhostHalo : BaseHalo
    {
        public float prevRadius = 0f;
        public float maxRadius = 80f;
        public float growRate = 2f;


        public GhostHalo(GenericBodyPart owner) : base(owner)
        {
        }


        public override void Update(bool eu)
        {
            prevRadius = radius;

            //destroy and return if owner is deleted or moves to another room
            if (owner?.owner?.owner?.slatedForDeletetion != false || 
                this.room != owner.owner?.owner?.room || 
                (HaloManager.activeType == Options.ActivateTypes.Dragging && connectionPos == null) || //remove halo when not dragging
                (HaloManager.activeType == Options.ActivateTypes.ToolsActive && !MouseDrag.State.activated)) //remove halo when mousedrag is not active
            {
                if (radius > 0f) {
                    radius -= growRate;
                } else {
                    this.Destroy();
                    this.room?.RemoveObject(this);
                    this.RemoveFromRoom();
                    return;
                }
            } else {
                if (radius < maxRadius)
                    radius += growRate;
            }
            radius = Mathf.Clamp(radius, 0f, maxRadius);

            this.lastPos = this.pos;
            pos = owner.pos;
            base.Update(eu); //lightning bolts and color cycle
        }


        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["GhostDistortion"];
            this.AddToContainer(sLeaser, rCam, null);
        }


        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            sLeaser.sprites[0].color = color;
            sLeaser.sprites[0].isVisible = !slatedForDeletetion;
            sLeaser.sprites[0].scale = Mathf.Lerp(prevRadius, radius, timeStacker) / 6f;
            Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
            sLeaser.sprites[0].x = vector.x - camPos.x;
            sLeaser.sprites[0].y = vector.y - camPos.y;
        }


        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[0]);
        }
    }
}
