using UnityEngine;
using RWCustom;

namespace ThinkRing
{
    //basically a tweaked copy from the game (OracleGraphics.Halo)
    public class OracleHalo : UpdatableAndDeletable, IDrawable
    {
        public GenericBodyPart owner; //determines position of halo
        public int firstSprite;
        public int totalSprites;
        public int firstBitSprite;
        public Connection[] connections;
        public MemoryBit[][] bits;
        public float[,] ringRotations;
        public float expand;
        public float lastExpand;
        public float getToExpand;
        public float push;
        public float lastPush;
        public float getToPush;
        public float white;
        public float lastWhite;
        public float getToWhite;

        //added to original
        public bool visibility = true;
        public bool suppressConnectionFires = false;
        public float noiseSuppress = 0f;
        public float size = 0f;
        public Vector2? connectionPos = null; //if not null, connections will fire
        public float boltFireChance = 0.3f;
        public int boltFireCounter = 0; //alternative to boltFireChance, a constant counter for bolt fires
        public Color color = Color.white;
        public bool randomBoltPositions = false;
        public bool shortestDistFromHalo = false;


        public OracleHalo(GenericBodyPart owner)
        {
            this.owner = owner;
            this.firstSprite = 0;
            this.totalSprites = 2;
            this.connections = new Connection[20];
            this.totalSprites += this.connections.Length;
            for (int i = 0; i < this.connections.Length; i++)
                this.connections[i] = new Connection(this);
            this.firstBitSprite = firstSprite + this.totalSprites;
            this.bits = new MemoryBit[3][];
            this.bits[0] = new MemoryBit[10];
            this.bits[1] = new MemoryBit[30];
            this.bits[2] = new MemoryBit[60];
            for (int j = 0; j < this.bits.Length; j++)
                for (int k = 0; k < this.bits[j].Length; k++)
                    this.bits[j][k] = new MemoryBit(this, new IntVector2(j, k));
            this.totalSprites += 100;
            this.ringRotations = new float[10, 5];
            this.expand = 1f;
            this.getToExpand = 1f;

            //added color options
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
        }


