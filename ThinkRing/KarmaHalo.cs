using UnityEngine;
using RWCustom;

namespace ThinkRing
{
    public class KarmaHalo : BaseHalo
    {
        public float prevRadius, maxRadius;
        public float growRate = 2f;
        const float spriteRadius = 134.5f; //size of sprite is 269x269


        public KarmaHalo(BodyPart owner) : base(owner)
        {
            maxRadius = Mathf.Lerp(50f, spriteRadius, (Options.maxRings.Value - 2) / 2f);
            totalSprites = 1;
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
            pos = owner.pos + new Vector2(0f, Options.haloOffset.Value ? 100f : 0f);
            base.Update(eu); //lightning bolts and color cycle
        }


        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("sprites" + System.IO.Path.DirectorySeparatorChar + "thinkring_karma", true);
            //sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["LevelMelt2"];
            this.AddToContainer(sLeaser, rCam, null);
        }


        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].color = color;
            sLeaser.sprites[0].isVisible = !slatedForDeletetion;
            sLeaser.sprites[0].scale = Mathf.Lerp(prevRadius, radius, timeStacker) / spriteRadius;
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
