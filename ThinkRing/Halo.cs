using UnityEngine;
using RWCustom;

namespace ThinkRing
{
    //basically a tweaked copy from the game (OracleGraphics.Halo)
    public class Halo : UpdatableAndDeletable, IDrawable
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
        public float averageVoice = 0f; //determines size
        public Vector2? connectionPos = null; //if not null, connections will fire
        public float boltFireChance = 0.6f;


        public Halo(GenericBodyPart owner)
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
        }


        public override void Update(bool eu)
        {
            base.Update(eu);

            //destroy and return if owner is deleted or moves to another room
            if (owner.owner?.owner?.slatedForDeletetion != false || 
                this.room != owner.owner?.owner?.room) {
                if (averageVoice > 0f) {
                    averageVoice -= 1f / 40f; //gradually get smaller (1s)
                    Plugin.Logger.LogDebug(averageVoice);
                } else {
                    this.Destroy();
                    this.visibility = false;
                    this.RemoveFromRoom();
                    return;
                }
            } else if (averageVoice < 1f) {
                averageVoice += 1f / 40f; //gradually get larger (1s)
                Plugin.Logger.LogDebug(averageVoice);
            }

            //============================================== Original Code ================================================

            //edited connectionsFireChance
            float connectionsFireChance = suppressConnectionFires ? 0f : boltFireChance;

            for (int i = 0; i < this.connections.Length; i++)
            {
                this.connections[i].lastLightUp = this.connections[i].lightUp;
                this.connections[i].lightUp *= 0.9f;
                if (UnityEngine.Random.value < connectionsFireChance / 40f && visibility && connectionPos.HasValue)
                {
                    this.connections[i].SetStuckAt(connectionPos.Value); //added this line to connect bolt to grabbed object
                    this.connections[i].lightUp = 1f;
                    this.room.PlaySound(SoundID.SS_AI_Halo_Connection_Light_Up, 0f, (1f * (1f - noiseSuppress)), 1f);
                }
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
                else if (UnityEngine.Random.value < 0.033333335f)
                {
                    this.ringRotations[j, 3] = this.ringRotations[j, 0] + ((UnityEngine.Random.value < 0.5f) ? -1f : 1f) * Mathf.Lerp(15f, 150f, UnityEngine.Random.value);
                }
            }
            for (int k = 0; k < this.bits.Length; k++)
                for (int l = 0; l < this.bits[k].Length; l++)
                    this.bits[k][l].Update();
            if (UnityEngine.Random.value < 0.016666668f && this.bits.Length != 0)
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
            return (3f + ring + Mathf.Lerp(this.lastPush, this.push, timeStacker) - 0.5f * averageVoice) * Mathf.Lerp(this.lastExpand, this.expand, timeStacker) * 7f;
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
                sLeaser.sprites[this.firstSprite + i].color = new Color(1f, 1f, 1f);
            }
            for (int j = 0; j < this.connections.Length; j++)
            {
                sLeaser.sprites[this.firstSprite + 2 + j] = TriangleMesh.MakeLongMesh(20, false, false);
                sLeaser.sprites[this.firstSprite + 2 + j].color = new Color(1f, 1f, 1f);
            }
            for (int k = 0; k < 100; k++)
            {
                sLeaser.sprites[this.firstBitSprite + k] = new FSprite("pixel", true);
                sLeaser.sprites[this.firstBitSprite + k].scaleX = 4f;
                sLeaser.sprites[this.firstBitSprite + k].color = new Color(1f, 1f, 1f);
            }
            this.AddToContainer(sLeaser, rCam, null);
        }


        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (sLeaser.sprites[this.firstSprite].isVisible != visibility)
            {
                for (int i = 0; i < 2 + this.connections.Length; i++)
                    sLeaser.sprites[this.firstSprite + i].isVisible = visibility;
                for (int j = 0; j < 100; j++)
                    sLeaser.sprites[this.firstBitSprite + j].isVisible = visibility;
            }
            Vector2 vector = this.Center(timeStacker);
            for (int k = 0; k < 2; k++)
            {
                sLeaser.sprites[this.firstSprite + k].x = vector.x - camPos.x;
                sLeaser.sprites[this.firstSprite + k].y = vector.y - camPos.y;
                sLeaser.sprites[this.firstSprite + k].scale = this.Radius((float)k, timeStacker) / 8f;
            }
            sLeaser.sprites[this.firstSprite].alpha = Mathf.Lerp(3f / this.Radius(0f, timeStacker), 1f, Mathf.Lerp(this.lastWhite, this.white, timeStacker));
            sLeaser.sprites[this.firstSprite + 1].alpha = 3f / this.Radius(1f, timeStacker);
            for (int l = 0; l < this.connections.Length; l++)
            {
                if (this.connections[l].lastLightUp > 0.05f || this.connections[l].lightUp > 0.05f)
                {
                    Vector2 vector2 = this.connections[l].stuckAt;
                    float d = 2f * Mathf.Lerp(this.connections[l].lastLightUp, this.connections[l].lightUp, timeStacker);
                    for (int m = 0; m < 20; m++)
                    {
                        float f = (float)m / 19f;
                        Vector2 a = Custom.DirVec(vector, this.connections[l].stuckAt);
                        Vector2 vector3 = Custom.Bezier(this.connections[l].stuckAt, this.connections[l].handle, vector + a * this.Radius(2f, timeStacker), vector + a * 400f, f);
                        Vector2 vector4 = Custom.DirVec(vector2, vector3);
                        Vector2 a2 = Custom.PerpendicularVector(vector4);
                        float d2 = Vector2.Distance(vector2, vector3);
                        (sLeaser.sprites[this.firstSprite + 2 + l] as TriangleMesh).MoveVertice(m * 4, vector3 - vector4 * d2 * 0.3f - a2 * d - camPos);
                        (sLeaser.sprites[this.firstSprite + 2 + l] as TriangleMesh).MoveVertice(m * 4 + 1, vector3 - vector4 * d2 * 0.3f + a2 * d - camPos);
                        (sLeaser.sprites[this.firstSprite + 2 + l] as TriangleMesh).MoveVertice(m * 4 + 2, vector3 - a2 * d - camPos);
                        (sLeaser.sprites[this.firstSprite + 2 + l] as TriangleMesh).MoveVertice(m * 4 + 3, vector3 + a2 * d - camPos);
                        vector2 = vector3;
                    }
                }
            }
            int spriteNum = this.firstBitSprite;
            for (int n = 0; n < this.bits.Length; n++)
            {
                for (int num2 = 0; num2 < this.bits[n].Length; num2++)
                {
                    float num3 = (float)num2 / (float)this.bits[n].Length * 360f + this.Rotation(n, timeStacker);
                    Vector2 vector5 = vector + Custom.DegToVec(num3) * this.Radius((float)n + 0.5f, timeStacker);
                    sLeaser.sprites[spriteNum].scaleY = 8f * this.bits[n][num2].Fill(timeStacker);
                    sLeaser.sprites[spriteNum].x = vector5.x - camPos.x;
                    sLeaser.sprites[spriteNum].y = vector5.y - camPos.y;
                    sLeaser.sprites[spriteNum].rotation = num3;
//                    sLeaser.sprites[spriteNum].alpha = 1f; //added so sprites are visible
                    spriteNum++;
                }
            }
        }


        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }


        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            sLeaser.RemoveAllSpritesFromContainer();
            if (newContatiner == null)
                newContatiner = rCam.ReturnFContainer("BackgroundShortcuts");
            for (int i = 0; i < this.totalSprites; i++)
                newContatiner.AddChild(sLeaser.sprites[i]);
        }


        public class Connection
        {
            public Halo halo;
            public Vector2 stuckAt;
            public Vector2 handle;
            public float lightUp;
            public float lastLightUp;


            public Connection(Halo halo)
            {
                this.halo = halo;
            }


            //edited Connection ctor to set stuckAt value at a later time
            public void SetStuckAt(Vector2 stuckAt)
            {
                Vector2 vector = stuckAt;
                vector.x = Mathf.Clamp(vector.x, stuckAt.x - 20f, stuckAt.x + 20f); //TODO stuckAt value in a radius around position?
                vector.y = Mathf.Clamp(vector.y, stuckAt.y - 20f, stuckAt.y + 20f);
                this.stuckAt = Vector2.Lerp(stuckAt, vector, 0.5f);
                this.handle = stuckAt + Custom.RNV() * Mathf.Lerp(400f, 700f, UnityEngine.Random.value);
            }
        }


        public class MemoryBit
        {
            public Halo halo;
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

            public MemoryBit(Halo halo, IntVector2 position)
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