        public override void Update(bool eu)
        {
            base.Update(eu);

            //destroy and return if owner is deleted or moves to another room
            if (owner.owner?.owner?.slatedForDeletetion != false || 
                this.room != owner.owner?.owner?.room || 
                (HaloManager.activeType == Options.ActivateTypes.Dragging && connectionPos == null) || //remove halo when not dragging
                (HaloManager.activeType == Options.ActivateTypes.ToolsActive && !MouseDrag.State.activated)) //remove halo when mousedrag is not active
            {
                if (size > 0f) {
                    size -= 1f / 40f; //gradually get smaller (1s)
                } else {
                    this.Destroy();
                    this.visibility = false;
                    this.room?.RemoveObject(this);
                    this.RemoveFromRoom();
                    return;
                }
            } else if (size < 1f) {
                size += 1f / 40f; //gradually get larger (1s)
            }
            size = Mathf.Clamp(size, 0f, 1f); //keep size a value from 0f to 1f

            //============================================== Original Code ================================================

            //edited connectionsFireChance
            float connectionsFireChance = suppressConnectionFires ? 0f : boltFireChance;

            //added contant bolt fire every 0.5s
            bool fireBolt = (boltFireCounter++ % 20 == 0);

            for (int i = 0; i < this.connections.Length; i++)
            {
                this.connections[i].lastLightUp = this.connections[i].lightUp;
                this.connections[i].lightUp *= 0.7f;
                if ((UnityEngine.Random.value < connectionsFireChance / 40f || fireBolt) 
                    && visibility && connectionPos.HasValue)
                {
                    Vector2 center = this.Center(0f);
                    Vector2 target = connectionPos.Value + Random.insideUnitCircle * 5f;
                    if (randomBoltPositions)
                        target = center + Custom.RNV() * 300f * Random.Range(0.5f, 1f);
                    Vector2 start = center + (shortestDistFromHalo ? Custom.DirVec(center, target) : Custom.DegToVec(Random.value * 360f)) * Radius(2f, 0f) * size;

                    if (HaloManager.lightningType == Options.LightningTypes.Oracle) {
                        //start position is calculated when drawing lightning
                        connections[i].SetStuckAt(target); //added to connect bolt to grabbed object
                        connections[i].lightUp = 1f;
                        if (Options.sound.Value)
                            room.PlaySound(SoundID.SS_AI_Halo_Connection_Light_Up, 0f, (1.5f * (1f - noiseSuppress)), 1f);
                    }
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

            for (int j = 0; j < this.ringRotations.GetLength(0); j++)
            {
                this.ringRotations[j, 1] = this.ringRotations[j, 0];
                if (this.ringRotations[j, 0] != this.ringRotations[j, 3])
                {
                    this.ringRotations[j, 4] += 1f / Mathf.Lerp(20f, Mathf.Abs(this.ringRotations[j, 2] - this.ringRotations[j, 3]), 0.5f);
                    this.ringRotations[j, 0] = Mathf.Lerp(this.ringRotations[j, 2], this.ringRotations[j, 3], Custom.SCurve(this.ringRotations[j, 4], 0.5f));
                    if (this.ringRotations[j, 4] > 1f)
                    {
                        this.ringRotations[j, 4] = 0f;
                        this.ringRotations[j, 2] = this.ringRotations[j, 3];
                        this.ringRotations[j, 0] = this.ringRotations[j, 3];
                    }
                }
                else if (UnityEngine.Random.value < 1f/30f)
                {
                    this.ringRotations[j, 3] = this.ringRotations[j, 0] + ((UnityEngine.Random.value < 0.5f) ? -1f : 1f) * Mathf.Lerp(15f, 150f, UnityEngine.Random.value);
                }
            }
            for (int k = 0; k < this.bits.Length; k++)
                for (int l = 0; l < this.bits[k].Length; l++)
                    this.bits[k][l].Update();
            if (UnityEngine.Random.value < 1f/60f && this.bits.Length != 0)
            {
                int num = UnityEngine.Random.Range(0, this.bits.Length);
                for (int m = 0; m < this.bits[num].Length; m++)
                    this.bits[num][m].SetToMax();
            }
            this.lastExpand = this.expand;
            this.lastPush = this.push;
            this.lastWhite = this.white;
            this.expand = Custom.LerpAndTick(this.expand, this.getToExpand, 0.05f, 0.0125f);
            this.push = Custom.LerpAndTick(this.push, this.getToPush, 0.02f, 0.025f);
            this.white = Custom.LerpAndTick(this.white, this.getToWhite, 0.07f, 0.022727273f);
            bool flag = false;
            if (UnityEngine.Random.value < 0.00625f)
            {
                if (UnityEngine.Random.value < 0.125f)
                {
                    flag = (this.getToWhite < 1f);
                    this.getToWhite = 1f;
                }
                else
                {
                    this.getToWhite = 0f;
                }
            }
            if (UnityEngine.Random.value < 0.00625f || flag)
                this.getToExpand = ((UnityEngine.Random.value < 0.5f && !flag) ? 1f : Mathf.Lerp(0.8f, 2f, Mathf.Pow(UnityEngine.Random.value, 1.5f)));
            if (UnityEngine.Random.value < 0.00625f || flag)
                this.getToPush = ((UnityEngine.Random.value < 0.5f && !flag) ? 0f : ((float)(-1 + UnityEngine.Random.Range(0, UnityEngine.Random.Range(1, 6)))));
        }


        public void ChangeAllRadi()
        {
            this.getToExpand = Mathf.Lerp(0.8f, 2f, Mathf.Pow(UnityEngine.Random.value, 1.5f));
            this.getToPush = (float)(-1 + UnityEngine.Random.Range(0, UnityEngine.Random.Range(1, 6)));
        }


        public float Radius(float ring, float timeStacker)
        {
            return (3f + ring + Mathf.Lerp(this.lastPush, this.push, timeStacker) - 0.5f * size) * Mathf.Lerp(this.lastExpand, this.expand, timeStacker) * 7f;
        }


        public float Rotation(int ring, float timeStacker)
        {
            return Mathf.Lerp(this.ringRotations[ring, 1], this.ringRotations[ring, 0], timeStacker);
        }


        public Vector2 Center(float timeStacker)
        {
            //edited to return exact head position instead of slightly above head
            return Vector2.Lerp(this.owner.lastPos, this.owner.pos, timeStacker);
        }


        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[this.totalSprites]; //added initializer, because oracle sprite leaser does not exist
            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[this.firstSprite + i] = new FSprite("Futile_White", true);
                sLeaser.sprites[this.firstSprite + i].shader = rCam.game.rainWorld.Shaders["VectorCircle"];
                sLeaser.sprites[this.firstSprite + i].color = color;
            }
            for (int j = 0; j < this.connections.Length; j++)
            {
                sLeaser.sprites[this.firstSprite + 2 + j] = TriangleMesh.MakeLongMesh(20, false, false);
                sLeaser.sprites[this.firstSprite + 2 + j].color = Options.whiteLightning.Value ? Color.white : color;
            }
            for (int k = 0; k < 100; k++)
            {
                sLeaser.sprites[this.firstBitSprite + k] = new FSprite("pixel", true);
                sLeaser.sprites[this.firstBitSprite + k].scaleX = 2f; //smaller width of bits, so they are visibly separated
                sLeaser.sprites[this.firstBitSprite + k].color = color;
            }
            this.AddToContainer(sLeaser, rCam, null); //added
        }


        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            //rgb cycle color type
            if (HaloManager.colorType == Options.ColorTypes.RGB) {
                Vector3 HSL = Custom.RGB2HSL(color);
                HSL.x += Options.rgbCycleSpeed.Value / 100000f;
                if (HSL.x > 1f)
                    HSL.x = 0f;
                color = Custom.HSL2RGB(HSL.x, HSL.y, HSL.z);
            }

