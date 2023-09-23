using UnityEngine;
using RWCustom;

namespace ThinkRing
{
    public class BaseHalo : UpdatableAndDeletable, IDrawable
    {
        public GenericBodyPart owner; //determines position of halo
        public int firstSprite = 0;
        public int totalSprites;
        public Vector2? connectionPos = null; //if not null, connections will fire
        public Color color = Color.white;
        public Color prevColor;


        public BaseHalo(GenericBodyPart owner)
        {
            this.owner = owner;

            if (HaloManager.colorType == Options.ColorTypes.Static)
                color = Options.staticHaloColor.Value;
            if (HaloManager.colorType == Options.ColorTypes.CharacterDarker && owner?.owner is PlayerGraphics) {
                color = PlayerGraphics.SlugcatColor((owner.owner as PlayerGraphics).CharacterForColor);
                color *= 0.326797f; //100% / 153 (Psychic) * 50 (Halo)
                color.a = 1f;
            }
            if (HaloManager.colorType == Options.ColorTypes.Character && owner?.owner is PlayerGraphics)
                color = PlayerGraphics.SlugcatColor((owner.owner as PlayerGraphics).CharacterForColor);
            if (HaloManager.colorType == Options.ColorTypes.RGB)
                color = Color.red; //start color
            prevColor = color;
        }


        public override void Update(bool eu)
        {
            base.Update(eu);

            //rgb cycle color type
            prevColor = color;
            if (HaloManager.colorType == Options.ColorTypes.RGB) {
                Vector3 HSL = Custom.RGB2HSL(color);
                HSL.x += Options.rgbCycleSpeed.Value / 100000f;
                if (HSL.x > 1f)
                    HSL.x = 0f;
                color = Custom.HSL2RGB(HSL.x, HSL.y, HSL.z);
            }
        }


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
