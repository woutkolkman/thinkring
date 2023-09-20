using UnityEngine;
using RWCustom;

namespace ThinkRing
{
    public class BaseHalo : UpdatableAndDeletable, IDrawable
    {
        public int firstSprite = 0;
        public int totalSprites;


        //added funtion to support interface IDrawable
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            sLeaser.RemoveAllSpritesFromContainer();
            if (newContainer == null)
                newContainer = rCam.ReturnFContainer("BackgroundShortcuts");
            for (int i = 0; i < this.totalSprites; i++)
                newContainer.AddChild(sLeaser.sprites[i]);
        }


        //added funtion to support interface IDrawable
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }


        //added funtion to support interface IDrawable
        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
        }


        //added funtion to support interface IDrawable
        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
        }
    }
}