            if (sLeaser.sprites[this.firstSprite].isVisible != visibility)
            {
                for (int i = 0; i < 2 + this.connections.Length; i++)
                    sLeaser.sprites[this.firstSprite + i].isVisible = visibility;
                for (int j = 0; j < 100; j++)
                    sLeaser.sprites[this.firstBitSprite + j].isVisible = visibility;
            }
            Vector2 center = this.Center(timeStacker);
            for (int k = 0; k < 2; k++)
            {
                sLeaser.sprites[this.firstSprite + k].x = center.x - camPos.x;
                sLeaser.sprites[this.firstSprite + k].y = center.y - camPos.y;
                sLeaser.sprites[this.firstSprite + k].scale = this.Radius((float)k, timeStacker) / 8f * size; //added size
                sLeaser.sprites[this.firstSprite + k].color = color; //added color assignment
            }
            sLeaser.sprites[this.firstSprite].alpha = Mathf.Lerp(3f / this.Radius(0f, timeStacker), 1f, Mathf.Lerp(this.lastWhite, this.white, timeStacker));
            sLeaser.sprites[this.firstSprite + 1].alpha = 3f / this.Radius(1f, timeStacker);
            for (int l = 0; l < this.connections.Length; l++)
            {
                if ((this.connections[l].lastLightUp > 0.05f || this.connections[l].lightUp > 0.05f) && visibility)
                {
                    if (connectionPos.HasValue && !randomBoltPositions)
                        this.connections[l].SetStuckAt(connectionPos.Value, false); //added to track grabbed object

                    Vector2 vector2 = this.connections[l].stuckAt;
                    float d = 2f * Mathf.Lerp(this.connections[l].lastLightUp, this.connections[l].lightUp, timeStacker);
                    for (int m = 0; m < 20; m++)
                    {
                        Vector2 angle = Custom.DirVec(center, this.connections[l].stuckAt);

                        //added alternative startposition
                        Vector2 start = center + (shortestDistFromHalo ? angle : this.connections[l].angleInHalo) * this.Radius(2f, timeStacker) * size;

                        float f = (float)m / 19f;
                        Vector2 vector3 = Custom.Bezier(this.connections[l].stuckAt, this.connections[l].handle, start, center + angle * 400f, f);
                        Vector2 vector4 = Custom.DirVec(vector2, vector3);
                        Vector2 a2 = Custom.PerpendicularVector(vector4);
                        float d2 = Vector2.Distance(vector2, vector3);
                        (sLeaser.sprites[this.firstSprite + 2 + l] as TriangleMesh).MoveVertice(m * 4, vector3 - vector4 * d2 * 0.3f - a2 * d - camPos);
                        (sLeaser.sprites[this.firstSprite + 2 + l] as TriangleMesh).MoveVertice(m * 4 + 1, vector3 - vector4 * d2 * 0.3f + a2 * d - camPos);
                        (sLeaser.sprites[this.firstSprite + 2 + l] as TriangleMesh).MoveVertice(m * 4 + 2, vector3 - a2 * d - camPos);
                        (sLeaser.sprites[this.firstSprite + 2 + l] as TriangleMesh).MoveVertice(m * 4 + 3, vector3 + a2 * d - camPos);
                        vector2 = vector3;
                        sLeaser.sprites[this.firstSprite + 2 + l].isVisible = true; //added to show connection
                        sLeaser.sprites[this.firstSprite + 2 + l].color = Options.whiteLightning.Value ? Color.white : color; //added color assignment
                    }
                } else { //added to hide connection once struck
                    sLeaser.sprites[this.firstSprite + 2 + l].isVisible = false;
                }
            }
            int spriteNum = this.firstBitSprite;
            for (int n = 0; n < this.bits.Length; n++)
            {
                for (int num2 = 0; num2 < this.bits[n].Length; num2++)
                {
                    float num3 = (float)num2 / (float)this.bits[n].Length * 360f + this.Rotation(n, timeStacker);
                    Vector2 vector5 = center + Custom.DegToVec(num3) * this.Radius((float)n + 0.5f, timeStacker) * size; //added size
                    sLeaser.sprites[spriteNum].scaleY = 8f * this.bits[n][num2].Fill(timeStacker);
                    sLeaser.sprites[spriteNum].x = vector5.x - camPos.x;
                    sLeaser.sprites[spriteNum].y = vector5.y - camPos.y;
                    sLeaser.sprites[spriteNum].rotation = num3;
                    sLeaser.sprites[spriteNum].color = color; //added color assignment
                    spriteNum++;
                }
            }
        }


        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }


        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            sLeaser.RemoveAllSpritesFromContainer();
            if (newContainer == null)
                newContainer = rCam.ReturnFContainer("BackgroundShortcuts");
            for (int i = 0; i < this.totalSprites; i++)
                newContainer.AddChild(sLeaser.sprites[i]);
        }


        public class Connection
        {
            public OracleHalo halo;
            public Vector2 stuckAt;
            public Vector2 handle;
            public float lightUp;
            public float lastLightUp;

            public Vector2 angleInHalo; //added random start angle


            public Connection(OracleHalo halo)
            {
                this.halo = halo;
            }


            //edited Connection ctor to set stuckAt value at a later time
            public void SetStuckAt(Vector2 stuckAt, bool newHandle = true)
            {
                Vector2 vector = stuckAt;
                vector.x = Mathf.Clamp(vector.x, stuckAt.x - 20f, stuckAt.x + 20f); //TODO stuckAt value in a radius around position?
                vector.y = Mathf.Clamp(vector.y, stuckAt.y - 20f, stuckAt.y + 20f);
                this.stuckAt = Vector2.Lerp(stuckAt, vector, 0.5f);
                if (newHandle) { //added optional newhandle
                    this.handle = stuckAt + Custom.RNV() * Mathf.Lerp(100f, 300f, UnityEngine.Random.value);
                    angleInHalo = Custom.RNV(); //added random start angle
                }
            }
        }


        public class MemoryBit
        {
            public OracleHalo halo;
            public IntVector2 position;
            public float filled;
            public float lastFilled;
            public float getToFilled;
            public float fillSpeed;
            public int blinkCounter;

            public float Fill(float timeStacker)
            {
                if (this.blinkCounter % 4 > 1 && this.filled == this.getToFilled)
                    return 0f;
                return Mathf.Lerp(this.lastFilled, this.filled, timeStacker);
            }

            public MemoryBit(OracleHalo halo, IntVector2 position)
            {
                this.halo = halo;
                this.position = position;
                this.filled = UnityEngine.Random.value;
                this.lastFilled = this.filled;
                this.getToFilled = this.filled;
                this.fillSpeed = 0f;
            }

            public void SetToMax()
            {
                this.getToFilled = 1f;
                this.fillSpeed = Mathf.Lerp(this.fillSpeed, 0.25f, 0.25f);
                this.blinkCounter = 20;
            }

            public void Update()
            {
                this.lastFilled = this.filled;
                if (this.filled != this.getToFilled)
                {
                    this.filled = Custom.LerpAndTick(this.filled, this.getToFilled, 0.03f, this.fillSpeed);
                    return;
                }
                if (this.blinkCounter > 0)
                {
                    this.blinkCounter--;
                    return;
                }
                if (UnityEngine.Random.value < 0.016666668f)
                {
                    this.getToFilled = UnityEngine.Random.value;
                    this.fillSpeed = 1f / Mathf.Lerp(2f, 80f, UnityEngine.Random.value);
                }
            }
        }
    }
}
