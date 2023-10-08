using UnityEngine;

namespace ThinkRing
{
    public class MoonSigil : UpdatableAndDeletable, IDrawable
    {
        public int moonSigilSprite, fadeSprite, totalSprites = 0;
        public Vector2 pos, lastPos;


        public MoonSigil()
        {
            moonSigilSprite = totalSprites++;
            fadeSprite = totalSprites++;
        }


        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[totalSprites];

            sLeaser.sprites[moonSigilSprite] = new FSprite("MoonSigil", true);
            sLeaser.sprites[moonSigilSprite].shader = rCam.game.rainWorld.Shaders["Hologram"];

            sLeaser.sprites[fadeSprite] = new FSprite("Futile_White", true);
            sLeaser.sprites[fadeSprite].scale = 12.5f;
            sLeaser.sprites[fadeSprite].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];

            this.AddToContainer(sLeaser, rCam, null);
        }


        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[moonSigilSprite].isVisible = !slatedForDeletetion;
            sLeaser.sprites[fadeSprite].isVisible = !slatedForDeletetion;

            Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);

            sLeaser.sprites[moonSigilSprite].color = new Color(0.12156863f, 0.28627452f, 0.48235294f);
            sLeaser.sprites[moonSigilSprite].x = vector.x - camPos.x;
            sLeaser.sprites[moonSigilSprite].x = vector.y + 12f - camPos.y;
            sLeaser.sprites[moonSigilSprite].alpha = 1f;

            sLeaser.sprites[fadeSprite].color = new Color(0f, 0f, 1f);
            sLeaser.sprites[fadeSprite].x = vector.x - camPos.x;
            sLeaser.sprites[fadeSprite].x = vector.y - camPos.y;
            sLeaser.sprites[fadeSprite].alpha = 0.2f;
        }


        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            sLeaser.RemoveAllSpritesFromContainer();
            rCam.ReturnFContainer("BackgroundShortcuts").AddChild(sLeaser.sprites[moonSigilSprite]);
            rCam.ReturnFContainer("Shortcuts").AddChild(sLeaser.sprites[fadeSprite]);
        }


        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }
    }
}
