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

        public Vector2 pos, lastPos;
        public float radius;

        public bool suppressConnectionFires = false;
        public float boltFireChance = 0.3f;
        public int boltFireCounter = 0; //alternative to boltFireChance, a constant counter for bolt fires
        public bool randomBoltPositions = false;
        public bool shortestDistFromHalo = false;
        public float noiseSuppress = 0f;


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

            float connectionsFireChance = suppressConnectionFires ? 0f : boltFireChance;
            bool fireBolt = (boltFireCounter++ % 20 == 0); //constant bolt fire every 0.5s

            for (int i = 0; i < 20; i++)
            {
                if ((UnityEngine.Random.value < connectionsFireChance / 40f || fireBolt) && connectionPos.HasValue)
                {
                    Vector2 center = pos;
                    Vector2 target = connectionPos.Value + Random.insideUnitCircle * 5f;
                    if (randomBoltPositions)
                        target = center + Custom.RNV() * 300f * Random.Range(0.5f, 1f);
                    Vector2 start = center + (shortestDistFromHalo ? Custom.DirVec(center, target) : Custom.DegToVec(Random.value * 360f)) * radius;

                    if (HaloManager.lightningType == Options.LightningTypes.RustyMachine)
                    {
                        LightningBolt obj = new LightningBolt(
                            start, target, Random.Range(0.1f, 0.3f), 
                            Options.whiteLightning.Value ? Color.white : color, color
                        );
                        room.AddObject(obj);
                        if (Options.sound.Value)
                            room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, 0f, (0.7f * (1f - noiseSuppress)), 1f);
                    }
                    if (HaloManager.lightningType == Options.LightningTypes.MoreSlugcats)
                    {
                        var obj = new MoreSlugcats.LightningBolt(start, target, 0, 0.2f)
                        {
                            intensity = 1f,
                            lifeTime = 4f,
                            lightningType = Custom.RGB2HSL(Options.whiteLightning.Value ? Color.white : color).x
                        };
                        room.AddObject(obj);
                        if (Options.sound.Value)
                            room.PlaySound(SoundID.Death_Lightning_Spark_Object, 0f, (0.5f * (1f - noiseSuppress)), 1f);
                    }
                }
                fireBolt = false;
            }

            connectionPos = null; //reset connectionPos
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
